using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        //user ongoing request -- for all roles
        public async Task<IActionResult> OnGoingRequests()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User); 
            var requests = await _context.Ticket
                .Where(req => req.CreatedById == currentUser.Id && req.StatusId < 5)
                .Include(req=>req.Category)
                .Include(req=> req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            return View();            
        }

        //admin request to be assigned

        [Authorize(Roles ="Admin, Manager")]
        public async Task<IActionResult> OnBeAssigned()
        {           
            var requests = await _context.Ticket
                .Where(req => req.StatusId ==1)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            return View("OnGoingRequests");
        }

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> OnGoingReqestsAdmin()
        {
            var requests = await _context.Ticket
                .Where(req => req.StatusId > 1 && req.StatusId < 5)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            return View();
        }

        public async Task<IActionResult> TaskAssignedToMe()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var requests = await _context.Ticket
                .Where(req => req.StatusId == 2 && req.AssignedToId == currentUser.Id)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            return View();
        }

        public async Task<IActionResult> TaskToCloseAssignedToMe()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var requests = await _context.Ticket
                .Where(req => (req.StatusId == 3 || req.StatusId==4) && req.AssignedToId == currentUser.Id)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            return View();
        }


        public async Task<IActionResult> ClosedRequests()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var requests = await _context.Ticket
                .Where(req => req.CreatedById == currentUser.Id && req.StatusId >= 5)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            return View();
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


        // POST: Ticket/CreateTicket      
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
                return RedirectToAction(nameof(OnGoingRequests));
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

        //Detail View, Assignment, Resolution, Closure


        // GET: Ticket/Details/5
        public async Task<IActionResult> TicketDetails(int? id)
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
            var category = await _context.TicketCategories.FindAsync(ticket.CategoryId);
            ticket.Category = category;
            var priority = await _context.TicketPriorities.FindAsync(ticket.PriorityId);
            ticket.Priority = priority;
            var location = await _context.TicketLocations.FindAsync(ticket.LocationId);
            ticket.Location = location;
            var status = await _context.TicketStatuses.FindAsync(ticket.StatusId);
            ticket.Status = status;
            if (ticket.CreatedById != null)
            {
                var createdBy = await _userManager.FindByIdAsync(ticket.CreatedById);
                ticket.AssignedTo = createdBy;
            }
            if (ticket.AssignedToId != null)
            {
                var assignTo = await _userManager.FindByIdAsync(ticket.AssignedToId);
                ticket.AssignedTo = assignTo;
            }
            if (ticket.AssignedById != null)
            {
                var assignBy = await _userManager.FindByIdAsync(ticket.AssignedById);
                ticket.AssignedBy = assignBy;
            }
            if (ticket.ClosedById != null)
            {
                var closedBy = await _userManager.FindByIdAsync(ticket.ClosedById);
                ticket.ClosedBy = closedBy;
            }
            return View(ticket);
        }

        // GET: Ticket/Details/5
        public async Task<IActionResult> EditTicket(int? id)
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
            var agentOrManager = await (from u in _context.Users
                                      join ur in _context.UserRoles
                                      on u.Id equals ur.UserId
                                      join r in _context.Roles
                                      on ur.RoleId equals r.Id
                                      select new
                                      {
                                          u.Id,
                                          u.UserName,
                                          u.FirstName,
                                          u.LastName,
                                          r.Name,                                         
                                      })
                                      .Where(r=>r.Name=="Agent" || r.Name=="Admin" || r.Name=="Manager")                                      
                                      .ToListAsync();
            ViewBag.AgentOrManager = agentOrManager;
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            ViewBag.CurUserId = currentUser.Id;

            var category = await _context.TicketCategories.FindAsync(ticket.CategoryId);
            ticket.Category = category;
            var priority = await _context.TicketPriorities.FindAsync(ticket.PriorityId);
            ticket.Priority = priority;
            var location = await _context.TicketLocations.FindAsync(ticket.LocationId);
            ticket.Location = location;
            var status = await _context.TicketStatuses.FindAsync(ticket.StatusId);
            ticket.Status = status;
            if (ticket.CreatedById != null)
            {
                var createdBy = await _userManager.FindByIdAsync(ticket.CreatedById);
                ticket.AssignedTo = createdBy;
            }
            if (ticket.AssignedToId != null)
            {
                var assignTo = await _userManager.FindByIdAsync(ticket.AssignedToId);
                ticket.AssignedTo = assignTo;
            }
            if (ticket.AssignedById != null)
            {
                var assignBy = await _userManager.FindByIdAsync(ticket.AssignedById);
                ticket.AssignedBy = assignBy;
            }
            if (ticket.ClosedById != null)
            {
                var closedBy = await _userManager.FindByIdAsync(ticket.ClosedById);
                ticket.ClosedBy = closedBy;
            }
            return View(ticket);
        }

        // GET: Ticket/Edit/5
        [HttpPost]
        public async Task<IActionResult> EditTicket(Ticket ticket, string submitButton)
        {      
            if (ticket == null)
            {
                return NotFound();
            }
            else
            {
                var existingTicket = _context.Ticket.Find(ticket.TicketId);
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                switch (submitButton)
                {                 
                    case "Assign": //task assignment
                        if(ticket.AssignedToId != null)
                        {
                            existingTicket.AssignedToId = ticket.AssignedToId;
                            existingTicket.StatusId = 2;
                            existingTicket.AssignedById = currentUser.Id;
                            existingTicket.AssignedDateTime = DateTime.Now;                           
                            _context.SaveChanges();
                        }                                                                    
                        break;                    
                    case "Save": //Resolution
                        if(ticket.StatusId !=2 && ticket.ResolutionComment.Length>0)
                        {
                            existingTicket.ResolutionComment = ticket.ResolutionComment;
                            existingTicket.ResolvedDateTime = DateTime.Now;
                            existingTicket.StatusId = ticket.StatusId;
                            _context.SaveChanges();
                        } 
                        break;
                    case "Reassign":   //task reassignment                     
                        existingTicket.AssignedToId = ticket.AssignedToId;
                        existingTicket.StatusId = 2;
                        existingTicket.AssignedById = currentUser.Id;
                        existingTicket.AssignedDateTime = DateTime.Now;
                        existingTicket.ResolutionComment = null;
                        existingTicket.ResolvedDateTime = null;
                        _context.SaveChanges();
                        break;
                    case "Close as Not Resolvable":    //request close although unresolved                  
                        existingTicket.ClosedById = currentUser.Id;
                        existingTicket.StatusId = 1001; //Note: later it need to change to 6 :1001 is due to current id in database
                        existingTicket.ClosedDateTime = DateTime.Now;
                        existingTicket.ClosingComment = ticket.ClosingComment;
                        _context.SaveChanges();
                        break;
                    case "Close":   //request closed after resolution
                        if (ticket.StatusId != 3 && ticket.ClosingComment.Length > 0)
                        {
                            existingTicket.ClosedById = currentUser.Id;
                            existingTicket.StatusId = 5;
                            existingTicket.ClosedDateTime = DateTime.Now;
                            existingTicket.ClosingComment = ticket.ClosingComment;
                            _context.SaveChanges();
                        }                        
                        break;
                    default:
                        //code for default - will never happlen
                        break;
                }
            }         
            return RedirectToAction("EditTicket", new {id = ticket.TicketId});
        }
    }
}
