namespace DocGen.Drawing.DocIOHelper;

internal interface IFontExtension
{
	float Size { get; }

	float GetEmHeight(FontStyle style);

	float GetCellAscent(FontStyle style);

	float GetCellDescent(FontStyle style);

	float GetLineSpacing(FontStyle style);
}
