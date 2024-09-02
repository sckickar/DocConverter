using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class WrongBiffStreamFormatException : ApplicationException
{
	public WrongBiffStreamFormatException()
		: this("")
	{
	}

	public WrongBiffStreamFormatException(string message)
		: base("Wrong format of biff8 file structures." + message)
	{
	}

	public WrongBiffStreamFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
