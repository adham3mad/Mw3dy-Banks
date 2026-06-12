using System;

namespace Mw3dy.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string Service { get; set; } = string.Empty; // "account" | "loan" | "mortgage" | "wealth" | "cards" | "business"
        
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public string Date { get; set; } = string.Empty; // yyyy-MM-dd
        public string Time { get; set; } = string.Empty; // HH:mm
        public string? Notes { get; set; }
        
        public string Status { get; set; } = "confirmed"; // confirmed | cancelled | completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
