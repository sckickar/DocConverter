using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class SmallBiffRecordDataException : ApplicationException
{
	public SmallBiffRecordDataException()
		: this("")
	{
	}

	public SmallBiffRecordDataException(string message)
		: base("BiffRecord data is too small. " + message)
	{
	}

	public SmallBiffRecordDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
