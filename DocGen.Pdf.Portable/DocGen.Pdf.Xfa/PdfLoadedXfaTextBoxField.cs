using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaTextBoxField : PdfLoadedXfaStyledField
{
	private string m_text;

	internal string Node = string.Empty;

	private float m_minW;

	private float m_maxW;

	private float m_minH;

	private float m_maxH;

	private char m_passwordChar;

	private int m_maxLength;

	private int m_combLength;

	private bool m_passwordEdit;

	private PdfXfaTextBoxType m_type;

	internal bool m_isExData;

	private string m_altText;

	private PdfMargins m_innerMargin;

	private bool isSkipDefaultMargin;

	public float MaximumWidth
	{
		get
		{
			return m_maxW;
		}
		set
		{
			m_maxW = value;
		}
	}

	public float MaximumHeight
	{
		get
		{
			return m_maxH;
		}
		set
		{
			m_maxH = value;
		}
	}

	public float MinimumWidth
	{
		get
		{
			return m_minW;
		}
		set
		{
			m_minW = value;
		}
	}

	public float MinimumHeight
	{
		get
		{
			return m_minH;
		}
		set
		{
			m_minH = value;
		}
	}

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	public PdfXfaTextBoxType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public char PasswordCharacter
	{
		get
		{
			return m_passwordChar;
		}
		set
		{
			m_passwordChar = value;
		}
	}

	public int MaximumLength
	{
		get
		{
			return m_maxLength;
		}
		set
		{
			if (value > 0)
			{
				m_maxLength = value;
			}
		}
	}

	public int CombLength
	{
		get
		{
			return m_combLength;
		}
		set
		{
			if (value >= 0)
			{
				m_combLength = value;
			}
		}
	}

	internal PdfLoadedXfaTextBoxField(string name)
	{
		base.Name = name;
	}

	internal PdfLoadedXfaTextBoxField()
	{
	}

	internal PdfLoadedXfaTextBoxField(string name, string text)
	{
		base.Name = name;
		Text = text;
	}

	internal SizeF GetFieldSize()
	{
		float num = 0f;
		float num2 = 0f;
		if (base.Width <= 0f)
		{
			if (MinimumWidth > 0f)
			{
				num2 = MinimumWidth;
			}
			else if (MaximumWidth > 0f)
			{
				num2 = MaximumWidth;
			}
		}
		else
		{
			num2 = base.Width;
		}
		if (base.Height <= 0f)
		{
			if (MinimumHeight > 0f)
			{
				num = MinimumHeight;
				if (num < MinimumHeight)
				{
					num = MinimumHeight;
				}
				PdfLoadedXfaForm pdfLoadedXfaForm = parent as PdfLoadedXfaForm;
				if (!string.IsNullOrEmpty(Text) && base.Font != null && pdfLoadedXfaForm != null && pdfLoadedXfaForm.FlowDirection != PdfLoadedXfaFlowDirection.Row)
				{
					SizeF sizeF = SizeF.Empty;
					if (m_isExData)
					{
						if (m_altText != null)
						{
							sizeF = base.Font.MeasureString(m_altText, num2);
						}
					}
					else
					{
						sizeF = base.Font.MeasureString(Text);
						if (sizeF.Width > num2)
						{
							sizeF = base.Font.MeasureString(Text, num2);
						}
					}
					if (num < sizeF.Height)
					{
						num = sizeF.Height;
						if (pdfLoadedXfaForm.FlowDirection != PdfLoadedXfaFlowDirection.Row)
						{
							num += base.Margins.Top + base.Margins.Bottom;
						}
					}
					if (MaximumHeight > 0f && sizeF.Height > MaximumHeight)
					{
						num = MaximumHeight;
					}
				}
			}
			else if (currentNode.Attributes["h"] == null)
			{
				if (base.Font != null)
				{
					if (m_isExData && !string.IsNullOrEmpty(m_altText))
					{
						base.Height = base.Font.MeasureString(m_altText, base.Width).Height;
					}
					else if (!string.IsNullOrEmpty(Text))
					{
						base.Height = base.Font.MeasureString(Text, base.Width).Height;
					}
					else if (base.Font.Height > base.Height)
					{
						base.Height = base.Font.Height + 0.5f;
					}
				}
			}
			else if (MaximumHeight > 0f)
			{
				num = MaximumHeight;
			}
		}
		else
		{
			num = base.Height;
		}
		if (base.Width < num2)
		{
			base.Width = num2;
		}
		if (base.Height < num)
		{
			base.Height = num;
		}
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle270 || base.Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(num, num2);
		}
		return new SizeF(num2, num);
	}

	private float GetWidth()
	{
		float result = 0f;
		if (base.Width <= 0f)
		{
			if (MaximumWidth > 0f)
			{
				result = MaximumWidth;
			}
			if (MinimumWidth > 0f)
			{
				result = MinimumWidth;
			}
		}
		else
		{
			result = base.Width;
		}
		return result;
	}

	internal void Read(XmlNode node, XmlDocument dataSetDoc)
	{
		currentNode = node;
		if (!(node.Name == "field"))
		{
			return;
		}
		ReadCommonProperties(node);
		if (node.Attributes["minW"] != null)
		{
			MinimumWidth = ConvertToPoint(node.Attributes["minW"].Value);
		}
		if (node.Attributes["minH"] != null)
		{
			MinimumHeight = ConvertToPoint(node.Attributes["minH"].Value);
		}
		if (node.Attributes["maxW"] != null)
		{
			MaximumWidth = ConvertToPoint(node.Attributes["maxW"].Value);
		}
		if (node.Attributes["maxH"] != null)
		{
			MaximumHeight = ConvertToPoint(node.Attributes["maxH"].Value);
		}
		if (node["value"] != null)
		{
			if (node["value"]["text"] != null)
			{
				Text = node["value"]["text"].Value;
				if (node["value"]["text"].Value == null)
				{
					Text = node["value"]["text"].InnerText;
				}
				if (node["value"]["text"].Attributes["maxChars"] != null)
				{
					m_maxLength = int.Parse(node["value"]["text"].Attributes["maxChars"].InnerText);
				}
			}
			else if (node["value"]["exData"] != null)
			{
				Text = node["value"]["exData"].InnerXml;
				m_altText = node["value"]["exData"].InnerText;
				m_isExData = true;
			}
		}
		if (node["ui"] != null)
		{
			if (node["ui"]["textEdit"] != null)
			{
				if (node["ui"]["textEdit"].Attributes["multiLine"] != null && int.Parse(node["ui"]["textEdit"].Attributes["multiLine"].InnerText) == 1)
				{
					Type = PdfXfaTextBoxType.Multiline;
				}
				if (node["ui"]["textEdit"]["comb"] != null && node["ui"]["textEdit"]["comb"].Attributes["numberOfCells"] != null)
				{
					CombLength = int.Parse(node["ui"]["textEdit"]["comb"].Attributes["numberOfCells"].InnerText);
					if (CombLength > 0)
					{
						Type = PdfXfaTextBoxType.Comb;
					}
				}
				if (node["ui"]["textEdit"]["border"] != null)
				{
					ReadBorder(node["ui"]["textEdit"]["border"], complete: false);
				}
				if (node["ui"]["textEdit"]["margin"] != null)
				{
					m_innerMargin = new PdfMargins();
					ReadMargin(node["ui"]["textEdit"]["margin"], m_innerMargin);
				}
			}
			else if (node["ui"]["passwordEdit"] != null)
			{
				Type = PdfXfaTextBoxType.Password;
				if (node["ui"]["passwordEdit"].Attributes["passwordChar"] != null)
				{
					m_passwordChar = char.Parse(node["ui"]["passwordEdit"].Attributes["passwordChar"].InnerText);
				}
				if (node["ui"]["passwordEdit"]["border"] != null)
				{
					ReadBorder(node["ui"]["passwordEdit"]["border"], complete: false);
				}
				m_passwordEdit = true;
				if (node["ui"]["passwordEdit"]["margin"] != null)
				{
					m_innerMargin = new PdfMargins();
					ReadMargin(node["ui"]["passwordEdit"]["margin"], m_innerMargin);
				}
			}
		}
		try
		{
			char[] separator = new char[1] { '.' };
			string text = string.Empty;
			bool flag = false;
			if (node["bind"] != null)
			{
				XmlNode xmlNode = node["bind"];
				if (xmlNode.Attributes["match"] != null && xmlNode.Attributes["match"].Value != "none")
				{
					flag = true;
				}
				if (xmlNode.Attributes["ref"] != null)
				{
					string value = xmlNode.Attributes["ref"].Value;
					bool flag2 = false;
					if (value.Contains("$record") || value.Contains("$"))
					{
						flag2 = true;
					}
					text = value.Replace("$record.", "");
					text = text.Replace("$.", "");
					text = text.Replace("[*]", "");
					if (!flag2 && xmlNode.Attributes["match"] != null && xmlNode.Attributes["match"].Value == "dataRef" && parent.bindingName != string.Empty)
					{
						text = parent.bindingName + "." + text;
					}
				}
				else if (xmlNode.Attributes["match"] != null && xmlNode.Attributes["match"].Value == "global")
				{
					text = base.Name;
				}
				else if (parent.bindingName != null && !flag)
				{
					text = parent.bindingName + "." + base.Name;
					text = text.Replace("$.", "");
					text = text.Replace(".", "/");
				}
			}
			if (text != string.Empty)
			{
				text = text.Replace("$.", "");
				text = text.Replace(".", "/");
				string text2 = parent.nodeName.Split(separator)[0];
				text2 = text2.Replace("[0]", "");
				text2 = text2 + "/" + text;
				text2 = "//" + text2;
				if (dataSetDoc != null)
				{
					XmlNode xmlNode2 = dataSetDoc.SelectSingleNode(text2);
					if (xmlNode2 != null)
					{
						if (xmlNode2.FirstChild != null)
						{
							if (m_isExData)
							{
								Text = xmlNode2.FirstChild.OuterXml;
								m_altText = xmlNode2.FirstChild.InnerText;
							}
							else
							{
								Text = xmlNode2.FirstChild.InnerText;
							}
						}
					}
					else if (parent.bindingName != null && !flag)
					{
						text2 = parent.nodeName.Split(separator)[0];
						text2 = text2.Replace("[0]", "");
						text = parent.bindingName + "." + base.Name;
						text = text.Replace("$.", "");
						text = text.Replace(".", "/");
						text2 = text2 + "/" + text;
						text2 = "//" + text2;
						xmlNode2 = dataSetDoc.SelectSingleNode(text2);
						if (xmlNode2 != null && xmlNode2.FirstChild != null)
						{
							Text = xmlNode2.FirstChild.InnerText;
						}
					}
				}
			}
		}
		catch
		{
		}
		nodeName = parent.nodeName;
		string text3 = string.Empty;
		if (parent.isBindingMatchNone)
		{
			text3 = nodeName;
			int startIndex = nodeName.LastIndexOf('.');
			text3 = text3.Remove(startIndex);
		}
		else if (base.Name == string.Empty)
		{
			text3 = nodeName;
		}
		string empty = string.Empty;
		empty = ((!(text3 != string.Empty)) ? base.Name : (text3 + "." + base.Name));
		if (dataSetDoc == null)
		{
			return;
		}
		string[] array = empty.Split('[');
		string text4 = string.Empty;
		string[] array2 = array;
		foreach (string text5 in array2)
		{
			if (text5.Contains("]"))
			{
				int num = text5.IndexOf(']') + 2;
				if (text5.Length > num)
				{
					text4 = text4 + "/" + text5.Substring(num);
				}
			}
			else
			{
				text4 += text5;
			}
		}
		text4 = "//" + text4;
		while (text4.Contains("#"))
		{
			int num2 = text4.IndexOf("#");
			if (num2 != -1)
			{
				string text6 = text4.Substring(0, num2 - 1);
				string text7 = text4.Substring(num2);
				int num3 = text7.IndexOf("/");
				string text8 = string.Empty;
				if (num3 != -1)
				{
					text8 = text7.Substring(num3);
				}
				text4 = text6 + text8;
				text4 = text4.TrimEnd(new char[1] { '/' });
			}
		}
		XmlNodeList xmlNodeList = dataSetDoc.SelectNodes(text4);
		int sameNameFieldsCount = parent.GetSameNameFieldsCount(base.Name);
		if (xmlNodeList != null && xmlNodeList.Count > sameNameFieldsCount)
		{
			XmlNode xmlNode3 = xmlNodeList[sameNameFieldsCount];
			if (xmlNode3 != null && xmlNode3.FirstChild != null)
			{
				Text = xmlNode3.FirstChild.InnerText;
			}
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
		if (Text != string.Empty && m_isDefaultFont)
		{
			CheckUnicodeFont(Text);
		}
		SizeF fieldSize = GetFieldSize();
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.LineAlignment = (PdfVerticalAlignment)base.VerticalAlignment;
		pdfStringFormat.Alignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		RectangleF bounds2 = default(RectangleF);
		bounds2.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
		bounds2.Size = new SizeF(fieldSize.Width - (base.Margins.Right + base.Margins.Left), fieldSize.Height - (base.Margins.Top + base.Margins.Bottom));
		if (!(bounds2.Height > 0f))
		{
			return;
		}
		bool flag = false;
		if (base.Border == null && parent is PdfLoadedXfaForm && (parent as PdfLoadedXfaForm).FlowDirection == PdfLoadedXfaFlowDirection.Row)
		{
			flag = true;
		}
		if (base.CompleteBorder != null && base.CompleteBorder.Visibility != PdfXfaVisibility.Hidden && base.CompleteBorder.Visibility != PdfXfaVisibility.Invisible && !flag)
		{
			base.CompleteBorder.DrawBorder(graphics, bounds);
		}
		if (base.Visibility != PdfXfaVisibility.Invisible && base.Caption != null)
		{
			if (base.Caption.Font == null)
			{
				base.Caption.Font = base.Font;
			}
			if (bounds2.Height < base.Caption.Font.Height)
			{
				if (bounds2.Height + (base.Margins.Top + base.Margins.Bottom) >= base.Caption.Font.Height)
				{
					bounds2.Height = base.Caption.Font.Height;
				}
				else
				{
					bounds2.Height = base.Caption.Font.Height;
				}
			}
			base.Caption.DrawText(graphics, bounds2, GetRotationAngle());
		}
		RectangleF bounds3 = GetBounds(bounds2, base.Rotate, base.Caption);
		if (Text != null && Text != string.Empty)
		{
			if (base.ForeColor.IsEmpty)
			{
				base.ForeColor = Color.Black;
			}
			if (bounds3.Height < base.Font.Height)
			{
				if (bounds3.Height + (base.Margins.Top + base.Margins.Bottom) >= base.Font.Height)
				{
					bounds3.Height = base.Font.Height;
				}
				else
				{
					bounds3.Height = base.Font.Height;
				}
				isSkipDefaultMargin = true;
			}
		}
		if (base.Visibility == PdfXfaVisibility.Hidden)
		{
			return;
		}
		if (Type == PdfXfaTextBoxType.Comb && CombLength > 0)
		{
			RectangleF tempBounds = bounds3;
			bounds3.Location = PointF.Empty;
			PdfTemplate pdfTemplate = new PdfTemplate(bounds3.Size);
			float num = bounds3.Width / (float)CombLength;
			PdfPen pen = null;
			PdfBrush brush = null;
			if (base.Border != null && base.Border.Visibility != PdfXfaVisibility.Hidden)
			{
				pen = base.Border.GetFlattenPen();
				brush = base.Border.GetBrush(bounds);
			}
			char[] array = null;
			if (Text != null && Text != string.Empty)
			{
				array = Text.ToCharArray();
			}
			RectangleF rectangleF = new RectangleF(bounds3.X, bounds3.Y, num, bounds3.Height);
			for (int i = 0; i < CombLength; i++)
			{
				if (base.Border != null && base.Border.Visibility != PdfXfaVisibility.Hidden)
				{
					pdfTemplate.Graphics.DrawRectangle(pen, brush, rectangleF);
				}
				if (array != null && array.Length > i)
				{
					pdfStringFormat.Alignment = PdfTextAlignment.Center;
					pdfTemplate.Graphics.DrawString(array[i].ToString(), base.Font, new PdfSolidBrush(base.ForeColor), rectangleF, pdfStringFormat);
				}
				rectangleF.X += num;
			}
			graphics.Save();
			graphics.TranslateTransform(tempBounds.X, tempBounds.Y);
			graphics.RotateTransform(-GetRotationAngle());
			graphics.DrawPdfTemplate(pdfTemplate, GetRenderingRect(tempBounds).Location);
			graphics.Restore();
			return;
		}
		if (base.Border != null && base.Border.Visibility != PdfXfaVisibility.Hidden)
		{
			base.Border.DrawBorder(graphics, bounds3);
		}
		if (Text == null || !(Text != string.Empty))
		{
			return;
		}
		if (m_isExData)
		{
			Text = m_altText;
		}
		if (Type == PdfXfaTextBoxType.Password)
		{
			int length = Text.Length;
			if (length > 0)
			{
				string text = string.Empty;
				_ = m_passwordChar;
				if (m_passwordChar != 0)
				{
					for (int j = 0; j < length; j++)
					{
						text += m_passwordChar;
					}
				}
				else
				{
					for (int k = 0; k < length; k++)
					{
						text += "*";
					}
				}
				Text = text;
			}
		}
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle0)
		{
			graphics.DrawString(Text, base.Font, new PdfSolidBrush(base.ForeColor), GetBounds(bounds3), pdfStringFormat);
			return;
		}
		graphics.Save();
		bounds3 = GetBounds(bounds3);
		graphics.TranslateTransform(bounds3.X, bounds3.Y);
		graphics.RotateTransform(-GetRotationAngle());
		RectangleF renderingRect = GetRenderingRect(bounds3);
		graphics.DrawString(Text, base.Font, new PdfSolidBrush(base.ForeColor), renderingRect, pdfStringFormat);
		graphics.Restore();
	}

	private RectangleF GetBounds(RectangleF bounds1)
	{
		if (m_innerMargin != null)
		{
			bounds1.X += m_innerMargin.Left;
			bounds1.Width -= m_innerMargin.Left + m_innerMargin.Right;
			if (!isSkipDefaultMargin)
			{
				bounds1.Y += m_innerMargin.Top;
				bounds1.Height -= m_innerMargin.Top + m_innerMargin.Bottom;
			}
		}
		return bounds1;
	}

	internal new void Save()
	{
		base.Save();
		if (MinimumHeight > 0f)
		{
			SetSize(currentNode, "minH", MinimumHeight);
		}
		if (MinimumWidth > 0f)
		{
			SetSize(currentNode, "minW", MinimumWidth);
		}
		if (MaximumHeight > 0f)
		{
			SetSize(currentNode, "maxH", MaximumHeight);
		}
		if (MaximumWidth > 0f)
		{
			SetSize(currentNode, "maxW", MaximumWidth);
		}
		if (Text != null || MaximumLength > 0)
		{
			if (currentNode["value"] != null)
			{
				if (currentNode["value"]["text"] != null)
				{
					if (MaximumLength > 0)
					{
						XmlNode xmlNode = currentNode["value"]["text"];
						if (xmlNode.Attributes["maxChars"] != null)
						{
							xmlNode.Attributes["maxChars"].InnerText = MaximumLength.ToString();
						}
						else
						{
							SetNewAttribute(xmlNode, "maxChars", MaximumLength.ToString());
						}
					}
				}
				else
				{
					XmlNode xmlNode2 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
					if (MaximumLength > 0)
					{
						SetNewAttribute(xmlNode2, "maxChars", MaximumLength.ToString());
					}
					currentNode["value"].AppendChild(xmlNode2);
				}
			}
			else
			{
				XmlNode xmlNode3 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "value", "");
				XmlNode xmlNode4 = xmlNode3.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
				if (Text != null)
				{
					xmlNode4.InnerText = Text;
				}
				if (MaximumLength > 0)
				{
					SetNewAttribute(xmlNode4, "maxChars", MaximumLength.ToString());
				}
				xmlNode3.AppendChild(xmlNode4);
				currentNode.AppendChild(xmlNode3);
			}
		}
		if (PasswordCharacter != 0 && Type == PdfXfaTextBoxType.Password && currentNode["ui"]["passwordEdit"] != null)
		{
			XmlNode xmlNode5 = currentNode["ui"]["passwordEdit"];
			if (xmlNode5.Attributes["passwordChar"] != null)
			{
				xmlNode5.Attributes["passwordChar"].Value = PasswordCharacter.ToString();
			}
			else
			{
				SetNewAttribute(xmlNode5, "passwordChar", PasswordCharacter.ToString());
			}
		}
		if (CombLength > 0 && Type == PdfXfaTextBoxType.Comb && currentNode["ui"]["textEdit"] != null)
		{
			XmlNode xmlNode6 = currentNode["ui"]["textEdit"];
			if (xmlNode6.Attributes["hScrollPolicy"] != null)
			{
				xmlNode6.Attributes["hScrollPolicy"].Value = "off";
			}
			else
			{
				SetNewAttribute(xmlNode6, "hScrollPolicy", "off");
			}
			if (xmlNode6["comb"] != null)
			{
				if (CombLength <= 0)
				{
					currentNode["ui"]["textEdit"].RemoveChild(xmlNode6["comb"]);
				}
				else if (xmlNode6["comb"].Attributes["numberOfCells"] != null)
				{
					xmlNode6["comb"].Attributes["numberOfCells"].Value = CombLength.ToString();
				}
				else
				{
					SetNewAttribute(xmlNode6["comb"], "numberOfCells", CombLength.ToString());
				}
			}
			else if (CombLength > 0)
			{
				XmlNode xmlNode7 = xmlNode6.OwnerDocument.CreateNode(XmlNodeType.Element, "comb", "");
				SetNewAttribute(xmlNode7, "numberOfCells", CombLength.ToString());
				xmlNode6.AppendChild(xmlNode7);
			}
		}
		if (currentNode["ui"]["textEdit"] != null)
		{
			XmlNode xmlNode8 = currentNode["ui"]["textEdit"];
			if (Type == PdfXfaTextBoxType.Multiline)
			{
				if (xmlNode8.Attributes["multiLine"] != null)
				{
					xmlNode8.Attributes["multiLine"].Value = "1";
				}
				else
				{
					SetNewAttribute(xmlNode8, "multiLine", "1");
				}
			}
		}
		if (base.Border == null)
		{
			return;
		}
		if (m_passwordEdit)
		{
			if (currentNode["ui"]["passwordEdit"]["border"] != null)
			{
				base.Border.Save(currentNode["ui"]["passwordEdit"]["border"]);
				return;
			}
			if (currentNode["border"] != null)
			{
				base.Border.Save(currentNode["border"]);
				return;
			}
			XmlNode xmlNode9 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "border", "");
			base.Border.Save(xmlNode9);
			currentNode["ui"]["passwordEdit"].AppendChild(xmlNode9);
		}
		else if (currentNode["ui"]["textEdit"]["border"] != null)
		{
			base.Border.Save(currentNode["ui"]["textEdit"]["border"]);
		}
		else if (currentNode["border"] != null)
		{
			base.Border.Save(currentNode["border"]);
		}
		else
		{
			XmlNode xmlNode10 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "border", "");
			base.Border.Save(xmlNode10);
			currentNode["ui"]["textEdit"].AppendChild(xmlNode10);
		}
	}

	internal void Fill(XmlWriter dataSetWriter, PdfLoadedXfaForm form)
	{
		if (form.acroForm.GetField(nodeName) is PdfLoadedTextBoxField pdfLoadedTextBoxField && Text != null)
		{
			pdfLoadedTextBoxField.Text = Text;
		}
		if (!string.IsNullOrEmpty(base.Name))
		{
			dataSetWriter.WriteStartElement(base.Name);
			if (Text != null)
			{
				dataSetWriter.WriteString(Text);
			}
			Save();
			dataSetWriter.WriteEndElement();
		}
	}
}
