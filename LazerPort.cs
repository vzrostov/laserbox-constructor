using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LevelConstructor
{
    public class LazerPort
	{
	    private Line _line;

		public enum State
		{
			Inactive,
			LaserInput,
			LaserSource
		}

		public Vector Direction { get; private set; }

		public State PortState { get; set; }

	    public void Off()
	    {
	        PortState = State.Inactive;
	        _line.Visibility = Visibility.Collapsed;
	    }

	    public void On(double x, double y)
	    {
            PortState = State.LaserSource;
	        _line.Visibility = Visibility.Visible;
	        _line.X2 = x;
	        _line.Y2 = y;
	    }

		public LazerPort(Vector direction, State portState, Line line)
		{
		    _line = line;
			Direction = direction;
			PortState = portState;
            Off();
		}

        internal void SetColor(Brush needbrush)
        {
            _line.Stroke = needbrush;
        }
    }
}