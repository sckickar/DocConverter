using System;

namespace DocGen.DocIO;

internal class StreamWriteException : Exception
{
	private const string DEF_MESSAGE = "Incorrect writes process";

	internal StreamWriteException(string message)
		: base(message)
	{
	}
}
