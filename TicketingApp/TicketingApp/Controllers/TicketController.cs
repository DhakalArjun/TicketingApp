using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TicketingApp.Data;
using TicketingApp.Models;
using TicketingApp.Services;

namespace TicketingApp.Controllers
{
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileService _fileService;

        public TicketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _fileService = fileService;
        }

        // GET: Ticket
        public async Task<IActionResult> Index()
        {
            return _context.Ticket != null ?
                        View(await _context.Ticket.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Ticket'  is null.");
        }

        // GET: Ticket/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Ticket == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        public class MyViewModel
        {
            public TicketCategory? Category { get; set; }
            //public List<TicketLocation> Locations { get; set; }
            //public List<TicketPriority> Priorities { get; set; } 
            public int SelectedLocation { get; set; }
            [Required(ErrorMessage = "Please select a priority")]            
            [Display(Name = "Priority")]
            public int SelectedPriority { get; set; }
            public int SelectedCategory { get; set; }
            [Display(Name = "Attachement (If any)")]
            public IFormFile? AttachFile { get; set; }
            [Required(ErrorMessage = "Title is required")]
            [Display(Name = "Title")]
            public string TicketTitle { get; set; }
            [Required(ErrorMessage = "Description is required")]           
            [Display(Name = "Description")]
            public string TicketDesc { get; set; }
        }
        
        
        // GET: Ticket/Create
        public async Task<IActionResult> Create()        {
            List<TicketCategory> Categories = await _context.TicketCategories.ToListAsync(); 
            return View(Categories);
        }

        // GET: Ticket/CreateTicket/{category_id}
        public async Task<IActionResult> CreateTicket(int category_id)
        {
            var category = await _context.TicketCategories.FindAsync(category_id);
            var locations = await _context.TicketLocations.ToListAsync();
            var priorities = await _context.TicketPriorities.ToListAsync();
            
            ViewBag.Locations = locations;
            ViewBag.Priorities = priorities;           

            var viewModel = new MyViewModel();
            if(category !=null) viewModel.Category = category;
                              
            return View(viewModel);
        }


        // POST: Ticket/Create       
        //[Bind("TicketId,Title,Description,CreatedDateTime,CreatedById,StatusId,CategoryId,LocationId,PriorityId")]
        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> CreateTicket(MyViewModel vm)
        {
            
            Ticket ticket = new Ticket();
            if (vm.AttachFile != null)
            {
                var result = _fileService.SaveImage(vm.AttachFile);
                if (result.Item1 == 1)
                {
                    var oldImage = ticket.TicketAttachement;
                    ticket.TicketAttachement = result.Item2;
                    await _context.SaveChangesAsync();                   
                    var deleteResult = _fileService.DeleteImage(oldImage);
                }
            }

            if (vm.SelectedPriority == 0)
            {
                ModelState.AddModelError("SelectedPriority", "Select a priority");
            }
            if (vm.SelectedLocation == 0 )
            {
                ModelState.AddModelError("SelectedLocation", "Select a location");
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                ticket.Title = vm.TicketTitle;
                ticket.Description = vm.TicketDesc;
                ticket.CreatedById = currentUser.Id;
                ticket.CreatedDateTime = DateTime.Now;
                ticket.StatusId = 1;
                ticket.CategoryId = vm.SelectedCategory;
                ticket.LocationId = vm.SelectedLocation;
                ticket.PriorityId = vm.SelectedPriority;
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var locations = await _context.TicketLocations.ToListAsync();
            var priorities = await _context.TicketPriorities.ToListAsync();

            ViewBag.Locations = locations;
            ViewBag.Priorities = priorities;
            var category = await _context.TicketCategories.FindAsync(vm.SelectedCategory);
            vm.Category  = category;
            return View(vm);
            
        }

        // GET: Ticket/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Ticket == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Ticket/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,Title,Created,Description,AssignedDateTime,ResolvedDateTime")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Ticket/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Ticket == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Ticket/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Ticket == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Ticket'  is null.");
            }
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket != null)
            {
                _context.Ticket.Remove(ticket);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
          return (_context.Ticket?.Any(e => e.TicketId == id)).GetValueOrDefault();
        }
    }
}
