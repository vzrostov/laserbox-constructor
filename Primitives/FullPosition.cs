using Point = System.Drawing.Point;

namespace LevelConstructor.ViewModels
{
	/// <summary>
	/// Full game object position description
	/// </summary>
    public class FullPosition
	{
		public Point? Start;
		public Point? Win;
		public int AngleStart;
		public int AngleWin;
	}

}