using System.Collections.Generic;
using SharpPgQuery.Diagnostics;
using SharpPgQuery.Syntax;

namespace SharpPgQuery
{
    /// <summary>
    /// Abstract base class that represents a PostgreSQL SQL parse tree.
    /// Mirrors the design of <c>Microsoft.CodeAnalysis.SyntaxTree</c>.
    /// </summary>
    public abstract class PgSyntaxTree
    {
        /// <summary>Gets the file path associated with this tree, or an empty string.</summary>
        public abstract string FilePath { get; }

        /// <summary>Gets the original SQL text that was parsed.</summary>
        public abstract string GetText();

        /// <summary>Gets the root node of the syntax tree.</summary>
        public abstract PgSyntaxNode GetRoot();

        /// <summary>Gets all diagnostics (errors and warnings) for this tree.</summary>
        public abstract IReadOnlyList<PgDiagnostic> GetDiagnostics();

        /// <summary>Gets whether this tree has any error diagnostics.</summary>
        public abstract bool HasErrors { get; }
    }
}
