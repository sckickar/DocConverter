using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

[Obsolete("Please use PdfAnnotation instead")]
public abstract class PdfAnnotation1 : IPdfWrapper
{
	private PdfColor m_color = PdfColor.Empty;

	private PdfAnnotationBorder m_border;

	private RectangleF m_rectangle = RectangleF.Empty;

	private PdfPage m_page;

	private string m_text = string.Empty;

	private PdfAnnotationFlags m_annotationFlags;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfAnnotationCollection m_annotations;

	public PdfColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			if (m_color != value)
			{
				m_color = value;
				PdfColorSpace colorSpace = PdfColorSpace.RGB;
				if (Page != null)
				{
					colorSpace = Page.Section.Parent.Document.ColorSpace;
				}
				PdfArray primitive = m_color.ToArray(colorSpace);
				m_dictionary.SetProperty("C", primitive);
			}
		}
	}

	public PdfAnnotationBorder Border
	{
		get
		{
			if (m_border == null)
			{
				m_border = new PdfAnnotationBorder();
			}
			return m_border;
		}
		set
		{
			m_border = value;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return m_rectangle;
		}
		set
		{
			if (m_rectangle != value)
			{
				m_rectangle = value;
			}
		}
	}

	public PointF Location
	{
		get
		{
			return m_rectangle.Location;
		}
		set
		{
			m_rectangle.Location = value;
		}
	}

	public SizeF Size
	{
		get
		{
			return m_rectangle.Size;
		}
		set
		{
			m_rectangle.Size = value;
		}
	}

	public PdfPage Page => m_page;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Text");
			}
			if (m_text != value)
			{
				m_text = value;
				m_dictionary.SetString("Contents", m_text);
			}
		}
	}

	public PdfAnnotationFlags AnnotationFlags
	{
		get
		{
			return m_annotationFlags;
		}
		set
		{
			if (m_annotationFlags != value)
			{
				m_annotationFlags = value;
				m_dictionary.SetNumber("F", (int)m_annotationFlags);
			}
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	protected PdfAnnotation1()
	{
		if (Dictionary.ContainsKey("Annots"))
		{
			m_annotations.Annotations = Dictionary["Annots"] as PdfArray;
		}
		Initialize();
	}

	protected PdfAnnotation1(RectangleF bounds)
	{
		Initialize();
		Bounds = bounds;
	}

	internal void SetPage(PdfPage page)
	{
		m_page = page;
		if (m_page == null)
		{
			m_dictionary.Remove(new PdfName("P"));
		}
		else
		{
			m_dictionary.SetProperty("P", new PdfReferenceHolder(m_page));
		}
	}

	internal void AssignLocation(PointF location)
	{
		m_rectangle.Location = location;
	}

	internal void AssignSize(SizeF size)
	{
		m_rectangle.Size = size;
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Type", new PdfName("Annot"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		if ((GetType().ToString().Contains("Pdf3DAnnotation") || GetType().ToString().Contains("PdfAttachmentAnnotation") || GetType().ToString().Contains("PdfSoundAnnotation") || GetType().ToString().Contains("PdfActionAnnotation")) && PdfDocument.ConformanceLevel != 0)
		{
			throw new PdfConformanceException("The specified annotation type is not supported by PDF/A1-B standard document.");
		}
		if (m_border != null)
		{
			m_dictionary.SetProperty("Border", m_border);
		}
		RectangleF rectangle = new RectangleF(m_rectangle.X, m_rectangle.Bottom, m_rectangle.Width, m_rectangle.Height);
		if (m_page != null)
		{
			PdfSection section = m_page.Section;
			rectangle.Location = section.PointToNativePdf(Page, rectangle.Location);
		}
		m_dictionary.SetProperty("Rect", PdfArray.FromRectangle(rectangle));
	}
}
