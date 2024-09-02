using System;
using System.Collections;

namespace DocGen.OfficeChart;

internal interface IWorksheets : IEnumerable
{
	IApplication Application { get; }

	int Count { get; }

	IWorksheet this[int Index] { get; }

	IWorksheet this[string sheetName] { get; }

	object Parent { get; }

	bool UseRangesCache { get; set; }

	IWorksheet AddCopy(int sheetIndex);

	IWorksheet AddCopy(int sheetIndex, OfficeWorksheetCopyFlags flags);

	IWorksheet AddCopy(IWorksheet sourceSheet);

	IWorksheet AddCopy(IWorksheet sheet, OfficeWorksheetCopyFlags flags);

	void AddCopy(IWorksheets worksheets);

	void AddCopy(IWorksheets worksheets, OfficeWorksheetCopyFlags flags);

	IWorksheet Create(string name);

	IWorksheet Create();

	void Remove(IWorksheet sheet);

	void Remove(string sheetName);

	void Remove(int index);

	IRange FindFirst(string findValue, OfficeFindType flags);

	IRange FindFirst(string findValue, OfficeFindType flags, OfficeFindOptions findOptions);

	IRange FindFirst(double findValue, OfficeFindType flags);

	IRange FindFirst(bool findValue);

	IRange FindFirst(DateTime findValue);

	IRange FindFirst(TimeSpan findValue);

	IRange[] FindAll(string findValue, OfficeFindType flags);

	IRange[] FindAll(string findValue, OfficeFindType flags, OfficeFindOptions findOptions);

	IRange[] FindAll(double findValue, OfficeFindType flags);

	IRange[] FindAll(bool findValue);

	IRange[] FindAll(DateTime findValue);

	IRange[] FindAll(TimeSpan findValue);

	IWorksheet AddCopyBefore(IWorksheet toCopy);

	IWorksheet AddCopyBefore(IWorksheet toCopy, IWorksheet sheetAfter);

	IWorksheet AddCopyAfter(IWorksheet toCopy);

	IWorksheet AddCopyAfter(IWorksheet toCopy, IWorksheet sheetBefore);
}
