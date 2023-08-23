using System.ComponentModel.DataAnnotations;

namespace TicketingApp.Models
{
    public class TicketStatus
    {
        [Key]
        public int StatusId { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
