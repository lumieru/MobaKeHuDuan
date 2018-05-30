using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Hdg
{
	public class ObservableList<T> : List<T>
	{
		public new T this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
				this.ListChanged(this);
			}
		}

        public event Action<ObservableList<T>> ListChanged;
        /*
		{
			[CompilerGenerated]
			add
			{
				Action<ObservableList<T>> action = this.ListChanged;
				Action<ObservableList<T>> action2;
				do
				{
					action2 = action;
					Action<ObservableList<T>> value2 = (Action<ObservableList<T>>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<ObservableList<T>>>(ref this.ListChanged, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<ObservableList<T>> action = this.ListChanged;
				Action<ObservableList<T>> action2;
				do
				{
					action2 = action;
					Action<ObservableList<T>> value2 = (Action<ObservableList<T>>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<ObservableList<T>>>(ref this.ListChanged, value2, action2);
				}
				while ((object)action != action2);
			}
		}
        */
		public new void Add(T item)
		{
			base.Add(item);
			this.ListChanged(this);
		}

		public new void Remove(T item)
		{
			base.Remove(item);
			this.ListChanged(this);
		}

		public new void AddRange(IEnumerable<T> collection)
		{
			base.AddRange(collection);
			this.ListChanged(this);
		}

		public new void RemoveRange(int index, int count)
		{
			base.RemoveRange(index, count);
			this.ListChanged(this);
		}

		public void ReplaceAll(T item)
		{
			base.Clear();
			base.Add(item);
			this.ListChanged(this);
		}

		public void ReplaceAll(IEnumerable<T> collection)
		{
			base.Clear();
			base.AddRange(collection);
			this.ListChanged(this);
		}

		public new void Clear()
		{
			base.Clear();
			this.ListChanged(this);
		}

		public new void Insert(int index, T item)
		{
			base.Insert(index, item);
			this.ListChanged(this);
		}

		public new void InsertRange(int index, IEnumerable<T> collection)
		{
			base.InsertRange(index, collection);
			this.ListChanged(this);
		}

		public new void RemoveAll(Predicate<T> match)
		{
			base.RemoveAll(match);
			this.ListChanged(this);
		}

		public ObservableList() : base()
		{
			this.ListChanged = delegate
			{
			};
		}
	}
}
