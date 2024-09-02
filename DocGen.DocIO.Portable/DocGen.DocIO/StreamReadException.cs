using System;

namespace DocGen.DocIO;

internal class StreamReadException : Exception
{
	private const string DEF_MESSAGE = "Was unable to read sufficient bytes from the stream";

	internal StreamReadException()
		: base("Was unable to read sufficient bytes from the stream")
	{
	}

	internal StreamReadException(Exception innerExc)
		: this("Was unable to read sufficient bytes from the stream", innerExc)
	{
	}

	internal StreamReadException(string message)
		: base(message)
	{
	}

	internal StreamReadException(string message, Exception innerExc)
		: base(message, innerExc)
	{
	}
}
