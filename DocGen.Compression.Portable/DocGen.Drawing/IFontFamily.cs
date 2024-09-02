namespace DocGen.Drawing;

internal interface IFontFamily
{
	string FontFamilyName { get; }

	bool IsStyleAvailable(FontStyle style);

	float GetEmHeight(FontStyle style);

	float GetCellAscent(FontStyle style);

	float GetCellDescent(FontStyle style);

	float GetLineSpacing(FontStyle style);
}
