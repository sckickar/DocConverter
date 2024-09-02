using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

internal class TextBoxSerializator : DrawingShapeSerializator
{
	public override void Serialize(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, RelationCollection vmlRelations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (!(shape is TextBoxShapeImpl textBoxShapeImpl))
		{
			throw new ArgumentOutOfRangeException("textBox");
		}
		FileDataHolder parentHolder = holder.ParentHolder;
		string text = ((shape.ParentShapes.Worksheet != null) ? "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing" : "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing");
		WorksheetImpl worksheetImpl = shape.Worksheet as WorksheetImpl;
		if (shape.EnableAlternateContent)
		{
			writer.WriteStartElement("mc", "AlternateContent", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteStartElement("mc", "Choice", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			writer.WriteAttributeString("xmlns", "a14", null, "http://schemas.microsoft.com/office/drawing/2010/main");
			writer.WriteAttributeString("Requires", "a14");
		}
		if (worksheetImpl != null)
		{
			writer.WriteStartElement("twoCellAnchor", text);
			writer.WriteAttributeString("editAs", DrawingShapeSerializator.GetEditAsValue(shape));
		}
		else
		{
			writer.WriteStartElement("relSizeAnchor", text);
		}
		SerializeAnchorPoint(writer, "from", shape.LeftColumn, shape.LeftColumnOffset, shape.TopRow, shape.TopRowOffset, worksheetImpl, text);
		SerializeAnchorPoint(writer, "to", shape.RightColumn, shape.RightColumnOffset, shape.BottomRow, shape.BottomRowOffset, worksheetImpl, text);
		if (shape.IsEquationShape)
		{
			Stream xmlDataStream = shape.XmlDataStream;
			xmlDataStream.Position = 0L;
			XmlReader reader = UtilityMethods.CreateReader(xmlDataStream);
			writer.WriteNode(reader, defattr: false);
		}
		else
		{
			writer.WriteStartElement("sp", text);
			Excel2007Serializator.SerializeAttribute(writer, "fLocksText", textBoxShapeImpl.IsTextLocked, defaultValue: true);
			Excel2007Serializator.SerializeAttribute(writer, "textlink", textBoxShapeImpl.TextLink, null);
			SerializeNonVisualProperties(writer, textBoxShapeImpl, holder, text);
			SerializeShapeProperites(writer, textBoxShapeImpl, parentHolder, holder.Relations, text);
			SerializeRichText(writer, text, textBoxShapeImpl);
			writer.WriteEndElement();
		}
		if (worksheetImpl != null)
		{
			writer.WriteElementString("clientData", text, string.Empty);
		}
		writer.WriteEndElement();
		if (shape.EnableAlternateContent)
		{
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	[SecurityCritical]
	private void SerializeShapeProperites(XmlWriter writer, TextBoxShapeImpl textBox, FileDataHolder holder, RelationCollection relations, string drawingsNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		writer.WriteStartElement("spPr", drawingsNamespace);
		Rectangle coordinates = textBox.Coordinates2007;
		DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/main", "http://schemas.openxmlformats.org/drawingml/2006/main", coordinates.X, coordinates.Y, coordinates.Width, coordinates.Height, textBox);
		SerializePresetGeometry(writer);
		SerializeFill(writer, textBox, holder, relations);
		writer.WriteEndElement();
	}

	private void SerializeNonVisualProperties(XmlWriter writer, TextBoxShapeImpl textBox, WorksheetDataHolder holder, string drawingsNamespace)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		writer.WriteStartElement("nvSpPr", drawingsNamespace);
		SerializeNVCanvasProperties(writer, textBox, holder, drawingsNamespace);
		writer.WriteStartElement("cNvSpPr", drawingsNamespace);
		writer.WriteAttributeString("txBox", "1");
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public static void SerializeRichText(XmlWriter writer, string drawingsNamespace, TextBoxShapeBase textBox)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		RichTextString textArea = (RichTextString)textBox.RichText;
		writer.WriteStartElement("txBody", drawingsNamespace);
		SerializeBodyProperties(writer, textArea, textBox);
		SerializeListStyles(writer, textArea);
		SerializeParagraphs(writer, textArea, textBox);
		writer.WriteEndElement();
	}

	private static void SerializeBodyProperties(XmlWriter writer, RichTextString textArea, TextBoxShapeBase textBox)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		writer.WriteStartElement("bodyPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		Dictionary<string, string> unknownBodyProperties = textBox.UnknownBodyProperties;
		if (unknownBodyProperties != null && unknownBodyProperties.Count > 0)
		{
			foreach (KeyValuePair<string, string> item in unknownBodyProperties)
			{
				writer.WriteAttributeString(item.Key, item.Value);
			}
		}
		SerializeTextRotation(writer, textBox);
		SerializeAnchor(writer, textBox);
		writer.WriteEndElement();
	}

	private static void SerializeAnchor(XmlWriter writer, TextBoxShapeBase textBox)
	{
		if (textBox.VAlignment != OfficeCommentVAlign.Top)
		{
			writer.WriteAttributeString("anchor", ((Excel2007CommentVAlign)textBox.VAlignment).ToString());
		}
	}

	private static void SerializeTextRotation(XmlWriter writer, TextBoxShapeBase textBox)
	{
		if (textBox.TextRotation != 0)
		{
			writer.WriteAttributeString("vert", ((Excel2007TextRotation)textBox.TextRotation).ToString());
		}
	}

	private static void SerializeListStyles(XmlWriter writer, RichTextString textArea)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		writer.WriteStartElement("lstStyle", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteEndElement();
	}

	private static void SerializeParagraphs(XmlWriter writer, RichTextString textArea, TextBoxShapeBase textBox)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
		TextWithFormat textWithFormat = textArea.TextObject;
		int num = 0;
		WorkbookImpl workbookImpl = (WorkbookImpl)textBox.Workbook;
		FontsCollection innerFonts = workbookImpl.InnerFonts;
		string text = textWithFormat.Text;
		if (text == null || text.Length <= 0 || text == "\n")
		{
			SerializeTextFeildElement(writer, textBox);
		}
		if (textWithFormat.FormattingRunsCount > 0 && textWithFormat.GetPositionByIndex(0) != 0)
		{
			textWithFormat = textWithFormat.TypedClone();
			int defaultFontIndex = textArea.DefaultFontIndex;
			textWithFormat.FormattingRuns[0] = ((defaultFontIndex >= 0) ? defaultFontIndex : 0);
		}
		int formattingRunsCount = textWithFormat.FormattingRunsCount;
		IOfficeFont officeFont;
		for (int i = 0; i < formattingRunsCount; i++)
		{
			int fontByIndex = textWithFormat.GetFontByIndex(i);
			int num2 = ((i != formattingRunsCount - 1) ? textWithFormat.GetPositionByIndex(i + 1) : text.Length) - 1;
			officeFont = innerFonts[fontByIndex];
			string text2 = text.Substring(num, num2 - num + 1);
			string[] separator = new string[1] { "\n" };
			string[] array = text2.Split(separator, StringSplitOptions.None);
			SerializeFormattingRunProperty(writer, workbookImpl, officeFont);
			int j = 0;
			for (int num3 = array.Length; j < num3; j++)
			{
				SerializeFormattingRun(writer, officeFont, "rPr", workbookImpl, array[j], textBox);
				if (j != num3 - 1)
				{
					SerializeParagraphRunProperites(writer, officeFont, "endParaRPr", workbookImpl, isTextLink: false);
					writer.WriteEndElement();
					writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
				}
			}
			num = num2 + 1;
		}
		int num4 = textArea.DefaultFontIndex;
		if (num4 < 0)
		{
			num4 = 0;
		}
		officeFont = innerFonts[num4];
		if (formattingRunsCount == 0 && text != null && text.Length > 0)
		{
			SerializeFormattingRun(writer, officeFont, "rPr", workbookImpl, text, textBox);
		}
		SerializeParagraphRunProperites(writer, officeFont, "endParaRPr", workbookImpl, isTextLink: false);
		writer.WriteEndElement();
	}

	private static void SerializeTextFeildElement(XmlWriter writer, TextBoxShapeBase textBox)
	{
		if (!(textBox is TextBoxShapeImpl { TextLink: { Length: >0 } textLink } textBoxShapeImpl))
		{
			return;
		}
		string name = textLink.Substring(1, textLink.Length - 1);
		IWorkbook workbook = textBoxShapeImpl.Workbook;
		if (textBoxShapeImpl.Worksheet is IWorksheet worksheet)
		{
			IRange range = worksheet.Range[name];
			IOfficeFont font = range.CellStyle.Font;
			string text = textBoxShapeImpl.FieldId;
			string text2 = textBoxShapeImpl.FieldType;
			if (text == null || text.Length < 0)
			{
				text = $"{{{Guid.NewGuid().ToString().ToUpper()}}}";
			}
			if (text2 == null || text2.Length < 0)
			{
				text2 = "TxLink";
			}
			writer.WriteStartElement("fld", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("id", text);
			writer.WriteAttributeString("type", text2);
			SerializeParagraphRunProperites(writer, font, "rPr", workbook, isTextLink: true);
			writer.WriteStartElement("t", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteString(range.DisplayText);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	private static void SerializeFormattingRunProperty(XmlWriter writer, IWorkbook book, TextBoxShapeBase textBox)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		writer.WriteStartElement("pPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		Excel2007Serializator.SerializeAttribute(writer, "algn", ((Excel2007CommentHAlign)textBox.HAlignment).ToString(), Excel2007CommentHAlign.l.ToString());
		writer.WriteEndElement();
	}

	private static void SerializeFormattingRunProperty(XmlWriter writer, IWorkbook book, IOfficeFont font)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		writer.WriteStartElement("pPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		Excel2007Serializator.SerializeAttribute(writer, "algn", (font as FontImpl).ParaAlign.ToString(), Excel2007CommentHAlign.l.ToString());
		writer.WriteEndElement();
	}

	private static void SerializeFormattingRun(XmlWriter writer, IOfficeFont font, string tagName, IWorkbook book, string text, TextBoxShapeBase textBox)
	{
		if (text.Length > 0)
		{
			writer.WriteStartElement("r", "http://schemas.openxmlformats.org/drawingml/2006/main");
			SerializeParagraphRunProperites(writer, font, tagName, book, isTextLink: false);
			writer.WriteStartElement("t", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteString(text);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}

	public static void SerializeParagraphRunProperites(XmlWriter writer, IOfficeFont textArea, string mainTagName, IWorkbook book, bool isTextLink)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		if (mainTagName == null || mainTagName.Length == 0)
		{
			throw new ArgumentException("mainTagName");
		}
		writer.WriteStartElement(mainTagName, "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("lang", CultureInfo.CurrentCulture.Name);
		string value = (textArea.Bold ? "1" : "0");
		string value2 = (textArea.Italic ? "1" : "0");
		writer.WriteAttributeString("b", value);
		writer.WriteAttributeString("i", value2);
		if (textArea.Strikethrough)
		{
			writer.WriteAttributeString("strike", "sngStrike");
		}
		writer.WriteAttributeString("sz", ((int)(textArea.Size * 100.0)).ToString());
		if (textArea.Underline != 0)
		{
			string value3 = ((textArea.Underline == OfficeUnderline.Single) ? "sng" : "dbl");
			writer.WriteAttributeString("u", value3);
		}
		int num = 0;
		if (textArea.Subscript)
		{
			num = -25000;
		}
		if (textArea.Superscript)
		{
			num = 30000;
		}
		if (num != 0)
		{
			writer.WriteAttributeString("baseline", num.ToString());
		}
		writer.WriteStartElement("solidFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		ChartSerializatorCommon.SerializeRgbColor(writer, textArea.Color, book);
		writer.WriteEndElement();
		if (isTextLink)
		{
			writer.WriteStartElement("latin", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("typeface", textArea.FontName);
			writer.WriteEndElement();
		}
		else if ((textArea as FontImpl).showFontName)
		{
			writer.WriteStartElement("latin", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("typeface", textArea.FontName);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	internal static void SerializeParagraphsAutoShapes(XmlWriter writer, RichTextString textArea, WorkbookImpl book)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
		TextWithFormat textWithFormat = textArea.TextObject;
		int num = 0;
		FontsCollection innerFonts = book.InnerFonts;
		if (textWithFormat.FormattingRunsCount > 0 && textWithFormat.GetPositionByIndex(0) != 0)
		{
			textWithFormat = textWithFormat.TypedClone();
			int defaultFontIndex = textArea.DefaultFontIndex;
			textWithFormat.FormattingRuns[0] = ((defaultFontIndex >= 0) ? defaultFontIndex : 0);
		}
		string text = textWithFormat.Text;
		int formattingRunsCount = textWithFormat.FormattingRunsCount;
		IOfficeFont officeFont;
		for (int i = 0; i < formattingRunsCount; i++)
		{
			int fontByIndex = textWithFormat.GetFontByIndex(i);
			int num2 = ((i != formattingRunsCount - 1) ? textWithFormat.GetPositionByIndex(i + 1) : text.Length) - 1;
			officeFont = innerFonts[fontByIndex];
			string[] array = text.Substring(num, num2 - num + 1).Split('\n');
			int j = 0;
			for (int num3 = array.Length; j < num3; j++)
			{
				SerializeFormattingRun(writer, officeFont, "rPr", book, array[j], null);
				if (j != num3 - 1)
				{
					SerializeParagraphRunProperites(writer, officeFont, "endParaRPr", book, isTextLink: false);
					writer.WriteEndElement();
					writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
				}
			}
			num = num2 + 1;
		}
		int num4 = textArea.DefaultFontIndex;
		if (num4 < 0)
		{
			num4 = 0;
		}
		officeFont = innerFonts[num4];
		if (formattingRunsCount == 0 && text != null && text.Length > 0)
		{
			SerializeFormattingRun(writer, officeFont, "rPr", book, text, null);
		}
		SerializeParagraphRunProperites(writer, officeFont, "endParaRPr", book, isTextLink: false);
		writer.WriteEndElement();
	}
}
