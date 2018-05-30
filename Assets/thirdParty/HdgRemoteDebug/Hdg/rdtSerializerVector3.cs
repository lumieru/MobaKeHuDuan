using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerVector3 : rdtSerializerInterface
	{
		public float x;

		public float y;

		public float z;

		public rdtSerializerVector3()
		{
		}

		public rdtSerializerVector3(Vector3 v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}

		public Vector3 ToUnityType()
		{
			return new Vector3(x, y, z);
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(x);
			w.Write(y);
			w.Write(z);
		}

		public void Read(BinaryReader r)
		{
			x = r.ReadSingle();
			y = r.ReadSingle();
			z = r.ReadSingle();
		}
	}
}
