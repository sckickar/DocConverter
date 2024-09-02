using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfListField : PdfAppearanceField
{
	private PdfListFieldItemCollection m_items;

	private int m_selectedIndex = -1;

	private int[] m_selectedIndexes = new int[0];

	public PdfListFieldItemCollection Items
	{
		get
		{
			if (m_items == null)
			{
				m_items = new PdfListFieldItemCollection(this);
				base.Dictionary.SetProperty("Opt", m_items);
			}
			return m_items;
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
			if (value < 0 || value >= Items.Count)
			{
				throw new ArgumentOutOfRangeException("SelectedIndex");
			}
			if (m_selectedIndex != value)
			{
				m_selectedIndex = value;
				base.Dictionary.SetProperty("I", new PdfArray(new int[1] { m_selectedIndex }));
			}
			NotifyPropertyChanged("SelectedIndex");
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
			for (int i = 0; i < m_items.Count; i++)
			{
				if (value == m_items[i].Text)
				{
					m_selectedIndex = i;
					break;
				}
			}
			PdfListFieldItem pdfListFieldItem = m_items[m_selectedIndex];
			if (pdfListFieldItem.Value != value)
			{
				pdfListFieldItem.Value = value;
			}
			base.Dictionary.SetProperty("I", new PdfArray(new int[1] { m_selectedIndex }));
			NotifyPropertyChanged("SelectedValue");
		}
	}

	public PdfListFieldItem SelectedItem => m_items[m_selectedIndex];

	internal int[] SelectedIndexes
	{
		get
		{
			return m_selectedIndexes;
		}
		set
		{
			foreach (int num in value)
			{
				if (num < 0 || num >= Items.Count)
				{
					throw new ArgumentOutOfRangeException("SelectedIndex");
				}
			}
			m_selectedIndexes = value;
			if (m_selectedIndexes.Length > 1)
			{
				base.Dictionary.SetProperty("I", new PdfArray(m_selectedIndexes));
			}
		}
	}

	public PdfListField(PdfPageBase page, string name)
		: base(page, name)
	{
	}

	internal PdfListField()
	{
	}

	internal override void Draw()
	{
		base.Draw();
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("FT", new PdfName("Ch"));
	}
}
