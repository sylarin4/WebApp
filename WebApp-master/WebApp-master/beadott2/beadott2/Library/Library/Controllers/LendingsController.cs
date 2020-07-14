using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using Library.Models;
using Library.Contexts;

namespace Library.Controllers
{
    public class LendingsController : BaseController
    {
        private readonly LibraryContext _context;

        
        public LendingsController(LibraryContext context, ILibraryService libraryService)
            : base(libraryService)
        {
            _context = context;
        }
        
        
        public async Task<IActionResult> Index()
        {
            return View(await _context.Lending.ToListAsync());
        }
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lending = await _context.Lending
                .SingleOrDefaultAsync(m => m.ID == id);
            if (lending == null)
            {
                return NotFound();
            }

            return View(lending);
        }

        

        // GET: Lendings/Create
        [HttpGet]
        public IActionResult Create(String lendVolParam)
        {
            return View("Create", _libraryService.GetCreateLendingView(lendVolParam));
        }

        // POST: Lendings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("VolID,GuestID,StartDay,EndDay")] LendingViewModel lendingView)
        {
            if (ModelState.IsValid)
            {
                Lending lending = _libraryService.CreateNewLending(lendingView);
                if ( lending != null)
                {
                    return RedirectToAction("Details", lending);
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect dates or this volume is already lended in this time period.");
                    return View("Create", lendingView);
                }
            }
            return View(lendingView);
        }
    }
}
