using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


using AdventureGameEditor.Data;
using AdventureGameEditor.Models;
using AdventureGameEditor.Models.Services;
using AdventureGameEditor.Models.Enums;

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
        public IActionResult Index()
        {
            return View(GetGames());
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
                                .Where(game=> game.Visibility == Visibility.Everyone ||
                                              (game.Visibility == Visibility.LoggedIn && User.Identity.Name != null) ||
                                              (game.Visibility == Visibility.Owner && game.Owner.UserName == User.Identity.Name))
                                .ToList();
        }

        #endregion

        public IActionResult RedirectToPrelude(String gameTitle)
        {
             if(! _context.Game.Where(game=> game.Title == gameTitle).FirstOrDefault().IsReadyToPlay)
            {
                ModelState.AddModelError("", "A " + gameTitle + " játék még nincsen befejezve, ezért sajnos még nem lehet vele játszani.");                
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

        public IActionResult GameViewerDetails(String gameTitle)
        {
            Game game = _context.Game.Where(game => game.Title == gameTitle)
                                                        .Include(game => game.CoverImage)
                                                        .Include(game => game.Prelude)
                                                        .Include(game => game.Owner)
                                                        .FirstOrDefault();
            return PartialView("GameViewerDetails",game);
        }
    }
}
