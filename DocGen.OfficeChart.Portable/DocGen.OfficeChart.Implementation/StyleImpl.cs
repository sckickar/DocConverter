using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class StyleImpl : ExtendedFormatWrapper, IStyle, IExtendedFormat, IParentApplication, IOptimizedUpdate, IComparable, INamedObject
{
	internal class StyleSettings
	{
		public FillImpl Fill;

		public FontSettings Font;

		public BorderSettings Borders;

		public StyleSettings(FillImpl fill, FontSettings font)
			: this(fill, font, null)
		{
		}

		public StyleSettings(FillImpl fill, FontSettings font, BorderSettings borders)
		{
			Fill = fill;
			Font = font;
			Borders = borders;
		}

		internal void Clear()
		{
			if (Fill != null)
			{
				Fill.Dispose();
			}
			if (Font != null)
			{
				Font.Dispose();
			}
			if (Borders != null)
			{
				Borders.Dispose();
			}
			Fill = null;
			Font = null;
			Borders = null;
		}
	}

	internal class FontSettings
	{
		public ChartColor Color;

		public int Size;

		public bool Bold;

		public bool Italic;

		public string Name;

		public FontSettings(ChartColor color)
			: this(color, 11)
		{
		}

		public FontSettings(ChartColor color, int size)
			: this(color, size, FontStyle.Regular)
		{
		}

		public FontSettings(ChartColor color, FontStyle fontStyle)
			: this(color, 11, fontStyle)
		{
		}

		public FontSettings(ChartColor color, int size, FontStyle fontStyle)
			: this(color, size, fontStyle, null)
		{
		}

		public FontSettings(ChartColor color, int size, FontStyle fontStyle, string name)
		{
			Color = color;
			Size = size;
			Bold = (fontStyle & FontStyle.Bold) != 0;
			Italic = (fontStyle & FontStyle.Italic) != 0;
			Name = name;
		}

		internal void Dispose()
		{
			Color.Dispose();
			Color = null;
		}
	}

	internal class BorderSettings
	{
		public ChartColor BorderColor;

		public OfficeLineStyle Left;

		public OfficeLineStyle Right;

		public OfficeLineStyle Top;

		public OfficeLineStyle Bottom;

		public BorderSettings(ChartColor color, OfficeLineStyle lineStyle)
		{
			BorderColor = color;
			Left = (Right = (Top = (Bottom = lineStyle)));
		}

		public BorderSettings(ChartColor color, OfficeLineStyle left, OfficeLineStyle right, OfficeLineStyle top, OfficeLineStyle bottom)
		{
			BorderColor = color;
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
		}

		internal void Dispose()
		{
			BorderColor.Dispose();
			BorderColor = null;
		}
	}

	[Flags]
	private enum StyleOptions
	{
		None = 0,
		UpdateStyleXF = 1,
		Temporary = 2
	}

	private const int Excel2007StylesStart = 10;

	public const int DEF_LESS = -1;

	public const int DEF_EQUAL = 0;

	public const int DEF_LARGER = 1;

	private const int RowLevelStyleIndex = 1;

	private const int ColumnLevelStyleIndex = 2;

	private StyleRecord m_style;

	private StyleExtRecord m_styleExt;

	private bool m_bNotCompareName;

	public new bool HasBorder
	{
		get
		{
			throw new ArgumentException("No need to implement");
		}
	}

	private string[] DefaultStyleNames => m_book.AppImplementation.DefaultStyleNames;

	public new bool BuiltIn => m_style.IsBuildInStyle;

	public new string Name
	{
		get
		{
			string text = null;
			bool flag = !BuiltIn;
			if (!flag)
			{
				int buildInOrNameLen = m_style.BuildInOrNameLen;
				text = DefaultStyleNames[buildInOrNameLen];
				if (buildInOrNameLen == 1 || buildInOrNameLen == 2)
				{
					text += m_style.OutlineStyleLevel + 1;
				}
				flag = text == null || text.Length == 0;
			}
			if (flag && m_style.StyleName != null)
			{
				text = m_style.StyleName;
			}
			return text;
		}
	}

	internal string StyleNameCache => Record.StyleNameCache;

	public new bool IsInitialized
	{
		get
		{
			string text = DefaultStyleNames[0];
			if (!(Name == text))
			{
				return !StylesCollection.CompareStyles(this, m_book.Styles[text]);
			}
			return false;
		}
	}

	public int Index => m_xFormat.Index;

	public bool NotCompareNames
	{
		get
		{
			return m_bNotCompareName;
		}
		set
		{
			m_bNotCompareName = value;
		}
	}

	[CLSCompliant(false)]
	public StyleRecord Record
	{
		get
		{
			UpdateStyleRecord();
			return m_style;
		}
	}

	public bool IsBuiltInCustomized
	{
		get
		{
			return Record.IsBuiltIncustomized;
		}
		set
		{
			Record.IsBuiltIncustomized = value;
		}
	}

	internal bool IsAsciiConverted
	{
		get
		{
			return Record.IsAsciiConverted;
		}
		set
		{
			Record.IsAsciiConverted = value;
		}
	}

	internal StyleExtRecord StyleExt
	{
		get
		{
			return m_styleExt;
		}
		set
		{
			m_styleExt = value;
		}
	}

	public event EventHandler BeforeChange;

	public event EventHandler AfterChange;

	public StyleImpl(WorkbookImpl book)
		: base(book)
	{
		m_style = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		SetFormatIndex(m_style.ExtendedFormatIndex);
	}

	[CLSCompliant(false)]
	public StyleImpl(WorkbookImpl book, StyleRecord style)
		: base(book)
	{
		m_style = style;
		SetFormatIndex(m_style.ExtendedFormatIndex);
		if (style.IsBuildInStyle && style.BuildInOrNameLen == 0)
		{
			m_font.IsDirectAccess = true;
		}
	}

	public StyleImpl(WorkbookImpl book, string strName)
		: this(book, strName, null)
	{
	}

	public StyleImpl(WorkbookImpl book, string strName, StyleImpl baseStyle)
		: this(book, strName, baseStyle, bIsBuiltIn: false)
	{
	}

	public StyleImpl(WorkbookImpl book, string strName, StyleImpl baseStyle, bool bIsBuiltIn)
		: this(book)
	{
		if (baseStyle != null)
		{
			StyleRecord style = baseStyle.m_style;
			m_style = style.Clone() as StyleRecord;
		}
		else
		{
			m_style = (StyleRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Style);
		}
		int num = -1;
		if (bIsBuiltIn)
		{
			num = Array.IndexOf(DefaultStyleNames, strName);
			if (num < 0)
			{
				throw new ArgumentOutOfRangeException("name", "Can't find built in name");
			}
			m_style.BuildInOrNameLen = (byte)num;
		}
		else
		{
			m_style.StyleName = strName;
		}
		m_style.IsBuildInStyle = bIsBuiltIn;
		ExtendedFormatImpl extendedFormatImpl = ((baseStyle != null) ? ((ExtendedFormatImpl)m_book.CreateExtFormat(baseStyle.Wrapped, bForceAdd: true)) : ((ExtendedFormatImpl)m_book.CreateExtFormat(bForceAdd: true)));
		extendedFormatImpl.ParentIndex = m_book.MaxXFCount;
		extendedFormatImpl.XFType = ExtendedFormatRecord.TXFType.XF_CELL;
		m_style.ExtendedFormatIndex = (ushort)extendedFormatImpl.Index;
		m_style.IsBuildInStyle = bIsBuiltIn;
		SetFormatIndex(extendedFormatImpl.Index);
		if (bIsBuiltIn && m_book.Version != 0 && num >= 10)
		{
			CopyDefaultStyleSettings(num);
		}
	}

	private void CopyDefaultStyleSettings(int index)
	{
		StyleSettings obj = (base.Application as ApplicationImpl).BuiltInStyleInfo[index];
		FillImpl fill = obj.Fill;
		if (fill != null)
		{
			Excel2007Parser.CopyFillSettings(fill, m_xFormat);
		}
		FontSettings font = obj.Font;
		if (font != null)
		{
			CopyFontSettings(font, m_font);
		}
		BorderSettings borders = obj.Borders;
		if (borders != null)
		{
			CopyBordersSettings(borders, m_xFormat);
		}
	}

	private void CopyBordersSettings(BorderSettings borders, ExtendedFormatImpl m_xFormat)
	{
		ChartColor borderColor = borders.BorderColor;
		if (borders.Left != 0)
		{
			m_xFormat.LeftBorderLineStyle = borders.Left;
			if (borderColor != null)
			{
				m_xFormat.LeftBorderColor.CopyFrom(borderColor, callEvent: true);
			}
		}
		if (borders.Right != 0)
		{
			m_xFormat.RightBorderLineStyle = borders.Right;
			if (borderColor != null)
			{
				m_xFormat.RightBorderColor.CopyFrom(borderColor, callEvent: true);
			}
		}
		if (borders.Top != 0)
		{
			m_xFormat.TopBorderLineStyle = borders.Top;
			if (borderColor != null)
			{
				m_xFormat.TopBorderColor.CopyFrom(borderColor, callEvent: true);
			}
		}
		if (borders.Bottom != 0)
		{
			m_xFormat.BottomBorderLineStyle = borders.Bottom;
			if (borderColor != null)
			{
				m_xFormat.BottomBorderColor.CopyFrom(borderColor, callEvent: true);
			}
		}
	}

	private void CopyFontSettings(FontSettings font, FontWrapper m_font)
	{
		ChartColor color = font.Color;
		m_font.BeginUpdate();
		if (color != null)
		{
			m_font.ColorObject.CopyFrom(color, callEvent: true);
		}
		m_font.Size = font.Size;
		m_font.Italic = font.Italic;
		m_font.Bold = font.Bold;
		string name = font.Name;
		if (name != null)
		{
			m_font.FontName = name;
		}
		m_font.EndUpdate();
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add(m_style);
	}

	public void UpdateStyleRecord()
	{
		m_style.ExtendedFormatIndex = (ushort)m_xFormat.Index;
	}

	public override void EndUpdate()
	{
		base.EndUpdate();
		if (base.BeginCallsCount == 0 && this.AfterChange != null)
		{
			this.AfterChange(this, EventArgs.Empty);
		}
		List<int> list = FindChildXFs();
		ExtendedFormatsCollection innerExtFormats = m_book.InnerExtFormats;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			int index = list[i];
			innerExtFormats[index].SynchronizeWithParent();
		}
	}

	public override void BeginUpdate()
	{
		if (base.BeginCallsCount == 0 && this.BeforeChange != null)
		{
			this.BeforeChange(this, EventArgs.Empty);
		}
		base.BeginUpdate();
	}

	public override object Clone(object parent)
	{
		StyleImpl obj = (StyleImpl)base.Clone(parent);
		obj.m_style = (StyleRecord)CloneUtils.CloneCloneable(m_style);
		return obj;
	}

	private List<int> FindChildXFs()
	{
		List<int> list = new List<int>();
		ExtendedFormatsCollection innerExtFormats = m_book.InnerExtFormats;
		int index = Index;
		int i = 0;
		for (int count = innerExtFormats.Count; i < count; i++)
		{
			if (innerExtFormats[i].ParentIndex == index)
			{
				list.Add(i);
			}
		}
		return list;
	}

	public int CompareTo(object obj)
	{
		if (!(obj is StyleImpl styleImpl))
		{
			return 1;
		}
		int num = m_font.Wrapped.CompareTo(styleImpl.m_font.Wrapped);
		if (num != 0)
		{
			return num;
		}
		num = m_xFormat.CompareToWithoutIndex(styleImpl.m_xFormat);
		if (num != 0)
		{
			return num;
		}
		if (!m_bNotCompareName && !styleImpl.m_bNotCompareName)
		{
			return Name.CompareTo(styleImpl.Name);
		}
		return num;
	}

	internal new void Dispose()
	{
		this.AfterChange = null;
		this.BeforeChange = null;
		m_style = null;
		base.Dispose();
	}
}
