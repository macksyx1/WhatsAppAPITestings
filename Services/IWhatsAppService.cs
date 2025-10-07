namespace WhatsAppTestLog.Services
{
    public interface IWhatsAppService
    {
        Task<bool> SendOTPAsync(string phoneNumber, string otpCode);
    }
}
