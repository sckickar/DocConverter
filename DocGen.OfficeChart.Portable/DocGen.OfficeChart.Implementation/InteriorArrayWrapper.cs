using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class InteriorArrayWrapper : CommonObject, IInterior
{
	private List<IRange> m_arrCells = new List<IRange>();

	public OfficeKnownColors PatternColorIndex
	{
		get
		{
			OfficeKnownColors officeKnownColors = OfficeKnownColors.Black;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					officeKnownColors = range.CellStyle.Interior.PatternColorIndex;
					flag = false;
				}
				else if (range.CellStyle.Interior.PatternColorIndex != officeKnownColors)
				{
					return OfficeKnownColors.Black;
				}
			}
			return officeKnownColors;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.PatternColorIndex = value;
			}
		}
	}

	public Color PatternColor
	{
		get
		{
			Color color = ColorExtension.Empty;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					color = range.CellStyle.Interior.PatternColor;
					flag = false;
				}
				else if (range.CellStyle.Interior.PatternColor != color)
				{
					return ColorExtension.Empty;
				}
			}
			return color;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.PatternColor = value;
			}
		}
	}

	public OfficeKnownColors ColorIndex
	{
		get
		{
			OfficeKnownColors officeKnownColors = OfficeKnownColors.Black;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					officeKnownColors = range.CellStyle.Interior.ColorIndex;
					flag = false;
				}
				else if (range.CellStyle.Interior.ColorIndex != officeKnownColors)
				{
					return OfficeKnownColors.Black;
				}
			}
			return officeKnownColors;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.ColorIndex = value;
			}
		}
	}

	public Color Color
	{
		get
		{
			Color color = ColorExtension.Empty;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					color = range.CellStyle.Interior.Color;
					flag = false;
				}
				else if (range.CellStyle.Interior.Color != color)
				{
					return ColorExtension.Empty;
				}
			}
			return color;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.Color = value;
			}
		}
	}

	public IGradient Gradient
	{
		get
		{
			IGradient gradient = null;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					gradient = range.CellStyle.Interior.Gradient;
					flag = false;
				}
				else if (range.CellStyle.Interior.Gradient != gradient)
				{
					return new GradientArrayWrapper((IRange)base.Parent);
				}
			}
			return gradient;
		}
	}

	public OfficePattern FillPattern
	{
		get
		{
			OfficePattern officePattern = OfficePattern.None;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					officePattern = range.CellStyle.Interior.FillPattern;
					flag = false;
				}
				else if (range.CellStyle.Interior.FillPattern != officePattern)
				{
					return OfficePattern.None;
				}
			}
			return officePattern;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.FillPattern = value;
			}
		}
	}

	public InteriorArrayWrapper(IRange range)
		: base((range as RangeImpl).Application, range)
	{
		m_arrCells.AddRange(range.Cells);
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}
}
