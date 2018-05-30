using System.IO;

namespace Hdg
{
	public struct rdtTcpMessageGetComponents : rdtTcpMessage
	{
		public int m_instanceId;

		public void Write(BinaryWriter w)
		{
			w.Write(m_instanceId);
		}

		public void Read(BinaryReader r)
		{
			m_instanceId = r.ReadInt32();
		}
	}
}
