using System;

namespace DocGen.Layouting.Exceptions;

internal class InvalidLayoutStateException : LayoutException
{
	private const string DEF_MESSAGE = "Fatal error";

	public InvalidLayoutStateException()
		: base("Fatal error")
	{
	}

	public InvalidLayoutStateException(Exception innerExc)
		: this("Fatal error", innerExc)
	{
	}

	public InvalidLayoutStateException(string message)
		: base(message)
	{
	}

	public InvalidLayoutStateException(string message, Exception innerExc)
		: base(message, innerExc)
	{
	}
}
