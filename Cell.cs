using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LevelConstructor.ViewModels;

namespace LevelConstructor
{
    public delegate void CellClick(Cell cell, MouseButtonEventArgs e);

    public class Cell : IDisposable
    {
        public static readonly Brush DefaultCellBrush = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/empty_cell.png")));
        public static readonly Brush UnavailableCellBrush = Brushes.Transparent;

        #region fields
        private readonly Rectangle _cellRect, _objRect, _shadowRect;
        private bool _selected;
        private CellType _type;
        private GameObject _gameObject;
        private bool _mouseDown;
        #endregion
        
        #region properties
        public bool Visibile
        {
            set
            {
                _cellRect.Visibility = value ? Visibility.Visible : Visibility.Collapsed;_cellRect.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                _objRect.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public int Row { get; set; }

        public int Column { get; set; }

        public bool Selected 
        {
            get { return _selected; }  
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    if (!value)
                    {
                        _objRect.Stroke = DefaultCellBrush;
                        _objRect.StrokeThickness = 0;
                    }
                    else
                    {
                        _objRect.Stroke = Brushes.DodgerBlue;
                        _objRect.StrokeThickness = 0.3;
                    }
                }
            }
        }

        public CellType Type
        {
            get { return _type; }
            set
            {
                if(_type == value)
                    return;
                _type = value;
                switch (value)
                {
                    case CellType.Default:
                        _cellRect.Fill = DefaultCellBrush;
                        break;
                    case CellType.Unavailable:
                        GameObject = null;
                        _cellRect.Fill = UnavailableCellBrush;
                        foreach (var line in Lines)
                        {
                            line.Visibility = Visibility.Collapsed;
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public GameObject GameObject
        {
            get { return _gameObject; }
            set
            {
                //if(_gameObject == value)
                 //   return;
                _gameObject = value;
                if (value == null)
                {
                    switch (Type)
                    {
                        case CellType.Default:
                            _cellRect.Fill = DefaultCellBrush;
                            break;
                        case CellType.Unavailable:
                            _gameObject = null;
                            _cellRect.Fill = UnavailableCellBrush;
                            foreach (var line in Lines)
                            {
                                line.Visibility = Visibility.Collapsed;
                            }
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    _shadowRect.Visibility = Visibility.Collapsed;
                    _objRect.Fill = Brushes.Transparent;
                }
                else
                {
                    _shadowRect.Visibility = Visibility.Visible;
                    _objRect.Fill = _gameObject.Brush;
                    _objRect.RenderTransform = new RotateTransform(_gameObject.Angle);
                }
            }
        }

        public double CanvasX { get; set; }

        public double CanvasY { get; set; }

        public Canvas Canvas { get; private set; }

        public Line[] Lines { get; set; }
        #endregion

        public event CellClick Click;

        public event ExitEventHandler OnDropEvent;

        public Cell(Rectangle cellRect, Rectangle objRect, Rectangle shadowRect, int row, int column, ContextMenu menu, Canvas canvas)
        {
            Canvas = canvas;
	        CanvasX = (double) cellRect.GetValue(Canvas.LeftProperty) + cellRect.Width / 2;// + 5;
	        CanvasY = (double) cellRect.GetValue(Canvas.TopProperty) + cellRect.Width / 2;// + 5;

            _shadowRect = shadowRect;
            _cellRect = cellRect;
            _objRect = objRect;
            Row = row;
            Column = column;

            _shadowRect.Visibility = Visibility.Collapsed;
            _shadowRect.Fill = BuilderResources.DefaultShadow;

            _cellRect.Fill = DefaultCellBrush;

            _objRect.Fill = Brushes.Transparent;
            _objRect.MouseDown += OnCellClick;
            _objRect.ContextMenu = menu;
            _objRect.RenderTransformOrigin = new Point(0.5, 0.5);
            _objRect.MouseLeftButtonDown += OnMouseDown;
            _objRect.MouseLeftButtonUp += OnMouseUp;
            _objRect.MouseLeave += OnMouseLeave;
            _objRect.Drop += OnDrop;
            _objRect.AllowDrop = true;

            Canvas.SetZIndex(_objRect, 3);
            Canvas.SetZIndex(_cellRect, 2);
            Canvas.SetZIndex(_shadowRect, 1);

            Lines = new Line[4];
            for (int i = 0; i < 4; i++)
            {
                Line line = new Line {X1 = CanvasX, Y1 = CanvasY, Stroke = Brushes.Red, StrokeThickness = 1, Opacity = 0.5};
                line.IsHitTestVisible = false;
                line.Visibility = Visibility.Collapsed;
                canvas.Children.Add(line);
                Canvas.SetZIndex(line, 4);
                Lines[i] = line;
            }
        }

        public Cell()
        {
            // TODO: Complete member initialization
        }

        public void Dispose()
        {
            OnDropEvent = null;
            Click = null;
        }

        private void OnCellClick(object sender, MouseButtonEventArgs e)
        {
            var handler = Click;
            if (handler != null)
                handler(this, e);
        }

        #region drag

        private void OnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            var cell = e.Data.GetData(GetType()) as Cell;

            if(cell.GameObject == null)
                return;

            if(GameObject != null)
                return;

            if (Type == CellType.Unavailable)// && cell.GameObject.IsMoved == false)
            {
                return;
                GameObject = GameObject;// new GameObject(cell.GameObject.Type, cell.GameObject.Angle, Lines);
                this.GameObject.Lines = Lines;
                cell.GameObject = null;
                if (cell.Selected)
                    cell.OnCellClick(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left));
            }
            else if (Type == CellType.Default)
            {
                GameObject = cell.GameObject;// new GameObject(cell.GameObject.Type, cell.GameObject.Angle, Lines);
                GameObject.Lines = Lines;
                cell.GameObject = null;
                if (cell.Selected)
                    cell.OnCellClick(this, new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left));
            }
			if(!Selected)
				OnCellClick(this, null);

            foreach (var line in cell.Lines)
            {
                line.Visibility = Visibility.Collapsed;
                
            }

            if(OnDropEvent != null)
                OnDropEvent(this, null);
        }

        private void OnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_mouseDown && GameObject != null)
            {
                if(!GameObject.IsMoved && AppCommon.Freeze)
                    return;
                var data = new DataObject(GetType(), this);
                DragDrop.DoDragDrop(_objRect, data, DragDropEffects.Move);
            }
            _mouseDown = false;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _mouseDown = false;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _mouseDown = true;
        }
        #endregion

        internal void UpdateBrush(GameObject go)
        {
            _objRect.Fill = go.Brush;
        }

        internal Cell GetClone()
        {
            return MemberwiseClone() as Cell;
        }
    }

}