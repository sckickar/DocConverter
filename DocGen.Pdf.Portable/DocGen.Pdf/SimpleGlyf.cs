using System.Collections.Generic;

namespace DocGen.Pdf;

internal class SimpleGlyf : TrueTypeGlyphs
{
	private List<OutlinePoint[]> contours;

	internal new List<OutlinePoint[]> Contours
	{
		get
		{
			if (contours == null)
			{
				contours = new List<OutlinePoint[]>();
			}
			return contours;
		}
	}

	public SimpleGlyf(FontFile2 fontFile, ushort glyphIndex)
		: base(fontFile, glyphIndex)
	{
	}

	public override void Read(ReadFontArray reader)
	{
		reader.getnextshort();
		reader.getnextshort();
		reader.getnextshort();
		reader.getnextshort();
		ushort[] array = new ushort[base.NumberOfContours];
		for (int i = 0; i < base.NumberOfContours; i++)
		{
			array[i] = reader.getnextUshort();
		}
		int num = array[base.NumberOfContours - 1] + 1;
		ushort num2 = reader.getnextUshort();
		byte[] array2 = new byte[num2];
		for (int j = 0; j < num2; j++)
		{
			array2[j] = reader.getnextbyte();
		}
		byte[] array3 = new byte[num];
		for (int k = 0; k < num; k++)
		{
			array3[k] = reader.getnextbyte();
			if (Repeat(array3, k))
			{
				byte b = array3[k];
				byte b2 = reader.getnextbyte();
				for (int l = 0; l < b2; l++)
				{
					array3[++k] = b;
				}
			}
		}
		int[] array4 = new int[num];
		for (int m = 0; m < num; m++)
		{
			if (m > 0)
			{
				array4[m] = array4[m - 1];
			}
			if (XIsByte(array3, m))
			{
				int num3 = reader.getnextbyte();
				if (!XIsSame(array3, m))
				{
					num3 = -num3;
				}
				array4[m] += num3;
			}
			else if (!XIsSame(array3, m))
			{
				array4[m] += reader.getnextshort();
			}
		}
		int[] array5 = new int[num];
		for (int n = 0; n < num; n++)
		{
			if (n > 0)
			{
				array5[n] = array5[n - 1];
			}
			if (YIsByte(array3, n))
			{
				int num4 = reader.getnextbyte();
				if (!YIsSame(array3, n))
				{
					num4 = -num4;
				}
				array5[n] += num4;
			}
			else if (!YIsSame(array3, n))
			{
				array5[n] += reader.getnextshort();
			}
		}
		contours = new List<OutlinePoint[]>();
		int num5 = 0;
		int num6 = 0;
		OutlinePoint[] array6 = new OutlinePoint[array[0] + 1];
		contours.Add(array6);
		for (int num7 = 0; num7 < num; num7++)
		{
			array6[num6++] = new OutlinePoint(array4[num7], array5[num7], array3[num7]);
			if (num7 == array[num5])
			{
				if (num5 == array.Length - 1)
				{
					return;
				}
				num5++;
				array6 = new OutlinePoint[array[num5] - array[num5 - 1]];
				contours.Add(array6);
				num6 = 0;
			}
		}
		array2 = null;
		array3 = null;
		array6 = null;
	}

	private static bool XIsByte(byte[] flags, int index)
	{
		return GetBit(flags[index], 1);
	}

	private static bool YIsByte(byte[] flags, int index)
	{
		return GetBit(flags[index], 2);
	}

	private static bool XIsSame(byte[] flags, int index)
	{
		return GetBit(flags[index], 4);
	}

	private static bool YIsSame(byte[] flags, int index)
	{
		return GetBit(flags[index], 5);
	}

	private static bool Repeat(byte[] flags, int index)
	{
		return GetBit(flags[index], 3);
	}

	internal static bool GetBit(int n, byte bit)
	{
		return (n & (1 << (int)bit)) != 0;
	}
}
