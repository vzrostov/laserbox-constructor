using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LevelConstructor.ViewModels
{
	public class RotationItem : PanelItem
	{
		public bool ByHour { get; private set; }

		public RotationItem(string name, BitmapImage image, ICommand command, bool byHour) : base(name, image, command)
		{
			ByHour = byHour;
		}
	}
}