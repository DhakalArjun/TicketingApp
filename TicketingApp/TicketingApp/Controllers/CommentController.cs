using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TicketingApp.Data;
using TicketingApp.Models;

namespace TicketingApp.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index(int ticketId)
        {
            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.TicketId == ticketId);
            var comments = await _context.Comment
                .Where(comment => comment.TicketId == ticketId)
                .OrderBy(comment => comment.CommentDateTime)
                .Include(comment => comment.CommentBy)
                .ToListAsync();
            ViewBag.PrevComments = comments;        
            var NewComment = new TicketingApp.Models.Comment();
            NewComment.TicketId = ticketId; 
            return View(NewComment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int ticketId, Comment com)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                com.CommentById = currentUser.Id;
                com.CommentDateTime = DateTime.Now;

                if (!ModelState.IsValid)
                { 
                    _context.Comment.Add(com);
                    await _context.SaveChangesAsync();                   
                }
                return RedirectToAction("Index", new { ticketId = com.TicketId });
            }
            catch (Exception ex)
            {
                // Log or debug the exception
                Debug.WriteLine(ex.Message); // Or use your preferred logging method

                // Return to the view with an error message
                ModelState.AddModelError(string.Empty, "An error occurred while adding the comment.");
                return View("Index");
            }
        }






    }
}
