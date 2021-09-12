using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Media;

namespace LevelConstructor
{
    public class LazerLogic
    {
        private readonly Cell[] _cells;
        private Cell[] _receivers;
        private Cell[] _teleports;

        private bool _isXMode; // режим когда лазеры проникают

        public LazerLogic(Cell[] cells)
        {
            _cells = cells;
        }

        public bool Init()
        {
            _isXMode = _cells.Where(x => x.GameObject != null).Any(x => x.GameObject.IsLaserXObject);

            foreach (var gameObject in _cells.Where(x => x.GameObject != null).Select(x => x.GameObject))
            {
	            gameObject.IncomingLasers = 0;
                gameObject.IsActive = false;
                foreach (var lazerPort in gameObject.Ports)
                {
                    lazerPort.Off();
                    lazerPort.SetColor((_isXMode)? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red);
                    lazerPort.PortState = LazerPort.State.Inactive;
                }
            }
            _portsEx.Clear();

            _receivers = _cells.Where(x => x.GameObject != null && x.GameObject.IsReceiver).ToArray();
            _teleports = _cells.Where(x => x.GameObject != null && x.GameObject.IsTeleport).ToArray();

            if (_teleports.Any() && _teleports.Count() != 2)
            {
                return false;
            }

            foreach (Cell cell in _cells.Where(c => c.GameObject != null && c.GameObject.Type.IsLaserSource()))
            {
                Activate(cell);
            }
	        return IsWin();
        }

        private void Activate(Cell cell)
        {
            var gameObj = cell.GameObject;
            gameObj.IsActive = true;
            foreach (LazerPort lazerPort in gameObj.Ports.Where(x => x.PortState != LazerPort.State.LaserInput))
            {
                ActivatePort(lazerPort, cell);
            }
        }

        private void ActivatePort(LazerPort port, Cell owner)
        {
            port.PortState = LazerPort.State.LaserSource;
            Cell crossCell = GetCrossObject(owner.Row, owner.Column, port.Direction);
            if (crossCell == null)
            {
                {
                    // луч уходит за пределы
                    double x, y;
                    if (port.Direction.X == 1)
                    {
                        x = (_cells.Where(c => c.Column > owner.Column).OrderBy(c => c.Column).LastOrDefault() ?? owner).CanvasX + 10;
                        y = owner.CanvasY;
                    }
                    else if (port.Direction.X == -1)
                    {
                        x = (_cells.Where(cell => cell.Column < owner.Column).OrderBy(cell => cell.Column).FirstOrDefault() ?? owner).CanvasX - 10;
                        y = owner.CanvasY;
                    }
                    else if (port.Direction.Y == 1)
                    {
                        x = owner.CanvasX;
                        y = (_cells.Where(cell => cell.Row > owner.Row).OrderBy(cell => cell.Row).LastOrDefault() ?? owner).CanvasY + 10;
                    }
                    else
                    {
                        x = owner.CanvasX;
                        y = (_cells.Where(cell => cell.Row < owner.Row).OrderBy(cell => cell.Row).FirstOrDefault() ?? owner).CanvasY - 10;
                    }
                    port.On(x, y);
                }
                if (_isXMode)
                {
                    // луч возвращается
                    Cell crossCellEx = GetCrossObject(owner.Row, owner.Column, port.Direction, true);
                    // координаты границы нового луча из-за пределов
                    double x1, y1;
                    if (port.Direction.X == 1)
                    {
                        x1 = (_cells.OrderBy(cell => cell.Column).FirstOrDefault() ?? owner).CanvasX - 10;
                        y1 = owner.CanvasY;
                    }
                    else if (port.Direction.X == -1)
                    {
                        x1 = (_cells.OrderBy(cell => cell.Column).LastOrDefault() ?? owner).CanvasX + 10;
                        y1 = owner.CanvasY;
                    }
                    else if (port.Direction.Y == 1)
                    {
                        x1 = owner.CanvasX;
                        y1 = (_cells.OrderBy(cell => cell.Row).FirstOrDefault() ?? owner).CanvasY - 10;
                    }
                    else
                    {
                        x1 = owner.CanvasX;
                        y1 = (_cells.OrderBy(cell => cell.Row).LastOrDefault() ?? owner).CanvasY + 10 ;
                    }
                    if (crossCellEx != null)
                    {
                        var newport = _portsEx.Add(new System.Windows.Point(x1, y1), port, crossCellEx.Canvas);
                        OnGameObjLazerEnter(crossCellEx, newport);
                    }
                    else
                        throw new Exception("Нет объекта для луча");
                }
            }
            else
            {
                OnGameObjLazerEnter(crossCell, port);
            }
        }
        
        PortFactory _portsEx = new PortFactory();
        class PortFactory
        {
            List<LazerPort> _ports = new List<LazerPort>();
            public void Clear() 
            {
                foreach (var lazerPort in _ports)
                {
                    lazerPort.Off();
                    lazerPort.PortState = LazerPort.State.Inactive;
                }
                _ports.Clear(); 
            }
            public LazerPort Add(System.Windows.Point point, LazerPort port, Canvas canvas)
            {
                var line = new Line() { X1 = point.X, Y1 = point.Y, Stroke = System.Windows.Media.Brushes.Green, StrokeThickness = 1, Opacity = 0.5 };
                line.IsHitTestVisible = false;
                line.Visibility = Visibility.Collapsed;
                canvas.Children.Add(line);
                Canvas.SetZIndex(line, 4);
                var newport = new LazerPort(port.Direction, LazerPort.State.LaserSource, line);
                _ports.Add(newport);
                return newport;
            }
        };

        private void OnGameObjLazerEnter(Cell cell, LazerPort remotePort)
        {
            GameObject obj = cell.GameObject;
	        obj.IncomingLasers++;
            if (obj.IsReceiver)
            {
                remotePort.On(cell.CanvasX, cell.CanvasY);
                //remotePort.On(cell.transform.position);
            }
            else if (obj.IsTeleport)
            {
                obj.IsActive = true;
                LazerPort inputPort = obj.Ports.First(x => x.Direction == -remotePort.Direction);
                remotePort.On(cell.CanvasX, cell.CanvasY);
                Cell anotherTeleport = _teleports.First(x => x != cell);

                anotherTeleport.GameObject.IsActive = true;
                LazerPort outputPort = anotherTeleport.GameObject.Ports.First(x => x.Direction == remotePort.Direction);
                if (outputPort.PortState != LazerPort.State.LaserSource)
                {
                    ActivatePort(outputPort, anotherTeleport);
                }
            }
            else
            {
                LazerPort inputPort = obj.Ports.FirstOrDefault(p => p.Direction == -remotePort.Direction);
                if (inputPort != null)
                {
                    remotePort.On(cell.CanvasX, cell.CanvasY);
                    if (inputPort.PortState != LazerPort.State.LaserSource)
                    {
                        inputPort.PortState = LazerPort.State.LaserInput;
                        if (!obj.IsActive)
                            Activate(cell);
                    }
                }
                else
                {
                    remotePort.On(cell.CanvasX, cell.CanvasY);
                }
            }
        }

        private Cell GetCrossObject(int row, int column, Vector direction, bool isxmode = false)
        {
            if (!isxmode)
            {
                if (direction.X == -1)
                {
                    return
                        _cells.Where(cell => cell.GameObject != null)
                            .Where(cell => cell.Column < column && cell.Row == row)
                            .OrderBy(cell => column - cell.Column)
                            .FirstOrDefault();
                }
                if (direction.X == 1)
                {
                    return
                        _cells.Where(cell => cell.GameObject != null)
                            .Where(cell => cell.Column > column && cell.Row == row)
                            .OrderBy(cell => cell.Column - column)
                            .FirstOrDefault();
                }
                if (direction.Y == -1)
                {
                    return
                        _cells.Where(cell => cell.GameObject != null)
                            .Where(cell => cell.Row < row && cell.Column == column)
                            .OrderBy(cell => row - cell.Row)
                            .FirstOrDefault();
                }
                return
                    _cells.Where(cell => cell.GameObject != null)
                            .Where(cell => cell.Row > row && cell.Column == column)
                            .OrderBy(cell => cell.Row - row)
                            .FirstOrDefault();
            }
            else
            {
                if (direction.X == -1)
                {
                    return
                        _cells.Where(cell => cell.GameObject != null)
                            .Where(cell => cell.Column >= column && cell.Row == row)
                            .OrderBy(cell => column - cell.Column)
                            .FirstOrDefault();
                }
                if (direction.X == 1)
                {
                    return
                        _cells.Where(cell => cell.GameObject != null)
                            .Where(cell => cell.Column <= column && cell.Row == row)
                            .OrderBy(cell => cell.Column - column)
                            .FirstOrDefault();
                }
                if (direction.Y == -1)
                {
                    return
                        _cells.Where(cell => cell.GameObject != null)
                            .Where(cell => cell.Row >= row && cell.Column == column)
                            .OrderBy(cell => row - cell.Row)
                            .FirstOrDefault();
                }
                return
                    _cells.Where(cell => cell.GameObject != null)
                            .Where(cell => cell.Row <= row && cell.Column == column)
                            .OrderBy(cell => cell.Row - row)
                            .FirstOrDefault();
            }
        }

	    private bool IsWin()
	    {
		    return _receivers.Any() && _receivers.Select(x => x.GameObject).All(Check);
	    }

	    private bool Check(GameObject gameObject)
	    {
		    switch (gameObject.Type)
		    {
			    case ObjectType.LaserReceiver_0:
				    return gameObject.IncomingLasers == 0;
			    case ObjectType.LaserReceiver_1:
				    return gameObject.IncomingLasers >= 1;
			    case ObjectType.LaserReceiver_2:
				    return gameObject.IncomingLasers >= 2;
			    case ObjectType.LaserReceiver_3:
				    return gameObject.IncomingLasers >= 3;
			    case ObjectType.LaserReceiver_4:
				    return gameObject.IncomingLasers >= 4;
			    default:
				    return true;
		    }
	    }
    }
}