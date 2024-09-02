using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfAction : IPdfWrapper
{
	private PdfAction m_action;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public PdfAction Next
	{
		get
		{
			return m_action;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Next");
			}
			if (m_action != value)
			{
				m_action = value;
				Dictionary.SetArray("Next", new PdfReferenceHolder(m_action));
			}
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	protected PdfAction()
	{
		Initialize();
	}

	protected virtual void Initialize()
	{
		Dictionary.SetProperty("Type", new PdfName("Action"));
	}
}
