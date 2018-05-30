using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerQuaternion : rdtSerializerInterface
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public rdtSerializerQuaternion()
		{
		}

		public rdtSerializerQuaternion(Quaternion v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			w = v.w;
		}

		public object ToUnityType()
		{
			return new Quaternion(x, y, z, w);
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
