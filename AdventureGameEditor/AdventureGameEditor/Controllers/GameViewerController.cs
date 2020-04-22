﻿using System;
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
    }
}
