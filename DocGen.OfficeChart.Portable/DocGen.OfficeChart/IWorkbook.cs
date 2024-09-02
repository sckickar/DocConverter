using System;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart;

internal interface IWorkbook : IParentApplication
{
	IWorksheet ActiveSheet { get; }

	int ActiveSheetIndex { get; set; }

	string Author { get; set; }

	bool IsHScrollBarVisible { get; set; }

	bool IsVScrollBarVisible { get; set; }

	string CodeName { get; set; }

	bool Date1904 { get; set; }

	bool PrecisionAsDisplayed { get; set; }

	bool IsCellProtection { get; }

	bool IsWindowProtection { get; }

	INames Names { get; }

	bool ReadOnly { get; }

	bool Saved { get; set; }

	IStyles Styles { get; }

	IWorksheets Worksheets { get; }

	bool HasMacros { get; }

	[Obsolete("IWorkbook.Palettte property is obsolete so please use the IWorkbook.Palette property instead. IWorkbook.Palettte will be removed in July 2006. Sorry for the inconvenience")]
	Color[] Palettte { get; }

	Color[] Palette { get; }

	int DisplayedTab { get; set; }

	bool ThrowOnUnknownNames { get; set; }

	bool DisableMacrosStart { get; set; }

	double StandardFontSize { get; set; }

	string StandardFont { get; set; }

	bool Allow3DRangesInDataValidation { get; set; }

	ICalculationOptions CalculationOptions { get; }

	string RowSeparator { get; }

	string ArgumentsSeparator { get; }

	IWorksheetGroup WorksheetGroup { get; }

	bool IsRightToLeft { get; set; }

	bool DisplayWorkbookTabs { get; set; }

	ITabSheets TabSheets { get; }

	bool DetectDateTimeInValue { get; set; }

	bool UseFastStringSearching { get; set; }

	bool ReadOnlyRecommended { get; set; }

	string PasswordToOpen { get; set; }

	int MaxRowCount { get; }

	int MaxColumnCount { get; }

	OfficeVersion Version { get; set; }

	event EventHandler OnFileSaved;

	event ReadOnlyFileEventHandler OnReadOnlyFile;

	void Activate();

	IOfficeFont AddFont(IOfficeFont fontToAdd);

	void Close(bool SaveChanges, string Filename);

	void Close(bool saveChanges);

	void Close();

	void SaveAs(Stream stream);

	void SaveAs(Stream stream, OfficeSaveType saveType);

	void SaveAs(Stream stream, string separator);

	void SetPaletteColor(int index, Color color);

	void ResetPalette();

	Color GetPaletteColor(OfficeKnownColors color);

	OfficeKnownColors GetNearestColor(Color color);

	OfficeKnownColors GetNearestColor(int r, int g, int b);

	OfficeKnownColors SetColorOrGetNearest(Color color);

	OfficeKnownColors SetColorOrGetNearest(int r, int g, int b);

	IOfficeFont CreateFont();

	IOfficeFont CreateFont(IOfficeFont baseFont);

	void Replace(string oldValue, string newValue);

	void Replace(string oldValue, double newValue);

	void Replace(string oldValue, DateTime newValue);

	void Replace(string oldValue, string[] newValues, bool isVertical);

	void Replace(string oldValue, int[] newValues, bool isVertical);

	void Replace(string oldValue, double[] newValues, bool isVertical);

	IRange FindFirst(string findValue, OfficeFindType flags);

	IRange FindFirst(string findValue, OfficeFindType flags, OfficeFindOptions findOptions);

	IRange FindStringStartsWith(string findValue, OfficeFindType flags);

	IRange FindStringStartsWith(string findValue, OfficeFindType flags, bool ignoreCase);

	IRange FindStringEndsWith(string findValue, OfficeFindType flags);

	IRange FindStringEndsWith(string findValue, OfficeFindType flags, bool ignoreCase);

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

	void SetSeparators(char argumentsSeparator, char arrayRowsSeparator);

	void Protect(bool bIsProtectWindow, bool bIsProtectContent);

	void Protect(bool bIsProtectWindow, bool bIsProtectContent, string password);

	void Unprotect();

	void Unprotect(string password);

	IWorkbook Clone();

	void SetWriteProtectionPassword(string password);
}
