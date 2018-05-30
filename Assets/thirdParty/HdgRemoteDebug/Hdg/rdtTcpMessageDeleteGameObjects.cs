using System.Collections.Generic;
using System.IO;

namespace Hdg
{
	public struct rdtTcpMessageDeleteGameObjects : rdtTcpMessage
	{
		public List<int> m_instanceIds;

		public void Write(BinaryWriter w)
		{
			int count = m_instanceIds.Count;
			w.Write(count);
			for (int i = 0; i < count; i++)
			{
				w.Write(m_instanceIds[i]);
			}
		}

		public void Read(BinaryReader r)
		{
			int count = r.ReadInt32();
			m_instanceIds = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				int id = r.ReadInt32();
				m_instanceIds.Add(id);
			}
		}
	}
}
