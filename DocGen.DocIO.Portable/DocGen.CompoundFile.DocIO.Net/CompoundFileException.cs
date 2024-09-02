using System;

namespace DocGen.CompoundFile.DocIO.Net;

public class CompoundFileException : Exception
{
	private const string DefaultExceptionMessage = "Unable to parse compound file. Wrong file format.";

	public CompoundFileException()
		: base("Unable to parse compound file. Wrong file format.")
	{
	}

	public CompoundFileException(string message)
		: base(message)
	{
	}
}
