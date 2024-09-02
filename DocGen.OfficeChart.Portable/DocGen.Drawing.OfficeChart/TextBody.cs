using System.IO;
using System.Xml;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.XmlReaders.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

namespace DocGen.Drawing.OfficeChart;

internal class TextBody
{
	private string attribute;

	private ShapeImplExt shape;

	public TextBody(ShapeImplExt shape, string attribute)
	{
		this.shape = shape;
		this.attribute = attribute;
	}

	private void TextBodyProperties(XmlWriter xmlTextWriter)
	{
		TextFrame textFrame = shape.TextFrame;
		xmlTextWriter.WriteStartElement("a", "bodyPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		if (textFrame.TextVertOverflowType != 0)
		{
			xmlTextWriter.WriteAttributeString("vertOverflow", Helper.GetVerticalFlowType(textFrame.TextVertOverflowType));
		}
		if (textFrame.TextHorzOverflowType != 0)
		{
			xmlTextWriter.WriteAttributeString("horzOverflow", Helper.GetHorizontalFlowType(textFrame.TextHorzOverflowType));
		}
		string value = "square";
		if (!textFrame.WrapTextInShape)
		{
			value = "none";
		}
		xmlTextWriter.WriteAttributeString("wrap", value);
		if (!textFrame.IsAutoMargins)
		{
			xmlTextWriter.WriteAttributeString("lIns", Helper.ToString(textFrame.GetLeftMargin()));
			xmlTextWriter.WriteAttributeString("tIns", Helper.ToString(textFrame.GetTopMargin()));
			xmlTextWriter.WriteAttributeString("rIns", Helper.ToString(textFrame.GetRightMargin()));
			xmlTextWriter.WriteAttributeString("bIns", Helper.ToString(textFrame.GetBottomMargin()));
		}
		string anchor = "t";
		bool anchorPosition = textFrame.GetAnchorPosition(out anchor);
		if (textFrame.TextDirection != 0)
		{
			string textDirection = textFrame.GetTextDirection(textFrame.TextDirection);
			if (textDirection != null)
			{
				xmlTextWriter.WriteAttributeString("vert", textDirection);
			}
		}
		xmlTextWriter.WriteAttributeString("anchor", anchor);
		if (anchorPosition)
		{
			xmlTextWriter.WriteAttributeString("anchorCtr", "1");
		}
		else
		{
			xmlTextWriter.WriteAttributeString("anchorCtr", "0");
		}
		if (textFrame.IsAutoSize)
		{
			xmlTextWriter.WriteElementString("a:spAutoFit", null);
		}
		if (textFrame.Columns.Number > 0)
		{
			xmlTextWriter.WriteAttributeString("numCol", Helper.ToString(textFrame.Columns.Number));
		}
		int num = (int)((double)textFrame.Columns.SpacingPt * 12700.0 + 0.5);
		if (num > 0)
		{
			xmlTextWriter.WriteAttributeString("spcCol", Helper.ToString(num));
		}
		xmlTextWriter.WriteEndElement();
	}

	private void TextParagraph(XmlWriter xmlTextWriter_0)
	{
		xmlTextWriter_0.WriteStartElement("a", "p", null);
		xmlTextWriter_0.WriteStartElement("a", "r", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter_0.WriteStartElement("a", "rPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter_0.WriteAttributeString("lang", "en-US");
		xmlTextWriter_0.WriteAttributeString("sz", "1100");
		xmlTextWriter_0.WriteEndElement();
		xmlTextWriter_0.WriteStartElement("a", "t", "http://schemas.openxmlformats.org/drawingml/2006/main");
		string text = shape.TextFrame.TextRange.Text;
		xmlTextWriter_0.WriteString(text);
		xmlTextWriter_0.WriteEndElement();
		xmlTextWriter_0.WriteEndElement();
		xmlTextWriter_0.WriteEndElement();
	}

	internal void Write(XmlWriter xmlTextWriter)
	{
		Stream value;
		if (shape.Logger.GetPreservedItem(PreservedFlag.RichText))
		{
			xmlTextWriter.WriteStartElement(attribute, "txBody", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
			RichTextString textArea = (RichTextString)shape.TextFrame.TextRange.RichText;
			TextBodyProperties(xmlTextWriter);
			TextBoxSerializator.SerializeParagraphsAutoShapes(xmlTextWriter, textArea, shape.Worksheet.ParentWorkbook);
			xmlTextWriter.WriteEndElement();
		}
		else if (shape.PreservedElements.TryGetValue("TextBody", out value) && value != null && value.Length > 0)
		{
			value.Position = 0L;
			ShapeParser.WriteNodeFromStream(xmlTextWriter, value);
		}
	}
}
