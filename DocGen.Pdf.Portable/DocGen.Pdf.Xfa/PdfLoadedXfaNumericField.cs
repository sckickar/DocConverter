using System.Text.RegularExpressions;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaNumericField : PdfLoadedXfaStyledField
{
	private double m_numericValue = double.NaN;

	internal string fullName = string.Empty;

	private PdfXfaNumericType m_fieldType;

	private int m_combLength;

	private string m_patternString = string.Empty;

	private PdfMargins m_innerMargin;

	private bool isSkipDefaultMargin;

	public double NumericValue
	{
		get
		{
			return m_numericValue;
		}
		set
		{
			m_numericValue = value;
		}
	}

	public int CombLenght
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

	public PdfXfaNumericType FieldType
	{
		get
		{
			return m_fieldType;
		}
		internal set
		{
			m_fieldType = value;
		}
	}

	public string PatternString => m_patternString;

	internal void Read(XmlNode node, XmlDocument dataSetDoc)
	{
		currentNode = node;
		if (!(node.Name == "field"))
		{
			return;
		}
		ReadCommonProperties(currentNode);
		if (node["value"] != null)
		{
			float result = 0f;
			if (node["value"]["float"] != null)
			{
				if (node["value"]["float"].Value != null)
				{
					float.TryParse(node["value"]["float"].Value, out result);
				}
				else if (!string.IsNullOrEmpty(node["value"]["float"].InnerText))
				{
					float.TryParse(node["value"]["float"].InnerText, out result);
				}
				m_fieldType = PdfXfaNumericType.Float;
			}
			else if (node["value"]["decimal"] != null)
			{
				if (node["value"]["decimal"].Value != null)
				{
					float.TryParse(node["value"]["decimal"].Value, out result);
				}
				else if (!string.IsNullOrEmpty(node["value"]["decimal"].InnerText))
				{
					float.TryParse(node["value"]["decimal"].InnerText, out result);
				}
				m_fieldType = PdfXfaNumericType.Decimal;
			}
			else if (node["value"]["integer"] != null)
			{
				if (node["value"]["integer"].Value != null)
				{
					float.TryParse(node["value"]["integer"].Value, out result);
				}
				else if (!string.IsNullOrEmpty(node["value"]["integer"].InnerText))
				{
					float.TryParse(node["value"]["integer"].InnerText, out result);
				}
				m_fieldType = PdfXfaNumericType.Integer;
			}
			NumericValue = result;
		}
		if (node["format"] != null && node["format"]["picture"] != null)
		{
			m_patternString = node["format"]["picture"].InnerText;
			if (m_patternString.Contains("$") || m_patternString.Contains("num.currency"))
			{
				m_fieldType = PdfXfaNumericType.Currency;
			}
			else if (m_patternString.Contains("%") || m_patternString.Contains("num.percent"))
			{
				m_fieldType = PdfXfaNumericType.Percent;
			}
			else if (m_patternString.Contains("num.integer"))
			{
				m_fieldType = PdfXfaNumericType.Integer;
			}
			else if (m_patternString.Contains("num.decimal"))
			{
				m_fieldType = PdfXfaNumericType.Decimal;
			}
		}
		if (node["ui"]["numericEdit"] != null)
		{
			if (node["ui"]["numericEdit"]["comb"] != null)
			{
				XmlNode xmlNode = node["ui"]["numericEdit"]["comb"];
				if (xmlNode.Attributes["numberOfCells"] != null)
				{
					string value = xmlNode.Attributes["numberOfCells"].Value;
					if (!string.IsNullOrEmpty(value))
					{
						int result2 = 0;
						int.TryParse(value, out result2);
						m_combLength = result2;
					}
				}
			}
			if (node["ui"]["numericEdit"]["border"] != null)
			{
				ReadBorder(node["ui"]["numericEdit"]["border"], complete: false);
			}
			if (node["ui"]["numericEdit"]["margin"] != null)
			{
				m_innerMargin = new PdfMargins();
				ReadMargin(node["ui"]["numericEdit"]["margin"], m_innerMargin);
			}
		}
		try
		{
			char[] separator = new char[1] { '.' };
			string text = string.Empty;
			bool flag = false;
			if (node["bind"] != null)
			{
				XmlNode xmlNode2 = node["bind"];
				if (xmlNode2.Attributes["match"] != null && xmlNode2.Attributes["match"].Value != "none")
				{
					flag = true;
				}
				if (xmlNode2.Attributes["ref"] != null)
				{
					string value2 = xmlNode2.Attributes["ref"].Value;
					bool flag2 = false;
					if (value2.Contains("$record") || value2.Contains("$"))
					{
						flag2 = true;
					}
					text = value2.Replace("$record.", "");
					text = text.Replace("$.", "");
					text = text.Replace("[*]", "");
					if (!flag2 && xmlNode2.Attributes["match"] != null && xmlNode2.Attributes["match"].Value == "dataRef" && parent.bindingName != string.Empty)
					{
						text = parent.bindingName + "." + text;
					}
				}
				else if (xmlNode2.Attributes["match"] != null && xmlNode2.Attributes["match"].Value == "global")
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
					double result3 = 0.0;
					XmlNode xmlNode3 = dataSetDoc.SelectSingleNode(text2);
					if (xmlNode3 != null)
					{
						if (xmlNode3.FirstChild != null && !string.IsNullOrEmpty(Regex.Replace(xmlNode3.FirstChild.InnerText, "[^0-9.]", "")))
						{
							double.TryParse(xmlNode3.FirstChild.InnerText, out result3);
							NumericValue = result3;
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
						xmlNode3 = dataSetDoc.SelectSingleNode(text2);
						if (xmlNode3 != null && xmlNode3.FirstChild != null && !string.IsNullOrEmpty(Regex.Replace(xmlNode3.FirstChild.InnerText, "[^0-9.]", "")))
						{
							double.TryParse(xmlNode3.FirstChild.InnerText, out result3);
							NumericValue = result3;
						}
					}
				}
			}
		}
		catch
		{
		}
		nodeName = parent.nodeName;
		string empty = string.Empty;
		empty = ((!(nodeName != string.Empty)) ? base.Name : (nodeName + "." + base.Name));
		if (dataSetDoc == null)
		{
			return;
		}
		string[] array = empty.Split('[');
		string text3 = string.Empty;
		string[] array2 = array;
		foreach (string text4 in array2)
		{
			if (text4.Contains("]"))
			{
				int num = text4.IndexOf(']') + 2;
				if (text4.Length > num)
				{
					text3 = text3 + "/" + text4.Substring(num);
				}
			}
			else
			{
				text3 += text4;
			}
		}
		text3 = "//" + text3;
		while (text3.Contains("#"))
		{
			int num2 = text3.IndexOf("#");
			if (num2 != -1)
			{
				string text5 = text3.Substring(0, num2 - 1);
				string text6 = text3.Substring(num2);
				int num3 = text6.IndexOf("/");
				string text7 = string.Empty;
				if (num3 != -1)
				{
					text7 = text6.Substring(num3);
				}
				text3 = text5 + text7;
				text3 = text3.TrimEnd(new char[1] { '/' });
			}
		}
		XmlNodeList xmlNodeList = dataSetDoc.SelectNodes(text3);
		int sameNameFieldsCount = parent.GetSameNameFieldsCount(base.Name);
		if (xmlNodeList != null && xmlNodeList.Count > sameNameFieldsCount)
		{
			XmlNode xmlNode4 = xmlNodeList[sameNameFieldsCount];
			if (xmlNode4 != null && xmlNode4.FirstChild != null && xmlNode4.FirstChild.InnerText != string.Empty)
			{
				double result4 = 0.0;
				double.TryParse(xmlNode4.FirstChild.InnerText, out result4);
				NumericValue = result4;
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
		SizeF size = GetSize();
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.LineAlignment = (PdfVerticalAlignment)base.VerticalAlignment;
		pdfStringFormat.Alignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		RectangleF bounds2 = default(RectangleF);
		bounds2.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
		bounds2.Size = new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom));
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
		if (base.Visibility != PdfXfaVisibility.Hidden && base.Caption != null)
		{
			if (base.Caption.Font == null)
			{
				base.Caption.Font = base.Font;
			}
			if (bounds2.Height < base.Caption.Font.Height && bounds2.Height + (base.Margins.Top + base.Margins.Bottom) >= base.Caption.Font.Height)
			{
				bounds2.Height = base.Caption.Font.Height;
			}
			base.Caption.DrawText(graphics, bounds2, GetRotationAngle());
		}
		RectangleF bounds3 = GetBounds(bounds2, base.Rotate, base.Caption);
		if (base.Visibility == PdfXfaVisibility.Hidden)
		{
			return;
		}
		if (base.ForeColor.IsEmpty)
		{
			base.ForeColor = Color.Black;
		}
		string text = string.Empty;
		if (!double.IsNaN(NumericValue))
		{
			if (bounds3.Height < base.Font.Height && bounds3.Height + (base.Margins.Top + base.Margins.Bottom) >= base.Font.Height)
			{
				bounds3.Height = base.Font.Height;
				isSkipDefaultMargin = true;
			}
			if (PatternString != string.Empty)
			{
				char[] separator = new char[1] { '.' };
				string[] array = PatternString.Split(separator);
				if (array.Length > 1)
				{
					text = "0.";
					string text2 = array[1];
					for (int i = 0; i < text2.Length; i++)
					{
						_ = text2[i];
						text += "0";
					}
				}
			}
		}
		string text3 = string.Empty;
		if (!double.IsNaN(NumericValue))
		{
			text3 = ((text != string.Empty) ? NumericValue.ToString(text) : NumericValue.ToString());
		}
		if (CombLenght > 0)
		{
			RectangleF tempBounds = bounds3;
			bounds3.Location = PointF.Empty;
			PdfTemplate pdfTemplate = new PdfTemplate(bounds3.Size);
			float num = bounds3.Width / (float)CombLenght;
			PdfPen pen = null;
			PdfBrush brush = null;
			if (base.Border != null && base.Border.Visibility != PdfXfaVisibility.Hidden)
			{
				pen = base.Border.GetFlattenPen();
				brush = base.Border.GetBrush(bounds);
			}
			char[] array2 = null;
			if (text3 != null && text3 != string.Empty)
			{
				array2 = text3.ToCharArray();
			}
			RectangleF rectangleF = new RectangleF(bounds3.X, bounds3.Y, num, bounds3.Height);
			for (int j = 0; j < CombLenght; j++)
			{
				if (base.Border != null && base.Border.Visibility != PdfXfaVisibility.Hidden)
				{
					pdfTemplate.Graphics.DrawRectangle(pen, brush, rectangleF);
				}
				if (array2 != null && array2.Length > j)
				{
					pdfStringFormat.Alignment = PdfTextAlignment.Center;
					pdfTemplate.Graphics.DrawString(array2[j].ToString(), base.Font, new PdfSolidBrush(base.ForeColor), rectangleF, pdfStringFormat);
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
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle0)
		{
			graphics.DrawString(text3, base.Font, new PdfSolidBrush(base.ForeColor), isSkipDefaultMargin ? bounds3 : GetBounds(bounds3), pdfStringFormat);
			return;
		}
		graphics.Save();
		if (!isSkipDefaultMargin)
		{
			bounds3 = GetBounds(bounds3);
		}
		graphics.TranslateTransform(bounds3.X, bounds3.Y);
		graphics.RotateTransform(-GetRotationAngle());
		RectangleF renderingRect = GetRenderingRect(bounds3);
		graphics.DrawString(text3, base.Font, new PdfSolidBrush(base.ForeColor), renderingRect, pdfStringFormat);
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

	internal SizeF GetNumericFieldSize()
	{
		if (base.Height <= 0f)
		{
			if (currentNode.Attributes["maxH"] != null)
			{
				base.Height = ConvertToPoint(currentNode.Attributes["maxH"].Value);
			}
			if (currentNode.Attributes["minH"] != null)
			{
				base.Height = ConvertToPoint(currentNode.Attributes["minH"].Value);
			}
		}
		if (base.Width <= 0f)
		{
			if (currentNode.Attributes["maxW"] != null)
			{
				base.Width = ConvertToPoint(currentNode.Attributes["maxW"].Value);
			}
			if (currentNode.Attributes["minW"] != null)
			{
				base.Width = ConvertToPoint(currentNode.Attributes["minW"].Value);
			}
		}
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle270 || base.Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(base.Height, base.Width);
		}
		return new SizeF(base.Width, base.Height);
	}

	internal new void Save()
	{
		base.Save();
		if (currentNode["value"] != null)
		{
			if (currentNode[FieldType.ToString().ToLower()] == null)
			{
				XmlNode newChild = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, FieldType.ToString().ToLower(), "");
				currentNode["value"].AppendChild(newChild);
			}
		}
		else
		{
			XmlNode xmlNode = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "value", "");
			XmlNode newChild2 = xmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, FieldType.ToString().ToLower(), "");
			xmlNode.AppendChild(newChild2);
			currentNode.AppendChild(xmlNode);
		}
		if (currentNode["ui"]["numericEdit"] != null)
		{
			if (currentNode["ui"]["numericEdit"]["comb"] != null)
			{
				XmlNode xmlNode2 = currentNode["ui"]["numericEdit"]["comb"];
				if (CombLenght <= 0)
				{
					currentNode["ui"]["numericEdit"].RemoveChild(xmlNode2);
				}
				else if (xmlNode2.Attributes["numberOfCells"] != null)
				{
					xmlNode2.Attributes["numberOfCells"].Value = CombLenght.ToString();
				}
				else
				{
					SetNewAttribute(xmlNode2, "numberOfCells", CombLenght.ToString());
				}
			}
			else if (CombLenght > 0)
			{
				XmlNode xmlNode3 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "comb", "");
				SetNewAttribute(xmlNode3, "numberOfCells", CombLenght.ToString());
				currentNode["ui"]["numericEdit"].AppendChild(xmlNode3);
			}
		}
		if (base.Border != null)
		{
			if (currentNode["ui"]["numericEdit"]["border"] != null)
			{
				base.Border.Save(currentNode["ui"]["numericEdit"]["border"]);
				return;
			}
			if (currentNode["border"] != null)
			{
				base.Border.Save(currentNode["border"]);
				return;
			}
			XmlNode xmlNode4 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "border", "");
			base.Border.Save(xmlNode4);
			currentNode["ui"]["numericEdit"].AppendChild(xmlNode4);
		}
	}

	internal void Fill(XmlWriter dataSetWriter, PdfLoadedXfaForm form)
	{
		if (form.acroForm.GetField(nodeName) is PdfLoadedTextBoxField pdfLoadedTextBoxField && !double.IsNaN(NumericValue))
		{
			pdfLoadedTextBoxField.Text = NumericValue.ToString();
		}
		if (!string.IsNullOrEmpty(base.Name))
		{
			dataSetWriter.WriteStartElement(base.Name);
			if (!double.IsNaN(NumericValue))
			{
				dataSetWriter.WriteString(NumericValue.ToString());
			}
			Save();
			dataSetWriter.WriteEndElement();
		}
	}
}
