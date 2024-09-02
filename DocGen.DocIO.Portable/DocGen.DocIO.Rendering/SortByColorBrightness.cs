using System.Collections.Generic;
using DocGen.DocIO.DLS;
using DocGen.Drawing;

namespace DocGen.DocIO.Rendering;

internal class SortByColorBrightness : IComparer<Border>
{
	public int Compare(Border firstBorder, Border secondBorder)
	{
		if (firstBorder != null && secondBorder != null)
		{
			Color color = firstBorder.Color;
			Color color2 = secondBorder.Color;
			int num = color.R + color.B + 2 * color.G;
			int num2 = color2.R + color2.B + 2 * color2.G;
			if (num < num2)
			{
				return 1;
			}
			if (num > num2)
			{
				return -1;
			}
			return 0;
		}
		return 0;
	}
}
