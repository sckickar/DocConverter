using DocGen.DocIO.DLS;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class FontMetric
{
	public double Ascent(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		return WordDocument.RenderHelper.GetAscent(font, scriptType);
	}

	public double Descent(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		return WordDocument.RenderHelper.GetDescent(font, scriptType);
	}
}
