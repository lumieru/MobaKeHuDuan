using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerColor : rdtSerializerInterface
	{
		public float r;

		public float g;

		public float b;

		public float a;

		public rdtSerializerColor()
		{
		}

		public rdtSerializerColor(Color c)
		{
			r = c.r;
			g = c.g;
			b = c.b;
			a = c.a;
		}

		public Color ToUnityType()
		{
			return new Color(r, g, b, a);
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
			r = br.ReadSingle();
			g = br.ReadSingle();
			b = br.ReadSingle();
			a = br.ReadSingle();
		}
	}
}
