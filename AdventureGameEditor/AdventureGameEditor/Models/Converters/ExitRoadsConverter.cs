using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models.Converters
{
    public class ExitRoadsConverter
    {
        public WayDirectionsViewModel ExitRoadsToWayDirectionsViewModel(ExitRoads exitRoads)
        {
            if (exitRoads.IsUpWay && exitRoads.IsRightWay && exitRoads.IsDownWay && exitRoads.IsLeftWay)
            {
                return WayDirectionsViewModel.UpRightDownLeft;
            }
            else if(exitRoads.IsRightWay && exitRoads.IsDownWay && exitRoads.IsLeftWay)
            {
                return WayDirectionsViewModel.RightDownLeft;
            }
            else if(exitRoads.IsUpWay && exitRoads.IsRightWay && exitRoads.IsDownWay)
            {
                return WayDirectionsViewModel.UpRightDown;
            }
            else if(exitRoads.IsUpWay && exitRoads.IsDownWay && exitRoads.IsLeftWay)
            {
                return WayDirectionsViewModel.UpDownLeft;
            }
            else if (exitRoads.IsUpWay && exitRoads.IsRightWay && exitRoads.IsLeftWay)
            {
                return WayDirectionsViewModel.UpRightLeft;
            }
            else if(exitRoads.IsDownWay && exitRoads.IsLeftWay)
            {
                return WayDirectionsViewModel.DownLeft;
            }
            else if(exitRoads.IsRightWay && exitRoads.IsLeftWay)
            {
                return WayDirectionsViewModel.RightLeft;
            }
            else if(exitRoads.IsRightWay && exitRoads.IsDownWay)
            {
                return WayDirectionsViewModel.RightDown;
            }
            else if(exitRoads.IsUpWay && exitRoads.IsLeftWay)
            {
                return WayDirectionsViewModel.UpLeft;
            }
            else if(exitRoads.IsUpWay && exitRoads.IsDownWay)
            {
                return WayDirectionsViewModel.UpDown;
            }
            else if(exitRoads.IsUpWay && exitRoads.IsRightWay)
            {
                return WayDirectionsViewModel.UpRight;
            }
            else if (exitRoads.IsLeftWay)
            {
                return WayDirectionsViewModel.Left;
            }
            else if (exitRoads.IsDownWay)
            {
                return WayDirectionsViewModel.Down;
            }
            else if (exitRoads.IsRightWay)
            {
                return WayDirectionsViewModel.Right;
            }
            else if (exitRoads.IsUpWay)
            {
                return WayDirectionsViewModel.Up;
            }
            else
            {
                return WayDirectionsViewModel.Empty;
            }
        }

        public ExitRoads WayDirectionsViewModelToExitRoads(WayDirectionsViewModel wayDirections)
        {
            switch (wayDirections)
            {
                case WayDirectionsViewModel.Empty:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = false,
                        IsDownWay = false,
                        IsLeftWay = false
                    };
                case WayDirectionsViewModel.Up:
                    return new ExitRoads
                    {
                        IsUpWay = true,
                        IsRightWay = false,
                        IsDownWay = false,
                        IsLeftWay = false
                    };
                case WayDirectionsViewModel.Right:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = true,
                        IsDownWay = false,
                        IsLeftWay = false
                    };
                case WayDirectionsViewModel.Down:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = false,
                        IsDownWay = true,
                        IsLeftWay = false
                    };
                case WayDirectionsViewModel.Left:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = false,
                        IsDownWay = false,
                        IsLeftWay = true
                    };
                case WayDirectionsViewModel.UpRight:
                    return new ExitRoads
                    {
                        IsUpWay = true,
                        IsRightWay = true,
                        IsDownWay = false,
                        IsLeftWay = false
                    };
                case WayDirectionsViewModel.UpDown:
                    return new ExitRoads
                    {
                        IsUpWay = true,
                        IsRightWay = false,
                        IsDownWay = true,
                        IsLeftWay = false
                    };
                case WayDirectionsViewModel.UpLeft:
                    return new ExitRoads
                    {
                        IsUpWay = true,
                        IsRightWay = false,
                        IsDownWay = false,
                        IsLeftWay = true
                    };
                case WayDirectionsViewModel.RightDown:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = true,
                        IsDownWay = true,
                        IsLeftWay = false
                    };
                case WayDirectionsViewModel.RightLeft:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = true,
                        IsDownWay = false,
                        IsLeftWay = true
                    };
                case WayDirectionsViewModel.DownLeft:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = false,
                        IsDownWay = true,
                        IsLeftWay = true
                    };
                case WayDirectionsViewModel.UpRightLeft:
                    return new ExitRoads
                    {
                        IsUpWay = true,
                        IsRightWay = true,
                        IsDownWay = false,
                        IsLeftWay = true
                    };
                case WayDirectionsViewModel.UpDownLeft:
                    return new ExitRoads
                    {
                        IsUpWay = true,
                        IsRightWay = false,
                        IsDownWay = true,
                        IsLeftWay = true
                    };
                case WayDirectionsViewModel.UpRightDown:
                    return new ExitRoads
                    {
                        IsUpWay = true,
                        IsRightWay = true,
                        IsDownWay = true,
                        IsLeftWay = false
                    };
                case WayDirectionsViewModel.RightDownLeft:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = true,
                        IsDownWay = true,
                        IsLeftWay = true
                    };
                case WayDirectionsViewModel.UpRightDownLeft:
                    return new ExitRoads
                    {
                        IsUpWay = true,
                        IsRightWay = true,
                        IsDownWay = true,
                        IsLeftWay = true
                    };
               default:
                    return new ExitRoads
                    {
                        IsUpWay = false,
                        IsRightWay = false,
                        IsDownWay = false,
                        IsLeftWay = false
                    };
            }
        }
    }
}
