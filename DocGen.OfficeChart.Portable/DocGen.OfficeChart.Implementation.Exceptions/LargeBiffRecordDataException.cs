using System;

namespace DocGen.OfficeChart.Implementation.Exceptions;

internal class LargeBiffRecordDataException : ApplicationException
{
	public LargeBiffRecordDataException()
		: this("")
	{
	}

	public LargeBiffRecordDataException(string message)
		: base("BiffRecord data is too large." + message)
	{
	}

	public LargeBiffRecordDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
