namespace ProjScan
{
    internal record Message(bool Warning, string Text)
    {
        public static Message NewWarning(string text, string? exemptionCode)
        {
            if (exemptionCode is null) return new Message(true, text);
            return new Message(true, $"{text} ({exemptionCode})");
        }
    }
}