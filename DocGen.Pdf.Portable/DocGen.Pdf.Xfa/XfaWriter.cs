using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

internal class XfaWriter
{
	private PdfXfaForm m_xfaForm;

	private XmlWriter m_writer;

	internal int m_fieldCount = 1;

	internal int m_subFormFieldCount = 1;

	private PdfXfaDocument xfaDocument;

	internal XmlWriter Write
	{
		get
		{
			return m_writer;
		}
		set
		{
			m_writer = value;
		}
	}

	internal PdfStream WriteDocumentTemplate(PdfXfaForm XfaForm)
	{
		m_xfaForm = XfaForm;
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		PdfStream pdfStream = new PdfStream();
		xmlWriterSettings.OmitXmlDeclaration = true;
		xmlWriterSettings.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
		Write = XmlWriter.Create(pdfStream.InternalStream, xmlWriterSettings);
		Write.WriteStartElement("template", "http://www.xfa.org/schema/xfa-template/3.3/");
		m_xfaForm.SaveMainForm(this);
		Write.WriteEndElement();
		Write.Close();
		return pdfStream;
	}

	internal PdfStream WritePreamble()
	{
		PdfStream pdfStream = new PdfStream();
		pdfStream.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?><xdp:xdp xmlns:xdp=\"http://ns.adobe.com/xdp/\">");
		return pdfStream;
	}

	internal void StartDataSets(XmlWriter dataWriter)
	{
		dataWriter.WriteStartElement("xfa", "datasets", "http://www.xfa.org/schema/xfa-data/1.0/");
		dataWriter.WriteStartElement("xfa", "data", null);
	}

	internal void EndDataSets(XmlWriter dataWriter)
	{
		dataWriter.WriteEndElement();
		dataWriter.WriteEndElement();
	}

	internal PdfStream WritePostable()
	{
		PdfStream pdfStream = new PdfStream();
		pdfStream.Write("</xdp:xdp>");
		return pdfStream;
	}

	internal PdfStream WriteConfig()
	{
		PdfStream pdfStream = new PdfStream();
		MemoryStream memoryStream = new MemoryStream();
		Write = XmlWriter.Create(memoryStream, new XmlWriterSettings
		{
			Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
			OmitXmlDeclaration = true
		});
		Write.WriteStartElement("config", "http://www.xfa.org/schema/xci/1.0/");
		Write.WriteStartElement("present");
		Write.WriteStartElement("pdf");
		Write.WriteStartElement("fontInfo");
		Write.WriteStartElement("embed");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteStartElement("version");
		Write.WriteString("1.65");
		Write.WriteEndElement();
		Write.WriteStartElement("creator");
		Write.WriteString("DocGen");
		Write.WriteEndElement();
		Write.WriteStartElement("producer");
		Write.WriteString("DocGen");
		Write.WriteEndElement();
		Write.WriteStartElement("scriptModel");
		Write.WriteString("XFA");
		Write.WriteEndElement();
		Write.WriteStartElement("interactive");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("tagged");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("encryption");
		Write.WriteStartElement("permissions");
		Write.WriteStartElement("accessibleContent");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("contentCopy");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("documentAssembly");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("formFieldFilling");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("modifyAnnots");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("print");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("printHighQuality");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("change");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteStartElement("plaintextMetadata");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteStartElement("compression");
		Write.WriteStartElement("level");
		Write.WriteString("6");
		Write.WriteEndElement();
		Write.WriteStartElement("compressLogicalStructure");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteStartElement("linearized");
		Write.WriteString("1");
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteStartElement("acrobat");
		Write.WriteStartElement("acrobat7");
		Write.WriteStartElement("dynamicRender");
		Write.WriteString("required");
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.Close();
		pdfStream.Write(memoryStream.GetBuffer());
		memoryStream.Dispose();
		return pdfStream;
	}

	internal void WriteUI(string name, Dictionary<string, string> values, PdfXfaBorder border)
	{
		WriteUI(name, values, border, 0);
	}

	internal void WriteUI(string name, Dictionary<string, string> values, PdfXfaBorder border, PdfPaddings padding)
	{
		WriteUI(name, values, border, 0, padding);
	}

	internal void WriteUI(string name, Dictionary<string, string> values, PdfXfaBorder border, int comb)
	{
		WriteUI(name, values, border, comb, null);
	}

	internal void WriteUI(string name, Dictionary<string, string> values, PdfXfaBorder border, int comb, PdfPaddings padding)
	{
		Write.WriteStartElement("ui");
		Write.WriteStartElement(name);
		if (values != null)
		{
			foreach (KeyValuePair<string, string> value in values)
			{
				if (value.Value != null && value.Value != "\0")
				{
					Write.WriteAttributeString(value.Key, value.Value);
				}
			}
		}
		if (comb > 0)
		{
			Write.WriteAttributeString("hScrollPolicy", "off");
			Write.WriteStartElement("comb");
			Write.WriteAttributeString("numberOfCells", comb.ToString());
			Write.WriteEndElement();
		}
		DrawBorder(border);
		if (padding != null)
		{
			WriteMargins(padding.Left, padding.Right, padding.Bottom, padding.Top);
		}
		Write.WriteEndElement();
		Write.WriteEndElement();
	}

	internal void WriteValue(string text, int maxChar)
	{
		Write.WriteStartElement("value");
		Write.WriteStartElement("text");
		if (maxChar > 0)
		{
			Write.WriteAttributeString("maxChars", maxChar.ToString());
		}
		Write.WriteString(text);
		Write.WriteEndElement();
		Write.WriteEndElement();
	}

	internal void WriteValue(string text, string value, int maxChar)
	{
		Write.WriteStartElement("value");
		Write.WriteStartElement(value);
		if (maxChar > 0)
		{
			Write.WriteAttributeString("maxChars", maxChar.ToString());
		}
		Write.WriteString(text);
		Write.WriteEndElement();
		Write.WriteEndElement();
	}

	internal void WriteMargins(float l, float r, float b, float t)
	{
		Write.WriteStartElement("margin");
		if (b != 0f)
		{
			Write.WriteAttributeString("bottomInset", b + "pt");
		}
		if (t != 0f)
		{
			Write.WriteAttributeString("topInset", t + "pt");
		}
		if (l != 0f)
		{
			Write.WriteAttributeString("leftInset", l + "pt");
		}
		if (r != 0f)
		{
			Write.WriteAttributeString("rightInset", r + "pt");
		}
		Write.WriteEndElement();
	}

	internal void WriteMargins(PdfMargins margins)
	{
		WriteMargins(margins.Left, margins.Right, margins.Bottom, margins.Top);
	}

	internal void WriteFontInfo(PdfFont font, PdfColor foreColor)
	{
		if (font != null)
		{
			WriteFontInfo(font.Name.ToString(), font.Size, font.Style, foreColor);
		}
		else
		{
			WriteFontInfo("Times New Roman", 0f, PdfFontStyle.Regular, foreColor);
		}
	}

	private void WriteFontInfo(string name, float size, PdfFontStyle style, PdfColor fillColor)
	{
		Write.WriteStartElement("font");
		Write.WriteAttributeString("typeface", name);
		if (size > 0f)
		{
			Write.WriteAttributeString("size", size + "pt");
		}
		switch (style)
		{
		case PdfFontStyle.Bold:
			Write.WriteAttributeString("weight", style.ToString().ToLower());
			break;
		case PdfFontStyle.Italic:
			Write.WriteAttributeString("posture", style.ToString().ToLower());
			break;
		case PdfFontStyle.Strikeout:
			Write.WriteAttributeString("lineThrough", "1");
			break;
		case PdfFontStyle.Underline:
			Write.WriteAttributeString("underline", "1");
			break;
		}
		DrawFillColor(fillColor);
		Write.WriteEndElement();
	}

	internal void SetSize(float fixedHeight, float fixedWidth, float minHeight, float minWidth)
	{
		SetSize(fixedHeight, fixedWidth, minHeight, minWidth, 0f, 0f);
	}

	internal void SetSize(float fixedHeight, float fixedWidth, float minHeight, float minWidth, float maxHeight, float maxWidth)
	{
		if (fixedHeight != 0f)
		{
			Write.WriteAttributeString("h", fixedHeight + "pt");
		}
		if (fixedWidth != 0f)
		{
			Write.WriteAttributeString("w", fixedWidth + "pt");
		}
		if (minHeight != 0f)
		{
			Write.WriteAttributeString("minH", minHeight + "pt");
		}
		if (minWidth != 0f)
		{
			Write.WriteAttributeString("minW", minWidth + "pt");
		}
		if (maxHeight != 0f)
		{
			Write.WriteAttributeString("maxH", maxHeight + "pt");
		}
		if (maxWidth != 0f)
		{
			Write.WriteAttributeString("maxW", maxWidth + "pt");
		}
	}

	internal void DrawLine(float thickness, string slope, string color)
	{
		Write.WriteStartElement("value");
		Write.WriteStartElement("line");
		if (slope != "")
		{
			Write.WriteAttributeString("slope", slope);
		}
		Write.WriteStartElement("edge");
		Write.WriteAttributeString("thickness", thickness + "pt");
		Write.WriteStartElement("color");
		Write.WriteAttributeString("value", color);
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteEndElement();
		Write.WriteEndElement();
	}

	internal void WriteCaption(string text, float reserve, PdfXfaHorizontalAlignment hAlign, PdfXfaVerticalAlignment vAligh)
	{
		Write.WriteStartElement("caption");
		if (reserve > 0f)
		{
			Write.WriteAttributeString("reserve", reserve + "pt");
		}
		WriteValue(text, 0);
		WritePragraph(vAligh, hAlign);
		Write.WriteEndElement();
	}

	internal void WriteItems(string text)
	{
		Write.WriteStartElement("items");
		Write.WriteStartElement("integer");
		Write.WriteString(text);
		Write.WriteEndElement();
		Write.WriteEndElement();
	}

	internal void WriteItems(string rollOver, string down)
	{
		Write.WriteStartElement("items");
		if (rollOver != null && rollOver != "")
		{
			Write.WriteStartElement("text");
			Write.WriteAttributeString("name", "rollover");
			Write.WriteString(rollOver);
			Write.WriteEndElement();
		}
		if (down != null && down != "")
		{
			Write.WriteStartElement("text");
			Write.WriteAttributeString("name", "down");
			Write.WriteString(down);
			Write.WriteEndElement();
		}
		Write.WriteEndElement();
	}

	internal void WriteListItems(List<string> list, string saveString)
	{
		Write.WriteStartElement("items");
		if (saveString != null)
		{
			Write.WriteAttributeString("save", saveString);
		}
		if (list.Count > 0)
		{
			foreach (string item in list)
			{
				Write.WriteStartElement("text");
				Write.WriteString(item);
				Write.WriteEndElement();
			}
		}
		Write.WriteEndElement();
	}

	internal void WriteToolTip(string text)
	{
		Write.WriteStartElement("assist");
		Write.WriteStartElement("toolTip");
		Write.WriteString(text);
		Write.WriteEndElement();
		Write.WriteEndElement();
	}

	internal void DrawFillColor(PdfColor fillColor)
	{
		if (fillColor.R != 0 || fillColor.G != 0 || fillColor.B != 0)
		{
			Write.WriteStartElement("fill");
			DrawColor(fillColor);
			Write.WriteEndElement();
		}
	}

	internal void WriteLocation(PointF location)
	{
		Write.WriteAttributeString("x", location.X + "pt");
		Write.WriteAttributeString("y", location.Y + "pt");
	}

	internal void WritePragraph(PdfXfaVerticalAlignment vAlign, PdfXfaHorizontalAlignment hAlign)
	{
		Write.WriteStartElement("para");
		string value = hAlign.ToString().ToLower();
		if (hAlign == PdfXfaHorizontalAlignment.JustifyAll)
		{
			value = "justifyAll";
		}
		Write.WriteAttributeString("hAlign", value);
		Write.WriteAttributeString("vAlign", vAlign.ToString().ToLower());
		Write.WriteEndElement();
	}

	internal void WritePattern(string value, bool isvalidate)
	{
		if (value != null)
		{
			if (isvalidate)
			{
				Write.WriteStartElement("validate");
				Write.WriteStartElement("picture");
				Write.WriteString(value);
				Write.WriteEndElement();
				Write.WriteEndElement();
			}
			Write.WriteStartElement("format");
			Write.WriteStartElement("picture");
			Write.WriteString(value);
			Write.WriteEndElement();
			Write.WriteEndElement();
		}
	}

	internal void DrawBorder(PdfXfaBorder border, bool isSkip)
	{
		if (border != null)
		{
			if (border.LeftEdge != null || border.RightEdge != null || border.TopEdge != null || border.BottomEdge != null)
			{
				DrawEdge(border.TopEdge);
				DrawEdge(border.RightEdge);
				DrawEdge(border.BottomEdge);
				DrawEdge(border.LeftEdge);
			}
			else
			{
				PdfXfaEdge pdfXfaEdge = new PdfXfaEdge();
				pdfXfaEdge.BorderStyle = border.Style;
				pdfXfaEdge.Color = border.Color;
				pdfXfaEdge.Thickness = border.Width;
				pdfXfaEdge.Visibility = border.Visibility;
				DrawEdge(pdfXfaEdge);
			}
		}
	}

	internal void DrawBorder(PdfXfaBorder border)
	{
		if (border != null)
		{
			Write.WriteStartElement("border");
			_ = border.Handedness;
			Write.WriteAttributeString("hand", border.Handedness.ToString().ToLower());
			if (border.Visibility != 0)
			{
				Write.WriteAttributeString("presence", border.Visibility.ToString().ToLower());
			}
			DrawBorder(border, isSkip: true);
			if (border.FillColor != null)
			{
				DrawFillColor(border.FillColor);
			}
			Write.WriteEndElement();
		}
	}

	internal void DrawBorder(PdfXfaBorder border, PdfXfaBrush fillColor)
	{
		if (border != null)
		{
			Write.WriteStartElement("border");
			_ = border.Handedness;
			Write.WriteAttributeString("hand", border.Handedness.ToString().ToLower());
			if (border.Visibility != 0)
			{
				Write.WriteAttributeString("presence", border.Visibility.ToString().ToLower());
			}
			DrawBorder(border, isSkip: true);
			DrawFillColor(fillColor);
			Write.WriteEndElement();
		}
	}

	internal void DrawEdge(PdfXfaEdge edge)
	{
		if (edge != null)
		{
			Write.WriteStartElement("edge");
			DrawStroke(edge.BorderStyle);
			if (edge.Thickness >= 0f)
			{
				Write.WriteAttributeString("thickness", edge.Thickness + "pt");
			}
			if (edge.Visibility != 0)
			{
				Write.WriteAttributeString("presence", edge.Visibility.ToString().ToLower());
			}
			if (edge.Color.R != 0 || edge.Color.G != 0 || edge.Color.B != 0)
			{
				DrawColor(edge.Color);
			}
			Write.WriteEndElement();
		}
		else
		{
			Write.WriteStartElement("edge");
			Write.WriteEndElement();
		}
	}

	internal void DrawCorner(PdfXfaCorner corner)
	{
		if (corner != null)
		{
			Write.WriteStartElement("corner");
			DrawStroke(corner.BorderStyle);
			if (corner.Thickness > 0f)
			{
				Write.WriteAttributeString("thickness", corner.Thickness + "pt");
			}
			if (corner.Radius > 0f)
			{
				Write.WriteAttributeString("radius", corner.Radius + "pt");
				Write.WriteAttributeString("join", corner.Shape.ToString().ToLower());
			}
			if (corner.Visibility != 0)
			{
				Write.WriteAttributeString("presence", corner.Visibility.ToString().ToLower());
			}
			if (corner.BorderColor.R != 0 || corner.BorderColor.G != 0 || corner.BorderColor.B != 0)
			{
				DrawColor(corner.BorderColor);
			}
			Write.WriteEndElement();
		}
	}

	internal void DrawColor(PdfColor color)
	{
		Write.WriteStartElement("color");
		Write.WriteAttributeString("value", color.R + "," + color.G + "," + color.B);
		Write.WriteEndElement();
	}

	private void DrawStroke(PdfXfaBorderStyle style)
	{
		if (style.ToString().ToLower() != "none")
		{
			string text = style.ToString();
			text = char.ToLower(text[0]) + text.Substring(1);
			Write.WriteAttributeString("stroke", text);
		}
	}

	internal void DrawFillColor(PdfXfaBrush fill)
	{
		if (fill == null)
		{
			return;
		}
		Write.WriteStartElement("fill");
		if (fill != null)
		{
			if (fill is PdfXfaSolidBrush)
			{
				PdfXfaSolidBrush pdfXfaSolidBrush = fill as PdfXfaSolidBrush;
				if (pdfXfaSolidBrush.Color.R != 0 || pdfXfaSolidBrush.Color.G != 0 || pdfXfaSolidBrush.Color.B != 0)
				{
					DrawColor(pdfXfaSolidBrush.Color);
				}
			}
			else if (fill is PdfXfaLinearBrush)
			{
				DrawLinearBrush(fill as PdfXfaLinearBrush);
			}
			else if (fill is PdfXfaRadialBrush)
			{
				DrawRadialBrush(fill as PdfXfaRadialBrush);
			}
		}
		Write.WriteEndElement();
	}

	internal void DrawRadialBrush(PdfXfaRadialBrush rBrush)
	{
		if (rBrush != null)
		{
			_ = rBrush.StartColor;
			DrawColor(rBrush.StartColor);
			Write.WriteStartElement("radial");
			string value = null;
			switch (rBrush.Type)
			{
			case PdfXfaRadialType.EdgeToCenter:
				value = "toCenter";
				break;
			case PdfXfaRadialType.CenterToEdge:
				value = "toEdge";
				break;
			}
			Write.WriteAttributeString("type", value);
			DrawColor(rBrush.EndColor);
			Write.WriteEndElement();
		}
	}

	internal void DrawLinearBrush(PdfXfaLinearBrush lBrush)
	{
		if (lBrush != null)
		{
			_ = lBrush.StartColor;
			DrawColor(lBrush.StartColor);
			Write.WriteStartElement("linear");
			string value = null;
			switch (lBrush.Type)
			{
			case PdfXfaLinearType.TopToBottom:
				value = "toBottom";
				break;
			case PdfXfaLinearType.RightToLeft:
				value = "toLeft";
				break;
			case PdfXfaLinearType.LeftToRight:
				value = "toRight";
				break;
			case PdfXfaLinearType.BottomToTop:
				value = "toTop";
				break;
			}
			Write.WriteAttributeString("type", value);
			DrawColor(lBrush.EndColor);
			Write.WriteEndElement();
		}
	}

	internal void SetRPR(PdfXfaRotateAngle rotation, PdfXfaVisibility presence, bool isReadOnly)
	{
		if (rotation != 0)
		{
			string value = "0";
			switch (rotation)
			{
			case PdfXfaRotateAngle.RotateAngle180:
				value = "180";
				break;
			case PdfXfaRotateAngle.RotateAngle270:
				value = "270";
				break;
			case PdfXfaRotateAngle.RotateAngle90:
				value = "90";
				break;
			}
			Write.WriteAttributeString("rotate", value);
		}
		if (isReadOnly)
		{
			Write.WriteAttributeString("access", "readOnly");
		}
		if (presence != 0)
		{
			Write.WriteAttributeString("presence", presence.ToString().ToLower());
		}
	}

	internal string GetDatePattern(PdfXfaDatePattern pattern)
	{
		string result = string.Empty;
		switch (pattern)
		{
		case PdfXfaDatePattern.Default:
			result = "MMM d, yyyy";
			break;
		case PdfXfaDatePattern.Short:
			result = "M/d/yyyy";
			break;
		case PdfXfaDatePattern.Medium:
			result = "MMM d, yyyy";
			break;
		case PdfXfaDatePattern.Long:
			result = "MMMM d, yyyy";
			break;
		case PdfXfaDatePattern.Full:
			result = "dddd, MMMM dd, yyyy";
			break;
		case PdfXfaDatePattern.DDMMMMYYYY:
			result = "dd MMMM, yyyy";
			break;
		case PdfXfaDatePattern.DDMMMYY:
			result = "dd-MMM-yy";
			break;
		case PdfXfaDatePattern.EEEEDDMMMMYYYY:
			result = "dddd, dd MMMM, yyyy";
			break;
		case PdfXfaDatePattern.EEEE_MMMMD_YYYY:
			result = "dddd, MMMM d, yyyy";
			break;
		case PdfXfaDatePattern.EEEEMMMMDDYYYY:
			result = "dddd, MMMM dd, yyyy";
			break;
		case PdfXfaDatePattern.MDYY:
			result = "M/d/yy";
			break;
		case PdfXfaDatePattern.MDYYYY:
			result = "M/d/yyyy";
			break;
		case PdfXfaDatePattern.MMDDYY:
			result = "MM/dd/yy";
			break;
		case PdfXfaDatePattern.MMDDYYYY:
			result = "MM/dd/yyyy";
			break;
		case PdfXfaDatePattern.MMMD_YYYY:
			result = "MMM d, yyyy";
			break;
		case PdfXfaDatePattern.MMMMD_YYYY:
			result = "MMMM d, yyyy";
			break;
		case PdfXfaDatePattern.MMMMDDYYYY:
			result = "MMMM dd, yyyy}";
			break;
		case PdfXfaDatePattern.MMMMYYYY:
			result = "MMMM, yyyy";
			break;
		case PdfXfaDatePattern.YYMMDD:
			result = "yy/MM/dd";
			break;
		case PdfXfaDatePattern.YYYYMMDD:
			result = "yyyy-MM-dd";
			break;
		case PdfXfaDatePattern.DDMMYYYY:
			result = "dd/MM/yyyy";
			break;
		}
		return result;
	}

	internal string GetTimePattern(PdfXfaTimePattern pattern)
	{
		string result = string.Empty;
		switch (pattern)
		{
		case PdfXfaTimePattern.Default:
			result = "h:mm:ss tt";
			break;
		case PdfXfaTimePattern.Short:
			result = "t";
			break;
		case PdfXfaTimePattern.Medium:
			result = "h:mm:ss tt";
			break;
		case PdfXfaTimePattern.Long:
			result = "hh:mm:ss tt \"GMT\"zzz";
			result = "T";
			break;
		case PdfXfaTimePattern.Full:
			result = "hh:mm:ss tt \"GMT\" zzz";
			break;
		case PdfXfaTimePattern.H_MM_A:
			result = "h:mm tt";
			break;
		case PdfXfaTimePattern.H_MM_SS:
			result = "H:mm:ss";
			break;
		case PdfXfaTimePattern.H_MM_SS_A:
			result = "h:mm:ss tt";
			break;
		case PdfXfaTimePattern.H_MM_SS_A_Z:
			result = "h:mm:ss tt \"GMT\" zzz";
			break;
		case PdfXfaTimePattern.HH_MM_SS:
			result = "HH:mm:ss";
			break;
		case PdfXfaTimePattern.HH_MM_SS_A:
			result = "hh:mm:ss tt";
			break;
		}
		return result;
	}

	internal string GetDateTimePattern(PdfXfaDatePattern d, PdfXfaTimePattern t)
	{
		return GetDatePattern(d) + " " + GetTimePattern(t);
	}

	internal string GetDatePattern(string pattern)
	{
		string datePattern = GetDatePattern(PdfXfaDatePattern.Default);
		switch (pattern)
		{
		case "date.short{}":
			datePattern = GetDatePattern(PdfXfaDatePattern.Short);
			break;
		case "date.medium{}":
			datePattern = GetDatePattern(PdfXfaDatePattern.Medium);
			break;
		case "date.long{}":
			datePattern = GetDatePattern(PdfXfaDatePattern.Long);
			break;
		case "date.full{}":
			datePattern = GetDatePattern(PdfXfaDatePattern.Full);
			break;
		case "DD MMMM, YYYY":
		case "date{DD MMMM, YYYY}":
			datePattern = GetDatePattern(PdfXfaDatePattern.DDMMMMYYYY);
			break;
		case "date{DD-MMM-YY}":
		case "DD-MMM-YY":
			datePattern = GetDatePattern(PdfXfaDatePattern.DDMMMYY);
			break;
		case "EEEE, DD MMMM, YYYY":
		case "date{EEEE, DD MMMM, YYYY}":
			datePattern = GetDatePattern(PdfXfaDatePattern.EEEEMMMMDDYYYY);
			break;
		case "EEEEE, MMMM D, YYYY":
		case "date{EEEE, MMMM D, YYYY}":
			datePattern = GetDatePattern(PdfXfaDatePattern.EEEE_MMMMD_YYYY);
			break;
		case "EEEE, MMMM DD, YYYY":
		case "date{EEEE, MMMM DD, YYYY}":
			datePattern = GetDatePattern(PdfXfaDatePattern.EEEEMMMMDDYYYY);
			break;
		case "date{M/D/YY}":
		case "M/D/YY":
			datePattern = GetDatePattern(PdfXfaDatePattern.MDYY);
			break;
		case "date{M/D/YYYY}":
		case "M/D/YYYY":
			datePattern = GetDatePattern(PdfXfaDatePattern.MDYYYY);
			break;
		case "date{MM/DD/YY}":
		case "MM/DD/YY":
			datePattern = GetDatePattern(PdfXfaDatePattern.MMDDYY);
			break;
		case "date{MM/DD/YYYY}":
		case "MM/DD/YYYY":
			datePattern = GetDatePattern(PdfXfaDatePattern.MMDDYYYY);
			break;
		case "MMM D, YYYY":
		case "date{MMM D, YYYY}":
			datePattern = GetDatePattern(PdfXfaDatePattern.MMMD_YYYY);
			break;
		case "MMMM D, YYYY":
		case "date{MMMM D, YYYY}":
			datePattern = GetDatePattern(PdfXfaDatePattern.MMMMD_YYYY);
			break;
		case "MMMM DD, YYYY":
		case "date{MMMM DD, YYYY}":
			datePattern = GetDatePattern(PdfXfaDatePattern.MMMMDDYYYY);
			break;
		case "date{MMMM, YYYY}":
		case "MMMM, YYYY":
			datePattern = GetDatePattern(PdfXfaDatePattern.MMMMYYYY);
			break;
		case "date{YY/MM/DD}":
		case "YY/MM/DD":
			datePattern = GetDatePattern(PdfXfaDatePattern.YYMMDD);
			break;
		case "date{YYYY-MM-DD}":
		case "YYYY-MM-DD":
			datePattern = GetDatePattern(PdfXfaDatePattern.YYYYMMDD);
			break;
		case "date{DD/MM/YYYY}":
		case "DD/MM/YYYY":
			datePattern = GetDatePattern(PdfXfaDatePattern.DDMMYYYY);
			break;
		}
		return datePattern;
	}

	internal string GetTimePattern(string pattern)
	{
		string timePattern = GetTimePattern(PdfXfaTimePattern.Default);
		switch (pattern)
		{
		case "time.short{}":
			timePattern = GetTimePattern(PdfXfaTimePattern.Short);
			break;
		case "time.medium{}":
			timePattern = GetTimePattern(PdfXfaTimePattern.Medium);
			break;
		case "time.long{}":
			timePattern = GetTimePattern(PdfXfaTimePattern.Long);
			break;
		case "time.full{}":
			timePattern = GetTimePattern(PdfXfaTimePattern.Full);
			break;
		case "time{h:MM A}":
		case "h:MM A":
			timePattern = GetTimePattern(PdfXfaTimePattern.H_MM_A);
			break;
		case "time{H:MM:SS}":
		case "H:MM:SS":
			timePattern = GetTimePattern(PdfXfaTimePattern.H_MM_SS);
			break;
		case "time{H:MM:SS A}":
		case "H:MM:SS A":
			timePattern = GetTimePattern(PdfXfaTimePattern.H_MM_SS_A);
			break;
		case "H:MM:SS A Z":
		case "time{H:MM:SS A Z}":
			timePattern = GetTimePattern(PdfXfaTimePattern.H_MM_SS_A_Z);
			break;
		case "time{HH:MM:SS}":
		case "HH:MM:SS":
			timePattern = GetTimePattern(PdfXfaTimePattern.HH_MM_SS);
			break;
		case "time{hh:MM:SS A}":
		case "hh:MM:SS A":
			timePattern = GetTimePattern(PdfXfaTimePattern.HH_MM_SS_A);
			break;
		}
		return timePattern;
	}
}
