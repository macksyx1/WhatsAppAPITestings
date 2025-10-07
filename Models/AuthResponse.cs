namespace WhatsAppTestLog.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public DateTime? Expires { get; set; }
    }
}
