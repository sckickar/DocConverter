using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class GlyphOutlinesCollection : List<PathFigure>
{
	public GlyphOutlinesCollection Clone()
	{
		GlyphOutlinesCollection glyphOutlinesCollection = new GlyphOutlinesCollection();
		using List<PathFigure>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			PathFigure current = enumerator.Current;
			glyphOutlinesCollection.Add(current.Clone());
		}
		return glyphOutlinesCollection;
	}

	public void Transform(Matrix transformMatrix)
	{
		using List<PathFigure>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Transform(transformMatrix);
		}
	}
}
