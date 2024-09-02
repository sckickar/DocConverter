using System;
using System.Globalization;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaDateTimeField : PdfLoadedXfaStyledField
{
	internal DateTime? m_value;

	internal string fieldName = string.Empty;

	private PdfXfaDateTimeFormat m_format;

	internal string patternString = string.Empty;

	internal bool m_isSet;

	private string m_pattern = string.Empty;

	internal string m_bindedvalue = string.Empty;

	private PdfMargins m_innerMargin;

	private bool isSkipDefaultMargin;

	public DateTime Value
	{
		get
		{
			return m_value ?? DateTime.MinValue;
		}
		set
		{
			m_value = value;
			m_isSet = true;
		}
	}

	public PdfXfaDateTimeFormat Format
	{
		get
		{
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	public string Pattern
	{
		get
		{
			if (string.IsNullOrEmpty(m_pattern) && !string.IsNullOrEmpty(patternString))
			{
				GetFieldPattern();
			}
			return m_pattern;
		}
		set
		{
			m_pattern = value;
		}
	}

	internal void SetDate(DateTime? value)
	{
		m_value = value;
		m_isSet = true;
	}

	private void GetFieldPattern()
	{
		XfaWriter xfaWriter = new XfaWriter();
		string pattern = TrimDatePattern(patternString);
		string pattern2 = TrimTimePattern(patternString);
		switch (Format)
		{
		case PdfXfaDateTimeFormat.Date:
			m_pattern = xfaWriter.GetDatePattern(pattern);
			break;
		case PdfXfaDateTimeFormat.Time:
			m_pattern = xfaWriter.GetTimePattern(pattern2);
			break;
		case PdfXfaDateTimeFormat.DateTime:
			m_pattern = xfaWriter.GetDatePattern(pattern) + " " + xfaWriter.GetTimePattern(pattern2);
			break;
		}
	}

	public void ClearValue()
	{
		m_value = null;
		m_isSet = true;
	}

	internal void Read(XmlNode node, XmlDocument dataSetDoc)
	{
		currentNode = node;
		if (node.Name == "field")
		{
			ReadCommonProperties(currentNode);
			if (node["value"] != null)
			{
				if (node["value"]["date"] != null)
				{
					Format = PdfXfaDateTimeFormat.Date;
					string innerText = node["value"]["date"].InnerText;
					if (!string.IsNullOrEmpty(innerText))
					{
						DateTime? dateTime = ParseDate(innerText);
						if (dateTime.HasValue)
						{
							Value = dateTime.Value;
						}
					}
				}
				else if (node["value"]["dateTime"] != null)
				{
					Format = PdfXfaDateTimeFormat.DateTime;
					string innerText2 = node["value"]["dateTime"].InnerText;
					if (!string.IsNullOrEmpty(innerText2))
					{
						DateTime? dateTime2 = ParseDate(innerText2);
						if (dateTime2.HasValue)
						{
							Value = dateTime2.Value;
						}
					}
				}
				else if (node["value"]["time"] != null)
				{
					Format = PdfXfaDateTimeFormat.Time;
					string innerText3 = node["value"]["time"].InnerText;
					if (!string.IsNullOrEmpty(innerText3))
					{
						DateTime? dateTime3 = ParseDate(innerText3);
						if (dateTime3.HasValue)
						{
							Value = dateTime3.Value;
						}
					}
				}
			}
		}
		if (node["format"] != null && node["format"]["picture"] != null)
		{
			patternString = node["format"]["picture"].InnerText;
		}
		if (node["ui"]["dateTimeEdit"]["border"] != null)
		{
			ReadBorder(node["ui"]["dateTimeEdit"]["border"], complete: false);
		}
		if (node["ui"]["dateTimeEdit"]["margin"] != null)
		{
			m_innerMargin = new PdfMargins();
			ReadMargin(node["ui"]["dateTimeEdit"]["margin"], m_innerMargin);
		}
		if (patternString == string.Empty && patternString == "" && node["ui"]["dateTimeEdit"]["picture"] != null)
		{
			patternString = node["ui"]["dateTimeEdit"]["picture"].InnerText;
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
							try
							{
								DateTimeFormatInfo dateTimeFormatInfo = new DateTimeFormatInfo();
								dateTimeFormatInfo.ShortDatePattern = GetPattern(Pattern);
								DateTime result = default(DateTime);
								if (DateTime.TryParse(xmlNode2.FirstChild.InnerText, dateTimeFormatInfo, DateTimeStyles.None, out result))
								{
									Value = Convert.ToDateTime(xmlNode2.FirstChild.InnerText, dateTimeFormatInfo);
								}
								else
								{
									Value = result;
								}
							}
							catch (Exception)
							{
								m_bindedvalue = xmlNode2.FirstChild.InnerText;
							}
						}
					}
					else if (parent.bindingName != null && !flag)
					{
						text2 = parent.nodeName.Split(separator)[0];
						text2 = text2.Replace("[0]", "");
						text = parent.bindingName + "." + base.Name;
						text = text.Replace(".", "/");
						text = text.Replace("$.", "");
						text2 = text2 + "/" + text;
						text2 = "//" + text2;
						xmlNode2 = dataSetDoc.SelectSingleNode(text2);
						if (xmlNode2 != null && xmlNode2.FirstChild != null)
						{
							try
							{
								DateTimeFormatInfo dateTimeFormatInfo2 = new DateTimeFormatInfo();
								dateTimeFormatInfo2.ShortDatePattern = GetPattern(Pattern);
								DateTime result2 = default(DateTime);
								if (DateTime.TryParse(xmlNode2.FirstChild.InnerText, dateTimeFormatInfo2, DateTimeStyles.None, out result2))
								{
									Value = Convert.ToDateTime(xmlNode2.FirstChild.InnerText, dateTimeFormatInfo2);
								}
								else
								{
									Value = result2;
								}
							}
							catch (Exception)
							{
								m_bindedvalue = xmlNode2.FirstChild.InnerText;
							}
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
		if (xmlNodeList == null || xmlNodeList.Count <= sameNameFieldsCount)
		{
			return;
		}
		XmlNode xmlNode3 = xmlNodeList[sameNameFieldsCount];
		if (xmlNode3 != null && xmlNode3.FirstChild != null)
		{
			DateTime? dateTime4 = ParseDate(xmlNode3.FirstChild.InnerText);
			if (dateTime4.HasValue)
			{
				Value = dateTime4.Value;
			}
		}
	}

	private DateTime? ParseDate(string text)
	{
		if (text != string.Empty)
		{
			DateTime result;
			try
			{
				DateTimeFormatInfo dateTimeFormatInfo = new DateTimeFormatInfo();
				dateTimeFormatInfo.ShortDatePattern = GetPattern(Pattern);
				if (!string.IsNullOrEmpty(Pattern))
				{
					if (DateTime.TryParse(text, dateTimeFormatInfo, DateTimeStyles.None, out result))
					{
						result = Convert.ToDateTime(text, dateTimeFormatInfo);
					}
				}
				else if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					return result;
				}
			}
			catch
			{
				if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					return result;
				}
			}
			return result;
		}
		return null;
	}

	internal new void Save()
	{
		base.Save();
		string text = Format.ToString().ToLower();
		if (Format == PdfXfaDateTimeFormat.DateTime)
		{
			text = "dateTime";
		}
		if (currentNode["value"] != null)
		{
			if (currentNode["value"]["date"] != null && text != "date")
			{
				XmlNode oldChild = currentNode["value"]["date"];
				currentNode["value"].RemoveChild(oldChild);
				setFormat(text);
			}
			if (currentNode["value"]["dateTime"] != null && text != "dateTime")
			{
				XmlNode oldChild2 = currentNode["value"]["dateTime"];
				currentNode["value"].RemoveChild(oldChild2);
				setFormat(text);
			}
			if (currentNode["value"]["time"] != null && text != "time")
			{
				XmlNode oldChild3 = currentNode["value"]["time"];
				currentNode["value"].RemoveChild(oldChild3);
				setFormat(text);
			}
		}
		else
		{
			XmlNode xmlNode = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "value", "");
			XmlNode newChild = xmlNode.OwnerDocument.CreateNode(XmlNodeType.Element, text, "");
			xmlNode.AppendChild(newChild);
			currentNode.AppendChild(xmlNode);
		}
		if (base.Border != null)
		{
			if (currentNode["ui"]["dateTimeEdit"]["border"] != null)
			{
				base.Border.Save(currentNode["ui"]["dateTimeEdit"]["border"]);
				return;
			}
			if (currentNode["border"] != null)
			{
				base.Border.Save(currentNode["border"]);
				return;
			}
			XmlNode xmlNode2 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "border", "");
			base.Border.Save(xmlNode2);
			currentNode["ui"]["dateTimeEdit"].AppendChild(xmlNode2);
		}
	}

	private void setFormat(string formatText)
	{
		XmlNode xmlNode = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, formatText, "");
		xmlNode.Value = m_value.ToString();
		currentNode["value"].AppendChild(xmlNode);
	}

	private string GetDate()
	{
		string result = string.Empty;
		XfaWriter xfaWriter = new XfaWriter();
		switch (Format)
		{
		case PdfXfaDateTimeFormat.Date:
		{
			string text2 = xfaWriter.GetDatePattern(patternString);
			if (!string.IsNullOrEmpty(Pattern))
			{
				text2 = GetPattern(Pattern);
			}
			result = m_value.Value.ToString(text2, CultureInfo.InvariantCulture);
			break;
		}
		case PdfXfaDateTimeFormat.DateTime:
			result = m_value.Value.Date.ToString();
			if (!string.IsNullOrEmpty(Pattern))
			{
				result = m_value.Value.ToString(GetPattern(Pattern), CultureInfo.InvariantCulture);
			}
			break;
		case PdfXfaDateTimeFormat.Time:
		{
			string text = xfaWriter.GetTimePattern(patternString);
			if (!string.IsNullOrEmpty(Pattern))
			{
				text = GetPattern(Pattern);
			}
			result = m_value.Value.ToString(text, CultureInfo.InvariantCulture);
			break;
		}
		}
		return result;
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
			if (bounds2.Height < base.Caption.Font.Height && bounds2.Height + (base.Margins.Top + base.Margins.Bottom) >= base.Caption.Font.Height)
			{
				bounds2.Height = base.Caption.Font.Height;
			}
			base.Caption.DrawText(graphics, bounds2, GetRotationAngle());
		}
		RectangleF bounds3 = GetBounds(bounds2, base.Rotate, base.Caption);
		if (base.ForeColor.IsEmpty)
		{
			base.ForeColor = Color.Black;
		}
		if (base.Visibility == PdfXfaVisibility.Invisible || base.Visibility == PdfXfaVisibility.Hidden)
		{
			return;
		}
		if (base.Border != null && base.Border.Visibility != PdfXfaVisibility.Hidden)
		{
			base.Border.DrawBorder(graphics, bounds3);
		}
		string text = string.Empty;
		if (m_isSet)
		{
			text = GetDate();
		}
		else if (m_bindedvalue != string.Empty)
		{
			text = m_bindedvalue;
		}
		if (!(text != string.Empty))
		{
			return;
		}
		if (bounds3.Height < base.Font.Height && bounds3.Height + (base.Margins.Top + base.Margins.Bottom) >= base.Font.Height)
		{
			bounds3.Height = base.Font.Height;
			isSkipDefaultMargin = true;
		}
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle0)
		{
			graphics.DrawString(text, base.Font, new PdfSolidBrush(base.ForeColor), isSkipDefaultMargin ? bounds3 : GetBounds(bounds3), pdfStringFormat);
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
		graphics.DrawString(text, base.Font, new PdfSolidBrush(base.ForeColor), renderingRect, pdfStringFormat);
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

	internal SizeF GetFieldSize()
	{
		float num = 0f;
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
		if (base.Height <= 0f)
		{
			float num2 = 0f;
			float num3 = 0f;
			if (currentNode.Attributes["minH"] != null)
			{
				num2 = ConvertToPoint(currentNode.Attributes["minH"].Value);
			}
			if (currentNode.Attributes["maxH"] != null)
			{
				num3 = ConvertToPoint(currentNode.Attributes["maxH"].Value);
			}
			if (num2 > 0f)
			{
				num = num2;
				if (num3 > 0f)
				{
					base.Height = num3;
				}
				else
				{
					if (num < num2)
					{
						num = num2;
					}
					if (base.Font != null && num < base.Font.Height)
					{
						num = base.Font.Height + base.Margins.Top + base.Margins.Bottom;
					}
				}
			}
			else if (num3 > 0f)
			{
				num = num3;
			}
		}
		else
		{
			num = base.Height;
		}
		if (base.Height < num)
		{
			base.Height = num;
		}
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle270 || base.Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(base.Height, base.Width);
		}
		return new SizeF(base.Width, base.Height);
	}

	internal void Fill(XmlWriter dataSetWriter, PdfLoadedXfaForm form)
	{
		PdfLoadedTextBoxField pdfLoadedTextBoxField = form.acroForm.GetField(nodeName) as PdfLoadedTextBoxField;
		string text = string.Empty;
		if (!string.IsNullOrEmpty(base.Name))
		{
			dataSetWriter.WriteStartElement(base.Name);
		}
		if (m_isSet)
		{
			if (m_value.HasValue)
			{
				XfaWriter xfaWriter = new XfaWriter();
				switch (Format)
				{
				case PdfXfaDateTimeFormat.Date:
				{
					string text3 = xfaWriter.GetDatePattern(patternString);
					if (!string.IsNullOrEmpty(Pattern))
					{
						text3 = GetPattern(Pattern);
					}
					text = m_value.Value.ToString(text3, CultureInfo.InvariantCulture);
					dataSetWriter.WriteString(text);
					break;
				}
				case PdfXfaDateTimeFormat.DateTime:
					text = m_value.Value.Date.ToString();
					if (!string.IsNullOrEmpty(Pattern))
					{
						text = m_value.Value.ToString(GetPattern(Pattern), CultureInfo.InvariantCulture);
					}
					dataSetWriter.WriteString(text);
					break;
				case PdfXfaDateTimeFormat.Time:
				{
					string text2 = xfaWriter.GetTimePattern(patternString);
					if (!string.IsNullOrEmpty(Pattern))
					{
						text2 = GetPattern(Pattern);
					}
					text = m_value.Value.ToString(text2, CultureInfo.InvariantCulture);
					dataSetWriter.WriteString(text);
					break;
				}
				}
			}
			Save();
			if (pdfLoadedTextBoxField != null)
			{
				pdfLoadedTextBoxField.Text = text;
			}
		}
		if (!string.IsNullOrEmpty(base.Name))
		{
			dataSetWriter.WriteEndElement();
		}
	}
}
