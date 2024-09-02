using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaRadioButtonField : PdfLoadedXfaStyledField
{
	internal bool m_isChecked;

	internal string vText;

	internal string iText;

	private PdfXfaCheckedStyle m_checkedStyle;

	private PdfXfaCheckBoxAppearance m_radioButtonAppearance = PdfXfaCheckBoxAppearance.Round;

	private float m_radioButtonSize;

	public bool IsChecked
	{
		get
		{
			return m_isChecked;
		}
		set
		{
			if (value && parent is PdfLoadedXfaRadioButtonGroup)
			{
				(parent as PdfLoadedXfaRadioButtonGroup).ResetSelection();
			}
			m_isChecked = value;
		}
	}

	public float RadioButtonSize
	{
		get
		{
			return m_radioButtonSize;
		}
		set
		{
			m_radioButtonSize = value;
		}
	}

	public PdfXfaCheckedStyle CheckedStyle
	{
		get
		{
			return m_checkedStyle;
		}
		set
		{
			m_checkedStyle = value;
		}
	}

	public PdfXfaCheckBoxAppearance RadioButtonAppearance
	{
		get
		{
			return m_radioButtonAppearance;
		}
		set
		{
			m_radioButtonAppearance = value;
		}
	}

	internal void ReadField(XmlNode node)
	{
		currentNode = node;
		if (!(node.Name == "field"))
		{
			return;
		}
		ReadCommonProperties(currentNode);
		if (node["ui"]["checkButton"] != null)
		{
			XmlAttributeCollection attributes = node["ui"]["checkButton"].Attributes;
			if (attributes["shape"] != null)
			{
				string value = attributes["shape"].Value;
				RadioButtonAppearance = ((!(value == "square")) ? PdfXfaCheckBoxAppearance.Round : PdfXfaCheckBoxAppearance.Square);
			}
			if (attributes["mark"] != null)
			{
				switch (attributes["mark"].Value)
				{
				case "check":
					CheckedStyle = PdfXfaCheckedStyle.Check;
					break;
				case "cross":
					CheckedStyle = PdfXfaCheckedStyle.Cross;
					break;
				case "circle":
					CheckedStyle = PdfXfaCheckedStyle.Circle;
					break;
				case "diamond":
					CheckedStyle = PdfXfaCheckedStyle.Diamond;
					break;
				case "square":
					CheckedStyle = PdfXfaCheckedStyle.Square;
					break;
				case "star":
					CheckedStyle = PdfXfaCheckedStyle.Star;
					break;
				case "default":
					CheckedStyle = PdfXfaCheckedStyle.Default;
					break;
				}
			}
			if (attributes["size"] != null)
			{
				RadioButtonSize = ConvertToPoint(attributes["size"].Value);
			}
			if (node["ui"]["checkButton"]["border"] != null)
			{
				ReadBorder(node["ui"]["checkButton"]["border"], complete: false);
			}
		}
		nodeName = parent.nodeName + "." + base.Name;
		if (node["value"] != null)
		{
			if (node["value"]["text"] != null)
			{
				vText = node["value"]["text"].InnerText;
			}
			if (node["value"]["items"] != null)
			{
				if (node["value"]["items"]["text"] != null)
				{
					iText = node["value"]["items"]["text"].InnerText;
				}
				else if (node["value"]["items"]["integer"] != null)
				{
					iText = node["value"]["items"]["integer"].InnerText;
				}
			}
		}
		if (node["items"] != null)
		{
			if (node["items"]["text"] != null)
			{
				iText = node["items"]["text"].InnerText;
			}
			else if (node["items"]["integer"] != null)
			{
				iText = node["items"]["integer"].InnerText;
			}
		}
	}

	internal void DrawRadioButton(PdfGraphics graphics, RectangleF bounds)
	{
		if (base.Font == null)
		{
			base.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		else if (base.Font.Height < 1f)
		{
			base.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		SizeF size = GetSize();
		new PdfStringFormat
		{
			LineAlignment = (PdfVerticalAlignment)base.VerticalAlignment,
			Alignment = ConvertToPdfTextAlignment(base.HorizontalAlignment)
		};
		RectangleF bounds2 = default(RectangleF);
		bounds2.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
		bounds2.Size = new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom));
		bool flag = false;
		if (base.Border == null && parent is PdfLoadedXfaForm && (parent as PdfLoadedXfaForm).FlowDirection == PdfLoadedXfaFlowDirection.Row)
		{
			flag = true;
		}
		if (base.CompleteBorder != null && !flag)
		{
			base.CompleteBorder.DrawBorder(graphics, bounds2);
		}
		if (base.Visibility != PdfXfaVisibility.Invisible && base.Caption != null)
		{
			if (base.Caption.Font == null)
			{
				base.Caption.Font = base.Font;
			}
			if (base.Caption.Font != null && bounds2.Height < base.Caption.Font.Height && bounds2.Height + base.Margins.Bottom + base.Margins.Top > base.Caption.Font.Height)
			{
				bounds2.Height = base.Caption.Font.Height;
			}
			if (base.Caption.Width == 0f && base.Caption.Text != null && base.Caption.Text != string.Empty)
			{
				float num = 10f;
				_ = RadioButtonSize;
				if (RadioButtonSize > 0f)
				{
					num = RadioButtonSize;
				}
				if (base.Caption.Position == PdfXfaPosition.Left || base.Caption.Position == PdfXfaPosition.Right)
				{
					base.Caption.Width = bounds2.Width - num;
				}
				else
				{
					base.Caption.Width = bounds2.Height - num;
				}
			}
			base.Caption.DrawText(graphics, bounds2, GetRotationAngle());
		}
		RectangleF bounds3 = GetBounds(bounds2, base.Rotate, base.Caption);
		PdfBrush foreBrush = PdfBrushes.Black;
		if (!base.ForeColor.IsEmpty)
		{
			foreBrush = new PdfSolidBrush(base.ForeColor);
		}
		PdfBrush backBrush = null;
		PdfPen pdfPen = null;
		PdfBorderStyle style = PdfBorderStyle.Solid;
		int num2 = 0;
		if (base.Border != null && base.Border.Visibility != PdfXfaVisibility.Hidden && base.Border.Visibility != PdfXfaVisibility.Invisible)
		{
			backBrush = base.Border.GetBrush(bounds);
			pdfPen = base.Border.GetFlattenPen();
			style = base.Border.GetBorderStyle();
			num2 = (int)base.Border.Width;
			if (num2 == 0 && pdfPen.Width > 0f)
			{
				num2 = 1;
			}
		}
		PdfCheckFieldState state = PdfCheckFieldState.Unchecked;
		if (IsChecked)
		{
			state = PdfCheckFieldState.Checked;
		}
		float num3 = 10f;
		_ = RadioButtonSize;
		if (RadioButtonSize > 0f)
		{
			num3 = RadioButtonSize;
		}
		float num4 = bounds3.Height - num3;
		float num5 = bounds3.Width - num3;
		if (base.VerticalAlignment == PdfXfaVerticalAlignment.Middle)
		{
			bounds3.Y += num4 / 2f;
		}
		else if (base.VerticalAlignment == PdfXfaVerticalAlignment.Bottom)
		{
			bounds3.Y += num4;
		}
		if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Center)
		{
			bounds3.X += num5 / 2f;
		}
		else if (base.HorizontalAlignment == PdfXfaHorizontalAlignment.Right)
		{
			bounds3.X += num5;
		}
		bounds3.Width = num3;
		bounds3.Height = num3;
		PdfCheckBoxStyle style2 = GetStyle(m_checkedStyle);
		PdfBrush shadowBrush = new PdfSolidBrush(Color.Empty);
		graphics.Save();
		PaintParams paintParams = new PaintParams(bounds3, backBrush, foreBrush, pdfPen, style, num2, shadowBrush, GetRotationAngle());
		FieldPainter.DrawRadioButton(graphics, paintParams, StyleToString(style2), state);
		graphics.Restore();
	}

	internal new void Save()
	{
		base.Save();
		if (currentNode["ui"]["checkButton"] == null)
		{
			return;
		}
		XmlAttributeCollection attributes = currentNode["ui"]["checkButton"].Attributes;
		if (attributes["shape"] != null)
		{
			if (attributes["shape"].Value != RadioButtonAppearance.ToString().ToLower())
			{
				attributes["shape"].Value = RadioButtonAppearance.ToString().ToLower();
			}
		}
		else
		{
			XmlAttribute xmlAttribute = currentNode.OwnerDocument.CreateAttribute("shape");
			xmlAttribute.Value = RadioButtonAppearance.ToString().ToLower();
			attributes.Append(xmlAttribute);
		}
		if (attributes["mark"] != null)
		{
			if (attributes["mark"].Value != CheckedStyle.ToString().ToLower())
			{
				attributes["mark"].Value = CheckedStyle.ToString().ToLower();
			}
		}
		else
		{
			XmlAttribute xmlAttribute2 = currentNode.OwnerDocument.CreateAttribute("mark");
			xmlAttribute2.Value = CheckedStyle.ToString().ToLower();
			attributes.Append(xmlAttribute2);
		}
		if (RadioButtonSize > 0f)
		{
			if (attributes["size"] != null)
			{
				attributes["size"].Value = RadioButtonSize + "pt";
			}
			else
			{
				XmlAttribute xmlAttribute3 = currentNode.OwnerDocument.CreateAttribute("size");
				xmlAttribute3.Value = RadioButtonSize + "pt";
				attributes.Append(xmlAttribute3);
			}
		}
		if (base.Border != null)
		{
			if (currentNode["ui"]["checkButton"]["border"] != null)
			{
				base.Border.Save(currentNode["ui"]["checkButton"]["border"]);
				return;
			}
			if (currentNode["border"] != null)
			{
				base.Border.Save(currentNode["border"]);
				return;
			}
			XmlNode xmlNode = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "border", "");
			base.Border.Save(xmlNode);
			currentNode["ui"]["checkButton"].AppendChild(xmlNode);
		}
	}
}
