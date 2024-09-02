using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaCheckBoxField : PdfLoadedXfaStyledField
{
	private bool m_isChecked;

	private float m_checkBoxSize;

	private PdfXfaCheckedStyle m_checkedStyle;

	private PdfXfaCheckBoxAppearance m_checkBoxAppearance;

	internal string m_innerText = string.Empty;

	internal bool isItemText;

	public bool IsChecked
	{
		get
		{
			return m_isChecked;
		}
		set
		{
			m_isChecked = value;
		}
	}

	public float CheckBoxSize
	{
		get
		{
			return m_checkBoxSize;
		}
		set
		{
			if (value > 0f)
			{
				m_checkBoxSize = value;
			}
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

	public PdfXfaCheckBoxAppearance CheckBoxAppearance
	{
		get
		{
			return m_checkBoxAppearance;
		}
		set
		{
			m_checkBoxAppearance = value;
		}
	}

	internal void Read(XmlNode node, XmlDocument dataSetDoc)
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
				CheckBoxAppearance = ((!(value == "square")) ? PdfXfaCheckBoxAppearance.Round : PdfXfaCheckBoxAppearance.Square);
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
				CheckBoxSize = ConvertToPoint(attributes["size"].Value);
			}
			if (node["ui"]["checkButton"]["border"] != null)
			{
				ReadBorder(node["ui"]["checkButton"]["border"], complete: false);
			}
		}
		if (node["items"] != null && node["items"]["text"] != null)
		{
			XmlNode xmlNode = node["items"]["text"];
			if (xmlNode.InnerText != string.Empty)
			{
				m_innerText = xmlNode.InnerText;
			}
			isItemText = true;
		}
		nodeName = parent.nodeName;
		string empty = string.Empty;
		empty = ((!(nodeName != string.Empty)) ? base.Name : (nodeName + "." + base.Name));
		if (dataSetDoc != null)
		{
			string[] array = empty.Split('[');
			string text = string.Empty;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text2.Contains("]"))
				{
					int num = text2.IndexOf(']') + 2;
					if (text2.Length > num)
					{
						text = text + "/" + text2.Substring(num);
					}
				}
				else
				{
					text += text2;
				}
			}
			text = "//" + text;
			while (text.Contains("#"))
			{
				int num2 = text.IndexOf("#");
				if (num2 != -1)
				{
					string text3 = text.Substring(0, num2 - 1);
					string text4 = text.Substring(num2);
					int num3 = text4.IndexOf("/");
					string text5 = string.Empty;
					if (num3 != -1)
					{
						text5 = text4.Substring(num3);
					}
					text = text3 + text5;
					text = text.TrimEnd(new char[1] { '/' });
				}
			}
			XmlNodeList xmlNodeList = dataSetDoc.SelectNodes(text);
			int sameNameFieldsCount = parent.GetSameNameFieldsCount(base.Name);
			if (xmlNodeList != null && xmlNodeList.Count > sameNameFieldsCount)
			{
				XmlNode xmlNode2 = xmlNodeList[sameNameFieldsCount];
				if (xmlNode2 != null && xmlNode2.FirstChild != null && xmlNode2.FirstChild.InnerText != string.Empty)
				{
					int num4 = int.Parse(xmlNode2.FirstChild.InnerText);
					IsChecked = num4 != 0;
				}
			}
		}
		else if (node["value"] != null && node["value"]["integer"] != null)
		{
			XmlNode xmlNode3 = node["value"]["integer"];
			if (xmlNode3.InnerText != string.Empty)
			{
				int num5 = int.Parse(xmlNode3.InnerText);
				IsChecked = num5 != 0;
			}
		}
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
			if (attributes["shape"].Value != CheckBoxAppearance.ToString().ToLower())
			{
				attributes["shape"].Value = CheckBoxAppearance.ToString().ToLower();
			}
		}
		else
		{
			XmlAttribute xmlAttribute = currentNode.OwnerDocument.CreateAttribute("shape");
			xmlAttribute.Value = CheckBoxAppearance.ToString().ToLower();
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
		if (CheckBoxSize > 0f)
		{
			if (attributes["size"] != null)
			{
				attributes["size"].Value = CheckBoxSize + "pt";
			}
			else
			{
				XmlAttribute xmlAttribute3 = currentNode.OwnerDocument.CreateAttribute("size");
				xmlAttribute3.Value = CheckBoxSize + "pt";
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

	internal void DrawField(PdfGraphics graphics, RectangleF bounds)
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
			base.CompleteBorder.DrawBorder(graphics, bounds);
		}
		if (base.Visibility != PdfXfaVisibility.Invisible && base.Caption != null)
		{
			if (base.Caption.Font == null)
			{
				if (base.Caption.Text != null && base.Caption.Text != string.Empty && PdfString.IsUnicode(base.Caption.Text) && m_isDefaultFont)
				{
					CheckUnicodeFont(base.Caption.Text);
				}
				base.Caption.Font = base.Font;
			}
			if (base.Caption.Font != null && bounds2.Height < base.Caption.Font.Height && bounds2.Height + base.Margins.Bottom + base.Margins.Top > base.Caption.Font.Height)
			{
				bounds2.Height = base.Caption.Font.Height;
			}
			if (base.Caption.Width == 0f && base.Caption.Text != null && base.Caption.Text != string.Empty)
			{
				float num = 10f;
				_ = CheckBoxSize;
				if (CheckBoxSize > 0f)
				{
					num = CheckBoxSize;
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
			num2 = (int)pdfPen.Width;
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
		_ = CheckBoxSize;
		if (CheckBoxSize > 0f)
		{
			num3 = CheckBoxSize;
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
		if (m_isDefaultFont && base.Font.Size > num3)
		{
			base.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, num3, PdfFontStyle.Regular);
		}
		bounds3.Width = num3;
		bounds3.Height = num3;
		PdfCheckBoxStyle style2 = GetStyle(m_checkedStyle);
		graphics.Save();
		PaintParams paintParams = new PaintParams(bounds3, backBrush, foreBrush, pdfPen, style, num2, null, GetRotationAngle());
		FieldPainter.DrawCheckBox(graphics, paintParams, StyleToString(style2), state);
		graphics.Restore();
	}

	internal void Fill(XmlWriter dataSetWriter, PdfLoadedXfaForm form)
	{
		string text = string.Empty;
		if (isItemText && form.GetSameNameFieldsCount(base.Name) > 0 && !IsChecked && int.Parse(nodeName.Split('.')[^1].Replace(base.Name + "[", "").Replace("]", "")) == 0)
		{
			foreach (PdfLoadedXfaCheckBoxField sameNameField in form.GetSameNameFields(base.Name, form))
			{
				if (sameNameField.IsChecked && sameNameField.currentNode["bind"] != null && sameNameField.currentNode["bind"].Attributes["match"] != null && sameNameField.currentNode["bind"].Attributes["match"].Value == "global")
				{
					text = ((!(sameNameField.m_innerText != string.Empty) || !(sameNameField.Name != string.Empty)) ? "1" : sameNameField.m_innerText);
				}
			}
		}
		if (form.acroForm.GetField(nodeName) is PdfLoadedCheckBoxField pdfLoadedCheckBoxField)
		{
			if (IsChecked)
			{
				pdfLoadedCheckBoxField.Checked = true;
			}
			else
			{
				pdfLoadedCheckBoxField.Checked = false;
			}
		}
		if (base.Name != string.Empty)
		{
			dataSetWriter.WriteStartElement(base.Name);
		}
		if (IsChecked)
		{
			if (m_innerText != string.Empty && base.Name != string.Empty)
			{
				dataSetWriter.WriteString(m_innerText);
			}
			else if (base.Name != string.Empty)
			{
				dataSetWriter.WriteString("1");
			}
		}
		else if (base.Name != string.Empty)
		{
			if (text != string.Empty)
			{
				dataSetWriter.WriteString(text);
			}
			else
			{
				dataSetWriter.WriteString("0");
			}
		}
		Save();
		if (base.Name != string.Empty)
		{
			dataSetWriter.WriteEndElement();
		}
	}
}
