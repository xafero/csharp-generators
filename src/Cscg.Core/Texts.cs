namespace Cscg.Core
{
    public static class Texts
    {
        public const string NewLine = "\n";

        public static string TrimNull(this string text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        public static string ToTitle(this string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }

        public static string ToString(bool? value)
        {
            if (value is { } b) return b ? "true" : "false";
            return null;
        }
    }
}