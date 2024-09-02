using System;
using System.Collections.Generic;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public class WPageSetup : FormatBase
{
	private const float DEF_PAGE_WIDTH = 595.3f;

	private const float DEF_PAGE_HEIGHT = 841.9f;

	private const float DEF_PAGE_MARGINS = 20f;

	private const float DEF_PAGE_MARGIN_LEFT = 50f;

	internal const float DEF_AUTO_TAB_LENGHT = 36f;

	private const float DEF_AR_TO_LETTER_LIMIT = 26f;

	private const int DEF_A_ASCII_INDEX = 64;

	internal const int LinePitchKey = 1;

	internal const int PitchTypeKey = 2;

	internal const int VerticalAlignKey = 3;

	internal const int PageOrientKey = 4;

	internal const int PageSizeKey = 5;

	internal const int EqualColWidthKey = 6;

	internal const int MarginKey = 7;

	internal const int DrawLinesBetwColsKey = 8;

	internal const int LineNumberingModeKey = 9;

	internal const int FootnotePositionKey = 10;

	internal const int FootnoteNumberFormatKey = 11;

	internal const int EndnoteNumberFormatKey = 12;

	internal const int RestartIndexForFootnotesKey = 13;

	internal const int RestartIndexForEndnoteKey = 14;

	internal const int InitialFootnoteNumberKey = 15;

	internal const int InitialEndnoteNumberKey = 16;

	internal const int PageNumberStyleKey = 17;

	internal const int PageNumberRestartKey = 18;

	internal const int PageNumberStartAtKey = 19;

	internal const int BidiKey = 20;

	internal const int EndnotePositionKey = 21;

	internal const int HeaderDistanceKey = 22;

	internal const int FooterDistanceKey = 23;

	internal const int OtherPagesTrayKey = 24;

	internal const int FirstPageTrayKey = 25;

	internal const int LineNumberingStartValueKey = 27;

	internal const int PageBorderIsInFrontKey = 28;

	internal const int PageBorderOffsetFromKey = 29;

	internal const int LineNumDistanceFromTextKey = 30;

	internal const int PageBorderApplyKey = 31;

	internal const int DifferentFirstPageKey = 32;

	internal const int DifferentOddAndEvenPageKey = 33;

	internal const int BorderKey = 34;

	internal const int PageNumbersKey = 35;

	internal const int spaceKey = 36;

	internal const int NumberOfColumnsKey = 37;

	internal const int LineNumnberingModKey = 39;

	internal FootEndNoteNumberFormat EndnoteNumberFormat
	{
		get
		{
			return (FootEndNoteNumberFormat)GetPropertyValue(12);
		}
		set
		{
			SetPropertyValue(12, value);
		}
	}

	internal FootEndNoteNumberFormat FootnoteNumberFormat
	{
		get
		{
			return (FootEndNoteNumberFormat)GetPropertyValue(11);
		}
		set
		{
			SetPropertyValue(11, value);
		}
	}

	internal EndnoteRestartIndex RestartIndexForEndnote
	{
		get
		{
			return (EndnoteRestartIndex)GetPropertyValue(14);
		}
		set
		{
			SetPropertyValue(14, value);
		}
	}

	internal FootnoteRestartIndex RestartIndexForFootnotes
	{
		get
		{
			return (FootnoteRestartIndex)GetPropertyValue(13);
		}
		set
		{
			SetPropertyValue(13, value);
		}
	}

	internal FootnotePosition FootnotePosition
	{
		get
		{
			return (FootnotePosition)GetPropertyValue(10);
		}
		set
		{
			SetPropertyValue(10, value);
		}
	}

	internal EndnotePosition EndnotePosition
	{
		get
		{
			return (EndnotePosition)GetPropertyValue(21);
		}
		set
		{
			SetPropertyValue(21, value);
		}
	}

	internal int InitialFootnoteNumber
	{
		get
		{
			return (int)GetPropertyValue(15);
		}
		set
		{
			SetPropertyValue(15, value);
		}
	}

	internal int InitialEndnoteNumber
	{
		get
		{
			return (int)GetPropertyValue(16);
		}
		set
		{
			SetPropertyValue(16, value);
		}
	}

	[Obsolete("This property has been deprecated. Use the DefaultTabWidth property of WordDocument class to set default tab width for the document.")]
	public float DefaultTabWidth
	{
		get
		{
			if (base.Document == null)
			{
				return 36f;
			}
			return base.Document.DefaultTabWidth;
		}
		set
		{
			if (base.Document != null)
			{
				base.Document.DefaultTabWidth = value;
			}
		}
	}

	public SizeF PageSize
	{
		get
		{
			return (SizeF)GetPropertyValue(5);
		}
		set
		{
			SetPropertyValue(5, value);
			if (!base.Document.IsOpening && !base.Document.IsCloning)
			{
				if (PageSize.Height >= PageSize.Width)
				{
					SetPropertyValue(4, PageOrientation.Portrait);
				}
				else
				{
					SetPropertyValue(4, PageOrientation.Landscape);
				}
			}
		}
	}

	public PageOrientation Orientation
	{
		get
		{
			return (PageOrientation)GetPropertyValue(4);
		}
		set
		{
			if (Orientation == value)
			{
				return;
			}
			if (!base.Document.IsOpening && !base.Document.IsCloning)
			{
				PageSize = new SizeF(PageSize.Height, PageSize.Width);
				if (PageSize.Height >= PageSize.Width)
				{
					SetPropertyValue(4, PageOrientation.Portrait);
				}
				else
				{
					SetPropertyValue(4, PageOrientation.Landscape);
				}
			}
			else
			{
				SetPropertyValue(4, value);
			}
		}
	}

	public PageAlignment VerticalAlignment
	{
		get
		{
			return (PageAlignment)GetPropertyValue(3);
		}
		set
		{
			SetPropertyValue(3, value);
		}
	}

	public MarginsF Margins
	{
		get
		{
			return (MarginsF)GetPropertyValue(7);
		}
		set
		{
			SetPropertyValue(7, value);
		}
	}

	public float HeaderDistance
	{
		get
		{
			return (float)GetPropertyValue(22);
		}
		set
		{
			if (value < 0f || value > 1584f)
			{
				throw new ArgumentException("HeaderDistance must be between 0 pt and 1584 pt.");
			}
			SetPropertyValue(22, value);
		}
	}

	public float FooterDistance
	{
		get
		{
			return (float)GetPropertyValue(23);
		}
		set
		{
			if (value < 0f || value > 1584f)
			{
				throw new ArgumentException("FooterDistance must be between 0 pt and 1584 pt.");
			}
			SetPropertyValue(23, value);
		}
	}

	public float ClientWidth => PageSize.Width - Margins.Left - Margins.Right;

	public bool DifferentFirstPage
	{
		get
		{
			return (bool)GetPropertyValue(32);
		}
		set
		{
			SetPropertyValue(32, value);
		}
	}

	public bool DifferentOddAndEvenPages
	{
		get
		{
			if (base.Document == null)
			{
				return false;
			}
			return base.Document.DifferentOddAndEvenPages;
		}
		set
		{
			if (base.Document != null)
			{
				base.Document.DifferentOddAndEvenPages = value;
			}
		}
	}

	public LineNumberingMode LineNumberingMode
	{
		get
		{
			return (LineNumberingMode)GetPropertyValue(9);
		}
		set
		{
			SetPropertyValue(9, value);
			if (LineNumberingStep == 0)
			{
				SetPropertyValue(39, 1);
			}
		}
	}

	public int LineNumberingStep
	{
		get
		{
			return (int)GetPropertyValue(39);
		}
		set
		{
			if (value < 1 || value > 100)
			{
				throw new ArgumentException("LineNumberingStep must be between 1 and 100");
			}
			SetPropertyValue(39, value);
		}
	}

	public int LineNumberingStartValue
	{
		get
		{
			return (int)GetPropertyValue(27);
		}
		set
		{
			if (value < 1 || value > 32767)
			{
				throw new ArgumentException("LineNumberingStartValue must be between 1 and 32767");
			}
			SetPropertyValue(27, value);
			if (LineNumberingStep == 0)
			{
				SetPropertyValue(39, 1);
			}
		}
	}

	public float LineNumberingDistanceFromText
	{
		get
		{
			return (float)GetPropertyValue(30);
		}
		set
		{
			if (value < 0f || value > 1584f)
			{
				throw new ArgumentException("LineNumberingDistanceFromText must be between 1 pt and 1584 pt.");
			}
			SetPropertyValue(30, value);
			if (LineNumberingStep == 0)
			{
				SetPropertyValue(39, 1);
			}
		}
	}

	public PageBordersApplyType PageBordersApplyType
	{
		get
		{
			return (PageBordersApplyType)GetPropertyValue(31);
		}
		set
		{
			SetPropertyValue(31, value);
		}
	}

	public PageBorderOffsetFrom PageBorderOffsetFrom
	{
		get
		{
			return (PageBorderOffsetFrom)GetPropertyValue(29);
		}
		set
		{
			SetPropertyValue(29, value);
		}
	}

	public bool IsFrontPageBorder
	{
		get
		{
			return (bool)GetPropertyValue(28);
		}
		set
		{
			SetPropertyValue(28, value);
		}
	}

	public Borders Borders
	{
		get
		{
			return (Borders)GetPropertyValue(34);
		}
		internal set
		{
			SetPropertyValue(34, value);
		}
	}

	public bool Bidi
	{
		get
		{
			return (bool)GetPropertyValue(20);
		}
		set
		{
			SetPropertyValue(20, value);
		}
	}

	internal bool EqualColumnWidth
	{
		get
		{
			return (bool)GetPropertyValue(6);
		}
		set
		{
			SetPropertyValue(6, value);
		}
	}

	public PageNumberStyle PageNumberStyle
	{
		get
		{
			return (PageNumberStyle)GetPropertyValue(17);
		}
		set
		{
			SetPropertyValue(17, value);
		}
	}

	public int PageStartingNumber
	{
		get
		{
			return (int)GetPropertyValue(19);
		}
		set
		{
			SetPropertyValue(19, value);
		}
	}

	public bool RestartPageNumbering
	{
		get
		{
			return (bool)GetPropertyValue(18);
		}
		set
		{
			SetPropertyValue(18, value);
		}
	}

	internal float LinePitch
	{
		get
		{
			return (float)GetPropertyValue(1);
		}
		set
		{
			SetPropertyValue(1, value);
		}
	}

	internal GridPitchType PitchType
	{
		get
		{
			return (GridPitchType)GetPropertyValue(2);
		}
		set
		{
			SetPropertyValue(2, value);
		}
	}

	internal bool DrawLinesBetweenCols
	{
		get
		{
			return (bool)GetPropertyValue(8);
		}
		set
		{
			SetPropertyValue(8, value);
		}
	}

	public PageNumbers PageNumbers
	{
		get
		{
			return (PageNumbers)GetPropertyValue(35);
		}
		internal set
		{
			SetPropertyValue(35, value);
		}
	}

	public PrinterPaperTray FirstPageTray
	{
		get
		{
			return (PrinterPaperTray)GetPropertyValue(25);
		}
		set
		{
			SetPropertyValue(25, value);
		}
	}

	public PrinterPaperTray OtherPagesTray
	{
		get
		{
			return (PrinterPaperTray)GetPropertyValue(24);
		}
		set
		{
			SetPropertyValue(24, value);
		}
	}

	internal int NumberOfColumns
	{
		get
		{
			return (int)GetPropertyValue(37);
		}
		set
		{
			SetPropertyValue(37, value);
		}
	}

	internal float ColumnSpace
	{
		get
		{
			return (float)GetPropertyValue(36);
		}
		set
		{
			SetPropertyValue(36, value);
		}
	}

	internal WPageSetup(WSection sec)
		: base(sec.Document, sec)
	{
		PageSize = new SizeF(595.3f, 841.9f);
		Margins = new MarginsF();
		if (base.IsFormattingChange)
		{
			Margins.SetOldPropertyHashMarginValues(50f, 20f, 20f, 20f, Margins.Gutter);
		}
		else
		{
			Margins.All = 20f;
			Margins.Left = 50f;
		}
		SetPropertyValue(22, -0.05f);
		SetPropertyValue(23, -0.05f);
	}

	internal void SetPageSetupProperty(string propertyName, object propertyValue)
	{
		switch (propertyName)
		{
		case "HeaderDistance":
			SetPropertyValue(22, (float)propertyValue);
			break;
		case "FooterDistance":
			SetPropertyValue(23, (float)propertyValue);
			break;
		case "LineNumberingStep":
			SetPropertyValue(39, (int)propertyValue);
			break;
		case "LineNumberingStartValue":
			SetPropertyValue(27, (int)propertyValue);
			break;
		case "LineNumberingDistanceFromText":
			SetPropertyValue(30, (float)propertyValue);
			break;
		case "LineNumberingMode":
			SetPropertyValue(9, (LineNumberingMode)propertyValue);
			break;
		}
	}

	internal void InitializeDocxPageSetup()
	{
		PageSize = new SizeF(612f, 792f);
		if (base.IsFormattingChange)
		{
			Margins.SetOldPropertyHashMarginValues(72f, 72f, 72f, 72f, 0f);
		}
		else
		{
			Margins.All = 72f;
			Margins.Gutter = 0f;
		}
		SetPropertyValue(23, 36f);
		SetPropertyValue(22, 36f);
	}

	internal bool Compare(WPageSetup pageSetup)
	{
		if (!Compare(1, pageSetup))
		{
			return false;
		}
		if (!Compare(2, pageSetup))
		{
			return false;
		}
		if (!Compare(4, pageSetup))
		{
			return false;
		}
		if (!Compare(3, pageSetup))
		{
			return false;
		}
		if (!Compare(5, pageSetup))
		{
			return false;
		}
		if (!Compare(6, pageSetup))
		{
			return false;
		}
		if (!Compare(8, pageSetup))
		{
			return false;
		}
		if (!Compare(9, pageSetup))
		{
			return false;
		}
		if (!Compare(10, pageSetup))
		{
			return false;
		}
		if (!Compare(21, pageSetup))
		{
			return false;
		}
		if (!Compare(11, pageSetup))
		{
			return false;
		}
		if (!Compare(12, pageSetup))
		{
			return false;
		}
		if (!Compare(13, pageSetup))
		{
			return false;
		}
		if (!Compare(14, pageSetup))
		{
			return false;
		}
		if (!Compare(15, pageSetup))
		{
			return false;
		}
		if (!Compare(16, pageSetup))
		{
			return false;
		}
		if (!Compare(17, pageSetup))
		{
			return false;
		}
		if (!Compare(18, pageSetup))
		{
			return false;
		}
		if (!Compare(19, pageSetup))
		{
			return false;
		}
		if (!Compare(22, pageSetup))
		{
			return false;
		}
		if (!Compare(20, pageSetup))
		{
			return false;
		}
		if (!Compare(23, pageSetup))
		{
			return false;
		}
		if (!Compare(24, pageSetup))
		{
			return false;
		}
		if (!Compare(25, pageSetup))
		{
			return false;
		}
		if (!Compare(39, pageSetup))
		{
			return false;
		}
		if (!Compare(27, pageSetup))
		{
			return false;
		}
		if (!Compare(28, pageSetup))
		{
			return false;
		}
		if (!Compare(29, pageSetup))
		{
			return false;
		}
		if (!Compare(31, pageSetup))
		{
			return false;
		}
		if (!Compare(30, pageSetup))
		{
			return false;
		}
		if (!Compare(32, pageSetup))
		{
			return false;
		}
		if (!Compare(33, pageSetup))
		{
			return false;
		}
		if (!Compare(37, pageSetup))
		{
			return false;
		}
		if (!Compare(36, pageSetup))
		{
			return false;
		}
		if (Margins != null && pageSetup.Margins != null && !Margins.Compare(pageSetup.Margins))
		{
			return false;
		}
		if (Borders != null && pageSetup.Borders != null && !Borders.Compare(pageSetup.Borders))
		{
			return false;
		}
		if (PageNumbers != null && pageSetup.PageNumbers != null && !PageNumbers.Compare(pageSetup.PageNumbers))
		{
			return false;
		}
		return true;
	}

	public void InsertPageNumbers(bool topOfPage, PageNumberAlignment horizontalAlignment)
	{
		HeaderFooter headerFooter = (topOfPage ? (base.OwnerBase as WSection).HeadersFooters.Header : (base.OwnerBase as WSection).HeadersFooters.Footer);
		IWParagraph iWParagraph = null;
		IWField iWField = null;
		int i = 0;
		for (int count = headerFooter.Paragraphs.Count; i < count; i++)
		{
			iWParagraph = headerFooter.Paragraphs[i];
			int j = 0;
			for (int count2 = iWParagraph.Items.Count; j < count2; j++)
			{
				if (iWParagraph.Items[j].EntityType == EntityType.Field)
				{
					WField wField = (WField)iWParagraph.Items[j];
					if (wField.FieldType == FieldType.FieldPage)
					{
						iWField = wField;
						break;
					}
				}
			}
		}
		if (iWField == null)
		{
			iWParagraph = headerFooter.AddParagraph();
			iWField = iWParagraph.AppendField("", FieldType.FieldPage);
		}
		iWParagraph.ParagraphFormat.WrapFrameAround = FrameWrapMode.Around;
		iWParagraph.ParagraphFormat.FrameX = (short)horizontalAlignment;
		iWParagraph.ParagraphFormat.FrameVerticalPos = 2;
	}

	internal string GetNumberFormatValue(byte numberFormat, int number)
	{
		string text = number.ToString();
		return numberFormat switch
		{
			1 => GetAsRoman(number).ToUpper(), 
			2 => GetAsRoman(number).ToLower(), 
			3 => GetAsLetter(number).ToUpper(), 
			4 => GetAsLetter(number).ToLower(), 
			_ => number.ToString(), 
		};
	}

	private string GetAsRoman(int number)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GenerateNumber(ref number, 1000, "M"));
		stringBuilder.Append(GenerateNumber(ref number, 900, "CM"));
		stringBuilder.Append(GenerateNumber(ref number, 500, "D"));
		stringBuilder.Append(GenerateNumber(ref number, 400, "CD"));
		stringBuilder.Append(GenerateNumber(ref number, 100, "C"));
		stringBuilder.Append(GenerateNumber(ref number, 90, "XC"));
		stringBuilder.Append(GenerateNumber(ref number, 50, "L"));
		stringBuilder.Append(GenerateNumber(ref number, 40, "XL"));
		stringBuilder.Append(GenerateNumber(ref number, 10, "X"));
		stringBuilder.Append(GenerateNumber(ref number, 9, "IX"));
		stringBuilder.Append(GenerateNumber(ref number, 5, "V"));
		stringBuilder.Append(GenerateNumber(ref number, 4, "IV"));
		stringBuilder.Append(GenerateNumber(ref number, 1, "I"));
		return stringBuilder.ToString();
	}

	private string GetAsLetter(int number)
	{
		Stack<int> stack = ConvertToLetter(number);
		StringBuilder stringBuilder = new StringBuilder();
		while (stack.Count > 0)
		{
			int number2 = stack.Pop();
			AppendChar(stringBuilder, number2);
		}
		return stringBuilder.ToString();
	}

	private static void AppendChar(StringBuilder builder, int number)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		if (number <= 0 || number > 26)
		{
			throw new ArgumentOutOfRangeException("number", "Value can not be less 0 and greater 26");
		}
		char value = (char)(64 + number);
		builder.Append(value);
	}

	private static Stack<int> ConvertToLetter(float arabic)
	{
		if (arabic < 0f)
		{
			throw new ArgumentOutOfRangeException("arabic", "Value can not be less 0");
		}
		Stack<int> stack = new Stack<int>();
		while ((float)(int)arabic > 26f)
		{
			float num = arabic % 26f;
			if (num == 0f)
			{
				arabic = arabic / 26f - 1f;
				num = 26f;
			}
			else
			{
				arabic /= 26f;
			}
			stack.Push((int)num);
		}
		if (arabic > 0f)
		{
			stack.Push((int)arabic);
		}
		return stack;
	}

	private string GenerateNumber(ref int value, int magnitude, string letter)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (value >= magnitude)
		{
			value -= magnitude;
			stringBuilder.Append(letter);
		}
		return stringBuilder.ToString();
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (DefaultTabWidth != 36f)
		{
			writer.WriteValue("AutoTabWidth", DefaultTabWidth);
		}
		if (PageSize.Height != 0f)
		{
			writer.WriteValue("PageHeight", PageSize.Height);
		}
		if (PageSize.Width != 0f)
		{
			writer.WriteValue("PageWidth", PageSize.Width);
		}
		if (VerticalAlignment != 0)
		{
			writer.WriteValue("Alignment", VerticalAlignment);
		}
		if (FooterDistance >= 0f)
		{
			writer.WriteValue("FooterDistance", FooterDistance);
		}
		if (HeaderDistance >= 0f)
		{
			writer.WriteValue("HeaderDistance", HeaderDistance);
		}
		if (Orientation != 0)
		{
			writer.WriteValue("Orientation", Orientation);
		}
		if (Margins.Bottom >= 0f)
		{
			writer.WriteValue("BottomMargin", Margins.Bottom);
		}
		if (Margins.Top >= 0f)
		{
			writer.WriteValue("TopMargin", Margins.Top);
		}
		if (Margins.Left >= 0f)
		{
			writer.WriteValue("LeftMargin", Margins.Left);
		}
		if (Margins.Right >= 0f)
		{
			writer.WriteValue("RightMargin", Margins.Right);
		}
		if (DifferentFirstPage)
		{
			writer.WriteValue("DifferentFirstPage", DifferentFirstPage);
		}
		if (DifferentOddAndEvenPages)
		{
			writer.WriteValue("DifferentOddEvenPage", DifferentOddAndEvenPages);
		}
		if (LineNumberingMode != LineNumberingMode.None)
		{
			writer.WriteValue("PageSetupLineNumMode", LineNumberingMode);
			if (LineNumberingStep != 0)
			{
				writer.WriteValue("PageSetupLineNumStep", LineNumberingStep);
			}
			if (LineNumberingDistanceFromText != 0f)
			{
				writer.WriteValue("PageSetupLineNumDistance", LineNumberingDistanceFromText);
			}
			writer.WriteValue("PageSetupLineNumStartValue", LineNumberingStartValue);
		}
		if (PageBordersApplyType != 0)
		{
			writer.WriteValue("PageSetupBorderApply", PageBordersApplyType);
		}
		if (!IsFrontPageBorder)
		{
			writer.WriteValue("PageSetupBorderIsInFront", IsFrontPageBorder);
		}
		if (PageBorderOffsetFrom != 0)
		{
			writer.WriteValue("PageSetupBorderOffsetFrom", PageBorderOffsetFrom);
		}
		if (EqualColumnWidth)
		{
			writer.WriteValue("EqualColWidth", EqualColumnWidth);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("AutoTabWidth"))
		{
			DefaultTabWidth = reader.ReadFloat("AutoTabWidth");
		}
		if (reader.HasAttribute("PageHeight"))
		{
			PageSize = new SizeF(PageSize.Width, reader.ReadFloat("PageHeight"));
		}
		if (reader.HasAttribute("PageWidth"))
		{
			PageSize = new SizeF(reader.ReadFloat("PageWidth"), PageSize.Height);
		}
		if (reader.HasAttribute("Alignment"))
		{
			VerticalAlignment = (PageAlignment)(object)reader.ReadEnum("Alignment", typeof(PageAlignment));
		}
		if (reader.HasAttribute("FooterDistance"))
		{
			SetPageSetupProperty("FooterDistance", reader.ReadFloat("FooterDistance"));
		}
		if (reader.HasAttribute("HeaderDistance"))
		{
			SetPageSetupProperty("HeaderDistance", reader.ReadFloat("HeaderDistance"));
		}
		if (reader.HasAttribute("Orientation"))
		{
			Orientation = (PageOrientation)(object)reader.ReadEnum("Orientation", typeof(PageOrientation));
		}
		if (reader.HasAttribute("BottomMargin"))
		{
			Margins.Bottom = reader.ReadFloat("BottomMargin");
		}
		if (reader.HasAttribute("TopMargin"))
		{
			Margins.Top = reader.ReadFloat("TopMargin");
		}
		if (reader.HasAttribute("LeftMargin"))
		{
			Margins.Left = reader.ReadFloat("LeftMargin");
		}
		if (reader.HasAttribute("RightMargin"))
		{
			Margins.Right = reader.ReadFloat("RightMargin");
		}
		if (reader.HasAttribute("DifferentFirstPage"))
		{
			DifferentFirstPage = reader.ReadBoolean("DifferentFirstPage");
		}
		if (reader.HasAttribute("DifferentOddEvenPage"))
		{
			DifferentOddAndEvenPages = reader.ReadBoolean("DifferentOddEvenPage");
		}
		if (reader.HasAttribute("PageSetupLineNumStep"))
		{
			SetPageSetupProperty("LineNumberingStep", reader.ReadInt("PageSetupLineNumStep"));
		}
		if (reader.HasAttribute("PageSetupLineNumDistance"))
		{
			SetPageSetupProperty("LineNumberingDistanceFromText", reader.ReadFloat("PageSetupLineNumDistance"));
		}
		if (reader.HasAttribute("PageSetupLineNumMode"))
		{
			SetPageSetupProperty("LineNumberingMode", (LineNumberingMode)(object)reader.ReadEnum("PageSetupLineNumMode", typeof(LineNumberingMode)));
		}
		if (reader.HasAttribute("PageSetupLineNumStartValue"))
		{
			SetPageSetupProperty("LineNumberingStartValue", reader.ReadInt("PageSetupLineNumStartValue"));
		}
		if (reader.HasAttribute("PageSetupBorderApply"))
		{
			PageBordersApplyType = (PageBordersApplyType)(object)reader.ReadEnum("PageSetupBorderApply", typeof(PageBordersApplyType));
		}
		if (reader.HasAttribute("PageSetupBorderIsInFront"))
		{
			IsFrontPageBorder = reader.ReadBoolean("PageSetupBorderIsInFront");
		}
		if (reader.HasAttribute("PageSetupBorderOffsetFrom"))
		{
			PageBorderOffsetFrom = (PageBorderOffsetFrom)(object)reader.ReadEnum("PageSetupBorderOffsetFrom", typeof(PageBorderOffsetFrom));
		}
		if (reader.HasAttribute("EqualColWidth"))
		{
			EqualColumnWidth = reader.ReadBoolean("EqualColWidth");
		}
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("borders", Borders);
	}

	public override string ToString()
	{
		return base.ToString();
	}

	internal WPageSetup Clone()
	{
		WPageSetup wPageSetup = new WPageSetup(base.OwnerBase as WSection);
		wPageSetup.ImportContainer(this);
		wPageSetup.CopyProperties(this);
		wPageSetup.Borders = Borders.Clone();
		wPageSetup.Borders.SetOwner(wPageSetup);
		wPageSetup.Margins = Margins.Clone();
		wPageSetup.Margins.SetOwner(wPageSetup);
		wPageSetup.PageNumbers = PageNumbers.Clone();
		wPageSetup.PageNumbers.SetOwner(wPageSetup);
		return wPageSetup;
	}

	internal override void Close()
	{
		if (Borders != null)
		{
			Borders.Close();
		}
		if (PageNumbers != null)
		{
			PageNumbers.Close();
		}
		if (Margins != null)
		{
			Margins.Close();
		}
		base.Close();
	}

	internal object GetPropertyValue(int propKey)
	{
		return base[propKey];
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		base[propKey] = value;
	}

	protected internal override void EnsureComposites()
	{
		EnsureComposites(34);
	}

	protected override FormatBase GetDefComposite(int key)
	{
		return key switch
		{
			34 => GetDefComposite(34, new Borders()), 
			7 => GetDefComposite(7, new MarginsF()), 
			35 => GetDefComposite(35, new PageNumbers()), 
			_ => null, 
		};
	}

	protected override object GetDefValue(int key)
	{
		switch (key)
		{
		case 1:
			return 0f;
		case 2:
			return GridPitchType.NoGrid;
		case 3:
			return PageAlignment.Top;
		case 4:
			return PageOrientation.Portrait;
		case 5:
			return default(SizeF);
		case 6:
			return true;
		case 8:
			return false;
		case 9:
			return LineNumberingMode.None;
		case 11:
			return FootEndNoteNumberFormat.Arabic;
		case 12:
			return FootEndNoteNumberFormat.LowerCaseRoman;
		case 13:
		case 14:
			return EndnoteRestartIndex.DoNotRestart;
		case 15:
		case 16:
			return 1;
		case 17:
			return PageNumberStyle.Arabic;
		case 18:
		case 20:
			return false;
		case 19:
			return 0;
		case 21:
			return EndnotePosition.DisplayEndOfDocument;
		case 10:
			return FootnotePosition.PrintAtBottomOfPage;
		case 24:
		case 25:
		case 27:
		case 39:
			return 0;
		case 28:
			return true;
		case 29:
			return PageBorderOffsetFrom.Text;
		case 31:
			return PageBordersApplyType.AllPages;
		case 30:
			return 18f;
		case 32:
		case 33:
			return false;
		case 22:
		case 23:
			return 0f;
		case 37:
			return 1;
		case 36:
			return 0f;
		default:
			throw new ArgumentException("key not found");
		}
	}
}
