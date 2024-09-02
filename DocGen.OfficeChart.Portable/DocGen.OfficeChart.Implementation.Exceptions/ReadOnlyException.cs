using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class ReadOnlyException : ApplicationException
{
	private const string DEF_MESSAGE = "Can't modify read-only data. ";

	public ReadOnlyException()
		: this("Can't modify read-only data. ")
	{
	}

	public ReadOnlyException(string message)
		: base("Can't modify read-only data. " + message)
	{
	}

	public ReadOnlyException(string message, Exception innerException)
		: base("Can't modify read-only data. " + message, innerException)
	{
	}
}
