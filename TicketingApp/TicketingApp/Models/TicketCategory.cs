using System.ComponentModel.DataAnnotations;

namespace TicketingApp.Models
{
    public class TicketCategory
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public string SubCategory { get; set; }
    }
}
