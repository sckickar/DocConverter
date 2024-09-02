using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class ParseException : ArgumentException
{
	private const string DEF_MESSAGE_FORMAT = "{0}. Formula: {1}, Position: {2}";

	public ParseException()
		: base("Can't parse formula.")
	{
	}

	public ParseException(string message)
		: base(message)
	{
	}

	public ParseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public ParseException(string message, string formula, int position, Exception innerException)
		: this($"{message}. Formula: {formula}, Position: {position}", innerException)
	{
	}
}
