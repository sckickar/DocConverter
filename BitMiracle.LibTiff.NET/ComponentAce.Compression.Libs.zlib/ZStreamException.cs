using System.IO;

namespace ComponentAce.Compression.Libs.zlib;

internal class ZStreamException : IOException
{
	public ZStreamException()
	{
	}

	public ZStreamException(string s)
		: base(s)
	{
	}
}
