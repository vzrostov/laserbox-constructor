using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LevelConstructor
{
    public class DropdownButton : Button
    {
        protected override void OnClick()
        {
            base.OnClick();
            if (ContextMenu != null)
            {
                ContextMenu.IsEnabled = true;
                ContextMenu.PlacementTarget = this;
                ContextMenu.Placement = PlacementMode.Bottom;
                ContextMenu.IsOpen = true;
            }
        }
    }
}
