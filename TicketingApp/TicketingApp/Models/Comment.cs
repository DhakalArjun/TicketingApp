using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketingApp.Data;
namespace TicketingApp.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required(ErrorMessage ="No thing to Post !")]
        public string CommentText { get; set; }            
        public DateTime CommentDateTime { get; set; }

        //foreign keys properties
        public int TicketId { get; set; }
        public string CommentById { get; set; }

        //Navigation properties for the related reference table - foreigh keys
        [ForeignKey("TicketId")]
        public virtual Ticket CommentFor { get; set; }
        [ForeignKey("CommentById")]
        public virtual ApplicationUser CommentBy { get; set; }
    }
}
