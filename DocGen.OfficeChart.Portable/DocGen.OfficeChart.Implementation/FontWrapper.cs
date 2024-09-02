using System;
using System.Collections;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class FontWrapper : CommonWrapper, IOfficeFont, IParentApplication, IOptimizedUpdate, IInternalFont
{
	private FontImpl m_font;

	private int m_charSet;

	private bool m_bReadOnly;

	private bool m_bRaiseEvents = true;

	private bool m_bDirectAccess;

	private ChartColor m_fontColor;

	private bool m_bIsAutoColor = true;

	private IRange m_range;

	public bool Bold
	{
		get
		{
			return m_font.Bold;
		}
		set
		{
			if (value != Bold)
			{
				BeginUpdate();
				m_font.Bold = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public OfficeKnownColors Color
	{
		get
		{
			return m_font.Color;
		}
		set
		{
			if (value != Color)
			{
				BeginUpdate();
				m_fontColor.SetIndexed(value);
				m_bIsAutoColor = false;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public Color RGBColor
	{
		get
		{
			return m_font.RGBColor;
		}
		set
		{
			if (value != RGBColor)
			{
				BeginUpdate();
				m_fontColor.SetRGB(value);
				m_bIsAutoColor = false;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public bool Italic
	{
		get
		{
			return m_font.Italic;
		}
		set
		{
			if (value != Italic)
			{
				BeginUpdate();
				m_font.Italic = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public bool MacOSOutlineFont
	{
		get
		{
			return m_font.MacOSOutlineFont;
		}
		set
		{
			if (value != MacOSOutlineFont)
			{
				BeginUpdate();
				m_font.MacOSOutlineFont = value;
				EndUpdate();
			}
		}
	}

	public bool MacOSShadow
	{
		get
		{
			return m_font.MacOSShadow;
		}
		set
		{
			if (value != MacOSShadow)
			{
				BeginUpdate();
				m_font.MacOSShadow = value;
				EndUpdate();
			}
		}
	}

	public double Size
	{
		get
		{
			return m_font.Size;
		}
		set
		{
			if (value != Size)
			{
				BeginUpdate();
				m_font.Size = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public bool Strikethrough
	{
		get
		{
			return m_font.Strikethrough;
		}
		set
		{
			if (value != Strikethrough)
			{
				BeginUpdate();
				m_font.Strikethrough = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public int Baseline
	{
		get
		{
			return m_font.BaseLine;
		}
		set
		{
			m_font.BaseLine = value;
		}
	}

	public bool Subscript
	{
		get
		{
			return m_font.Subscript;
		}
		set
		{
			if (value != Subscript)
			{
				BeginUpdate();
				m_font.Subscript = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public bool Superscript
	{
		get
		{
			return m_font.Superscript;
		}
		set
		{
			if (value != Superscript)
			{
				BeginUpdate();
				m_font.Superscript = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public OfficeUnderline Underline
	{
		get
		{
			return m_font.Underline;
		}
		set
		{
			if (value != Underline)
			{
				BeginUpdate();
				m_font.Underline = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public string FontName
	{
		get
		{
			return m_font.FontName;
		}
		set
		{
			if (value != FontName)
			{
				BeginUpdate();
				m_font.FontName = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	internal int CharSet
	{
		get
		{
			return m_charSet;
		}
		set
		{
			m_charSet = value;
		}
	}

	public OfficeFontVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_font.VerticalAlignment;
		}
		set
		{
			if (value != VerticalAlignment)
			{
				BeginUpdate();
				m_font.VerticalAlignment = value;
				if (Range != null)
				{
					(Range.RichText as RichTextString).UpdateRTF(Range, m_font);
				}
				EndUpdate();
			}
		}
	}

	public bool IsAutoColor
	{
		get
		{
			return m_bIsAutoColor;
		}
		set
		{
			m_bIsAutoColor = value;
		}
	}

	internal bool HasCapOrCharacterSpaceOrKerning
	{
		get
		{
			return m_font.HasCapOrCharacterSpaceOrKerning;
		}
		set
		{
			m_font.HasCapOrCharacterSpaceOrKerning = value;
		}
	}

	internal bool IsCapitalize
	{
		get
		{
			return m_font.IsCapitalize;
		}
		set
		{
			m_font.IsCapitalize = value;
		}
	}

	internal double CharacterSpacingValue
	{
		get
		{
			return m_font.CharacterSpacingValue;
		}
		set
		{
			m_font.CharacterSpacingValue = value;
		}
	}

	internal double KerningValue
	{
		get
		{
			return m_font.KerningValue;
		}
		set
		{
			m_font.KerningValue = value;
		}
	}

	public IApplication Application => m_font.Application;

	public object Parent => m_font.Parent;

	public int FontIndex => m_font.Index;

	public FontImpl Wrapped
	{
		get
		{
			return m_font;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_font = value;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return m_bReadOnly;
		}
		set
		{
			if (!value && m_bReadOnly)
			{
				throw new ArgumentOutOfRangeException("Can't change this property for read-only fonts");
			}
			m_bReadOnly = value;
		}
	}

	public WorkbookImpl Workbook => m_font.ParentWorkbook;

	public bool IsDirectAccess
	{
		get
		{
			return m_bDirectAccess;
		}
		set
		{
			m_bDirectAccess = value;
		}
	}

	public ChartColor ColorObject => m_fontColor;

	internal IRange Range
	{
		get
		{
			return m_range;
		}
		set
		{
			m_range = value;
		}
	}

	public int Index => m_font.Index;

	public FontImpl Font => m_font;

	public event EventHandler AfterChangeEvent;

	public FontWrapper()
	{
		m_fontColor = new ChartColor(ColorExtension.Black);
		m_fontColor.AfterChange += ColorObjectUpdate;
	}

	public FontWrapper(FontImpl font)
		: this()
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		m_font = font;
		m_fontColor.CopyFrom(font.ColorObject, callEvent: false);
	}

	public FontWrapper(FontImpl font, bool bReadOnly, bool bRaiseEvents)
		: this(font)
	{
		m_bReadOnly = bReadOnly;
		m_bRaiseEvents = bRaiseEvents;
	}

	public Font GenerateNativeFont()
	{
		return m_font.GenerateNativeFont();
	}

	public void ColorObjectUpdate()
	{
		BeginUpdate();
		m_font.ColorObject.CopyFrom(m_fontColor, callEvent: true);
		EndUpdate();
	}

	public FontWrapper Clone(WorkbookImpl book, object parent, IDictionary dicFontIndexes)
	{
		FontWrapper fontWrapper = new FontWrapper();
		int num = m_font.Index;
		if (dicFontIndexes != null)
		{
			num = (int)dicFontIndexes[num];
		}
		fontWrapper.m_bReadOnly = m_bReadOnly;
		fontWrapper.m_font = ((FontImpl)book.InnerFonts[num]).Clone(book);
		fontWrapper.m_bIsAutoColor = m_bIsAutoColor;
		fontWrapper.m_font.Index = num;
		return fontWrapper;
	}

	public override void BeginUpdate()
	{
		if (base.BeginCallsCount == 0)
		{
			if (m_bReadOnly)
			{
				throw new ReadOnlyException();
			}
			if (!m_bRaiseEvents)
			{
				return;
			}
			if (!m_bDirectAccess)
			{
				m_font = (FontImpl)Workbook.CreateFont(m_font, bAddToCollection: false);
			}
		}
		base.BeginUpdate();
	}

	public override void EndUpdate()
	{
		base.EndUpdate();
		if (base.BeginCallsCount == 0 && m_bRaiseEvents)
		{
			WorkbookImpl workbook = Workbook;
			if (!m_bDirectAccess)
			{
				m_font = (FontImpl)workbook.AddFont(m_font);
			}
			workbook.SetChanged();
			if (this.AfterChangeEvent != null)
			{
				this.AfterChangeEvent(this, EventArgs.Empty);
			}
		}
	}

	internal void InvokeAfterChange()
	{
		if (this.AfterChangeEvent != null)
		{
			this.AfterChangeEvent(this, EventArgs.Empty);
		}
	}

	internal void Dispose()
	{
		this.AfterChangeEvent = null;
		m_fontColor.Dispose();
		m_font.Clear();
		m_font = null;
		m_fontColor = null;
	}
}
