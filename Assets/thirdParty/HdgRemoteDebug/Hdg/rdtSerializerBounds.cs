using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerBounds : rdtSerializerInterface
	{
		private rdtSerializerVector3 centre;

		private rdtSerializerVector3 size;

		public rdtSerializerBounds()
		{
		}

		public rdtSerializerBounds(Bounds b)
		{
			centre = new rdtSerializerVector3(b.center);
			size = new rdtSerializerVector3(b.size);
		}

		public Bounds ToUnityType()
		{
			return new Bounds(centre.ToUnityType(), size.ToUnityType());
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			centre.Write(w);
			size.Write(w);
		}

		public void Read(BinaryReader r)
		{
			centre = new rdtSerializerVector3();
			centre.Read(r);
			size = new rdtSerializerVector3();
			size.Read(r);
		}
	}
}
