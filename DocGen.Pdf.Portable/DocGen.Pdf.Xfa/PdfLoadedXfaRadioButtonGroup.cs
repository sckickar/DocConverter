using System.Collections.Generic;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Xfa;

public class PdfLoadedXfaRadioButtonGroup : PdfLoadedXfaField
{
	private new List<PdfLoadedXfaRadioButtonField> m_fields = new List<PdfLoadedXfaRadioButtonField>();

	internal int m_selectedItemIndex = -1;

	internal string vText;

	private float m_width;

	private float m_height;

	private PointF m_location;

	private PdfXfaVisibility m_visibility;

	private bool m_readOnly;

	internal PdfLoadedXfaFlowDirection m_flowDirection;

	internal SizeF Size = SizeF.Empty;

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

	public new PdfXfaVisibility Visibility
	{
		get
		{
			return m_visibility;
		}
		set
		{
			m_visibility = value;
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

	public PdfLoadedXfaRadioButtonField[] Fields => m_fields.ToArray();

	internal void Add(PdfLoadedXfaRadioButtonField field)
	{
		m_fields.Add(field);
	}

	internal void ReadField(XmlNode node, XmlDocument dataSetDoc)
	{
		currentNode = node;
		if (!(node.Name == "exclGroup"))
		{
			return;
		}
		if (node.Attributes["name"] != null)
		{
			base.Name = node.Attributes["name"].Value;
		}
		if (node.Attributes["w"] != null)
		{
			Width = ConvertToPoint(node.Attributes["w"].Value);
		}
		if (node.Attributes["h"] != null)
		{
			Height = ConvertToPoint(node.Attributes["h"].Value);
		}
		if (node.Attributes["x"] != null)
		{
			Location = new PointF(ConvertToPoint(node.Attributes["x"].Value), Location.Y);
		}
		if (node.Attributes["y"] != null)
		{
			Location = new PointF(Location.X, ConvertToPoint(node.Attributes["y"].Value));
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
				XmlNode xmlNode = xmlNodeList[sameNameFieldsCount];
				if (xmlNode != null && xmlNode.FirstChild != null)
				{
					vText = xmlNode.FirstChild.InnerText;
				}
			}
		}
		foreach (XmlNode item in node)
		{
			if (item.Name == "field")
			{
				PdfLoadedXfaRadioButtonField pdfLoadedXfaRadioButtonField = new PdfLoadedXfaRadioButtonField();
				pdfLoadedXfaRadioButtonField.parent = this;
				pdfLoadedXfaRadioButtonField.ReadField(item);
				m_fields.Add(pdfLoadedXfaRadioButtonField);
				if (vText != null && pdfLoadedXfaRadioButtonField.vText == vText)
				{
					pdfLoadedXfaRadioButtonField.IsChecked = true;
				}
				else if (vText != null && vText == pdfLoadedXfaRadioButtonField.iText)
				{
					pdfLoadedXfaRadioButtonField.IsChecked = true;
				}
				else if (vText == null && pdfLoadedXfaRadioButtonField.vText == pdfLoadedXfaRadioButtonField.iText)
				{
					pdfLoadedXfaRadioButtonField.IsChecked = true;
				}
			}
		}
		if (node.Attributes["layout"] != null)
		{
			switch (node.Attributes["layout"].Value)
			{
			case "tb":
				m_flowDirection = PdfLoadedXfaFlowDirection.TopToBottom;
				break;
			case "lr-tb":
				m_flowDirection = PdfLoadedXfaFlowDirection.LeftToRight;
				break;
			case "rl-tb":
				m_flowDirection = PdfLoadedXfaFlowDirection.RightToLeft;
				break;
			case "row":
				m_flowDirection = PdfLoadedXfaFlowDirection.Row;
				break;
			case "table":
				m_flowDirection = PdfLoadedXfaFlowDirection.Table;
				break;
			default:
				m_flowDirection = PdfLoadedXfaFlowDirection.None;
				break;
			}
		}
	}

	internal SizeF GetSize()
	{
		SizeF result = SizeF.Empty;
		if (m_flowDirection == PdfLoadedXfaFlowDirection.None)
		{
			if (Width > 0f && Height > 0f)
			{
				result = new SizeF(Width, Height);
			}
			else
			{
				float num = 0f;
				float num2 = 0f;
				PdfLoadedXfaRadioButtonField[] fields = Fields;
				foreach (PdfLoadedXfaRadioButtonField pdfLoadedXfaRadioButtonField in fields)
				{
					if (num < pdfLoadedXfaRadioButtonField.Location.X + pdfLoadedXfaRadioButtonField.Width)
					{
						num = pdfLoadedXfaRadioButtonField.Location.X + pdfLoadedXfaRadioButtonField.Width;
					}
					if (num2 < pdfLoadedXfaRadioButtonField.Location.Y + pdfLoadedXfaRadioButtonField.Height)
					{
						num2 = pdfLoadedXfaRadioButtonField.Location.Y + pdfLoadedXfaRadioButtonField.Height;
					}
				}
				result = new SizeF(num + base.Margins.Right, num2 + base.Margins.Bottom);
			}
		}
		else if (m_flowDirection == PdfLoadedXfaFlowDirection.TopToBottom)
		{
			float num3 = 0f;
			result.Width = Width;
			PdfLoadedXfaRadioButtonField[] fields = Fields;
			foreach (PdfLoadedXfaRadioButtonField pdfLoadedXfaRadioButtonField2 in fields)
			{
				result.Height += pdfLoadedXfaRadioButtonField2.Height;
				if (num3 < pdfLoadedXfaRadioButtonField2.Width)
				{
					num3 = pdfLoadedXfaRadioButtonField2.Width;
				}
			}
			if (Width == 0f)
			{
				result.Width = num3;
			}
		}
		else if (m_flowDirection == PdfLoadedXfaFlowDirection.LeftToRight || m_flowDirection == PdfLoadedXfaFlowDirection.RightToLeft)
		{
			float num4 = 0f;
			result.Width = Width;
			PointF pointF = new PointF(base.Margins.Left, base.Margins.Top);
			PdfLoadedXfaRadioButtonField[] fields = Fields;
			foreach (PdfLoadedXfaRadioButtonField pdfLoadedXfaRadioButtonField3 in fields)
			{
				if (Width == 0f)
				{
					result.Width += pdfLoadedXfaRadioButtonField3.Width;
				}
				else if (pointF.X + pdfLoadedXfaRadioButtonField3.Width > Width - base.Margins.Right)
				{
					pointF.X = base.Margins.Left;
					result.Height += num4;
					num4 = 0f;
				}
				pointF.X += pdfLoadedXfaRadioButtonField3.Width;
				if (num4 < pdfLoadedXfaRadioButtonField3.Height)
				{
					num4 = pdfLoadedXfaRadioButtonField3.Height;
				}
			}
			result.Height += num4;
		}
		Width = result.Width;
		Height = result.Height;
		return result;
	}

	internal void DrawRadiButtonGroup(PdfGraphics graphics, RectangleF bounds)
	{
		bool flag = false;
		if (m_flowDirection == PdfLoadedXfaFlowDirection.None)
		{
			PdfLoadedXfaRadioButtonField[] fields = Fields;
			foreach (PdfLoadedXfaRadioButtonField pdfLoadedXfaRadioButtonField in fields)
			{
				if (pdfLoadedXfaRadioButtonField.Visibility != PdfXfaVisibility.Hidden && pdfLoadedXfaRadioButtonField.Visibility != PdfXfaVisibility.Invisible)
				{
					if (flag)
					{
						pdfLoadedXfaRadioButtonField.IsChecked = false;
					}
					RectangleF bounds2 = new RectangleF(new PointF(pdfLoadedXfaRadioButtonField.Location.X + bounds.Location.X, pdfLoadedXfaRadioButtonField.Location.Y + bounds.Location.Y), new SizeF(pdfLoadedXfaRadioButtonField.Width, pdfLoadedXfaRadioButtonField.Height));
					pdfLoadedXfaRadioButtonField.DrawRadioButton(graphics, bounds2);
					if (pdfLoadedXfaRadioButtonField.IsChecked)
					{
						flag = true;
					}
				}
			}
		}
		else if (m_flowDirection == PdfLoadedXfaFlowDirection.TopToBottom)
		{
			PointF location = new PointF(bounds.Location.X, bounds.Location.Y);
			PdfLoadedXfaRadioButtonField[] fields = Fields;
			foreach (PdfLoadedXfaRadioButtonField pdfLoadedXfaRadioButtonField2 in fields)
			{
				if (pdfLoadedXfaRadioButtonField2.Visibility != PdfXfaVisibility.Hidden)
				{
					if (flag)
					{
						pdfLoadedXfaRadioButtonField2.IsChecked = false;
					}
					RectangleF bounds3 = new RectangleF(location, new SizeF(pdfLoadedXfaRadioButtonField2.Width, pdfLoadedXfaRadioButtonField2.Height));
					if (pdfLoadedXfaRadioButtonField2.Visibility != PdfXfaVisibility.Invisible)
					{
						pdfLoadedXfaRadioButtonField2.DrawRadioButton(graphics, bounds3);
					}
					location.Y += pdfLoadedXfaRadioButtonField2.Height;
					if (pdfLoadedXfaRadioButtonField2.IsChecked)
					{
						flag = true;
					}
				}
			}
		}
		else
		{
			if (m_flowDirection != PdfLoadedXfaFlowDirection.LeftToRight && m_flowDirection != PdfLoadedXfaFlowDirection.RightToLeft)
			{
				return;
			}
			PointF location2 = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
			PointF empty = PointF.Empty;
			float num = 0f;
			PdfLoadedXfaRadioButtonField[] fields = Fields;
			foreach (PdfLoadedXfaRadioButtonField pdfLoadedXfaRadioButtonField3 in fields)
			{
				if (pdfLoadedXfaRadioButtonField3.Visibility != PdfXfaVisibility.Hidden)
				{
					if (empty.X + pdfLoadedXfaRadioButtonField3.Width > Width - base.Margins.Right)
					{
						location2.X = bounds.Location.X + base.Margins.Left;
						empty.X = base.Margins.Left;
						empty.Y += num;
						location2.Y += num;
						num = 0f;
					}
					if (flag)
					{
						pdfLoadedXfaRadioButtonField3.IsChecked = false;
					}
					RectangleF bounds4 = new RectangleF(location2, new SizeF(pdfLoadedXfaRadioButtonField3.Width, pdfLoadedXfaRadioButtonField3.Height));
					if (pdfLoadedXfaRadioButtonField3.Visibility != PdfXfaVisibility.Invisible)
					{
						pdfLoadedXfaRadioButtonField3.DrawRadioButton(graphics, bounds4);
					}
					location2.X += pdfLoadedXfaRadioButtonField3.Width;
					empty.X += pdfLoadedXfaRadioButtonField3.Width;
					if (num < pdfLoadedXfaRadioButtonField3.Height)
					{
						num = pdfLoadedXfaRadioButtonField3.Height;
					}
					if (pdfLoadedXfaRadioButtonField3.IsChecked)
					{
						flag = true;
					}
				}
			}
		}
	}

	internal void SetIndex()
	{
		if (m_selectedItemIndex != -1)
		{
			return;
		}
		for (int i = 0; i < m_fields.Count; i++)
		{
			if (m_fields[i].IsChecked)
			{
				m_selectedItemIndex = i;
			}
		}
	}

	internal void ResetSelection()
	{
		for (int i = 0; i < m_fields.Count; i++)
		{
			m_fields[i].m_isChecked = false;
		}
	}

	internal void Save()
	{
		for (int i = 0; i < Fields.Length; i++)
		{
			Fields[i].Save();
		}
	}

	internal void Fill(XmlWriter dataSetWriter, PdfLoadedXfaForm form)
	{
		PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = form.acroForm.GetField(nodeName) as PdfLoadedRadioButtonListField;
		Save();
		SetIndex();
		if (pdfLoadedRadioButtonListField != null && m_selectedItemIndex != -1)
		{
			pdfLoadedRadioButtonListField.SelectedIndex = m_selectedItemIndex;
		}
		if (base.Name != string.Empty)
		{
			dataSetWriter.WriteStartElement(base.Name);
		}
		if (m_selectedItemIndex >= 0)
		{
			if (!string.IsNullOrEmpty(Fields[m_selectedItemIndex].iText) && base.Name != string.Empty)
			{
				dataSetWriter.WriteString(Fields[m_selectedItemIndex].iText);
			}
			else if (!string.IsNullOrEmpty(Fields[m_selectedItemIndex].vText) && base.Name != string.Empty)
			{
				dataSetWriter.WriteString(Fields[m_selectedItemIndex].vText);
			}
		}
		else if (!string.IsNullOrEmpty(vText) && base.Name != string.Empty)
		{
			dataSetWriter.WriteString(vText);
		}
		if (base.Name != string.Empty)
		{
			dataSetWriter.WriteEndElement();
		}
	}
}
