using System.Collections.Generic;

namespace DocGen.Pdf;

internal class Cmap4 : CmapTables
{
	private Segments[] segments;

	private ushort m_firstCode;

	public override ushort FirstCode => m_firstCode;

	public override ushort GetGlyphId(ushort charCode)
	{
		Segments[] array = this.segments;
		foreach (Segments segments in array)
		{
			if (segments.IsContain(charCode))
			{
				return segments.GetGlyphId(charCode);
			}
		}
		return 0;
	}

	public override void Read(ReadFontArray reader)
	{
		reader.getnextUshort();
		reader.getnextUshort();
		ushort num = (ushort)(reader.getnextUshort() / 2);
		reader.getnextUshort();
		reader.getnextUshort();
		reader.getnextUshort();
		ushort[] array = new ushort[num];
		ushort[] array2 = new ushort[num];
		short[] array3 = new short[num];
		ushort[] array4 = new ushort[num];
		segments = new Segments[num];
		m_firstCode = ushort.MaxValue;
		for (int i = 0; i < num; i++)
		{
			array[i] = reader.getnextUshort();
		}
		reader.getnextUshort();
		for (int j = 0; j < num; j++)
		{
			array2[j] = reader.getnextUshort();
			if (m_firstCode > array2[j])
			{
				m_firstCode = array2[j];
			}
		}
		for (int k = 0; k < num; k++)
		{
			array3[k] = reader.getnextshort();
		}
		for (int l = 0; l < num; l++)
		{
			long num2 = reader.Pointer;
			array4[l] = reader.getnextUshort();
			if (array4[l] <= 0)
			{
				segments[l] = new Segments(array2[l], array[l], array3[l]);
				continue;
			}
			int pointer = reader.Pointer;
			num2 += array4[l];
			ushort[] array5 = new ushort[array[l] - array2[l] + 1];
			new Dictionary<int, int>();
			reader.Pointer = (int)num2;
			for (int m = 0; m < array5.Length; m++)
			{
				array5[m] = reader.getnextUshort();
			}
			segments[l] = new Segments(array2[l], array[l], array3[l], array5);
			reader.Pointer = pointer;
		}
	}
}
