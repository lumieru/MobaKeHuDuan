using System.IO;

namespace Hdg
{
	public class rdtSerializerButton : rdtSerializerInterface
	{
		public bool Pressed;

		public rdtSerializerButton()
		{
		}

		public rdtSerializerButton(bool inpressed)
		{
			Pressed = inpressed;
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return this;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as rdtSerializerButton);
		}

		public bool Equals(rdtSerializerButton p)
		{
			if (p == null)
			{
				return false;
			}
			return Pressed == p.Pressed;
		}

		public override int GetHashCode()
		{
			return Pressed.GetHashCode();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Pressed);
		}

		public void Read(BinaryReader r)
		{
			Pressed = r.ReadBoolean();
		}
	}
}
