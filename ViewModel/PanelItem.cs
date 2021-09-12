using System.Windows.Input;
using System.Windows.Media;

namespace LevelConstructor.ViewModels
{
	public class PanelItem : ViewModelBase
	{
		public string Name { get; private set; }

		public ImageSource Image { get; private set; }

		public System.Windows.Controls.Image Icon { get; private set; }

		private bool _enabled;
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (value != _enabled)
				{
					_enabled = value;
					RaisePropertyChanged("Enabled");
				}
			}
		}

		public ICommand Command { get; private set; }

		public PanelItem(string name, ImageSource image, ICommand command)
		{
			Name = name;
			Image = image;
			Command = command;
			if(image != null)
				Icon = new System.Windows.Controls.Image() { Source = image};
		}
	}
}