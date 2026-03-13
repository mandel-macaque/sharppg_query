using System;
using System.Linq;
using SharpPgQuery;
using SharpPgQuery.Diagnostics;
using SharpPgQuery.Syntax;
using Xunit;

namespace SharpPgQuery.Tests
{
    public class PgQueryTests
    {
        // ── Parsing ──────────────────────────────────────────────────────────

        [Fact]
        public void ParseText_SimpleSelect_ReturnsTreeWithoutErrors()
        {
            using var tree = PgSqlSyntaxTree.ParseText("SELECT 1");
            Assert.False(tree.HasErrors);
            Assert.Empty(tree.GetDiagnostics());
        }

        [Fact]
        public void ParseText_InvalidSql_ReturnsTreeWithErrors()
        {
            using var tree = PgSqlSyntaxTree.ParseText("SELECT FROM WHERE");
            Assert.True(tree.HasErrors);
            Assert.NotEmpty(tree.GetDiagnostics());
            Assert.Equal(PgDiagnosticSeverity.Error, tree.GetDiagnostics()[0].Severity);
        }

        [Fact]
        public void ParseText_ReturnsTextProperty()
        {
            const string sql = "SELECT 1";
            using var tree = PgSqlSyntaxTree.ParseText(sql);
            Assert.Equal(sql, tree.GetText());
        }

        [Fact]
        public void ParseText_WithPath_SetsFilePath()
        {
            using var tree = PgSqlSyntaxTree.ParseText("SELECT 1", "query.sql");
            Assert.Equal("query.sql", tree.FilePath);
        }

        [Fact]
        public void ParseText_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PgSqlSyntaxTree.ParseText(null!));
        }

        // ── Root node ────────────────────────────────────────────────────────

        [Fact]
        public void GetRoot_SimpleSelect_ReturnsRootNode()
        {
            using var tree = PgSqlSyntaxTree.ParseText("SELECT 1");
            var root = tree.GetRoot();
            Assert.NotNull(root);
            Assert.Equal(PgSyntaxKind.ParseResult, root.Kind);
            Assert.Null(root.Parent);
        }

        [Fact]
        public void GetRoot_ErrorTree_ThrowsInvalidOperationException()
        {
            using var tree = PgSqlSyntaxTree.ParseText("NOT VALID SQL !!!");
            Assert.True(tree.HasErrors);
            Assert.Throws<InvalidOperationException>(() => tree.GetRoot());
        }

        // ── Descendants ──────────────────────────────────────────────────────

        [Fact]
        public void DescendantNodes_SimpleSelect_ContainsSelectStatement()
        {
            using var tree = PgSqlSyntaxTree.ParseText("SELECT 1");
            var root = tree.GetRoot();
            var nodes = root.DescendantNodes();
            Assert.Contains(nodes, n => n.Kind == PgSyntaxKind.SelectStatement);
        }

        [Fact]
        public void DescendantNodes_MultipleStatements_FindsAllSelectStatements()
        {
            using var tree = PgSqlSyntaxTree.ParseText("SELECT 1; SELECT 2");
            var root = tree.GetRoot();
            var selectNodes = root.DescendantNodes()
                .Where(n => n.Kind == PgSyntaxKind.SelectStatement)
                .ToList();
            Assert.Equal(2, selectNodes.Count);
        }

        // ── PgQuery static helpers ───────────────────────────────────────────

        [Fact]
        public void ParseSql_ReturnsTree()
        {
            using var tree = PgQuery.ParseSql("SELECT id FROM users");
            Assert.False(tree.HasErrors);
            var root = tree.GetRoot();
            Assert.Contains(root.DescendantNodes(), n => n.Kind == PgSyntaxKind.SelectStatement);
        }

        [Fact]
        public void NormalizeSql_ReplacesLiterals()
        {
            var normalized = PgQuery.NormalizeSql("SELECT * FROM t WHERE id = 42");
            Assert.Contains("$1", normalized);
        }

        [Fact]
        public void NormalizeSql_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PgQuery.NormalizeSql(null!));
        }

        // ── Diagnostics ──────────────────────────────────────────────────────

        [Fact]
        public void Diagnostics_ContainsMessageForInvalidSql()
        {
            using var tree = PgSqlSyntaxTree.ParseText("SELECT (");
            Assert.True(tree.HasErrors);
            var diag = tree.GetDiagnostics()[0];
            Assert.False(string.IsNullOrEmpty(diag.Message));
            Assert.True(diag.CursorPosition >= 0);
        }

        // ── Dispose ──────────────────────────────────────────────────────────

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var tree = PgSqlSyntaxTree.ParseText("SELECT 1");
            tree.Dispose();
            tree.Dispose(); // should not throw
        }

        [Fact]
        public void GetRoot_AfterDispose_ThrowsObjectDisposedException()
        {
            var tree = PgSqlSyntaxTree.ParseText("SELECT 1");
            tree.Dispose();
            Assert.Throws<ObjectDisposedException>(() => tree.GetRoot());
        }
    }
}
