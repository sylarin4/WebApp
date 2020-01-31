using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using Library.Contexts;
using Library.Models;
using Library.Data;

namespace Library.Controllers
{

    [Route("api/Identity")]
    public class AdminIdentityController : ControllerBase
    {
        private readonly LibrarianContext _librarianContext;
        private readonly UserManager<Librarian> _userManager;
        private readonly SignInManager<Librarian> _signInManager;

        public AdminIdentityController(LibrarianContext context, UserManager<Librarian> userManager,
            SignInManager<Librarian> signInManager)
        {
            _librarianContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody]UserDTO user)
        {
            var loginIsSucceed = await DoLogin(user.UserName, user.UserPassword);
            if (loginIsSucceed)
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // Utility function.
        [ValidateAntiForgeryToken]
        public async Task<bool> DoLogin(string userName, string userPassword)
        {
            if (!ModelState.IsValid)
                return false;

            var result = await _signInManager.PasswordSignInAsync(userName, userPassword, false, false);
            if (!result.Succeeded)
            {
                return false;
            }
            return true;
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        // For register test users.
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody]UserDTO user)
        {

            Trace.WriteLine("User: " + user.UserName + " " + user.UserPassword + " " + user.Id);

            bool registerIsSucceeded = await DoRegister(user.UserName, user.UserPassword);
            if (registerIsSucceeded)
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // Utility function.
        // Registers a test user.
        [ValidateAntiForgeryToken]
        private async Task<bool> DoRegister(string userName, string userPassword)
        {
            if (!ModelState.IsValid)
                return false;

            Librarian guest = new Librarian
            {
                Name = userName,
                LibrarianID = 1,
                PhoneNumber = "123456789",
                Email = "test@testuser.test",
                UserName = userName

            };
            var result = await _userManager.CreateAsync(guest, userPassword);
            if (!result.Succeeded)
            {
                return false;
            }
            return true;
        }

    }
        
}
