using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class InvalidRangeException : ApplicationException
{
	private const string DEF_MESSAGE = "Invalid range. ";

	public InvalidRangeException()
		: base("Invalid range. ")
	{
	}

	public InvalidRangeException(string message)
		: base("Invalid range. " + message)
	{
	}

	public InvalidRangeException(string message, Exception innerException)
		: base("Invalid range. " + message, innerException)
	{
	}
}
