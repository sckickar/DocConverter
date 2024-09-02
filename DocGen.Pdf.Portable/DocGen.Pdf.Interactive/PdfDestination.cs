using System;
using DocGen.Drawing;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfDestination : IPdfWrapper
{
	private PdfDestinationMode m_destinationMode;

	private float m_zoom;

	private PointF m_location = PointF.Empty;

	private RectangleF m_bounds = RectangleF.Empty;

	private PdfPageBase m_page;

	private int m_index = -1;

	private PdfArray m_array = new PdfArray();

	private bool m_isValid = true;

	internal bool isModified;

	public float Zoom
	{
		get
		{
			return m_zoom;
		}
		set
		{
			if (m_zoom != value)
			{
				m_zoom = value;
				InitializePrimitive();
				isModified = true;
			}
		}
	}

	public PdfPageBase Page
	{
		get
		{
			return m_page;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Page");
			}
			if (m_page != value)
			{
				m_page = value;
				InitializePrimitive();
				isModified = true;
			}
		}
	}

	public int PageIndex
	{
		get
		{
			if (m_index == -1 && Page != null && Page is PdfPage { Document: not null } pdfPage)
			{
				m_index = pdfPage.Document.Pages.IndexOf(pdfPage);
			}
			return m_index;
		}
		internal set
		{
			m_index = value;
			isModified = true;
		}
	}

	public PdfDestinationMode Mode
	{
		get
		{
			return m_destinationMode;
		}
		set
		{
			if (m_destinationMode != value)
			{
				m_destinationMode = value;
				InitializePrimitive();
				isModified = true;
			}
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
			if (m_location != value)
			{
				m_location = value;
				InitializePrimitive();
				isModified = true;
			}
		}
	}

	internal RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			if (m_bounds != value)
			{
				m_bounds = value;
				InitializePrimitive();
				isModified = true;
			}
		}
	}

	public bool IsValid => m_isValid;

	IPdfPrimitive IPdfWrapper.Element
	{
		get
		{
			InitializePrimitive();
			return m_array;
		}
	}

	public PdfDestination(PdfPageBase page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfPageRotateAngle pdfPageRotateAngle = PdfPageRotateAngle.RotateAngle0;
		if (page.Rotation != 0 && page.Rotation != PdfPageRotateAngle.RotateAngle90)
		{
			pdfPageRotateAngle = page.Rotation;
		}
		if (page is PdfPage)
		{
			if ((page as PdfPage).m_section != null)
			{
				PdfPageRotateAngle rotate = (page as PdfPage).Section.PageSettings.Rotate;
				if (rotate != 0 && rotate != PdfPageRotateAngle.RotateAngle90 && rotate != pdfPageRotateAngle)
				{
					pdfPageRotateAngle = rotate;
				}
			}
		}
		else if (page is PdfLoadedPage)
		{
			PageIndex = ((page as PdfLoadedPage).Document as PdfLoadedDocument).Pages.IndexOf(page);
			isModified = false;
		}
		if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
		{
			m_location = new PointF(page.Size.Width, m_location.Y);
		}
		else if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
		{
			m_location = new PointF(0f, 0f);
		}
		else if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
		{
			m_location = new PointF(page.Size.Width, 0f);
		}
		else
		{
			m_location = new PointF(0f, m_location.Y);
		}
		m_page = page;
	}

	public PdfDestination(PdfPageBase page, PointF location)
		: this(page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_location = location;
	}

	internal PdfDestination(PdfPageBase page, RectangleF rect)
		: this(page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_location = rect.Location;
		m_bounds = rect;
	}

	internal PdfDestination()
	{
	}

	internal void SetValidation(bool valid)
	{
		m_isValid = valid;
	}

	private PointF PointToNativePdf(PdfPage page, PointF point)
	{
		return page.Section.PointToNativePdf(page, point);
	}

	private void InitializePrimitive()
	{
		if (m_page == null)
		{
			return;
		}
		m_array.Clear();
		if (((IPdfWrapper)m_page).Element != null)
		{
			m_array.Add(new PdfReferenceHolder(m_page));
		}
		switch (m_destinationMode)
		{
		case PdfDestinationMode.Location:
		{
			PdfPage pdfPage2 = m_page as PdfPage;
			PointF pointF = PointF.Empty;
			if (pdfPage2 != null)
			{
				pointF = ((pdfPage2.m_section == null) ? m_location : PointToNativePdf(pdfPage2, m_location));
			}
			else if (m_page is PdfLoadedPage pdfLoadedPage2)
			{
				pointF.X = m_location.X;
				pointF.Y = pdfLoadedPage2.Size.Height - m_location.Y;
			}
			m_array.Add(new PdfName("XYZ"));
			m_array.Add(new PdfNumber(pointF.X));
			m_array.Add(new PdfNumber(pointF.Y));
			m_array.Add(new PdfNumber(m_zoom));
			break;
		}
		case PdfDestinationMode.FitToPage:
			m_array.Add(new PdfName("Fit"));
			break;
		case PdfDestinationMode.FitR:
			m_array.Add(new PdfName("FitR"));
			m_array.Add(new PdfNumber(m_bounds.X));
			m_array.Add(new PdfNumber(m_bounds.Y));
			m_array.Add(new PdfNumber(m_bounds.Width));
			m_array.Add(new PdfNumber(m_bounds.Height));
			break;
		case PdfDestinationMode.FitH:
		{
			PdfLoadedPage pdfLoadedPage = m_page as PdfLoadedPage;
			PdfPage pdfPage = m_page as PdfPage;
			float num = 0f;
			num = ((pdfLoadedPage == null) ? (pdfPage.Size.Height - m_location.Y) : (pdfLoadedPage.Size.Height - m_location.Y));
			m_array.Add(new PdfName("FitH"));
			m_array.Add(new PdfNumber(num));
			break;
		}
		}
	}

	private void Initialize()
	{
	}
}
