using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Connections.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Connections.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Connections.Models.HomeViewModels;

namespace Connections.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;

        }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(HomeController.About));
            }
            else
                return
                    View();
        }

        [Authorize]
        public async Task<IActionResult> About()
        {
            AboutViewModel viewModel = new AboutViewModel();

            ApplicationUser currentUser = await _userManager.GetUserAsync(HttpContext.User);

            viewModel.Description = currentUser.Description;

            // Load the users for each confirmed friend
            List<ApplicationUser> friendsUsers = new List<ApplicationUser>();
            var friends = _context.Friends.Where(f => f.UserID == currentUser.Id && f.StatusOfRequest);
            foreach (Friend f in friends.ToList())
            {
                var appUser = _context.ApplicationUser.Find(f.FriendID);
                friendsUsers.Add(appUser);
            }

            friends = _context.Friends.Where(f => f.FriendID == currentUser.Id && f.StatusOfRequest);
            foreach (Friend f in friends.ToList())
            {
                var appUser = _context.ApplicationUser.Find(f.UserID);
                friendsUsers.Add(appUser);
            }

            viewModel.Friends = friendsUsers;

            // Load the users for each pending invite
            var pendingInvites = _context.Friends.Where(f => f.FriendID == currentUser.Id && !f.StatusOfRequest);
            List<ApplicationUser> pendingInvitesUsers = new List<ApplicationUser>();

            foreach (Friend f in pendingInvites.ToList())
            {
                var appUser = _context.ApplicationUser.Find(f.UserID);
                pendingInvitesUsers.Add(appUser);
            }

            viewModel.Invites = pendingInvitesUsers;

            // Still need to pass the data into the views and make them display everything properly
            return View(viewModel);
        }

        
        [Authorize]
        public async Task<IActionResult> Users()
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(HttpContext.User);

            // Only show users that are not self and not already friends                
            List<Friend> friends = _context.Friends
                                            .Where(f => f.UserID == currentUser.Id || f.FriendID == currentUser.Id)
                                            .ToList();

            List<int> userIdsToNotShow = new List<int>();
            foreach (Friend f in friends)
            {
                userIdsToNotShow.Add(f.FriendID);
                userIdsToNotShow.Add(f.UserID);
            }

            var ids = userIdsToNotShow.ToArray();
            var users = _context.ApplicationUser.Where(u => !ids.Contains(u.Id));

            return View(users.ToList());
        }

        [Authorize]
        public async Task<IActionResult> Connect(int friendId)
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var friend = _context.Friends.Where(f => f.FriendID == friendId && f.UserID == currentUser.Id);
            if (0 == friend.Count())
            {
                Friend newFriend = new Friend();
                newFriend.UserID = currentUser.Id;
                newFriend.FriendID = friendId;

                _context.Friends.Add(newFriend);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(HomeController.About));
        }

        public async Task<IActionResult> Accept(int userId)
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var friend = await _context.Friends
                                       .Where(f => f.UserID == userId && f.FriendID == currentUser.Id)
                                       .SingleOrDefaultAsync();

            friend.StatusOfRequest = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(HomeController.About));
        }

        public async Task<IActionResult> Ignore(int userId)
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var friend = await _context.Friends
                                       .Where(f => f.UserID == userId && f.FriendID == currentUser.Id)
                                       .SingleOrDefaultAsync();

            _context.Friends.Remove(friend);
            _context.SaveChanges();

            return RedirectToAction(nameof(HomeController.About));
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.ApplicationUser
               .AsNoTracking()
               .SingleOrDefaultAsync(m => m.Id == id);

            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}