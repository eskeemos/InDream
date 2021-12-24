namespace System.Templates
{
    public class Exchange
    {
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string  ApiSecret { get; set; }
        public bool KeysProvided
            => !string.IsNullOrWhiteSpace(ApiSecret) || !string.IsNullOrWhiteSpace(ApiKey);
    }
}
