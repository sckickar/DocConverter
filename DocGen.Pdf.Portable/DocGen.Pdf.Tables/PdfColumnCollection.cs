using System;

namespace DocGen.Pdf.Tables;

public class PdfColumnCollection : PdfCollection
{
	public PdfColumn this[int index] => base.List[index] as PdfColumn;

	internal PdfColumnCollection()
	{
	}

	public void Add(PdfColumn column)
	{
		base.List.Add(column);
	}

	internal float[] GetWidths(float totalWidth)
	{
		return GetWidths(totalWidth, 0, base.Count - 1, columnProportionalSizing: false);
	}

	internal float[] GetWidths(float totalWidth, int startColumn, int endColumn, bool columnProportionalSizing)
	{
		int num = endColumn - startColumn + 1;
		if (num > base.Count)
		{
			throw new ArgumentException("The start and end column indices doesn't match.");
		}
		float[] array = new float[num];
		float num2 = 0f;
		int num3 = num;
		for (int i = startColumn; i <= endColumn; i++)
		{
			num2 += (array[i - startColumn] = this[i].Width);
		}
		if (totalWidth > 0f)
		{
			float num4 = totalWidth / num2;
			if (!columnProportionalSizing)
			{
				for (int j = 0; j < num; j++)
				{
					if (this[j].isCustomWidth)
					{
						array[j] = this[j].Width;
						totalWidth -= this[j].Width;
						num3--;
					}
					else
					{
						array[j] = -1f;
					}
				}
				for (int k = 0; k < num; k++)
				{
					float num5 = totalWidth / (float)num3;
					if (array[k] <= 0f)
					{
						array[k] = num5;
					}
				}
			}
			else
			{
				for (int l = 0; l < num; l++)
				{
					array[l] *= num4;
				}
			}
		}
		return array;
	}
}
