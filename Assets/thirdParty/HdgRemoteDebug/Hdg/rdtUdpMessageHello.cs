using System.IO;

namespace Hdg
{
	public class rdtUdpMessageHello
	{
		public string m_deviceName;

		public string m_deviceType;

		public string m_devicePlatform;

		public string m_serverVersion;

		public int m_serverPort;

		public void Write(BinaryWriter w)
		{
			w.Write(m_deviceName);
			w.Write(m_deviceType);
			w.Write(m_devicePlatform);
			w.Write(m_serverVersion);
			w.Write(m_serverPort);
		}

		public void Read(BinaryReader r)
		{
			m_deviceName = r.ReadString();
			m_deviceType = r.ReadString();
			m_devicePlatform = r.ReadString();
			m_serverVersion = r.ReadString();
			m_serverPort = r.ReadInt32();
		}
	}
}
