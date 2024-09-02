using System;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.Collections.Grouping;

internal class FontGroup : CommonObject, IOfficeFont, IParentApplication, IOptimizedUpdate
{
	private StyleGroup m_styleGroup;

	public IOfficeFont this[int index] => m_styleGroup[index].Font;

	public int Count => m_styleGroup.Count;

	public bool Bold
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool bold = this[0].Bold;
			for (int i = 1; i < count; i++)
			{
				if (bold != this[i].Bold)
				{
					return false;
				}
			}
			return bold;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Bold = value;
			}
		}
	}

	public OfficeKnownColors Color
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeKnownColors.Black;
			}
			OfficeKnownColors color = this[0].Color;
			for (int i = 1; i < count; i++)
			{
				if (color != this[i].Color)
				{
					return OfficeKnownColors.Black;
				}
			}
			return color;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Color = value;
			}
		}
	}

	public Color RGBColor
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return ColorExtension.Empty;
			}
			Color rGBColor = this[0].RGBColor;
			for (int i = 1; i < count; i++)
			{
				if (rGBColor != this[i].RGBColor)
				{
					return ColorExtension.Empty;
				}
			}
			return rGBColor;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].RGBColor = value;
			}
		}
	}

	public bool Italic
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool italic = this[0].Italic;
			for (int i = 1; i < count; i++)
			{
				if (italic != this[i].Italic)
				{
					return false;
				}
			}
			return italic;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Italic = value;
			}
		}
	}

	public bool MacOSOutlineFont
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool macOSOutlineFont = this[0].MacOSOutlineFont;
			for (int i = 1; i < count; i++)
			{
				if (macOSOutlineFont != this[i].MacOSOutlineFont)
				{
					return false;
				}
			}
			return macOSOutlineFont;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].MacOSOutlineFont = value;
			}
		}
	}

	public bool MacOSShadow
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool macOSShadow = this[0].MacOSShadow;
			for (int i = 1; i < count; i++)
			{
				if (macOSShadow != this[i].MacOSShadow)
				{
					return false;
				}
			}
			return macOSShadow;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].MacOSShadow = value;
			}
		}
	}

	public double Size
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return -2147483648.0;
			}
			double size = this[0].Size;
			for (int i = 1; i < count; i++)
			{
				if (size != this[i].Size)
				{
					return double.MinValue;
				}
			}
			return size;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Size = value;
			}
		}
	}

	public bool Strikethrough
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool strikethrough = this[0].Strikethrough;
			for (int i = 1; i < count; i++)
			{
				if (strikethrough != this[i].Strikethrough)
				{
					return false;
				}
			}
			return strikethrough;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Strikethrough = value;
			}
		}
	}

	public bool Subscript
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool subscript = this[0].Subscript;
			for (int i = 1; i < count; i++)
			{
				if (subscript != this[i].Subscript)
				{
					return false;
				}
			}
			return subscript;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Subscript = value;
			}
		}
	}

	public bool Superscript
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool superscript = this[0].Superscript;
			for (int i = 1; i < count; i++)
			{
				if (superscript != this[i].Superscript)
				{
					return false;
				}
			}
			return superscript;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Superscript = value;
			}
		}
	}

	public OfficeUnderline Underline
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeUnderline.None;
			}
			OfficeUnderline underline = this[0].Underline;
			for (int i = 1; i < count; i++)
			{
				if (underline != this[i].Underline)
				{
					return OfficeUnderline.None;
				}
			}
			return underline;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Underline = value;
			}
		}
	}

	public string FontName
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return null;
			}
			string fontName = this[0].FontName;
			for (int i = 1; i < count; i++)
			{
				if (fontName != this[i].FontName)
				{
					return null;
				}
			}
			return fontName;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].FontName = value;
			}
		}
	}

	public OfficeFontVerticalAlignment VerticalAlignment
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return OfficeFontVerticalAlignment.Baseline;
			}
			OfficeFontVerticalAlignment verticalAlignment = this[0].VerticalAlignment;
			for (int i = 1; i < count; i++)
			{
				if (verticalAlignment != this[i].VerticalAlignment)
				{
					return OfficeFontVerticalAlignment.Baseline;
				}
			}
			return verticalAlignment;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].VerticalAlignment = value;
			}
		}
	}

	public bool IsAutoColor => false;

	public FontGroup(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
	}

	private void FindParents()
	{
		m_styleGroup = FindParent(typeof(StyleGroup)) as StyleGroup;
		if (m_styleGroup == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent style group.");
		}
	}

	public Font GenerateNativeFont()
	{
		throw new NotSupportedException();
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}
}
