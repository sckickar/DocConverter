using System.IO;
using System.Xml;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

public class PdfXfaDocument : ICloneable
{
	private PdfXfaPageCollection m_pages = new PdfXfaPageCollection();

	private PdfXfaForm m_form;

	internal int m_pageCount;

	private PdfDocument m_document;

	private PdfFileStructure m_fileStructure;

	internal PdfXfaType formType;

	internal XmlWriter dataSetWriter;

	internal PdfArray m_imageArray = new PdfArray();

	internal string m_formName = string.Empty;

	private PdfXfaPageSettings m_pageSettings = new PdfXfaPageSettings();

	public PdfXfaPageSettings PageSettings
	{
		get
		{
			return m_pageSettings;
		}
		set
		{
			if (value != null)
			{
				m_pageSettings = value;
			}
		}
	}

	public PdfXfaPageCollection Pages
	{
		get
		{
			m_pages.m_parent = this;
			return m_pages;
		}
		internal set
		{
			m_pages = value;
		}
	}

	public PdfXfaForm XfaForm
	{
		get
		{
			return m_form;
		}
		set
		{
			m_form = value;
		}
	}

	public string FormName
	{
		get
		{
			return m_formName;
		}
		set
		{
			if (value != null)
			{
				m_formName = value;
			}
		}
	}

	internal void Save(PdfDocument doc)
	{
		if (XfaForm != null)
		{
			XfaForm.Save(doc, formType);
		}
	}

	public void Save(Stream stream, PdfXfaType type)
	{
		m_document = new PdfDocument();
		formType = type;
		m_document.Form.Xfa = this;
		if (XfaForm != null)
		{
			XfaForm.m_xfaDocument = this;
			XfaForm.m_formType = type;
		}
		m_document.Save(stream);
	}

	public void Close()
	{
		m_document.Close(completely: true);
	}

	public object Clone()
	{
		PdfXfaDocument obj = MemberwiseClone() as PdfXfaDocument;
		obj.XfaForm = XfaForm.Clone() as PdfXfaForm;
		obj.FormName = FormName;
		obj.Pages = Pages.Clone() as PdfXfaPageCollection;
		obj.PageSettings = PageSettings;
		return obj;
	}
}
