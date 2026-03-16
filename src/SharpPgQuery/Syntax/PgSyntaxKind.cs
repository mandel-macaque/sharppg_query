namespace SharpPgQuery.Syntax
{
    /// <summary>
    /// Identifies the kind of a syntax node in a PostgreSQL parse tree.
    /// Mirrors the role of <c>Microsoft.CodeAnalysis.CSharp.SyntaxKind</c>.
    /// Additional kinds correspond to the node types returned by libpg_query.
    /// </summary>
    public enum PgSyntaxKind
    {
        /// <summary>No syntax kind specified (default value).</summary>
        None = 0,

        /// <summary>The root of the parse tree.</summary>
        ParseResult = 1,

        /// <summary>A list of statements.</summary>
        StatementList = 2,

        /// <summary>A single statement wrapper.</summary>
        Statement = 3,

        // ── DML Statements ──────────────────────────────────────────────────

        /// <summary>A SELECT statement.</summary>
        SelectStatement = 100,

        /// <summary>An INSERT statement.</summary>
        InsertStatement = 101,

        /// <summary>An UPDATE statement.</summary>
        UpdateStatement = 102,

        /// <summary>A DELETE statement.</summary>
        DeleteStatement = 103,

        /// <summary>A VALUES clause.</summary>
        ValuesStatement = 104,

        // ── DDL Statements ──────────────────────────────────────────────────

        /// <summary>A CREATE TABLE statement.</summary>
        CreateTableStatement = 200,

        /// <summary>An ALTER TABLE statement.</summary>
        AlterTableStatement = 201,

        /// <summary>A DROP statement.</summary>
        DropStatement = 202,

        /// <summary>A CREATE INDEX statement.</summary>
        CreateIndexStatement = 203,

        /// <summary>A RENAME statement.</summary>
        RenameStatement = 204,

        // ── Expressions ─────────────────────────────────────────────────────

        /// <summary>An integer literal.</summary>
        IntegerLiteral = 300,

        /// <summary>A floating point literal.</summary>
        FloatLiteral = 301,

        /// <summary>A string literal (or other constant).</summary>
        StringLiteral = 302,

        /// <summary>A Boolean literal.</summary>
        BooleanLiteral = 303,

        /// <summary>A null literal.</summary>
        NullLiteral = 304,

        /// <summary>A column reference.</summary>
        ColumnRef = 310,

        /// <summary>A function call.</summary>
        FunctionCall = 311,

        /// <summary>A type cast.</summary>
        TypeCast = 312,

        /// <summary>A binary expression (e.g., a + b).</summary>
        BinaryExpression = 313,

        /// <summary>A unary expression (e.g., NOT x).</summary>
        UnaryExpression = 314,

        /// <summary>A result target (SELECT list item).</summary>
        ResTarget = 320,

        /// <summary>A range variable (table reference).</summary>
        RangeVar = 321,

        // ── Clauses ──────────────────────────────────────────────────────────

        /// <summary>A WHERE clause expression.</summary>
        WhereClause = 400,

        /// <summary>A JOIN expression.</summary>
        JoinExpr = 401,

        /// <summary>A GROUP BY clause.</summary>
        GroupByClause = 402,

        /// <summary>A HAVING clause.</summary>
        HavingClause = 403,

        /// <summary>An ORDER BY clause.</summary>
        OrderByClause = 404,

        /// <summary>A LIMIT clause.</summary>
        LimitClause = 405,

        // ── Generic ──────────────────────────────────────────────────────────

        /// <summary>A generic JSON object node not mapped to a specific kind.</summary>
        Unknown = 9999,
    }
}
