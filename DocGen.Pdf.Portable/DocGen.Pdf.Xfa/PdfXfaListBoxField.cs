using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaListBoxField : PdfXfaStyledField
{
	private int m_selectedIndex = -1;

	private string m_selectedValue = string.Empty;

	private List<string> m_items = new List<string>();

	private PdfXfaSelectionMode m_selectionMode;

	private PdfXfaCaption m_caption = new PdfXfaCaption();

	internal RectangleF currentBounds = RectangleF.Empty;

	internal new PdfXfaForm parent;

	private object m_dataSource;

	private PdfPaddings m_padding = new PdfPaddings(0f, 0f, 0f, 0f);

	public PdfPaddings Padding
	{
		get
		{
			return m_padding;
		}
		set
		{
			if (value != null)
			{
				m_padding = value;
			}
		}
	}

	public object DataSource
	{
		get
		{
			return m_dataSource;
		}
		set
		{
			if (value == null)
			{
				return;
			}
			m_dataSource = value;
			if (m_dataSource is List<string>)
			{
				Items = (List<string>)m_dataSource;
			}
			else if (m_dataSource is string[])
			{
				string[] array = m_dataSource as string[];
				for (int i = 0; i < array.Length; i++)
				{
					Items.Add(array[i]);
				}
			}
		}
	}

	public PdfXfaCaption Caption
	{
		get
		{
			return m_caption;
		}
		set
		{
			m_caption = value;
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
			if (value != null)
			{
				m_selectedValue = value;
			}
		}
	}

	public PdfXfaSelectionMode SelectionMode
	{
		get
		{
			return m_selectionMode;
		}
		set
		{
			m_selectionMode = value;
		}
	}

	public PdfXfaListBoxField(string name, SizeF size)
	{
		base.Height = size.Height;
		base.Width = size.Width;
		base.Name = name;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaListBoxField(string name, SizeF size, List<string> items)
	{
		base.Height = size.Height;
		base.Width = size.Width;
		base.Name = name;
		Items = items;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaListBoxField(string name, float width, float height)
	{
		base.Height = height;
		base.Width = width;
		base.Name = name;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaListBoxField(string name, float width, float height, List<string> items)
	{
		base.Height = height;
		base.Width = width;
		base.Name = name;
		Items = items;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "listBox" + xfaWriter.m_fieldCount;
		}
		xfaWriter.Write.WriteStartElement("field");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		SetSize(xfaWriter);
		xfaWriter.SetRPR(base.Rotate, base.Visibility, base.ReadOnly);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (SelectionMode == PdfXfaSelectionMode.Single)
		{
			dictionary.Add("open", "always");
		}
		else
		{
			dictionary.Add("open", "multiSelect");
		}
		xfaWriter.WriteUI("choiceList", dictionary, base.Border, 0, Padding);
		xfaWriter.WriteListItems(Items, "1");
		if (m_selectedValue != null)
		{
			if (Items.Contains(m_selectedValue))
			{
				xfaWriter.WriteValue(m_selectedValue, 0);
			}
		}
		else if (m_selectedIndex > 0 && m_selectedIndex - 1 <= Items.Count)
		{
			xfaWriter.WriteValue(Items[m_selectedIndex - 1], 0);
		}
		SetMFTP(xfaWriter);
		if (Caption != null)
		{
			Caption.Save(xfaWriter);
		}
		xfaWriter.Write.WriteEndElement();
	}

	internal PdfField SaveAcroForm(PdfPage page, RectangleF bounds, string name)
	{
		PdfListBoxField pdfListBoxField = new PdfListBoxField(page, name);
		pdfListBoxField.TextAlignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		if (base.Font == null)
		{
			pdfListBoxField.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		else
		{
			pdfListBoxField.Font = base.Font;
		}
		if (base.ReadOnly || parent.ReadOnly || parent.m_isReadOnly)
		{
			pdfListBoxField.ReadOnly = true;
		}
		if (base.Visibility == PdfXfaVisibility.Invisible)
		{
			pdfListBoxField.Visibility = PdfFormFieldVisibility.Hidden;
		}
		if (SelectionMode == PdfXfaSelectionMode.Multiple)
		{
			pdfListBoxField.MultiSelect = true;
		}
		foreach (string item in Items)
		{
			pdfListBoxField.Items.Add(new PdfListFieldItem(item, item));
		}
		if (SelectedIndex != -1)
		{
			pdfListBoxField.SelectedIndex = SelectedIndex;
		}
		if (SelectedValue != string.Empty && SelectedValue != null)
		{
			pdfListBoxField.SelectedIndex = Items.IndexOf(SelectedValue);
		}
		RectangleF bounds2 = default(RectangleF);
		SizeF size = GetSize();
		bounds2.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
		bounds2.Size = new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom));
		if (base.Visibility != PdfXfaVisibility.Invisible)
		{
			Caption.DrawText(page, bounds2, GetRotationAngle());
		}
		pdfListBoxField.Bounds = GetBounds(bounds2, base.Rotate, Caption);
		pdfListBoxField.Widget.WidgetAppearance.RotationAngle = GetRotationAngle();
		if (base.Border != null)
		{
			base.Border.ApplyAcroBorder(pdfListBoxField);
		}
		if (base.ForeColor != PdfColor.Empty)
		{
			pdfListBoxField.ForeColor = base.ForeColor;
		}
		return pdfListBoxField;
	}

	private void SetSize(XfaWriter xfaWriter)
	{
		if (Caption != null)
		{
			if (Caption.Font == null && base.Font != null)
			{
				Caption.Font = base.Font;
			}
			SizeF sizeF = default(SizeF);
			if (Caption.Width > 0f)
			{
				float width2 = (sizeF.Height = Caption.Width);
				sizeF.Width = width2;
			}
			else
			{
				sizeF = Caption.MeasureString();
			}
			if (Caption.Position == PdfXfaPosition.Bottom || Caption.Position == PdfXfaPosition.Top)
			{
				xfaWriter.SetSize(base.Height, base.Width, 0f, 0f);
				Caption.Width = sizeF.Height;
			}
			else if (Caption.Position == PdfXfaPosition.Left || Caption.Position == PdfXfaPosition.Right)
			{
				xfaWriter.SetSize(base.Height, base.Width, 0f, 0f);
				Caption.Width = sizeF.Width;
			}
		}
		else
		{
			xfaWriter.SetSize(base.Height, base.Width, 0f, 0f);
		}
	}

	public object Clone()
	{
		PdfXfaListBoxField obj = (PdfXfaListBoxField)MemberwiseClone();
		obj.Caption = Caption.Clone() as PdfXfaCaption;
		return obj;
	}
}
