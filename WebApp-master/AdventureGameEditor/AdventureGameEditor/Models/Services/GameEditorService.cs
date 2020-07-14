using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;

using AdventureGameEditor.Data;
using AdventureGameEditor.Models.ViewModels.GameEditor;
using AdventureGameEditor.Models.Enums;
using AdventureGameEditor.Models.DatabaseModels.Game;
using AdventureGameEditor.Utilities;
using MySqlX.XDevAPI.Relational;

namespace AdventureGameEditor.Models.Services
{
    public class GameEditorService : IGameEditorService
    {
        #region Attributes

        protected readonly AdventureGameEditorContext _context;

        #endregion

        #region Constructor
        public GameEditorService(AdventureGameEditorContext context)
        {
            _context = context;
        }

        #endregion

        #region Edit game

        // Try to delete the game. If done, return true, if not, return false.
        public Boolean DeleteGame(String userName, String gameTitle)
        {
            Game game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                     .Include(g => g.Map)
                                     .ThenInclude(m => m.Row)
                                     .ThenInclude(r => r.Trial)
                                     .ThenInclude(t => t.Alternatives)
                                     .ThenInclude(a => a.TrialResult)
                                     .Include(g => g.Prelude)
                                     .ThenInclude(p => p.Image)
                                     .Include(g => g.GameLostResult)
                                     .ThenInclude(gr => gr.Image)
                                     .Include(g => g.GameWonResult)
                                     .ThenInclude(gr => gr.Image)
                                     .FirstOrDefault();
            if(game == null)
            {
                return false;
            }
            try
            {
                // Delte all the fields.
                List<Field> fieldsToDelete = _context.Field.Where(f => f.GameTitle == game.Title)
                                                            .Include(f => f.Image)
                                                            .Include(f => f.Trial)
                                                            .ThenInclude(t => t.Alternatives)
                                                            .ThenInclude(a => a.TrialResult)
                                                            .ToList();
                foreach(Field field in fieldsToDelete)
                {
                    _context.Field.Remove(field);
                }

                foreach(GameResult gameResult in _context.GameResult
                                                          .Where(gr => gr.GameTitle == game.Title)
                                                          .Include(gr=>gr.Image)
                                                          .ToList())
                {
                    _context.GameResult.Remove(gameResult);
                }
                _context.Game.Remove(game);
                _context.SaveChanges();
            }
            catch
            {
                return false;
            }
            
            return true;
        }

        #endregion

        #region Create game
        public Boolean InicializeGame(String title, int mapSize, Visibility visibility, User owner, 
            IFormFile coverImage)
        {
            if (_context.Game.Any(game => game.Title == title)) return false;

            // Initialize a map.            
            List <MapRow> map = new List<MapRow>();
            for(int i = 0; i < mapSize; ++i)
            {
                MapRow row = new MapRow
                {
                    Row = new List<Field>()
                };
                for (int j = 0; j < mapSize; ++j)
                {
                    
                    row.Row.Add(
                        new Field
                        {
                            UserName = owner.UserName,
                            GameTitle = title,
                            RowNumber = i,
                            ColNumber = j,
                            Text = "",
                            IsRightWay = false,
                            IsLeftWay = false,
                            IsUpWay = false,
                            IsDownWay = false
                        });                    
                }
                map.Add(row);
            }

            // Initilaize Image
            Image image = new Image();
            if(coverImage != null)
            {
                image.Name = coverImage.FileName;
                image.Picture = ConvertIFormFileToImage(coverImage);
            }
            else
            {
                image = null;
            }

            // Save the initialized game to the database.
            _context.Game.Add(
                new Game
                {
                    Title = title,
                    Visibility = visibility,
                    TableSize = mapSize,
                    PlayCounter = 0,
                    Owner = owner,
                    Map = map,
                    CurrentWayDirectionsCode = 0,
                    CoverImage = image,
                    IsReadyToPlay = false
                }
                );
            _context.SaveChanges();
            return true;
        }

        #endregion

        #region Create map

        #region // ---------- Getters ---------- //
        public MapViewModel GetMapViewModel(String userName, String gameTitle)
        {
            Game game = _context.Game
                .Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Map)
                .ThenInclude(m => m.Row)
                .FirstOrDefault();
            return new MapViewModel
            {
                MapSize = game.TableSize,
                GameTitle = game.Title,
                Map = SortMap(game.Map.ToList())
            };
        }

        public MapPieceViewModel GetFieldViewModel(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            return new MapPieceViewModel()
            {
                GameTitle = gameTitle,
                Field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber)
            };
        }

        public FileResult ImageForMap(int? wayDirectionsCode)
        {
            if (wayDirectionsCode == null) wayDirectionsCode = 0;
            Byte[] imageContent = GetImage((int)wayDirectionsCode, MapTheme.Default);
            return new FileContentResult(imageContent, "image/png");
        }

        public String GetTextAtCoordinate(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            return _context.Field.Where(f => f.GameTitle == gameTitle && f.UserName == userName && f.ColNumber == colNumber
                        && f.RowNumber == rowNumber)
                .Select(f => f.Text)
                .FirstOrDefault();
        }
        #endregion

        #region // ---------- Setters --------- //
        public void SetExitRoads(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            int wayDirectionsCode = GetCurrentWayDirectionsCode(userName, gameTitle);
            Field field = _context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle
                    && f.RowNumber == rowNumber && f.ColNumber == colNumber).FirstOrDefault();
            if(field != null)
            {
                // Set direction "left".
                if(wayDirectionsCode % 10 == 0)
                    field.IsLeftWay = false;
                else
                    field.IsLeftWay = true;

                // Set direction "down".
                if((wayDirectionsCode/ 10)%10 == 0)
                    field.IsDownWay = false;
                else
                    field.IsDownWay = true;

                // Set direction "right".
                if((wayDirectionsCode/100)%10 == 0)
                    field.IsRightWay = false;
                else
                    field.IsRightWay = true;

                // Set direction "up".
                if((wayDirectionsCode/1000)%10 == 0)
                    field.IsUpWay = false;
                else
                    field.IsUpWay = true;
                _context.SaveChanges();
            }
        }

        public void SetCurrentWayDirectionsCode(String userName, String gameTitle, int newWayDirectionsCode)
        {
            _context.Game
                .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                .FirstOrDefault()
                .CurrentWayDirectionsCode = newWayDirectionsCode;
            _context.SaveChanges();
        }

        #endregion


        #endregion

        #region Create map content

        #region //---------- Setters ----------//

        #region Initializing      

        public List<String> InitializeAlternativeTexts(int count)
        {
            List<String> alternativeTexts = new List<String>();
            for(int i = 0; i < count; ++i)
            {
                alternativeTexts.Add("");
            }
            return alternativeTexts;
        }

        public List<TrialResult> InitializeTrialResults(int count)
        {
            List<TrialResult> trialResults = new List<TrialResult>();
            for(int i = 0; i < count; ++i)
            {
                trialResults.Add(new TrialResult()
                {
                    ResultType = ResultType.Nothing,
                    Text = ""
                });
            }
            return trialResults;
        }
        #endregion

        #region Save 

         public void AddTextAndImageForField(String userName, String gameTitle, int rowNumber, int colNumber,
             String text, IFormFile image)
        {
            Field field = _context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle && 
                f.RowNumber == rowNumber && f.ColNumber == colNumber).FirstOrDefault();
            field.Text = text;

            if(image != null)
            {
                field.Image = new Image()
                {
                    Picture = ConvertIFormFileToImage(image),
                    Name = image.FileName
                };
            }
            _context.SaveChanges();
        }

        public void SaveTrial(String userName, String gameTitle, int rowNumber, int colNumber, List<String> alternativeTexts, 
            List<TrialResult> alternativeTrialResults, TrialType trialType, String trialText )
        {
            Trial trial = new Trial();
            trial.Alternatives = new List<Alternative>();
            trial.TrialType = trialType;

            for(int i = 0; i < alternativeTexts.Count; ++i)
            {
                trial.Alternatives.Add(new Alternative()
                {
                    Text = alternativeTexts[i],
                    TrialResult = alternativeTrialResults[i]
                });
            }

            trial.Text = trialText;

            Field field = _context.Field
                .Where(f => f.UserName == userName && f.GameTitle == gameTitle && f.ColNumber == colNumber && f.RowNumber == rowNumber)
                .FirstOrDefault();
            field.Trial = trial;
            _context.SaveChanges();
        }

        #endregion

        #endregion


        #region //---------- Getters ----------// 
        public Trial GetTrial(String userName, String gameTitle, int colNumber, int rowNumber)
        {
            return _context.Field
                .Where(field => field.UserName == userName && field.GameTitle == gameTitle)
                .Where(field => field.ColNumber == colNumber && field.RowNumber == rowNumber)
                .Include(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .ThenInclude(alternatives => alternatives.TrialResult)
                .Select(field => field.Trial)
                .FirstOrDefault();
        }

        public MapContentViewModel GetMapContentViewModel(String userName, String gameTitle)
        {
            Game game = _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Map)
                .ThenInclude(map => map.Row)
                .FirstOrDefault();
            return new MapContentViewModel()
            {
                GameTitle = game.Title,
                Map = SortMap(game.Map.ToList()),
                MapSize = game.TableSize
            };
        }
        public FileContentResult GetFieldImage(int imageID)
        {
            if (imageID == -1) return null;
            byte[] picture = _context.Field
                .Include(field => field.Image)
                .Where(field => field.Image.ID == imageID)
                .Select(field => field.Image.Picture)
                .FirstOrDefault();
            if (picture == null) return null;
            return new FileContentResult(picture , "image/png");
        }


        public FieldTrialContentViewModel GetFieldTrialContentViewModel(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            // Load the field.
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);

            // Initializeing.
            List<String> alternativeTexts = new List<String>();
            List<TrialResult> alternativeTrialResults = new List<TrialResult>();
            TrialType trialType = TrialType.LuckTrial;

            // If trial wasn't created previously, initialize it.
            if (field.Trial == null)
            {
                alternativeTexts = InitializeAlternativeTexts(4);
                alternativeTrialResults = InitializeTrialResults(4);
            }

            // If trial was created previously, load that data.
            else
            {
                trialType = field.Trial.TrialType;
                foreach (Alternative alternative in field.Trial.Alternatives)
                {
                    alternativeTexts.Add(alternative.Text);
                    alternativeTrialResults.Add(alternative.TrialResult);
                }
            }

            // Convert data to FieldContentViewModel type.
            FieldTrialContentViewModel model = new FieldTrialContentViewModel()
            {
                GameTitle = gameTitle,
                ColNumber = colNumber,
                RowNumber = rowNumber,
                TrialType = trialType,
                AlternativeTexts = alternativeTexts,
                TrialResults = alternativeTrialResults,
                Text= field.Trial != null ? field.Trial.Text : ""
            };

            return model;
        }


        #endregion

        #endregion

        #region Set start and target fields

        #region //---------- Setters ----------//

        // Save the start field which's coordinates are given in the function's attributes (rowNumber and colNumber).
        public void SaveStartField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            Game game = GetGameWithoutIncludes(userName, gameTitle);
            game.StartField = GetFieldWithoutIncludes(userName, gameTitle, rowNumber, colNumber);
            _context.SaveChanges();
        }

        public void SaveTargetField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            Game game = GetGameWithoutIncludes(userName, gameTitle);
            game.TargetField = GetFieldWithoutIncludes(userName, gameTitle, rowNumber, colNumber);
            _context.SaveChanges();
        }

        #endregion

        #endregion

        #region Show game details

        #region //---------- Getters ----------//

        public GameDetailsViewModel GetGameDetailsViewModel(String userName, String gameTitle)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            return new GameDetailsViewModel()
            {
                OwnerName = userName,
                Title= gameTitle,
                CoverImageID = game.CoverImage != null ? game.CoverImage.ID : -1,
                Visibility = game.Visibility,
                TableSize = game.TableSize,
                Map = SortMap(game.Map.ToList()),
                StartField = game.StartField != null ? game.StartField : null,
                TargetField = game.TargetField != null ? game.TargetField : null,
                GameLostResult = game.GameLostResult != null ? game.GameLostResult.Text : null,
                GameLostImageID = game.GameLostResult != null && game.GameLostResult.Image != null ?
                                                            game.GameLostResult.Image.ID : -1,
                GameWonResult = game.GameWonResult != null ? game.GameWonResult.Text : null,
                GameWonImageID = game.GameWonResult != null && game.GameWonResult.Image != null ?
                                                            game.GameWonResult.Image.ID : -1,
                Prelude = game.Prelude != null ? game.Prelude.Text : null,
                PreludeImageID = game.Prelude != null && game.Prelude.Image != null ? game.Prelude.Image.ID : -1,

                Summary= game.Summary != null ? game.Summary : ""
            };
        }

        public FieldDetailsViewModel GetFieldDetailsViewModel(String userName, String gameTitle, int colNumber, int rowNumber)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            FieldDetailsViewModel fieldDetails = new FieldDetailsViewModel()
            {
                ColNumber = colNumber,
                RowNumber = rowNumber,
                TextContent = field.Text,
                IsTrial = field.Trial == null ? false : true,
                PictureID = field.Image != null ? field.Image.ID : -1
            };
            if(field.Trial != null)
            {
                Trace.WriteLine(field.Trial.Text);
                fieldDetails.TrialText = field.Trial.Text;
                fieldDetails.AlternativeTexts = new List<string>();
                fieldDetails.TrialResults = new List<TrialResult>();
                for(int i = 0; i < field.Trial.Alternatives.Count; ++i)
                {
                    fieldDetails.AlternativeTexts.Add(field.Trial.Alternatives.ElementAt(i).Text);
                    fieldDetails.TrialResults.Add(field.Trial.Alternatives.ElementAt(i).TrialResult);
                }
            }
            return fieldDetails;
        }

        public FileContentResult GetPreludeImage(int imageID)
        {
            if (imageID < 0) return null;
            byte[] picture = _context.Game.Where(g => g.Prelude.Image.ID == imageID)
                .Include(g => g.Prelude)
                .ThenInclude(p => p.Image)
                .Select(g => g.Prelude.Image.Picture)
                .FirstOrDefault();
            if (picture == null) return null;
            return new FileContentResult(picture, "image/png");
        }

        public FileContentResult GetGameResultImage(int imageID)
        {
            if (imageID < 0) return null;
            byte[] picture = _context.GameResult
                                    .Include(result => result.Image)
                                    .Where(result => result.Image.ID == imageID)
                                    .Select(result => result.Image.Picture)
                                    .FirstOrDefault();
            if (picture == null) return null;
            return new FileContentResult(picture, "image/png");
        }

        public FileContentResult GetCoverImage(int imageID)
        {
            if (imageID < 0) return null;
            byte[] picture = _context.Game.Include(game => game.CoverImage)
                                            .Where(game => game.CoverImage.ID == imageID)
                                            .Select(game => game.CoverImage.Picture)
                                            .FirstOrDefault();
            if (picture == null) return null;
            return new FileContentResult(picture, "image/png");
        }


        #endregion

        #endregion

        #region Create game result

        public Boolean SaveGameResults(String userName, String gameTitle, String gameWonResult, String gameLostResult,
            String prelude, IFormFile preludeImage, IFormFile gameWonImage, IFormFile gameLostImage, 
            IFormFile newCoverImage, String summary)
        {
            // If the results empty or not set, don't save them.
            if (gameLostResult == null || gameWonResult == null || gameLostResult == "" || gameWonResult == "")
            {
                return false;
            }

            Game game = _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Owner)
                .Include(g => g.CoverImage)
                .Include(g => g.GameLostResult)
                .ThenInclude(result => result.Image)
                .Include(g => g.GameWonResult)
                .ThenInclude(result => result.Image)
                .Include(g => g.Prelude)
                .ThenInclude(p => p.Image)
                .FirstOrDefault();
            if (game == null)
            {
                return false;
            }

            // Saveing informations about game lost result.
            if(game.GameLostResult == null)
            {
                game.GameLostResult = new GameResult()
                    {
                        Owner = game.Owner,
                        GameTitle = gameTitle,
                        IsGameWonResult = false,
                        Text = gameLostResult
                    };
            }
            else
            {
                game.GameLostResult.Text = gameLostResult;
            }
            if (gameLostImage != null)
            {
                game.GameLostResult.Image = new Image()
                {
                    Name = gameLostImage.FileName,
                    Picture = ConvertIFormFileToImage(gameLostImage)
                };
            }

            // Saveing information about game won result.
            if(game.GameWonResult == null)
            {
                game.GameWonResult = new GameResult()
                {
                    Owner = game.Owner,
                    GameTitle = gameTitle,
                    IsGameWonResult = true,
                    Text = gameWonResult
                };
            }
            else
            {
                game.GameWonResult.Text = gameWonResult;
            }
            if (gameWonImage != null)
            {
                game.GameWonResult.Image = new Image()
                {
                    Name = gameWonImage.FileName,
                    Picture = ConvertIFormFileToImage(gameWonImage)
                };
            }

            // Saveing informations about prelude.
            if(game.Prelude == null)
            {
                game.Prelude = new Prelude()
                {
                    Text = prelude,
                    Owner = game.Owner,
                    GameTitle = gameTitle
                };
            }
            else
            {
                game.Prelude.Text = prelude;
            }
            
            if (preludeImage != null)
            {
                game.Prelude.Image = new Image()
                {
                    Name = preludeImage.FileName,
                    Picture = ConvertIFormFileToImage(preludeImage)
                };
            }

            //Saveing the new cover image if it's set.
            if(newCoverImage != null)
            {
                game.CoverImage = new Image()
                {
                    Name = newCoverImage.FileName,
                    Picture = ConvertIFormFileToImage(newCoverImage)
                };
            }

            // Saveing summary.
            game.Summary = summary;
                 
            _context.SaveChanges();
            return true;
        }

        public GameResultViewModel GetGameResult(String userName, String gameTitle)
        {
            Game game = _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Owner)
                .Include(g => g.CoverImage)
                .Include(g => g.GameLostResult)
                .ThenInclude(result => result.Image)
                .Include(g => g.GameWonResult)
                .ThenInclude(result => result.Image)
                .Include(g => g.Prelude)
                .ThenInclude(p => p.Image)
                .FirstOrDefault();
            GameResultViewModel model = new GameResultViewModel()
            {
                GameTitle = gameTitle,
                Prelude = game.Prelude != null ? game.Prelude.Text : "",
                GameWonResult = game.GameWonResult != null ? game.GameWonResult.Text : "",
                GameLostResult = game.GameLostResult != null ? game.GameLostResult.Text : "",
                GameWonImageID = game.GameWonResult != null && game.GameWonResult.Image != null ? game.GameWonResult.Image.ID : -1,
                GameLostImageID = game.GameLostResult != null && game.GameLostResult.Image != null ? game.GameLostResult.Image.ID : -1,
                PreludeImageID = game.Prelude != null && game.Prelude.Image != null ? game.Prelude.Image.ID : -1,
                Summary = game.Summary != null ? game.Summary : ""
            };
            return model;
        }

        #endregion

        #region Check game

        public Boolean IsValidMap(String gameTitle, String userName)
        {
            List<MapRow> map = GetMap(userName, gameTitle);
            foreach (MapRow row in map)
            {
                foreach (Field field in row.Row)
                {
                    // Check right direction
                    if (field.IsRightWay)
                    {
                        // Check if there's a way but no more fields to go in this direction.
                        if (field.ColNumber + 1 >= row.Row.Count) return false;
                        // Check if there's field but that field has no way to the currently.
                        if (!GetField(userName, gameTitle, field.RowNumber, field.ColNumber + 1).IsLeftWay) return false;
                    }

                    // Check left direction.
                    if (field.IsLeftWay)
                    {
                        if (field.ColNumber - 1 < 0) return false;
                        if (!GetField(userName, gameTitle, field.RowNumber, field.ColNumber - 1).IsRightWay) return false;
                    }

                    // Check up direction.
                    if (field.IsUpWay)
                    {
                        if (field.RowNumber - 1 < 0) return false;
                        if (!GetField(userName, gameTitle, field.RowNumber - 1, field.ColNumber).IsDownWay) return false;
                    }

                    // Check down direction.
                    if (field.IsDownWay)
                    {
                        if (field.RowNumber + 1 >= row.Row.Count) return false;
                        if (!GetField(userName, gameTitle, field.RowNumber + 1, field.ColNumber).IsUpWay) return false;
                    }
                }
            }
            return true;
        }

        public Boolean IsStartFieldSet(String userName, String gameTitle)
        {
            Game game = _context.Game.Where(game => game.Owner.UserName == userName && game.Title == gameTitle)
                                       .Include(game => game.StartField)
                                       .FirstOrDefault();
            return game.StartField != null && (game.StartField.IsDownWay || game.StartField.IsLeftWay
                                                || game.StartField.IsRightWay || game.StartField.IsUpWay);
        }

        public Boolean IsTargetFieldSet(String userName, String gameTitle)
        {
            Game game = _context.Game.Where(game => game.Owner.UserName == userName && game.Title == gameTitle)
                                       .Include(game => game.TargetField)
                                       .FirstOrDefault();
            return game.TargetField != null && (game.TargetField.IsDownWay || game.TargetField.IsLeftWay
                                                || game.TargetField.IsRightWay || game.TargetField.IsUpWay);
        }

        public Boolean IsPreludeFilled(String userName, String gameTitle)
        {
            return _context.Game.Any(game => game.Owner.UserName == userName &&
                                             game.Title == gameTitle &&
                                             game.Prelude != null &&
                                             game.Prelude.Text != null&&
                                             game.Prelude.Text != "");
        }

        public Boolean IsGameWonFilled(String userName, String gameTitle)
        {
            return _context.Game.Any(game => game.Owner.UserName == userName &&
                                             game.Title == gameTitle &&
                                             game.GameWonResult != null&&
                                             game.GameWonResult.Text != null &&
                                             game.GameWonResult.Text != "");
        }

        public Boolean IsGameLostFilled(String userName, String gameTitle)
        {
            return _context.Game.Any(game => game.Owner.UserName == userName &&
                                             game.Title == gameTitle &&
                                             game.GameLostResult != null &&
                                             game.GameLostResult.Text != null&&
                                             game.GameLostResult.Text != "");
        }

        public void SetReadyToPlay(String userName, String gameTitle)
        {
            Game game = GetGameWithoutIncludes(userName, gameTitle);
            if (!game.IsReadyToPlay)
            {
                game.IsReadyToPlay = true;
                _context.SaveChanges();
            }
        }

        public void SetNotReadyToPlay(String userName, String gameTitle)
        {
            Game game = GetGameWithoutIncludes(userName, gameTitle);
            if (game.IsReadyToPlay)
            {
                game.IsReadyToPlay = false;
                _context.SaveChanges();
            }
        }

        #endregion

        #region Search for solution of a game

        // Search for an optimal path from the start field to the target field in the game's map
        // to find out if it has solution and if has, how long is that optimal path.
        public int? SearchForSolution(String userName, String gameTitle)
        {

            //==----- Initialize variables for the algorithm. -----==//

            // Initialize graph from the map.
            List<List<GraphNode>> graph = InitializeGraphFromMap(userName, gameTitle);

            // Intialize and check start and target field
            Field startField = _context.Game.Where(game => game.Owner.UserName == userName && game.Title == gameTitle)
                                            .Select(game => game.StartField)
                                            .FirstOrDefault();
            Field targetField = _context.Game.Where(game => game.Owner.UserName == userName && game.Title == gameTitle)
                                            .Select(game => game.TargetField)
                                            .FirstOrDefault();
            if(startField == null || targetField == null)
            {
                return null;
            }  

            // Intilaize start field distance from the start field to 0.
            graph[startField.RowNumber][startField.ColNumber].PathLength = 0;

            // Intialize the known nodes' list.
            List<GraphNode> knownNodes = new List<GraphNode>();
            knownNodes.Add(graph[startField.RowNumber][startField.ColNumber]);

            // Initialize opened nodes with the start field.
            List<GraphNode> openedNodes = new List<GraphNode>();
            openedNodes.Add(graph[startField.RowNumber][startField.ColNumber]);

            // Intitleize currentNode.
            GraphNode currentNode;

            //==----- The algorithm -----==//

            // If there's no more opened nodes, that means we analysed all and don't find the target.
            // So there's no solution.
            while (openedNodes.Any())
            {

                // Get the most optimal node to analyse from the opened nodes.
                currentNode = SearchNextNode(openedNodes, targetField);

                // If this node is the target, we found the solution.
                if(currentNode.ColNumber == targetField.ColNumber && currentNode.RowNumber == targetField.RowNumber)
                {
                    return currentNode.PathLength;
                }

                // Remove the current node from opened nodes, because we will analyse it now, so it will be no 
                // longer opened.
                openedNodes.Remove(currentNode);

                // Analyse the current node.
                foreach(GraphNode node in GetChildren(currentNode, graph))
                {
                    // If this is an unvisited node, add it to the path,
                    // if this is a visited node but we found a better way to it, refresh the node with this 
                    // better way's data.
                    if(!knownNodes.Contains(node) || currentNode.PathLength + 1 < node.PathLength)
                    {
                        // The children of the currentNode are the neighbors of it, so theirs distance from 
                        // each other is 1. So: ( c(node, currentNode) == 1 ).
                        node.Parent = currentNode;
                        node.PathLength = currentNode.PathLength + 1;
                        openedNodes.Add(node);
                        knownNodes.Add(node);
                    }
                }
            }
            
            // If the loop eneds, that means there's no more opened nodes and we didn't find the target field.
            // It means that there's no way from the start field to the target field, so there's no solution.
            return null;
        }


        #region Helper functions

        // Intializes the graph from the game map for searching.
        private List<List<GraphNode>> InitializeGraphFromMap(String userName, String gameTitle)
        {
            // Get the game's map from the database.
            List<MapRow> map = SortMap(GetMap(userName, gameTitle));

            // Initialize the empty graph.
            List<List<GraphNode>> graph = new List<List<GraphNode>>();

            // Fill the empty graph with information from the game's map.
            for (int i = 0; i < map.Count; ++i)
            {
                graph.Add(new List<GraphNode>());
                for (int j = 0; j < map[i].Row.Count; ++j)
                {
                    Field field = _context.Field.Where(field => field.GameTitle == gameTitle && field.UserName == userName
                            && field.ColNumber == j && field.RowNumber == i).FirstOrDefault();
                    graph[i].Add(new GraphNode());
                    graph[i][j] = new GraphNode()
                    {
                        ColNumber = field.ColNumber,
                        RowNumber = field.RowNumber,
                        Parent = null,
                        PathLength = 15 * 15 + 1,
                        IsUpWay = field.IsUpWay,
                        IsDownWay = field.IsDownWay,
                        IsRightWay = field.IsRightWay,
                        IsLeftWay = field.IsLeftWay
                    };
                }
            }
            return graph;
        }

        // Calculates a number, which represents that the node is how far from the target field
        // and how many ways go out of it. (The closer to the target, the more outgoing way is the better.)
        private int HeuristicFunction(GraphNode node, Field targetField)
        {
            // Count the numbers of ways we can go out of the field represented by this node.
            int ways = 4;
            if (node.IsUpWay) ways--;
            if (node.IsDownWay) ways--;
            if (node.IsRightWay) ways--;
            if (node.IsLeftWay) ways--;

            // Estimate the distance of the target field.
            return Math.Abs(node.RowNumber - targetField.RowNumber) + Math.Abs(node.ColNumber - targetField.ColNumber) + ways;
        }


        // Gets the most optimal node from the opened nodes list.
        private GraphNode SearchNextNode(List<GraphNode> openedNodes, Field targetField)
        {
            // Search for the node with the best heuristic number in the opened node's set.
            GraphNode minNode = openedNodes[0];
            foreach(GraphNode node in openedNodes)
            {
                if(HeuristicFunction(node, targetField) 
                    < HeuristicFunction(minNode, targetField) )
                {
                    minNode = node;
                }
            }
            return minNode;
        }


        // Get all the children of a node (which is it's neighbours).
        private List<GraphNode> GetChildren(GraphNode node, List<List<GraphNode>> graph)
        {
            List<GraphNode> neighbors = new List<GraphNode>();
            if (node.IsUpWay && node.RowNumber - 1 >= 0)
            {
                neighbors.Add(graph[node.RowNumber - 1][node.ColNumber]);
            }
            if(node.IsDownWay && node.RowNumber+1 < graph.Count)
            {
                neighbors.Add(graph[node.RowNumber + 1][node.ColNumber]);
            }
            if(node.IsRightWay && node.ColNumber+1 < graph.Count)
            {
                neighbors.Add(graph[node.RowNumber][node.ColNumber + 1]);
            }
            if(node.IsLeftWay && node.ColNumber-1 >= 0)
            {
                neighbors.Add(graph[node.RowNumber][node.ColNumber - 1]);
            }
            return neighbors;
        }
        #endregion

        #endregion

        #region Usually used getter functions

        public String GetFieldTextContent(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            String textContent =_context.Field
                .Where(field => field.UserName == userName && field.GameTitle == gameTitle
                && field.RowNumber == rowNumber && field.ColNumber == colNumber)
                .Select(field => field.Text)
                .FirstOrDefault();
            if(textContent == null)
            {
                return "";
            }
            else
            {
                return textContent;
            }
        }
        public Field GetField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            return GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
        }

        public List<MapRow> GetMap(String userName, String gameTitle)
        {
            return SortMap(_context.Game
                .Where(game => game.Owner.UserName == userName & game.Title == gameTitle)
                .Include(game => game.Map)
                .ThenInclude(map => map.Row)
                .Select(game => game.Map)
                .FirstOrDefault()
                .ToList());
        }

        #endregion

        #region Private helper functions

        private Field GetFieldWithoutIncludes(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            return _context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle &&
                    f.RowNumber == rowNumber && f.ColNumber == colNumber)
                .FirstOrDefault();
        }

        private Game GetGameWithoutIncludes(String userName, String gameTitle)
        {
            return _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle).FirstOrDefault();
        }

        private byte[] ConvertIFormFileToImage(IFormFile file)
        {
            byte[] image = null;
            using (var fs = file.OpenReadStream())
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                image = ms.ToArray();
            }
            return image;
        }

        private List<MapRow> SortMap(List<MapRow> map)
        {
            foreach(MapRow row in map)
            {
                row.Row = row.Row.OrderBy(row => row.ColNumber).ToList();
            }
            return map;
        }

        private Game GetGameAtTitle(String userName, String gameTitle)
        {
            return _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Owner)
                .Include(g => g.CoverImage)
                .Include(g => g.GameLostResult)
                .ThenInclude(result => result.Image)
                .Include(g => g.GameWonResult)
                .ThenInclude(result => result.Image)
                .Include(g => g.Prelude)
                .ThenInclude(p => p.Image)
                .Include(g => g.Map)
                .ThenInclude(map => map.Row)
                .FirstOrDefault();
        }

        private Field GetFieldAtCoordinate(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            return _context.Field
                .Where(field => field.UserName == userName && field.GameTitle == gameTitle)
                .Where(field => field.ColNumber == colNumber && field.RowNumber == rowNumber)
                .Include(field => field.Image)
                .Include(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .ThenInclude(alternatives => alternatives.TrialResult)
                .FirstOrDefault();
        }

        private Byte[] GetImage(int wayDirections, MapTheme theme)
        {
            return _context.MapImage
                    .Where(image => image.WayDirectionsCode == wayDirections && image.Theme == theme)
                    .Select(image => image.Image)
                    .FirstOrDefault();
        }

        private int GetCurrentWayDirectionsCode(String userName, String gameTitle)
        {
            return _context.Game
                .Where(game => game.Owner.UserName == userName & game.Title == gameTitle)
                .Select(game => game.CurrentWayDirectionsCode)
                .FirstOrDefault();
        }

        #endregion
    }

}
