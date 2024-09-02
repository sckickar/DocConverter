using System.Collections.Generic;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaListBoxField : PdfLoadedXfaStyledField
{
	private List<string> m_items = new List<string>();

	private int m_selectedIndex = -1;

	private string m_selectedValue = string.Empty;

	private PdfXfaSelectionMode m_selectionMode;

	private string[] m_selectedItems;

	private PdfMargins m_innerMargin;

	public string[] SelectedItems
	{
		get
		{
			return m_selectedItems;
		}
		set
		{
			if (value != null && value.Length != 0)
			{
				m_selectedItems = value;
				m_selectedValue = m_selectedItems[0];
			}
		}
	}

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
			throw new PdfException("The Value doesn't exists");
		}
	}

	public PdfXfaSelectionMode SelectionMode => m_selectionMode;

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
		}
		XmlAttributeCollection attributes = node["ui"]["choiceList"].Attributes;
		if (attributes["open"] != null && attributes["open"].Value.ToLower() == "multiselect")
		{
			m_selectionMode = PdfXfaSelectionMode.Multiple;
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
				if (xmlNode2 != null && xmlNode2.FirstChild != null)
				{
					if (SelectionMode == PdfXfaSelectionMode.Multiple)
					{
						List<string> list = new List<string>();
						foreach (XmlNode item2 in xmlNode2)
						{
							if (item2.Name == "value")
							{
								list.Add(item2.InnerText);
							}
						}
						m_selectedItems = list.ToArray();
					}
					else
					{
						m_selectedValue = xmlNode2.FirstChild.InnerText;
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(m_selectedValue) && m_items.Contains(m_selectedValue))
		{
			m_selectedIndex = m_items.IndexOf(m_selectedValue);
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
		_ = currentNode["ui"]["choiceList"].Attributes;
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
					XmlNode xmlNode3 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
					xmlNode3.InnerText = SelectedValue;
					currentNode["value"].AppendChild(xmlNode3);
				}
			}
			else
			{
				XmlNode xmlNode4 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "value", "");
				XmlNode xmlNode5 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "text", "");
				xmlNode5.InnerText = SelectedValue;
				xmlNode4.AppendChild(xmlNode5);
				currentNode.AppendChild(xmlNode4);
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
			XmlNode xmlNode6 = currentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "border", "");
			base.Border.Save(xmlNode6);
			currentNode["ui"]["choiceList"].AppendChild(xmlNode6);
		}
	}

	internal void DrawListBoxField(PdfGraphics graphics, RectangleF bounds)
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
		RectangleF empty = RectangleF.Empty;
		empty = ((base.Rotate != 0 && base.Rotate != PdfXfaRotateAngle.RotateAngle180) ? new RectangleF(0f, 0f, bounds3.Height, bounds3.Width) : new RectangleF(0f, 0f, bounds3.Width, bounds3.Height));
		PdfTemplate pdfTemplate = new PdfTemplate(empty.Size);
		if (base.Visibility == PdfXfaVisibility.Invisible || base.Visibility == PdfXfaVisibility.Hidden)
		{
			return;
		}
		if (base.Border != null)
		{
			base.Border.DrawBorder(pdfTemplate.Graphics, empty);
		}
		if (Items != null && Items.Count > 0)
		{
			empty = GetBounds(empty);
			float height = base.Font.MeasureString(Items[0]).Height;
			float num = empty.Height / height + 0.5f;
			num = ((Items.Count > 0) ? (num / (float)Items.Count) : 0f);
			float num2 = empty.Y + num;
			float num3 = 0f;
			PdfBrush brush = new PdfSolidBrush(new PdfColor(153, 193, 219));
			if (base.ForeColor.IsEmpty)
			{
				base.ForeColor = Color.Black;
			}
			foreach (string item in Items)
			{
				if (!(num3 + (height + 0.5f + num) <= empty.Height))
				{
					continue;
				}
				RectangleF rectangleF = new RectangleF(empty.X, num2, empty.Width, height + 0.5f + num);
				num2 += height + 0.5f + num;
				num3 += height + 0.5f + num;
				if (m_selectedValue != string.Empty && item == m_selectedValue)
				{
					pdfTemplate.Graphics.Save();
					pdfTemplate.Graphics.DrawRectangle(brush, rectangleF);
					pdfTemplate.Graphics.Restore();
				}
				else if (SelectionMode == PdfXfaSelectionMode.Multiple && m_selectedItems != null)
				{
					for (int i = 0; i < m_selectedItems.Length; i++)
					{
						if (m_selectedItems[i] == item)
						{
							pdfTemplate.Graphics.Save();
							pdfTemplate.Graphics.DrawRectangle(brush, rectangleF);
							pdfTemplate.Graphics.Restore();
						}
					}
				}
				pdfTemplate.Graphics.DrawString(item, base.Font, new PdfSolidBrush(base.ForeColor), rectangleF, pdfStringFormat);
			}
		}
		graphics.Save();
		graphics.TranslateTransform(bounds3.X, bounds3.Y);
		graphics.RotateTransform(-GetRotationAngle());
		graphics.DrawPdfTemplate(pdfTemplate, GetRenderingRect(bounds3).Location);
		graphics.Restore();
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
		PdfLoadedListBoxField pdfLoadedListBoxField = form.acroForm.GetField(nodeName) as PdfLoadedListBoxField;
		if (base.Name != string.Empty)
		{
			dataSetWriter.WriteStartElement(base.Name);
		}
		if (SelectionMode == PdfXfaSelectionMode.Multiple && SelectedItems != null && SelectedItems.Length != 0 && base.Name != string.Empty)
		{
			for (int i = 0; i < SelectedItems.Length; i++)
			{
				dataSetWriter.WriteStartElement("value");
				dataSetWriter.WriteString(SelectedItems[i]);
				dataSetWriter.WriteEndElement();
			}
			if (pdfLoadedListBoxField != null && pdfLoadedListBoxField.Items.Count > 0)
			{
				pdfLoadedListBoxField.SelectedValue = SelectedItems;
			}
		}
		else if (!string.IsNullOrEmpty(SelectedValue) && base.Name != string.Empty)
		{
			dataSetWriter.WriteString(SelectedValue);
			if (pdfLoadedListBoxField != null && pdfLoadedListBoxField.Items.Count > 0)
			{
				pdfLoadedListBoxField.SelectedValue = new string[1] { SelectedValue };
			}
		}
		else if (SelectedIndex != -1 && base.Name != string.Empty)
		{
			dataSetWriter.WriteString(Items[SelectedIndex]);
			if (pdfLoadedListBoxField != null && pdfLoadedListBoxField.Items.Count > 0)
			{
				pdfLoadedListBoxField.SelectedIndex = new int[1] { SelectedIndex };
			}
		}
		if (base.Name != string.Empty)
		{
			dataSetWriter.WriteEndElement();
		}
		Save();
	}
}
