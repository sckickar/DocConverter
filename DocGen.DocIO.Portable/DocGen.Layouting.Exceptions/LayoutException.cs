using System;

namespace DocGen.Layouting.Exceptions;

internal class LayoutException : ApplicationException
{
	private const string DEF_MESSAGE = "Incorrect layouting process";

	public LayoutException()
		: base("Incorrect layouting process")
	{
	}

	public LayoutException(Exception innerExc)
		: this("Incorrect layouting process", innerExc)
	{
	}

	public LayoutException(string message)
		: base(message)
	{
	}

	public LayoutException(string message, Exception innerExc)
		: base(message, innerExc)
	{
	}
}
