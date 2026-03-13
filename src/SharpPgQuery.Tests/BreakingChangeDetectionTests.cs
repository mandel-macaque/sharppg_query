using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SharpPgQuery;
using Xunit;

namespace SharpPgQuery.Tests
{
    public class BreakingChangeDetectionTests
    {
        [Theory]
        [InlineData("ALTER TABLE users DROP COLUMN email", "drops column email")]
        [InlineData("ALTER TABLE users RENAME COLUMN email TO contact_email", "RENAME column email")]
        [InlineData("ALTER TABLE users ALTER COLUMN email SET NOT NULL", "sets NOT NULL")]
        [InlineData("ALTER TABLE users ADD CONSTRAINT users_email_key UNIQUE (email)", "adds UNIQUE constraint")]
        [InlineData("UPDATE users SET email = lower(email)", "UPDATE rewrites data")]
        public void Detects_breaking_changes_from_migration_steps(string sql, string expectedSnippet)
        {
            var reasons = BreakingChangeDetector.GetBreakingChanges(sql).ToList();
            Assert.Contains(reasons, reason => reason.Contains(expectedSnippet, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Ignores_queries_without_schema_changes()
        {
            var sql = "CREATE TABLE audit_log(id uuid); SELECT 1;";
            var reasons = BreakingChangeDetector.GetBreakingChanges(sql).ToList();
            Assert.Empty(reasons);
        }

        [Fact]
        public void Flags_unclassified_alter_table_commands_by_default()
        {
            var sql = "ALTER TABLE users ADD COLUMN bio text";
            var reasons = BreakingChangeDetector.GetBreakingChanges(sql).ToList();
            Assert.NotEmpty(reasons);
            Assert.Contains(reasons, r => r.Contains("ALTER TABLE users", StringComparison.OrdinalIgnoreCase));
        }
    }

    internal static class BreakingChangeDetector
    {
        private static readonly HashSet<string> BreakingAlterSubtypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "AT_DropColumn",
            "AT_SetNotNull",
        };

        private static readonly HashSet<string> BreakingConstraintTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "CONSTR_UNIQUE",
        };

        private static readonly HashSet<string> BreakingRenameTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "OBJECT_COLUMN",
        };

        public static IReadOnlyList<string> GetBreakingChanges(string sql)
        {
            var reasons = new List<string>();
            using var tree = PgSqlSyntaxTree.ParseText(sql);

            if (tree.HasErrors)
            {
                reasons.Add("Parse error in SQL");
                return reasons;
            }

            var rootJson = tree.GetRoot().RawJson;
            if (!rootJson.TryGetProperty("stmts", out var stmts) || stmts.ValueKind != JsonValueKind.Array)
                return reasons;

            foreach (var stmtWrapper in stmts.EnumerateArray())
            {
                if (!stmtWrapper.TryGetProperty("stmt", out var stmt))
                    continue;

                foreach (var node in stmt.EnumerateObject())
                {
                    switch (node.Name)
                    {
                        case "AlterTableStmt":
                            AnalyzeAlterTable(node.Value, reasons);
                            break;
                        case "RenameStmt":
                            AnalyzeRename(node.Value, reasons);
                            break;
                        case "DropStmt":
                            reasons.Add("DROP statement changes existing schema");
                            break;
                        case "UpdateStmt":
                            reasons.Add("UPDATE rewrites data");
                            break;
                    }
                }
            }

            return reasons;
        }

        private static void AnalyzeAlterTable(JsonElement alterTable, List<string> reasons)
        {
            var tableName = TryGetString(alterTable, "relation", "relname") ?? "<unknown>";
            if (!alterTable.TryGetProperty("cmds", out var cmds) || cmds.ValueKind != JsonValueKind.Array)
            {
                reasons.Add($"ALTER TABLE {tableName} change");
                return;
            }

            foreach (var cmdWrapper in cmds.EnumerateArray())
            {
                if (!cmdWrapper.TryGetProperty("AlterTableCmd", out var cmd))
                {
                    reasons.Add($"ALTER TABLE {tableName} unknown command");
                    continue;
                }

                var subtype = TryGetString(cmd, "subtype");
                if (BreakingAlterSubtypes.Contains(subtype ?? string.Empty))
                {
                    var column = TryGetString(cmd, "name") ?? "<unknown>";
                    reasons.Add(subtype switch
                    {
                        "AT_DropColumn" => $"ALTER TABLE {tableName} drops column {column}",
                        "AT_SetNotNull" => $"ALTER TABLE {tableName} sets NOT NULL on column {column}",
                        _ => $"ALTER TABLE {tableName} command {subtype}"
                    });
                    continue;
                }

                if (string.Equals(subtype, "AT_AddConstraint", StringComparison.OrdinalIgnoreCase))
                {
                    if (cmd.TryGetProperty("def", out var def)
                        && def.TryGetProperty("Constraint", out var constraint))
                    {
                        var constraintType = TryGetString(constraint, "contype");
                        if (constraintType != null && BreakingConstraintTypes.Contains(constraintType))
                        {
                            var constraintName = TryGetString(constraint, "conname") ?? "<unnamed>";
                            reasons.Add($"ALTER TABLE {tableName} adds UNIQUE constraint {constraintName}");
                            continue;
                        }
                    }

                    reasons.Add($"ALTER TABLE {tableName} adds constraint");
                    continue;
                }

                // By policy, treat any unclassified ALTER TABLE command as breaking.
                reasons.Add($"ALTER TABLE {tableName} command {subtype ?? "<unknown>"}");
            }
        }

        private static void AnalyzeRename(JsonElement renameStmt, List<string> reasons)
        {
            var renameType = TryGetString(renameStmt, "renameType");
            if (renameType != null && BreakingRenameTypes.Contains(renameType))
            {
                var tableName = TryGetString(renameStmt, "relation", "relname") ?? "<unknown>";
                var columnName = TryGetString(renameStmt, "subname") ?? "<unknown>";
                reasons.Add($"RENAME COLUMN {columnName} on {tableName}");
                return;
            }

            reasons.Add("RENAME statement");
        }

        private static string? TryGetString(JsonElement element, params string[] propertyPath)
        {
            JsonElement current = element;
            foreach (var property in propertyPath)
            {
                if (!current.TryGetProperty(property, out current))
                    return null;
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
        }
    }
}
