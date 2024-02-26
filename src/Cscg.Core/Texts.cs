namespace Cscg.Core
{
    public static class Texts
    {
        public static string TrimNull(this string text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        public static string ToTitle(this string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }

        public static string Combine(string space, string name)
        {
            var full = $"{space}.{name}";
            return full;
        }
    }
}