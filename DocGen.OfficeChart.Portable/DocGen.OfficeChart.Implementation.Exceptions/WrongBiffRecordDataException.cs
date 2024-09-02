using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class WrongBiffRecordDataException : ApplicationException
{
	public WrongBiffRecordDataException()
		: this("")
	{
	}

	public WrongBiffRecordDataException(string message)
		: base("Wrong BiffRecord data format. " + message)
	{
	}

	public WrongBiffRecordDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
