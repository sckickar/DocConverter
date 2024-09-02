namespace DocGen.Office;

internal interface ITrueTypeFont
{
	float Size { get; }

	TrueTypeFontMetrics Metrics { get; }

	ITrueTypeFontPrimitive GetInternals();

	bool EqualsToFont(Font font);

	void CreateInternals();

	void CreateInternals(string originalFontName);

	int GetCharWidth(char charCode);

	int GetLineWidth(string line);

	float GetLineWidth(string line, TrueTypeFontStringFormat format, out float boundWidth);

	float GetLineWidth(string line, TrueTypeFontStringFormat format);

	void Close();
}
