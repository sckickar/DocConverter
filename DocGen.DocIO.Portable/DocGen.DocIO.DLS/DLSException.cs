using System;

namespace DocGen.DocIO.DLS;

public class DLSException : Exception
{
	private const string DEF_MESSAGE = "Exception in DLS library";

	public DLSException()
		: base("Exception in DLS library")
	{
	}

	public DLSException(Exception innerExc)
		: this("Exception in DLS library", innerExc)
	{
	}

	public DLSException(string message)
		: base(message)
	{
	}

	public DLSException(string message, Exception innerExc)
		: base(message, innerExc)
	{
	}
}
