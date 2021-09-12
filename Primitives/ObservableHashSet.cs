using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LevelConstructor.ViewModels
{
	public class ObservableHashSet<T> : ObservableCollection<T>
	{
		public ObservableHashSet()
		{
		}

		public ObservableHashSet(List<T> list) : base(list)
		{
		}

		public ObservableHashSet(IEnumerable<T> collection) : base(collection)
		{
		}

		protected override void InsertItem(int index, T item)
		{
			if (Contains(item))
			{
				return;
			}
			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, T item)
		{
			int i = IndexOf(item);
			if (i >= 0 && i != index)
			{
				return;
			}
			base.SetItem(index, item);
		}
	}
}