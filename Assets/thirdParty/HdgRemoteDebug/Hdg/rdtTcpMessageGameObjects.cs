using System.Collections.Generic;
using System.IO;

namespace Hdg
{
	public struct rdtTcpMessageGameObjects : rdtTcpMessage
	{
		public struct Gob
		{
			public bool m_enabled;

			public string m_name;

			public int m_instanceId;

			public bool m_hasParent;

			public int m_parentInstanceId;

			public string m_scene;

			public override string ToString()
			{
				string name = m_name;
				if (rdtDebug.s_logLevel == rdtDebug.LogLevel.Debug)
				{
					name = name + ":" + m_instanceId;
				}
				return name;
			}

			public override bool Equals(object obj)
			{
				Gob o = (Gob)obj;
				return m_instanceId == o.m_instanceId;
			}

			public override int GetHashCode()
			{
				return m_instanceId;
			}

			public void Write(BinaryWriter w)
			{
				w.Write(m_enabled);
				w.Write(m_name);
				w.Write(m_instanceId);
				w.Write(m_hasParent);
				w.Write(m_parentInstanceId);
				w.Write(m_scene);
			}

			public void Read(BinaryReader r)
			{
				m_enabled = r.ReadBoolean();
				m_name = r.ReadString();
				m_instanceId = r.ReadInt32();
				m_hasParent = r.ReadBoolean();
				m_parentInstanceId = r.ReadInt32();
				m_scene = r.ReadString();
			}
		}

		public List<Gob> m_allGobs;

		public void Write(BinaryWriter w)
		{
			int num = m_allGobs.Count;
			w.Write(num);
			for (int i = 0; i < num; i++)
			{
				m_allGobs[i].Write(w);
			}
		}

		public void Read(BinaryReader r)
		{
			m_allGobs = new List<Gob>();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Gob gob = default(Gob);
				gob.Read(r);
				m_allGobs.Add(gob);
			}
		}
	}
}
