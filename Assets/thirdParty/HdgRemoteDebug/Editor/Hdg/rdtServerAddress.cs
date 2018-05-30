using System;
using System.Net;

namespace Hdg
{
	[Serializable]
	public class rdtServerAddress : IComparable<rdtServerAddress>
	{
		public string m_deviceName;

		public string m_deviceType;

		public string m_devicePlatform;

		private byte[] m_address;

		public int m_port;

		public double m_timer;

		private string m_formattedName;

		public string m_serverVersion;

		public IPAddress IPAddress
		{
			get
			{
				if (m_address != null && m_address.Length != 0)
				{
					return new IPAddress(m_address);
				}
				return null;
			}
		}

		public string FormattedName
		{
			get
			{
				return m_formattedName;
			}
		}

		public rdtServerAddress(IPAddress address, int port, string name, string type, string platform, string serverVersion)
		{
			m_serverVersion = serverVersion;
			m_address = address.GetAddressBytes();
			m_port = port;
			m_deviceName = (name ?? "");
			m_deviceType = (type ?? "");
			m_devicePlatform = (platform ?? "");
			m_timer = 0.0;
			bool unknownName = m_deviceName.Equals("<unknown>");
			m_formattedName = (unknownName ? "" : (m_deviceName + " "));
			if (!string.IsNullOrEmpty(m_devicePlatform))
			{
				if (!unknownName)
				{
					m_formattedName += "- ";
				}
				m_formattedName = m_formattedName + m_devicePlatform + " ";
			}
			m_formattedName += string.Format("({0}@{1}:{2})", m_deviceType, address.ToString(), m_port);
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			rdtServerAddress s = o as rdtServerAddress;
			if (s.m_deviceName == m_deviceName && s.m_deviceType == m_deviceType && s.m_devicePlatform == m_devicePlatform && s.IPAddress.Equals(IPAddress))
			{
				return s.m_port == m_port;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash2 = 13;
			if (m_deviceName != null)
			{
				hash2 = hash2 * 7 + m_deviceName.GetHashCode();
			}
			if (m_deviceType != null)
			{
				hash2 = hash2 * 7 + m_deviceType.GetHashCode();
			}
			if (m_devicePlatform != null)
			{
				hash2 = hash2 * 7 + m_devicePlatform.GetHashCode();
			}
			hash2 = hash2 * 7 + m_port.GetHashCode();
			return hash2 * 7 + m_address.GetHashCode();
		}

		public override string ToString()
		{
			return FormattedName;
		}

		public int CompareTo(rdtServerAddress other)
		{
			return FormattedName.CompareTo(other.FormattedName);
		}
	}
}
