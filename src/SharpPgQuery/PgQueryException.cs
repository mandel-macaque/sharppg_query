using System;

namespace SharpPgQuery
{
    /// <summary>
    /// The exception thrown when a libpg_query operation fails.
    /// </summary>
    public sealed class PgQueryException : Exception
    {
        /// <summary>
        /// Initialises a new instance of <see cref="PgQueryException"/>.
        /// </summary>
        /// <param name="message">The error message from libpg_query.</param>
        /// <param name="cursorPosition">The character offset in the query (1-based, 0 if unknown).</param>
        public PgQueryException(string message, int cursorPosition = 0)
            : base(message)
        {
            CursorPosition = cursorPosition;
        }

        /// <summary>Gets the character offset in the query where the error occurred (1-based, 0 if unknown).</summary>
        public int CursorPosition { get; }
    }
}
