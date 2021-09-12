using System.Windows.Input;

namespace LevelConstructor.ViewModels
{
	/// <summary>
	/// Any game objects placed in right panel to add on the field
	/// </summary>
	public class ObjectItem : PanelItem
	{
		public ObjectType Type { get; private set; }

		public ObjectItem(string name, ICommand command, ObjectType type)
			: base(name, BuilderResources.GetObjectImage(type), command)
		{
			Type = type;
		}

	}
}