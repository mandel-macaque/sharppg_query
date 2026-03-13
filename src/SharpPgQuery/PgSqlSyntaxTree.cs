using System;
using System.Collections.Generic;
using System.Text.Json;
using SharpPgQuery.Diagnostics;
using SharpPgQuery.Native;
using SharpPgQuery.Syntax;

namespace SharpPgQuery
{
    /// <summary>
    /// Represents a parsed PostgreSQL SQL syntax tree.
    /// Mirrors the design of <c>Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree</c>.
    /// </summary>
    /// <remarks>
    /// Use the static <see cref="ParseText(string, string?)"/> factory method to create instances.
    /// The tree holds a reference to a <see cref="JsonDocument"/> that backs all
    /// <see cref="PgSyntaxNode"/> instances, so disposing the tree releases that memory.
    /// </remarks>
    public sealed class PgSqlSyntaxTree : PgSyntaxTree, IDisposable
    {
        private readonly string _text;
        private readonly string _filePath;
        private readonly PgDiagnostic[]? _diagnostics;
        private readonly JsonDocument? _document;
        private PgSyntaxNode? _root;
        private bool _disposed;

        private PgSqlSyntaxTree(
            string text,
            string filePath,
            JsonDocument? document,
            PgDiagnostic[]? diagnostics)
        {
            _text = text;
            _filePath = filePath;
            _document = document;
            _diagnostics = diagnostics;
        }

        // ── Factory ──────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a SQL string and returns a <see cref="PgSqlSyntaxTree"/>.
        /// </summary>
        /// <param name="text">The SQL text to parse.</param>
        /// <param name="path">An optional file path to associate with the tree.</param>
        /// <returns>A new <see cref="PgSqlSyntaxTree"/> that may contain errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
        public static PgSqlSyntaxTree ParseText(string text, string? path = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var result = PgQueryNativeMethods.Parse(text);
            try
            {
                if (result.Error != IntPtr.Zero)
                {
                    var nativeError = NativeInteropHelper.ReadError(result.Error)!;
                    var diagnostic = new PgDiagnostic(
                        message: nativeError.Message,
                        severity: PgDiagnosticSeverity.Error,
                        cursorPosition: nativeError.Cursorpos,
                        filename: nativeError.Filename,
                        lineNumber: nativeError.Lineno);

                    return new PgSqlSyntaxTree(
                        text,
                        path ?? string.Empty,
                        document: null,
                        diagnostics: new[] { diagnostic });
                }

                var json = NativeInteropHelper.PtrToStringUtf8(result.ParseTree);
                var document = JsonDocument.Parse(json ?? "{}");
                return new PgSqlSyntaxTree(
                    text,
                    path ?? string.Empty,
                    document,
                    diagnostics: null);
            }
            finally
            {
                PgQueryNativeMethods.FreeParseResult(result);
            }
        }

        // ── PgSyntaxTree members ─────────────────────────────────────────────

        /// <inheritdoc/>
        public override string FilePath => _filePath;

        /// <inheritdoc/>
        public override string GetText() => _text;

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the tree has parse errors. Check <see cref="HasErrors"/> first.
        /// </exception>
        public override PgSyntaxNode GetRoot()
        {
            ThrowIfDisposed();
            if (_document == null)
                throw new InvalidOperationException(
                    "Cannot get the root of a tree with parse errors. Check HasErrors and GetDiagnostics() first.");

            return _root ??= new PgSyntaxNode(
                _document.RootElement,
                parent: null,
                PgSyntaxKind.ParseResult);
        }

        /// <inheritdoc/>
        public override IReadOnlyList<PgDiagnostic> GetDiagnostics() =>
            (IReadOnlyList<PgDiagnostic>?)_diagnostics ?? Array.Empty<PgDiagnostic>();

        /// <inheritdoc/>
        public override bool HasErrors => _diagnostics != null && _diagnostics.Length > 0;

        // ── IDisposable ──────────────────────────────────────────────────────

        /// <summary>
        /// Disposes the underlying <see cref="JsonDocument"/> to release its pooled memory.
        /// Nodes obtained from <see cref="GetRoot()"/> must not be used after this call.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _document?.Dispose();
                _root = null;
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PgSqlSyntaxTree));
        }
    }
}
