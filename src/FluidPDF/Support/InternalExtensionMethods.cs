using System;
using System.Runtime.CompilerServices;

namespace FluidPDF.Support
{
    internal static class InternalExtensionMethods
    {
        internal static bool IsNullOrBlankString(this string? s) => string.IsNullOrWhiteSpace(s);
        internal static bool IsNotNullAndNotBlank(this string? s) => !string.IsNullOrWhiteSpace(s);
        internal static string? ToNullIfBlank(this string? s) =>
            s.IsNullOrBlankString() ? null : s;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetNonNullOrThrow<T>(
            this T? item,
            [CallerArgumentExpression(nameof(item))] string paramName = "") where T : class =>
            item ?? throw new ArgumentNullException(paramName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetNonNullOrThrow<T>(
            this T? item,
            [CallerArgumentExpression(nameof(item))] string paramName = "") where T : struct =>
            item ?? throw new ArgumentNullException(paramName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetNonNullOrThrow<T>(this T? item, Func<Exception> exFactory) where T : class =>
            item ?? throw exFactory();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetNonNullOrThrow<T>(this T? item, Func<Exception> exFactory) where T : struct =>
            item ?? throw exFactory();
    }
}
