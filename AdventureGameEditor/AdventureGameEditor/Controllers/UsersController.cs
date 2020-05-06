using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdventureGameEditor.Data;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

using AdventureGameEditor.Models;
using AdventureGameEditor.Models.ViewModels.UserAutentication;
using AdventureGameEditor.Models.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AdventureGameEditor.Controllers
{
    public class UsersController : BaseController
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        protected readonly IGameEditorService _gameEditorService;
        public UsersController(AdventureGameEditorContext context, UserManager<User> userManager,
            SignInManager<User> signInManager, IGameEditorService gameEditorService) : base(context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _gameEditorService = gameEditorService;
        }

        #region Index

        // GET: Users
        public IActionResult Index()
        {
            return View();
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        #endregion

        #region Get and edit profile

        public IActionResult GetProfile()
        {
            User user = _context.User.Where(user => user.UserName == User.Identity.Name).FirstOrDefault();
            return View("Profile", new ProfileViewModel()
            {
                UserName = user.UserName,
                NickName = user.NickName,
                EmailAddress = user.Email
            });
        }

        public IActionResult GetEditUserMenuPartialView()
        {
            return PartialView("EditUserMenu");
        }

        public IActionResult EditPassword()
        {
            return View("EditPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPassword(EditPasswordViewModel editPasswordData)
        {
            if (!ModelState.IsValid)
                return View("EditPassword");
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, editPasswordData.NewPassword);
            if (result.Succeeded)
            {
                ViewBag.Succeeded = true;
                return View("EditPassword");
            }
            else
            {
                ModelState.AddModelError("", "A jelszó módosítása sikertelen volt!");
                return View("EditPassword");
            }
        }
  
        public IActionResult EditEmailAddress()
        {
            return View("EditEmailAddress");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmailAddress(EditEmailAddressViewModel editEmailData)
        {
            if (!ModelState.IsValid)
            {
                return View("EditEmailAddress");
            }
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                user.Email = editEmailData.NewEmailAddress;
                await _userManager.UpdateAsync(user);
            }
            catch
            {
                ViewBag.Succeeded = false;
                ModelState.AddModelError("", "A jelszó módosítása sikertelen volt.");
                return View("EditEmailAddress");
            }
            ViewBag.Succeeded = true;
            return View("EditEmailAddress");
            
        }

        public IActionResult EnsureDelteUser()
        {
            return View("EnsureDeleteRegistration");
        }

        public async  Task<IActionResult> DelteUser()
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "A regisztráció törlése sikertelen volt!");
                return View("EnsureDeleteRegistration");
            }
            
            // Get the user we would like to delte.
            User user = await _userManager.FindByNameAsync(User.Identity.Name);
            
            // If we couldn't find, send error message.
            if(user == null)
            {
                ModelState.AddModelError("", "A regisztráció törlése sikertelen volt!");
                return View("EnsureDeleteRegistration");
            }

            // Delte games which is visible only for this user, or unfinished.
            foreach(Game game in _context.Game.Where(g => g.Owner == user))
            {
                _gameEditorService.DeleteGame(user.UserName, game.Title);
            }
            await _context.SaveChangesAsync();
            
            // Sign out the user, then delete.
            await _signInManager.SignOutAsync();
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index", "Home");
        }


        #endregion

        #region Login and logout
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName, Password")] LoginViewModel loginData)
        {
            if (!ModelState.IsValid)
                return View("Login", loginData);
            // Log in the user.
            var result = await _signInManager.PasswordSignInAsync(loginData.UserName, loginData.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Helytelen felhasználónév vagy jelszó.");
                return View("Login", loginData);
            }
            var user = _context.User.FirstOrDefault(user => user.UserName == loginData.UserName);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Register

        // GET: Users/Register
        public IActionResult Register()
        {
            return View("Register");
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("UserName, NickName, EmailAddress, ValidateEmailAddress, Password, ValidatePassword")] RegisterViewModel registerData)
        {
            bool registerIsSucceeded = await DoRegister(registerData.UserName, registerData.NickName, registerData.EmailAddress, registerData.Password);
            if (!registerIsSucceeded)
            {
                return View("Register");
            }
            return RedirectToAction("Index", "Home");
        }

        private async Task<bool> DoRegister(String userName, String userNickName, String userEmailAddress, String userPassword)
        {
            //TODO: check repeats in database (nickname and name have to be unique)
            if (!ModelState.IsValid)
                return false;
            User user = new User
            {
                UserName = userName,
                NickName = userNickName,
                Email = userEmailAddress
            };
            if(_context.User.Any(u => u.UserName == user.UserName))
            {
                ModelState.AddModelError("", "Ez a felhasználónév már foglalt!");
            }
            if(_context.User.Any(u => u.NickName == user.NickName))
            {
                ModelState.AddModelError("", "Ez a nick név már foglalt!");
            }
            if (!ModelState.IsValid) return false;
            var result = await _userManager.CreateAsync(user, userPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return false;
            }
            await _context.SaveChangesAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);
            return true;
        }

        #endregion

        #region Private helper functions

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        #endregion
    }
}
