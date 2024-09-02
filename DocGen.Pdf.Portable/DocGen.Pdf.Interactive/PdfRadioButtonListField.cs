using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfRadioButtonListField : PdfField
{
	private PdfRadioButtonItemCollection m_items;

	private int m_selectedIndex = -1;

	public int SelectedIndex
	{
		get
		{
			return m_selectedIndex;
		}
		set
		{
			if (value < 0 || value >= Items.Count)
			{
				throw new ArgumentOutOfRangeException("SelectedIndex");
			}
			if (m_selectedIndex != value)
			{
				m_selectedIndex = value;
				PdfRadioButtonListItem pdfRadioButtonListItem = m_items[m_selectedIndex];
				base.Dictionary.SetName("V", pdfRadioButtonListItem.Value);
				base.Dictionary.SetName("DV", pdfRadioButtonListItem.Value);
				NotifyPropertyChanged("SelectedIndex");
			}
		}
	}

	public string SelectedValue
	{
		get
		{
			return m_items[m_selectedIndex].Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("SelectedValue");
			}
			for (int i = 0; i < Items.Count; i++)
			{
				PdfRadioButtonListItem pdfRadioButtonListItem = m_items[i];
				if (pdfRadioButtonListItem.Value == value)
				{
					m_selectedIndex = i;
					base.Dictionary.SetName("V", pdfRadioButtonListItem.Value);
					base.Dictionary.SetName("DV", pdfRadioButtonListItem.Value);
				}
			}
			NotifyPropertyChanged("SelectedValue");
		}
	}

	public PdfRadioButtonListItem SelectedItem
	{
		get
		{
			PdfRadioButtonListItem result = null;
			if (m_selectedIndex != -1)
			{
				result = m_items[m_selectedIndex];
			}
			return result;
		}
	}

	public PdfRadioButtonItemCollection Items
	{
		get
		{
			if (m_items == null)
			{
				m_items = new PdfRadioButtonItemCollection(this);
				base.Dictionary.SetProperty("Kids", m_items);
			}
			return m_items;
		}
	}

	public PdfRadioButtonListField(PdfPageBase page, string name)
		: base(page, name)
	{
		Flags |= FieldFlags.Radio;
		base.Dictionary.SetProperty("FT", new PdfName("Btn"));
	}

	internal override void Draw()
	{
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			Items[i].Draw();
		}
	}
}
