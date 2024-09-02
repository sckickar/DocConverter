using System.Collections.Generic;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaComboBoxField : PdfLoadedXfaStyledField
{
	private List<string> m_items = new List<string>();

	private int m_selectedIndex = -1;

	private string m_selectedValue = string.Empty;

	private List<string> m_hiddenItems = new List<string>();

	private PdfMargins m_innerMargin;

	public List<string> Items
	{
		get
		{
			return m_items;
		}
		set
		{
			if (value != null)
			{
				m_items = value;
			}
		}
	}

	public int SelectedIndex
	{
		get
		{
			return m_selectedIndex;
		}
		set
		{
			if (value >= 0 && value < m_items.Count)
			{
				m_selectedIndex = value;
				m_selectedValue = m_items[value];
			}
		}
	}

	public string SelectedValue
	{
		get
		{
			return m_selectedValue;
		}
		set
		{
			if (value != null && m_items.Contains(value))
			{
				m_selectedValue = value;
				return;
			}
			if (m_hiddenItems.Contains(value))
			{
				int num = m_hiddenItems.IndexOf(value);
				if (num < m_items.Count)
				{
					m_selectedValue = m_items[num];
					m_selectedIndex = m_items.IndexOf(m_selectedValue);
				}
				return;
			}
			throw new PdfException("The Value doesn't exists");
		}
	}

	public List<string> HiddenItems => m_hiddenItems;

	internal void ReadField(XmlNode node, XmlDocument dataSetDoc)
	{
		currentNode = node;
		if (!(node.Name == "field"))
		{
			return;
		}
		ReadCommonProperties(node);
		if (node["value"] != null && node["value"]["text"] != null)
		{
			m_selectedValue = node["value"]["text"].InnerText;
		}
		if (node["items"] != null)
		{
			foreach (XmlNode item in node["items"])
			{
				if (item.Name == "text")
				{
					m_items.Add(item.InnerText);
				}
			}
			if (node["items"] != null)
			{
				foreach (XmlNode item2 in node.LastChild)
				{
					if (item2.Name == "text")
					{
						m_hiddenItems.Add(item2.InnerText);
					}
				}
			}
		}
		if (node["ui"]["choiceList"]["border"] != null)
		{
			ReadBorder(node["ui"]["choiceList"]["border"], complete: false);
		}
		if (node["ui"]["choiceList"]["margin"] != null)
		{
			m_innerMargin = new PdfMargins();
			ReadMargin(node["ui"]["choiceList"]["margin"], m_innerMargin);
		}
		nodeName = parent.nodeName;
		string empty = string.Empty;
		string text = string.Empty;
		if (parent.isBindingMatchNone)
		{
			text = nodeName;
			int startIndex = nodeName.LastIndexOf('.');
			text = text.Remove(startIndex);
		}
		else if (base.Name == string.Empty)
		{
			text = nodeName;
		}
		empty = ((!(text != string.Empty)) ? base.Name : (text + "." + base.Name));
		if (dataSetDoc != null)
		{
			string[] array = empty.Split('[');
			string text2 = string.Empty;
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				if (text3.Contains("]"))
				{
					int num = text3.IndexOf(']') + 2;
					if (text3.Length > num)
					{
						text2 = text2 + "/" + text3.Substring(num);
					}
				}
				else
				{
					text2 += text3;
				}
			}
			text2 = "//" + text2;
			while (text2.Contains("#"))
			{
				int num2 = text2.IndexOf("#");
				if (num2 != -1)
				{
					string text4 = text2.Substring(0, num2 - 1);
					string text5 = text2.Substring(num2);
					int num3 = text5.IndexOf("/");
					string text6 = string.Empty;
					if (num3 != -1)
					{
						text6 = text5.Substring(num3);
					}
					text2 = text4 + text6;
					text2 = text2.TrimEnd(new char[1] { '/' });
				}
			}
			XmlNodeList xmlNodeList = dataSetDoc.SelectNodes(text2);
			int sameNameFieldsCount = parent.GetSameNameFieldsCount(base.Name);
			if (xmlNodeList != null && xmlNodeList.Count > sameNameFieldsCount)
			{
				XmlNode xmlNode3 = xmlNodeList[sameNameFieldsCount];
				if (xmlNode3 != null)
				{
					if (xmlNode3.FirstChild != null)
					{
						m_selectedValue = xmlNode3.FirstChild.InnerText;
					}
					if (m_items.Count > 0 && !m_items.Contains(m_selectedValue) && m_hiddenItems.Contains(m_selectedValue))
					{
						m_selectedValue = m_items[m_hiddenItems.IndexOf(m_selectedValue)];
					}
				}
			}
		}
		if (string.IsNullOrEmpty(m_selectedValue))
		{
			return;
		}
		if (m_items.Contains(m_selectedValue))
		{
			m_selectedIndex = m_items.IndexOf(m_selectedValue);
			return;
		}
		foreach (string item3 in m_items)
		{
			char obj = m_selectedValue.ToUpper()[0];
			string text7 = item3;
			for (int i = 0; i < text7.Length; i++)
			{
				if (text7[i].Equals(obj) && m_selectedValue.Length < item3.Length)
				{
					m_selectedValue = item3;
					break;
				}
			}
		}
	}

	internal new void Save()
	{
		base.Save();
		XmlElement xmlElement = currentNode["items"];
		if (xmlElement != null)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				if (xmlElement.ChildNodes.Count > i)
				{
					XmlNode xmlNode = xmlElement.ChildNodes[i];
					if (xmlNode.InnerText != Items[i])
					{
						xmlNode.InnerText = Items[i];
					}
				}
				else
				{
					XmlNode xmlNode2 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
					xmlNode2.InnerText = Items[i];
					xmlElement.AppendChild(xmlNode2);
				}
			}
			if (xmlElement.ChildNodes.Count > Items.Count)
			{
				int count = xmlElement.ChildNodes.Count;
				for (int j = 0; j < count; j++)
				{
					if (j >= Items.Count)
					{
						xmlElement.RemoveChild(xmlElement.ChildNodes[Items.Count]);
					}
				}
			}
		}
		else if (xmlElement == null && Items.Count > 0)
		{
			XmlNode newChild = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "items", "");
			currentNode.AppendChild(newChild);
		}
		if (currentNode["items"] != null && Items.Count > 0)
		{
			XmlElement xmlElement2 = currentNode["items"];
			for (int k = 0; k < Items.Count; k++)
			{
				if (xmlElement2.ChildNodes.Count > k)
				{
					XmlNode xmlNode3 = xmlElement2.ChildNodes[k];
					if (xmlNode3.InnerText != Items[k])
					{
						xmlNode3.InnerText = Items[k];
					}
				}
				else
				{
					XmlNode xmlNode4 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
					xmlNode4.InnerText = Items[k];
					xmlElement2.AppendChild(xmlNode4);
				}
			}
		}
		if (SelectedValue != null)
		{
			if (currentNode["value"] != null)
			{
				if (currentNode["value"]["text"] != null)
				{
					currentNode["value"]["text"].InnerText = SelectedValue;
				}
				else
				{
					XmlNode xmlNode5 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
					xmlNode5.InnerText = SelectedValue;
					currentNode["value"].AppendChild(xmlNode5);
				}
			}
			else
			{
				XmlNode xmlNode6 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "value", "");
				XmlNode xmlNode7 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
				xmlNode7.InnerText = SelectedValue;
				xmlNode6.AppendChild(xmlNode7);
				currentNode.AppendChild(xmlNode6);
			}
		}
		if (base.Border != null)
		{
			if (currentNode["ui"]["choiceList"]["border"] != null)
			{
				base.Border.Save(currentNode["ui"]["choiceList"]["border"]);
				return;
			}
			if (currentNode["border"] != null)
			{
				base.Border.Save(currentNode["border"]);
				return;
			}
			XmlNode xmlNode8 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "border", "");
			base.Border.Save(xmlNode8);
			currentNode["ui"]["choiceList"].AppendChild(xmlNode8);
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
			if (base.Caption.Font != null && bounds2.Height < base.Caption.Font.Height && bounds2.Height + base.Margins.Bottom + base.Margins.Top > base.Caption.Font.Height)
			{
				bounds2.Height = base.Caption.Font.Height;
			}
			base.Caption.DrawText(graphics, bounds2, GetRotationAngle());
		}
		RectangleF bounds3 = GetBounds(bounds2, base.Rotate, base.Caption);
		if (base.Visibility != PdfXfaVisibility.Invisible && base.Visibility != PdfXfaVisibility.Hidden)
		{
			if (base.Border != null)
			{
				base.Border.DrawBorder(graphics, bounds3);
			}
			if (base.ForeColor.IsEmpty)
			{
				base.ForeColor = Color.Black;
			}
			if (m_selectedValue != string.Empty)
			{
				graphics.Save();
				bounds3 = GetBounds(bounds3);
				graphics.TranslateTransform(bounds3.X, bounds3.Y);
				graphics.RotateTransform(-GetRotationAngle());
				RectangleF renderingRect = GetRenderingRect(bounds3);
				graphics.DrawString(m_selectedValue, base.Font, new PdfSolidBrush(base.ForeColor), renderingRect, pdfStringFormat);
				graphics.Restore();
			}
		}
	}

	private RectangleF GetBounds(RectangleF bounds1)
	{
		if (m_innerMargin != null)
		{
			bounds1.X += m_innerMargin.Left;
			bounds1.Y += m_innerMargin.Top;
			bounds1.Width -= m_innerMargin.Left + m_innerMargin.Right;
			bounds1.Height -= m_innerMargin.Top + m_innerMargin.Bottom;
		}
		return bounds1;
	}

	internal void Fill(XmlWriter dataSetWriter, PdfLoadedXfaForm form)
	{
		PdfLoadedComboBoxField pdfLoadedComboBoxField = form.acroForm.GetField(nodeName) as PdfLoadedComboBoxField;
		if (base.Name != string.Empty)
		{
			dataSetWriter.WriteStartElement(base.Name);
		}
		if (!string.IsNullOrEmpty(SelectedValue) && base.Name != string.Empty)
		{
			dataSetWriter.WriteString(SelectedValue);
			if (pdfLoadedComboBoxField != null && pdfLoadedComboBoxField.Items.Count > 0)
			{
				pdfLoadedComboBoxField.SelectedValue = SelectedValue;
			}
		}
		else if (SelectedIndex != -1 && base.Name != string.Empty)
		{
			dataSetWriter.WriteString(Items[SelectedIndex]);
			if (pdfLoadedComboBoxField != null && pdfLoadedComboBoxField.Items.Count > 0 && pdfLoadedComboBoxField.Items.Count > SelectedIndex)
			{
				pdfLoadedComboBoxField.SelectedIndex = SelectedIndex;
			}
		}
		if (base.Name != string.Empty)
		{
			dataSetWriter.WriteEndElement();
		}
		Save();
	}
}
