using System.IO;

namespace Hdg
{
	public struct rdtTcpMessageUpdateGameObjectProperties : rdtTcpMessage
	{
		public enum Flags
		{
			UpdateEnabled = 1,
			UpdateTag,
			UpdateLayer = 4
		}

		public int m_instanceId;

		public Flags m_flags;

		public bool m_enabled;

		public string m_tag;

		public int m_layer;

		public void Write(BinaryWriter w)
		{
			w.Write(m_instanceId);
			w.Write(m_enabled);
			w.Write(m_tag);
			w.Write(m_layer);
			w.Write((int)m_flags);
		}

		public void Read(BinaryReader r)
		{
			m_instanceId = r.ReadInt32();
			m_enabled = r.ReadBoolean();
			m_tag = r.ReadString();
			m_layer = r.ReadInt32();
			m_flags = (Flags)r.ReadInt32();
		}

		public void SetFlag(Flags flag, bool enabled)
		{
			if (enabled)
			{
				m_flags |= flag;
			}
			else
			{
				m_flags &= ~flag;
			}
		}

		public bool HasFlag(Flags flag)
		{
			return (m_flags & flag) != (Flags)0;
		}
	}
}
