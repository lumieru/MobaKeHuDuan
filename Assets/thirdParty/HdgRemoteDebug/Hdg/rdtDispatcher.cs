using System;
using System.Collections.Generic;

namespace Hdg
{
	public class rdtDispatcher
	{
		private Queue<Action> m_callbacks = new Queue<Action>();

		public void Clear()
		{
			lock (m_callbacks)
			{
				m_callbacks.Clear();
			}
		}

		public void Enqueue(Action action)
		{
			lock (m_callbacks)
			{
				m_callbacks.Enqueue(action);
			}
		}

		public void Update()
		{
			lock (m_callbacks)
			{
				while (m_callbacks.Count > 0)
				{
					m_callbacks.Dequeue()();
				}
			}
		}
	}
}
