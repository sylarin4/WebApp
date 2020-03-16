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

        public GameEditorController(AdventureGameEditorContext context, IGameEditorService gameEditorService)
            :base(context)
        {
            _gameEditorService = gameEditorService;
        }

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

        // ---------- Getters ---------- //
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

        // ---------- Setters ---------- //

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

        #region Create map content (Used at "CreateMapContent" view and at it's partial views)

        // ---------- Getters ---------- //


        // Get the view with the map. So the user can select which field he/she wants to add text or trial.
        public IActionResult CreateMapContent(String gameTitle)
        {
            return View("CreateMapContent", _gameEditorService.GetMapViewModel(User.Identity.Name, gameTitle));
        }


        // Loads a form to add content to the selected field.
        public IActionResult GetFormForField(String gameTitle, int rowNumber, int colNumber)
        {
            // Initialize trial.
            Trial trial = _gameEditorService.InitializeTrial(User.Identity.Name, gameTitle, rowNumber, colNumber);

            return PartialView("FormForAddFieldContent", new FieldContentViewModel()
            {
                GameTitle = gameTitle,
                RowNumber = rowNumber,
                ColNumber = colNumber,
                TextContent = _gameEditorService.GetTextAtCoordinate(User.Identity.Name, gameTitle, rowNumber, colNumber),
                Trial = trial
            });
        }

        public IActionResult GetNewAlternative(int index, String gameTitle, int rowNumber, int colNumber)
        {
            return PartialView("AlternativeForFormPartialView", 
                new AlternativeViewModel() { 
                    Trial = _gameEditorService.GetTrial(User.Identity.Name, gameTitle, rowNumber, colNumber),
                    Index = index 
                });
        }

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


        // ---------- Setters ---------- //


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetTextForField([Bind("GameTitle, RowNumbe, ColNumber, TextContent, Trial")] FieldContentViewModel textData)
        {
            Trace.WriteLine(textData.GameTitle + " " + textData.ColNumber + " " + textData.RowNumber + " " + textData.TextContent);
            _gameEditorService.AddTextToAFieldAt(User.Identity.Name, textData.GameTitle, textData.RowNumber, textData.ColNumber, textData.TextContent);
            Trace.WriteLine(_gameEditorService.GetTextAtCoordinate(User.Identity.Name, textData.GameTitle, textData.RowNumber, textData.ColNumber));
            return View("CreateMapContent", _gameEditorService.GetMapViewModel(User.Identity.Name, textData.GameTitle));
        }

        // It's not used now.
        public void SaveTextContent(String gameTitle, int rowNumber, int colNumber, String textContent)
        {
            Trace.WriteLine(gameTitle + " " + colNumber + " " + rowNumber + " " + textContent);
            _gameEditorService.AddTextToAFieldAt(User.Identity.Name, gameTitle, rowNumber, colNumber, textContent);
        }

        /*public IActionResult AddNewAlternativeForForm(String gameTitle, int rowNumber, int colNumber)
        {
            _gameEditorService.AddNewAlternativeToForm(User.Identity.Name, gameTitle, rowNumber, colNumber);
        }*/

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
