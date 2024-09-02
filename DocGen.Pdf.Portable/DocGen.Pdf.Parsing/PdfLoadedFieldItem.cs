using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedFieldItem
{
	internal PdfLoadedStyledField m_field;

	internal int m_collectionIndex;

	private PdfDictionary m_dictionary;

	private PdfPageBase m_page;

	protected PdfLoadedStyledField Field => m_field;

	internal PdfLoadedStyledField Parent => m_field;

	internal PdfCrossTable CrossTable => Parent.CrossTable;

	internal PdfDictionary Dictionary => m_dictionary;

	public RectangleF Bounds
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			RectangleF bounds = m_field.Bounds;
			m_field.m_defaultIndex = defaultIndex;
			return bounds;
		}
		set
		{
			int defaultIndex = m_field.m_defaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			m_field.Bounds = value;
			m_field.m_defaultIndex = defaultIndex;
			Field.NotifyPropertyChanged("Bounds", m_collectionIndex);
		}
	}

	public PointF Location
	{
		get
		{
			return Bounds.Location;
		}
		set
		{
			Bounds = new RectangleF(value, Bounds.Size);
			Field.NotifyPropertyChanged("Location", m_collectionIndex);
		}
	}

	public SizeF Size
	{
		get
		{
			return Bounds.Size;
		}
		set
		{
			Bounds = new RectangleF(Bounds.Location, value);
			Field.NotifyPropertyChanged("Size", m_collectionIndex);
		}
	}

	internal PdfPen BorderPen
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			PdfPen borderPen = m_field.BorderPen;
			m_field.m_defaultIndex = defaultIndex;
			return borderPen;
		}
	}

	internal PdfBorderStyle BorderStyle
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			PdfBorderStyle borderStyle = m_field.BorderStyle;
			m_field.m_defaultIndex = defaultIndex;
			return borderStyle;
		}
	}

	internal float[] DashPatern
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.DefaultIndex = m_collectionIndex;
			float[] dashPatern = m_field.DashPatern;
			m_field.DefaultIndex = defaultIndex;
			return dashPatern;
		}
	}

	internal float BorderWidth
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			float borderWidth = m_field.BorderWidth;
			m_field.m_defaultIndex = defaultIndex;
			return borderWidth;
		}
	}

	internal PdfStringFormat StringFormat
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			PdfStringFormat stringFormat = m_field.StringFormat;
			m_field.m_defaultIndex = defaultIndex;
			return stringFormat;
		}
	}

	internal PdfBrush BackBrush
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			PdfBrush backBrush = m_field.BackBrush;
			m_field.m_defaultIndex = defaultIndex;
			return backBrush;
		}
	}

	internal PdfBrush ForeBrush
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			PdfBrush foreBrush = m_field.ForeBrush;
			m_field.m_defaultIndex = defaultIndex;
			return foreBrush;
		}
	}

	internal PdfBrush ShadowBrush
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			PdfBrush shadowBrush = m_field.ShadowBrush;
			m_field.m_defaultIndex = defaultIndex;
			return shadowBrush;
		}
	}

	internal PdfFont Font
	{
		get
		{
			int defaultIndex = m_field.DefaultIndex;
			m_field.m_defaultIndex = m_collectionIndex;
			PdfFont font = m_field.Font;
			m_field.m_defaultIndex = defaultIndex;
			return font;
		}
	}

	public PdfPageBase Page
	{
		get
		{
			if (m_page == null)
			{
				int defaultIndex = m_field.DefaultIndex;
				m_field.m_defaultIndex = m_collectionIndex;
				m_page = m_field.Page;
				PdfName key = new PdfName("P");
				if (m_field.Kids != null && m_field.Kids.Count > 0 && CrossTable.Document is PdfLoadedDocument pdfLoadedDocument && m_dictionary != null)
				{
					if (m_dictionary.ContainsKey(key))
					{
						if (CrossTable.GetObject(m_dictionary["P"]) is PdfDictionary dic)
						{
							PdfReference reference = CrossTable.GetReference(m_dictionary);
							foreach (PdfPageBase page in pdfLoadedDocument.Pages)
							{
								PdfArray pdfArray = page.ObtainAnnotations();
								if (pdfArray == null)
								{
									continue;
								}
								for (int i = 0; i < pdfArray.Count; i++)
								{
									PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
									if (pdfReferenceHolder != null && pdfReferenceHolder.Reference == reference)
									{
										m_page = pdfLoadedDocument.Pages.GetPage(dic);
										m_field.m_defaultIndex = defaultIndex;
										return m_page;
									}
								}
							}
							m_field.DefaultIndex = defaultIndex;
							m_page = null;
						}
					}
					else
					{
						PdfReference reference2 = CrossTable.GetReference(m_dictionary);
						foreach (PdfLoadedPage page2 in pdfLoadedDocument.Pages)
						{
							PdfArray pdfArray2 = page2.ObtainAnnotations();
							if (pdfArray2 == null)
							{
								continue;
							}
							for (int j = 0; j < pdfArray2.Count; j++)
							{
								if ((pdfArray2[j] as PdfReferenceHolder).Reference == reference2)
								{
									m_page = page2;
									return m_page;
								}
							}
						}
						m_page = null;
					}
				}
				m_field.DefaultIndex = defaultIndex;
			}
			return m_page;
		}
		internal set
		{
			m_page = value;
		}
	}

	internal int PageIndex
	{
		get
		{
			PdfLoadedPage pdfLoadedPage = Page as PdfLoadedPage;
			PdfPage pdfPage = Page as PdfPage;
			if (pdfLoadedPage != null && pdfLoadedPage.Document != null)
			{
				if (pdfLoadedPage.Document is PdfLoadedDocument pdfLoadedDocument)
				{
					return pdfLoadedDocument.Pages.IndexOf(pdfLoadedPage);
				}
				return -1;
			}
			if (pdfPage != null && pdfPage.Document != null)
			{
				return pdfPage.Document?.Pages.IndexOf(pdfPage) ?? (-1);
			}
			return -1;
		}
	}

	internal PdfLoadedFieldItem(PdfLoadedStyledField field, int index, PdfDictionary dictionary)
	{
		m_field = field;
		m_collectionIndex = index;
		m_dictionary = dictionary;
	}
}
