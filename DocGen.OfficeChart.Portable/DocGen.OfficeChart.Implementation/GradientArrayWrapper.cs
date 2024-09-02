using System;
using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class GradientArrayWrapper : CommonObject, IGradient
{
	private List<IRange> m_arrCells = new List<IRange>();

	public ChartColor BackColorObject
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Color BackColor
	{
		get
		{
			int num = 0;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					num = range.CellStyle.Interior.Gradient.BackColor.ToArgb();
					flag = false;
				}
				else if (range.CellStyle.Interior.Gradient.BackColor.ToArgb() != num)
				{
					return ColorExtension.Empty;
				}
			}
			return ColorExtension.FromArgb(num);
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.Gradient.BackColor = value;
			}
		}
	}

	public OfficeKnownColors BackColorIndex
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
					officeKnownColors = range.CellStyle.Interior.Gradient.BackColorIndex;
					flag = false;
				}
				else if (range.CellStyle.Interior.Gradient.BackColorIndex != officeKnownColors)
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
				m_arrCells[i].CellStyle.Interior.Gradient.BackColorIndex = value;
			}
		}
	}

	public ChartColor ForeColorObject
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Color ForeColor
	{
		get
		{
			int num = 0;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					num = range.CellStyle.Interior.Gradient.ForeColor.ToArgb();
					flag = false;
				}
				else if (range.CellStyle.Interior.Gradient.ForeColor.ToArgb() != num)
				{
					return ColorExtension.Empty;
				}
			}
			return ColorExtension.FromArgb(num);
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.Gradient.ForeColor = value;
			}
		}
	}

	public OfficeKnownColors ForeColorIndex
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
					officeKnownColors = range.CellStyle.Interior.Gradient.ForeColorIndex;
					flag = false;
				}
				else if (range.CellStyle.Interior.Gradient.ForeColorIndex != officeKnownColors)
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
				m_arrCells[i].CellStyle.Interior.Gradient.ForeColorIndex = value;
			}
		}
	}

	public OfficeGradientStyle GradientStyle
	{
		get
		{
			OfficeGradientStyle officeGradientStyle = OfficeGradientStyle.Horizontal;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					officeGradientStyle = range.CellStyle.Interior.Gradient.GradientStyle;
					flag = false;
				}
				else if (range.CellStyle.Interior.Gradient.GradientStyle != officeGradientStyle)
				{
					return OfficeGradientStyle.Horizontal;
				}
			}
			return officeGradientStyle;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.Gradient.GradientStyle = value;
			}
		}
	}

	public OfficeGradientVariants GradientVariant
	{
		get
		{
			OfficeGradientVariants officeGradientVariants = OfficeGradientVariants.ShadingVariants_1;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					officeGradientVariants = range.CellStyle.Interior.Gradient.GradientVariant;
					flag = false;
				}
				else if (range.CellStyle.Interior.Gradient.GradientVariant != officeGradientVariants)
				{
					return OfficeGradientVariants.ShadingVariants_1;
				}
			}
			return officeGradientVariants;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Interior.Gradient.GradientVariant = value;
			}
		}
	}

	public GradientArrayWrapper(IRange range)
		: base((range as RangeImpl).Application, range)
	{
		m_arrCells.AddRange(range.Cells);
	}

	public int CompareTo(IGradient gradient)
	{
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			if (m_arrCells[i].CellStyle.Interior.Gradient.CompareTo(gradient) != 0)
			{
				return 1;
			}
		}
		return 0;
	}

	public void TwoColorGradient()
	{
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			m_arrCells[i].CellStyle.Interior.Gradient.TwoColorGradient();
		}
	}

	public void TwoColorGradient(OfficeGradientStyle style, OfficeGradientVariants variant)
	{
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			m_arrCells[i].CellStyle.Interior.Gradient.TwoColorGradient(style, variant);
		}
	}

	public void BeginUpdate()
	{
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			((GradientWrapper)m_arrCells[i].CellStyle.Interior.Gradient).BeginUpdate();
		}
	}

	public void EndUpdate()
	{
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			((GradientWrapper)m_arrCells[i].CellStyle.Interior.Gradient).EndUpdate();
		}
	}
}
