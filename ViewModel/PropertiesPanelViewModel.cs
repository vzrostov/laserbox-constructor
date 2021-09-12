using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LevelConstructor.ViewModels
{
    public class PropertiesViewModel : ViewModelBase
    {
		private GameField _gameField;
	    private PanelItem _toogleController; 

		#region Properties

		public ObservableCollection<ObjectItem> MovedObjects { get; private set; } 

        public ObservableCollection<ObjectItem> StacionarObjects { get; private set; }

        public ObservableCollection<ObjectItem> Receivers { get; private set; } 

        public ObservableCollection<ObjectItem> ObjectsItems { get; private set; }

        public ObservableCollection<PanelItem> ContextMenuItems { get; private set; } 

        public ObservableCollection<AreaItem> AreaItems { get; private set; }

		public RotationItem RotateLeft { get; private set; }

		public RotationItem RotateRight { get; private set; }

        public ICommand SelectAllCommand { get; private set; }

	    private Visibility _controllersSetupVisibility;
	    public Visibility ControllersSetupVisibility
	    {
		    get { return _controllersSetupVisibility; }
		    set
		    {
			    if (_controllersSetupVisibility != value)
			    {
				    _controllersSetupVisibility = value;
					RaisePropertyChanged("ControllersSetupVisibility");
			    }
		    }
	    }

	    public bool RedControllerEnabled
	    {
            get
            {
                return _gameField.SelectedCells.Where(c => c.GameObject != null).Select(c => c.GameObject).
                    All(g => g.ConnectedControllerType == ControllerType.Red);
            }
            set
		    {
                if (value)
                    BlueControllerEnabled = GreenControllerEnabled = false;
                foreach (var gameObject in _gameField.SelectedCells.Where(c => c.GameObject != null).Select(c => c.GameObject))
                {
                    if (value)
                    {
                        gameObject.ConnectedControllerType = ControllerType.Red;
                        _gameField.SelectedCells.ForEach(cell => cell.UpdateBrush(gameObject));
                    }
                    else
                        if (gameObject.ConnectedControllerType == ControllerType.Red)
                        {
                            gameObject.ConnectedControllerType = null;
                            _gameField.SelectedCells.ForEach(cell => cell.UpdateBrush(gameObject));
                        }
                }
                RaisePropertyChanged("RedControllerEnabled");
                _gameField.SetButton();
		    }
	    }

        public bool GreenControllerEnabled
        {
            get
            {
                return _gameField.SelectedCells.Where(c => c.GameObject != null).Select(c => c.GameObject).
                    All(g => g.ConnectedControllerType == ControllerType.Green);
            }
            set
            {
                if (value)
                    BlueControllerEnabled = RedControllerEnabled = false;
                foreach (var gameObject in _gameField.SelectedCells.Where(c => c.GameObject != null).Select(c => c.GameObject))
                {
                    if (value)
                    {
                        gameObject.ConnectedControllerType = ControllerType.Green;
                        _gameField.SelectedCells.ForEach(cell => cell.UpdateBrush(gameObject));
                    }
                    else
                        if (gameObject.ConnectedControllerType == ControllerType.Green)
                        {
                            gameObject.ConnectedControllerType = null;
                            _gameField.SelectedCells.ForEach(cell => cell.UpdateBrush(gameObject));
                        }
                }
                RaisePropertyChanged("GreenControllerEnabled");
                _gameField.SetButton();
            }
        }

        public bool BlueControllerEnabled
        {
            get 
            {
                return _gameField.SelectedCells.Where(c => c.GameObject != null).Select(c => c.GameObject).
                    All(g => g.ConnectedControllerType == ControllerType.Blue); 
            }
            set
            {
                if(value)
                    GreenControllerEnabled = RedControllerEnabled = false;
                foreach (var gameObject in _gameField.SelectedCells.Where(c => c.GameObject != null).Select(c => c.GameObject))
                {
                    if (value)
                    {
                        gameObject.ConnectedControllerType = ControllerType.Blue;
                        _gameField.SelectedCells.ForEach(cell => cell.UpdateBrush(gameObject));
                    }
                    else
                        if (gameObject.ConnectedControllerType == ControllerType.Blue)
                        {
                            gameObject.ConnectedControllerType = null;
                            _gameField.SelectedCells.ForEach(cell => cell.UpdateBrush(gameObject));
                        }
                }
                RaisePropertyChanged("BlueControllerEnabled");
            }
        }

		public bool ACWControllerEnabled
		{
            get
            {
                return _gameField.SelectedCells.Where(c => c.GameObject != null).Select(c => c.GameObject).
                    All(g => g.ButtonDirection == "-");
            }
            set
            {
                if (value)
                    CWControllerEnabled = false;
                ControllerType? selct = // смотрим цвет выделенного
                    _gameField.SelectedCells.Where(c => c.GameObject != null)
                        .Select(c => c.GameObject)
                        .First()
                        .ConnectedControllerType;
                if(selct!=null)
                foreach (var gameObject in _gameField.Cells.Where(c => c.GameObject != null).Select(c => c.GameObject))
                {
                    if (selct == gameObject.ConnectedControllerType) // для всех = цвету выделенного
                    {
                        if (value)
                            gameObject.ButtonDirection = "-";
                        else if (gameObject.ButtonDirection == "-") gameObject.ButtonDirection = "";
                    }
                }
                RaisePropertyChanged("ACWControllerEnabled");
            }
        }

		public bool CWControllerEnabled
		{
            get
            {
                return _gameField.SelectedCells.Where(c => c.GameObject != null).Select(c => c.GameObject).
                    All(g => g.ButtonDirection == "+");
            }
            set
            {
                if (value)
                    ACWControllerEnabled = false;
                ControllerType? selct = // смотрим цвет выделенного
                    _gameField.SelectedCells.Where(c => c.GameObject != null)
                        .Select(c => c.GameObject)
                        .First()
                        .ConnectedControllerType;
                if(selct!=null)
                foreach (var gameObject in _gameField.Cells.Where(c => c.GameObject != null).Select(c => c.GameObject))
                {
                    if (selct == gameObject.ConnectedControllerType) // для всех = цвету выделенного
                    {
                        if (value)
                            gameObject.ButtonDirection = "+";
                        else if (gameObject.ButtonDirection == "+") gameObject.ButtonDirection = "";
                    }
                }
                RaisePropertyChanged("CWControllerEnabled");
            }
        }

	    #endregion

        public PropertiesViewModel()
        {
	        var objectCommand = new RelayCommand(o => AddGameObject((o as ObjectItem).Type), x => true);

            ObjectsItems = new ObservableCollection<ObjectItem>
            {
                new ObjectItem("Неподвжный отражатель (4)", objectCommand, ObjectType.StacionarReflectorAll),
                new ObjectItem("Неподвижный отражатель", objectCommand, ObjectType.StacionarReflectorAngle),
                new ObjectItem("Неподвижный отражатель (3)", objectCommand, ObjectType.StacionarReflectorTriangle),
                new ObjectItem("Неподвижный отражатель (tube)", objectCommand, ObjectType.StacionarTube),
                new ObjectItem("Стационарный телепорт", objectCommand, ObjectType.StacionarTeleport),
                new ObjectItem("Страционарное зеркало", objectCommand, ObjectType.StacionarMirror),
                
                new ObjectItem("Подвижный отражатель", objectCommand, ObjectType.MovedReflectorAngle),
                new ObjectItem("Подвижный отражатель (3)", objectCommand, ObjectType.MovedReflectorTriangle),
                new ObjectItem("Подвижный отражатель (4)", objectCommand, ObjectType.MovedReflectorAll),
                new ObjectItem("Подвижный отражатель (tube)", objectCommand, ObjectType.MovedReflectorTube),
                new ObjectItem("Подвижный телепорт", objectCommand, ObjectType.MovedTeleport),
                new ObjectItem("Подвижное препядствие", objectCommand, ObjectType.MovedNone),

                new ObjectItem("Барьер", objectCommand, ObjectType.Barier),
                new ObjectItem("Излучатель", objectCommand, ObjectType.LaserSource),
                new ObjectItem("Подвижный Излучатель", objectCommand, ObjectType.MovedLaserSource),
                new ObjectItem("Излучатель X", objectCommand, ObjectType.LaserXSource),
                new ObjectItem("Подвижный Излучатель X", objectCommand, ObjectType.MovedLaserXSource),
				new ObjectItem("Receiver (0)", objectCommand, ObjectType.LaserReceiver_0),
                new ObjectItem("Receiver (1)", objectCommand, ObjectType.LaserReceiver_1),
                new ObjectItem("Receiver (2)", objectCommand, ObjectType.LaserReceiver_2),
                new ObjectItem("Receiver (3)", objectCommand, ObjectType.LaserReceiver_3),
                new ObjectItem("Receiver (4)", objectCommand, ObjectType.LaserReceiver_4),
				
                new ObjectItem("Кнопка Зеленая", objectCommand, ObjectType.StacionarControllerGreen),
                new ObjectItem("Кнопка Красная", objectCommand, ObjectType.StacionarControllerRed),
                new ObjectItem("Кнопка Синяя", objectCommand, ObjectType.StacionarControllerBlue)
            };

            var receivers = new[]
            {
				ObjectType.LaserReceiver_0,
                ObjectType.LaserReceiver_1, ObjectType.LaserReceiver_2, ObjectType.LaserReceiver_3,
                ObjectType.LaserReceiver_4
            };
            MovedObjects = new ObservableCollection<ObjectItem>(ObjectsItems.Where(x => GameObject.CanMove(x.Type) && !receivers.Contains(x.Type)));
            StacionarObjects = new ObservableCollection<ObjectItem>(ObjectsItems.Where(x => !GameObject.CanMove(x.Type) && !receivers.Contains(x.Type)));
            Receivers = new ObservableCollection<ObjectItem>(ObjectsItems.Where(x => receivers.Contains(x.Type)));

            var cellTypeCommand = new RelayCommand(x => SetAreaType((x as AreaItem).AreaType) , x => true);
            AreaItems = new ObservableCollection<AreaItem>
            {
                new AreaItem("Доступная", new BitmapImage(new Uri("pack://application:,,,/Resources/available.png")), cellTypeCommand, CellType.Default) {Enabled = true},
                new AreaItem("Не доступная", new BitmapImage(new Uri("pack://application:,,,/Resources/notavailable.png")), cellTypeCommand, CellType.Unavailable) { Enabled = true}
            };

            var removeCommand = new RelayCommand(x => RemoveGameObjects());
            PanelItem removeItem = new PanelItem("Удалить", null, removeCommand) {Enabled = true};

	        var toogleControllerCommand = new RelayCommand(x => ToogleController());
			_toogleController = new PanelItem("Controller On/Off", null, toogleControllerCommand);
            ContextMenuItems = new ObservableCollection<PanelItem>(ObjectsItems)
            {
                removeItem,
				_toogleController
            };

            SelectAllCommand = new RelayCommand(x => _gameField.SelectAll(), x => true);
            RotateLeft = new RotationItem("Влево", null, new RelayCommand(x => RotateItem(true)), true);
            RotateRight = new RotationItem("Вправо", null, new RelayCommand(x => RotateItem(false)), false);

        }

	    public void SetGameField(GameField gameField)
	    {
		    if (gameField == null) throw new ArgumentNullException("gameField");
			if (gameField == _gameField)
				return;
		    _gameField = gameField;
			_gameField.SelectionChanged += GameFieldOnSelectionChanged;
	    }

	    private void GameFieldOnSelectionChanged(object sender, SelectionChangedEventArgs e)
	    {
			InitControllerSetup(e.SelectedCells);
			_toogleController.Enabled = false;

			if (e.SelectedCells.Count == 0 || e.SelectedCells.Count > 1)
			{
				DisableAll();
				return;
			}

			if (e.SelectedCells.Count == 1)
			{
				var cell = e.SelectedCells.First();
				if (cell.Type == CellType.Default)
					EnableAll();
				else
				{
					DisableAll();
					EnableStacionar();
					EnableItem(ObjectType.LaserReceiver_0, true);
					EnableItem(ObjectType.LaserReceiver_1, true);
					EnableItem(ObjectType.LaserReceiver_2, true);
					EnableItem(ObjectType.LaserReceiver_3, true);
					EnableItem(ObjectType.LaserReceiver_4, true);
				}
				EnableItem(ObjectType.StacionarControllerRed,  !_gameField.HasObject(ObjectType.StacionarControllerRed));
				EnableItem(ObjectType.StacionarControllerBlue, !_gameField.HasObject(ObjectType.StacionarControllerBlue));
                EnableItem(ObjectType.StacionarControllerGreen, !_gameField.HasObject(ObjectType.StacionarControllerGreen));
				if (cell.GameObject != null && cell.GameObject.IsController)
					_toogleController.Enabled = true;
			}

	    }

	    public void EnableStacionar()
        {
            foreach (var stacionarObject in StacionarObjects)
            {
                stacionarObject.Enabled = !AppCommon.Freeze;
            }
        }

        public void EnableAll()
        {
            foreach (var objectsItem in ObjectsItems)
            {
                objectsItem.Enabled = !AppCommon.Freeze;
            }
        }

        public void DisableAll()
        {
            foreach (var objectsItem in ObjectsItems)
            {
                objectsItem.Enabled = false;
            }
        }

        public void EnableItem(ObjectType type, bool enabled)
        {
            var item = ObjectsItems.First(x => x.Type == type);
            if (AppCommon.Freeze)
                item.Enabled = false;
            else
                item.Enabled = enabled;
        }

        private void AddGameObject(ObjectType type)
        {
            if (_gameField != null)
            {
                _gameField.SetObject(type, 0);
            }
        }

		private void RemoveGameObjects()
		{
			if (AppCommon.Freeze)
				return;
			if (_gameField == null)
				return;
			_gameField.RemoveSelected();
		}

        private void SetAreaType(CellType type)
        {
            if(AppCommon.Freeze)
                return;
            if(_gameField == null)
                return;
            _gameField.SetAreaType(type);
        }

        private void RotateItem(bool byHour)
        {
            if(AppCommon.Freeze)
                return;
            if(_gameField == null)
                return;
            _gameField.Rotate(byHour);
        }

	    private void ToogleController()
	    {
		    _gameField.PushController(_gameField.SelectedCells[0].GameObject);
	    }

	    private void InitControllerSetup(List<Cell> selectedCells)
	    {
            if (selectedCells.Count != 1 || selectedCells.All(c => c.GameObject == null || !c.GameObject.IsSpined))
		    {
			    ControllersSetupVisibility = Visibility.Collapsed;
				return;
		    }

		    ControllersSetupVisibility = Visibility.Visible;

		    GameObject gameObject = selectedCells[0].GameObject;
            RedControllerEnabled = gameObject.ConnectedControllerType == ControllerType.Red;
            BlueControllerEnabled = gameObject.ConnectedControllerType == ControllerType.Blue;
            GreenControllerEnabled = gameObject.ConnectedControllerType == ControllerType.Green;
            ACWControllerEnabled = gameObject.ButtonDirection == "-";
            CWControllerEnabled = gameObject.ButtonDirection == "+";
	    }
    }
}
