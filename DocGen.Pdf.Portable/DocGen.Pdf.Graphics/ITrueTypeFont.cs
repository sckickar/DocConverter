using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

internal interface ITrueTypeFont
{
	float Size { get; }

	PdfFontMetrics Metrics { get; }

	IPdfPrimitive GetInternals();

	bool EqualsToFont(PdfFont font);

	void CreateInternals();

	float GetCharWidth(char charCode);

	float GetLineWidth(string line);

	void Close();
}
