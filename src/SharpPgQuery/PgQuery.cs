using System;
using SharpPgQuery.Native;

namespace SharpPgQuery
{
    /// <summary>
    /// Provides the primary entry points for the SharpPgQuery library.
    /// Mirrors the role of <c>Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree</c>
    /// static helpers.
    /// </summary>
    public static class PgQuery
    {
        /// <summary>
        /// Parses a SQL string and returns a <see cref="PgSqlSyntaxTree"/>.
        /// </summary>
        /// <param name="sql">The SQL text to parse.</param>
        /// <param name="path">An optional file path to associate with the tree.</param>
        /// <returns>A new <see cref="PgSqlSyntaxTree"/> that may contain errors.</returns>
        public static PgSqlSyntaxTree ParseSql(string sql, string? path = null) =>
            PgSqlSyntaxTree.ParseText(sql, path);

        /// <summary>
        /// Normalizes a SQL query by replacing literal values with positional
        /// parameters (e.g. <c>$1</c>, <c>$2</c>).
        /// </summary>
        /// <param name="sql">The SQL text to normalize.</param>
        /// <returns>The normalized query string.</returns>
        /// <exception cref="PgQueryException">Thrown when the query cannot be normalized.</exception>
        public static string NormalizeSql(string sql)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));

            var result = PgQueryNativeMethods.Normalize(sql);
            try
            {
                if (result.Error != IntPtr.Zero)
                {
                    var error = NativeInteropHelper.ReadError(result.Error)!;
                    throw new PgQueryException(error.Message, error.Cursorpos);
                }

                return NativeInteropHelper.PtrToStringUtf8(result.NormalizedQuery)
                    ?? string.Empty;
            }
            finally
            {
                PgQueryNativeMethods.FreeNormalizeResult(result);
            }
        }
    }
}
