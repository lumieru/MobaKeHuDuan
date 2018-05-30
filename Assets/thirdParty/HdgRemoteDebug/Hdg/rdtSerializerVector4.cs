using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerVector4 : rdtSerializerInterface
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public rdtSerializerVector4()
		{
		}

		public rdtSerializerVector4(Vector4 v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			w = v.w;
		}

		public Vector4 ToUnityType()
		{
			return new Vector4(x, y, z, w);
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write(x);
			bw.Write(y);
			bw.Write(z);
			bw.Write(w);
		}

		public void Read(BinaryReader r)
		{
			x = r.ReadSingle();
			y = r.ReadSingle();
			z = r.ReadSingle();
			w = r.ReadSingle();
		}
	}
}
