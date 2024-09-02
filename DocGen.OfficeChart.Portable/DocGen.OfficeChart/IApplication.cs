using System;
using System.IO;
using DocGen.Compression;

namespace DocGen.OfficeChart;

internal interface IApplication : IParentApplication
{
	IRange ActiveCell { get; }

	IWorksheet ActiveSheet { get; }

	IWorkbook ActiveWorkbook { get; }

	IWorkbooks Workbooks { get; }

	IWorksheets Worksheets { get; }

	IRange Range { get; }

	bool FixedDecimal { get; set; }

	bool UseSystemSeparators { get; set; }

	int Build { get; }

	int FixedDecimalPlaces { get; set; }

	int SheetsInNewWorkbook { get; set; }

	string DecimalSeparator { get; set; }

	string ThousandsSeparator { get; set; }

	string UserName { get; set; }

	string Value { get; }

	bool ChangeStyleOnCellEdit { get; set; }

	OfficeSkipExtRecords SkipOnSave { get; set; }

	double StandardHeight { get; set; }

	bool StandardHeightFlag { get; set; }

	double StandardWidth { get; set; }

	bool OptimizeFonts { get; set; }

	bool OptimizeImport { get; set; }

	char RowSeparator { get; set; }

	char ArgumentsSeparator { get; set; }

	string CSVSeparator { get; set; }

	string StandardFont { get; set; }

	double StandardFontSize { get; set; }

	[Obsolete("Use DataProviderType property instead")]
	bool UseNativeOptimization { get; set; }

	bool UseFastRecordParsing { get; set; }

	int RowStorageAllocationBlockSize { get; set; }

	bool DeleteDestinationFile { get; set; }

	OfficeVersion DefaultVersion { get; set; }

	CompressionLevel? CompressionLevel { get; set; }

	bool PreserveCSVDataTypes { get; set; }

	IOfficeChartToImageConverter ChartToImageConverter { get; set; }

	event ProgressEventHandler ProgressEvent;

	event PasswordRequiredEventHandler OnPasswordRequired;

	event PasswordRequiredEventHandler OnWrongPassword;

	double CentimetersToPoints(double Centimeters);

	double InchesToPoints(double Inches);

	bool IsSupported(Stream Stream);

	double ConvertUnits(double value, MeasureUnits from, MeasureUnits to);
}
