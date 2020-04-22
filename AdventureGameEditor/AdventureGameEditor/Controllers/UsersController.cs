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

namespace AdventureGameEditor.Controllers
{
    public class UsersController : BaseController
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public UsersController(AdventureGameEditorContext context, UserManager<User> userManager,
            SignInManager<User> signInManager):base(context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            //TODO: check repeats in database (nickname and name hace to be uniq)
            if (!ModelState.IsValid)
                return false;
            User user = new User
            {
                UserName = userName,
                NickName = userNickName,
                Email = userEmailAddress
            };
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
