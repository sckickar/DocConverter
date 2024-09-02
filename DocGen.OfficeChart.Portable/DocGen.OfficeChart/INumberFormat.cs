namespace DocGen.OfficeChart;

internal interface INumberFormat : IParentApplication
{
	int Index { get; }

	string FormatString { get; }

	OfficeFormatType FormatType { get; }

	bool IsFraction { get; }

	bool IsScientific { get; }

	bool IsThousandSeparator { get; }

	int DecimalPlaces { get; }
}
