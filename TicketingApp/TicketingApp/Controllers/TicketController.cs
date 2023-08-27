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
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RoleManager<ApplicationUser> _roleManager;
        private readonly EmailSender _emailSender;



        public TicketController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IFileService fileService, IWebHostEnvironment webHostEnvironment, EmailSender emailSender, RoleManager<ApplicationUser> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _fileService = fileService;
            _webHostEnvironment = webHostEnvironment;           
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        //user ongoing request -- for all user
        public async Task<IActionResult> OnGoingRequests()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User); 
            var requests = await _context.Ticket
                .Where(req => req.CreatedById == currentUser.Id && req.StatusId < 5)
                .Include(req=>req.Category)
                .Include(req=> req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.PageHeading = "My Ongoing Requests";          
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
            ViewBag.PageHeading = "List of Request Need To Be Assigned";
            return View();
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
            ViewBag.PageHeading = "List of Ongoing Tasks";
            return View();
        }

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> RequestNeedToBeClosed()
        {
            var requests = await _context.Ticket
                .Where(req => req.StatusId ==3 || req.StatusId ==4)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.PageHeading = "List of Task Need To Be Closed";
            return View();
        }

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> AllClosedRequests()
        {
            var requests = await _context.Ticket
                .Where(req => req.StatusId >= 5)
                .OrderByDescending(req => req.CreatedDateTime)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.PageHeading = "Closed Requests";
            return View();
        }

        [Authorize(Roles = "Admin, Manager, Agent")]
        public async Task<IActionResult> ClosedRequestsAssignToMe()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var requests = await _context.Ticket
                .Where(req => req.StatusId >= 5 && req.AssignedToId==currentUser.Id)
                .OrderByDescending(req => req.CreatedDateTime)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.PageHeading = "Closed Requests";
            return View("ClosedRequests");
        }

        [Authorize(Roles = "Admin, Manager, Agent")]
        public async Task<IActionResult> TaskAssignedToMe()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var requests = await _context.Ticket
                .Where(req => req.StatusId == 2 && req.AssignedToId == currentUser.Id)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.PageHeading = "List of Tasks Assigned To Me";
            return View();
        }

        [Authorize(Roles = "Admin, Manager, Agent")]
        //List of my tasks that completed but not closed
        public async Task<IActionResult> TaskToCloseAssignedToMe()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser != null)
            {
                var requests = await _context.Ticket
                .Where(req => (req.StatusId == 3 || req.StatusId == 4) && req.AssignedToId == currentUser.Id)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();
                ViewBag.Requests = requests;
                ViewBag.PageHeading = "List of Request Completed But Not Closed";
            }            
            return View();
        }

        //List of closed request those are created by me       
        public async Task<IActionResult> ClosedRequests()
        {
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var requests = await _context.Ticket
                .Where(req => req.CreatedById == currentUser.Id && req.StatusId >= 5)
                .Include(req => req.Category)
                .Include(req => req.Status)
                .ToListAsync();

            ViewBag.Requests = requests;
            ViewBag.PageHeading = "My Closed Request";
            return View();
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
        public async Task<IActionResult> Create()
        {
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
                var managerRole = await _roleManager.FindByNameAsync("Manager");
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
                if(managerRole != null && currentUser !=null)
                {            
                    string subject = "New Requested Created";
                    string messageToManager = "Hello, \nNew new requested has been created for \n Service: " + ticket.Category.Category + " \n Category : " + ticket.Category.SubCategory + "\n Created By: " + currentUser.FirstName + " " + currentUser.LastName + "(" + currentUser.Email + ")" + "\nPriority: " + ticket.Priority.TktPriority;
                    string messageToUser = "Hello, \n Your requested has been created successfully \n Service: " + ticket.Category.Category + " \n Category : " + ticket.Category.SubCategory + "\nTo view the progress on your request, you can go click View Ongoing Requests";

                    await _emailSender.SendEmailAsync(managerRole.Email, subject, messageToManager);
                    await _emailSender.SendEmailAsync(currentUser.Email, subject, messageToUser);
                }
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
            if(currentUser != null)
            {
                ViewBag.CurUserId = currentUser.Id;
            } 
            var category = await _context.TicketCategories.FindAsync(ticket.CategoryId);
            if(category != null)
            {
                ticket.Category = category;
            }            
            var priority = await _context.TicketPriorities.FindAsync(ticket.PriorityId);
            if(priority != null)
            {
                ticket.Priority = priority;
            }
            
            var location = await _context.TicketLocations.FindAsync(ticket.LocationId);
            if(location != null)
            {
                ticket.Location = location;
            }
            
            var status = await _context.TicketStatuses.FindAsync(ticket.StatusId);
            if(status != null)
            {
                ticket.Status = status;
            }
           
            if (ticket.CreatedById != null)
            {
                var createdBy = await _userManager.FindByIdAsync(ticket.CreatedById);
                ticket.CreatedBy= createdBy;
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
                var managerRole = await _roleManager.FindByNameAsync("Manager");
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
                            var assignedToRole = await _roleManager.FindByIdAsync(ticket.AssignedToId);
                            string subject = "New Task Assigned";                               
                                string message = "Hello, \n A task has been assigned to you, details of the task: \n Service: " + ticket.Category.Category + " \n Category : " + ticket.Category.SubCategory +  "\nPriority: " + ticket.Priority.TktPriority + "\nTo view the details of task, you can click 'View Task Just Assigned'";
                                await _emailSender.SendEmailAsync(assignedToRole.Email, subject, message);
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
                        var assignedToRole2 = await _roleManager.FindByIdAsync(ticket.AssignedToId);
                        string subject2 = "New Task Assigned";
                        string message2 = "Hello, \n A task has been assigned to you, details of the task:  \n Service: " + ticket.Category.Category + " \n Category : " + ticket.Category.SubCategory + "\nPriority: " + ticket.Priority.TktPriority + "\nTo view the details of task, you can click 'View Task Just Assigned'";
                        await _emailSender.SendEmailAsync(assignedToRole2.Email, subject2, message2);
                        break;
                    case "Close as Not Resolvable":    //request close although unresolved                  
                        existingTicket.ClosedById = currentUser.Id;
                        existingTicket.StatusId = 1001; //Note: later it need to change to 6 :1001 is due to current id in database
                        existingTicket.ClosedDateTime = DateTime.Now;
                        existingTicket.ClosingComment = ticket.ClosingComment;
                        _context.SaveChanges();
                        var assignedToRole3 = await _roleManager.FindByIdAsync(ticket.AssignedToId);
                        string subject3 = "Your Request is closed - issue not resolvable";
                        string message3 = "Hello, \n Your requested has been closed but unfortunately the issue cannot be closed. Request details was: \n Service: " + ticket.Category.Category + " \n Category : " + ticket.Category.SubCategory + "\nPriority: " + ticket.Priority.TktPriority + "\nTo view the details of task, you can click 'View Task Just Assigned'";
                        await _emailSender.SendEmailAsync(assignedToRole3.Email, subject3, message3);
                        break;


                        break;
                    case "Close":   //request closed after resolution
                        if (ticket.StatusId != 3 && ticket.ClosingComment.Length > 0)
                        {
                            existingTicket.ClosedById = currentUser.Id;
                            existingTicket.StatusId = 5;
                            existingTicket.ClosedDateTime = DateTime.Now;
                            existingTicket.ClosingComment = ticket.ClosingComment;
                            _context.SaveChanges();
                            var assignedToRole4 = await _roleManager.FindByIdAsync(ticket.AssignedToId);
                            string subject4 = "Your Request is closed - issue resolved";
                            string message4 = "Hello, \n Your requested has been fullfilled and closed. Request details was: \n Service: " + ticket.Category.Category + " \n Category : " + ticket.Category.SubCategory + "\nPriority: " + ticket.Priority.TktPriority + "\nTo view the details of task, you can click 'View Task Just Assigned'";
                            await _emailSender.SendEmailAsync(assignedToRole4.Email, subject4, message4);
                        }                        
                        break;
                    default:
                        //code for default - will never happlen
                        break;
                }
            }         
            return RedirectToAction("EditTicket", new {id = ticket.TicketId});
        }

       
        public IActionResult OpenFile(string relativePath, string fileName)  //This action current not in use
        {
            try
            {
                string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, relativePath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    string fileContent = System.IO.File.ReadAllText(filePath);
                    return Content(fileContent);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        public IActionResult ViewFile(string relativePath, string fileName)
        {
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, relativePath, fileName);

            if (System.IO.File.Exists(filePath))
            {              
                var mimeType = GetMimeType(fileName);
                return PhysicalFile(filePath, mimeType); 
            }
            else
            {
                return NotFound();
            }
        }

        private string GetMimeType(string fileName)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var mimeType))
            {
                mimeType = "application/octet-stream"; // Default MIME type
            }
            return mimeType;
        }
    }
}
