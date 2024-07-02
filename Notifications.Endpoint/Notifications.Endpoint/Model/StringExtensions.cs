namespace Notifications.Endpoint.Model
{
    public static class StringExtensions
    {
        public static string PrepareForChatBot(this string text)
        {
            return text.Replace("_", "\\_")
                .Replace("*", "\\*")
                .Replace("[", "\\[")
                .Replace("`", "\\`")
                .Replace("#", "\\#")
                .Replace("!", "\\!");
        }
    }
}
