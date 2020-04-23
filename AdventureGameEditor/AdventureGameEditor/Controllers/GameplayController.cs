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

        // Intialization of GameplayData.
        public IActionResult GameplayView(String gameTitle)
        {
            GameplayViewModel model = _gameplayService.GetGameplayViewModel(User.Identity.Name, gameTitle);
            _gameplayService.StepPlayCounter(gameTitle);
            if (model == null)
            {
                return RedirectToAction("Index", "GameViewer");
            }
            return View("GameplayView", model);
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
            if (prelude.Image == null)
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

        public FileContentResult RenderFieldImage(int imageID)
        {
            return _gameplayService.GetFieldImage(imageID);
        }

        public IActionResult GetGameResult(String gameTitle)
        {
            return View("GameResultView", _gameplayService.GetGameResult(gameTitle, User.Identity.Name));
        }

        public FileContentResult RenderGameResultImage(int imageID)
        {
            return _gameEditorService.GetGameResultImage(imageID);
        }

        public String GetLifeCount(String gameTitle)
        {
            return "Életek száma: " + _gameplayService.GetLifeCount(User.Identity.Name, gameTitle);
        }

        #endregion

        #region Step game
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
                FieldImageID = newPlayerPosition.Image != null ? newPlayerPosition.Image.ID : -1,
                Trial = newPlayerPosition.Trial,
                IsRightWay = newPlayerPosition.IsRightWay,
                IsLeftWay = newPlayerPosition.IsLeftWay,
                IsUpWay = newPlayerPosition.IsUpWay,
                IsDownWay = newPlayerPosition.IsDownWay,
                IsVisited = _gameplayService.GetIsVisitedField(User.Identity.Name, gameTitle, 
                    newPlayerPosition.ColNumber, newPlayerPosition.RowNumber),
                IsAtTargetField = _gameplayService.IsAtTargetField(gameTitle, newPlayerPosition.RowNumber, newPlayerPosition.ColNumber),
                LifeCount = _gameplayService.GetLifeCount(User.Identity.Name, gameTitle)
            });
            
        }

        public String ChoseAlternativeForTrial(String gameTitle, int colNumber, int rowNumber, int trialNumber)
        {
            Trace.WriteLine("gameTitle: " + gameTitle + " colNumber: " + colNumber + " rowNumber: " + rowNumber + " trialNumber: " + trialNumber);

            String result = _gameplayService.GetTrial(gameTitle, colNumber, rowNumber).Alternatives[trialNumber].TrialResult.Text;
            if(_gameplayService.GetTrial(gameTitle, colNumber, rowNumber).Alternatives[trialNumber].TrialResult.ResultType
                == ResultType.GameLost)
            {
                result += "\n Sajnos rossz döntésed következtében elveszítettél egy életpontot!";
            }               

            return result;
        }
        

        public IActionResult LoadDirectionButtonsAfterTrial(String gameTitle, int colNumber, int rowNumber, int trialNumber,
            Boolean isAtTargetField)
        {
           return PartialView("DirectionButtonsPartialView", _gameplayService.GetDirectionButtonsAfterTrial(
               User.Identity.Name, gameTitle, colNumber, rowNumber, trialNumber, isAtTargetField));
        }

        #endregion
    }
}
