using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class PdfDocumentTemplate
{
	private PdfPageTemplateElement m_left;

	private PdfPageTemplateElement m_top;

	private PdfPageTemplateElement m_right;

	private PdfPageTemplateElement m_bottom;

	private PdfPageTemplateElement m_evenLeft;

	private PdfPageTemplateElement m_evenTop;

	private PdfPageTemplateElement m_evenRight;

	private PdfPageTemplateElement m_evenBottom;

	private PdfPageTemplateElement m_oddLeft;

	private PdfPageTemplateElement m_oddTop;

	private PdfPageTemplateElement m_oddRight;

	private PdfPageTemplateElement m_oddBottom;

	private PdfStampCollection m_stamps;

	internal PdfMargins blinkMargin;

	public PdfPageTemplateElement Left
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = CheckElement(value, TemplateType.Left);
		}
	}

	public PdfPageTemplateElement Top
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = CheckElement(value, TemplateType.Top);
		}
	}

	public PdfPageTemplateElement Right
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = CheckElement(value, TemplateType.Right);
		}
	}

	public PdfPageTemplateElement Bottom
	{
		get
		{
			return m_bottom;
		}
		set
		{
			m_bottom = CheckElement(value, TemplateType.Bottom);
		}
	}

	public PdfPageTemplateElement EvenLeft
	{
		get
		{
			return m_evenLeft;
		}
		set
		{
			m_evenLeft = CheckElement(value, TemplateType.Left);
		}
	}

	public PdfPageTemplateElement EvenTop
	{
		get
		{
			return m_evenTop;
		}
		set
		{
			m_evenTop = CheckElement(value, TemplateType.Top);
		}
	}

	public PdfPageTemplateElement EvenRight
	{
		get
		{
			return m_evenRight;
		}
		set
		{
			m_evenRight = CheckElement(value, TemplateType.Right);
		}
	}

	public PdfPageTemplateElement EvenBottom
	{
		get
		{
			return m_evenBottom;
		}
		set
		{
			m_evenBottom = CheckElement(value, TemplateType.Bottom);
		}
	}

	public PdfPageTemplateElement OddLeft
	{
		get
		{
			return m_oddLeft;
		}
		set
		{
			m_oddLeft = CheckElement(value, TemplateType.Left);
		}
	}

	public PdfPageTemplateElement OddTop
	{
		get
		{
			return m_oddTop;
		}
		set
		{
			m_oddTop = CheckElement(value, TemplateType.Top);
		}
	}

	public PdfPageTemplateElement OddRight
	{
		get
		{
			return m_oddRight;
		}
		set
		{
			m_oddRight = CheckElement(value, TemplateType.Right);
		}
	}

	public PdfPageTemplateElement OddBottom
	{
		get
		{
			return m_oddBottom;
		}
		set
		{
			m_oddBottom = CheckElement(value, TemplateType.Bottom);
		}
	}

	public PdfStampCollection Stamps
	{
		get
		{
			if (m_stamps == null)
			{
				m_stamps = new PdfStampCollection();
			}
			return m_stamps;
		}
	}

	internal PdfPageTemplateElement GetLeft(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfPageTemplateElement result = null;
		if (page.Document.Pages != null && (EvenLeft != null || OddLeft != null || Left != null))
		{
			result = ((!IsEven(page)) ? ((OddLeft != null) ? OddLeft : Left) : ((EvenLeft != null) ? EvenLeft : Left));
		}
		return result;
	}

	internal PdfPageTemplateElement GetTop(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfPageTemplateElement result = null;
		if (page.Document.Pages != null && (EvenTop != null || OddTop != null || Top != null))
		{
			result = ((!IsEven(page)) ? ((OddTop != null) ? OddTop : Top) : ((EvenTop != null) ? EvenTop : Top));
		}
		return result;
	}

	internal PdfPageTemplateElement GetRight(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfPageTemplateElement result = null;
		if (page.Document.Pages != null && (EvenRight != null || OddRight != null || Right != null))
		{
			result = ((!IsEven(page)) ? ((OddRight != null) ? OddRight : Right) : ((EvenRight != null) ? EvenRight : Right));
		}
		return result;
	}

	internal PdfPageTemplateElement GetBottom(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfPageTemplateElement result = null;
		if (page.Document.Pages != null && (EvenBottom != null || OddBottom != null || Bottom != null))
		{
			result = ((!IsEven(page)) ? ((OddBottom != null) ? OddBottom : Bottom) : ((EvenBottom != null) ? EvenBottom : Bottom));
		}
		return result;
	}

	private bool IsEven(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfDocumentPageCollection pages = page.Section.Document.Pages;
		int num = 0;
		num = ((!pages.PageCollectionIndex.ContainsKey(page)) ? (pages.IndexOf(page) + 1) : (pages.PageCollectionIndex[page] + 1));
		return num % 2 == 0;
	}

	private PdfPageTemplateElement CheckElement(PdfPageTemplateElement templateElement, TemplateType type)
	{
		if (templateElement != null)
		{
			if (templateElement.Type != 0)
			{
				throw new NotSupportedException("Can't reassign the template element. Please, create new one.");
			}
			templateElement.Type = type;
		}
		return templateElement;
	}
}
