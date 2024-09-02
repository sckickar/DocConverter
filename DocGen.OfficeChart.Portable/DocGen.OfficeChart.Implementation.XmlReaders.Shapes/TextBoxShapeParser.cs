using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DocGen.Compression;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.XmlReaders.Shapes;

internal class TextBoxShapeParser
{
	public static void ParseTextBox(ITextBox textBox, XmlReader reader, Excel2007Parser parser)
	{
		ParseTextBox(textBox, reader, parser, null);
	}

	internal static void ParseTextBox(ITextBox textBox, XmlReader reader, Excel2007Parser parser, List<string> lstRelationIds)
	{
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		reader.Read();
		ShapeImpl shapeImpl = textBox as ShapeImpl;
		shapeImpl.HasLineFormat = false;
		shapeImpl.HasFill = false;
		bool flag = false;
		TextBoxShapeImpl textBoxShapeImpl = textBox as TextBoxShapeImpl;
		while (reader.NodeType != 0)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "txBody":
					ParseRichText(reader, parser, textBox);
					break;
				case "spPr":
					ParseShapeProperties(textBox, reader, parser);
					break;
				case "nvSpPr":
					ParseNonVisualShapeProperties(textBox as IShape, reader, parser);
					break;
				case "style":
				{
					flag = true;
					shapeImpl.StyleStream = ShapeParser.ReadNodeAsStream(reader);
					XmlReader xmlReader = UtilityMethods.CreateReader(shapeImpl.StyleStream, "lnRef");
					ShapeLineFormatImpl shapeLineFormatImpl = textBoxShapeImpl.Line as ShapeLineFormatImpl;
					int num = -1;
					if (xmlReader.MoveToAttribute("idx"))
					{
						num = int.Parse(xmlReader.Value);
					}
					if ((!textBoxShapeImpl.IsLineProperties || !shapeLineFormatImpl.IsNoFill) && !shapeLineFormatImpl.IsSolidFill)
					{
						switch (num)
						{
						case 0:
							shapeLineFormatImpl.Visible = false;
							break;
						default:
							shapeLineFormatImpl.Visible = true;
							break;
						case -1:
							break;
						}
					}
					shapeLineFormatImpl.DefaultLineStyleIndex = num;
					xmlReader = UtilityMethods.CreateReader(shapeImpl.StyleStream, "fillRef");
					if (!textBoxShapeImpl.IsNoFill && !textBoxShapeImpl.IsFill && xmlReader.MoveToAttribute("idx"))
					{
						if (int.Parse(xmlReader.Value) == 0)
						{
							textBoxShapeImpl.Fill.Visible = false;
						}
						else
						{
							textBoxShapeImpl.Fill.Visible = true;
						}
					}
					break;
				}
				case "solidFill":
				{
					IInternalFill fill = (textBox as TextBoxShapeImpl).Fill as IInternalFill;
					ChartParserCommon.ParseSolidFill(reader, parser, fill);
					break;
				}
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
		if (textBoxShapeImpl != null && !flag && !textBoxShapeImpl.IsFill && !textBoxShapeImpl.IsNoFill)
		{
			textBoxShapeImpl.Fill.Visible = false;
		}
	}

	private static void ParseNonVisualShapeProperties(IShape shape, XmlReader reader, Excel2007Parser parser)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (parser == null)
		{
			throw new ArgumentNullException("parser");
		}
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "cNvPr")
					{
						Excel2007Parser.ParseNVCanvasProperties(reader, shape);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					reader.Read();
				}
			}
		}
		reader.Read();
	}

	private static void ParseShapeProperties(ITextBox textBox, XmlReader reader, Excel2007Parser parser)
	{
		reader.Read();
		TextBoxShapeImpl textBoxShapeImpl = (TextBoxShapeImpl)textBox;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "solidFill":
				{
					IInternalFill fill2 = textBoxShapeImpl.Fill as IInternalFill;
					ChartParserCommon.ParseSolidFill(reader, parser, fill2);
					textBoxShapeImpl.IsFill = true;
					break;
				}
				case "noFill":
				{
					IInternalFill fill2 = textBoxShapeImpl.Fill as IInternalFill;
					fill2.Visible = false;
					textBoxShapeImpl.IsNoFill = true;
					reader.Read();
					break;
				}
				case "ln":
				{
					textBoxShapeImpl.IsLineProperties = true;
					ShapeLineFormatImpl border = (ShapeLineFormatImpl)textBoxShapeImpl.Line;
					ParseLineProperties(reader, border, bRoundCorners: false, parser);
					break;
				}
				case "xfrm":
					if (reader.MoveToAttribute("rot"))
					{
						textBoxShapeImpl.ShapeRotation = (int)Math.Round((double)Convert.ToInt64(reader.Value) / 60000.0);
					}
					if (reader.MoveToAttribute("flipH"))
					{
						textBoxShapeImpl.FlipHorizontal = XmlConvertExtension.ToBoolean(reader.Value);
					}
					if (reader.MoveToAttribute("flipV"))
					{
						textBoxShapeImpl.FlipVertical = XmlConvertExtension.ToBoolean(reader.Value);
					}
					textBoxShapeImpl.Coordinates2007 = ParseForm(reader);
					break;
				case "gradFill":
				{
					Stream stream = ShapeParser.ReadNodeAsStream(reader);
					stream.Position = 0L;
					XmlReader reader2 = UtilityMethods.CreateReader(stream);
					IInternalFill obj = textBoxShapeImpl.Fill as IInternalFill;
					obj.FillType = OfficeFillType.Gradient;
					obj.PreservedGradient = ChartParserCommon.ParseGradientFill(reader2, parser);
					textBoxShapeImpl.IsFill = true;
					break;
				}
				case "prstGeom":
					if (reader.LocalName == "prstGeom")
					{
						string presetGeometry = "";
						if (reader.MoveToAttribute("prst"))
						{
							presetGeometry = reader.Value;
						}
						textBoxShapeImpl.PresetGeometry = presetGeometry;
						reader.Skip();
					}
					break;
				case "grpFill":
					textBoxShapeImpl.IsFill = true;
					textBoxShapeImpl.IsGroupFill = true;
					reader.Skip();
					break;
				case "blipFill":
				{
					textBoxShapeImpl.IsFill = true;
					IInternalFill fill = textBoxShapeImpl.Fill as IInternalFill;
					ChartParserCommon.ParsePictureFill(reader, fill, textBoxShapeImpl.Worksheet.DataHolder.DrawingsRelations, textBoxShapeImpl.ParentWorkbook.DataHolder);
					break;
				}
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Read();
			}
		}
	}

	private static Rectangle ParseForm(XmlReader reader)
	{
		Rectangle result = Rectangle.Empty;
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			int x = 0;
			int y = 0;
			int width = 0;
			int height = 0;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "off"))
					{
						if (localName == "ext")
						{
							if (reader.MoveToAttribute("cx"))
							{
								width = XmlConvertExtension.ToInt32(reader.Value);
							}
							if (reader.MoveToAttribute("cy"))
							{
								height = XmlConvertExtension.ToInt32(reader.Value);
							}
						}
						else
						{
							reader.Skip();
						}
					}
					else
					{
						if (reader.MoveToAttribute("x"))
						{
							x = XmlConvertExtension.ToInt32(reader.Value);
						}
						if (reader.MoveToAttribute("y"))
						{
							y = XmlConvertExtension.ToInt32(reader.Value);
						}
					}
				}
				else
				{
					reader.Skip();
				}
			}
			result = new Rectangle(x, y, width, height);
		}
		reader.Read();
		return result;
	}

	private static void ParseRichText(XmlReader reader, Excel2007Parser parser, ITextBox textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		RichTextString richTextString = textBox.RichText as RichTextString;
		if (reader.MoveToAttribute("fLocksText"))
		{
			textBox.IsTextLocked = XmlConvertExtension.ToBoolean(reader.Value);
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "bodyPr":
					ParseBodyProperties(reader, richTextString, textBox);
					break;
				case "lstStyle":
					ParseListStyles(reader, richTextString);
					break;
				case "p":
					ParseParagraphs(reader, textBox, parser);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		richTextString.TextObject.Defragment();
		reader.Read();
	}

	private static void ParseBodyProperties(XmlReader reader, RichTextString textArea, ITextBox textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "bodyPr")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		TextBoxShapeImpl textBoxShapeImpl = textBox as TextBoxShapeImpl;
		if (reader.HasAttributes)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			reader.MoveToFirstAttribute();
			int i = 0;
			for (int attributeCount = reader.AttributeCount; i < attributeCount; i++)
			{
				dictionary[reader.LocalName] = reader.Value;
				switch (reader.LocalName)
				{
				case "lIns":
					textBoxShapeImpl.TextBodyPropertiesHolder.SetLeftMargin(int.Parse(reader.Value));
					break;
				case "rIns":
					textBoxShapeImpl.TextBodyPropertiesHolder.SetRightMargin(int.Parse(reader.Value));
					break;
				case "bIns":
					textBoxShapeImpl.TextBodyPropertiesHolder.SetBottomMargin(int.Parse(reader.Value));
					break;
				case "tIns":
					textBoxShapeImpl.TextBodyPropertiesHolder.SetTopMargin(int.Parse(reader.Value));
					break;
				}
				reader.MoveToNextAttribute();
			}
			if (dictionary.ContainsKey("anchor"))
			{
				ParseAnchor(dictionary["anchor"], textBox);
				dictionary.Remove("anchor");
			}
			if (dictionary.ContainsKey("vert"))
			{
				ParseTextRotation(dictionary["vert"], textBox);
				dictionary.Remove("vert");
			}
			dictionary.Remove("a");
			(textBox as TextBoxShapeBase).UnknownBodyProperties = dictionary;
		}
		reader.MoveToElement();
		reader.Skip();
	}

	private static void ParseTextRotation(string rotationValue, ITextBox textBox)
	{
		Excel2007TextRotation textRotation = (Excel2007TextRotation)Enum.Parse(typeof(Excel2007TextRotation), rotationValue, ignoreCase: false);
		textBox.TextRotation = (OfficeTextRotation)textRotation;
	}

	private static void ParseAnchor(string anchorValue, ITextBox textBox)
	{
		Excel2007CommentVAlign vAlignment = (Excel2007CommentVAlign)Enum.Parse(typeof(Excel2007CommentVAlign), anchorValue, ignoreCase: false);
		textBox.VAlignment = (OfficeCommentVAlign)vAlignment;
	}

	private static void ParseListStyles(XmlReader reader, RichTextString textArea)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "lstStyle")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Skip();
	}

	private static void ParseParagraphs(XmlReader reader, ITextBox textBox, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (reader.LocalName != "p")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		RichTextString richTextString = textBox.RichText as RichTextString;
		string text = richTextString.Text;
		if (text != null && text.Length != 0 && !text.EndsWith("\n"))
		{
			richTextString.AddText("\n", richTextString.GetFont(text.Length - 1));
		}
		reader.Read();
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "pPr":
					ParseParagraphProperites(reader, textBox);
					break;
				case "r":
					ParseParagraphRun(reader, richTextString, parser, textBox);
					break;
				case "endParaRPr":
					ParseParagraphEnd(reader, richTextString, parser);
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	internal static void ParseParagraphEnd(XmlReader reader, RichTextString textArea, Excel2007Parser parser)
	{
		IOfficeFont font = ParseParagraphRunProperites(reader, textArea, parser);
		textArea.AddText("\n", font);
	}

	private static void ParseParagraphProperites(XmlReader reader, ITextBox textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		if (reader.MoveToAttribute("algn"))
		{
			Excel2007CommentHAlign hAlignment = (Excel2007CommentHAlign)Enum.Parse(typeof(Excel2007CommentHAlign), reader.Value, ignoreCase: false);
			textBox.HAlignment = (OfficeCommentHAlign)hAlignment;
		}
		reader.MoveToElement();
		reader.Skip();
	}

	internal static void ParseParagraphRun(XmlReader reader, RichTextString textArea, Excel2007Parser parser, ITextBox textBox)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (reader.LocalName != "r")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		reader.Read();
		string text = null;
		IOfficeFont officeFont = null;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "rPr"))
				{
					if (localName == "t")
					{
						text = reader.ReadElementContentAsString();
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					officeFont = ParseParagraphRunProperites(reader, textArea, parser);
				}
			}
			else
			{
				reader.Skip();
			}
		}
		if (text == null || text.Length == 0)
		{
			text = "\n";
		}
		if (textBox != null)
		{
			(officeFont as FontImpl).ParaAlign = (Excel2007CommentHAlign)textBox.HAlignment;
			(officeFont as FontImpl).HasParagrapAlign = true;
		}
		textArea.AddText(text, officeFont);
		reader.Read();
	}

	private static FontImpl ParseParagraphRunProperites(XmlReader reader, RichTextString textArea, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		WorkbookImpl workbook = textArea.Workbook;
		FontImpl fontImpl = (FontImpl)workbook.CreateFont(workbook.InnerFonts[0], bAddToCollection: false);
		if (reader.MoveToAttribute("b"))
		{
			fontImpl.Bold = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("i"))
		{
			fontImpl.Italic = XmlConvertExtension.ToBoolean(reader.Value);
		}
		if (reader.MoveToAttribute("strike"))
		{
			fontImpl.Strikethrough = reader.Value != "noStrike";
		}
		if (reader.MoveToAttribute("sz"))
		{
			fontImpl.Size = (double)int.Parse(reader.Value) / 100.0;
		}
		if (reader.MoveToAttribute("u"))
		{
			if (reader.Value == "sng")
			{
				fontImpl.Underline = OfficeUnderline.Single;
			}
			else if (reader.Value == "dbl")
			{
				fontImpl.Underline = OfficeUnderline.Double;
			}
		}
		fontImpl.showFontName = false;
		reader.MoveToElement();
		if (!reader.IsEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					string localName = reader.LocalName;
					if (!(localName == "latin"))
					{
						if (localName == "solidFill")
						{
							ChartParserCommon.ParseSolidFill(reader, parser, fontImpl.ColorObject);
						}
						else
						{
							reader.Skip();
						}
						continue;
					}
					if (reader.MoveToAttribute("typeface"))
					{
						fontImpl.FontName = reader.Value;
						reader.MoveToElement();
					}
					fontImpl.showFontName = true;
					reader.Skip();
				}
				else
				{
					reader.Skip();
				}
			}
		}
		reader.Skip();
		return fontImpl;
	}

	internal static void ParseLineProperties(XmlReader reader, ShapeLineFormatImpl border, bool bRoundCorners, Excel2007Parser parser)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (border == null)
		{
			throw new ArgumentNullException("border");
		}
		if (reader.LocalName != "ln")
		{
			throw new XmlException("Unexpected xml tag");
		}
		bool isEmptyElement = reader.IsEmptyElement;
		if (reader.MoveToAttribute("w"))
		{
			int num = (int)Math.Round((double)int.Parse(reader.Value) / 12700.0);
			border.Weight = num;
		}
		if (reader.MoveToAttribute("cmpd"))
		{
			Excel2007ShapeLineStyle style = (Excel2007ShapeLineStyle)Enum.Parse(typeof(Excel2007ShapeLineStyle), reader.Value, ignoreCase: false);
			border.Style = (OfficeShapeLineStyle)style;
		}
		bool flag = false;
		string text = null;
		if (!isEmptyElement)
		{
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
					case "noFill":
						border.Weight = 0.0;
						border.Visible = false;
						border.IsNoFill = true;
						reader.Read();
						break;
					case "round":
						border.IsRound = true;
						reader.Skip();
						break;
					case "solidFill":
					{
						ChartColor chartColor = new ChartColor(OfficeKnownColors.Black);
						ChartParserCommon.ParseSolidFill(reader, parser, chartColor);
						border.ForeColor = chartColor.GetRGB(border.Workbook);
						flag = true;
						break;
					}
					case "prstDash":
						text = ChartParserCommon.ParseValueTag(reader);
						break;
					case "pattFill":
						reader.Skip();
						break;
					case "headEnd":
						ParseArrowSettings(reader, border, isHead: true);
						break;
					case "tailEnd":
						ParseArrowSettings(reader, border, isHead: false);
						break;
					default:
						reader.Skip();
						break;
					}
				}
				else
				{
					reader.Skip();
				}
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			border.DashStyle = OfficeShapeDashLineStyle.Solid;
			if (text != null)
			{
				border.AppImplementation.StringEnum.LineDashTypeXmltoEnum.TryGetValue(text, out var value);
				border.DashStyle = value;
			}
		}
		reader.Read();
	}

	private static void ParseArrowSettings(XmlReader reader, ShapeLineFormatImpl border, bool isHead)
	{
		if (reader.HasAttributes)
		{
			if (reader.MoveToAttribute("len"))
			{
				if (isHead)
				{
					border.BeginArrowheadLength = GetHeadLength(reader.Value);
				}
				else
				{
					border.EndArrowheadLength = GetHeadLength(reader.Value);
				}
			}
			if (reader.MoveToAttribute("type"))
			{
				if (isHead)
				{
					border.BeginArrowHeadStyle = GetHeadStyle(reader.Value);
				}
				else
				{
					border.EndArrowHeadStyle = GetHeadStyle(reader.Value);
				}
			}
			if (reader.MoveToAttribute("w") && isHead)
			{
				border.BeginArrowheadWidth = GetHeadWidth(reader.Value);
			}
		}
		reader.Skip();
	}

	private static OfficeShapeArrowWidth GetHeadWidth(string value)
	{
		return value switch
		{
			"lg" => OfficeShapeArrowWidth.ArrowHeadWide, 
			"med" => OfficeShapeArrowWidth.ArrowHeadMedium, 
			"sm" => OfficeShapeArrowWidth.ArrowHeadNarrow, 
			_ => OfficeShapeArrowWidth.ArrowHeadMedium, 
		};
	}

	private static OfficeShapeArrowStyle GetHeadStyle(string value)
	{
		return value switch
		{
			"arrow" => OfficeShapeArrowStyle.LineArrowOpen, 
			"diamond" => OfficeShapeArrowStyle.LineArrowDiamond, 
			"none" => OfficeShapeArrowStyle.LineNoArrow, 
			"oval" => OfficeShapeArrowStyle.LineArrowOval, 
			"stealth" => OfficeShapeArrowStyle.LineArrowStealth, 
			"triangle" => OfficeShapeArrowStyle.LineArrow, 
			_ => OfficeShapeArrowStyle.LineNoArrow, 
		};
	}

	private static OfficeShapeArrowLength GetHeadLength(string value)
	{
		return value switch
		{
			"lg" => OfficeShapeArrowLength.ArrowHeadLong, 
			"med" => OfficeShapeArrowLength.ArrowHeadMedium, 
			"sm" => OfficeShapeArrowLength.ArrowHeadShort, 
			_ => OfficeShapeArrowLength.ArrowHeadMedium, 
		};
	}
}
