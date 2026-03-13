using System.Collections.Generic;
using System.Text.Json;

namespace SharpPgQuery.Syntax
{
    /// <summary>
    /// Maps libpg_query JSON property names to <see cref="PgSyntaxKind"/> values.
    /// </summary>
    internal static class SyntaxKindMap
    {
        private static readonly Dictionary<string, PgSyntaxKind> s_map =
            new Dictionary<string, PgSyntaxKind>
            {
                // Statements
                { "SelectStmt",       PgSyntaxKind.SelectStatement },
                { "InsertStmt",       PgSyntaxKind.InsertStatement },
                { "UpdateStmt",       PgSyntaxKind.UpdateStatement },
                { "DeleteStmt",       PgSyntaxKind.DeleteStatement },
                { "ValuesStmt",       PgSyntaxKind.ValuesStatement },

                // DDL
                { "CreateStmt",       PgSyntaxKind.CreateTableStatement },
                { "AlterTableStmt",   PgSyntaxKind.AlterTableStatement },
                { "DropStmt",         PgSyntaxKind.DropStatement },
                { "IndexStmt",        PgSyntaxKind.CreateIndexStatement },

                // Expressions
                { "Integer",          PgSyntaxKind.IntegerLiteral },
                { "Float",            PgSyntaxKind.FloatLiteral },
                { "String",           PgSyntaxKind.StringLiteral },
                { "Boolean",          PgSyntaxKind.BooleanLiteral },
                { "Null",             PgSyntaxKind.NullLiteral },
                { "ColumnRef",        PgSyntaxKind.ColumnRef },
                { "FuncCall",         PgSyntaxKind.FunctionCall },
                { "TypeCast",         PgSyntaxKind.TypeCast },
                { "A_Expr",           PgSyntaxKind.BinaryExpression },
                { "BooleanTest",      PgSyntaxKind.UnaryExpression },
                { "ResTarget",        PgSyntaxKind.ResTarget },
                { "RangeVar",         PgSyntaxKind.RangeVar },

                // Clauses / parse-tree nodes
                { "stmts",            PgSyntaxKind.StatementList },
                { "stmt",             PgSyntaxKind.Statement },
                { "whereClause",      PgSyntaxKind.WhereClause },
                { "joinType",         PgSyntaxKind.JoinExpr },
                { "JoinExpr",         PgSyntaxKind.JoinExpr },
                { "groupClause",      PgSyntaxKind.GroupByClause },
                { "havingClause",     PgSyntaxKind.HavingClause },
                { "sortClause",       PgSyntaxKind.OrderByClause },
                { "limitCount",       PgSyntaxKind.LimitClause },

                // Root
                { "ParseResult",      PgSyntaxKind.ParseResult },
            };

        /// <summary>Resolves a JSON property name to a <see cref="PgSyntaxKind"/>.</summary>
        internal static PgSyntaxKind Resolve(string propertyName) =>
            s_map.TryGetValue(propertyName, out var kind) ? kind : PgSyntaxKind.Unknown;

        /// <summary>
        /// Infers the kind of a JSON object by inspecting its first property key.
        /// Used when iterating array elements.
        /// </summary>
        internal static PgSyntaxKind ResolveFromObjectKeys(JsonElement element)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (s_map.TryGetValue(property.Name, out var kind))
                    return kind;
            }

            return PgSyntaxKind.Unknown;
        }
    }
}
