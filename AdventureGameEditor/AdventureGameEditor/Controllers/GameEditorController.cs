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

        #region CreateMap (Used at "CreateMap" view)

        #region // ---------- Getters ---------- //
        public IActionResult CreateMap(MapViewModel model)
        {
            return View("CreateMap", model);
        }

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

        public IActionResult GetFormForFieldText(String gameTitle, int colNumber, int rowNumber)
        {
            return PartialView("FormForAddFieldTextPartialView", new FieldTextContentViewModel()
            {
                GameTitle = gameTitle,
                ColNumber = colNumber,
                RowNumber = rowNumber,
                TextContent = _gameEditorService.GetFieldTextContent(User.Identity.Name, gameTitle, rowNumber, colNumber)
            });
        }

        public IActionResult GetFormForFieldTrial(String gameTitle, int colNumber, int rowNumber)
        {
            return PartialView("FormForAddFieldTrialPartialView", 
                _gameEditorService.GetFieldTrialContentViewModel(User.Identity.Name, gameTitle, rowNumber, colNumber));
        }

        // Currently not used.
        // Loads a form to add content to the selected field.
        public IActionResult GetFormForField(String gameTitle, int rowNumber, int colNumber)
        {
            Trace.WriteLine("rowNumber: " + rowNumber +"colNumber: " + colNumber);
            return PartialView("FormForAddFieldContent",
                _gameEditorService.GetFieldContentViewModel(User.Identity.Name, gameTitle, rowNumber, colNumber));
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

        // It isn't used now.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTextForField(/*[Bind("GameTitle, ColNumber, RowNumbe, TextContent, Trial, AlternativeTexts, TrialResults, TrialType")]*/ FieldContentViewModel fieldData)
        {
            // Test writeing on console.
            // Trace.WriteLine(fieldData.GameTitle + " " + fieldData.ColNumber + " " + fieldData.RowNumber + " " + fieldData.TextContent);

            /*for(int i = 0; i < fieldData.AlternativeTexts.Count; ++i)
            {
                Trace.WriteLine(fieldData.AlternativeTexts.ElementAt(i));
                Trace.WriteLine(fieldData.TrialResults.ElementAt(i));
            }*/

            // Convert lists to arrays to pass them to the service's functions for save.
            String[] attributeTextsArray = new String[fieldData.AlternativeTexts.Count];
            TrialResult[] trialResultsArray = new TrialResult[fieldData.TrialResults.Count];
            for(int i = 0; i < fieldData.AlternativeTexts.Count; ++i)
            {
                attributeTextsArray[i] = fieldData.AlternativeTexts[i];
                trialResultsArray[i] = fieldData.TrialResults[i];
            }

            // Saveing data.
            _gameEditorService.AddTextToAFieldAt(User.Identity.Name, fieldData.GameTitle, fieldData.RowNumber, fieldData.ColNumber, fieldData.TextContent);
            _gameEditorService.SaveTrial(User.Identity.Name, fieldData.GameTitle, fieldData.RowNumber, fieldData.ColNumber,
                 fieldData.AlternativeTexts, fieldData.TrialResults, fieldData.TrialType);

            // Return the user to the map content filling page to add texts and trials to the other fields too.
            MapContentViewModel model = _gameEditorService.GetMapContentViewModel(User.Identity.Name, fieldData.GameTitle);
            model.FunctionName = "LoadFormForAddFieldContent";
            model.Action = "térkép mezőinek kitöltése";
            model.NextControllerAction = "CreateMapStartField";
            return View("CreateMapContent", model);
        }

        // Save the text of a field (
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTextContentForField( FieldTextContentViewModel fieldData)
        {
            // Saveing data.
            _gameEditorService.AddTextToAFieldAt(User.Identity.Name, fieldData.GameTitle, fieldData.RowNumber, fieldData.ColNumber, fieldData.TextContent);
            
            // Return the user to the map content filling page to add texts and trials to the other fields too.
            MapContentViewModel model = _gameEditorService.GetMapContentViewModel(User.Identity.Name, fieldData.GameTitle);
            model.FunctionName = "LoadButtonsForAddFieldContent";
            model.Action = "térkép mezőinek kitöltése";
            model.NextControllerAction = "CreateMapStartField";
            model.IsFieldSelected = true;
            model.SelectedField = _gameEditorService.GetField(User.Identity.Name, fieldData.GameTitle, fieldData.RowNumber, fieldData.ColNumber);
            return View("CreateMapContent", model);
        }

        // Sace a trial of a field.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTrialForField(FieldTrialContentViewModel fieldData)
        {

            // Convert lists to arrays to pass them to the service's functions for save.
            String[] attributeTextsArray = new String[fieldData.AlternativeTexts.Count];
            TrialResult[] trialResultsArray = new TrialResult[fieldData.TrialResults.Count];
            for (int i = 0; i < fieldData.AlternativeTexts.Count; ++i)
            {
                attributeTextsArray[i] = fieldData.AlternativeTexts[i];
                trialResultsArray[i] = fieldData.TrialResults[i];
            }

            // Saveing data.
            _gameEditorService.SaveTrial(User.Identity.Name, fieldData.GameTitle, fieldData.RowNumber, fieldData.ColNumber,
                 fieldData.AlternativeTexts, fieldData.TrialResults, fieldData.TrialType);

            // Return the user to the map content filling page to add texts and trials to the other fields too.
            MapContentViewModel model = _gameEditorService.GetMapContentViewModel(User.Identity.Name, fieldData.GameTitle);
            model.FunctionName = "LoadButtonsForAddFieldContent";
            model.Action = "térkép mezőinek kitöltése";
            model.NextControllerAction = "CreateMapStartField";
            model.IsFieldSelected = true;
            model.SelectedField = _gameEditorService.GetField(User.Identity.Name, fieldData.GameTitle, fieldData.RowNumber, fieldData.ColNumber);
            return View("CreateMapContent", model);
        }


        // It's not used now.
        public void SaveTextContent(String gameTitle, int rowNumber, int colNumber, String textContent)
        {
            //Trace.WriteLine(gameTitle + " " + colNumber + " " + rowNumber + " " + textContent);
            _gameEditorService.AddTextToAFieldAt(User.Identity.Name, gameTitle, rowNumber, colNumber, textContent);
        }

        /*public IActionResult AddNewAlternativeForForm(String gameTitle, int rowNumber, int colNumber)
        {
            _gameEditorService.AddNewAlternativeToForm(User.Identity.Name, gameTitle, rowNumber, colNumber);
        }*/
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
            model.NextControllerAction = "GetGameDetailsPartialView";
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

        #region Show details for a game

        #region //---------- Getters ----------//

        public IActionResult GetGameDetailsPartialView(String gameTitle)
        {
            return View("GameDetails", _gameEditorService.GetGameDetailsViewModel(User.Identity.Name, gameTitle));
        }

        public IActionResult GetFieldDetailsPartialView(String gameTitle, int colNumber, int rowNumber)
        {
            return PartialView("FieldDetails", _gameEditorService.GetFieldDetailsViewModel(User.Identity.Name, gameTitle, colNumber, rowNumber));
        }

        #endregion

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
