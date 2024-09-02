using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class BlankEnumerable<TElement>
{
	private static TElement[] instance;

	public static IEnumerable<TElement> Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new TElement[0];
			}
			return instance;
		}
	}
}
