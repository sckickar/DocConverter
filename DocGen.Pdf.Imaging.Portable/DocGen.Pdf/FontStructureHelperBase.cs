using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal interface FontStructureHelperBase
{
	string FontName { get; }

	bool IsEmbedded { get; set; }

	string FontEncoding { get; }

	Dictionary<double, string> CharacterMapTable { get; }

	Dictionary<string, double> ReverseMapTable { get; }

	PdfDictionary FontDictionary { get; }

	Dictionary<int, int> FontGlyphWidths { get; }

	float FontSize { get; }

	Dictionary<string, string> DifferencesDictionary { get; }

	string Decode(string text, bool v);
}
