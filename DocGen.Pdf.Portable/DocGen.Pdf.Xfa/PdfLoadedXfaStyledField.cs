using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

public abstract class PdfLoadedXfaStyledField : PdfLoadedXfaField
{
	private string m_toolTip;

	private PdfXfaCaption m_caption;

	private PdfFont m_font;

	private PdfColor m_foreColor;

	private PdfXfaBorder m_border;

	private string m_presence;

	private PdfXfaVerticalAlignment m_vAlign;

	private PdfXfaHorizontalAlignment m_hAlign;

	private string hAlign;

	private string vAlign;

	private float m_width;

	private float m_height;

	private PointF m_location;

	private bool m_readOnly;

	private PdfXfaRotateAngle m_rotate;

	private PdfXfaBorder m_completeBorder;

	internal bool m_isDefaultFont;

	internal float lineHeight;

	internal PdfXfaRotateAngle Rotate
	{
		get
		{
			return m_rotate;
		}
		set
		{
			m_rotate = value;
		}
	}

	internal PdfXfaBorder CompleteBorder
	{
		get
		{
			return m_completeBorder;
		}
		set
		{
			m_completeBorder = value;
		}
	}

	public bool ReadOnly
	{
		get
		{
			return m_readOnly;
		}
		set
		{
			m_readOnly = value;
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

	public float Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	public PointF Location
	{
		get
		{
			return m_location;
		}
		set
		{
			m_location = value;
		}
	}

	public PdfFont Font
	{
		internal get
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

	public string ToolTip
	{
		get
		{
			return m_toolTip;
		}
		set
		{
			if (value != null)
			{
				m_toolTip = value;
			}
		}
	}

	public PdfXfaCaption Caption
	{
		get
		{
			if (m_caption == null)
			{
				return null;
			}
			return m_caption;
		}
		set
		{
			if (value != null)
			{
				m_caption = value;
			}
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

	public PdfXfaBorder Border
	{
		get
		{
			return m_border;
		}
		set
		{
			if (value != null)
			{
				m_border = value;
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

	internal SizeF GetSize()
	{
		if (Height <= 0f)
		{
			if (currentNode.Attributes["maxH"] != null)
			{
				Height = ConvertToPoint(currentNode.Attributes["maxH"].Value);
			}
			if (currentNode.Attributes["minH"] != null)
			{
				Height = ConvertToPoint(currentNode.Attributes["minH"].Value);
				if (Font != null && Font.Height > Height)
				{
					Height = Font.Height + 0.5f;
				}
			}
		}
		if (Width <= 0f)
		{
			if (currentNode.Attributes["maxW"] != null)
			{
				Width = ConvertToPoint(currentNode.Attributes["maxW"].Value);
			}
			if (currentNode.Attributes["minW"] != null)
			{
				Width = ConvertToPoint(currentNode.Attributes["minW"].Value);
			}
		}
		if (Rotate == PdfXfaRotateAngle.RotateAngle270 || Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(Height, Width);
		}
		return new SizeF(Width, Height);
	}

	internal PdfFont GetFont()
	{
		if (Font == null)
		{
			Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		else if (Font.Height < 1f)
		{
			Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		return Font;
	}

	internal void ReadCommonProperties(XmlNode node)
	{
		if (node.Attributes["name"] != null)
		{
			base.Name = node.Attributes["name"].Value;
		}
		if (node.Attributes["x"] != null)
		{
			Location = new PointF(ConvertToPoint(node.Attributes["x"].Value), Location.Y);
		}
		if (node.Attributes["y"] != null)
		{
			Location = new PointF(Location.X, ConvertToPoint(node.Attributes["y"].Value));
		}
		if (node.Attributes["w"] != null)
		{
			Width = ConvertToPoint(node.Attributes["w"].Value);
		}
		if (node.Attributes["h"] != null)
		{
			Height = ConvertToPoint(node.Attributes["h"].Value);
		}
		if (node.Attributes["access"] != null && node.Attributes["access"].InnerText == "readOnly")
		{
			ReadOnly = true;
		}
		if (node.Attributes["presence"] != null)
		{
			m_presence = node.Attributes["presence"].Value;
			switch (m_presence.ToLower())
			{
			case "hidden":
				base.Visibility = PdfXfaVisibility.Hidden;
				break;
			case "visible":
				base.Visibility = PdfXfaVisibility.Visible;
				break;
			case "invisible":
				base.Visibility = PdfXfaVisibility.Invisible;
				break;
			case "inactive":
				base.Visibility = PdfXfaVisibility.Inactive;
				break;
			default:
				base.Visibility = PdfXfaVisibility.Visible;
				break;
			}
		}
		if (node.Attributes["rotate"] != null)
		{
			switch (node.Attributes["rotate"].Value)
			{
			case "90":
				Rotate = PdfXfaRotateAngle.RotateAngle90;
				break;
			case "180":
				Rotate = PdfXfaRotateAngle.RotateAngle180;
				break;
			case "270":
				Rotate = PdfXfaRotateAngle.RotateAngle270;
				break;
			}
		}
		if (node["caption"] != null)
		{
			ReadCaption(node["caption"]);
		}
		if (node["margin"] != null)
		{
			ReadMargin(node["margin"]);
		}
		if (node["assist"] != null && node["assist"]["toolTip"] != null)
		{
			ToolTip = node["assist"]["toolTip"].InnerText;
		}
		if (node["border"] != null)
		{
			ReadBorder(node["border"], complete: true);
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
			if (node["para"].Attributes["marginLeft"] != null)
			{
				float num = ConvertToPoint(node["para"].Attributes["marginLeft"].Value);
				base.Margins.Left += num;
			}
			if (node["para"].Attributes["marginRight"] != null)
			{
				float num2 = ConvertToPoint(node["para"].Attributes["marginRight"].Value);
				base.Margins.Right += num2;
			}
			if (node["para"].Attributes["marginTop"] != null)
			{
				float num3 = ConvertToPoint(node["para"].Attributes["marginTop"].Value);
				base.Margins.Top += num3;
			}
			if (node["para"].Attributes["marginBottom"] != null)
			{
				float num4 = ConvertToPoint(node["para"].Attributes["marginBottom"].Value);
				base.Margins.Bottom += num4;
			}
			if (node["para"].Attributes["lineHeight"] != null)
			{
				lineHeight = ConvertToPoint(node["para"].Attributes["lineHeight"].Value);
			}
		}
		if (node["font"] != null)
		{
			ReadFontInfo(node["font"]);
		}
	}

	internal void ReadBorder(XmlNode node, bool complete)
	{
		if (complete)
		{
			CompleteBorder = new PdfXfaBorder();
			CompleteBorder.Read(node);
		}
		else
		{
			m_border = new PdfXfaBorder();
			m_border.Read(node);
		}
	}

	private void ReadCaption(XmlNode node)
	{
		m_caption = new PdfXfaCaption(flag: true);
		m_caption.Read(node);
	}

	private void ReadFontInfo(XmlNode fNode)
	{
		string text = string.Empty;
		float num = ((lineHeight > 0f) ? lineHeight : 10f);
		PdfFontStyle style = PdfFontStyle.Regular;
		if (fNode.Attributes["typeface"] != null)
		{
			text = fNode.Attributes["typeface"].Value;
		}
		if (fNode.Attributes["size"] != null)
		{
			num = ConvertToPoint(fNode.Attributes["size"].Value);
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
		case "Helvetica":
		case "Courier":
		case "Symbol":
		case "ZapfDingbats":
			switch (text)
			{
			case "Times New Roman":
			case "TimesRoman":
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, num, style);
				break;
			case "Helvetica":
				Font = new PdfStandardFont(PdfFontFamily.Helvetica, num, style);
				break;
			case "Courier":
				Font = new PdfStandardFont(PdfFontFamily.Courier, num, style);
				break;
			case "Symbol":
				Font = new PdfStandardFont(PdfFontFamily.Symbol, num, style);
				break;
			case "ZapfDingbats":
				Font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, num, style);
				break;
			}
			break;
		default:
			if (text != "" && num != 0f)
			{
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, num, style);
			}
			break;
		}
		m_isDefaultFont = true;
		if (Font != null && Font.Name != text)
		{
			Font = new PdfStandardFont(PdfFontFamily.TimesRoman, num, style);
			m_isDefaultFont = true;
		}
	}

	internal void CheckUnicodeFont(string text)
	{
		if (text != null && PdfString.IsUnicode(text) && Font != null && Font is PdfStandardFont)
		{
			Font = new PdfStandardFont(PdfFontFamily.TimesRoman, Font.Size, Font.Style);
			m_isDefaultFont = true;
		}
	}

	internal void Save()
	{
		if (ReadOnly)
		{
			if (currentNode.Attributes["access"] != null)
			{
				currentNode.Attributes["access"].Value = "readOnly";
			}
			else
			{
				XmlAttribute xmlAttribute = currentNode.OwnerDocument.CreateAttribute("access");
				xmlAttribute.InnerText = "readOnly";
				currentNode.Attributes.Append(xmlAttribute);
			}
		}
		if (Width > 0f)
		{
			SetSize(currentNode, "w", Width);
		}
		if (Height > 0f)
		{
			SetSize(currentNode, "h", Height);
		}
		if (Caption != null)
		{
			if (currentNode["caption"] != null)
			{
				Caption.Save(currentNode["caption"]);
			}
			else
			{
				XmlNode xmlNode = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "caption", "");
				Caption.Save(xmlNode);
				currentNode.AppendChild(xmlNode);
			}
		}
		if ((m_presence != null || m_presence != "") && base.Visibility != 0 && m_presence != base.Visibility.ToString().ToLower())
		{
			if (currentNode.Attributes["presence"] != null)
			{
				currentNode.Attributes["presence"].Value = base.Visibility.ToString().ToLower();
			}
			else
			{
				SetNewAttribute(currentNode, "presence", base.Visibility.ToString().ToLower());
			}
		}
		if (Font != null)
		{
			SetFont(currentNode);
		}
		SetMargin(currentNode);
		if (ToolTip != null && ToolTip != "")
		{
			if (currentNode["assist"] != null)
			{
				if (currentNode["assist"]["toolTip"] != null)
				{
					currentNode["assist"]["toolTip"].InnerText = ToolTip;
				}
				else
				{
					XmlNode xmlNode2 = currentNode["assist"].OwnerDocument.CreateNode(XmlNodeType.Element, "toolTip", "");
					xmlNode2.InnerText = ToolTip;
					currentNode["assist"].AppendChild(xmlNode2);
				}
			}
			else
			{
				XmlNode xmlNode3 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "assist", "");
				XmlNode xmlNode4 = xmlNode3.OwnerDocument.CreateNode(XmlNodeType.Element, "toolTip", "");
				xmlNode4.InnerText = ToolTip;
				xmlNode3.AppendChild(xmlNode4);
				currentNode.AppendChild(xmlNode3);
			}
		}
		if (hAlign != null && hAlign != HorizontalAlignment.ToString().ToLower())
		{
			string value = HorizontalAlignment.ToString().ToLower();
			if (HorizontalAlignment == PdfXfaHorizontalAlignment.JustifyAll)
			{
				value = "justifyAll";
			}
			if (currentNode["para"] != null)
			{
				if (currentNode["para"].Attributes["hAlign"] != null)
				{
					currentNode["para"].Attributes["hAlign"].Value = value;
				}
				else
				{
					SetNewAttribute(currentNode["para"], "hAlign", value);
				}
			}
			else
			{
				XmlNode xmlNode5 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "para", "");
				SetNewAttribute(xmlNode5, "hAlign", value);
				currentNode.AppendChild(xmlNode5);
			}
		}
		if (vAlign == null || !(VerticalAlignment.ToString().ToLower() != vAlign))
		{
			return;
		}
		if (currentNode["para"] != null)
		{
			if (currentNode["para"].Attributes["vAlign"] != null)
			{
				currentNode["para"].Attributes["vAlign"].Value = VerticalAlignment.ToString().ToLower();
			}
			else
			{
				SetNewAttribute(currentNode["para"], "vAlign", VerticalAlignment.ToString().ToLower());
			}
		}
		else
		{
			XmlNode xmlNode6 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "para", "");
			SetNewAttribute(xmlNode6, "vAlign", VerticalAlignment.ToString().ToLower());
			currentNode.AppendChild(xmlNode6);
		}
	}

	private void SetMargin(XmlNode node)
	{
		if (base.Margins.Bottom == 0f && base.Margins.Top == 0f && base.Margins.Left == 0f && base.Margins.Right == 0f)
		{
			return;
		}
		if (node["margin"] != null)
		{
			XmlAttributeCollection attributes = node["margin"].Attributes;
			if (attributes["leftInset"] != null)
			{
				attributes["leftInset"].Value = base.Margins.Left + "pt";
			}
			else
			{
				SetNewAttribute(node["margin"], "leftInset", base.Margins.Left + "pt");
			}
			if (attributes["rightInset"] != null)
			{
				attributes["rightInset"].Value = base.Margins.Right + "pt";
			}
			else
			{
				SetNewAttribute(node["margin"], "rightInset", base.Margins.Right + "pt");
			}
			if (attributes["bottomInset"] != null)
			{
				attributes["bottomInset"].Value = base.Margins.Bottom + "pt";
			}
			else
			{
				SetNewAttribute(node["margin"], "bottomInset", base.Margins.Bottom + "pt");
			}
			if (attributes["topInset"] != null)
			{
				attributes["topInset"].Value = base.Margins.Top + "pt";
			}
			else
			{
				SetNewAttribute(node["margin"], "topInset", base.Margins.Top + "pt");
			}
		}
		else
		{
			XmlNode node2 = node.OwnerDocument.CreateNode(XmlNodeType.Element, "margin", "");
			SetNewAttribute(node2, "leftInset", base.Margins.Left + "pt");
			SetNewAttribute(node2, "rightInset", base.Margins.Right + "pt");
			SetNewAttribute(node2, "bottomInset", base.Margins.Bottom + "pt");
			SetNewAttribute(node2, "topInset", base.Margins.Top + "pt");
		}
	}

	internal PdfCheckBoxStyle GetStyle(PdfXfaCheckedStyle style)
	{
		PdfCheckBoxStyle result = PdfCheckBoxStyle.Check;
		switch (style)
		{
		case PdfXfaCheckedStyle.Check:
			result = PdfCheckBoxStyle.Check;
			break;
		case PdfXfaCheckedStyle.Circle:
			result = PdfCheckBoxStyle.Circle;
			break;
		case PdfXfaCheckedStyle.Cross:
			result = PdfCheckBoxStyle.Cross;
			break;
		case PdfXfaCheckedStyle.Diamond:
			result = PdfCheckBoxStyle.Diamond;
			break;
		case PdfXfaCheckedStyle.Square:
			result = PdfCheckBoxStyle.Square;
			break;
		case PdfXfaCheckedStyle.Star:
			result = PdfCheckBoxStyle.Star;
			break;
		}
		return result;
	}

	internal string StyleToString(PdfCheckBoxStyle style)
	{
		return style switch
		{
			PdfCheckBoxStyle.Circle => "l", 
			PdfCheckBoxStyle.Cross => "8", 
			PdfCheckBoxStyle.Diamond => "u", 
			PdfCheckBoxStyle.Square => "n", 
			PdfCheckBoxStyle.Star => "H", 
			_ => "4", 
		};
	}

	private void SetFont(XmlNode node)
	{
		if (Font == null || node["font"] == null)
		{
			return;
		}
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
		if (ForeColor.R == 0 && ForeColor.G == 0 && ForeColor.B == 0)
		{
			return;
		}
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

	internal void SetNewAttribute(XmlNode node, string name, string value)
	{
		XmlAttribute xmlAttribute = node.OwnerDocument.CreateAttribute(name);
		xmlAttribute.Value = value;
		node.Attributes.Append(xmlAttribute);
	}

	internal int GetRotationAngle()
	{
		int result = 0;
		if (Rotate != 0)
		{
			switch (Rotate)
			{
			case PdfXfaRotateAngle.RotateAngle180:
				result = 180;
				break;
			case PdfXfaRotateAngle.RotateAngle270:
				result = 270;
				break;
			case PdfXfaRotateAngle.RotateAngle90:
				result = 90;
				break;
			}
		}
		return result;
	}

	internal RectangleF GetRenderingRect(RectangleF tempBounds)
	{
		RectangleF result = default(RectangleF);
		switch (GetRotationAngle())
		{
		case 180:
			result = new RectangleF(0f - tempBounds.Width, 0f - tempBounds.Height, tempBounds.Width, tempBounds.Height);
			break;
		case 90:
			result = new RectangleF(0f - tempBounds.Height, 0f, tempBounds.Height, tempBounds.Width);
			break;
		case 270:
			result = new RectangleF(0f, 0f - tempBounds.Width, tempBounds.Height, tempBounds.Width);
			break;
		case 0:
			result = new RectangleF(0f, 0f, tempBounds.Width, tempBounds.Height);
			break;
		}
		return result;
	}
}
