using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

public class PdfXfaComboBoxField : PdfXfaStyledField
{
	private bool m_allowTextEntry;

	private List<string> m_items = new List<string>();

	private int m_selectedIndex = -1;

	private string m_selectedValue = string.Empty;

	private PdfXfaCaption m_caption = new PdfXfaCaption();

	internal PdfPage m_page;

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

	public bool AllowTextEntry
	{
		get
		{
			return m_allowTextEntry;
		}
		set
		{
			m_allowTextEntry = value;
		}
	}

	public PdfXfaComboBoxField(string name, SizeF size)
	{
		base.Height = size.Height;
		base.Width = size.Width;
		base.Name = name;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaComboBoxField(string name, SizeF size, List<string> items)
	{
		base.Height = size.Height;
		base.Width = size.Width;
		base.Name = name;
		Items = items;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaComboBoxField(string name, float width, float height)
	{
		base.Height = height;
		base.Width = width;
		base.Name = name;
		Padding.Left = 3f;
		Padding.Right = 3f;
	}

	public PdfXfaComboBoxField(string name, float width, float height, List<string> items)
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
			base.Name = "comboBox" + xfaWriter.m_fieldCount;
		}
		xfaWriter.Write.WriteStartElement("field");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		SetSize(xfaWriter);
		xfaWriter.SetRPR(base.Rotate, base.Visibility, base.ReadOnly);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("open", "onEntry");
		if (AllowTextEntry)
		{
			dictionary.Add("textEntry", "1");
		}
		xfaWriter.WriteUI("choiceList", dictionary, base.Border, 0, Padding);
		xfaWriter.WriteListItems(Items, "1");
		if (m_selectedValue != null && m_selectedValue != string.Empty)
		{
			if (Items.Contains(m_selectedValue))
			{
				xfaWriter.WriteValue(m_selectedValue, 0);
			}
			else if (!AllowTextEntry)
			{
				throw new PdfException("The selected value doesn't match.");
			}
		}
		else if (m_selectedIndex > 0)
		{
			if (m_selectedIndex > Items.Count)
			{
				throw new ArgumentOutOfRangeException("SelectedIndex");
			}
			xfaWriter.WriteValue(Items[m_selectedIndex], 0);
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
		PdfComboBoxField pdfComboBoxField = new PdfComboBoxField(page, name);
		pdfComboBoxField.TextAlignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		if (base.Font == null)
		{
			pdfComboBoxField.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Regular);
		}
		else
		{
			pdfComboBoxField.Font = base.Font;
		}
		if (base.ReadOnly || parent.ReadOnly || parent.m_isReadOnly)
		{
			pdfComboBoxField.ReadOnly = true;
		}
		if (base.Border != null)
		{
			base.Border.ApplyAcroBorder(pdfComboBoxField);
		}
		if (base.ForeColor != PdfColor.Empty)
		{
			pdfComboBoxField.ForeColor = base.ForeColor;
		}
		if (base.Visibility == PdfXfaVisibility.Invisible)
		{
			pdfComboBoxField.Visibility = PdfFormFieldVisibility.Hidden;
		}
		foreach (string item in Items)
		{
			pdfComboBoxField.Items.Add(new PdfListFieldItem(item, item));
		}
		if (SelectedIndex != -1)
		{
			pdfComboBoxField.SelectedIndex = SelectedIndex;
		}
		if (SelectedValue != string.Empty && SelectedValue != null)
		{
			pdfComboBoxField.SelectedIndex = Items.IndexOf(SelectedValue);
		}
		RectangleF bounds2 = default(RectangleF);
		SizeF size = GetSize();
		bounds2.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
		bounds2.Size = new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom));
		if (base.Visibility != PdfXfaVisibility.Invisible)
		{
			Caption.DrawText(page, bounds2, GetRotationAngle());
		}
		pdfComboBoxField.Bounds = GetBounds(bounds2, base.Rotate, Caption);
		pdfComboBoxField.Widget.WidgetAppearance.RotationAngle = GetRotationAngle();
		return pdfComboBoxField;
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
		PdfXfaComboBoxField obj = (PdfXfaComboBoxField)MemberwiseClone();
		obj.Caption = Caption.Clone() as PdfXfaCaption;
		return obj;
	}
}
