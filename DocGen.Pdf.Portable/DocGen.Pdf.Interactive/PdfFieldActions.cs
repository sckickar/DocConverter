using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfFieldActions : IPdfWrapper
{
	private PdfAnnotationActions m_annotationActions;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfJavaScriptAction m_keyPressed;

	private PdfJavaScriptAction m_format;

	private PdfJavaScriptAction m_validate;

	private PdfJavaScriptAction m_calculate;

	public PdfJavaScriptAction KeyPressed
	{
		get
		{
			return m_keyPressed;
		}
		set
		{
			if (m_keyPressed != value)
			{
				m_keyPressed = value;
				m_dictionary.SetProperty("K", m_keyPressed);
			}
		}
	}

	public PdfJavaScriptAction Format
	{
		get
		{
			return m_format;
		}
		set
		{
			if (m_format != value)
			{
				m_format = value;
				m_dictionary.SetProperty("F", m_format);
			}
		}
	}

	public PdfJavaScriptAction Validate
	{
		get
		{
			return m_validate;
		}
		set
		{
			if (m_validate != value)
			{
				m_validate = value;
				m_dictionary.SetProperty("V", m_validate);
			}
		}
	}

	public PdfJavaScriptAction Calculate
	{
		get
		{
			return m_calculate;
		}
		set
		{
			if (m_calculate != value)
			{
				m_calculate = value;
				m_dictionary.SetProperty("C", m_calculate);
			}
		}
	}

	public PdfAction MouseEnter
	{
		get
		{
			return m_annotationActions.MouseEnter;
		}
		set
		{
			m_annotationActions.MouseEnter = value;
		}
	}

	public PdfAction MouseLeave
	{
		get
		{
			return m_annotationActions.MouseLeave;
		}
		set
		{
			m_annotationActions.MouseLeave = value;
		}
	}

	public PdfAction MouseUp
	{
		get
		{
			return m_annotationActions.MouseUp;
		}
		set
		{
			m_annotationActions.MouseUp = value;
		}
	}

	public PdfAction MouseDown
	{
		get
		{
			return m_annotationActions.MouseDown;
		}
		set
		{
			m_annotationActions.MouseDown = value;
		}
	}

	public PdfAction GotFocus
	{
		get
		{
			return m_annotationActions.GotFocus;
		}
		set
		{
			m_annotationActions.GotFocus = value;
		}
	}

	public PdfAction LostFocus
	{
		get
		{
			return m_annotationActions.LostFocus;
		}
		set
		{
			m_annotationActions.LostFocus = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfFieldActions(PdfAnnotationActions annotationActions)
	{
		if (annotationActions == null)
		{
			throw new ArgumentNullException("annotationActrions");
		}
		m_annotationActions = annotationActions;
	}
}
