using System.ComponentModel.DataAnnotations;

namespace TicketingApp.Models
{
    public class TicketPriority
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string TktPriority { get; set; }
    }
}
