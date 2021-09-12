using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LevelConstructor
{
    public class GameObject
	{
		#region static

        private static int s_id = 1;

        private static readonly ObjectType[] LaserObjects = { 
                                                                ObjectType.LaserSource, ObjectType.MovedLaserSource,
                                                            };

        private static readonly ObjectType[] LaserXObjects = { 
                                                                ObjectType.LaserXSource, ObjectType.MovedLaserXSource,
                                                            };

		private static readonly ObjectType[] MovedObjects = { 
                                                                ObjectType.MovedReflectorAngle, ObjectType.MovedReflectorAll, 
                                                                ObjectType.MovedTeleport, ObjectType.MovedReflectorTriangle, 
                                                                ObjectType.MovedReflectorTube, ObjectType.MovedNone,
                                                                ObjectType.MovedLaserSource, ObjectType.MovedLaserXSource,
                                                            };

		private static readonly ObjectType[] SpinedObjects = { 
                                                                ObjectType.MovedReflectorAngle, 
                                                                ObjectType.MovedReflectorTriangle, 
                                                                ObjectType.MovedReflectorTube,
                                                                ObjectType.StacionarReflectorAngle, ObjectType.StacionarReflectorTriangle,
                                                                ObjectType.StacionarMirror, ObjectType.StacionarTube,
                                                                ObjectType.MovedLaserSource, ObjectType.MovedLaserXSource,
                                                                ObjectType.LaserSource, ObjectType.LaserXSource
                                                            };

        private static readonly ObjectType[] AngleObjects = { 
                                                                ObjectType.MovedReflectorAngle, 
                                                                ObjectType.StacionarReflectorAngle,
                                                                ObjectType.StacionarMirror
                                                            };
        private static readonly ObjectType[] Angle2Objects = { 
                                                                ObjectType.MovedReflectorTriangle, 
                                                                ObjectType.StacionarReflectorTriangle
                                                            };
        
        private static readonly ObjectType[] Receivers =
	    {
		    ObjectType.LaserReceiver_0, ObjectType.LaserReceiver_1,
		    ObjectType.LaserReceiver_2, ObjectType.LaserReceiver_3, 
            ObjectType.LaserReceiver_4
	    };

		private static readonly ObjectType[] Teleports = { ObjectType.StacionarTeleport, ObjectType.MovedTeleport };

        private static readonly ObjectType[] Controllers = { ObjectType.StacionarControllerRed, ObjectType.StacionarControllerGreen, ObjectType.StacionarControllerBlue };

        private static readonly ObjectType[] HardObjects = { ObjectType.StacionarControllerRed, ObjectType.StacionarControllerGreen, 
                                                               ObjectType.StacionarControllerBlue, ObjectType.MovedLaserSource,
                                                               ObjectType.LaserXSource, ObjectType.MovedLaserXSource };

        internal static bool CanMove(ObjectType objectType)
        {
            return MovedObjects.Contains(objectType);
        }

		#endregion

        public int Id { get; private set; }

		public bool IsActive { get; set; }

        public bool IsAngleObject { get { return AngleObjects.Contains(Type); } }
        public bool IsAngle2Object { get { return Angle2Objects.Contains(Type); } }

        public bool IsHardObject { get { return HardObjects.Contains(Type); } }

        public bool IsAnyLaserObject { get { return IsLaserObject || IsLaserXObject; } }
        public bool IsLaserObject { get { return LaserObjects.Contains(Type); } }
        public bool IsLaserXObject { get { return LaserXObjects.Contains(Type); } }

		public bool IsReceiver { get { return Receivers.Contains(Type); } }

        public bool IsMoved { get { return MovedObjects.Contains(Type); } }

        public bool IsSpined { get { return SpinedObjects.Contains(Type); } }

		public bool IsTeleport {  get { return Teleports.Contains(Type); }}

		public bool IsController { get { return Controllers.Contains(Type); } }

	    public ControllerType? ControllerType
	    {
		    get
		    {
			    if (!IsController)
				    return null;
			    switch (Type)
			    {
					case ObjectType.StacionarControllerBlue:
						return LevelConstructor.ControllerType.Blue;
                    case ObjectType.StacionarControllerGreen:
						return LevelConstructor.ControllerType.Green;
					case ObjectType.StacionarControllerRed:
						return LevelConstructor.ControllerType.Red;
					default:
					    throw new NotSupportedException();
			    }
		    }
	    }

        private ControllerType? _connectedControllerType;
        /// <summary>
        /// у вращаемого элемента - к какой кнопке привязан
        /// </summary>
        public ControllerType? ConnectedControllerType
        {
            get
            {
                if (!IsSpined)
                    return null;
                return _connectedControllerType;
            }
            set { _connectedControllerType = value; }
        }

        public int GetButtonIndex(ControllerType? ct)
        {
            if (ct != null)
                return (int) ct.GetValueOrDefault();
            return 0;
        }

        public ObjectType Type { get; private set; }

		public int IncomingLasers { get; set; }

        private int _angle;
        public int Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                InitPorts(Lines);
            }
        }

        public ObjectType? ObjectTybeByController(ControllerType? Type)
        {
            switch (Type)
            {
                case (LevelConstructor.ControllerType.Red):
                    return ObjectType.StacionarControllerRed;
                case (LevelConstructor.ControllerType.Blue):
                    return ObjectType.StacionarControllerBlue;
                case (LevelConstructor.ControllerType.Green):
                    return ObjectType.StacionarControllerGreen;
                default:
                    return null;
            }
        }

        public Brush Brush
        {
            get
            {
                return GetObjectRealBrush();
            }
        }

        private Brush GetObjectRealBrush()
        {
            if (ObjectTybeByController(ConnectedControllerType) != null)
            {
                return BuilderResources.BrushFrom2Sources(Type, ObjectTybeByController(ConnectedControllerType) ?? default(ObjectType) );
            }
            return BuilderResources.GetObjectBrush(Type);
        }

        public List<LazerPort> Ports { get; private set; }

        private Line[] _lines;

        public Line[] Lines
        {
            get { return _lines; }
            set
            {
                if(_lines != null)
                    foreach (var line in _lines)
                    {
                        line.Visibility = Visibility.Collapsed;
                    }
                _lines = value;
                foreach (var line in _lines)
                {
                    line.Visibility = Visibility.Collapsed;
                }
                InitPorts(_lines);
            }
        }

        public GameObject(ObjectType type, int angle, Line[] lines)
        {
            Id = s_id++;
            Type = type;
            _angle = angle;
			Ports = new List<LazerPort>();
            Lines = lines;
        }

	    private void InitPorts(Line[] lines)
	    {
            Ports.Clear();
		    switch (Type)
		    {
				case ObjectType.LaserSource:
                case ObjectType.MovedLaserSource:
                case ObjectType.LaserXSource:
                case ObjectType.MovedLaserXSource:
                    Ports.Add(new LazerPort(RotateVector2d(-1, 0, Angle), LazerPort.State.Inactive, lines[0]));
					break;
				case ObjectType.MovedReflectorAngle:
				case ObjectType.StacionarReflectorAngle:
				case ObjectType.StacionarMirror:
					Ports.Add(new LazerPort(RotateVector2d(1, 0, Angle), LazerPort.State.Inactive, lines[0]));
					Ports.Add(new LazerPort(RotateVector2d(0, 1, Angle), LazerPort.State.Inactive, lines[1]));
					break;
                case ObjectType.MovedReflectorAll:
                case ObjectType.StacionarReflectorAll:
                    Ports.Add(new LazerPort(RotateVector2d(1, 0, Angle), LazerPort.State.Inactive, lines[0]));
					Ports.Add(new LazerPort(RotateVector2d(0, 1, Angle), LazerPort.State.Inactive, lines[1]));
                    Ports.Add(new LazerPort(RotateVector2d(-1, 0, Angle), LazerPort.State.Inactive, lines[2]));
					Ports.Add(new LazerPort(RotateVector2d(0, -1, Angle), LazerPort.State.Inactive, lines[3]));
                    break;
                case ObjectType.MovedReflectorTriangle:
                case ObjectType.StacionarReflectorTriangle:
                    Ports.Add(new LazerPort(RotateVector2d(1, 0, Angle), LazerPort.State.Inactive, lines[0]));
					Ports.Add(new LazerPort(RotateVector2d(0, 1, Angle), LazerPort.State.Inactive, lines[1]));
                    Ports.Add(new LazerPort(RotateVector2d(-1, 0, Angle), LazerPort.State.Inactive, lines[2]));
                    break;
                case ObjectType.MovedReflectorTube:
                case ObjectType.StacionarTube:
                    Ports.Add(new LazerPort(RotateVector2d(1, 0, Angle), LazerPort.State.Inactive, lines[0]));
                    Ports.Add(new LazerPort(RotateVector2d(-1, 0, Angle), LazerPort.State.Inactive, lines[2]));
		            break;
                case ObjectType.MovedTeleport:
                case ObjectType.StacionarTeleport:
                    Ports.Add(new LazerPort(RotateVector2d(1, 0, Angle), LazerPort.State.Inactive, lines[0]));
                    Ports.Add(new LazerPort(RotateVector2d(0, 1, Angle), LazerPort.State.Inactive, lines[1]));
                    Ports.Add(new LazerPort(RotateVector2d(-1, 0, Angle), LazerPort.State.Inactive, lines[2]));
                    Ports.Add(new LazerPort(RotateVector2d(0, -1, Angle), LazerPort.State.Inactive, lines[3]));
                    break;
		    }
	    }

		static Vector RotateVector2d(double x, double y, double angle)
		{
			angle = angle*(Math.PI/180);
			var vec = new Vector(x*Math.Cos(angle) - y*Math.Sin(angle), x*Math.Sin(angle) + y*Math.Cos(angle));
            return new Vector(Math.Abs(vec.X) == 1 ? vec.X : 0, Math.Abs(vec.Y) == 1 ? vec.Y : 0);
		}

        public int ButtonIndex 
        { 
            get 
            {
                if (IsController) 
                    return GetButtonIndex(ControllerType);
                return GetButtonIndex(ConnectedControllerType);
            } 
        }

        private string _buttonDirection = "";
        public string ButtonDirection 
        { 
            get { return _buttonDirection;  } 
            set { _buttonDirection = value; } 
        }
    }
}