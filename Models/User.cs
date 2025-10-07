using System.ComponentModel.DataAnnotations;

namespace WhatsAppTestLog.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public bool IsVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }
    }
}
