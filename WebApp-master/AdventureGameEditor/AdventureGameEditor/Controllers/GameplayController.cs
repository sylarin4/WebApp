﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using AdventureGameEditor.Data;
using AdventureGameEditor.Models;
using AdventureGameEditor.Models.Services;
using AdventureGameEditor.Models.ViewModels.Gameplay;
using AdventureGameEditor.Models.Enums;
using AdventureGameEditor.Models.DatabaseModels.Game;
using AdventureGameEditor.Models.Converters;

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

        public IActionResult GetLifeCount(String gameTitle)
        {
            return PartialView("LifeCountPartialView", _gameplayService.GetLifeCount(User.Identity.Name, gameTitle));
        }

        #endregion

        #region Step game
        public IActionResult StepGame(String gameTitle, int rowNumber, int colNumber, string direction)
        {
            Field newPlayerPosition = _gameplayService.StepGame(User.Identity.Name, gameTitle,
                DirectionConverter.StringToDirection(direction));
            return PartialView("GameplayFieldDetails", _gameplayService.GetGameplayFieldViewModel(
                User.Identity.Name, gameTitle, newPlayerPosition));
            
        }

        #endregion

        #region Chose trial

        public String ChoseAlternativeForTrial(String gameTitle, int colNumber, int rowNumber, int trialNumber)
        {         
            return _gameplayService.GetTrial(gameTitle, colNumber, rowNumber).Alternatives[trialNumber].TrialResult.Text;
        }

        public String GetInformTextAboutTrialResult(String gameTitle, int colNumber, int rowNumber, int trialNumber)
        {
            switch (_gameplayService.GetTrial(gameTitle, colNumber, rowNumber).Alternatives[trialNumber].TrialResult.ResultType)
            {
                case ResultType.LoseLife:
                    return "Sajnos rossz döntésed következtében elveszítettél egy életpontot!";
                case ResultType.Teleport:
                    return "Valahol máshol térsz magadhoz...";
                case ResultType.GetLife:
                    return "Hirtelen erőre kapsz. Nyertél egy életpontot!";
                case ResultType.GetTargetDirection:
                    String result = "Hirtelen megérzésed azt súgja, hogy ";
                    switch(_gameplayService.GetTargetDirection(gameTitle, 
                        _gameplayService.GetField(gameTitle, rowNumber, colNumber)))
                    {
                        case CompassPoint.North:
                            result += "északnak";
                            break;
                        case CompassPoint.South:
                            result += "délnek";
                            break;
                        case CompassPoint.East:
                            result += "kelet felé";
                            break;
                        case CompassPoint.West:
                            result += "nyugatnak";
                            break;
                        case CompassPoint.NorthEast:
                            result += "északkelet felé";
                            break;
                        case CompassPoint.NorthWest:
                            result += "északnyugat felé";
                            break;
                        case CompassPoint.SouthEast:
                            result += "délkelet felé";
                            break;
                        case CompassPoint.SouthWest:
                            result += "délnyugat felé";
                            break;
                        case CompassPoint.Here:
                            result += "megérkeztél.";
                            return result;
                        default:
                            result += "fogalmad sincs, merre";
                            break;
                    };
                    result += " kéne tovább haladnod.";
                    return result;
                default:
                    return "";
            }
        }

        // Ajax function will load it to page after a trial.
        public IActionResult LoadDirectionButtonsAfterTrial(String gameTitle, int colNumber, int rowNumber, int trialNumber,
            Boolean isAtTargetField)
        {
           return PartialView("DirectionButtonsPartialView", _gameplayService.GetDirectionButtonsAfterTrial(
               User.Identity.Name, gameTitle, colNumber, rowNumber, trialNumber, isAtTargetField));
        }

        #endregion

        #region Teleport

        // We will step the player in random directions for 2-5 times, and return the field got this way.
        public IActionResult DoTeleport(String gameTitle, int rowNumber, int colNumber)
        {
            // Get randomly how many times we will step the player.
            Random randGen = new Random();
            int stepCount = randGen.Next(1, 6);  // creates a rundom number between 2 and 5

            // Store the start field so we will able to check if return here during stepping.
            Field startField = _gameplayService.GetField(gameTitle, rowNumber, colNumber);

            // Initialize fields to step.
            Field stepField = startField;
            for(int i = 0; i < stepCount; ++i)
            {
                stepField = _gameplayService.GetNextField(stepField, _gameplayService.GetGameMapSize(gameTitle),
                    _gameplayService.GetRandDirection());
            }

            // Check if we return to the start field.
            while(stepField == startField)
            {
                stepField = _gameplayService.GetNextField(stepField, _gameplayService.GetGameMapSize(gameTitle),
                    _gameplayService.GetRandDirection());
            }

            return PartialView("GameplayFieldDetails", _gameplayService.GetGameplayFieldViewModel(
                User.Identity.Name, gameTitle, stepField));

        }
        #endregion 
    }
}
