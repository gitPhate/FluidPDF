using System.Runtime.CompilerServices;
using System;

namespace FluidPDF.Support
{
    internal static class InternalExtensionMethods
    {
        internal static bool IsNullOrBlankString(this string? s) => string.IsNullOrWhiteSpace(s);
        internal static bool IsNotNullAndNotBlank(this string? s) => !string.IsNullOrWhiteSpace(s);
        internal static string? ToNullIfBlank(this string? s) =>
            s.IsNullOrBlankString() ? null : s;

        internal static T GetNonNullOrThrow<T>(this T? item, [CallerMemberName] string methodName = "")
        {
            if (item != null)
            {
                return item;
            }

            throw new ArgumentNullException(methodName);
        }
    }
}
