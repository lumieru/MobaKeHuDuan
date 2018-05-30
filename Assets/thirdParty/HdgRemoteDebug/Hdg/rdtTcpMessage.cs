using System.IO;

namespace Hdg
{
	public interface rdtTcpMessage
	{
		void Write(BinaryWriter w);

		void Read(BinaryReader r);
	}
}
