using System;
using System.Collections.Generic;

namespace LevelConstructor
{
    public class SelectionChangedEventArgs : EventArgs
	{
		public List<Cell> Cells { get; set; }
		public List<Cell> SelectedCells { get; set; }
	}
}