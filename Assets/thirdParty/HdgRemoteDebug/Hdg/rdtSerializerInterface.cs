using System.IO;

namespace Hdg
{
	public interface rdtSerializerInterface
	{
		object Deserialize(rdtSerializerRegistry registry);

		void Write(BinaryWriter w);

		void Read(BinaryReader r);
	}
}
