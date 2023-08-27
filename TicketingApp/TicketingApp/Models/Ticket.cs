using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketingApp.Data;

namespace TicketingApp.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }
        [Required]
        public string Title { get; set; }        
        public DateTime CreatedDateTime { get; set; }
        [Required]
        public string Description { get; set; }         
        public DateTime? AssignedDateTime { get; set; }
        public DateTime? ResolvedDateTime { get; set; }
        public DateTime? ClosedDateTime { get; set; }
        public string? TicketAttachement { get; set; }
        [Display(Name = "Resolution Description")]
        public string? ResolutionComment { get; set; }
        [Display(Name = "Closing Description")]

        public string? ClosingComment { get; set; }




        //foreign keys properties
        
        public string CreatedById { get; set; }
        public int StatusId { get; set; }
        public int CategoryId { get; set; }
        [Required]
        public int LocationId { get; set; }
        [Required]
        public int PriorityId { get; set; }
        public string? AssignedToId { get; set; }
        public string? AssignedById { get; set; }
        public string? ClosedById { get; set; }


        //Navigation properties for the related reference table - foreigh keys
        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; }
        [ForeignKey("StatusId")]
        public virtual TicketStatus Status { get; set; }
        [ForeignKey("CategoryId")]
        public virtual TicketCategory Category { get; set; }
        [ForeignKey("LocationId")]
        public virtual TicketLocation Location { get; set; }
        [ForeignKey("PriorityId")]
        public virtual TicketPriority Priority { get; set; }
        [ForeignKey("AssignedToId")]
        public virtual ApplicationUser? AssignedTo { get; set; }
        [ForeignKey("AssignedById")]
        public virtual ApplicationUser? AssignedBy { get; set; }
        [ForeignKey("ClosedById")]
        public virtual ApplicationUser? ClosedBy { get; set; }
    }
}
