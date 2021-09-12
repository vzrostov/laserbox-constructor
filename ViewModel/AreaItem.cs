using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LevelConstructor.ViewModels
{
	public class AreaItem : PanelItem
	{
		public CellType AreaType { get; private set; }

		public AreaItem(string name, BitmapImage image, ICommand command, CellType areaType) : base(name, image, command)
		{
			AreaType = areaType;
		}
	}
}