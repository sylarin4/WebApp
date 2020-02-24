﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

using AdventureGameEditor.Data;
using AdventureGameEditor.Models;

namespace AdventureGameEditor.Controllers
{
    public class GameEditorController : BaseController
    {
        protected readonly IGameEditorService _gameEditorService;

        public GameEditorController(AdventureGameEditorContext context, IGameEditorService gameEditorService)
            :base(context)
        {
            _gameEditorService = gameEditorService;

        }

        #region CreateGame
        public IActionResult CreateGame()
        {
            return View("CreateGame");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGame([Bind("Title, Visibility, TableSize")] BasicGameDataViewModel gameData)
        {
            User owner = await _context.User.FirstOrDefaultAsync(user => user.UserName == User.Identity.Name);
            Boolean initialGameSucceeded = _gameEditorService.InicializeGame(gameData.Title, gameData.TableSize, gameData.Visibility, owner);
            if (!initialGameSucceeded)
            {
                ModelState.AddModelError("","Már van egy ilyen nevű kalandjátékod.");
                return View("CreateGame");
            }
            return View("CreateMap", _gameEditorService.GetMapViewModel(User.Identity.Name, gameData.Title));
        }

        #endregion

        #region CreateMap


        public PartialViewResult GetMap()
        {
            return PartialView("Map");
        }
        public IActionResult Test(string gameTitle, int rowNumber, int colNumber, int wayDirectionsCode)
        {
            _gameEditorService.SetExitRoads(User.Identity.Name, gameTitle, rowNumber, colNumber, wayDirectionsCode);
            return View("CreateMap", _gameEditorService.GetMapViewModel(User.Identity.Name, gameTitle));
        }

        public IActionResult SetRoadID(int newID, String gameTitle)
        {
            MapViewModel model = _gameEditorService.GetMapViewModel(User.Identity.Name, gameTitle);
            model.WayDirectionsCode = newID;
            return View("CreateMap", model);
        }

        public FileResult GetMapImage(int? wayDirectionsCode)
        {
            return _gameEditorService.ImageForMap(wayDirectionsCode);
        }

        #endregion

        #region Default functions
        // GET: GameEditor
        public async Task<IActionResult> Index()
        {
            return View(await _context.Game.ToListAsync());
        }

        // GET: GameEditor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game
                .FirstOrDefaultAsync(m => m.ID == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: GameEditor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GameEditor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Visibility,PlayCounter,TableSize")] Game game)
        {
            if (ModelState.IsValid)
            {
                _context.Add(game);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: GameEditor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        // POST: GameEditor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Visibility,PlayCounter,TableSize")] Game game)
        {
            if (id != game.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.ID))
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
            return View(game);
        }

        // GET: GameEditor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game
                .FirstOrDefaultAsync(m => m.ID == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: GameEditor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Game.FindAsync(id);
            _context.Game.Remove(game);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Game.Any(e => e.ID == id);
        }

        #endregion
    }
}