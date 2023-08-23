using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TicketingApp.Models
{
    public class TicketLocation
    {
        [Key]        
        public int LocId { get; set; }
        [Required]       
        public string Location { get; set; }
    }
}
