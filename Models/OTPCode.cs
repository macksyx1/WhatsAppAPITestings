using System.ComponentModel.DataAnnotations;

namespace WhatsAppTestLog.Models
{
    public class OTPCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Code { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
