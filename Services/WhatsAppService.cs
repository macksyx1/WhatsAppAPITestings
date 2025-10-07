using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace WhatsAppTestLog.Services
{
    public class WhatsAppService: IWhatsAppService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _whatsappFromNumber;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _accountSid = configuration["Twilio:AccountSID"];
            _authToken = configuration["Twilio:AuthToken"];
            _whatsappFromNumber = configuration["Twilio:WhatsAppFromNumber"];
            _logger = logger;

            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task<bool> SendOTPAsync(string phoneNumber, string otpCode)
        {
            try
            {
                // Format phone number for WhatsApp (ensure it includes country code)
                var formattedNumber = FormatPhoneNumber(phoneNumber);

                var message = await MessageResource.CreateAsync(
                    body: $"Your verification code is: {otpCode}. This code will expire in 10 minutes.",
                    from: new Twilio.Types.PhoneNumber(_whatsappFromNumber),
                    to: new Twilio.Types.PhoneNumber(formattedNumber)
                );

                _logger.LogInformation($"OTP sent to {phoneNumber}. Message SID: {message.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP to {phoneNumber}");
                return false;
            }
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove any non-digit characters
            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Ensure it starts with country code (example: for US, ensure +1 prefix)
            // You might want to customize this based on your requirements
            if (!digitsOnly.StartsWith("1") && digitsOnly.Length == 10)
            {
                digitsOnly = "1" + digitsOnly;
            }

            return $"whatsapp:+{digitsOnly}";
        }
    }
}
