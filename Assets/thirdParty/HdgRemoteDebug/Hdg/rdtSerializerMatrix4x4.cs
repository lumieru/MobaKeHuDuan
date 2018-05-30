using System.IO;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerMatrix4x4 : rdtSerializerInterface
	{
		private rdtSerializerVector4 col0;

		private rdtSerializerVector4 col1;

		private rdtSerializerVector4 col2;

		private rdtSerializerVector4 col3;

		public rdtSerializerMatrix4x4()
		{
		}

		public rdtSerializerMatrix4x4(Matrix4x4 m)
		{
			col0 = new rdtSerializerVector4(m.GetColumn(0));
			col1 = new rdtSerializerVector4(m.GetColumn(1));
			col2 = new rdtSerializerVector4(m.GetColumn(2));
			col3 = new rdtSerializerVector4(m.GetColumn(3));
		}

		public Matrix4x4 ToUnityType()
		{
			Matrix4x4 i = default(Matrix4x4);
			i.SetColumn(0, col0.ToUnityType());
			i.SetColumn(1, col1.ToUnityType());
			i.SetColumn(2, col2.ToUnityType());
			i.SetColumn(3, col3.ToUnityType());
			return i;
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return ToUnityType();
		}

		public void Write(BinaryWriter w)
		{
			col0.Write(w);
			col1.Write(w);
			col2.Write(w);
			col3.Write(w);
		}

		public void Read(BinaryReader r)
		{
			col0 = new rdtSerializerVector4();
			col0.Read(r);
			col1 = new rdtSerializerVector4();
			col1.Read(r);
			col2 = new rdtSerializerVector4();
			col2.Read(r);
			col3 = new rdtSerializerVector4();
			col3.Read(r);
		}
	}
}
