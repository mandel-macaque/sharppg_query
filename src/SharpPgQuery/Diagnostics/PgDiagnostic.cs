namespace SharpPgQuery.Diagnostics
{
    /// <summary>
    /// Represents a diagnostic (error or warning) produced during parsing.
    /// Follows the design of <c>Microsoft.CodeAnalysis.Diagnostic</c>.
    /// </summary>
    public sealed class PgDiagnostic
    {
        /// <summary>
        /// Initialises a new instance of <see cref="PgDiagnostic"/>.
        /// </summary>
        public PgDiagnostic(
            string message,
            PgDiagnosticSeverity severity,
            int cursorPosition,
            string? filename = null,
            int lineNumber = 0)
        {
            Message = message;
            Severity = severity;
            CursorPosition = cursorPosition;
            Filename = filename;
            LineNumber = lineNumber;
        }

        /// <summary>Gets the human-readable error message.</summary>
        public string Message { get; }

        /// <summary>Gets the severity of this diagnostic.</summary>
        public PgDiagnosticSeverity Severity { get; }

        /// <summary>Gets the character offset in the query where the error occurred (1-based, 0 if unknown).</summary>
        public int CursorPosition { get; }

        /// <summary>Gets the source file name reported by libpg_query, if any.</summary>
        public string? Filename { get; }

        /// <summary>Gets the line number reported by libpg_query, if any.</summary>
        public int LineNumber { get; }

        /// <inheritdoc/>
        public override string ToString() =>
            CursorPosition > 0
                ? $"{Severity}: {Message} (position {CursorPosition})"
                : $"{Severity}: {Message}";
    }
}
