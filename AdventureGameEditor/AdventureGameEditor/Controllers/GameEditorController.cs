﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;

using AdventureGameEditor.Data;
using AdventureGameEditor.Models;
using AdventureGameEditor.Utilities;

namespace AdventureGameEditor.Controllers
{
    public class GameEditorController : BaseController

    {
        protected readonly IGameEditorService _gameEditorService;

        #region Constructor
        public GameEditorController(AdventureGameEditorContext context, IGameEditorService gameEditorService)
            :base(context)
        {
            _gameEditorService = gameEditorService;
        }
        #endregion

        #region CreateGame (Used at "CreateGame" view)
        public IActionResult CreateGame()
        {
            return View("CreateGame");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGame(BasicGameDataViewModel gameData)
        {
            User owner = await _context.User.FirstOrDefaultAsync(user => user.UserName == User.Identity.Name);
            if(gameData.CoverImage != null && !FormFileExtensions.IsImage(gameData.CoverImage))
            {
                ModelState.AddModelError("", "A kép formátuma nem támogatott.");
                return View("CreateGame");
            }
            Boolean initialGameSucceeded = _gameEditorService.InicializeGame(gameData.Title, gameData.TableSize, gameData.Visibility, owner, 
                gameData.CoverImage);
            if (!initialGameSucceeded)
            {
                ModelState.AddModelError("","Már van ilyen nevű kalandjáték.");
                return View("CreateGame");
            }
            return View("CreateMap", _gameEditorService.GetMapViewModel(User.Identity.Name, gameData.Title));
        }

        #endregion

        #region Edit game

        public IActionResult EditGame(String gameTitle)
        {
            return GetGameDetailsPartialView(gameTitle);
        }

        // It's used for editing only, because in creatin, we load this page after posting CreateGame view's form.
        public IActionResult CreateMap(String gameTitle)
        {
            return View("CreateMap", _gameEditorService.GetMapViewModel(User.Identity.Name, gameTitle));
        }

        #endregion

        #region CreateMap (Used at "CreateMap" view)

        #region // ---------- Getters ---------- //
        

        public PartialViewResult GetMap()
        {
            return PartialView("Map");
        }

        // Get the image of the field which is specified by the code. 
        // TODO: we will need the style of the map when will have more then one style. (The currently is the 'test" style.)
        public FileResult GetMapImage(int? wayDirectionsCode)
        {
            return _gameEditorService.ImageForMap(wayDirectionsCode);
        }

        #endregion


        #region // ---------- Setters ---------- //

        // When the user select which type of field he/she wants to fill to the map,
        // save it to the database, so we will know which type of field to fill to the map
        // when the user will select a field of the map (if he/she does).
        public void SetRoadID(String gameTitle, int wayDirectionsID)
        {
            _gameEditorService.SetCurrentWayDirectionsCode(User.Identity.Name, gameTitle, wayDirectionsID);
        }

        // When the user select a field of the map, 
        // set the new field type for it (which selected earlier)
        // and refresh the old field for the new one.
        [HttpGet]
        public ActionResult RefreshField(String gameTitle, int rowNumber, int colNumber)
        {
            // Set the new field in the map.
            _gameEditorService.SetExitRoads(User.Identity.Name, gameTitle, rowNumber, colNumber);

            // Refresh the field.
            return PartialView("FieldPartialView", _gameEditorService.GetFieldViewModel(User.Identity.Name, gameTitle, rowNumber, colNumber));
        }

        #endregion

        #endregion

        #region Create map content (Used at "CreateMapContent" view and at it's partial views)

        #region // ---------- Getters ---------- //


        // Get the view with the map. So the user can select which field he/she wants to add text or trial.
        public IActionResult CreateMapContent(String gameTitle)
        {            
            MapContentViewModel model = _gameEditorService.GetMapContentViewModel(User.Identity.Name, gameTitle);
            model.FunctionName = "LoadButtonsForAddFieldContent";
            model.Action = "térkép mezőinek kitöltése";
            model.NextControllerAction = "CreateMapStartField";
            model.IsFieldSelected = false;
            return View("CreateMapContent", model);
        }

        public IActionResult GetButtonsForAddFieldContent(String gameTitle, int rowNumber, int colNumber)
        {
            return PartialView("ButtonsForAddFieldContentPartialView", 
                _gameEditorService.GetFieldViewModel(User.Identity.Name, gameTitle, rowNumber, colNumber));
        }

        public IActionResult GetFormForFieldContent(String gameTitle, int colNumber, int rowNumber)
        {
            Field field = _gameEditorService.GetField(User.Identity.Name, gameTitle, rowNumber, colNumber);
            return PartialView("FormForAddFieldContentPartialView", new FieldTextAndImageContentViewModel()
            {
                GameTitle = gameTitle,
                ColNumber = colNumber,
                RowNumber = rowNumber,
                TextContent = field.Text,
                CurrentImageID = field.Image != null ? field.Image.ID : -1
            });
        }

        public IActionResult GetFormForFieldTrial(String gameTitle, int colNumber, int rowNumber)
        {
            return PartialView("FormForAddFieldTrialPartialView", 
                _gameEditorService.GetFieldTrialContentViewModel(User.Identity.Name, gameTitle, rowNumber, colNumber));
        }


        // Currently not used.
        public IActionResult GetNewAlternative(int index, String gameTitle, int rowNumber, int colNumber)
        {
            return PartialView("AlternativeForFormPartialView", 
                new AlternativeViewModel() { 
                    Trial = _gameEditorService.GetTrial(User.Identity.Name, gameTitle, rowNumber, colNumber),
                    Index = index 
                });
        }

        // Currently not used.
        public IActionResult RefreshAddAlternativeButton(int index, String gameTitle, int rowNumber, int colNumber)
        {
            return PartialView("AddAlternativeButtonPartialView",
                new AddAlternativeButtonViewModel()
                {
                    GameTitle = gameTitle,
                    Index = index + 1,
                    RowNumber = rowNumber,
                    ColNumber = colNumber
                });
        }

        #endregion

        #region // ---------- Setters ---------- //

        // Save the text and image of a field.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTextContentForField( FieldTextAndImageContentViewModel fieldData)
        {
            String errorMessage = "";
            // Check if model state is valid.
            if (!ModelState.IsValid)
            {
                errorMessage = "Hiba történt. Kérem próbálja újra!";

            }
            else if( fieldData.NewImage != null && !FormFileExtensions.IsImage(fieldData.NewImage))
            {
                errorMessage = "A kép formátuma nem megfelelő.";
            }
            else
            {
                // Saveing data.
                _gameEditorService.AddTextAndImageForField(User.Identity.Name, fieldData.GameTitle, 
                    fieldData.RowNumber, fieldData.ColNumber, fieldData.TextContent, fieldData.NewImage);
            }            
            
            // Return the user to the map content filling page to add texts and trials to the other fields too.
            MapContentViewModel model = _gameEditorService.GetMapContentViewModel(User.Identity.Name, fieldData.GameTitle);
            model.FunctionName = "LoadButtonsForAddFieldContent";
            model.Action = "térkép mezőinek kitöltése";
            model.NextControllerAction = "CreateMapStartField";
            model.IsFieldSelected = true;
            model.SelectedField = _gameEditorService.GetField(User.Identity.Name, fieldData.GameTitle, fieldData.RowNumber, fieldData.ColNumber);
            model.ErrorMessage = errorMessage;
            return View("CreateMapContent", model);
        }

        // Save a trial of a field.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTrialForField(FieldTrialContentViewModel trialData)
        {

            // Convert lists to arrays to pass them to the service's functions for save.
            String[] attributeTextsArray = new String[trialData.AlternativeTexts.Count];
            TrialResult[] trialResultsArray = new TrialResult[trialData.TrialResults.Count];
            for (int i = 0; i < trialData.AlternativeTexts.Count; ++i)
            {
                attributeTextsArray[i] = trialData.AlternativeTexts[i];
                trialResultsArray[i] = trialData.TrialResults[i];
            }

            // Saveing data.
            _gameEditorService.SaveTrial(User.Identity.Name, trialData.GameTitle, trialData.RowNumber, trialData.ColNumber,
                 trialData.AlternativeTexts, trialData.TrialResults, trialData.TrialType, trialData.Text);

            // Return the user to the map content filling page to add texts and trials to the other fields too.
            MapContentViewModel model = _gameEditorService.GetMapContentViewModel(User.Identity.Name, trialData.GameTitle);
            model.FunctionName = "LoadButtonsForAddFieldContent";
            model.Action = "térkép mezőinek kitöltése";
            model.NextControllerAction = "CreateMapStartField";
            model.IsFieldSelected = true;
            model.SelectedField = _gameEditorService.GetField(User.Identity.Name, trialData.GameTitle, trialData.RowNumber, trialData.ColNumber);
            return View("CreateMapContent", model);
        }

        #endregion

        #endregion

        #region Create start and target field

        #region //---------- Getters ----------//

        public IActionResult CreateMapStartField(String gameTitle)
        {
            MapContentViewModel model = _gameEditorService.GetMapContentViewModel(User.Identity.Name, gameTitle);
            model.FunctionName = "SaveStartField";
            model.Action = "kezdő mezőjének kiválasztása";
            model.NextControllerAction = "CreateMapTargetField";
            return View("CreateMapContent", model);
        }

        public IActionResult CreateMapTargetField(String gameTitle)
        {
            MapContentViewModel model = _gameEditorService.GetMapContentViewModel(User.Identity.Name, gameTitle);
            model.FunctionName = "SaveTargetField";
            model.Action = "célmezőjének kiválasztása";
            model.NextControllerAction = "CreateGameResult";
            return View("CreateMapContent", model);
        }

        #endregion

        #region //---------- Setters ---------//

        public String SetStartField(String gameTitle, int rowNumber, int colNumber)
        {
            _gameEditorService.SaveStartField(User.Identity.Name, gameTitle, rowNumber, colNumber);
            return "Start mező sorszáma " + rowNumber + ", oszlopszáma " + colNumber + " lett beállítva.";
        }

        public String SetTargetField(String gameTitle, int rowNumber, int colNumber)
        {
            _gameEditorService.SaveTargetField(User.Identity.Name, gameTitle, rowNumber, colNumber);
            return "Célmező sorszáma " + rowNumber + ", oszopszáma " + colNumber + "lett beállítva.";
        }
        #endregion

        #endregion

        #region Create game result

        #region //---------- Getters ----------//

        public IActionResult CreateGameResult(String gameTitle)
        {
            return View("CreateGameResultView", _gameEditorService.GetGameResult(User.Identity.Name, gameTitle));
        }
        public FileContentResult RenderFieldImage(int imageID)
        {
            return _gameEditorService.GetFieldImage(imageID);
        }

        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateGameResult(GameResultViewModel gameResult)
        {
            List<String> errorMessages = new List<String>();
            // Check if any problem occured.
            if (!ModelState.IsValid)
            {
                errorMessages.Add("Hiba történt. Kérem próbálja újra!");
                // Return to the CreateGameResultView to try it again.
                return View("CreateGameResultView", new GameResultViewModel()
                {
                    GameTitle = gameResult.GameTitle,
                    ErrorMessages = errorMessages
                });
            }

            // If there's uploaded file for prelude, check if it's a valid image.
            if (gameResult.PreludeImage != null && !FormFileExtensions.IsImage(gameResult.PreludeImage))
            {
                errorMessages.Add("Az előtörténethez kiválasztott fájl formátuma nem támogatott. \n" +
                    "A fájl mentése siekertelen volt.\n" +
                    "Lehet, hogy nem képet adott meg?");
                gameResult.PreludeImage = null;
            }

            if(gameResult.GameWonImage != null && !FormFileExtensions.IsImage(gameResult.GameWonImage))
            {
                errorMessages.Add("A győzelem esetén megjelenítendő kép formátuma nem támogatott.\n" +
                    "A fájl mentése siekertelen volt.\n" +
                    "Lehet, hogy nem képet adott meg?");
                gameResult.GameWonImage = null;
            }

            if(gameResult.GameLostImage != null && !FormFileExtensions.IsImage(gameResult.GameLostImage))
            {
                errorMessages.Add("A vereség esetén megejelenítendő kép formátuma nem támogatott.\n" +
                    "A fájl mentése siekertelen volt.\n" +
                    "Lehet, hogy nem képet adott meg?");
                gameResult.GameLostImage = null;
            }

            // Save form attributes if all fields filled and no other problems occured.
            if( _gameEditorService.SaveGameResults(User.Identity.Name, gameResult.GameTitle, gameResult.GameWonResult,
                gameResult.GameLostResult, gameResult.Prelude, gameResult.PreludeImage, gameResult.GameWonImage,
                gameResult.GameLostImage))
            {
                if(errorMessages.Count > 0)
                {
                    return View("CreateGameResultView", new GameResultViewModel()
                    {
                        GameTitle = gameResult.GameTitle,
                        ErrorMessages = errorMessages,
                        Prelude = gameResult.Prelude,
                        GameWonResult = gameResult.GameWonResult,
                        GameLostResult = gameResult.GameLostResult
                    });
                }
                return GetGameDetailsPartialView(gameResult.GameTitle);
            }
            else
            {
                errorMessages.Add("A mentés sikertelen volt. Lehet, hogy nem töltött ki minden szöveg mezőt. Kérem, próbálja újra!");
                
            }
            // If there was any problem, return to the CreateGameResultView and list the error messages.
            return View("CreateGameResultView", new GameResultViewModel()
            {
                GameTitle = gameResult.GameTitle,
                ErrorMessages = errorMessages
            });
        }

        #endregion 

        #region Show details for a game

        #region //---------- Getters ----------//

        public IActionResult GetGameDetailsPartialView(String gameTitle)
        {
            return View("GameDetails", _gameEditorService.GetGameDetailsViewModel(User.Identity.Name, gameTitle));
        }
        

        public FileContentResult RenderPreludeImage(int imageID)
        {
            return _gameEditorService.GetPreludeImage(imageID);
        }

        public FileContentResult RenderGameResultImage(int imageID)
        {
            return _gameEditorService.GetGameResultImage(imageID);
        }

        public FileContentResult RenderCoverImage(int imageID)
        {
            return _gameEditorService.GetCoverImage(imageID);
        }

        public IActionResult GetFieldDetailsPartialView(String gameTitle, int colNumber, int rowNumber)
        {
            return PartialView("FieldDetails", _gameEditorService.GetFieldDetailsViewModel(User.Identity.Name, gameTitle, colNumber, rowNumber));
        }

        #endregion

        #endregion

        #region Check game

        public IActionResult CheckGame(String gameTitle)
        {
            CheckGameViewModel model = new CheckGameViewModel()
            {
                GameTitle = gameTitle,
                IsMapValid = _gameEditorService.IsValidMap(gameTitle, User.Identity.Name),
                IsStartFieldSet = _gameEditorService.IsStartFieldSet(User.Identity.Name, gameTitle),
                IsTargetFieldSet = _gameEditorService.IsTargetFieldSet(User.Identity.Name, gameTitle),
                IsPreludeFilled = _gameEditorService.IsPreludeFilled(User.Identity.Name, gameTitle),
                IsGameLostFilled = _gameEditorService.IsGameLostFilled(User.Identity.Name, gameTitle),
                IsGameWonFilled = _gameEditorService.IsGameWonFilled(User.Identity.Name, gameTitle)
            };
            if(model.IsMapValid && model.IsStartFieldSet && model.IsTargetFieldSet && model.IsPreludeFilled
                && model.IsGameLostFilled && model.IsGameWonFilled)
            {
                _gameEditorService.SetReadyToPlay(User.Identity.Name, gameTitle);
            }
            return View("CheckGameView", model);
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
