using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpPgQuery.Native
{
    /// <summary>
    /// Provides helper utilities for interoperating with native libpg_query memory.
    /// </summary>
    internal static class NativeInteropHelper
    {
        /// <summary>
        /// Converts a null-terminated UTF-8 byte pointer to a managed string.
        /// Compatible with netstandard2.0 and later runtimes.
        /// </summary>
        internal static string? PtrToStringUtf8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            int length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
                length++;

            if (length == 0)
                return string.Empty;

            byte[] buffer = new byte[length];
            Marshal.Copy(ptr, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Converts a PgQueryErrorNative pointer to a managed PgQueryError.
        /// Returns null if the pointer is zero.
        /// </summary>
        internal static PgQueryErrorManaged? ReadError(IntPtr errorPtr)
        {
            if (errorPtr == IntPtr.Zero)
                return null;

            var native = Marshal.PtrToStructure<PgQueryErrorNative>(errorPtr);
            return new PgQueryErrorManaged(
                message: PtrToStringUtf8(native.Message) ?? string.Empty,
                funcname: PtrToStringUtf8(native.Funcname),
                filename: PtrToStringUtf8(native.Filename),
                lineno: native.Lineno,
                cursorpos: native.Cursorpos,
                context: PtrToStringUtf8(native.Context));
        }
    }

    /// <summary>
    /// Managed representation of a libpg_query error.
    /// </summary>
    internal sealed class PgQueryErrorManaged
    {
        internal PgQueryErrorManaged(
            string message,
            string? funcname,
            string? filename,
            int lineno,
            int cursorpos,
            string? context)
        {
            Message = message;
            Funcname = funcname;
            Filename = filename;
            Lineno = lineno;
            Cursorpos = cursorpos;
            Context = context;
        }

        internal string Message { get; }
        internal string? Funcname { get; }
        internal string? Filename { get; }
        internal int Lineno { get; }
        internal int Cursorpos { get; }
        internal string? Context { get; }
    }
}
