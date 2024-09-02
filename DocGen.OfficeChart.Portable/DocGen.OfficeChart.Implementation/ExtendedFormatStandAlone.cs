namespace DocGen.OfficeChart.Implementation;

internal class ExtendedFormatStandAlone : ExtendedFormatImpl
{
	private FontImpl font;

	public override IOfficeFont Font => font;

	public ExtendedFormatStandAlone(ExtendedFormatImpl format)
		: base(format.AppImplementation, format.Parent)
	{
		format.CopyTo(this);
		font = base.AppImplementation.CreateFont(format.Font);
		base.FontIndex = -1;
		InitializeColors();
		CopyColorsFrom(format);
	}
}
