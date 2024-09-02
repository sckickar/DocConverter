using System;

namespace DocGen.Compression.Zip;

public class ZipException : ApplicationException
{
	public ZipException(string message)
		: base("Zip exception." + message)
	{
	}
}
