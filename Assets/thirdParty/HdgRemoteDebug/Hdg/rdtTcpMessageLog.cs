using System.IO;
using UnityEngine;

namespace Hdg
{
	public struct rdtTcpMessageLog : rdtTcpMessage
	{
		public string m_message;

		public string m_stackTrace;

		public LogType m_logType;

		public rdtTcpMessageLog(string message, string stackTrace, LogType logType)
		{
			m_message = message;
			m_stackTrace = stackTrace;
			m_logType = logType;
		}

		public override string ToString()
		{
			string s = m_message;
			if (!string.IsNullOrEmpty(m_stackTrace))
			{
				s = s + "\n" + m_stackTrace;
			}
			return s;
		}

		public void Write(BinaryWriter w)
		{
			w.Write(m_message);
			w.Write(m_stackTrace);
			w.Write((int)m_logType);
		}

		public void Read(BinaryReader r)
		{
			m_message = r.ReadString();
			m_stackTrace = r.ReadString();
			m_logType = (LogType)r.ReadInt32();
		}
	}
}
