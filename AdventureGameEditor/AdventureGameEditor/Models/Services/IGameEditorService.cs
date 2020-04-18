using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AdventureGameEditor.Models
{
    public interface IGameEditorService
    {
        #region Create game
        public Boolean InicializeGame(String title, int mapSize, Visibility visibility, User owner, 
            IFormFile CoverImage);
        #endregion
        #region Create map
        #region Getters
        public MapViewModel GetMapViewModel(String userName, String gameTitle);
        public MapPieceViewModel GetFieldViewModel(String userName, String gameTitle, int rowNumber, int colNumber);
        public FileResult ImageForMap(int? wayDirectionsCode);
        public String GetTextAtCoordinate(String userName, String gameTitle, int rowNumber, int colNumber);
        #endregion
        #region Setters
        public void SetExitRoads(String userName, String gameTitle, int rowNumber, int colNumber);
        public void SetCurrentWayDirectionsCode(String userName, String gameTitle, int newWayDirectionsCode);
        #endregion
        #endregion
        #region Create map content
        #region Initializers
        public Trial InitializeTrial(String userName, String gameTitle, int rowNumber, int colNumber);
        public List<Alternative> InitializeAlternatives(int count);
        public List<String> InitializeAlternativeTexts(int count);
        public List<TrialResult> InitializeTrialResults(int count);
        #endregion
        #region Save
        public void AddTextAndImageForField(String userName, String gameTitle, int rowNumber, int colNumber, String Text, IFormFile image);
        public void SaveTrial(String userName, String gameTitle, int rowNumber, int colNumber, List<String> alternativeTexts,
           List<TrialResult> alternativeTrialResults, TrialType trialType, String trialText);
        public void AddNewAlternativeToForm(String userName, String gameTitle, int rowNumber, int colNumber);
        #endregion
        #region Getters
        public MapContentViewModel GetMapContentViewModel(String userName, String gameTitle);
        public Trial GetTrial(String userName, String gameTitle, int colNumber, int rowNumber);

        // Currently unused.
         public FieldContentViewModel GetFieldContentViewModel(String userName, String gameTitle, int rowNumber, int colNumber);
        public FieldTrialContentViewModel GetFieldTrialContentViewModel(String userName, String gameTitle, int rowNumber, int colNumber);
        public FileContentResult GetFieldImage(int imageID);
        #endregion
        #endregion
        #region Set start and target fields
        public void SaveStartField(String userName, String gameTitle, int rowNumber, int colNumber);
        public void SaveTargetField(String userName, String gameTitle, int rowNumber, int colNumber);
        #endregion
        #region Show game details
        #region Getters
        public GameDetailsViewModel GetGameDetailsViewModel(String userName, String gameTitle);
        public FieldDetailsViewModel GetFieldDetailsViewModel(String userName, String gameTitle, int colNumber, int rowNumber);
        public FileContentResult GetPreludeImage(int imageID);
        public FileContentResult GetGameResultImage(int imageID);
        public FileContentResult GetCoverImage(int imageID);
        #endregion
        #endregion
        #region Create game result
        public Boolean SaveGameResults(String userName, String gameTitle, String gameWonResult, String gameLostResult,
            String prelude, IFormFile preludeImage, IFormFile gameWonImage, IFormFile gameLostImage);
        public GameResultViewModel GetGameResult(String userName, String gameTitle);
        #endregion
        #region Usually used getter functions
        public String GetFieldTextContent(String userName, String gameTitle, int rowNumber, int colNumber);
        public Field GetField(String userName, String gameTitle, int rowNumber, int colNumber);
        #endregion
    }
}
