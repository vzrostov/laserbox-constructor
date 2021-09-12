using System.Collections.Generic;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Point = System.Drawing.Point;

namespace LevelConstructor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region fields
        private RelayCommand _removeMoveable;
        private RelayCommand _newLevelCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _loadCommand;
        private RelayCommand _shakeCommand;
        private RelayCommand _turnHCommand;
        private RelayCommand _turnVCommand;
        private RelayCommand _viewWinCombination;
        private RelayCommand _viewStartCombination;
        private GameField _gameField; // basic model
        private Dictionary<GameObject, FullPosition> _positions = new Dictionary<GameObject, FullPosition>();
        #endregion

        private const string _cappTitleName = "LaserBox Level Constructor";

        private string _appTitleName = _cappTitleName;
        private string getAppTitleName(string v) => string.IsNullOrEmpty(v) ? _cappTitleName : _cappTitleName + ": [" + v + "]";
        public string AppTitleName
        {
            get { return _appTitleName; }
            set
            {
                if (_appTitleName != getAppTitleName(value))
                {
                    
                    _appTitleName = getAppTitleName(value);
                    RaisePropertyChanged("AppTitleName");
                }
            }
        }

	    private int _splitCount;
	    public int SplitCount
	    {
			get { return _splitCount; }
		    set
		    {
			    if (value != _splitCount)
			    {
				    _splitCount = value;
					RaisePropertyChanged("SplitCount");
			    }
		    }
	    }

		private int _recvCount;
		public int RecvCount
		{
			get { return _recvCount; }
			set
			{
				if (value != _recvCount)
				{
					_recvCount = value;
					RaisePropertyChanged("RecvCount");
				}
			}
		}

        private bool _isEditMode = true;
        public bool IsEditMode
        {
            get { return _isEditMode; }
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    AppCommon.Freeze = !value;
                    RaisePropertyChanged("IsEditMode");
                }
            }
        }

		private bool _isWin = false;
		public bool IsWin
		{
			get { return _isWin; }
			set
			{
				if (_isWin != value)
				{
					_isWin = value;
					RaisePropertyChanged("IsWin");
				}
			}
		}

        public Cell[] Cells
        {
            set
            {
				_gameField.Initialize(value);
            }
        }

		public IFieldInitializer FieldInitializer { get; set; }

        public PropertiesViewModel Properties { get; private set; }

        public MainViewModel()
        {
			_gameField = new GameField(); 
			_gameField.Changed += win =>
			{
				IsWin = win;
				RecvCount = _gameField.RecvCount();
				SplitCount = _gameField.SplitCount();
			};
            Properties = new PropertiesViewModel();
			Properties.SetGameField(_gameField);
        }

        #region Properties

        private bool _isWinComb = true;
        public bool IsWinCombination
        {
            get { return _isWinComb; }
            set
            {
                if(_isWinComb == value)
                    return;
                _isWinComb = value;
                RaisePropertyChanged("IsWinCombination");
                RaisePropertyChanged("IsStartCombination");
                if (value)
                    ShowWinCombination();
                else 
                    ShowStartCombination();
            }
        }

        public bool IsStartCombination
        {
            get { return !IsWinCombination; }
        }

        public ImageSource LevelBackground
        {
           get { return BuilderResources.Background; }
            
        }

	    private string _rows = "9";
	    public string Rows
	    {
		    get { return _rows; }
		    set
		    {
			    if (_rows != value)
			    {
				    _rows = value;
					RaisePropertyChanged("Rows");
			    }
		    }
	    }

		private string _columns = "6";
		public string Columns
		{
			get { return _columns; }
			set
			{
				if (_columns != value)
				{
					_columns = value;
					RaisePropertyChanged("Columns");
				}
			}
		}

        #endregion

        #region Commands

        public ICommand NewLevelCommand
        {
            get
            {
                if(_newLevelCommand == null)
                    _newLevelCommand = new RelayCommand(x => CreateNewLevel());
                return _newLevelCommand;
            }
        }

        public ICommand SaveLevelCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(x => SaveLevel(), x => true);
                }
                return _saveCommand;
            }
        }

        public ICommand LoadLevelCommand
        {
            get
            {
                if(_loadCommand == null)
                    _loadCommand = new RelayCommand(x => LoadLevel());
                return _loadCommand;
            }
        }

        public ICommand TurnHCommand
        {
            get
            {
                if (_turnHCommand == null)
                    _turnHCommand = new RelayCommand(x => TurnH());
                return _turnHCommand;
            }
        }

        public ICommand TurnVCommand
        {
            get
            {
                if (_turnVCommand == null)
                    _turnVCommand = new RelayCommand(x => TurnV());
                return _turnVCommand;
            }
        }

        public ICommand ShakeCommand
        {
            get
            {
                if(_shakeCommand == null)
                {
                    _shakeCommand = new RelayCommand(x => _gameField.ShakeIt());
                }
                return _shakeCommand;
            }
        }

        public ICommand ViewWinCombination
        {
            get
            {
                if(_viewWinCombination == null)
                    _viewWinCombination = new RelayCommand(x => ShowWinCombination());
                return _viewWinCombination;
            }
        }

        public ICommand ViewStartCombination
        {
            get
            {
                if (_viewStartCombination == null)
                    _viewStartCombination = new RelayCommand(x => ShowStartCombination());
                return _viewStartCombination;
            }
        }

	    public ICommand RemoveMoveable
	    {
		    get
		    {
			    if(_removeMoveable == null)
					_removeMoveable = new RelayCommand(x => _gameField.RemoveMoveable());
			    return _removeMoveable;
		    }
	    }
        #endregion

        #region Helpers
        private void CreateNewLevel()
        {
	        int cols, rows;
	        if (!int.TryParse(Rows, out rows))
	        {
		        MessageBox.Show("Invalid rows");
				return;
	        }
			if (!int.TryParse(Columns, out cols))
			{
				MessageBox.Show("Invalid cols");
				return;
			}
            _gameField.Clear();
			_positions = new Dictionary<GameObject, FullPosition>();
	        Cells = FieldInitializer.InitGameField(cols, rows);
	        IsWinCombination = true;
            AppTitleName = String.Empty;
	        //_isWinComb = true;
	        //RaisePropertyChanged("IsWinCombination");
        }

        private bool ValidateAll()
        {
            if (!IsWin)
            {
                MessageBox.Show("Расстановка не победная.");
                return false;
            }
            if ( !_gameField.Cells.Select(cell => cell.GameObject).Where(go => go != null).Any(go => go.IsMoved) )
            {
                MessageBox.Show("Отсутствуют подвижные элементы.");
                return false;
            }
            if (
                !_gameField.Cells.Select(cell => cell.GameObject).Where(go => go != null).Any(go => (go.Type == ObjectType.LaserSource) ||
                    (go.Type == ObjectType.MovedLaserSource) || (go.Type == ObjectType.LaserXSource) || (go.Type == ObjectType.MovedLaserXSource))
                )
            {
                MessageBox.Show("Отсутствуют лазеры или ресиверы.");
                return false;
            }
            if (
                _gameField.Cells.Select(cell => cell.GameObject).Where(go => go != null).Any(go => (go.Type == ObjectType.LaserSource) || (go.Type == ObjectType.MovedLaserSource)) 
                && 
                _gameField.Cells.Select(cell => cell.GameObject).Where(go => go != null).Any(go => (go.Type == ObjectType.LaserXSource) || (go.Type == ObjectType.MovedLaserXSource)) 
                )
            {
                MessageBox.Show("Одновременное использование лазеров разного типа недопустимо.");
                return false;
            }
            if (
                _gameField.HasObject(ObjectType.StacionarControllerRed)
                &&
                !_gameField.Cells.Select(cell => cell.GameObject).Where(go => go != null).Any(go => (go.ConnectedControllerType == ControllerType.Red))
                )
            {
                MessageBox.Show("Нет для красной кнопки элементов.");
                return false;
            }
            if (
                _gameField.HasObject(ObjectType.StacionarControllerBlue)
                &&
                !_gameField.Cells.Select(cell => cell.GameObject).Where(go => go != null).Any(go => (go.ConnectedControllerType == ControllerType.Blue))
                )
            {
                MessageBox.Show("Нет для синей кнопки элементов.");
                return false;
            }
            if (
                _gameField.HasObject(ObjectType.StacionarControllerGreen)
                &&
                !_gameField.Cells.Select(cell => cell.GameObject).Where(go => go != null).Any(go => (go.ConnectedControllerType == ControllerType.Green))
                )
            {
                MessageBox.Show("Нет для зеленой кнопки элементов.");
                return false;
            }
            foreach (var cell in _gameField.Cells)
            {
                if (cell.Type == CellType.Unavailable)
                {
                    if (cell.GameObject != null && cell.GameObject.IsMoved)
                    {
                        MessageBox.Show(string.Format("Подвижный объект над недоступной ячейкой Row:{0} Col:{1}",
                            cell.Row, cell.Column));
                        return false;
                    }
                }
                if (_positions.Any(p => p.Value.Start == null))
                {
                    MessageBox.Show("Не установлена начальная комбинация.");
                    return false;
                }
                if (_gameField.ChangedAfterShake)
                {
                    MessageBox.Show("Не установлена начальная комбинация, так как затронута победная.");
                    return false;
                }
                if (_positions.Any(p => p.Value.Win == null))
                {
                    MessageBox.Show("Не установлена победная комбинация.");
                    return false;
                }
                if (_positions.All(p => p.Value.Start == p.Value.Win))
                {
                    MessageBox.Show("Не перемешана начальная комбинация.");
                    return false;
                }
                if (cell.GameObject != null && cell.GameObject.IsSpined)
                {
                    if (cell.GameObject.ConnectedControllerType != null && cell.GameObject.ButtonDirection == "")
                    {
                        MessageBox.Show("Не выбрано направление поворота для объекта с поворотом.");
                        return false;
                    }
                    if (cell.GameObject.ConnectedControllerType == ControllerType.Red && !_gameField.HasObject(ObjectType.StacionarControllerRed))
                    {
                        MessageBox.Show("Нет красной кнопки.");
                        return false;
                    }
                    if (cell.GameObject.ConnectedControllerType == ControllerType.Blue && !_gameField.HasObject(ObjectType.StacionarControllerBlue))
                    {
                        MessageBox.Show("Нет синей кнопки.");
                        return false;
                    }
                    if (cell.GameObject.ConnectedControllerType == ControllerType.Green && !_gameField.HasObject(ObjectType.StacionarControllerGreen))
                    {
                        MessageBox.Show("Нет зеленой кнопки.");
                        return false;
                    }
                }
            }
            return true;
        }

        private void SaveLevel()
        {
            if(!ValidateAll())
                return;
/*            var dialog = new SaveFileDialog() { Title = "Название уровня" };
            if (dialog.ShowDialog() != true)
                dialog.FileName = "level " + DateTime.Now.ToString("YYYY MMMM DD HHMM") + ".xml";
            if (!dialog.FileName.EndsWith(".xml"))
                dialog.FileName += ".xml";
            */
            for (int lap = 0; lap < 4; lap++) // параллельная запись 4 файлов
            {
                string exname = (_gameField.Cells.Select(cell => cell.GameObject).Where(go => go != null).Any(go => (go.IsHardObject))) ? "H " : " ";

                var FileName = "level" + exname + DateTime.Now.ToString("yyyy MMMM dd HHmmss") + ((lap == 0) ? "" : "-"+lap.ToString()) + ".xml";
                IsWinCombination = false;

                var doc = new XDocument();
                var irows = _gameField.Cells.Max(x => x.Row) + 1;
                var icols = _gameField.Cells.Max(x => x.Column) + 1;
                bool isWritten = (icols != 6) || (irows != 9); // пишем только если не 6на9 для экономии
                var level = (isWritten) ?
                    new XElement("level", new XAttribute("name", "n"), new XAttribute("number", "1"), new XAttribute("cols", icols), new XAttribute("rows", irows))
                    : new XElement("level", new XAttribute("name", "n"), new XAttribute("number", "1"));
                foreach (var cell in _gameField.Cells)
                {
                    var cellElement = new XElement("cell",
                        new XAttribute("row", CalcRow(cell.Row, lap, irows)),
                        new XAttribute("column", CalcColumn(cell.Column, lap, icols)),
                        new XAttribute("type", cell.Type),
                        new XAttribute("object", cell.GameObject == null ? "" : cell.GameObject.Type.ToStringExt()),
                        new XAttribute("direction", cell.GameObject == null ? "" : CalcAngle(cell.GameObject, lap).ToString()));
                    if (cell.GameObject != null && cell.GameObject.IsMoved)
                    {
                        var winCell = _positions[cell.GameObject].Win.Value;
                        cellElement.Add(
                            new XAttribute("winRow", CalcRow(winCell.Y, lap, irows)),
                            new XAttribute("winColumn", CalcColumn(winCell.X, lap, icols)),
                            new XAttribute("id", cell.GameObject.Id));
                    }
                    if (cell.GameObject != null && cell.GameObject.IsController && (cell.GameObject.ButtonIndex > 0)) // кнопка 
                    {
                        cellElement.Add(new XAttribute("buttonIndex", cell.GameObject.ButtonIndex));
                    }
                    if (cell.GameObject != null)
                        if (!cell.GameObject.IsController && (cell.GameObject.ButtonIndex > 0) &&
                            !string.IsNullOrEmpty(cell.GameObject.ButtonDirection) && cell.GameObject.IsSpined) // зависит от кнопки
                        {
                            cellElement.Add(new XAttribute("buttonIndex", cell.GameObject.ButtonIndex));
                            cellElement.Add(new XAttribute("buttonDirection", cell.GameObject.ButtonDirection));
                        }
                    level.Add(cellElement);
                }
                var sorted = level.Elements("cell").OrderBy(at => at.Attribute("row").Value).ToArray();
                level.Elements().Remove();
                level.Add(sorted);
                doc.Add(level);

                using (var stream = File.Create(FileName))
                {
                    doc.Save(stream);
                }
                if(lap==0)
                    AppTitleName = FileName;
            }
        }

        private int CalcAngle(GameObject gameObject, int lap)
        {
            if (lap == 0) // нет поворота 
                return gameObject.Angle;
            if (lap == 1) // поворот только относительно горизонтали
            {
                if (gameObject.IsAngleObject)
                {
                    if(gameObject.Angle==0 || gameObject.Angle==180)
                        return ((gameObject.Angle - 90) % 360);
                    else
                        return ((gameObject.Angle + 90) % 360);
                }
                if (gameObject.IsAngle2Object && (gameObject.Angle == 0 || gameObject.Angle == 180))
                    return ((gameObject.Angle + 180) % 360);
                if (gameObject.IsAnyLaserObject && (gameObject.Angle == 90 || gameObject.Angle == 270))
                    return ((gameObject.Angle + 180) % 360);
            }
            if (lap == 2) // поворот только относительно вертикали
            {
                if (gameObject.IsAngleObject)
                {
                    if (gameObject.Angle == 90 || gameObject.Angle == 270)
                        return ((gameObject.Angle - 90) % 360);
                    else
                        return ((gameObject.Angle + 90) % 360);
                }
                if (gameObject.IsAngle2Object && (gameObject.Angle == 90 || gameObject.Angle == 270))
                    return ((gameObject.Angle + 180) % 360);
                if (gameObject.IsAnyLaserObject && (gameObject.Angle == 0 || gameObject.Angle == 180))
                    return ((gameObject.Angle + 180) % 360);
            }
            if (lap == 3) // поворот полный
            {
                if (gameObject.IsAngleObject)
                {
                    /*if (gameObject.Angle == 90 || gameObject.Angle == 270)
                        return ((gameObject.Angle - 90) % 360);
                    else
                        return ((gameObject.Angle + 90) % 360);*/
                    return ((gameObject.Angle + 180) % 360);
                }
                if (gameObject.IsAngle2Object /*&& (gameObject.Angle == 90 || gameObject.Angle == 270)*/)
                    return ((gameObject.Angle + 180) % 360);
                if (gameObject.IsAnyLaserObject /*&& (gameObject.Angle == 0 || gameObject.Angle == 180)*/)
                    return ((gameObject.Angle + 180) % 360);
            }
            return gameObject.Angle;
        }

        private int CalcColumn(int c, int lap, int countCols)
        {
            if ((lap == 0) || (lap == 1)) // нет поворота или поворот только относительно горизонтали
                return c;
            if ((lap == 2) || (lap == 3)) // поворот относительно вертикали и полный поворот
                return countCols - c - 1;
            return c;
        }

        private int CalcRow(int r, int lap, int countRows)
        {
            if ((lap == 0) || (lap == 2)) // нет поворота или поворот только относительно вертикали
                return r;
            if ((lap == 1) || (lap==3)) // поворот относительно горизонтали и полный поворот
                return countRows - r - 1;
            return r;
        }

        private void LoadLevel()
        {
            IsEditMode = true;
			_positions = new Dictionary<GameObject, FullPosition>();
            _gameField.Clear();
            var dialog = new OpenFileDialog { Multiselect = false, Title = "Файл конфигурации уровня" };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            try
            {
                var document = XDocument.Load(dialog.FileName);
                var level = document.Element("level");

				int rows = level.Elements("cell").Select(c => int.Parse(c.Attribute("row").Value)).Max() + 1;
				int cols = level.Elements("cell").Select(c => int.Parse(c.Attribute("column").Value)).Max() + 1;
	            Rows = rows.ToString();
	            Columns = cols.ToString();
	            Cells = FieldInitializer.InitGameField(cols, rows);

                foreach (var e in level.Elements("cell"))
                {
                    int row = int.Parse(e.Attribute("row").Value);
                    int column = int.Parse(e.Attribute("column").Value);
	                int winRow = e.Attribute("winRow") != null ? int.Parse(e.Attribute("winRow").Value) : row;
	                int winColumn = e.Attribute("winColumn") != null ? int.Parse(e.Attribute("winColumn").Value) : column;
                    CellType type = (CellType)Enum.Parse(typeof(CellType), e.Attribute("type").Value);

                    var cell = _gameField.Cells.FirstOrDefault(x => x.Row == winRow && x.Column == winColumn);
                    if (cell == null)
                        throw new ApplicationException(string.Format("Не найдена ячейка с координатами {0} {1}", row, column));
                    GameObject gameObject = null;
                    if (!string.IsNullOrEmpty(e.Attribute("object").Value))
                    {
                        ObjectType objType;
                        if(e.Attribute("object").Value == "Button")
                        {
                            if (!string.IsNullOrEmpty(e.Attribute("buttonIndex").Value))
                            {
                                switch(e.Attribute("buttonIndex").Value)
                                {
                                    case ("1"):
                                        objType = ObjectType.StacionarControllerRed;
                                        break;
                                    case ("2"):
                                        objType = ObjectType.StacionarControllerGreen;
                                        break;
                                    case ("3"):
                                        objType = ObjectType.StacionarControllerBlue;
                                        break;
                                    default:
                                        objType = ObjectType.StacionarControllerGreen;
                                        break;
                                }
                            }
                            else
                                throw new ApplicationException(string.Format("У кнопки нет индекса"));

                        }
                        else
                            objType = (ObjectType)Enum.Parse(typeof(ObjectType), e.Attribute("object").Value);
                        int angle = int.Parse(e.Attribute("direction").Value);
                        gameObject = new GameObject(objType, angle, cell.Lines);
                    }

	                if (gameObject != null && gameObject.IsMoved)
	                {
		                _positions[gameObject] = new FullPosition() { Start = new Point(column, row), Win = new Point(winColumn, winRow)};
	                }
                    // is spinned object
                    if (gameObject != null)
                    if(!gameObject.IsController) // не кнопка
                    if (e.Attribute("buttonIndex") != null && !string.IsNullOrEmpty(e.Attribute("buttonIndex").Value)) 
                    {
                        if (e.Attribute("buttonDirection") == null || string.IsNullOrEmpty(e.Attribute("buttonDirection").Value))
                            throw new ApplicationException(string.Format("У поворачиваемого объекта нет направления, но есть индекс кнопки"));
                        switch (e.Attribute("buttonIndex").Value)
                        {
                            case ("1"):
                                gameObject.ConnectedControllerType = ControllerType.Red;
                                break;
                            case ("2"):
                                gameObject.ConnectedControllerType = ControllerType.Green;
                                break;
                            case ("3"):
                                gameObject.ConnectedControllerType = ControllerType.Blue;
                                break;
                            default:
                                gameObject.ConnectedControllerType = ControllerType.Green;
                                break;
                        }
                        gameObject.ButtonDirection = e.Attribute("buttonDirection").Value;
                    }
                    //
                    cell.Type = type;
                    if (gameObject != null)
                        cell.GameObject = gameObject;
                }

	            var z = _gameField.Cells.Where(x => x.GameObject != null && x.GameObject.IsMoved).ToArray();

	            //IsWinCombination = true;
				_isWinComb = true;
				RaisePropertyChanged("IsWinCombination");
                RaisePropertyChanged("IsStartCombination");
                //ShowWinCombination();
                _gameField.InitLazers();
                AppTitleName = dialog.FileName;
            }
            catch (Exception ex)
            {
                _gameField.Clear();
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowWinCombination()
        {
			Properties.EnableAll();
			SaveCurrent(false);
			LoadCombination(true);
			AppCommon.Freeze = false;
        }

        private void ShowStartCombination()
		{
			Properties.DisableAll();
			SaveCurrent(true);
            LoadCombination(false);
	        AppCommon.Freeze = true;
		}

        private void SaveCurrent(bool win)
        {
	        var objects = _gameField.Cells.Where(c => c.GameObject != null && c.GameObject.IsMoved).Select(x => x.GameObject).ToArray();
	        if (_positions.Keys.Count == objects.Length && _positions.Keys.All(objects.Contains) // проверяем не менялись ли объекты
                && !_gameField.ChangedAfterShake) // ничего не менялось после shake
	        {
		        foreach (var cell in _gameField.Cells.Where(c => c.GameObject != null && c.GameObject.IsMoved))
		        {
			        FullPosition pos = _positions[cell.GameObject];
			        if (win)
						pos.Win = new Point(cell.Column, cell.Row);
					else 
						pos.Start = new Point(cell.Column, cell.Row);
		        }
	        }
	        else
	        {
	            SetStartToWin();
	        }
        }

        // вызывается для сброса стартовой
        private void SetStartToWin()
        {
            _positions = new Dictionary<GameObject, FullPosition>();
            foreach (var cell in _gameField.Cells.Where(c => c.GameObject != null && c.GameObject.IsMoved))
            {
                FullPosition pos = new FullPosition {Win = new Point(cell.Column, cell.Row), Start = new Point(cell.Column, cell.Row)};
                _positions.Add(cell.GameObject, pos);
            }
        }

        private void LoadCombination(bool win)
        {
            foreach (Cell cell in _gameField.Cells.Where(x => x.GameObject != null && x.GameObject.IsMoved))
            {
                cell.GameObject = null;
            }
            foreach (var kv in _positions)
            {
                var gameObj = kv.Key;
                var point = win ? kv.Value.Win.Value : kv.Value.Start.Value;
                var targetCell = _gameField.Cells.First(x => x.Row == point.Y && x.Column == point.X);
                targetCell.GameObject = null;
                targetCell.GameObject = gameObj;
                targetCell.GameObject.Lines = targetCell.Lines;
            }
            _gameField.InitLazers();
        }

        #endregion

        private void TurnH()
        {
            TurnObjects();
        }
        
        private void TurnV()
        { 
        }

        private void TurnObjects()
        {
            var newCells = new List<Cell>();
            var irows = int.Parse(Rows);
            var axis = irows / 2; // ось деления пополам
            foreach (Cell cell in _gameField.Cells)
            {
//                if(axis > cell.Row)
//                {
                    Cell newcell = new Cell();
                    newcell = cell.GetClone();
                    newcell.Row = irows - cell.Row - 1;
                    //var oldca = _gameField.Cells.Where(c => c.Row == cell.Row && c.Column == cell.Column);
                    Cell oldc = _gameField.Cells.Where(c => c.Row == newcell.Row && c.Column == newcell.Column).First();
                    newcell.CanvasX = oldc.CanvasX;
                    newcell.CanvasY = oldc.CanvasY;
//                }
/*                else
                    if (axis < cell.Row)
                    {
                        cell.Row -= axis;
                    }*/
                cell.Dispose();
                newCells.Add(newcell);
            }
            Cells = newCells.ToArray();
        }
    }

}