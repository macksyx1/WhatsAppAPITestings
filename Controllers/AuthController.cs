using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatsAppTestLog.Data;
using WhatsAppTestLog.Models;
using WhatsAppTestLog.Services;

namespace WhatsAppTestLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext context,
            IWhatsAppService whatsAppService,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate phone number
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return BadRequest(new AuthResponse { Success = false, Message = "Phone number is required" });
                }

                // Clean phone number
                var cleanPhoneNumber = new string(request.PhoneNumber.Where(char.IsDigit).ToArray());

                // Find or create user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == cleanPhoneNumber);

                if (user == null)
                {
                    user = new User
                    {
                        PhoneNumber = cleanPhoneNumber,
                        IsVerified = false
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Generate OTP
                var otpCode = GenerateOTP();
                var otpEntry = new OTPCode
                {
                    PhoneNumber = cleanPhoneNumber,
                    Code = otpCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false
                };

                // Remove any existing OTPs for this number
                var existingOtps = await _context.OTPCodes
                    .Where(o => o.PhoneNumber == cleanPhoneNumber && !o.IsUsed)
                    .ToListAsync();

                _context.OTPCodes.RemoveRange(existingOtps);
                _context.OTPCodes.Add(otpEntry);
                await _context.SaveChangesAsync();

                // Send OTP via WhatsApp
                var sent = await _whatsAppService.SendOTPAsync(cleanPhoneNumber, otpCode);

                if (!sent)
                {
                    return StatusCode(500, new AuthResponse { Success = false, Message = "Failed to send OTP" });
                }

                return Ok(new AuthResponse { Success = true, Message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<AuthResponse>> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Code))
                {
                    return BadRequest(new AuthResponse { Success = false, Message = "Phone number and code are required" });
                }

                var cleanPhoneNumber = new string(request.PhoneNumber.Where(char.IsDigit).ToArray());
                var cleanCode = request.Code.Trim();

                // Find valid OTP
                var otp = await _context.OTPCodes
                    .Where(o => o.PhoneNumber == cleanPhoneNumber &&
                               o.Code == cleanCode &&
                               !o.IsUsed &&
                               o.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    return BadRequest(new AuthResponse { Success = false, Message = "Invalid or expired OTP" });
                }

                // Mark OTP as used
                otp.IsUsed = true;

                // Find user and update verification status
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == cleanPhoneNumber);

                if (user == null)
                {
                    return BadRequest(new AuthResponse { Success = false, Message = "User not found" });
                }

                user.IsVerified = true;
                user.LastLogin = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = _tokenService.GenerateToken(user);
                var tokenExpiry = DateTime.UtcNow.AddMinutes(60); // Match your JWT expiry

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "OTP verified successfully",
                    Token = token,
                    Expires = tokenExpiry
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP verification");
                return StatusCode(500, new AuthResponse { Success = false, Message = "An error occurred" });
            }
        }

        private string GenerateOTP(int length = 6)
        {
            var random = new Random();
            var otp = string.Empty;

            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10).ToString();
            }

            return otp;
        }
    }
}
