namespace SharpPgQuery.Diagnostics
{
    /// <summary>
    /// Specifies the severity level of a diagnostic.
    /// Mirrors the design of <c>Microsoft.CodeAnalysis.DiagnosticSeverity</c>.
    /// </summary>
    public enum PgDiagnosticSeverity
    {
        /// <summary>A diagnostic that does not prevent parsing but should be noted.</summary>
        Warning = 1,

        /// <summary>A diagnostic that represents a parse or semantic error.</summary>
        Error = 2,
    }
}
