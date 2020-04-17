using System;
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
    public class GameplayController : BaseController
    {
        #region Attrinutes
        protected readonly IGameEditorService _gameEditorService;
        protected readonly IGameplayService _gameplayService;

        #endregion

        #region Constructor

        public GameplayController(AdventureGameEditorContext context, IGameEditorService gameEditorService, 
            IGameplayService gameplayService) : base(context)
        {
            _gameEditorService = gameEditorService;
            _gameplayService = gameplayService;
        }

        #endregion

        public IActionResult GameplayView(String gameTitle)
        {
            GameplayViewModel model = _gameplayService.GetGameplayViewModel(User.Identity.Name, gameTitle);
            _gameplayService.SetPlayCounter(gameTitle);
            if(model == null)
            {
                return RedirectToAction("Index", "GameViewer");
            }
            return View("GameplayView",model);
        }

        public IActionResult GameplayFieldPartialView(GameplayFieldViewModel field)
        {
            return PartialView("GameplayFieldPartialView", field);
        }

        #region Getters

        public IActionResult GetPrelude(String gameTitle)
        {
            Prelude prelude = _context.Game
                .Where(game => game.Title == gameTitle)
                .Include(g => g.Prelude)
                .ThenInclude(p => p.Image)
                .Select(game => game.Prelude)
                .FirstOrDefault();
            int? preludeImgID;
            if(prelude.Image == null)
            {
                preludeImgID = null;
            }
            else
            {
                preludeImgID = prelude.Image.ID;
            }
            PreludeViewModel model = new PreludeViewModel()
            {
                Text = prelude.Text,
                GameTitle = prelude.GameTitle,
                PictureID = preludeImgID
            };
            return View("PreludeView", model);
        }

        public FileContentResult RenderPreludeImage(int imageID)
        {
            return _gameEditorService.GetPreludeImage(imageID);
        }

        // Get the image of the field which is specified by the code. 
        // TODO: we will need the style of the map when will have more then one style. (The currently is the 'test" style.)
        public FileResult GetMapImage(int? wayDirectionsCode)
        {
            return _gameEditorService.ImageForMap(wayDirectionsCode);
        }

        #endregion

        public IActionResult StepGame(String gameTitle, int rowNumber, int colNumber, string direction)
        {
            Trace.WriteLine("Előtte: rowNumber: " + rowNumber + " colNumber: " + colNumber);
            Field newPlayerPosition = _gameplayService.StepGame(User.Identity.Name, gameTitle, direction);
            Trace.WriteLine("rowNumber: " + newPlayerPosition.RowNumber + " colNumber: " + newPlayerPosition.ColNumber);
            return PartialView("GameplayFieldDetails", new GameplayFieldViewModel()
            {
                GameTitle = gameTitle,
                RowNumber = newPlayerPosition.RowNumber,
                ColNumber = newPlayerPosition.ColNumber,
                Text = newPlayerPosition.Text,
                Trial = newPlayerPosition.Trial,
                IsRightWay = newPlayerPosition.IsRightWay,
                IsLeftWay = newPlayerPosition.IsLeftWay,
                IsUpWay = newPlayerPosition.IsUpWay,
                IsDownWay = newPlayerPosition.IsDownWay,
                IsVisited = _gameplayService.GetIsVisitedField(User.Identity.Name, gameTitle, 
                    newPlayerPosition.ColNumber, newPlayerPosition.RowNumber),
                IsAtTargetField = _gameplayService.IsAtTargetField(gameTitle, newPlayerPosition.RowNumber, newPlayerPosition.ColNumber)
            });
            
        }

        public String ChoseAlternativeForTrial(String gameTitle, int colNumber, int rowNumber, int trialNumber)
        {
            Trace.WriteLine("gameTitle: " + gameTitle + " colNumber: " + colNumber + " rowNumber: " + rowNumber + " trialNumber: " + trialNumber);  
            
            return _gameplayService.GetTrial(gameTitle,colNumber, rowNumber).Alternatives[trialNumber].TrialResult.Text;
        }

        public IActionResult LoadDirectionButtonsAfterTrial(String gameTitle, int colNumber, int rowNumber, int trialNumber,
            Boolean isAtTargetField)
        {
            Field field = _gameplayService.GetField(gameTitle, rowNumber, colNumber);
            switch(_gameplayService.GetTrial(gameTitle, colNumber, rowNumber).Alternatives[trialNumber].TrialResult.ResultType)
            {
                case ResultType.GameLost:
                    _gameplayService.SetGameOver(User.Identity.Name, gameTitle, false);
                    return PartialView("DirectionButtonsPartialView", new DirectionButtonsViewModel()
                    {
                        GameTitle = gameTitle,
                        RowNumber = rowNumber,
                        ColNumber = colNumber, 
                        GameLost = true,
                        GameWon = false,
                        IsDownWay = field.IsDownWay,
                        IsLeftWay = field.IsLeftWay,
                        IsRightWay = field.IsRightWay,
                        IsUpWay = field.IsUpWay
                    });
                case ResultType.GameWon:
                    _gameplayService.SetGameOver(User.Identity.Name, gameTitle, true);
                    return PartialView("DirectionButtonsPartialView", new DirectionButtonsViewModel()
                    {
                        GameTitle = gameTitle,
                        RowNumber = rowNumber,
                        ColNumber = colNumber,
                        GameLost = false,
                        GameWon = true,
                        IsDownWay = field.IsDownWay,
                        IsLeftWay = field.IsLeftWay,
                        IsRightWay = field.IsRightWay,
                        IsUpWay = field.IsUpWay
                    });
                default:
                    if (isAtTargetField)
                    {
                        _gameplayService.SetGameOver(User.Identity.Name, gameTitle, true);
                    }
                    return PartialView("DirectionButtonsPartialView", new DirectionButtonsViewModel()
                    {
                        GameTitle = gameTitle,
                        RowNumber = rowNumber,
                        ColNumber = colNumber,
                        GameLost = false,
                        GameWon = isAtTargetField,
                        IsDownWay = field.IsDownWay,
                        IsLeftWay = field.IsLeftWay,
                        IsRightWay = field.IsRightWay,
                        IsUpWay = field.IsUpWay
                    });
            }
        }

        public IActionResult GetGameResult(String gameTitle)
        {
            return View("GameResultView", _gameplayService.GetGameResult(gameTitle, User.Identity.Name));
        }

        public FileContentResult RenderGameResultImage(int imageID)
        {
            return _gameEditorService.GetGameResultImage(imageID);
        }

        #region Default functions

        // GET: Gameplay
        public async Task<IActionResult> Index()
        {
            return View(await _context.GameplayData.ToListAsync());
        }

        // GET: Gameplay/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameplayData = await _context.GameplayData
                .FirstOrDefaultAsync(m => m.ID == id);
            if (gameplayData == null)
            {
                return NotFound();
            }

            return View(gameplayData);
        }

        // GET: Gameplay/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Gameplay/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StepCount,IsGameOver,StartDate,LastPlayDate")] GameplayData gameplayData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gameplayData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gameplayData);
        }

        // GET: Gameplay/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameplayData = await _context.GameplayData.FindAsync(id);
            if (gameplayData == null)
            {
                return NotFound();
            }
            return View(gameplayData);
        }

        // POST: Gameplay/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,StepCount,IsGameOver,StartDate,LastPlayDate")] GameplayData gameplayData)
        {
            if (id != gameplayData.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gameplayData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameplayDataExists(gameplayData.ID))
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
            return View(gameplayData);
        }

        // GET: Gameplay/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameplayData = await _context.GameplayData
                .FirstOrDefaultAsync(m => m.ID == id);
            if (gameplayData == null)
            {
                return NotFound();
            }

            return View(gameplayData);
        }

        // POST: Gameplay/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gameplayData = await _context.GameplayData.FindAsync(id);
            _context.GameplayData.Remove(gameplayData);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameplayDataExists(int id)
        {
            return _context.GameplayData.Any(e => e.ID == id);
        }

        #endregion
    }
}
