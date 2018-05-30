using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerRect : rdtSerializerInterface
	{
		private float x;

		private float y;

		private float width;

		private float height;

		public rdtSerializerRect()
		{
		}

		public rdtSerializerRect(Rect r)
		{
			x = r.x;
			y = r.y;
			width = r.width;
			height = r.height;
		}

		public Rect ToUnityType()
		{
			return new Rect(x, y, width, height);
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(x);
			w.Write(y);
			w.Write(width);
			w.Write(height);
		}

		public void Read(BinaryReader r)
		{
			x = r.ReadSingle();
			y = r.ReadSingle();
			width = r.ReadSingle();
			height = r.ReadSingle();
		}
	}
}
