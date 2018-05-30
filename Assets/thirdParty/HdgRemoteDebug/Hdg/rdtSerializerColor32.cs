using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerColor32 : rdtSerializerInterface
	{
		public byte r;

		public byte g;

		public byte b;

		public byte a;

		public rdtSerializerColor32()
		{
		}

		public rdtSerializerColor32(Color32 c)
		{
			r = c.r;
			g = c.g;
			b = c.b;
			a = c.a;
		}

		public Color32 ToUnityType()
		{
			return new Color32(r, g, b, a);
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(r);
			w.Write(g);
			w.Write(b);
			w.Write(a);
		}

		public void Read(BinaryReader br)
		{
			r = br.ReadByte();
			g = br.ReadByte();
			b = br.ReadByte();
			a = br.ReadByte();
		}
	}
}
