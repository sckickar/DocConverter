using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class WrongBiffStreamPartException : ApplicationException
{
	public WrongBiffStreamPartException()
		: this("")
	{
	}

	public WrongBiffStreamPartException(string message)
		: base("Tried to parse a noncorresponding part of Biff8 stream." + message)
	{
	}

	public WrongBiffStreamPartException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
