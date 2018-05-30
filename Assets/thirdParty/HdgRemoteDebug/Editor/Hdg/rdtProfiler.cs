using System;
using UnityEngine;

namespace Hdg
{
	internal class rdtProfiler : IDisposable
	{
		private DateTime m_start;

		private string m_description;

		public rdtProfiler(string desc)
		{
			m_start = DateTime.Now;
			m_description = desc;
		}

		public void Dispose()
		{
			TimeSpan delta = DateTime.Now - m_start;
			Debug.Log(string.Format("{0} took {1}s", m_description, delta.TotalSeconds));
		}
	}
}
