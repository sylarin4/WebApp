using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


using AdventureGameEditor.Data;
using AdventureGameEditor.Models;
using System.IO;

namespace AdventureGameEditor.Controllers
{
    public class GameViewerController : BaseController
    {

        protected readonly IGameEditorService _gameEditorService;
        public GameViewerController(AdventureGameEditorContext context, IGameEditorService gameEditorService)
            : base(context)
        {
            _gameEditorService = gameEditorService;
        }


        #region Index
        public async Task<IActionResult> Index()
        {
            return View(await _context.Game
                                .Include(game => game.CoverImage)
                                .Include(game => game.Owner)
                                .ToListAsync());
        }

        #endregion

        #region Orders
        public IActionResult OrderByTitle()
        {
            return View("Index", GetGames().OrderBy(game => game.Title));
        }

        public IActionResult OrderByOwner()
        {
            return View("Index", GetGames().OrderBy(game => game.Owner.NickName));
        }

        public IActionResult OrderByPopularity()
        {
            return View("Index", GetGames().OrderByDescending(game => game.PlayCounter));
        }


        public IActionResult GetOwnGames()
        {
            return View("Index", GetGames().Where(game => game.Owner.UserName == User.Identity.Name)
                .OrderBy(game => game.Title));
        }

        public List<Game> GetGames()
        {
            return _context.Game.Include(game => game.CoverImage)
                                .Include(game => game.Owner)
                                .ToList();
        }

        #endregion

        public IActionResult RedirectToPrelude(String gameTitle)
        {
             if(! _context.Game.Where(game=> game.Title == gameTitle).FirstOrDefault().IsReadyToPlay)
            {
                ModelState.AddModelError("", "A " + gameTitle + " c. játék még nincsen befejezve, ezért sajnos még nem lehet vele játszani.");                
                return View("~/Views/GameViewer/Index.cshtml", _context.Game
                                .Include(game => game.CoverImage)
                                .Include(game => game.Owner)
                                .ToList());
            }
            else
            {
                return RedirectToAction("GetPrelude", "Gameplay", new { gameTitle = gameTitle });
            }
        }

        public FileContentResult RenderCoverImage(int imageID)
        {
            return _gameEditorService.GetCoverImage(imageID);
        }


        #region Currently not used default functions
        // GET: GameViewer/Details/5
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

        // GET: GameViewer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GameViewer/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Visibility,PlayCounter,TableSize,CurrentWayDirectionsCode")] Game game)
        {
            if (ModelState.IsValid)
            {
                _context.Add(game);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(game);
        }

        // GET: GameViewer/Edit/5
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

        // POST: GameViewer/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Visibility,PlayCounter,TableSize,CurrentWayDirectionsCode")] Game game)
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

        // GET: GameViewer/Delete/5
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

        // POST: GameViewer/Delete/5
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
