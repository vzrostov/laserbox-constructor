using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LevelConstructor
{
    public delegate void ChangedDelegate(bool isWin);

	public partial class GameField
	{

		#region fields

		private List<Cell> _cells;
		private LazerLogic _lazerLogic;

		#endregion

		public GameField()
		{
			SelectedCells = new List<Cell>();
		    ChangedAfterShake = false;
		}

		public List<Cell> SelectedCells { get; set; }

		public event EventHandler<SelectionChangedEventArgs> SelectionChanged; 

		public event ChangedDelegate Changed;

        // устанавливается если что-то поменялось и
        // сбрасывается если произошла установка стартовой позиции.
        public bool ChangedAfterShake { get; set; }

        protected void SomethingChanged()
        {
            ChangedAfterShake = true;
        }
        
        protected void RaiseWinStateChanged(bool value)
        {
			var handler = Changed;
			if(handler != null)
				handler(value);
		}

		public IEnumerable<Cell> Cells { get { return _cells; }} 

		public void Initialize(IEnumerable<Cell> cells)
		{
			if (cells == null) throw new ArgumentNullException("cells");
			_cells = cells.ToList();
			_lazerLogic = new LazerLogic(_cells.ToArray());
		    foreach (var cell in _cells)
		    {
		        cell.Click += OnCellClick;
		        cell.OnDropEvent += (sender, args) =>
		        {
		            RaiseWinStateChanged(_lazerLogic.Init());
		            SomethingChanged();
		        };
		    cell.Selected = false;
			}
		}

		public void SelectAll()
		{
			foreach (var cell in _cells.Except(SelectedCells))
			{
				OnCellClick(cell, new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left));
			} 
		}

		public void SetAreaType(CellType type)
		{
			foreach (var cell in SelectedCells)
			{
				cell.Type = type;
			}
			UpdateProperties();
		    SomethingChanged();
		}

		public void SetObject(ObjectType type, int direction)
		{
			if(SelectedCells.Count == 0)
				return;
			foreach (var cell in SelectedCells)
			{
				cell.GameObject = new GameObject(type, direction, cell.Lines);
			}
			RaiseWinStateChanged(_lazerLogic.Init());
		    SomethingChanged();
		}

        public void SetButton()
        {
//            RaiseWinStateChanged(_lazerLogic.Init());
//            SomethingChanged();
        }

		public void Rotate(bool byHour)
		{
			if (SelectedCells.Count == 1)
			{
				var cell = SelectedCells.First();
				RotateNew(cell, byHour);
				RaiseWinStateChanged(_lazerLogic.Init());
                SomethingChanged();
            }
		}

		public void RemoveSelected()
		{
			foreach (var cell in SelectedCells)
			{
				foreach (var line in cell.Lines)
				{
					line.Visibility = Visibility.Collapsed;
				}
				if (cell.GameObject != null)
					cell.GameObject = null;
				cell.Selected = false;
			}
			SelectedCells.Clear();
			RaiseWinStateChanged(_lazerLogic.Init());
            SomethingChanged();
        }

		public void Clear()
		{
			SelectedCells.Clear();
			foreach (var cell in _cells)
			{
				foreach (var line in cell.Lines)
				{
					line.Visibility = Visibility.Collapsed;
				}
				cell.GameObject = null;
				cell.Selected = false;
				cell.Type = CellType.Default;
			}
			RaiseWinStateChanged(_lazerLogic.Init());
            SomethingChanged();
        }

		public void RemoveMoveable()
		{
			foreach (var cell in _cells.Where(c => c.GameObject != null && c.GameObject.IsMoved))
			{
				cell.Selected = false;
				cell.GameObject = null;
			}
			RaiseWinStateChanged(_lazerLogic.Init());
            SomethingChanged();
            SelectedCells.Clear();
		}

		public void ShakeIt()
		{
			foreach (var selectedCell in SelectedCells)
			{
				selectedCell.Selected = false;
			}
			SelectedCells.Clear();

			var moved = _cells.Where(x => x.GameObject != null && x.GameObject.IsMoved).ToArray();
			if(!moved.Any())
				return;

			for (int i = 0; i < 100; i++)
			{
				for(int j = 0; j < moved.Length; j++)
				{
					var newCell = TryMove(moved[j]);
					if (newCell != null)
						moved[j] = newCell;
				}
			}
			foreach (var line in _cells.SelectMany(x => x.Lines))
			{
				line.Visibility = Visibility.Collapsed;
			}
			RaiseWinStateChanged(_lazerLogic.Init());
            ChangedAfterShake = false; // !!!!
        }

		public void InitLazers()
		{
			RaiseWinStateChanged(_lazerLogic.Init());
		}

		public int SplitCount()
		{
			var objects = _cells.Where(x => x.GameObject != null).Select(x => x.GameObject).ToArray();
			return objects.Count(x => x.Type == ObjectType.MovedReflectorAll || x.Type == ObjectType.StacionarReflectorAll)*2
			       +
			       objects.Count(
				       x => x.Type == ObjectType.MovedReflectorTriangle || x.Type == ObjectType.StacionarReflectorTriangle)
                   + objects.Count(x => x.Type.IsLaserSource());
		}

		public int RecvCount()
		{
			var objects = _cells.Where(x => x.GameObject != null).Select(x => x.GameObject).ToArray();
			return objects.Count(x => x.Type == ObjectType.LaserReceiver_4)*4
			       + objects.Count(x => x.Type == ObjectType.LaserReceiver_3)*3
			       + objects.Count(x => x.Type == ObjectType.LaserReceiver_2)*2
			       + objects.Count(x => x.Type == ObjectType.LaserReceiver_1);
		}

		public void PushController(GameObject obj)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			if (!obj.IsController) throw new ArgumentException("obj");
			if (!obj.ControllerType.HasValue) throw new ArgumentException("obj");

			foreach (Cell cell in _cells.Where(c => c.GameObject != null && (c.GameObject.ConnectedControllerType == obj.ConnectedControllerType)))
			{
				RotateNew(cell, true);
			}
			RaiseWinStateChanged(_lazerLogic.Init());
            SomethingChanged();
        }

		private void OnCellClick(Cell cell, MouseButtonEventArgs mouseButtonEventArgs)
		{
			if(cell.Selected && mouseButtonEventArgs.RightButton == MouseButtonState.Pressed)
				return;
			if (Keyboard.IsKeyDown(Key.LeftAlt) && cell.GameObject != null && cell.GameObject.IsController)
			{
                foreach (var c in Cells.Where(c => c.GameObject != null && (c.GameObject.ConnectedControllerType == cell.GameObject.ControllerType)))
				{
					RotateNew(c, false);
				}
				RaiseWinStateChanged(_lazerLogic.Init());
                SomethingChanged();
                return;
			}
			cell.Selected = !cell.Selected;
			if (Keyboard.IsKeyDown(Key.LeftCtrl) && cell.Selected)
			{
				SelectedCells.Add(cell);
			}
			else
			{
				if (cell.Selected)
				{
					foreach (var selectedCell in SelectedCells)
					{
						selectedCell.Selected = false;
					}
					SelectedCells.Clear();
					SelectedCells.Add(cell);
				}
				else
				{
					SelectedCells.Remove(cell);
				}
			}
			UpdateProperties();
		}

		private void UpdateProperties()
		{
			var handler = SelectionChanged;
			if(handler != null)
				handler(this, new SelectionChangedEventArgs() { Cells = _cells, SelectedCells = SelectedCells});
		}

		public bool HasObject(ObjectType type)
		{
			return _cells.Where(x => x.GameObject != null).Any(x => x.GameObject.Type == type);
		}

		private Random _rnd = new Random();

		private Cell TryMove(Cell cell)
		{
			var nearCells = new[]
			{
				_cells.FirstOrDefault(x => x.Column == cell.Column + 1 && x.Row == cell.Row),
				_cells.FirstOrDefault(x => x.Column == cell.Column && x.Row == cell.Row + 1),
				_cells.FirstOrDefault(x => x.Column == cell.Column - 1 && x.Row == cell.Row),
				_cells.FirstOrDefault(x => x.Column == cell.Column && x.Row == cell.Row - 1),
			}.Where(x => x != null && x.Type == CellType.Default && x.GameObject == null).ToArray();
			if (nearCells.Any())
			{
				var newCell = nearCells[_rnd.Next(0, nearCells.Length)];
				newCell.GameObject = cell.GameObject;//new GameObject(cell.GameObject.Type, cell.GameObject.Angle, newCell.Lines);
				newCell.GameObject.Lines = newCell.Lines;
				cell.GameObject = null;
				return newCell;
			}
			return null;
		}

		private void RotateNew(Cell cell, bool byHour)
		{
			var angle = byHour ? -90 : 90;
			if (cell.GameObject != null)
			{
				angle = cell.GameObject.Angle + angle;
				cell.GameObject.Angle = angle < 0 ? 360 + (angle % 360) : angle % 360;
				cell.GameObject = cell.GameObject;
			}
		}
	}
}