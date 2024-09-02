using System;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class UnexpectedRecordException : ApplicationException
{
	private const string DEF_MESSAGE = "Unexpected record.";

	private const string DEF_MESSAGE_CODE = "Unexpected record {0}.";

	public UnexpectedRecordException()
		: base("Unexpected record.")
	{
	}

	public UnexpectedRecordException(TBIFFRecord recordCode)
		: base($"Unexpected record {recordCode}.")
	{
	}

	public UnexpectedRecordException(string message)
		: base($"Unexpected record {message}.")
	{
	}

	public UnexpectedRecordException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
