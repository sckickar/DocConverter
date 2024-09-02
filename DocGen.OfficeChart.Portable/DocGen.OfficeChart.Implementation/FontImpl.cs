using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class FontImpl : CommonObject, IOfficeFont, IParentApplication, IOptimizedUpdate, ICloneable, IComparable, IInternalFont, ICloneParent, IDisposable
{
	internal const ushort FONTBOLD = 700;

	internal const ushort FONTNORMAL = 400;

	private const int DEF_INCORRECT_INDEX = -1;

	internal const int DEF_BAD_INDEX = 4;

	private const float DEF_SMALL_BOLD_FONT_MULTIPLIER = 1.15f;

	private const float DEF_BOLD_FONT_MULTIPLIER = 1.07f;

	private const int DEF_INDEX = 64;

	private FontRecord m_font;

	private WorkbookImpl m_book;

	private int m_index = -1;

	private byte m_btCharSet = 1;

	private Font m_fontNative;

	private ChartColor m_color;

	private string m_strLanguage;

	private string m_scheme;

	private int m_family;

	private bool m_hasLatin;

	private bool m_hasComplexScripts;

	private bool m_hasEastAsianFont;

	private string m_actualFont;

	private string[] m_arrItalicFonts = new string[1] { "Brush Script MT" };

	internal TextSettings m_textSettings;

	private Dictionary<string, Stream> _preservedElements;

	private Excel2007CommentHAlign m_paraAlign = Excel2007CommentHAlign.l;

	private bool m_bHasParaAlign;

	internal bool showFontName = true;

	public bool Bold
	{
		get
		{
			return m_font.BoldWeight >= 700;
		}
		set
		{
			if (!value)
			{
				if (m_font.IsItalic)
				{
					new Font(m_font.FontName, 10f, FontStyle.Italic, GraphicsUnit.Pixel, CharSet);
				}
				else
				{
					new Font(m_font.FontName, 10f, FontStyle.Regular, GraphicsUnit.Pixel, CharSet);
				}
			}
			if (value != Bold)
			{
				m_font.BoldWeight = (ushort)(value ? 700 : 400);
				SetChanged();
			}
		}
	}

	public OfficeKnownColors Color
	{
		get
		{
			return m_color.GetIndexed(m_book);
		}
		set
		{
			m_color.SetIndexed(value);
		}
	}

	public Color RGBColor
	{
		get
		{
			return m_color.GetRGB(m_book);
		}
		set
		{
			m_color.SetRGB(value, m_book);
		}
	}

	public bool Italic
	{
		get
		{
			return m_font.IsItalic;
		}
		set
		{
			if (!value)
			{
				new Font(m_font.FontName, 10f, FontStyle.Bold, GraphicsUnit.Pixel, CharSet);
			}
			else
			{
				FontStyle supportedFontStyle = GetSupportedFontStyle(m_font.FontName);
				new Font(m_font.FontName, 10f, supportedFontStyle, GraphicsUnit.Pixel, CharSet);
			}
			if (m_font.IsItalic != value)
			{
				m_font.IsItalic = value;
				SetChanged();
			}
		}
	}

	public bool MacOSOutlineFont
	{
		get
		{
			return m_font.IsMacOutline;
		}
		set
		{
			m_font.IsMacOutline = value;
			SetChanged();
		}
	}

	public bool MacOSShadow
	{
		get
		{
			return m_font.IsMacShadow;
		}
		set
		{
			m_font.IsMacShadow = value;
			SetChanged();
		}
	}

	public double Size
	{
		get
		{
			return (double)(int)m_font.FontHeight / 20.0;
		}
		set
		{
			if (value < 1.0 || value > 409.0)
			{
				throw new ArgumentOutOfRangeException("Font.Size", "Font.Size out of range. Size must be less then 409 and greater than 1.");
			}
			ushort num = (ushort)(value * 20.0);
			if (m_font.FontHeight != num)
			{
				m_font.FontHeight = (ushort)(value * 20.0);
				SetChanged();
			}
		}
	}

	public bool Strikethrough
	{
		get
		{
			return m_font.IsStrikeout;
		}
		set
		{
			m_font.IsStrikeout = value;
			SetChanged();
		}
	}

	public bool Subscript
	{
		get
		{
			return m_font.SuperSubscript == OfficeFontVerticalAlignment.Subscript;
		}
		set
		{
			if (value == Subscript)
			{
				return;
			}
			if (value)
			{
				m_font.SuperSubscript = OfficeFontVerticalAlignment.Subscript;
				if (BaseLine >= 0)
				{
					BaseLine = -25000;
				}
			}
			else if (m_font.SuperSubscript == OfficeFontVerticalAlignment.Subscript)
			{
				m_font.SuperSubscript = OfficeFontVerticalAlignment.Baseline;
				if (BaseLine < 0)
				{
					BaseLine = 0;
				}
			}
			SetChanged();
		}
	}

	public bool Superscript
	{
		get
		{
			return m_font.SuperSubscript == OfficeFontVerticalAlignment.Superscript;
		}
		set
		{
			if (value == Superscript)
			{
				return;
			}
			if (value)
			{
				m_font.SuperSubscript = OfficeFontVerticalAlignment.Superscript;
				if (BaseLine <= 0)
				{
					BaseLine = 30000;
				}
			}
			else if (m_font.SuperSubscript == OfficeFontVerticalAlignment.Superscript)
			{
				m_font.SuperSubscript = OfficeFontVerticalAlignment.Baseline;
				if (BaseLine > 0)
				{
					BaseLine = 0;
				}
			}
			SetChanged();
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
			m_font.Underline = value;
			SetChanged();
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
			if (value != m_font.FontName)
			{
				m_font.FontName = value;
				SetChanged();
				switch (GetSupportedFontStyle(m_font.FontName))
				{
				case FontStyle.Bold:
					m_font.BoldWeight = 700;
					SetChanged();
					break;
				case FontStyle.Italic:
					m_font.IsItalic = true;
					SetChanged();
					break;
				}
			}
		}
	}

	public OfficeFontVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_font.SuperSubscript;
		}
		set
		{
			m_font.SuperSubscript = value;
		}
	}

	public bool IsAutoColor => false;

	internal int BaseLine
	{
		get
		{
			return m_font.Baseline;
		}
		set
		{
			m_font.Baseline = value;
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
			SetChanged();
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
			SetChanged();
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
			SetChanged();
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
			SetChanged();
		}
	}

	[CLSCompliant(false)]
	public FontRecord Record => m_font;

	internal WorkbookImpl ParentWorkbook
	{
		get
		{
			if (m_book == null)
			{
				m_book = (WorkbookImpl)FindParent(typeof(WorkbookImpl));
			}
			return m_book;
		}
	}

	internal int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			if (m_index != value)
			{
				m_index = value;
			}
		}
	}

	public byte CharSet
	{
		get
		{
			return m_font.Charset;
		}
		set
		{
			m_font.Charset = value;
		}
	}

	internal Dictionary<string, Stream> PreservedElements => _preservedElements ?? (_preservedElements = new Dictionary<string, Stream>());

	internal byte Family
	{
		get
		{
			return m_font.Family;
		}
		set
		{
			m_font.Family = value;
		}
	}

	public ChartColor ColorObject => m_color;

	public string Language
	{
		get
		{
			return m_strLanguage;
		}
		set
		{
			m_strLanguage = value;
		}
	}

	internal bool HasLatin
	{
		get
		{
			return m_hasLatin;
		}
		set
		{
			m_hasLatin = value;
		}
	}

	internal bool HasComplexScripts
	{
		get
		{
			return m_hasComplexScripts;
		}
		set
		{
			m_hasComplexScripts = value;
		}
	}

	internal bool HasEastAsianFont
	{
		get
		{
			return m_hasEastAsianFont;
		}
		set
		{
			m_hasEastAsianFont = value;
		}
	}

	internal Excel2007CommentHAlign ParaAlign
	{
		get
		{
			return m_paraAlign;
		}
		set
		{
			m_paraAlign = value;
		}
	}

	internal bool HasParagrapAlign
	{
		get
		{
			return m_bHasParaAlign;
		}
		set
		{
			m_bHasParaAlign = value;
		}
	}

	int IInternalFont.Index => Index;

	public FontImpl Font => this;

	internal string ActualFontName
	{
		get
		{
			return m_actualFont;
		}
		set
		{
			m_actualFont = value;
		}
	}

	internal event ValueChangedEventHandler IndexChanged;

	public FontImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_font = (FontRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Font);
		m_font.FontName = base.AppImplementation.StandardFont;
		m_font.FontHeight = (ushort)SizeInTwips(base.AppImplementation.StandardFontSize);
		InitializeColor();
		InitializeParent();
	}

	[CLSCompliant(false)]
	public FontImpl(IApplication application, object parent, BiffReader reader)
		: this(application, parent)
	{
		Parse(reader);
	}

	[CLSCompliant(false)]
	public FontImpl(IApplication application, object parent, FontRecord record)
		: this(application, parent)
	{
		m_font = record;
		UpdateColor();
	}

	[CLSCompliant(false)]
	public FontImpl(IApplication application, object parent, FontImpl font)
		: this(application, parent)
	{
		m_font = font.Record;
		if (font != null)
		{
			m_color = font.ColorObject;
		}
		UpdateColor();
	}

	public FontImpl(IOfficeFont baseFont)
		: this((baseFont is FontImpl) ? (baseFont as FontImpl).ParentWorkbook.Application : (baseFont as FontWrapper).Workbook.Application, baseFont.Parent)
	{
		if (baseFont is FontImpl)
		{
			m_font = (FontRecord)((FontImpl)baseFont).Record.Clone();
		}
		else
		{
			if (!(baseFont is FontWrapper))
			{
				throw new ArgumentException("baseFont must be FontImpl or FontWrapper class instance");
			}
			m_font = (FontRecord)((FontWrapper)baseFont).Wrapped.Record.Clone();
		}
		UpdateColor();
	}

	private void InitializeColor()
	{
		m_color = new ChartColor((OfficeKnownColors)m_font.PaletteColorIndex);
		m_color.AfterChange += UpdateRecord;
	}

	internal void UpdateRecord()
	{
		m_font.PaletteColorIndex = (ushort)m_color.GetIndexed(m_book);
		SetChanged();
	}

	private void UpdateColor()
	{
		if (m_color.ColorType == ColorType.RGB || m_color.ColorType == ColorType.Theme)
		{
			m_color.SetRGB(m_color.GetRGB(m_book));
		}
		else
		{
			m_color.SetIndexed((OfficeKnownColors)m_font.PaletteColorIndex);
		}
	}

	private void InitializeParent()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentException("Cannot find parent workbook.");
		}
	}

	private void Parse(BiffReader reader)
	{
		if (reader.IsEOF)
		{
			throw new ApplicationException("Reached end of stream. Font object cannot be initialized.");
		}
		BiffRecordRaw record = reader.GetRecord();
		if (record.TypeCode != TBIFFRecord.Font)
		{
			throw new ApplicationException("Record extracted from stream is not a Font Record");
		}
		m_font = (FontRecord)record;
		UpdateColor();
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		records.Add(m_font);
	}

	public void CopyTo(FontImpl twin)
	{
		m_font.CopyTo(twin.m_font);
		twin.CharSet = CharSet;
	}

	public void SetChanged()
	{
		ParentWorkbook.Saved = false;
		m_fontNative = null;
	}

	public Font GenerateNativeFont()
	{
		if (m_fontNative == null)
		{
			m_fontNative = GenerateNativeFont((float)Size);
		}
		return m_fontNative;
	}

	public Font GenerateNativeFont(float size)
	{
		FontStyle fontStyle = FontStyle.Regular;
		switch (GetSupportedFontStyle(m_font.FontName))
		{
		case FontStyle.Bold:
			Bold = true;
			break;
		case FontStyle.Italic:
			Italic = true;
			break;
		}
		if (Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (Strikethrough)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (Underline != 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		if (Array.IndexOf(m_arrItalicFonts, FontName) >= 0)
		{
			fontStyle |= FontStyle.Italic;
		}
		return new Font(FontName, size, fontStyle, GraphicsUnit.Point, m_btCharSet);
	}

	public void Parse(Font nativeFont)
	{
		if (nativeFont == null)
		{
			throw new ArgumentNullException("nativeFont");
		}
		FontName = nativeFont.Name;
		Size = (int)nativeFont.Size;
		Strikethrough = nativeFont.Strikeout;
		Bold = nativeFont.Bold;
		Italic = nativeFont.Italic;
		Underline = (nativeFont.Underline ? OfficeUnderline.Single : OfficeUnderline.None);
		UpdateColor();
	}

	public SizeF MeasureString(string strValue)
	{
		Size size = base.AppImplementation.MeasureString(strValue, this, new SizeF(2.1474836E+09f, 2.1474836E+09f)).ToSize();
		return new SizeF(size.Width, size.Height - 1);
	}

	public SizeF MeasureStringSpecial(string strValue)
	{
		return new SizeF(0f, 0f);
	}

	public SizeF MeasureCharacter(char value)
	{
		return new SizeF(0f, 0f);
	}

	private void RaiseIndexChangedEvent(ValueChangedEventArgs args)
	{
		if (this.IndexChanged != null)
		{
			this.IndexChanged(this, args);
		}
	}

	public FontImpl TypedClone()
	{
		FontImpl obj = MemberwiseClone() as FontImpl;
		obj.m_font = m_font.Clone() as FontRecord;
		return obj;
	}

	public object Clone()
	{
		return TypedClone();
	}

	public FontImpl Clone(object parent)
	{
		FontImpl fontImpl = new FontImpl(base.Application, parent);
		fontImpl.HasLatin = HasLatin;
		fontImpl.m_bIsDisposed = m_bIsDisposed;
		fontImpl.m_index = -1;
		fontImpl.m_font = (FontRecord)m_font.Clone();
		if (m_color != null && m_color.ColorType != ColorType.Indexed)
		{
			fontImpl.m_color = m_color.Clone();
		}
		else
		{
			fontImpl.m_color = new ChartColor((OfficeKnownColors)fontImpl.m_font.PaletteColorIndex);
		}
		fontImpl.m_color.AfterChange += UpdateRecord;
		fontImpl.IsCapitalize = IsCapitalize;
		fontImpl.CharacterSpacingValue = CharacterSpacingValue;
		fontImpl.KerningValue = KerningValue;
		return fontImpl;
	}

	public static int SizeInTwips(double fontSize)
	{
		return (int)(fontSize * 20.0);
	}

	public static double SizeInPoints(int twipsSize)
	{
		return (float)twipsSize / 20f;
	}

	public static int UpdateFontIndexes(int iOldIndex, Dictionary<int, int> dicNewIndexes, OfficeParseOptions options)
	{
		int value = iOldIndex;
		dicNewIndexes?.TryGetValue(iOldIndex, out value);
		return value;
	}

	public FontStyle GetSupportedFontStyle(string fontName)
	{
		FontStyle[] array = new FontStyle[3]
		{
			FontStyle.Regular,
			FontStyle.Bold,
			FontStyle.Italic
		};
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				new Font(FontName, 10f, array[i], GraphicsUnit.Pixel, 1);
				return array[i];
			}
			catch (Exception)
			{
			}
		}
		return FontStyle.Regular;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is FontImpl fontImpl))
		{
			return false;
		}
		if (GetHashCode() != fontImpl.GetHashCode())
		{
			return false;
		}
		if (fontImpl.m_font.Equals(m_font) && m_btCharSet == fontImpl.m_btCharSet)
		{
			return m_color == fontImpl.m_color;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_font.GetHashCode();
	}

	public int CompareTo(object obj)
	{
		if (!(obj is FontImpl fontImpl))
		{
			throw new ArgumentNullException("font");
		}
		int num = m_font.CompareTo(fontImpl.m_font);
		if (num == 0)
		{
			num = m_btCharSet - fontImpl.m_btCharSet;
		}
		if (num == 0)
		{
			num = ((!(m_color == fontImpl.m_color)) ? 1 : 0);
		}
		return num;
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}

	object ICloneParent.Clone(object parent)
	{
		return Clone(parent);
	}

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
	}

	internal void Clear()
	{
		this.IndexChanged = null;
		if (m_fontNative != null)
		{
			m_fontNative.Dispose();
		}
		if (m_color != null)
		{
			m_color.Dispose();
		}
		m_font = null;
		m_color = null;
		m_fontNative = null;
		if (_preservedElements != null)
		{
			foreach (KeyValuePair<string, Stream> preservedElement in _preservedElements)
			{
				preservedElement.Value.Dispose();
			}
			_preservedElements.Clear();
			_preservedElements = null;
		}
		Dispose();
	}
}
