using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using SendGrid.Helpers.Mail;
using System.ComponentModel.DataAnnotations;
using TicketingApp.Areas.Identity.Pages.Account.Manage;
using TicketingApp.Data;

namespace TicketingApp.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AppUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
     

        public AppUserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
          
        }
        public async Task<IActionResult> Index()
        {
            var appUserRoles = await (from u in _dbContext.Users
                                      join ur in _dbContext.UserRoles
                                      on u.Id equals ur.UserId
                                      join r in _dbContext.Roles
                                      on ur.RoleId equals r.Id
                                      select new
                                      {
                                          u.Id,
                                          u.UserName,
                                          u.FirstName,
                                          u.LastName,
                                          r.Name
                                      }).ToListAsync();
            ViewBag.AppUserRoles = appUserRoles;            
            return View();
        }

        public UpdateUserRoleViewModel Input { get; set; }

        public class UpdateUserRoleViewModel
        {
            [Display(Name = "User Id")]
            public string UserId { get; set; }
            [Display(Name = "Username")]
            public string UserName { get; set; }
            [Display(Name = "Current Role")]
            public string CurrentRole { get; set; }
            [Required]
            [MinLength(2)]
            [Display(Name = "New Role")]
            public string NewRole { get; set; }
            [Required]
            public IEnumerable<IdentityRole> AllRoles { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUserRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles;

            Input = new UpdateUserRoleViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                CurrentRole = userRoles.FirstOrDefault(),
                NewRole = "", 
                AllRoles = allRoles,
            };

            return View(Input);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            if (!string.IsNullOrEmpty(newRole))
            {  
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var existingRoles = await _userManager.GetRolesAsync(user);
                var roleExists = await _roleManager.RoleExistsAsync(newRole);
                foreach (var role in existingRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }

            
                if (!roleExists)
                {
                    // You might want to handle the case where the role doesn't exist
                    return BadRequest("Role doesn't exist");
                }

                await _userManager.AddToRoleAsync(user, newRole);

                return RedirectToAction("Index");
            }
            else
            {
                //return BadRequest("Select New Role");
                //return RedirectToAction("UpdateUserRole", new {userId = userId});

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles;

                Input = new UpdateUserRoleViewModel
                {
                    UserId = userId,
                    UserName = user.UserName,
                    CurrentRole = userRoles.FirstOrDefault(),
                    NewRole = "",
                    AllRoles = allRoles,
                };                
                return View(Input);                              
            }
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var usr = await _dbContext.Users.FindAsync(id);

            if (usr != null)
            {
                var usrRole = await _userManager.GetRolesAsync(usr);
                if(!usrRole.Equals("Admin"))
                {
                    _dbContext.Users.Remove(usr);
                    await _dbContext.SaveChangesAsync();
                } 
            }
            return RedirectToAction("Index");
        }
    }
}
