namespace Cscg.Core
{
    public static class Texts
    {
        public static string TrimNull(this string text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }
    }
}