using System;

namespace DocGen.OfficeChart;

internal class ExcelWorkbookNotSavedException : ApplicationException
{
	public ExcelWorkbookNotSavedException(string message)
		: base("Excel Binary workbook was not saved. " + message)
	{
	}

	public ExcelWorkbookNotSavedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
