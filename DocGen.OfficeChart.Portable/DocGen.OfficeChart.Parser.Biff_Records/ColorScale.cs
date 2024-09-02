using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class ColorScale
{
	private const ushort DEF_MINIMUM_SIZE = 6;

	private ushort m_undefined;

	private byte m_interpCurve;

	private byte m_gradient;

	private bool m_clamp = true;

	private bool m_background = true;

	private byte m_clampAndBackground = 3;

	private List<CFInterpolationCurve> m_arrCFInterp = new List<CFInterpolationCurve>();

	private List<CFGradientItem> m_arrCFGradient = new List<CFGradientItem>();

	public List<CFInterpolationCurve> ListCFInterpolationCurve
	{
		get
		{
			return m_arrCFInterp;
		}
		set
		{
			m_arrCFInterp = value;
		}
	}

	public List<CFGradientItem> ListCFGradientItem
	{
		get
		{
			return m_arrCFGradient;
		}
		set
		{
			m_arrCFGradient = value;
		}
	}

	public ushort DefaultRecordSize => 6;

	public ColorScale()
	{
		m_arrCFInterp = new List<CFInterpolationCurve>();
		m_arrCFGradient = new List<CFGradientItem>();
	}

	private void CopyColorScale()
	{
	}

	public int ParseColorScale(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_undefined = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadByte(iOffset);
		iOffset++;
		m_interpCurve = provider.ReadByte(iOffset);
		iOffset++;
		m_gradient = provider.ReadByte(iOffset);
		iOffset++;
		m_clampAndBackground = provider.ReadByte(iOffset);
		iOffset++;
		for (int i = 0; i < m_interpCurve; i++)
		{
			CFInterpolationCurve cFInterpolationCurve = new CFInterpolationCurve();
			iOffset = cFInterpolationCurve.ParseCFGradientInterp(provider, iOffset, version);
			m_arrCFInterp.Add(cFInterpolationCurve);
		}
		for (int j = 0; j < m_gradient; j++)
		{
			CFGradientItem cFGradientItem = new CFGradientItem();
			iOffset = cFGradientItem.ParseCFGradient(provider, iOffset, version);
			m_arrCFGradient.Add(cFGradientItem);
		}
		CopyColorScale();
		return iOffset;
	}

	public int GetStoreSize(OfficeVersion version)
	{
		int num = 0;
		foreach (CFInterpolationCurve item in ListCFInterpolationCurve)
		{
			num += item.GetStoreSize(version);
		}
		int num2 = 0;
		foreach (CFGradientItem item2 in ListCFGradientItem)
		{
			num2 += item2.GetStoreSize(version);
		}
		return 6 + num + num2;
	}

	private double CalculateNumValue(int position)
	{
		double result = 0.0;
		if (ListCFInterpolationCurve.Count == 3)
		{
			if (position == 1)
			{
				result = 0.0;
			}
			if (position == 2)
			{
				result = 0.5;
			}
			if (position == 3)
			{
				result = 1.0;
			}
		}
		if (ListCFInterpolationCurve.Count == 2)
		{
			if (position == 1)
			{
				result = 0.0;
			}
			if (position == 2)
			{
				result = 1.0;
			}
		}
		return result;
	}

	private uint ColorToUInt(Color color)
	{
		return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
	}

	private Color UIntToColor(uint color)
	{
		byte alpha = (byte)(color >> 24);
		byte red = (byte)(color >> 16);
		byte green = (byte)(color >> 8);
		byte blue = (byte)color;
		return Color.FromArgb(alpha, red, green, blue);
	}

	private Color ConvertARGBToRGBA(Color colorValue)
	{
		byte b = colorValue.B;
		byte g = colorValue.G;
		byte r = colorValue.R;
		colorValue = Color.FromArgb(colorValue.A, b, g, r);
		return colorValue;
	}

	private Color ConvertRGBAToARGB(Color colorValue)
	{
		byte a = colorValue.A;
		byte b = colorValue.B;
		byte g = colorValue.G;
		byte r = colorValue.R;
		colorValue = Color.FromArgb(a, b, g, r);
		return colorValue;
	}

	internal void ClearAll()
	{
		m_arrCFInterp.Clear();
		m_arrCFGradient.Clear();
		m_arrCFInterp = null;
		m_arrCFGradient = null;
	}
}
