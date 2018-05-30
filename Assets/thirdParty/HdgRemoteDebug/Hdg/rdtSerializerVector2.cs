using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerVector2 : rdtSerializerInterface
	{
		public float x;

		public float y;

		public rdtSerializerVector2()
		{
		}

		public rdtSerializerVector2(Vector2 v)
		{
			x = v.x;
			y = v.y;
		}

		public Vector2 ToUnityType()
		{
			return new Vector2(x, y);
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(x);
			w.Write(y);
		}

		public void Read(BinaryReader r)
		{
			x = r.ReadSingle();
			y = r.ReadSingle();
		}
	}
}
