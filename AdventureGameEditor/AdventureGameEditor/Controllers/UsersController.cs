﻿using System;
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
        private readonly AdventureGameEditorContext _context;

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public UsersController(AdventureGameEditorContext context, UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region Index

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.User.ToListAsync());
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
            // bejelentkeztetjük a felhasználót
            var result = await _signInManager.PasswordSignInAsync(loginData.UserName, loginData.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Helytelen felhasználónév vagy jelszó.");
                return View("Login", loginData);
            }
            var user = _context.User.FirstOrDefault(user => user.UserName == loginData.UserName);
            await _signInManager.SignInAsync(user, isPersistent: false);
            
            Trace.WriteLine("\n\n\n" + String.IsNullOrEmpty(User.Identity.Name) + "\n\n\n");
            Trace.WriteLine(loginData.UserName + " " + loginData.Password + "\n\n\n");
            Trace.WriteLine("User.Identity.Name " + User.Identity.Name);
            Trace.WriteLine("User.Identity.IsAuthenticated " + User.Identity.IsAuthenticated);
            Trace.WriteLine("User.Identity.AuthenticationType " + User.Identity.AuthenticationType);
            Trace.WriteLine("User.Identity.ToString " + User.Identity.ToString());


            //TODO: make other page to redirect to            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            //TODO: make other page to redirect to
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Register

        // GET: Users/Register
        public IActionResult Register()
        {
            return View("Register");
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("UserName, NickName, EmailAddress, ValidateEmailAddress, Password, ValidatePassword")] RegisterViewModel registerData)
        {
            bool registerIsSucceeded = await DoRegister(registerData.UserName, registerData.NickName, registerData.EmailAddress, registerData.Password);
            if (registerIsSucceeded)
            {
                return View("Register");
            }
            return View("Register", registerData);
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
            //TODO: ennél azért valami profibbat de kezdetben ez is ok lesz.
            ModelState.AddModelError("", "A regsiztráció sikeres volt. :)");
            return true;
        }

        #endregion
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,NickName,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
