using System;
using System.Globalization;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

public class PdfXfaCaption
{
	private string m_text;

	internal float m_width;

	private PdfFont m_font;

	private PdfXfaHorizontalAlignment m_hAlign;

	private PdfXfaVerticalAlignment m_vAlign = PdfXfaVerticalAlignment.Middle;

	private PdfXfaPosition m_position;

	private PdfColor m_foreColor;

	internal XmlNode currentNode;

	private string hAlign;

	private string vAlign;

	private RectangleF m_bounds = RectangleF.Empty;

	private PdfPage m_page;

	internal PdfXfaField parent;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (value != null)
			{
				m_text = value;
			}
		}
	}

	public PdfFont Font
	{
		get
		{
			return m_font;
		}
		set
		{
			if (value != null)
			{
				m_font = value;
			}
		}
	}

	public PdfXfaHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_hAlign;
		}
		set
		{
			m_hAlign = value;
		}
	}

	public PdfXfaVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_vAlign;
		}
		set
		{
			m_vAlign = value;
		}
	}

	public PdfXfaPosition Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	public PdfColor ForeColor
	{
		get
		{
			return m_foreColor;
		}
		set
		{
			m_foreColor = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	public PdfXfaCaption()
	{
		Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
	}

	internal PdfXfaCaption(bool flag)
	{
	}

	internal void Save(XfaWriter xfaWriter)
	{
		xfaWriter.Write.WriteStartElement("caption");
		xfaWriter.Write.WriteAttributeString("placement", Position.ToString().ToLower());
		if (m_width > 0f)
		{
			xfaWriter.Write.WriteAttributeString("reserve", m_width + "pt");
		}
		xfaWriter.WriteValue(Text, 0);
		xfaWriter.WritePragraph(VerticalAlignment, HorizontalAlignment);
		xfaWriter.WriteFontInfo(Font, ForeColor);
		xfaWriter.Write.WriteEndElement();
	}

	internal void DrawText(PdfPageBase page, RectangleF bounds)
	{
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.LineAlignment = (PdfVerticalAlignment)VerticalAlignment;
		PdfBrush brush = PdfBrushes.Black;
		if (ForeColor != PdfColor.Empty && (ForeColor.Red != 0f || ForeColor.Green != 0f || ForeColor.Blue != 0f))
		{
			brush = new PdfSolidBrush(ForeColor);
		}
		pdfStringFormat.Alignment = ConvertToPdfTextAlignment(HorizontalAlignment);
		if (m_width > 0f && Text != null)
		{
			if (Position == PdfXfaPosition.Top)
			{
				page.Graphics.DrawString(Text, Font, brush, new RectangleF(bounds.Location, new SizeF(bounds.Width, m_width)), pdfStringFormat);
			}
			else if (Position == PdfXfaPosition.Bottom)
			{
				page.Graphics.DrawString(Text, Font, brush, new RectangleF(new PointF(bounds.Location.X, bounds.Y + (bounds.Height - m_width)), new SizeF(bounds.Width, m_width)), pdfStringFormat);
			}
			else if (Position == PdfXfaPosition.Left)
			{
				page.Graphics.DrawString(Text, Font, brush, new RectangleF(bounds.Location, new SizeF(m_width, bounds.Height)), pdfStringFormat);
			}
			else
			{
				page.Graphics.DrawString(Text, Font, brush, new RectangleF(new PointF(bounds.Location.X + (bounds.Width - m_width), bounds.Location.Y), new SizeF(m_width, bounds.Height)), pdfStringFormat);
			}
		}
	}

	internal void DrawText(PdfGraphics graphics, RectangleF bounds)
	{
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.LineAlignment = (PdfVerticalAlignment)VerticalAlignment;
		PdfBrush brush = PdfBrushes.Black;
		if (ForeColor != PdfColor.Empty && (ForeColor.Red != 0f || ForeColor.Green != 0f || ForeColor.Blue != 0f))
		{
			brush = new PdfSolidBrush(ForeColor);
		}
		pdfStringFormat.Alignment = ConvertToPdfTextAlignment(HorizontalAlignment);
		if (m_width > 0f && Text != null)
		{
			if (Position == PdfXfaPosition.Top)
			{
				graphics.DrawString(Text, Font, brush, new RectangleF(bounds.Location, new SizeF(bounds.Width, m_width)), pdfStringFormat);
			}
			else if (Position == PdfXfaPosition.Bottom)
			{
				graphics.DrawString(Text, Font, brush, new RectangleF(new PointF(bounds.Location.X, bounds.Y + (bounds.Height - m_width)), new SizeF(bounds.Width, m_width)), pdfStringFormat);
			}
			else if (Position == PdfXfaPosition.Left)
			{
				graphics.DrawString(Text, Font, brush, new RectangleF(bounds.Location, new SizeF(m_width, bounds.Height)), pdfStringFormat);
			}
			else
			{
				graphics.DrawString(Text, Font, brush, new RectangleF(new PointF(bounds.Location.X + (bounds.Width - m_width), bounds.Location.Y), new SizeF(m_width, bounds.Height)), pdfStringFormat);
			}
		}
	}

	internal void DrawText(PdfGraphics graphics, RectangleF bounds, int rotationAngle)
	{
		PdfStringFormat format = new PdfStringFormat(ConvertToPdfTextAlignment(HorizontalAlignment), (PdfVerticalAlignment)VerticalAlignment);
		if (Font == null)
		{
			Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		else if (Font.Height < 1f)
		{
			Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		PdfBrush brush = PdfBrushes.Black;
		if (ForeColor != PdfColor.Empty && (ForeColor.Red != 0f || ForeColor.Green != 0f || ForeColor.Blue != 0f))
		{
			brush = new PdfSolidBrush(ForeColor);
		}
		RectangleF rectangleF = RectangleF.Empty;
		if (Text == null || !(Text != string.Empty))
		{
			return;
		}
		if (PdfString.IsUnicode(Text) && Font is PdfTrueTypeFont)
		{
			Text = PdfGraphics.NormalizeText(Font, Text);
		}
		SizeF sizeF = SizeF.Empty;
		float num = 0f;
		bool flag = false;
		if (Font != null)
		{
			sizeF = Font.MeasureString(Text);
		}
		if ((Position == PdfXfaPosition.Top || Position == PdfXfaPosition.Bottom) && m_width < Font.Height)
		{
			num = m_width;
			if (num > 0f)
			{
				flag = true;
			}
			m_width = Font.Height;
		}
		switch (Position)
		{
		case PdfXfaPosition.Top:
			switch (rotationAngle)
			{
			case 90:
				rectangleF = new RectangleF(bounds.X, bounds.Y + (bounds.Height - m_width), bounds.Height, m_width);
				break;
			case 180:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - sizeF.Width), bounds.Y + (bounds.Height - m_width), bounds.Width, m_width);
				break;
			case 270:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - m_width), bounds.Y, bounds.Height, m_width);
				break;
			case 0:
				rectangleF = new RectangleF(bounds.Location, new SizeF(bounds.Width, m_width));
				break;
			}
			break;
		case PdfXfaPosition.Bottom:
			switch (rotationAngle)
			{
			case 90:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - m_width), bounds.Y + (bounds.Height - m_width), bounds.Height, m_width);
				break;
			case 180:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - sizeF.Width), bounds.Y, bounds.Width, m_width);
				break;
			case 270:
				rectangleF = new RectangleF(bounds.X, bounds.Y, bounds.Height, m_width);
				break;
			case 0:
				rectangleF = new RectangleF(new PointF(bounds.Location.X, bounds.Y + (bounds.Height - m_width)), new SizeF(bounds.Width, m_width));
				break;
			}
			break;
		case PdfXfaPosition.Left:
			switch (rotationAngle)
			{
			case 90:
				rectangleF = new RectangleF(bounds.X, bounds.Y + (bounds.Height - m_width), m_width, bounds.Width);
				break;
			case 180:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - m_width), bounds.Y, m_width, bounds.Height);
				break;
			case 270:
				rectangleF = new RectangleF(bounds.X, bounds.Y, m_width, bounds.Width);
				break;
			case 0:
				rectangleF = new RectangleF(bounds.Location, new SizeF(m_width, bounds.Height));
				break;
			}
			break;
		case PdfXfaPosition.Right:
			switch (rotationAngle)
			{
			case 90:
				rectangleF = new RectangleF(bounds.X, bounds.Y, m_width, bounds.Width);
				break;
			case 180:
				rectangleF = new RectangleF(bounds.X, bounds.Y, m_width, bounds.Height);
				break;
			case 270:
				rectangleF = new RectangleF(bounds.X, bounds.Y + (bounds.Height - m_width), m_width, bounds.Width);
				break;
			case 0:
				rectangleF = new RectangleF(new PointF(bounds.Location.X + (bounds.Width - m_width), bounds.Location.Y), new SizeF(m_width, bounds.Height));
				break;
			}
			break;
		}
		graphics.Save();
		graphics.TranslateTransform(rectangleF.X, rectangleF.Y);
		if (rotationAngle != 0)
		{
			graphics.RotateTransform(-rotationAngle);
		}
		RectangleF layoutRectangle = RectangleF.Empty;
		switch (rotationAngle)
		{
		case 180:
			layoutRectangle = new RectangleF(0f - rectangleF.Width, 0f - rectangleF.Height, rectangleF.Width, rectangleF.Height);
			break;
		case 90:
			layoutRectangle = new RectangleF(0f - m_width, 0f, rectangleF.Width, rectangleF.Height);
			break;
		case 270:
			layoutRectangle = new RectangleF(0f, 0f - rectangleF.Height, rectangleF.Width, rectangleF.Height);
			break;
		case 0:
			layoutRectangle = new RectangleF(0f, 0f, rectangleF.Width, rectangleF.Height);
			break;
		}
		graphics.DrawString(Text, Font, brush, layoutRectangle, format);
		graphics.Restore();
		if (flag)
		{
			m_width = num;
		}
	}

	internal void DrawText(PdfPageBase page, RectangleF bounds, int rotationAngle)
	{
		PdfStringFormat format = new PdfStringFormat(ConvertToPdfTextAlignment(HorizontalAlignment), (PdfVerticalAlignment)VerticalAlignment);
		PdfBrush brush = PdfBrushes.Black;
		if (ForeColor != PdfColor.Empty && (ForeColor.Red != 0f || ForeColor.Green != 0f || ForeColor.Blue != 0f))
		{
			brush = new PdfSolidBrush(ForeColor);
		}
		RectangleF rectangleF = RectangleF.Empty;
		if (!(m_width > 0f) || Text == null || !(Text != string.Empty))
		{
			return;
		}
		SizeF sizeF = SizeF.Empty;
		if (Font != null)
		{
			sizeF = Font.MeasureString(Text);
		}
		switch (Position)
		{
		case PdfXfaPosition.Top:
			switch (rotationAngle)
			{
			case 90:
				rectangleF = new RectangleF(bounds.X, bounds.Y + (bounds.Height - m_width), bounds.Height, m_width);
				break;
			case 180:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - sizeF.Width), bounds.Y + (bounds.Height - m_width), bounds.Width, m_width);
				break;
			case 270:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - m_width), bounds.Y, bounds.Height, m_width);
				break;
			case 0:
				rectangleF = new RectangleF(bounds.Location, new SizeF(bounds.Width, m_width));
				break;
			}
			break;
		case PdfXfaPosition.Bottom:
			switch (rotationAngle)
			{
			case 90:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - m_width), bounds.Y + (bounds.Height - m_width), bounds.Height, m_width);
				break;
			case 180:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - sizeF.Width), bounds.Y, bounds.Width, m_width);
				break;
			case 270:
				rectangleF = new RectangleF(bounds.X, bounds.Y, bounds.Height, m_width);
				break;
			case 0:
				rectangleF = new RectangleF(new PointF(bounds.Location.X, bounds.Y + (bounds.Height - m_width)), new SizeF(bounds.Width, m_width));
				break;
			}
			break;
		case PdfXfaPosition.Left:
			switch (rotationAngle)
			{
			case 90:
				rectangleF = new RectangleF(bounds.X, bounds.Y + (bounds.Height - m_width), m_width, bounds.Width);
				break;
			case 180:
				rectangleF = new RectangleF(bounds.X + (bounds.Width - m_width), bounds.Y, m_width, bounds.Height);
				break;
			case 270:
				rectangleF = new RectangleF(bounds.X, bounds.Y, m_width, bounds.Width);
				break;
			case 0:
				rectangleF = new RectangleF(bounds.Location, new SizeF(m_width, bounds.Height));
				break;
			}
			break;
		case PdfXfaPosition.Right:
			switch (rotationAngle)
			{
			case 90:
				rectangleF = new RectangleF(bounds.X, bounds.Y, m_width, bounds.Width);
				break;
			case 180:
				rectangleF = new RectangleF(bounds.X, bounds.Y, m_width, bounds.Height);
				break;
			case 270:
				rectangleF = new RectangleF(bounds.X, bounds.Y + (bounds.Height - m_width), m_width, bounds.Width);
				break;
			case 0:
				rectangleF = new RectangleF(new PointF(bounds.Location.X + (bounds.Width - m_width), bounds.Location.Y), new SizeF(m_width, bounds.Height));
				break;
			}
			break;
		}
		PdfGraphics graphics = page.Graphics;
		graphics.Save();
		graphics.TranslateTransform(rectangleF.X, rectangleF.Y);
		if (rotationAngle != 0)
		{
			graphics.RotateTransform(-rotationAngle);
		}
		RectangleF layoutRectangle = RectangleF.Empty;
		switch (rotationAngle)
		{
		case 180:
			layoutRectangle = new RectangleF(0f - sizeF.Width, 0f - rectangleF.Height, rectangleF.Width, rectangleF.Height);
			break;
		case 90:
			layoutRectangle = new RectangleF(0f - m_width, 0f, rectangleF.Width, rectangleF.Height);
			break;
		case 270:
			layoutRectangle = new RectangleF(0f, 0f - rectangleF.Height, rectangleF.Width, rectangleF.Height);
			break;
		case 0:
			layoutRectangle = new RectangleF(0f, 0f, rectangleF.Width, rectangleF.Height);
			break;
		}
		graphics.DrawString(Text, Font, brush, layoutRectangle, format);
		graphics.Restore();
	}

	private PdfTextAlignment ConvertToPdfTextAlignment(PdfXfaHorizontalAlignment align)
	{
		PdfTextAlignment result = PdfTextAlignment.Center;
		switch (align)
		{
		case PdfXfaHorizontalAlignment.Center:
			result = PdfTextAlignment.Center;
			break;
		case PdfXfaHorizontalAlignment.Justify:
		case PdfXfaHorizontalAlignment.JustifyAll:
			result = PdfTextAlignment.Justify;
			break;
		case PdfXfaHorizontalAlignment.Left:
			result = PdfTextAlignment.Left;
			break;
		case PdfXfaHorizontalAlignment.Right:
			result = PdfTextAlignment.Right;
			break;
		}
		return result;
	}

	internal SizeF MeasureString()
	{
		return MeasureString(Text);
	}

	internal SizeF MeasureString(string text)
	{
		if (Font == null)
		{
			Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12f, PdfFontStyle.Regular);
		}
		if (Text != null)
		{
			return Font.MeasureString(text);
		}
		return SizeF.Empty;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	internal void Read(XmlNode node)
	{
		VerticalAlignment = PdfXfaVerticalAlignment.Top;
		currentNode = node;
		if (!(node.Name == "caption"))
		{
			return;
		}
		if (node.Attributes["placement"] != null)
		{
			switch (node.Attributes["placement"].Value)
			{
			case "left":
				Position = PdfXfaPosition.Left;
				break;
			case "right":
				Position = PdfXfaPosition.Right;
				break;
			case "top":
				Position = PdfXfaPosition.Top;
				break;
			case "bottom":
				Position = PdfXfaPosition.Bottom;
				break;
			}
		}
		if (node.Attributes["reserve"] != null)
		{
			Width = ConvertToPoint(node.Attributes["reserve"].Value);
		}
		if (node["font"] != null)
		{
			ReadFontInfo(node["font"]);
		}
		if (node["para"] != null)
		{
			if (node["para"].Attributes["hAlign"] != null)
			{
				hAlign = node["para"].Attributes["hAlign"].Value;
				switch (hAlign)
				{
				case "left":
					HorizontalAlignment = PdfXfaHorizontalAlignment.Left;
					break;
				case "right":
					HorizontalAlignment = PdfXfaHorizontalAlignment.Right;
					break;
				case "center":
					HorizontalAlignment = PdfXfaHorizontalAlignment.Center;
					break;
				case "justify":
					HorizontalAlignment = PdfXfaHorizontalAlignment.Justify;
					break;
				case "justifyAll":
					HorizontalAlignment = PdfXfaHorizontalAlignment.JustifyAll;
					break;
				}
			}
			if (node["para"].Attributes["vAlign"] != null)
			{
				vAlign = node["para"].Attributes["vAlign"].Value;
				switch (vAlign)
				{
				case "bottom":
					VerticalAlignment = PdfXfaVerticalAlignment.Bottom;
					break;
				case "middle":
					VerticalAlignment = PdfXfaVerticalAlignment.Middle;
					break;
				case "top":
					VerticalAlignment = PdfXfaVerticalAlignment.Top;
					break;
				}
			}
		}
		if (node["value"] != null)
		{
			if (node["value"]["text"] != null)
			{
				Text = node["value"]["text"].InnerText;
			}
			else if (node["value"]["exData"] != null)
			{
				Text = node["value"]["exData"].InnerText;
			}
		}
	}

	internal void Save(XmlNode node)
	{
		if (Position != 0)
		{
			if (node.Attributes["placement"] != null)
			{
				node.Attributes["placement"].Value = Position.ToString().ToLower();
			}
			else
			{
				SetNewAttribute(node, "placement", Position.ToString().ToLower());
			}
		}
		if (Width > 0f)
		{
			if (node.Attributes["reserve"] != null)
			{
				node.Attributes["reserve"].Value = Width + "pt";
			}
			else
			{
				SetNewAttribute(node, "reserve", Width + "pt");
			}
		}
		if (Font != null && node["font"] != null)
		{
			XmlNode xmlNode = node["font"];
			if (xmlNode.Attributes["typeface"] != null)
			{
				xmlNode.Attributes["typeface"].Value = Font.Name;
			}
			else
			{
				SetNewAttribute(xmlNode, "typeface", Font.Name);
			}
			if (Font.Size > 0f && Font.Size != 0.1f)
			{
				if (xmlNode.Attributes["size"] != null)
				{
					xmlNode.Attributes["size"].Value = Font.Size + "pt";
				}
				else
				{
					SetNewAttribute(xmlNode, "size", Font.Size + "pt");
				}
			}
			switch (Font.Style)
			{
			case PdfFontStyle.Bold:
				if (xmlNode.Attributes["weight"] != null)
				{
					xmlNode.Attributes["weight"].Value = Font.Style.ToString().ToLower();
				}
				else
				{
					SetNewAttribute(xmlNode, "weight", Font.Style.ToString().ToLower());
				}
				break;
			case PdfFontStyle.Italic:
				if (xmlNode.Attributes["posture"] != null)
				{
					xmlNode.Attributes["posture"].Value = Font.Style.ToString().ToLower();
				}
				else
				{
					SetNewAttribute(xmlNode, "posture", Font.Style.ToString().ToLower());
				}
				break;
			case PdfFontStyle.Strikeout:
				if (xmlNode.Attributes["linethrough"] != null)
				{
					xmlNode.Attributes["linethrough"].Value = "1";
				}
				else
				{
					SetNewAttribute(xmlNode, "linethrough", "1");
				}
				break;
			case PdfFontStyle.Underline:
				if (xmlNode.Attributes["underline"] != null)
				{
					xmlNode.Attributes["underline"].Value = "1";
				}
				else
				{
					SetNewAttribute(xmlNode, "underline", "1");
				}
				break;
			}
			_ = ForeColor;
			if (ForeColor.R != 0 || ForeColor.G != 0 || ForeColor.B != 0)
			{
				string value = ForeColor.R + "," + ForeColor.G + "," + ForeColor.B;
				if (xmlNode["fill"] != null)
				{
					if (xmlNode["fill"]["color"] != null)
					{
						if (xmlNode["fill"]["color"].Attributes["value"] != null)
						{
							xmlNode["fill"]["color"].Attributes["value"].Value = value;
						}
						else
						{
							SetNewAttribute(xmlNode["fill"]["color"], "value", value);
						}
					}
					else
					{
						XmlNode xmlNode2 = xmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "color", "");
						SetNewAttribute(xmlNode2, "value", value);
						xmlNode["fill"].AppendChild(xmlNode2);
					}
				}
				else
				{
					XmlNode xmlNode3 = xmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, "fill", "");
					XmlNode xmlNode4 = xmlNode3.OwnerDocument.CreateNode(XmlNodeType.Element, "color", "");
					SetNewAttribute(xmlNode4, "value", value);
					xmlNode3.AppendChild(xmlNode4);
					xmlNode.AppendChild(xmlNode3);
				}
			}
		}
		if (hAlign != null && hAlign != HorizontalAlignment.ToString().ToLower())
		{
			string value2 = HorizontalAlignment.ToString().ToLower();
			if (HorizontalAlignment == PdfXfaHorizontalAlignment.JustifyAll)
			{
				value2 = "justifyAll";
			}
			if (node["para"] != null)
			{
				if (node["para"].Attributes["hAlign"] != null)
				{
					node["para"].Attributes["hAlign"].Value = value2;
				}
				else
				{
					SetNewAttribute(node["para"], "hAlign", value2);
				}
			}
			else
			{
				XmlNode xmlNode5 = node.OwnerDocument.CreateNode(XmlNodeType.Element, "para", "");
				SetNewAttribute(xmlNode5, "hAlign", value2);
				node.AppendChild(xmlNode5);
			}
		}
		if (vAlign != null && VerticalAlignment.ToString().ToLower() != vAlign)
		{
			if (node["para"] != null)
			{
				if (node["para"].Attributes["vAlign"] != null)
				{
					node["para"].Attributes["vAlign"].Value = VerticalAlignment.ToString().ToLower();
				}
				else
				{
					SetNewAttribute(node["para"], "vAlign", VerticalAlignment.ToString().ToLower());
				}
			}
			else
			{
				XmlNode xmlNode6 = node.OwnerDocument.CreateNode(XmlNodeType.Element, "para", "");
				SetNewAttribute(xmlNode6, "vAlign", VerticalAlignment.ToString().ToLower());
				node.AppendChild(xmlNode6);
			}
		}
		if (Text == null || !(Text != ""))
		{
			return;
		}
		if (node["value"] != null)
		{
			if (node["value"]["text"] != null)
			{
				node["value"]["text"].InnerText = Text;
			}
			else if (node["value"]["exData"] != null)
			{
				if (node["value"]["exData"].InnerText != Text)
				{
					node["value"]["exData"].InnerText = Text;
				}
			}
			else
			{
				XmlNode xmlNode7 = node.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
				xmlNode7.InnerText = Text;
				node["value"].AppendChild(xmlNode7);
			}
		}
		else
		{
			XmlNode xmlNode8 = node.OwnerDocument.CreateNode(XmlNodeType.Element, "value", "");
			XmlNode xmlNode9 = xmlNode8.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
			xmlNode9.InnerText = Text;
			xmlNode8.AppendChild(xmlNode9);
			node.AppendChild(xmlNode8);
		}
	}

	private void SetNewAttribute(XmlNode node, string name, string value)
	{
		XmlAttribute xmlAttribute = node.OwnerDocument.CreateAttribute(name);
		xmlAttribute.Value = value;
		node.Attributes.Append(xmlAttribute);
	}

	private float ConvertToPoint(string value)
	{
		float result = 0f;
		if (value.Contains("pt"))
		{
			result = Convert.ToSingle(value.Trim('p', 't', 'm'), CultureInfo.InvariantCulture);
		}
		else if (value.Contains("m"))
		{
			result = Convert.ToSingle(value.Trim('p', 't', 'm'), CultureInfo.InvariantCulture);
			result *= 2.8346457f;
		}
		else if (value.Contains("in"))
		{
			result = Convert.ToSingle(value.Trim('i', 'n'), CultureInfo.InvariantCulture);
			result *= 72f;
		}
		return result;
	}

	private void ReadFontInfo(XmlNode fNode)
	{
		string text = string.Empty;
		float size = 10f;
		PdfFontStyle style = PdfFontStyle.Regular;
		if (fNode.Attributes["typeface"] != null)
		{
			text = fNode.Attributes["typeface"].Value;
		}
		if (fNode.Attributes["size"] != null)
		{
			size = ConvertToPoint(fNode.Attributes["size"].Value);
		}
		if (fNode.Attributes["weight"] != null)
		{
			style = PdfFontStyle.Bold;
		}
		else if (fNode.Attributes["posture"] != null)
		{
			style = PdfFontStyle.Italic;
		}
		else if (fNode.Attributes["linethrough"] != null)
		{
			style = PdfFontStyle.Strikeout;
		}
		else if (fNode.Attributes["underline"] != null)
		{
			style = PdfFontStyle.Underline;
		}
		if (fNode["fill"] != null)
		{
			XmlNode xmlNode = fNode["fill"]["color"];
			if (xmlNode != null && xmlNode.Attributes["value"] != null)
			{
				string[] array = xmlNode.Attributes["value"].Value.Split(',');
				ForeColor = new PdfColor(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2]));
			}
		}
		switch (text)
		{
		case "Times New Roman":
		case "TimesRoman":
		case "Helvetica":
		case "Courier":
		case "Symbol":
		case "ZapfDingbats":
			switch (text)
			{
			case "Times New Roman":
			case "TimesRoman":
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, size, style);
				break;
			case "Helvetica":
				Font = new PdfStandardFont(PdfFontFamily.Helvetica, size, style);
				break;
			case "Courier":
				Font = new PdfStandardFont(PdfFontFamily.Courier, size, style);
				break;
			case "Symbol":
				Font = new PdfStandardFont(PdfFontFamily.Symbol, size, style);
				break;
			case "ZapfDingbats":
				Font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, size, style);
				break;
			}
			break;
		default:
			if (text != "")
			{
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, size, style);
			}
			break;
		}
		if (Font != null && Font.Name != text)
		{
			Font = new PdfStandardFont(PdfFontFamily.TimesRoman, size, style);
		}
	}
}
