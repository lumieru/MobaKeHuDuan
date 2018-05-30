using System.Collections.Generic;
using System.IO;

namespace Hdg
{
	public struct rdtTcpMessageSetArraySize : rdtTcpMessage
	{
		public int m_gameObjectInstanceId;

		public int m_componentInstanceId;

		public string m_componentName;

		public int m_size;

		public List<rdtTcpMessageComponents.Property> m_properties;

		public void Write(BinaryWriter w)
		{
			w.Write(m_gameObjectInstanceId);
			w.Write(m_componentInstanceId);
			w.Write(m_componentName);
			w.Write(m_size);
			rdtTcpMessageComponents.Component.WriteProperties(w, m_properties);
		}

		public void Read(BinaryReader r)
		{
			m_gameObjectInstanceId = r.ReadInt32();
			m_componentInstanceId = r.ReadInt32();
			m_componentName = r.ReadString();
			m_size = r.ReadInt32();
			m_properties = rdtTcpMessageComponents.Component.ReadProperties(r);
		}
	}
}
