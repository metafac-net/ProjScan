namespace ProjScan
{
    internal record Message(bool Warning, string Text)
    {
        public static Message NewWarning(string text) => new Message(true, text);
    }
}