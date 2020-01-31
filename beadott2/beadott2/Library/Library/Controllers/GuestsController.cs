using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Library.Models;
using Library.Contexts;

namespace Library.Controllers
{
    public class GuestsController : BaseController
    {
        private readonly LibraryContext _context;
        private readonly UserManager<Guest> _userManager;
        private readonly SignInManager<Guest> _signInManager;

        public GuestsController(LibraryContext context, ILibraryService libraryService,
            UserManager<Guest> userManager, SignInManager<Guest> signInManager) 
            : base(libraryService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        
        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            if (!ModelState.IsValid)
                return View("Login", user);

            // bejelentkeztetjük a felhasználót
            var result = await _signInManager.PasswordSignInAsync(user.Name, user.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Incorrect username or password.");
                return View("Login", user);
            }
            return RedirectToAction("Index", "Books");
        }        

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Books"); 
        }

        #endregion

        #region Register

        // GET: Guests/Create
        public IActionResult Create()
        {
            return View("Create");
        }
        // POST: Guests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,PhoneNumber,Password, Email")] RegisterGuestViewModel user)
        {
            // végrehajtjuk az ellenőrzéseket
            if (!ModelState.IsValid)
                return View("Create", user);

            Guest guest = new Guest
            {
                UserName = user.Name,
                Email = user.Email,
                Name = user.Name,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
            };
            var result = await _userManager.CreateAsync(guest, user.Password);
            if (!result.Succeeded)
            {
                // Felvesszük a felhasználó létrehozásával kapcsolatos hibákat.
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View("Create", user);
            }
            await _signInManager.SignInAsync(guest, false);
            return RedirectToAction("Index", "Books");
            
        }

        #endregion
    }

}
