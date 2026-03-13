using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpPgQuery.Native
{
    /// <summary>
    /// Represents a native libpg_query error structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryErrorNative
    {
        public IntPtr Message;
        public IntPtr Funcname;
        public IntPtr Filename;
        public int Lineno;
        public int Cursorpos;
        public IntPtr Context;
    }

    /// <summary>
    /// Represents the native result of pg_query_parse.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryParseResultNative
    {
        public IntPtr ParseTree;    // char* (JSON string)
        public IntPtr StderrBuffer; // char*
        public IntPtr Error;        // PgQueryError*
    }

    /// <summary>
    /// Represents the native result of pg_query_normalize.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PgQueryNormalizeResultNative
    {
        public IntPtr NormalizedQuery; // char*
        public IntPtr Error;           // PgQueryError*
    }

    /// <summary>
    /// Provides raw P/Invoke declarations for the libpg_query native library.
    /// The native functions accept null-terminated UTF-8 strings (char*).
    /// We marshal them manually via <see cref="AllocUtf8"/> / <see cref="FreeUtf8"/>
    /// to remain compatible with netstandard2.0 where
    /// <c>UnmanagedType.LPUTF8Str</c> is unavailable.
    /// <para>
    /// The library name <c>"pg_query"</c> follows the .NET P/Invoke convention
    /// where the runtime automatically resolves platform-specific names:
    /// <c>libpg_query.so</c> on Linux and <c>libpg_query.dylib</c> on macOS.
    /// </para>
    /// </summary>
    internal static class PgQueryNativeMethods
    {
        private const string LibraryName = "pg_query";

        // ── Private P/Invoke declarations (accept IntPtr for the char* parameters) ──

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "pg_query_parse")]
        private static extern PgQueryParseResultNative ParseInternal(IntPtr input);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "pg_query_free_parse_result")]
        internal static extern void FreeParseResult(PgQueryParseResultNative result);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "pg_query_normalize")]
        private static extern PgQueryNormalizeResultNative NormalizeInternal(IntPtr input);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "pg_query_free_normalize_result")]
        internal static extern void FreeNormalizeResult(PgQueryNormalizeResultNative result);

        // ── Public helpers that handle UTF-8 marshaling ──────────────────────

        /// <summary>Parses a SQL query and returns the parse tree as a JSON string.</summary>
        internal static PgQueryParseResultNative Parse(string input)
        {
            IntPtr ptr = AllocUtf8(input);
            try
            {
                return ParseInternal(ptr);
            }
            finally
            {
                FreeUtf8(ptr);
            }
        }

        /// <summary>Normalizes a SQL query (replaces literals with positional parameters).</summary>
        internal static PgQueryNormalizeResultNative Normalize(string input)
        {
            IntPtr ptr = AllocUtf8(input);
            try
            {
                return NormalizeInternal(ptr);
            }
            finally
            {
                FreeUtf8(ptr);
            }
        }

        // ── UTF-8 marshaling helpers ─────────────────────────────────────────

        private static IntPtr AllocUtf8(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            // Allocate enough space for the bytes plus a null terminator.
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            return ptr;
        }

        private static void FreeUtf8(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
        }
    }
}
