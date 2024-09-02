using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontEmptyEnumerable<TElement>
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
