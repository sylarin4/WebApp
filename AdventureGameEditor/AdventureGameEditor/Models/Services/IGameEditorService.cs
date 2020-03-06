using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AdventureGameEditor.Models
{
    public interface IGameEditorService
    {
        public Boolean InicializeGame(String title, int mapSize, Visibility visibility, User owner);
        public MapViewModel GetMapViewModel(String userName, String gameTitle);
        public MapPieceViewModel GetMapPieceViewModel(String userName, String gameTitle, int rowNumber, int colNumber);
        public void AddTextToAFieldAt(String userName, String gameTitle, int rowNumber, int colNumber, String Text);
        public void SetExitRoads(String userName, String gameTitle, int rowNumber, int colNumber);
        public FileResult ImageForMap(int? wayDirectionsCode);
        public void SetCurrentWayDirectionsCode(String userName, String gameTitle, int newWayDirectionsCode);
    }
}
