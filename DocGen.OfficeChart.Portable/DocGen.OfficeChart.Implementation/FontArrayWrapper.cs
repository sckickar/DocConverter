using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class FontArrayWrapper : CommonObject, IOfficeFont, IParentApplication, IOptimizedUpdate
{
	private List<IRange> m_arrCells = new List<IRange>();

	public bool Italic
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag2)
				{
					flag = range.CellStyle.Font.Italic;
					flag2 = false;
				}
				else if (range.CellStyle.Font.Italic != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.Italic = value;
			}
		}
	}

	public OfficeKnownColors Color
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
					officeKnownColors = range.CellStyle.Font.Color;
					flag = false;
				}
				else if (range.CellStyle.Font.Color != officeKnownColors)
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
				m_arrCells[i].CellStyle.Font.Color = value;
			}
		}
	}

	public Color RGBColor
	{
		get
		{
			OfficeKnownColors color = Color;
			return m_arrCells[0].Worksheet.Workbook.GetPaletteColor(color);
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.RGBColor = value;
			}
		}
	}

	public bool Bold
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag2)
				{
					flag = range.CellStyle.Font.Bold;
					flag2 = false;
				}
				else if (range.CellStyle.Font.Bold != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.Bold = value;
			}
		}
	}

	public bool MacOSOutlineFont
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag2)
				{
					flag = range.CellStyle.Font.MacOSOutlineFont;
					flag2 = false;
				}
				else if (range.CellStyle.Font.MacOSOutlineFont != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.MacOSOutlineFont = value;
			}
		}
	}

	public bool MacOSShadow
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag2)
				{
					flag = range.CellStyle.Font.MacOSShadow;
					flag2 = false;
				}
				else if (range.CellStyle.Font.MacOSShadow != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.MacOSShadow = value;
			}
		}
	}

	public double Size
	{
		get
		{
			double num = 0.0;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					num = range.CellStyle.Font.Size;
					flag = false;
				}
				else if (range.CellStyle.Font.Size != num)
				{
					return 0.0;
				}
			}
			return num;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.Size = value;
			}
		}
	}

	public bool Strikethrough
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag2)
				{
					flag = range.CellStyle.Font.Strikethrough;
					flag2 = false;
				}
				else if (range.CellStyle.Font.Strikethrough != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.Strikethrough = value;
			}
		}
	}

	public bool Subscript
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag2)
				{
					flag = range.CellStyle.Font.Subscript;
					flag2 = false;
				}
				else if (range.CellStyle.Font.Subscript != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.Subscript = value;
			}
		}
	}

	public bool Superscript
	{
		get
		{
			bool flag = false;
			bool flag2 = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag2)
				{
					flag = range.CellStyle.Font.Superscript;
					flag2 = false;
				}
				else if (range.CellStyle.Font.Superscript != flag)
				{
					return false;
				}
			}
			return flag;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.Superscript = value;
			}
		}
	}

	public OfficeUnderline Underline
	{
		get
		{
			OfficeUnderline officeUnderline = OfficeUnderline.None;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					officeUnderline = range.CellStyle.Font.Underline;
					flag = false;
				}
				else if (range.CellStyle.Font.Underline != officeUnderline)
				{
					return OfficeUnderline.None;
				}
			}
			return officeUnderline;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.Underline = value;
			}
		}
	}

	public string FontName
	{
		get
		{
			string text = null;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					text = range.CellStyle.Font.FontName;
					flag = false;
				}
				else if (range.CellStyle.Font.FontName != text)
				{
					return null;
				}
			}
			return text;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.FontName = value;
			}
		}
	}

	public OfficeFontVerticalAlignment VerticalAlignment
	{
		get
		{
			OfficeFontVerticalAlignment officeFontVerticalAlignment = OfficeFontVerticalAlignment.Baseline;
			bool flag = true;
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				IRange range = m_arrCells[i];
				if (flag)
				{
					officeFontVerticalAlignment = range.CellStyle.Font.VerticalAlignment;
					flag = false;
				}
				else if (range.CellStyle.Font.VerticalAlignment != officeFontVerticalAlignment)
				{
					officeFontVerticalAlignment = OfficeFontVerticalAlignment.Baseline;
					break;
				}
			}
			return officeFontVerticalAlignment;
		}
		set
		{
			int i = 0;
			for (int count = m_arrCells.Count; i < count; i++)
			{
				m_arrCells[i].CellStyle.Font.VerticalAlignment = value;
			}
		}
	}

	public bool IsAutoColor => false;

	public FontArrayWrapper(IRange range)
		: base((range as RangeImpl).Application, range)
	{
		m_arrCells.AddRange(range.Cells);
	}

	public FontArrayWrapper(List<IRange> lstRange, IApplication application)
		: base(application, lstRange[0])
	{
		m_arrCells = lstRange;
	}

	public Font GenerateNativeFont()
	{
		return m_arrCells[0].CellStyle.Font.GenerateNativeFont();
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}
}
