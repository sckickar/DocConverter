using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontGlyphOutlinesCollection : List<SystemFontPathFigure>
{
	public SystemFontGlyphOutlinesCollection Clone()
	{
		SystemFontGlyphOutlinesCollection systemFontGlyphOutlinesCollection = new SystemFontGlyphOutlinesCollection();
		using List<SystemFontPathFigure>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			SystemFontPathFigure current = enumerator.Current;
			systemFontGlyphOutlinesCollection.Add(current.Clone());
		}
		return systemFontGlyphOutlinesCollection;
	}

	public void Transform(SystemFontMatrix transformMatrix)
	{
		using List<SystemFontPathFigure>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Transform(transformMatrix);
		}
	}
}
