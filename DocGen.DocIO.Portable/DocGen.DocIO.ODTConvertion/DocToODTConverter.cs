using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ODF.Base;
using DocGen.DocIO.ODF.Base.ODFImplementation;
using DocGen.DocIO.ODF.Base.ODFSerialization;
using DocGen.DocIO.ODFConverter.Base.ODFImplementation;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.DocIO.ODTConvertion;

internal class DocToODTConverter
{
	private WordDocument m_document;

	internal ODocument m_oDocument = new ODocument();

	internal OParagraph paragraph;

	internal OParagraphItem opara;

	internal ODFStyle odfStyle;

	private ODFWriter m_writer;

	private ODFStyleCollection odfstyleCollection1 = new ODFStyleCollection();

	private int m_docPrId;

	private List<string> pageNames;

	private BeforeBreak m_lastBreak;

	private byte m_flag;

	private int m_relationShipID;

	internal bool IsWritingHeaderFooter
	{
		get
		{
			return (m_flag & 1) != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public DocToODTConverter(WordDocument document)
	{
		m_document = document;
		m_writer = new ODFWriter();
		pageNames = new List<string>();
	}

	internal void ConvertToODF(Stream stream)
	{
		m_writer.SerializeMetaData();
		m_writer.SerializeMimeType();
		m_writer.SerializeSettings();
		MapDocumentStyles();
		MapContent();
		m_writer.SerializeDocumentManifest();
		m_writer.SaveDocument(stream);
		Close();
	}

	internal void MapDocumentStyles()
	{
		MemoryStream stream = m_writer.SerializeStyleStart();
		ConvertFontFace();
		m_writer.SerializeDataStylesStart();
		m_writer.SerializeTableDefaultStyle();
		ConvertDefaultStyles();
		m_writer.SerializeEnd();
		ConvertAutomaticAndMasterStyles();
		m_writer.SerializeStylesEnd(stream);
	}

	internal void ConvertFontFace()
	{
		List<FontFace> list = new List<FontFace>();
		List<string> list2 = new List<string>();
		if (m_document.FFNStringTable != null && m_document.FFNStringTable.FontFamilyNameRecords != null)
		{
			FontFamilyNameRecord[] fontFamilyNameRecords = m_document.FFNStringTable.FontFamilyNameRecords;
			foreach (FontFamilyNameRecord fontFamilyNameRecord in fontFamilyNameRecords)
			{
				_ = m_document.Styles;
				string[] array = fontFamilyNameRecord.FontName.Split('\0');
				FontFace fontFace = new FontFace(array[0]);
				fontFace.Name = array[0];
				FontFamilyGeneric fontFamilyID = (FontFamilyGeneric)fontFamilyNameRecord.FontFamilyID;
				fontFace.FontFamilyGeneric = fontFamilyID;
				fontFace.FontPitch = (FontPitch)fontFamilyNameRecord.PitchRequest;
				list.Add(fontFace);
				list2.Add(fontFamilyNameRecord.FontName);
			}
		}
		if (m_document.UsedFontNames.Count > 0)
		{
			foreach (string usedFontName in m_document.UsedFontNames)
			{
				if (!list2.Contains(usedFontName))
				{
					FontFamilyNameRecord fontFamilyNameRecord2 = new FontFamilyNameRecord();
					fontFamilyNameRecord2.FontName = usedFontName;
					_ = m_document.Styles;
					string[] array2 = fontFamilyNameRecord2.FontName.Split('\0');
					FontFace fontFace2 = new FontFace(array2[0]);
					fontFace2.Name = array2[0];
					FontFamilyGeneric fontFamilyID2 = (FontFamilyGeneric)fontFamilyNameRecord2.FontFamilyID;
					fontFace2.FontFamilyGeneric = fontFamilyID2;
					fontFace2.FontPitch = (FontPitch)fontFamilyNameRecord2.PitchRequest;
					list.Add(fontFace2);
				}
			}
		}
		m_writer.SerializeFontFaceDecls(list);
	}

	internal void MapContent()
	{
		MemoryStream stream = m_writer.SerializeContentNameSpace();
		ConvertFontFace();
		GetBody();
		m_writer.SerializeContentEnd(stream);
	}

	private TextProperties CopyCharFormatToTextFormat(WCharacterFormat charFormat, TextProperties textProp)
	{
		if (charFormat != null)
		{
			if (charFormat.PropertiesHash.Count > 0)
			{
				textProp = new TextProperties();
			}
			if (textProp != null)
			{
				if (charFormat.Position != 0f)
				{
					textProp.TextPosition = charFormat.Position * 100f / charFormat.FontSize + "% 100%";
				}
				if (charFormat.CharStyleName != null)
				{
					textProp.CharStyleName = charFormat.CharStyleName;
				}
				if (charFormat.HasValue(53) && charFormat.Hidden)
				{
					textProp.IsTextDisplay = false;
				}
				if (charFormat.HasValue(10))
				{
					switch (charFormat.SubSuperScript)
					{
					case DocGen.DocIO.DLS.SubSuperScript.SubScript:
						textProp.TextPosition = "sub 63.6%";
						break;
					case DocGen.DocIO.DLS.SubSuperScript.SuperScript:
						textProp.TextPosition = "super 63.6%";
						break;
					}
				}
				if (charFormat.HasValue(2))
				{
					textProp.FontName = charFormat.FontName;
				}
				if (charFormat.HasValue(3))
				{
					textProp.FontSize = charFormat.FontSize;
				}
				if (charFormat.HasValue(4) && charFormat.Bold)
				{
					textProp.FontWeight = FontWeight.bold;
					textProp.FontWeightAsian = FontWeight.bold;
					textProp.FontWeightComplex = (charFormat.Bold ? FontWeight.bold : FontWeight.normal);
				}
				if (charFormat.HasValue(127))
				{
					textProp.TextScale = charFormat.Scaling;
				}
				if (charFormat.HasValue(1))
				{
					textProp.Color = charFormat.TextColor;
				}
				if (charFormat.HasValue(5) && charFormat.Italic)
				{
					textProp.FontStyle = ODFFontStyle.italic;
				}
				if (charFormat.HasValue(18))
				{
					textProp.LetterSpacing = charFormat.CharacterSpacing;
				}
				if (charFormat.HasValue(50) && charFormat.Shadow)
				{
					textProp.Shadow = charFormat.Shadow;
				}
				if (charFormat.HasValue(55) && charFormat.SmallCaps)
				{
					textProp.TextTransform = Transform.uppercase;
				}
				else if (charFormat.HasValue(54) && charFormat.AllCaps)
				{
					textProp.TextTransform = Transform.uppercase;
				}
				if (charFormat.HasValue(71) && charFormat.OutLine)
				{
					textProp.TextOutline = true;
				}
				if (charFormat.HasValue(125))
				{
					textProp.LetterKerning = charFormat.IsKernFont;
				}
				if (charFormat.HasValue(6) || charFormat.HasValue(14))
				{
					textProp.LinethroughStyle = BorderLineStyle.solid;
					textProp.LinethroughColor = GetColor(charFormat.TextColor);
					TextProperties textProperties = textProp;
					int linethroughType;
					if (!charFormat.HasValue(14) || !charFormat.DoubleStrike)
					{
						if (!charFormat.HasValue(6) || !charFormat.Strikeout)
						{
							linethroughType = 0;
						}
						else
						{
							DocGen.DocIO.ODF.Base.LineType lineType2 = (textProp.LinethroughType = DocGen.DocIO.ODF.Base.LineType.single);
							linethroughType = (int)lineType2;
						}
					}
					else
					{
						linethroughType = 2;
					}
					textProperties.LinethroughType = (DocGen.DocIO.ODF.Base.LineType)linethroughType;
				}
				if (charFormat.HasValue(63))
				{
					textProp.BackgroundColor = charFormat.HighlightColor;
				}
				if (charFormat.HasValue(7))
				{
					textProp.TextUnderlineStyle = GetUnderlineStyle(charFormat.UnderlineStyle);
					textProp.TextUnderlineColor = GetColor(charFormat.TextBackgroundColor);
				}
				if (charFormat.HasValue(24))
				{
					textProp.TextDisplay = TextDisplay.none;
					textProp.IsTextDisplay = true;
				}
				if (charFormat.HasValue(51) && charFormat.Emboss)
				{
					textProp.FontRelief = FontRelief.embossed;
				}
			}
			return textProp;
		}
		return null;
	}

	private BorderLineStyle GetUnderlineStyle(UnderlineStyle charstyle)
	{
		BorderLineStyle result = BorderLineStyle.none;
		switch (charstyle)
		{
		case UnderlineStyle.Dash:
			result = BorderLineStyle.dashed;
			break;
		case UnderlineStyle.DotDash:
			result = BorderLineStyle.dotdash;
			break;
		case UnderlineStyle.DotDotDash:
			result = BorderLineStyle.dotdotdash;
			break;
		case UnderlineStyle.Dotted:
			result = BorderLineStyle.dotted;
			break;
		case UnderlineStyle.DashLong:
			result = BorderLineStyle.longdash;
			break;
		case UnderlineStyle.Single:
			result = BorderLineStyle.solid;
			break;
		case UnderlineStyle.Wavy:
			result = BorderLineStyle.wave;
			break;
		}
		return result;
	}

	private ODFParagraphProperties CopyParaFormatToParagraphPropertiesFormat(WParagraphFormat paraformat)
	{
		ODFParagraphProperties oDFParagraphProperties = null;
		if (paraformat != null)
		{
			if (paraformat.HasValue(21) || paraformat.HasValue(6) || paraformat.HasValue(10) || !paraformat.Borders.NoBorder || paraformat.HasValue(5) || paraformat.HasValue(52) || paraformat.HasValue(8) || paraformat.HasValue(9) || paraformat.HasValue(2) || paraformat.HasValue(3) || paraformat.HasKey(58) || paraformat.HasKey(57) || (paraformat.Borders.Left.IsBorderDefined && !paraformat.Borders.Left.IsDefault) || (paraformat.Borders.Right.IsBorderDefined && !paraformat.Borders.Right.IsDefault) || (paraformat.Borders.Top.IsBorderDefined && !paraformat.Borders.Top.IsDefault) || (paraformat.Borders.Bottom.IsBorderDefined && !paraformat.Borders.Bottom.IsDefault) || paraformat.HasValue(0) || paraformat.HasValue(31) || (paraformat.Tabs != null && paraformat.Tabs.Count > 0))
			{
				oDFParagraphProperties = new ODFParagraphProperties();
			}
			if (paraformat.Borders.Top.BorderType != 0 && paraformat.Borders.Top.BorderType == paraformat.Borders.Left.BorderType && paraformat.Borders.Top.BorderType == paraformat.Borders.Bottom.BorderType && paraformat.Borders.Top.BorderType == paraformat.Borders.Right.BorderType)
			{
				oDFParagraphProperties.Border = new ODFBorder();
				if (paraformat.Borders.Top.Space > 0f)
				{
					oDFParagraphProperties.PaddingTop = paraformat.Borders.Top.Space / 72f;
					oDFParagraphProperties.PaddingBottom = paraformat.Borders.Bottom.Space / 72f;
					oDFParagraphProperties.PaddingLeft = paraformat.Borders.Left.Space / 72f;
					oDFParagraphProperties.PaddingRight = paraformat.Borders.Right.Space / 72f;
				}
				float num = paraformat.Borders.Top.LineWidth / 72f;
				oDFParagraphProperties.Border.LineWidth = num.ToString();
				oDFParagraphProperties.Border.LineStyle = GetUnderlineStyle(paraformat.Borders.Top.BorderType);
				oDFParagraphProperties.Border.LineColor = paraformat.Borders.Top.Color;
			}
			else
			{
				if (paraformat.Borders.Left.IsBorderDefined && !paraformat.Borders.Left.IsDefault)
				{
					oDFParagraphProperties.BorderLeft = new ODFBorder();
					if (paraformat.Borders.Left.Space > 0f)
					{
						oDFParagraphProperties.PaddingLeft = paraformat.Borders.Left.Space / 72f;
					}
					float num2 = paraformat.Borders.Left.LineWidth / 72f;
					oDFParagraphProperties.BorderLeft.LineWidth = num2.ToString();
					oDFParagraphProperties.BorderLeft.LineStyle = GetUnderlineStyle(paraformat.Borders.Left.BorderType);
					oDFParagraphProperties.BorderLeft.LineColor = paraformat.Borders.Left.Color;
				}
				if (paraformat.Borders.Right.IsBorderDefined && !paraformat.Borders.Right.IsDefault)
				{
					oDFParagraphProperties.BorderRight = new ODFBorder();
					if (paraformat.Borders.Right.Space > 0f)
					{
						oDFParagraphProperties.PaddingRight = paraformat.Borders.Right.Space / 72f;
					}
					float num3 = paraformat.Borders.Right.LineWidth / 72f;
					oDFParagraphProperties.BorderRight.LineWidth = num3.ToString();
					oDFParagraphProperties.BorderRight.LineStyle = GetUnderlineStyle(paraformat.Borders.Right.BorderType);
					oDFParagraphProperties.BorderRight.LineColor = paraformat.Borders.Right.Color;
				}
				if (paraformat.Borders.Top.IsBorderDefined && !paraformat.Borders.Top.IsDefault)
				{
					oDFParagraphProperties.BorderTop = new ODFBorder();
					if (paraformat.Borders.Top.Space > 0f)
					{
						oDFParagraphProperties.PaddingTop = paraformat.Borders.Top.Space / 72f;
					}
					float num4 = paraformat.Borders.Top.LineWidth / 72f;
					oDFParagraphProperties.BorderTop.LineWidth = num4.ToString();
					oDFParagraphProperties.BorderTop.LineStyle = GetUnderlineStyle(paraformat.Borders.Top.BorderType);
					oDFParagraphProperties.BorderTop.LineColor = paraformat.Borders.Top.Color;
				}
				if (paraformat.Borders.Bottom.IsBorderDefined && !paraformat.Borders.Bottom.IsDefault)
				{
					oDFParagraphProperties.BorderBottom = new ODFBorder();
					if (paraformat.Borders.Top.Space > 0f)
					{
						oDFParagraphProperties.PaddingBottom = paraformat.Borders.Bottom.Space / 72f;
					}
					float num5 = paraformat.Borders.Bottom.LineWidth / 72f;
					oDFParagraphProperties.BorderBottom.LineWidth = num5.ToString();
					oDFParagraphProperties.BorderBottom.LineStyle = GetUnderlineStyle(paraformat.Borders.Bottom.BorderType);
					oDFParagraphProperties.BorderBottom.LineColor = paraformat.Borders.Bottom.Color;
				}
			}
			if (paraformat.HasValue(0))
			{
				oDFParagraphProperties.TextAlign = GetAlignment(paraformat);
			}
			if (paraformat.HasValue(5))
			{
				oDFParagraphProperties.TextIndent = paraformat.FirstLineIndent / 72f;
			}
			if (paraformat.HasValue(21))
			{
				oDFParagraphProperties.BackgroundColor = GetColor(paraformat.BackColor);
			}
			if (paraformat.HasValue(8))
			{
				oDFParagraphProperties.BeforeSpacing = paraformat.BeforeSpacing / 72f;
			}
			if (paraformat.HasValue(9))
			{
				oDFParagraphProperties.AfterSpacing = paraformat.AfterSpacing / 72f;
			}
			if (paraformat.HasValue(2))
			{
				oDFParagraphProperties.LeftIndent = paraformat.LeftIndent / 72f;
			}
			if (paraformat.HasValue(3))
			{
				oDFParagraphProperties.RightIndent = paraformat.RightIndent / 72f;
			}
			if (paraformat.HasValue(52))
			{
				if (paraformat.LineSpacingRule == LineSpacingRule.Multiple && (double)paraformat.LineSpacing != 12.8)
				{
					oDFParagraphProperties.LineSpacing = Math.Floor(paraformat.LineSpacing / 12f * 100f);
				}
				if (paraformat.LineSpacingRule == LineSpacingRule.AtLeast)
				{
					oDFParagraphProperties.LineHeightAtLeast = Math.Round(paraformat.LineSpacing / 72f, 4);
				}
				if (paraformat.LineSpacingRule == LineSpacingRule.Exactly)
				{
					oDFParagraphProperties.LineHeight = paraformat.LineSpacing / 72f;
				}
			}
			if (paraformat.HasValue(6))
			{
				oDFParagraphProperties.KeepTogether = KeepTogether.always;
			}
			if (paraformat.HasValue(10))
			{
				oDFParagraphProperties.KeepWithNext = KeepTogether.always;
			}
			if (paraformat.HasValue(31) && paraformat.Bidi)
			{
				oDFParagraphProperties.WritingMode = WritingMode.RLTB;
			}
			if (paraformat.HasValue(31) && paraformat.Bidi)
			{
				oDFParagraphProperties.WritingMode = WritingMode.RLTB;
			}
			if (paraformat.Tabs != null)
			{
				TabCollection tabs = paraformat.Tabs;
				if (tabs.Count > 0)
				{
					for (int i = 0; i < tabs.Count; i++)
					{
						TabStops tabStops = new TabStops();
						Tab tab = tabs[i];
						tabStops.TextPosition = tab.Position / 72f;
						tabStops.TextAlignType = GetTabAlignment(tab.Justification);
						tabStops.TabStopLeader = GetTabStop(tab.TabLeader);
						oDFParagraphProperties.TabStops.Add(tabStops);
					}
				}
			}
		}
		return oDFParagraphProperties;
	}

	private ODFParagraphProperties ResetInlineParagraphFormat(WParagraphFormat paraformat, ODFParagraphProperties paraProp)
	{
		if (paraformat != null)
		{
			if (paraProp == null)
			{
				paraProp = new ODFParagraphProperties();
			}
			if (paraformat.Borders.Top.BorderType != 0 && paraformat.Borders.Top.BorderType == paraformat.Borders.Left.BorderType && paraformat.Borders.Top.BorderType == paraformat.Borders.Bottom.BorderType && paraformat.Borders.Top.BorderType == paraformat.Borders.Right.BorderType)
			{
				paraProp.Border = new ODFBorder();
				if (paraformat.Borders.Top.Space > 0f)
				{
					paraProp.PaddingTop = paraformat.Borders.Top.Space / 72f;
					paraProp.PaddingBottom = paraformat.Borders.Bottom.Space / 72f;
					paraProp.PaddingLeft = paraformat.Borders.Left.Space / 72f;
					paraProp.PaddingRight = paraformat.Borders.Right.Space / 72f;
				}
				float num = paraformat.Borders.Top.LineWidth / 72f;
				paraProp.Border.LineWidth = num.ToString();
				paraProp.Border.LineStyle = GetUnderlineStyle(paraformat.Borders.Top.BorderType);
				paraProp.Border.LineColor = paraformat.Borders.Top.Color;
			}
			else
			{
				if (paraformat.Borders.Left.IsBorderDefined && !paraformat.Borders.Left.IsDefault)
				{
					paraProp.BorderLeft = new ODFBorder();
					if (paraformat.Borders.Left.Space > 0f)
					{
						paraProp.PaddingLeft = paraformat.Borders.Left.Space / 72f;
					}
					float num2 = paraformat.Borders.Left.LineWidth / 72f;
					paraProp.BorderLeft.LineWidth = num2.ToString();
					paraProp.BorderLeft.LineStyle = GetUnderlineStyle(paraformat.Borders.Left.BorderType);
					paraProp.BorderLeft.LineColor = paraformat.Borders.Left.Color;
				}
				if (paraformat.Borders.Right.IsBorderDefined && !paraformat.Borders.Right.IsDefault)
				{
					paraProp.BorderRight = new ODFBorder();
					if (paraformat.Borders.Right.Space > 0f)
					{
						paraProp.PaddingRight = paraformat.Borders.Right.Space / 72f;
					}
					float num3 = paraformat.Borders.Right.LineWidth / 72f;
					paraProp.BorderRight.LineWidth = num3.ToString();
					paraProp.BorderRight.LineStyle = GetUnderlineStyle(paraformat.Borders.Right.BorderType);
					paraProp.BorderRight.LineColor = paraformat.Borders.Right.Color;
				}
				if (paraformat.Borders.Top.IsBorderDefined && !paraformat.Borders.Top.IsDefault)
				{
					paraProp.BorderTop = new ODFBorder();
					if (paraformat.Borders.Top.Space > 0f)
					{
						paraProp.PaddingTop = paraformat.Borders.Top.Space / 72f;
					}
					float num4 = paraformat.Borders.Top.LineWidth / 72f;
					paraProp.BorderTop.LineWidth = num4.ToString();
					paraProp.BorderTop.LineStyle = GetUnderlineStyle(paraformat.Borders.Top.BorderType);
					paraProp.BorderTop.LineColor = paraformat.Borders.Top.Color;
				}
				if (paraformat.Borders.Bottom.IsBorderDefined && !paraformat.Borders.Bottom.IsDefault)
				{
					paraProp.BorderBottom = new ODFBorder();
					if (paraformat.Borders.Top.Space > 0f)
					{
						paraProp.PaddingBottom = paraformat.Borders.Bottom.Space / 72f;
					}
					float num5 = paraformat.Borders.Bottom.LineWidth / 72f;
					paraProp.BorderBottom.LineWidth = num5.ToString();
					paraProp.BorderBottom.LineStyle = GetUnderlineStyle(paraformat.Borders.Bottom.BorderType);
					paraProp.BorderBottom.LineColor = paraformat.Borders.Bottom.Color;
				}
			}
			if (paraformat.HasValue(0))
			{
				paraProp.TextAlign = GetAlignment(paraformat);
			}
			if (paraformat.HasValue(5))
			{
				paraProp.TextIndent = paraformat.FirstLineIndent / 72f;
			}
			if (paraformat.HasValue(21))
			{
				paraProp.BackgroundColor = GetColor(paraformat.BackColor);
			}
			if (paraformat.HasValue(8))
			{
				paraProp.BeforeSpacing = paraformat.BeforeSpacing / 72f;
			}
			if (paraformat.HasValue(9))
			{
				paraProp.AfterSpacing = paraformat.AfterSpacing / 72f;
			}
			if (paraformat.HasValue(2))
			{
				paraProp.LeftIndent = paraformat.LeftIndent / 72f;
			}
			if (paraformat.HasValue(3))
			{
				paraProp.RightIndent = paraformat.RightIndent / 72f;
			}
			if (paraformat.HasValue(52))
			{
				if (paraformat.LineSpacingRule == LineSpacingRule.Multiple && (double)paraformat.LineSpacing != 12.8)
				{
					paraProp.LineSpacing = Math.Floor(paraformat.LineSpacing / 12f * 100f);
				}
				if (paraformat.LineSpacingRule == LineSpacingRule.AtLeast)
				{
					paraProp.LineHeightAtLeast = Math.Round(paraformat.LineSpacing / 72f, 4);
				}
				if (paraformat.LineSpacingRule == LineSpacingRule.Exactly)
				{
					paraProp.LineHeight = paraformat.LineSpacing / 72f;
				}
			}
			if (paraformat.HasValue(6))
			{
				paraProp.KeepTogether = KeepTogether.always;
			}
			if (paraformat.HasValue(10))
			{
				paraProp.KeepWithNext = KeepTogether.always;
			}
			if (paraformat.HasValue(31) && paraformat.Bidi)
			{
				paraProp.WritingMode = WritingMode.RLTB;
			}
			if (paraformat.HasValue(31) && paraformat.Bidi)
			{
				paraProp.WritingMode = WritingMode.RLTB;
			}
			if (paraformat.Tabs != null)
			{
				TabCollection tabs = paraformat.Tabs;
				if (tabs.Count > 0)
				{
					for (int i = 0; i < tabs.Count; i++)
					{
						TabStops tabStops = new TabStops();
						Tab tab = tabs[i];
						tabStops.TextPosition = tab.Position / 72f;
						tabStops.TextAlignType = GetTabAlignment(tab.Justification);
						tabStops.TabStopLeader = GetTabStop(tab.TabLeader);
						paraProp.TabStops.Add(tabStops);
					}
				}
			}
		}
		return paraProp;
	}

	private TextAlign GetAlignment(WParagraphFormat paragraphFormat)
	{
		TextAlign result = TextAlign.left;
		switch (paragraphFormat.LogicalJustification)
		{
		case HorizontalAlignment.Center:
			result = TextAlign.center;
			break;
		case HorizontalAlignment.Right:
			result = TextAlign.right;
			break;
		}
		return result;
	}

	private BorderLineStyle GetUnderlineStyle(BorderStyle style)
	{
		BorderLineStyle result = BorderLineStyle.none;
		switch (style)
		{
		case BorderStyle.Dot:
			result = BorderLineStyle.dotted;
			break;
		case BorderStyle.DotDash:
			result = BorderLineStyle.dotdash;
			break;
		case BorderStyle.DotDotDash:
			result = BorderLineStyle.dotdotdash;
			break;
		case BorderStyle.Wave:
			result = BorderLineStyle.wave;
			break;
		case BorderStyle.Single:
			result = BorderLineStyle.solid;
			break;
		case BorderStyle.DashSmallGap:
			result = BorderLineStyle.dashed;
			break;
		}
		return result;
	}

	private string GetColor(Color color)
	{
		return "#" + (color.ToArgb() & 0xFFFFFF).ToString("X6");
	}

	internal void ConvertDefaultStyles()
	{
		DefaultStyleCollection defaultStyleCollection = new DefaultStyleCollection();
		IStyleCollection styles = m_document.Styles;
		DefaultStyle defaultStyle = new DefaultStyle();
		defaultStyle.ParagraphProperties = CopyParaFormatToParagraphPropertiesFormat(m_document.DefParaFormat);
		defaultStyle.Textproperties = CopyCharFormatToTextFormat(m_document.DefCharFormat, defaultStyle.Textproperties);
		defaultStyleCollection.Add(defaultStyle);
		m_writer.SerializeDefaultStyles(defaultStyleCollection);
		for (int i = 0; i < styles.Count; i++)
		{
			ODFStyleCollection oDFStyleCollection = new ODFStyleCollection();
			ODFStyle oDFStyle = new ODFStyle();
			IStyle style = styles[i];
			if (style.StyleType == StyleType.ParagraphStyle)
			{
				WParagraphStyle wParagraphStyle = styles[i] as WParagraphStyle;
				WParagraphFormat paragraphFormat = wParagraphStyle.ParagraphFormat;
				WCharacterFormat characterFormat = wParagraphStyle.CharacterFormat;
				oDFStyle.ParagraphProperties = CopyParaFormatToParagraphPropertiesFormat(paragraphFormat);
				oDFStyle.Textproperties = CopyCharFormatToTextFormat(characterFormat, oDFStyle.Textproperties);
				oDFStyle.Name = wParagraphStyle.Name;
				if (paragraphFormat.Tabs != null)
				{
					TabCollection tabs = paragraphFormat.Tabs;
					if (tabs.Count > 0)
					{
						for (int j = 0; j < tabs.Count; j++)
						{
							TabStops tabStops = new TabStops();
							Tab tab = tabs[j];
							tabStops.TextPosition = tab.Position / 72f;
							tabStops.TextAlignType = GetTabAlignment(tab.Justification);
							tabStops.TabStopLeader = GetTabStop(tab.TabLeader);
							oDFStyle.ParagraphProperties = new ODFParagraphProperties();
							oDFStyle.ParagraphProperties.TabStops.Add(tabStops);
						}
					}
				}
				oDFStyle.Family = GetStyleType(wParagraphStyle.StyleType);
				if (StartsWithExt(style.Name, "TOC"))
				{
					m_oDocument.TOCStyles.Add(oDFStyle);
				}
				oDFStyleCollection.Add(oDFStyle);
				m_writer.SerializeODFStyles(oDFStyleCollection);
			}
			if (style.StyleType == StyleType.CharacterStyle)
			{
				WCharacterStyle wCharacterStyle = styles[i] as WCharacterStyle;
				WCharacterFormat characterFormat2 = wCharacterStyle.CharacterFormat;
				oDFStyle.Textproperties = CopyCharFormatToTextFormat(characterFormat2, oDFStyle.Textproperties);
				oDFStyle.Name = wCharacterStyle.Name;
				oDFStyle.Family = GetStyleType(wCharacterStyle.StyleType);
				oDFStyleCollection.Add(oDFStyle);
				m_writer.SerializeODFStyles(oDFStyleCollection);
			}
		}
	}

	private TextAlign GetTabAlignment(TabJustification tabAlignment)
	{
		TextAlign result = TextAlign.left;
		switch (tabAlignment)
		{
		case TabJustification.Left:
			result = TextAlign.left;
			break;
		case TabJustification.Right:
			result = TextAlign.right;
			break;
		case TabJustification.Centered:
			result = TextAlign.center;
			break;
		}
		return result;
	}

	private TabStopLeader GetTabStop(TabLeader tabLeader)
	{
		return tabLeader switch
		{
			TabLeader.Dotted => TabStopLeader.Dotted, 
			TabLeader.Heavy => TabStopLeader.Heavy, 
			TabLeader.Hyphenated => TabStopLeader.Hyphenated, 
			TabLeader.Single => TabStopLeader.Single, 
			_ => TabStopLeader.NoLeader, 
		};
	}

	private ODFFontFamily GetStyleType(StyleType styleType)
	{
		ODFFontFamily result = ODFFontFamily.Paragraph;
		switch (styleType)
		{
		case StyleType.ParagraphStyle:
			result = ODFFontFamily.Paragraph;
			break;
		case StyleType.CharacterStyle:
			result = ODFFontFamily.Text;
			break;
		case StyleType.TableStyle:
			result = ODFFontFamily.Table;
			break;
		case StyleType.OtherStyle:
			result = ODFFontFamily.Section;
			break;
		}
		return result;
	}

	internal void GetBody()
	{
		OTextBodyItem oTextBodyItem = null;
		odfstyleCollection1.Dispose();
		int fieldEndMarkIndex = 0;
		int breakIndex = -1;
		foreach (WSection section in m_document.Sections)
		{
			bool flag = false;
			string sectionStyleName = string.Empty;
			if (section.BreakCode == SectionBreakCode.NoBreak)
			{
				flag = true;
				odfStyle = new ODFStyle();
				odfStyle.Family = ODFFontFamily.Section;
				odfStyle.ODFSectionProperties = ConvertDocToODFSectionProperties(section);
				sectionStyleName = odfstyleCollection1.Add(odfStyle);
			}
			for (int i = 0; i < section.Body.Items.Count; i++)
			{
				TextBodyItem textBodyItem = section.Body.Items[i];
				switch (textBodyItem.EntityType)
				{
				case EntityType.Paragraph:
				{
					bool isInSameTextBody = false;
					bool isInSameParagraph = false;
					int fieldEndOwnerParagraphIndex = 0;
					oTextBodyItem = GetOParagraph(textBodyItem, ref fieldEndMarkIndex, ref isInSameParagraph, ref isInSameTextBody, ref fieldEndOwnerParagraphIndex, ref breakIndex, flag);
					if (i == 0 && flag)
					{
						oTextBodyItem.IsFirstItemOfSection = true;
						oTextBodyItem.SectionStyleName = sectionStyleName;
					}
					if (i == section.Body.Items.Count - 1 && flag)
					{
						oTextBodyItem.IsLastItemOfSection = true;
					}
					if ((oTextBodyItem as OParagraph).OParagraphItemCollection.Count > 0 && !isInSameParagraph)
					{
						OParagraphItem oParagraphItem = (oTextBodyItem as OParagraph).OParagraphItemCollection[(oTextBodyItem as OParagraph).OParagraphItemCollection.Count - 1];
						if (oParagraphItem is OMergeField || oParagraphItem is OField)
						{
							i = fieldEndOwnerParagraphIndex - 1;
						}
					}
					else
					{
						fieldEndMarkIndex = 0;
					}
					break;
				}
				case EntityType.Table:
					oTextBodyItem = GetTableContent(textBodyItem);
					break;
				}
				m_oDocument.Body.TextBodyItems.Add(oTextBodyItem);
				if (breakIndex != -1)
				{
					i--;
					fieldEndMarkIndex = breakIndex + 1;
					breakIndex = -1;
				}
			}
		}
		m_writer.SerializeAutoStyleStart();
		m_writer.SerializeContentAutoStyles(odfstyleCollection1);
		m_writer.SerializeContentListStyles(m_oDocument.ListStyles);
		m_writer.SerializeEnd();
		m_writer.SerializeDocIOContent(m_oDocument);
	}

	private DocGen.DocIO.ODF.Base.ODFImplementation.SectionProperties ConvertDocToODFSectionProperties(WSection section)
	{
		return new DocGen.DocIO.ODF.Base.ODFImplementation.SectionProperties
		{
			MarginRight = 0,
			MarginLeft = 0
		};
	}

	private OTextBodyItem GetOTextBodyItem(TextBodyItem TextbodyItem)
	{
		OTextBodyItem result = null;
		switch (TextbodyItem.EntityType)
		{
		case EntityType.Paragraph:
		{
			int fieldEndMarkIndex = 0;
			bool isInSameTextBody = false;
			bool isInSameParagraph = false;
			int fieldEndOwnerParagraphIndex = 0;
			int breakIndex = -1;
			result = GetOParagraph(TextbodyItem, ref fieldEndMarkIndex, ref isInSameParagraph, ref isInSameTextBody, ref fieldEndOwnerParagraphIndex, ref breakIndex, isContinuousSection: false);
			break;
		}
		case EntityType.Table:
			result = GetTableContent(TextbodyItem);
			break;
		}
		return result;
	}

	private OTextBodyItem GetOParagraph(TextBodyItem TextbodyItem, ref int fieldEndMarkIndex, ref bool isInSameParagraph, ref bool isInSameTextBody, ref int fieldEndOwnerParagraphIndex, ref int breakIndex, bool isContinuousSection)
	{
		OParagraph oParagraph = new OParagraph();
		Heading heading = null;
		WParagraph wParagraph = TextbodyItem as WParagraph;
		WParagraphFormat paragraphFormat = wParagraph.ParagraphFormat;
		ParagraphItemCollection items = wParagraph.Items;
		bool flag = false;
		odfStyle = new ODFStyle();
		IWParagraphStyle style = wParagraph.GetStyle();
		if (!IsWritingHeaderFooter && !isContinuousSection && !wParagraph.IsInCell && wParagraph.Index == 0 && pageNames.Count > 0 && pageNames.Count > 0)
		{
			odfStyle.MasterPageName = pageNames[0];
			flag = true;
			pageNames.RemoveAt(0);
		}
		odfStyle.ParentStyleName = wParagraph.StyleName;
		if (style != null && !style.Name.Equals("Normal") && (StartsWithExt(style.Name, "Heading") || StartsWithExt(style.Name, "Title")))
		{
			heading = new Heading();
			heading.StyleName = style.Name;
			oParagraph.Header = new Heading();
			if (style.CharacterFormat != null)
			{
				odfStyle.ParagraphProperties = CopyParaFormatToParagraphPropertiesFormat(style.ParagraphFormat);
				odfStyle.Textproperties = CopyCharFormatToTextFormat(style.CharacterFormat, odfStyle.Textproperties);
				odfStyle.Family = ODFFontFamily.Paragraph;
				oParagraph.Header.StyleName = heading.StyleName;
			}
		}
		else if (wParagraph.ListFormat.CurrentListStyle != null)
		{
			string name = wParagraph.ListFormat.CurrentListStyle.Name;
			for (int i = 0; i < m_oDocument.ListStyles.Count; i++)
			{
				if (m_oDocument.ListStyles[i].Name == name)
				{
					oParagraph.ListStyleName = m_oDocument.ListStyles[i].CurrentStyleName;
					oParagraph.ListLevelNumber = wParagraph.ListFormat.ListLevelNumber;
					break;
				}
			}
		}
		else if (paragraphFormat != null)
		{
			odfStyle.ParagraphProperties = CopyParaFormatToParagraphPropertiesFormat(paragraphFormat);
			if (odfStyle.ParagraphProperties != null)
			{
				odfStyle.Family = ODFFontFamily.Paragraph;
			}
		}
		if (wParagraph.ParagraphFormat.PropertiesHash.Count > 0)
		{
			odfStyle.ParagraphProperties = ResetInlineParagraphFormat(wParagraph.ParagraphFormat, odfStyle.ParagraphProperties);
		}
		if (flag)
		{
			odfStyle.ParagraphProperties.BeforeBreak = BeforeBreak.page;
		}
		else if (m_lastBreak != 0)
		{
			odfStyle.ParagraphProperties.BeforeBreak = m_lastBreak;
			m_lastBreak = BeforeBreak.auto;
		}
		odfstyleCollection1.Add(odfStyle);
		oParagraph.StyleName = odfStyle.Name;
		if (StartsWithExt(odfStyle.ParentStyleName, "TOC"))
		{
			oParagraph.TocMark = odfStyle.ParentStyleName;
			foreach (ODFStyle tOCStyle in m_oDocument.TOCStyles)
			{
				if (odfStyle.ParentStyleName == tOCStyle.Name && tOCStyle.ParagraphProperties.TabStops.Count == 0)
				{
					tOCStyle.ParagraphProperties = odfStyle.ParagraphProperties;
				}
			}
		}
		for (int j = fieldEndMarkIndex; j < items.Count; j++)
		{
			ParagraphItem paragraphItem = items[j];
			switch (paragraphItem.EntityType)
			{
			case EntityType.TextRange:
			{
				opara = new OTextRange();
				odfStyle = new ODFStyle();
				WTextRange wTextRange = paragraphItem as WTextRange;
				opara.TextProperties = CopyCharFormatToTextFormat(wTextRange.CharacterFormat, odfStyle.Textproperties);
				if (opara.TextProperties != null)
				{
					if (opara.TextProperties != null)
					{
						odfStyle = new ODFStyle();
						opara.Span = true;
						odfStyle.Family = ODFFontFamily.Text;
						odfStyle.Textproperties = opara.TextProperties;
					}
					odfstyleCollection1.Add(odfStyle);
					opara.StyleName = odfStyle.Name;
				}
				string text4 = CombineTextInSubsequentTextRanges(items, ref j);
				opara.Text = text4;
				oParagraph.OParagraphItemCollection.Add(opara);
				break;
			}
			case EntityType.Symbol:
			{
				opara = new OTextRange();
				odfStyle = new ODFStyle();
				WSymbol wSymbol = paragraphItem as WSymbol;
				opara.TextProperties = CopyCharFormatToTextFormat(wSymbol.CharacterFormat, odfStyle.Textproperties);
				if (opara.TextProperties == null && !string.IsNullOrEmpty(wSymbol.FontName))
				{
					opara.TextProperties = new TextProperties();
					opara.TextProperties.FontName = wSymbol.FontName;
				}
				if (opara.TextProperties != null)
				{
					if (opara.TextProperties != null)
					{
						odfStyle = new ODFStyle();
						opara.Span = true;
						odfStyle.Family = ODFFontFamily.Text;
						odfStyle.Textproperties = opara.TextProperties;
					}
					odfstyleCollection1.Add(odfStyle);
					opara.StyleName = odfStyle.Name;
				}
				opara.Text = Convert.ToChar(wSymbol.CharacterCode).ToString();
				oParagraph.OParagraphItemCollection.Add(opara);
				break;
			}
			case EntityType.Break:
			{
				opara = new OBreak();
				OBreakType breakType = (OBreakType)(paragraphItem as Break).BreakType;
				switch (breakType)
				{
				case OBreakType.PageBreak:
					(opara as OBreak).BreakType = OBreakType.PageBreak;
					m_lastBreak = BeforeBreak.page;
					if (j != items.Count - 1)
					{
						breakIndex = j;
						return oParagraph;
					}
					break;
				case OBreakType.ColumnBreak:
					(opara as OBreak).BreakType = OBreakType.ColumnBreak;
					m_lastBreak = BeforeBreak.column;
					if (j != items.Count - 1)
					{
						breakIndex = j;
						return oParagraph;
					}
					break;
				}
				if (breakType == OBreakType.LineBreak)
				{
					(opara as OBreak).BreakType = OBreakType.LineBreak;
					opara.ParagraphProperties = new ODFParagraphProperties();
					opara.ParagraphProperties.LineBreak = true;
				}
				oParagraph.OParagraphItemCollection.Add(opara);
				break;
			}
			case EntityType.MergeField:
			{
				OMergeField oMergeField = new OMergeField();
				WMergeField wMergeField = paragraphItem as WMergeField;
				List<WCharacterFormat> resultCharacterFormatting = wMergeField.GetResultCharacterFormatting();
				oMergeField.TextProperties = CopyCharFormatToTextFormat((resultCharacterFormatting.Count == 0) ? null : resultCharacterFormatting[0], odfStyle.Textproperties);
				if (oMergeField.TextProperties != null)
				{
					if (oMergeField.TextProperties != null)
					{
						odfStyle = new ODFStyle();
						oMergeField.Span = true;
						odfStyle.Family = ODFFontFamily.Text;
						odfStyle.Textproperties = oMergeField.TextProperties;
					}
					odfstyleCollection1.Add(odfStyle);
					oMergeField.StyleName = odfStyle.Name;
				}
				if (!string.IsNullOrEmpty(wMergeField.Text))
				{
					oMergeField.Text = wMergeField.Text;
				}
				if (!string.IsNullOrEmpty(wMergeField.FieldName))
				{
					oMergeField.FieldName = wMergeField.FieldName;
				}
				if (!string.IsNullOrEmpty(wMergeField.TextBefore))
				{
					oMergeField.TextBefore = wMergeField.TextBefore;
				}
				if (!string.IsNullOrEmpty(wMergeField.TextAfter))
				{
					oMergeField.TextAfter = wMergeField.TextAfter;
				}
				oParagraph.OParagraphItemCollection.Add(oMergeField);
				WFieldMark fieldEnd = wMergeField.FieldEnd;
				int indexInOwnerCollection = fieldEnd.GetIndexInOwnerCollection();
				if (wMergeField.OwnerParagraph == fieldEnd.OwnerParagraph)
				{
					j = indexInOwnerCollection++;
					isInSameTextBody = true;
					isInSameParagraph = true;
				}
				else if (wMergeField.OwnerParagraph.OwnerTextBody == fieldEnd.OwnerParagraph.OwnerTextBody)
				{
					WParagraph ownerParagraph = fieldEnd.OwnerParagraph;
					if (ownerParagraph != null)
					{
						fieldEndOwnerParagraphIndex = ownerParagraph.GetIndexInOwnerCollection();
					}
					isInSameTextBody = true;
					fieldEndMarkIndex = indexInOwnerCollection;
					return oParagraph;
				}
				break;
			}
			case EntityType.Field:
			{
				OField oField = new OField();
				WField wField = paragraphItem as WField;
				oField.TextProperties = CopyCharFormatToTextFormat(wField.CharacterFormat, odfStyle.Textproperties);
				if (oField.TextProperties != null)
				{
					if (oField.TextProperties != null)
					{
						odfStyle = new ODFStyle();
						oField.Span = true;
						odfStyle.Family = ODFFontFamily.Text;
						odfStyle.Textproperties = oField.TextProperties;
					}
					odfstyleCollection1.Add(odfStyle);
					oField.StyleName = odfStyle.Name;
				}
				oField.FormattingString = wField.FormattingString;
				oField.Text = wField.FieldResult;
				oField.FieldValue = wField.FieldValue;
				oField.Text = wField.Text;
				if (wField.CharacterFormat.HasValue(73))
				{
					oField.FieldCulture = wField.GetCulture((LocaleIDs)wField.CharacterFormat.LocaleIdASCII);
				}
				else
				{
					oField.FieldCulture = CultureInfo.CurrentCulture;
				}
				if (wField.FieldType == FieldType.FieldDate)
				{
					oField.OFieldType = OFieldType.FieldDate;
				}
				else if (wField.FieldType == FieldType.FieldHyperlink)
				{
					oField.OFieldType = OFieldType.FieldHyperlink;
				}
				else if (wField.FieldType == FieldType.FieldNumPages)
				{
					oField.OFieldType = OFieldType.FieldNumPages;
					string numberFormat = string.Empty;
					string text2 = wField.RemoveMergeFormat(wField.FormattingString, ref numberFormat);
					if (!string.IsNullOrEmpty(text2))
					{
						oField.PageNumberFormat = GetNumberFormat(text2);
					}
				}
				else if (wField.FieldType == FieldType.FieldPage)
				{
					oField.OFieldType = OFieldType.FieldPage;
					string numberFormat2 = string.Empty;
					string text3 = wField.RemoveMergeFormat(wField.FormattingString, ref numberFormat2);
					if (!string.IsNullOrEmpty(text3))
					{
						oField.PageNumberFormat = GetNumberFormat(text3);
					}
				}
				else if (wField.FieldType == FieldType.FieldAuthor)
				{
					oField.OFieldType = OFieldType.FieldAuthor;
				}
				else if (wField.FieldType == FieldType.FieldIf)
				{
					oField.OFieldType = OFieldType.FieldIf;
					opara = new OTextRange();
					opara.Text = wField.Text;
					oParagraph.OParagraphItemCollection.Add(opara);
				}
				if (oField.OFieldType != OFieldType.FieldIf)
				{
					oParagraph.OParagraphItemCollection.Add(oField);
				}
				WFieldMark fieldEnd2 = wField.FieldEnd;
				int indexInOwnerCollection2 = fieldEnd2.GetIndexInOwnerCollection();
				if (wField.OwnerParagraph == fieldEnd2.OwnerParagraph)
				{
					j = indexInOwnerCollection2++;
					isInSameTextBody = true;
					isInSameParagraph = true;
				}
				else if (wField.OwnerParagraph.OwnerTextBody == fieldEnd2.OwnerParagraph.OwnerTextBody)
				{
					WParagraph ownerParagraph2 = fieldEnd2.OwnerParagraph;
					if (ownerParagraph2 != null)
					{
						fieldEndOwnerParagraphIndex = ownerParagraph2.GetIndexInOwnerCollection();
					}
					isInSameTextBody = true;
					fieldEndMarkIndex = indexInOwnerCollection2;
					return oParagraph;
				}
				break;
			}
			case EntityType.BookmarkStart:
			{
				OBookmarkStart oBookmarkStart = new OBookmarkStart();
				oBookmarkStart.Name = (paragraphItem as BookmarkStart).Name;
				oParagraph.OParagraphItemCollection.Add(oBookmarkStart);
				break;
			}
			case EntityType.BookmarkEnd:
			{
				OBookmarkEnd oBookmarkEnd = new OBookmarkEnd();
				oBookmarkEnd.Name = (paragraphItem as BookmarkEnd).Name;
				oParagraph.OParagraphItemCollection.Add(oBookmarkEnd);
				break;
			}
			case EntityType.Picture:
			{
				OPicture oPicture = new OPicture();
				WPicture picture = paragraphItem as WPicture;
				odfStyle = new ODFStyle();
				GetOPicture(oPicture, picture, odfStyle);
				string text = UpdateShapeId(picture, isOlePicture: false, isPictureBullet: false, null);
				if (string.IsNullOrEmpty(oPicture.OPictureHRef))
				{
					oPicture.OPictureHRef = "media/image" + text;
				}
				oParagraph.OParagraphItemCollection.Add(oPicture);
				break;
			}
			}
		}
		return oParagraph;
	}

	private PageNumberFormat GetNumberFormat(string pageNumberFormat)
	{
		if (pageNumberFormat.EndsWith("ROMAN"))
		{
			return PageNumberFormat.UpperRoman;
		}
		if (pageNumberFormat.EndsWith("roman"))
		{
			return PageNumberFormat.LowerRoman;
		}
		if (pageNumberFormat.EndsWith("ALPHABET"))
		{
			return PageNumberFormat.UpperAlphabet;
		}
		if (pageNumberFormat.EndsWith("alphabetic"))
		{
			return PageNumberFormat.LowerAlphabet;
		}
		if (pageNumberFormat.EndsWith("Ordinal"))
		{
			return PageNumberFormat.Ordinal;
		}
		if (pageNumberFormat.EndsWith("OrdText"))
		{
			return PageNumberFormat.OrdinalText;
		}
		if (pageNumberFormat.EndsWith("Arabic"))
		{
			return PageNumberFormat.Arabic;
		}
		if (pageNumberFormat.EndsWith("CardText"))
		{
			return PageNumberFormat.CardinalText;
		}
		if (pageNumberFormat.EndsWith("Hex"))
		{
			return PageNumberFormat.Hexa;
		}
		if (pageNumberFormat.EndsWith("DollarText"))
		{
			return PageNumberFormat.DollorText;
		}
		if (pageNumberFormat.EndsWith("ARABICDASH") || pageNumberFormat.EndsWith("ArabicDash"))
		{
			return PageNumberFormat.ArabicDash;
		}
		return PageNumberFormat.Arabic;
	}

	private void GetOPicture(OPicture oPicture, WPicture picture, ODFStyle odfStyle)
	{
		oPicture.TextProperties = CopyCharFormatToTextFormat(picture.CharacterFormat, odfStyle.Textproperties);
		if (oPicture.TextProperties != null)
		{
			if (oPicture.TextProperties != null)
			{
				odfStyle = new ODFStyle();
				oPicture.Span = true;
				odfStyle.Family = ODFFontFamily.Text;
				odfStyle.Textproperties = oPicture.TextProperties;
			}
			odfstyleCollection1.Add(odfStyle);
			oPicture.StyleName = odfStyle.Name;
		}
		oPicture.Height = picture.Height;
		oPicture.Width = picture.Width;
		oPicture.HeightScale = picture.HeightScale;
		oPicture.WidthScale = picture.WidthScale;
		oPicture.HorizontalPosition = picture.HorizontalPosition;
		oPicture.VerticalPosition = picture.VerticalPosition;
		oPicture.OrderIndex = picture.OrderIndex;
		if (picture.TextWrappingStyle == DocGen.DocIO.DLS.TextWrappingStyle.Inline)
		{
			oPicture.TextWrappingStyle = DocGen.DocIO.ODF.Base.TextWrappingStyle.Inline;
		}
		else if (picture.TextWrappingStyle == DocGen.DocIO.DLS.TextWrappingStyle.Behind)
		{
			oPicture.TextWrappingStyle = DocGen.DocIO.ODF.Base.TextWrappingStyle.Behind;
		}
		else if (picture.TextWrappingStyle == DocGen.DocIO.DLS.TextWrappingStyle.InFrontOfText)
		{
			oPicture.TextWrappingStyle = DocGen.DocIO.ODF.Base.TextWrappingStyle.InFrontOfText;
		}
		else if (picture.TextWrappingStyle == DocGen.DocIO.DLS.TextWrappingStyle.Square)
		{
			oPicture.TextWrappingStyle = DocGen.DocIO.ODF.Base.TextWrappingStyle.Square;
		}
		else if (picture.TextWrappingStyle == DocGen.DocIO.DLS.TextWrappingStyle.Through)
		{
			oPicture.TextWrappingStyle = DocGen.DocIO.ODF.Base.TextWrappingStyle.Through;
		}
		else if (picture.TextWrappingStyle == DocGen.DocIO.DLS.TextWrappingStyle.Tight)
		{
			oPicture.TextWrappingStyle = DocGen.DocIO.ODF.Base.TextWrappingStyle.Tight;
		}
		else if (picture.TextWrappingStyle == DocGen.DocIO.DLS.TextWrappingStyle.TopAndBottom)
		{
			oPicture.TextWrappingStyle = DocGen.DocIO.ODF.Base.TextWrappingStyle.TopAndBottom;
		}
		if (!string.IsNullOrEmpty(picture.Name))
		{
			oPicture.Name = picture.Name;
		}
		oPicture.ShapeId = GetNextDocPrID();
		if (!string.IsNullOrEmpty(picture.OPictureHRef))
		{
			oPicture.OPictureHRef = picture.OPictureHRef;
		}
	}

	private string ConvertToValidXmlString(string text)
	{
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			char character = text[i];
			text2 = ((!IsValidXmlChar(character)) ? (text2 + XmlConvert.EncodeName(character.ToString())) : (text2 + character));
		}
		return text2;
	}

	private bool IsValidXmlChar(ushort character)
	{
		if (character != 9 && character != 10 && character != 13 && (character < 32 || character > 55295))
		{
			if (character >= 57344)
			{
				return character <= 65533;
			}
			return false;
		}
		return true;
	}

	private int GetNextDocPrID()
	{
		return ++m_docPrId;
	}

	private string UpdateShapeId(WPicture picture, bool isOlePicture, bool isPictureBullet, WOleObject oleObject)
	{
		string result = string.Empty;
		if (!isPictureBullet)
		{
			IEntity entity = (isOlePicture ? GetOleObjectOwner(oleObject) : GetPictureOwner(picture));
			if (entity is WSection || entity is WTextBox || entity is WTableRow || entity is WParagraph || entity is BlockContentControl || entity is Shape || entity is HeaderFooter)
			{
				result = AddImageRelation(m_oDocument.DocumentImages, picture.ImageRecord);
			}
		}
		else
		{
			result = AddImageRelation(m_oDocument.DocumentImages, picture.ImageRecord);
		}
		return result;
	}

	private string AddImageRelation(Dictionary<string, ImageRecord> imageCollection, ImageRecord imageRecord)
	{
		string text = string.Empty;
		if (imageCollection.ContainsValue(imageRecord))
		{
			foreach (string key in imageCollection.Keys)
			{
				if (imageRecord == imageCollection[key])
				{
					text = key;
					break;
				}
			}
		}
		else
		{
			text = GetNextRelationShipID();
			imageCollection.Add(text, imageRecord);
		}
		return text;
	}

	private string GetNextRelationShipID()
	{
		return $"rId{++m_relationShipID}";
	}

	private void ResetRelationShipID()
	{
		m_relationShipID = 0;
	}

	private IEntity GetPictureOwner(WPicture pic)
	{
		Entity entity = pic.Owner;
		WParagraph wParagraph = null;
		if (pic.Owner is WOleObject)
		{
			entity = (pic.Owner as WOleObject).OwnerParagraph;
		}
		if (entity.EntityType == EntityType.InlineContentControl)
		{
			wParagraph = entity.Owner as WParagraph;
		}
		else if (entity.EntityType == EntityType.Paragraph)
		{
			wParagraph = entity as WParagraph;
		}
		WTableCell wTableCell = wParagraph.Owner as WTableCell;
		Entity owner = wParagraph.Owner.Owner;
		IEntity entity2 = ((wTableCell == null) ? wParagraph.Owner : wTableCell.OwnerRow.OwnerTable.OwnerTextBody);
		owner = entity2.Owner;
		if (GetBaseEntity(pic) is HeaderFooter result)
		{
			return result;
		}
		return owner;
	}

	private IEntity GetOleObjectOwner(WOleObject oleObject)
	{
		WParagraph ownerParagraph = oleObject.OwnerParagraph;
		WTableCell wTableCell = ownerParagraph.Owner as WTableCell;
		Entity owner = ownerParagraph.Owner.Owner;
		IEntity entity = ((wTableCell == null) ? oleObject.OwnerParagraph.Owner : wTableCell.OwnerRow.OwnerTable.OwnerTextBody);
		owner = entity.Owner;
		if (GetBaseEntity(oleObject) is HeaderFooter result)
		{
			return result;
		}
		return owner;
	}

	private Entity GetBaseEntity(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WSection) && !(entity2 is HeaderFooter));
		return entity2;
	}

	private void GetTableBorder(WTable table, OTable table1)
	{
		odfStyle = new ODFStyle();
		odfStyle.Family = ODFFontFamily.Table;
		odfStyle.TableProperties = new OTableProperties();
		odfStyle.TableProperties.TableWidth = table.Width / 72f;
		odfStyle.TableProperties.MarginLeft = table.IndentFromLeft / 72f;
		if (table.TableFormat.HorizontalAlignment == RowAlignment.Right)
		{
			odfStyle.TableProperties.HoriAlignment = HoriAlignment.Right;
		}
		if (table.Index == 0 && pageNames.Count > 0 && pageNames.Count > 0)
		{
			for (int i = 0; i <= pageNames.Count; i++)
			{
				if (pageNames.Count != i)
				{
					odfStyle.MasterPageName = pageNames[i];
					odfStyle.ParentStyleName = table.StyleName;
				}
			}
		}
		odfstyleCollection1.Add(odfStyle);
		table1.StyleName = odfStyle.Name;
	}

	private void GetRowHeight(WTableRow row, OTableRow tableRow)
	{
		odfStyle = new ODFStyle();
		odfStyle.TableRowProperties = new OTableRowProperties();
		if (row.Height > 0f)
		{
			odfStyle.TableRowProperties = new OTableRowProperties();
			odfStyle.TableRowProperties.RowHeight = row.Height / 72f;
		}
		if (!row.RowFormat.IsBreakAcrossPages)
		{
			odfStyle.TableRowProperties.IsBreakAcrossPages = true;
		}
		if (row.RowFormat.IsHeaderRow)
		{
			odfStyle.TableRowProperties.IsHeaderRow = true;
		}
		odfStyle.Family = ODFFontFamily.Table_Row;
		odfstyleCollection1.Add(odfStyle);
		tableRow.StyleName = odfstyleCollection1.Add(odfStyle);
		odfstyleCollection1.Add(odfStyle);
		tableRow.StyleName = odfStyle.Name;
	}

	private Border GetRightBorder(int cellIndex, int cellLast, Borders borders, WTableCell m_cell)
	{
		Border border = borders.Right;
		if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
		{
			border = ((cellIndex != cellLast) ? m_cell.OwnerRow.RowFormat.Borders.Vertical : m_cell.OwnerRow.RowFormat.Borders.Right);
		}
		if (!border.IsBorderDefined)
		{
			border = ((cellIndex != cellLast) ? m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Vertical : m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Right);
		}
		return border;
	}

	private Border GetLeftBorder(int cellIndex, WTableCell m_cell, Borders borders)
	{
		Border border = borders.Left;
		if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
		{
			border = ((cellIndex != 0) ? m_cell.OwnerRow.RowFormat.Borders.Vertical : m_cell.OwnerRow.RowFormat.Borders.Left);
		}
		if (!border.IsBorderDefined)
		{
			border = ((cellIndex != 0) ? m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Vertical : m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Left);
		}
		return border;
	}

	private Border GetBottomBorder(int cellIndex, int cellLast, int rowIndex, int rowLast, WTableCell m_cell, Borders borders)
	{
		Border border = borders.Bottom;
		if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
		{
			border = ((rowIndex != rowLast) ? m_cell.OwnerRow.RowFormat.Borders.Horizontal : m_cell.OwnerRow.RowFormat.Borders.Bottom);
		}
		if (!border.IsBorderDefined)
		{
			border = ((rowIndex != rowLast) ? m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Horizontal : m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Bottom);
		}
		return border;
	}

	private Border GetTopBorder(int cellIndex, int rowIndex, Borders borders, WTableCell m_cell, int previousRowIndex)
	{
		Border border = borders.Top;
		if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
		{
			border = ((rowIndex != 0) ? m_cell.OwnerRow.RowFormat.Borders.Horizontal : m_cell.OwnerRow.RowFormat.Borders.Top);
		}
		if (!border.IsBorderDefined)
		{
			border = ((rowIndex != 0) ? m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Horizontal : m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Top);
		}
		return border;
	}

	private void GetCellStyle(WTableCell m_cell, TableStyleTableProperties tableStyle, OTableCell cell, Paddings paddings, WTableStyle tablebackcolor)
	{
		Borders borders = m_cell.CellFormat.Borders;
		int cellIndex = m_cell.GetCellIndex();
		int rowIndex = m_cell.OwnerRow.GetRowIndex();
		int cellLast = m_cell.OwnerRow.Cells.Count - 1;
		int rowLast = m_cell.OwnerRow.OwnerTable.Rows.Count - 1;
		Border topBorder = GetTopBorder(cellIndex, rowIndex, borders, m_cell, rowIndex - 1);
		Border bottomBorder = GetBottomBorder(cellIndex, cellLast, rowIndex, rowLast, m_cell, borders);
		Border leftBorder = GetLeftBorder(cellIndex, m_cell, borders);
		Border rightBorder = GetRightBorder(cellIndex, cellLast, borders, m_cell);
		if (topBorder.BorderType != 0 && topBorder.BorderType == bottomBorder.BorderType && topBorder.BorderType == leftBorder.BorderType && topBorder.BorderType == rightBorder.BorderType && topBorder.LineWidth != 0f && topBorder.LineWidth == bottomBorder.LineWidth && topBorder.LineWidth == leftBorder.LineWidth && topBorder.LineWidth == rightBorder.LineWidth && topBorder.Color != Color.Empty && topBorder.Color == bottomBorder.Color && topBorder.Color == leftBorder.Color && topBorder.Color == rightBorder.Color)
		{
			odfStyle = new ODFStyle();
			odfStyle.Family = ODFFontFamily.Table_Cell;
			odfStyle.TableCellProperties = new OTableCellProperties();
			odfStyle.TableCellProperties.Border = new ODFBorder();
			float num = topBorder.LineWidth / 72f;
			odfStyle.TableCellProperties.Border.LineWidth = num.ToString();
			odfStyle.TableCellProperties.Border.LineColor = topBorder.Color;
			odfStyle.TableCellProperties.Border.LineStyle = GetUnderlineStyle(topBorder.BorderType);
			if (paddings != null && m_cell.CellFormat.Paddings.Top == 0f && m_cell.CellFormat.Paddings.Bottom == 0f && m_cell.CellFormat.Paddings.Left == 0f && m_cell.CellFormat.Paddings.Right == 0f)
			{
				odfStyle.TableCellProperties.PaddingTop = paddings.Top / 72f;
				odfStyle.TableCellProperties.PaddingRight = paddings.Right / 72f;
				odfStyle.TableCellProperties.PaddingLeft = paddings.Left / 72f;
				odfStyle.TableCellProperties.PaddingBottom = paddings.Bottom / 72f;
			}
			else if (m_cell.CellFormat.Paddings.Bottom != 0f || m_cell.CellFormat.Paddings.Left != 0f || m_cell.CellFormat.Paddings.Right != 0f || m_cell.CellFormat.Paddings.Top != 0f)
			{
				odfStyle.TableCellProperties.PaddingTop = m_cell.CellFormat.Paddings.Top / 72f;
				odfStyle.TableCellProperties.PaddingRight = m_cell.CellFormat.Paddings.Right / 72f;
				odfStyle.TableCellProperties.PaddingLeft = m_cell.CellFormat.Paddings.Left / 72f;
				odfStyle.TableCellProperties.PaddingBottom = m_cell.CellFormat.Paddings.Bottom / 72f;
			}
			if (m_cell.CellFormat.BackColor != Color.Empty || m_cell.OwnerRow.RowFormat.BackColor != Color.Empty)
			{
				odfStyle.TableCellProperties.BackColor = m_cell.CellFormat.BackColor;
			}
			if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Bottom)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.bottom;
			}
			else if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Middle)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.middle;
			}
			else if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Top)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.top;
			}
			odfstyleCollection1.Add(odfStyle);
			cell.StyleName = odfStyle.Name;
		}
		else if (topBorder.LineWidth > 0f || topBorder.BorderType != 0 || topBorder.Color != Color.Empty || rightBorder.LineWidth > 0f || rightBorder.BorderType != 0 || rightBorder.Color != Color.Empty || bottomBorder.LineWidth > 0f || bottomBorder.BorderType != 0 || bottomBorder.Color != Color.Empty || leftBorder.LineWidth > 0f || leftBorder.BorderType != 0 || leftBorder.Color != Color.Empty)
		{
			odfStyle = new ODFStyle();
			odfStyle.Family = ODFFontFamily.Table_Cell;
			odfStyle.TableCellProperties = new OTableCellProperties();
			if (topBorder.LineWidth > 0f || topBorder.BorderType != 0 || topBorder.Color != Color.Empty)
			{
				odfStyle.TableCellProperties.BorderTop = new ODFBorder();
				if (paddings.Top > 0f)
				{
					odfStyle.TableCellProperties.PaddingTop = paddings.Top / 72f;
				}
				float num2 = topBorder.LineWidth / 72f;
				odfStyle.TableCellProperties.BorderTop.LineWidth = num2.ToString();
				odfStyle.TableCellProperties.BorderTop.LineStyle = GetUnderlineStyle(topBorder.BorderType);
				odfStyle.TableCellProperties.BorderTop.LineColor = topBorder.Color;
			}
			if (rightBorder.LineWidth > 0f || rightBorder.BorderType != 0 || rightBorder.Color != Color.Empty)
			{
				odfStyle.TableCellProperties.BorderRight = new ODFBorder();
				if (paddings.Right > 0f)
				{
					odfStyle.TableCellProperties.PaddingRight = paddings.Right / 72f;
				}
				float num3 = rightBorder.LineWidth / 72f;
				odfStyle.TableCellProperties.BorderRight.LineWidth = num3.ToString();
				odfStyle.TableCellProperties.BorderRight.LineStyle = GetUnderlineStyle(rightBorder.BorderType);
				odfStyle.TableCellProperties.BorderRight.LineColor = rightBorder.Color;
			}
			if (bottomBorder.LineWidth > 0f || bottomBorder.BorderType != 0 || bottomBorder.Color != Color.Empty)
			{
				odfStyle.TableCellProperties.BorderBottom = new ODFBorder();
				if (paddings.Bottom > 0f)
				{
					odfStyle.TableCellProperties.PaddingBottom = paddings.Bottom / 72f;
				}
				float num4 = bottomBorder.LineWidth / 72f;
				odfStyle.TableCellProperties.BorderBottom.LineWidth = num4.ToString();
				odfStyle.TableCellProperties.BorderBottom.LineStyle = GetUnderlineStyle(bottomBorder.BorderType);
				odfStyle.TableCellProperties.BorderBottom.LineColor = bottomBorder.Color;
			}
			if (leftBorder.LineWidth > 0f || leftBorder.BorderType != 0 || leftBorder.Color != Color.Empty)
			{
				odfStyle.TableCellProperties.BorderLeft = new ODFBorder();
				if (paddings.Left > 0f)
				{
					odfStyle.TableCellProperties.PaddingLeft = paddings.Left / 72f;
				}
				float num5 = leftBorder.LineWidth / 72f;
				odfStyle.TableCellProperties.BorderLeft.LineWidth = num5.ToString();
				odfStyle.TableCellProperties.BorderLeft.LineStyle = GetUnderlineStyle(leftBorder.BorderType);
				odfStyle.TableCellProperties.BorderLeft.LineColor = leftBorder.Color;
			}
			if (m_cell.CellFormat.BackColor != Color.Empty || m_cell.OwnerRow.RowFormat.BackColor != Color.Empty)
			{
				odfStyle.TableCellProperties.BackColor = m_cell.CellFormat.BackColor;
			}
			if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Bottom)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.bottom;
			}
			else if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Middle)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.middle;
			}
			else if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Top)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.top;
			}
			odfstyleCollection1.Add(odfStyle);
			cell.StyleName = odfStyle.Name;
		}
		else
		{
			if (tableStyle == null)
			{
				return;
			}
			if (tableStyle.Borders.Top.BorderType != 0 && tableStyle.Borders.Top.BorderType == tableStyle.Borders.Bottom.BorderType && tableStyle.Borders.Top.BorderType == tableStyle.Borders.Left.BorderType && tableStyle.Borders.Top.BorderType == tableStyle.Borders.Right.BorderType && tableStyle.Borders.Top.LineWidth != 0f && tableStyle.Borders.Top.LineWidth == tableStyle.Borders.Bottom.LineWidth && tableStyle.Borders.Top.LineWidth == tableStyle.Borders.Left.LineWidth && tableStyle.Borders.Top.LineWidth == tableStyle.Borders.Right.LineWidth && tableStyle.Borders.Top.Color != Color.Empty && tableStyle.Borders.Top.Color == tableStyle.Borders.Bottom.Color && tableStyle.Borders.Top.Color == tableStyle.Borders.Left.Color && tableStyle.Borders.Top.Color == tableStyle.Borders.Right.Color)
			{
				odfStyle = new ODFStyle();
				odfStyle.Family = ODFFontFamily.Table_Cell;
				odfStyle.TableCellProperties = new OTableCellProperties();
				if (bottomBorder.BorderType != 0 && topBorder.BorderType != 0 && leftBorder.BorderType != 0 && rightBorder.BorderType != 0)
				{
					odfStyle.TableCellProperties.Border = new ODFBorder();
					float num6 = tableStyle.Borders.Top.LineWidth / 72f;
					odfStyle.TableCellProperties.Border.LineWidth = num6.ToString();
					odfStyle.TableCellProperties.Border.LineColor = tableStyle.Borders.Top.Color;
					odfStyle.TableCellProperties.Border.LineStyle = GetUnderlineStyle(tableStyle.Borders.Top.BorderType);
				}
				if (tableStyle.Paddings != null)
				{
					odfStyle.TableCellProperties.PaddingTop = tableStyle.Paddings.Top / 72f;
					odfStyle.TableCellProperties.PaddingRight = tableStyle.Paddings.Right / 72f;
					odfStyle.TableCellProperties.PaddingLeft = tableStyle.Paddings.Left / 72f;
					odfStyle.TableCellProperties.PaddingBottom = tableStyle.Paddings.Bottom / 72f;
				}
				if (tablebackcolor.CellProperties.BackColor != Color.Empty)
				{
					odfStyle.TableCellProperties.BackColor = tablebackcolor.CellProperties.BackColor;
				}
				if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Bottom)
				{
					odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.bottom;
				}
				else if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Middle)
				{
					odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.middle;
				}
				else if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Top)
				{
					odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.top;
				}
				odfstyleCollection1.Add(odfStyle);
				cell.StyleName = odfStyle.Name;
				return;
			}
			odfStyle = new ODFStyle();
			odfStyle.Family = ODFFontFamily.Table_Cell;
			odfStyle.TableCellProperties = new OTableCellProperties();
			if (tableStyle.Borders.Top.LineWidth > 0f || tableStyle.Borders.Top.BorderType != 0 || tableStyle.Borders.Top.Color != Color.Empty)
			{
				odfStyle.TableCellProperties.BorderTop = new ODFBorder();
				if (tableStyle.Borders.Top.Space > 0f)
				{
					odfStyle.TableCellProperties.PaddingTop = tableStyle.Borders.Top.Space / 72f;
				}
				else if (m_cell.CellFormat.Paddings.Top > 0f)
				{
					odfStyle.TableCellProperties.PaddingTop = m_cell.CellFormat.Paddings.Top / 72f;
				}
				float num7 = tableStyle.Borders.Top.LineWidth / 72f;
				odfStyle.TableCellProperties.BorderTop.LineWidth = num7.ToString();
				odfStyle.TableCellProperties.BorderTop.LineStyle = GetUnderlineStyle(tableStyle.Borders.Top.BorderType);
				odfStyle.TableCellProperties.BorderTop.LineColor = tableStyle.Borders.Top.Color;
			}
			if (tableStyle.Borders.Right.LineWidth > 0f || tableStyle.Borders.Right.BorderType != 0 || tableStyle.Borders.Right.Color != Color.Empty)
			{
				odfStyle.TableCellProperties.BorderRight = new ODFBorder();
				if (tableStyle.Borders.Right.Space > 0f)
				{
					odfStyle.TableCellProperties.PaddingRight = tableStyle.Borders.Right.Space / 72f;
				}
				else if (m_cell.CellFormat.Paddings.Right > 0f)
				{
					odfStyle.TableCellProperties.PaddingRight = m_cell.CellFormat.Paddings.Right / 72f;
				}
				float num8 = tableStyle.Borders.Right.LineWidth / 72f;
				odfStyle.TableCellProperties.BorderRight.LineWidth = num8.ToString();
				odfStyle.TableCellProperties.BorderRight.LineStyle = GetUnderlineStyle(tableStyle.Borders.Right.BorderType);
				odfStyle.TableCellProperties.BorderRight.LineColor = tableStyle.Borders.Right.Color;
			}
			if (tableStyle.Borders.Bottom.LineWidth > 0f || tableStyle.Borders.Bottom.BorderType != 0 || tableStyle.Borders.Bottom.Color != Color.Empty)
			{
				odfStyle.TableCellProperties.BorderBottom = new ODFBorder();
				if (tableStyle.Borders.Bottom.Space > 0f)
				{
					odfStyle.TableCellProperties.PaddingBottom = tableStyle.Borders.Bottom.Space / 72f;
				}
				else if (m_cell.CellFormat.Paddings.Bottom > 0f)
				{
					odfStyle.TableCellProperties.PaddingBottom = m_cell.CellFormat.Paddings.Bottom / 72f;
				}
				float num9 = tableStyle.Borders.Bottom.LineWidth / 72f;
				odfStyle.TableCellProperties.BorderBottom.LineWidth = num9.ToString();
				odfStyle.TableCellProperties.BorderBottom.LineStyle = GetUnderlineStyle(tableStyle.Borders.Bottom.BorderType);
				odfStyle.TableCellProperties.BorderBottom.LineColor = tableStyle.Borders.Bottom.Color;
			}
			if (tableStyle.Borders.Left.LineWidth > 0f || tableStyle.Borders.Left.BorderType != 0 || tableStyle.Borders.Left.Color != Color.Empty)
			{
				odfStyle.TableCellProperties.BorderLeft = new ODFBorder();
				if (tableStyle.Borders.Left.Space > 0f)
				{
					odfStyle.TableCellProperties.PaddingLeft = tableStyle.Borders.Left.Space / 72f;
				}
				else if (m_cell.CellFormat.Paddings.Left > 0f)
				{
					odfStyle.TableCellProperties.PaddingLeft = m_cell.CellFormat.Paddings.Left / 72f;
				}
				float num10 = tableStyle.Borders.Left.LineWidth / 72f;
				odfStyle.TableCellProperties.BorderLeft.LineWidth = num10.ToString();
				odfStyle.TableCellProperties.BorderLeft.LineStyle = GetUnderlineStyle(tableStyle.Borders.Left.BorderType);
				odfStyle.TableCellProperties.BorderLeft.LineColor = tableStyle.Borders.Left.Color;
			}
			if (tablebackcolor.CellProperties.BackColor != Color.Empty)
			{
				odfStyle.TableCellProperties.BackColor = tablebackcolor.CellProperties.BackColor;
			}
			if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Bottom)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.bottom;
			}
			else if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Middle)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.middle;
			}
			else if (m_cell.CellFormat.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Top)
			{
				odfStyle.TableCellProperties.VerticalAlign = VerticalAlign.top;
			}
			odfstyleCollection1.Add(odfStyle);
			cell.StyleName = odfStyle.Name;
		}
	}

	private void GetColumnWidth(WTable table, OTableColumn cellColumn, OTable table1)
	{
		if (table.TableGrid.Count == 0)
		{
			return;
		}
		WTableColumnCollection tableGrid = table.TableGrid;
		float num = 0f;
		if (tableGrid.Count > 0)
		{
			int i = 0;
			for (int count = tableGrid.Count; i < count; i++)
			{
				cellColumn = new OTableColumn();
				float endOffset = tableGrid[i].EndOffset;
				double columnWidth = Math.Round(endOffset - num) / 1440.0;
				odfStyle = new ODFStyle();
				odfStyle.Family = ODFFontFamily.Table_Column;
				odfStyle.TableColumnProperties = new OTableColumnProperties();
				odfStyle.TableColumnProperties.ColumnWidth = columnWidth;
				odfstyleCollection1.Add(odfStyle);
				num = endOffset;
				cellColumn.StyleName = odfStyle.Name;
				table1.Columns.Add(cellColumn);
			}
		}
	}

	private Paddings GetCellPaddingBasedOnTable(WTableCell cell)
	{
		Paddings paddings = new Paddings();
		if (cell.OwnerRow.RowFormat.Paddings.HasKey(1))
		{
			paddings.Left = cell.OwnerRow.RowFormat.Paddings.Left;
		}
		else if (cell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(1))
		{
			paddings.Left = cell.OwnerRow.OwnerTable.TableFormat.Paddings.Left;
		}
		else if (cell.Document.ActualFormatType == FormatType.Doc)
		{
			paddings.Left = 0f;
		}
		else
		{
			paddings.Left = 5.4f;
		}
		if (cell.OwnerRow.RowFormat.Paddings.HasKey(4))
		{
			paddings.Right = cell.OwnerRow.RowFormat.Paddings.Right;
		}
		else if (cell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(4))
		{
			paddings.Right = cell.OwnerRow.OwnerTable.TableFormat.Paddings.Right;
		}
		else if (cell.Document.ActualFormatType == FormatType.Doc)
		{
			paddings.Right = 0f;
		}
		else
		{
			paddings.Right = 5.4f;
		}
		if (cell.OwnerRow.RowFormat.Paddings.HasKey(2))
		{
			paddings.Top = cell.OwnerRow.RowFormat.Paddings.Top;
		}
		else if (cell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(2))
		{
			paddings.Top = cell.OwnerRow.OwnerTable.TableFormat.Paddings.Top;
		}
		else
		{
			paddings.Top = 0f;
		}
		if (cell.OwnerRow.RowFormat.Paddings.HasKey(3))
		{
			paddings.Bottom = cell.OwnerRow.RowFormat.Paddings.Bottom;
		}
		else if (cell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(3))
		{
			paddings.Bottom = cell.OwnerRow.OwnerTable.TableFormat.Paddings.Bottom;
		}
		else
		{
			paddings.Bottom = 0f;
		}
		return paddings;
	}

	private OTextBodyItem GetTableContent(TextBodyItem TextbodyItem)
	{
		List<OTable> list = new List<OTable>();
		WTable wTable = TextbodyItem as WTable;
		WTableStyle wTableStyle = wTable.GetStyle() as WTableStyle;
		TableStyleTableProperties tableStyle = null;
		if (wTableStyle != null)
		{
			tableStyle = wTableStyle.TableProperties;
		}
		new OTableColumn();
		OTable oTable = new OTable();
		GetTableBorder(wTable, oTable);
		OTableColumn cellColumn = new OTableColumn();
		GetColumnWidth(wTable, cellColumn, oTable);
		WRowCollection rows = wTable.Rows;
		for (int i = 0; i < rows.Count; i++)
		{
			WTableRow wTableRow = rows[i];
			_ = wTableRow.RowFormat.IsBreakAcrossPages;
			OTableRow oTableRow = new OTableRow();
			GetRowHeight(wTableRow, oTableRow);
			if (wTableRow.Cells.Count > 0)
			{
				for (int j = 0; j < wTableRow.Cells.Count; j++)
				{
					WTableCell wTableCell = wTableRow.Cells[j];
					OTableCell oTableCell = new OTableCell();
					Paddings cellPaddingBasedOnTable = GetCellPaddingBasedOnTable(wTableCell);
					if (wTableCell.GridSpan > 1)
					{
						oTableCell.ColumnsSpanned = wTableCell.GridSpan;
					}
					GetCellStyle(wTableCell, tableStyle, oTableCell, cellPaddingBasedOnTable, wTableStyle);
					BodyItemCollection items = wTableCell.Items;
					for (int k = 0; k < items.Count; k++)
					{
						TextBodyItem textbodyItem = items[k];
						OTextBodyItem oTextBodyItem = GetOTextBodyItem(textbodyItem);
						oTableCell.TextBodyIetm.Add(oTextBodyItem);
					}
					oTableRow.Cells.Add(oTableCell);
				}
			}
			oTable.Rows.Add(oTableRow);
		}
		list.Add(oTable);
		return oTable;
	}

	private string CombineTextInSubsequentTextRanges(ParagraphItemCollection paraItemCollection, ref int index)
	{
		WTextRange wTextRange = paraItemCollection[index] as WTextRange;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(wTextRange.Text);
		while (wTextRange.NextSibling != null && wTextRange.NextSibling.EntityType == EntityType.TextRange && wTextRange.CharacterFormat.Compare((wTextRange.NextSibling as WTextRange).CharacterFormat))
		{
			wTextRange = wTextRange.NextSibling as WTextRange;
			stringBuilder.Append(wTextRange.Text);
			index++;
		}
		return stringBuilder.ToString();
	}

	internal void LoadHeaderFooterContents(WHeadersFooters headersFooters, MasterPage page)
	{
		IsWritingHeaderFooter = true;
		HeaderFooterContent headerFooterContent = null;
		if (headersFooters.OddHeader != null)
		{
			headerFooterContent = new HeaderFooterContent();
			page.Header = GetHeaderFooterContent(headerFooterContent, headersFooters.OddHeader);
		}
		if (headersFooters.EvenHeader != null)
		{
			headerFooterContent = new HeaderFooterContent();
			page.HeaderLeft = GetHeaderFooterContent(headerFooterContent, headersFooters.EvenHeader);
		}
		if (headersFooters.FirstPageHeader != null)
		{
			headerFooterContent = new HeaderFooterContent();
			page.FirstPageHeader = GetHeaderFooterContent(headerFooterContent, headersFooters.FirstPageHeader);
		}
		if (headersFooters.OddFooter != null)
		{
			headerFooterContent = new HeaderFooterContent();
			page.Footer = GetHeaderFooterContent(headerFooterContent, headersFooters.OddFooter);
		}
		if (headersFooters.EvenFooter != null)
		{
			headerFooterContent = new HeaderFooterContent();
			page.FooterLeft = GetHeaderFooterContent(headerFooterContent, headersFooters.EvenFooter);
		}
		if (headersFooters.FirstPageFooter != null)
		{
			headerFooterContent = new HeaderFooterContent();
			page.FirstPageFooter = GetHeaderFooterContent(headerFooterContent, headersFooters.FirstPageFooter);
		}
		IsWritingHeaderFooter = false;
	}

	internal HeaderFooterContent GetHeaderFooterContent(HeaderFooterContent oHeaderFooter, HeaderFooter headerFooter)
	{
		foreach (TextBodyItem childEntity in headerFooter.ChildEntities)
		{
			OTextBodyItem oTextBodyItem = GetOTextBodyItem(childEntity);
			oHeaderFooter.ChildItems.Add(oTextBodyItem);
		}
		if (oHeaderFooter.ChildItems.Count <= 0)
		{
			return null;
		}
		return oHeaderFooter;
	}

	private void ConvertAutomaticAndMasterStyles()
	{
		PageLayoutCollection pageLayoutCollection = new PageLayoutCollection();
		MasterPageCollection masterPageCollection = new MasterPageCollection();
		WPageSetup wPageSetup = null;
		for (int i = 0; i < m_document.Sections.Count; i++)
		{
			PageLayout pageLayout = new PageLayout();
			WSection wSection = m_document.Sections[i];
			if (wSection.Columns.Count > 1)
			{
				pageLayout.ColumnsCount = wSection.Columns.Count;
				pageLayout.ColumnsGap = wSection.Columns[0].Space / 72f;
			}
			bool num = wSection.BreakCode == SectionBreakCode.NoBreak;
			string text = pageLayoutCollection.Add(pageLayout);
			if (num)
			{
				pageLayoutCollection.Remove(text);
			}
			MasterPage masterPage = new MasterPage();
			LoadHeaderFooterContents(wSection.HeadersFooters, masterPage);
			masterPage.PageLayoutName = text;
			if (!num)
			{
				wPageSetup = wSection.PageSetup;
				ConvertPageLayOut(wPageSetup, pageLayout, masterPage);
			}
			string text2 = masterPageCollection.Add(masterPage);
			pageNames.Add(text2);
			if (num)
			{
				masterPageCollection.Remove(text2);
				pageNames.Remove(text2);
			}
		}
		m_writer.SerializeAutomaticStyles(pageLayoutCollection);
		if (m_document.ListStyles != null && m_document.ListStyles.Count > 0)
		{
			MapListStyles();
		}
		if (odfstyleCollection1.DictStyles.Count > 0)
		{
			m_writer.SerializeContentAutoStyles(odfstyleCollection1);
		}
		m_writer.SerializeEnd();
		if (m_oDocument.ListStyles != null && m_oDocument.ListStyles.Count > 0)
		{
			m_writer.SerializeContentListStyles(m_oDocument.ListStyles);
		}
		m_writer.SerializeMasterStylesStart();
		m_writer.SerializeMasterStyles(masterPageCollection, pageNames);
		m_writer.SerializeEnd();
	}

	private void MapListStyles()
	{
		odfStyle = new ODFStyle();
		foreach (ListStyle listStyle in m_document.ListStyles)
		{
			OListStyle oListStyle = new OListStyle();
			int count = m_oDocument.ListStyles.Count;
			ODFStyle oDFStyle = odfStyle;
			int num = ++count;
			oDFStyle.ListStyleName = "LFO" + num;
			oListStyle.Name = listStyle.Name;
			oListStyle.CurrentStyleName = odfStyle.ListStyleName;
			int num2 = 1;
			for (int i = 0; i < listStyle.Levels.Count; i++)
			{
				WListLevel wListLevel = listStyle.Levels[i];
				ListLevelProperties listLevelProperties = new ListLevelProperties();
				if (wListLevel.CharacterFormat != null && !wListLevel.CharacterFormat.IsDefault)
				{
					listLevelProperties.Style = new ODFStyle();
					listLevelProperties.Style.Textproperties = CopyCharFormatToTextFormat(wListLevel.CharacterFormat, listLevelProperties.Style.Textproperties);
					if (listLevelProperties.Style.Textproperties != null)
					{
						listLevelProperties.Style.Family = ODFFontFamily.Text;
						listLevelProperties.Style.Name = "WWChar" + odfStyle.ListStyleName + "LVL" + num2;
					}
					odfstyleCollection1.Add(listLevelProperties.Style);
				}
				if (listLevelProperties.Style != null && listLevelProperties.Style.Textproperties != null)
				{
					listLevelProperties.TextProperties = listLevelProperties.Style.Textproperties;
				}
				if (wListLevel.PatternType == ListPatternType.Arabic)
				{
					listLevelProperties.NumberFormat = ListNumberFormat.Decimal;
				}
				else if (wListLevel.PatternType == ListPatternType.LowRoman)
				{
					listLevelProperties.NumberFormat = ListNumberFormat.LowerRoman;
				}
				else if (wListLevel.PatternType == ListPatternType.UpLetter)
				{
					listLevelProperties.NumberFormat = ListNumberFormat.UpperLetter;
				}
				else if (wListLevel.PatternType == ListPatternType.UpRoman)
				{
					listLevelProperties.NumberFormat = ListNumberFormat.UpperRoman;
				}
				else if (wListLevel.PatternType == ListPatternType.LowLetter)
				{
					listLevelProperties.NumberFormat = ListNumberFormat.LowerLetter;
				}
				else if (wListLevel.PatternType == ListPatternType.Bullet)
				{
					listLevelProperties.NumberFormat = ListNumberFormat.Bullet;
				}
				listLevelProperties.NumberSufix = wListLevel.NumberSuffix;
				listLevelProperties.LeftMargin = wListLevel.ParagraphFormat.LeftIndent / 72f;
				listLevelProperties.TextIndent = wListLevel.ParagraphFormat.FirstLineIndent / 72f;
				listLevelProperties.SpaceBefore = listLevelProperties.LeftMargin - Math.Abs(listLevelProperties.TextIndent);
				listLevelProperties.MinimumLabelWidth = Math.Abs(listLevelProperties.TextIndent);
				listLevelProperties.TextAlignment = GetListLevelAlingment(wListLevel.NumberAlignment);
				if (!string.IsNullOrEmpty(wListLevel.BulletCharacter))
				{
					listLevelProperties.BulletCharacter = wListLevel.BulletCharacter;
				}
				if (wListLevel.PicBullet != null)
				{
					OPicture oPicture = new OPicture();
					WPicture picBullet = wListLevel.PicBullet;
					odfStyle = new ODFStyle();
					GetOPicture(oPicture, picBullet, odfStyle);
					UpdateShapeId(picBullet, isOlePicture: false, isPictureBullet: true, null);
					listLevelProperties.PictureBullet = oPicture;
					listLevelProperties.PictureHRef = picBullet.OPictureHRef;
					listLevelProperties.IsPictureBullet = true;
				}
				oListStyle.ListLevels.Add(listLevelProperties);
				num2++;
			}
			m_oDocument.ListStyles.Add(oListStyle);
		}
	}

	private TextAlign GetListLevelAlingment(ListNumberAlignment numberAlignment)
	{
		return numberAlignment switch
		{
			ListNumberAlignment.Left => TextAlign.start, 
			ListNumberAlignment.Center => TextAlign.center, 
			ListNumberAlignment.Right => TextAlign.end, 
			_ => TextAlign.start, 
		};
	}

	private void ConvertPageLayOut(WPageSetup pageSetup, PageLayout layout, MasterPage masterPage)
	{
		layout.PageLayoutProperties.PageWidth = pageSetup.PageSize.Width / 72f;
		layout.PageLayoutProperties.PageHeight = pageSetup.PageSize.Height / 72f;
		if (!pageSetup.Borders.NoBorder)
		{
			layout.PageLayoutProperties.MarginTop = pageSetup.Borders.Top.Space / 72f;
			layout.PageLayoutProperties.MarginBottom = pageSetup.Borders.Bottom.Space / 72f;
			layout.PageLayoutProperties.MarginLeft = pageSetup.Borders.Left.Space / 72f;
			layout.PageLayoutProperties.MarginRight = pageSetup.Borders.Right.Space / 72f;
			layout.PageLayoutProperties.Border = new ODFBorder();
			float num = pageSetup.Borders.Top.LineWidth / 72f;
			layout.PageLayoutProperties.Border.LineWidth = num.ToString();
			layout.PageLayoutProperties.Border.LineColor = pageSetup.Borders.Top.Color;
			layout.PageLayoutProperties.Border.LineStyle = GetUnderlineStyle(pageSetup.Borders.Top.BorderType);
		}
		if (pageSetup.Margins != null)
		{
			layout.PageLayoutProperties.MarginTop = pageSetup.Margins.Top / 72f;
			layout.PageLayoutProperties.MarginBottom = pageSetup.Margins.Bottom / 72f;
			layout.PageLayoutProperties.MarginLeft = pageSetup.Margins.Left / 72f;
			layout.PageLayoutProperties.MarginRight = pageSetup.Margins.Right / 72f;
		}
		layout.PageLayoutProperties.PageOrientation = (PrintOrientation)pageSetup.Orientation;
		if (masterPage.Header != null || masterPage.HeaderLeft != null || masterPage.FirstPageHeader != null || masterPage.Footer != null || masterPage.FooterLeft != null || masterPage.FirstPageFooter != null)
		{
			double num2 = pageSetup.HeaderDistance / 72f;
			double marginTop = layout.PageLayoutProperties.MarginTop;
			if (marginTop > num2)
			{
				layout.HeaderStyle.HeaderDistance = marginTop - num2;
			}
			layout.PageLayoutProperties.MarginTop = num2;
			double num3 = pageSetup.FooterDistance / 72f;
			double marginBottom = layout.PageLayoutProperties.MarginBottom;
			if (marginBottom > num3)
			{
				layout.FooterStyle.FooterDistance = marginBottom - num3;
			}
			layout.PageLayoutProperties.MarginBottom = num3;
		}
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	internal void Close()
	{
		if (m_oDocument != null)
		{
			m_oDocument.Close();
			m_oDocument = null;
		}
		if (paragraph != null)
		{
			paragraph.Dispose();
			paragraph = null;
		}
		if (opara != null)
		{
			opara.Dispose();
			opara = null;
		}
		if (odfStyle != null)
		{
			odfStyle.Close();
			odfStyle = null;
		}
		if (m_writer != null)
		{
			m_writer.Dispose();
			m_writer = null;
		}
		if (odfstyleCollection1 != null)
		{
			odfstyleCollection1.Dispose();
			odfstyleCollection1 = null;
		}
	}
}
