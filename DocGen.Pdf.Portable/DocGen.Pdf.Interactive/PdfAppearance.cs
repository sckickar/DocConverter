using System;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfAppearance : IPdfWrapper
{
	private PdfTemplate m_templateNormal;

	private PdfTemplate m_templateMouseHover;

	private PdfTemplate m_templatePressed;

	private PdfAnnotation m_annotation;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfTemplate m_appearanceLayer;

	internal bool IsCompletedValidationAppearance;

	public PdfTemplate Normal
	{
		get
		{
			if (m_templateNormal == null)
			{
				m_templateNormal = new PdfTemplate(m_annotation.Size);
				m_dictionary.SetProperty("N", new PdfReferenceHolder(m_templateNormal));
			}
			if (m_templateNormal != null && m_templateNormal.IsSignatureAppearanceValidation && !IsCompletedValidationAppearance)
			{
				return AppearanceLayer;
			}
			return m_templateNormal;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Normal");
			}
			if (m_templateNormal != value)
			{
				m_templateNormal = value;
				m_dictionary.SetProperty("N", new PdfReferenceHolder(m_templateNormal));
			}
		}
	}

	public PdfTemplate MouseHover
	{
		get
		{
			if (m_templateMouseHover == null)
			{
				m_templateMouseHover = new PdfTemplate(m_annotation.Size);
				m_dictionary.SetProperty("R", new PdfReferenceHolder(m_templateMouseHover));
			}
			return m_templateMouseHover;
		}
		set
		{
			if (m_templateMouseHover != value)
			{
				m_templateMouseHover = value;
				m_dictionary.SetProperty("R", new PdfReferenceHolder(m_templateMouseHover));
			}
		}
	}

	public PdfTemplate Pressed
	{
		get
		{
			if (m_templatePressed == null)
			{
				m_templatePressed = new PdfTemplate(m_annotation.Size);
				m_dictionary.SetProperty("D", new PdfReferenceHolder(m_templatePressed));
			}
			return m_templatePressed;
		}
		set
		{
			if (value != m_templatePressed)
			{
				m_templatePressed = value;
				m_dictionary.SetProperty("D", new PdfReferenceHolder(m_templatePressed));
			}
		}
	}

	internal PdfTemplate AppearanceLayer
	{
		get
		{
			if (m_appearanceLayer == null)
			{
				PdfPageBase pdfPageBase = null;
				pdfPageBase = ((m_annotation.Page == null) ? ((PdfPageBase)m_annotation.LoadedPage) : ((PdfPageBase)m_annotation.Page));
				if (pdfPageBase != null && (pdfPageBase.Rotation == PdfPageRotateAngle.RotateAngle90 || pdfPageBase.Rotation == PdfPageRotateAngle.RotateAngle270))
				{
					m_appearanceLayer = new PdfTemplate(m_annotation.Size.Height, m_annotation.Size.Width);
				}
				else
				{
					m_appearanceLayer = new PdfTemplate(m_annotation.Size.Width, m_annotation.Size.Height);
				}
				m_appearanceLayer.CustomPdfTemplateName = "n2";
			}
			return m_appearanceLayer;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfAppearance(PdfAnnotation annotation)
	{
		m_annotation = annotation;
		m_annotation.Appearance = this;
	}

	internal PdfTemplate GetNormalTemplate()
	{
		return m_templateNormal;
	}

	internal PdfTemplate GetPressedTemplate()
	{
		return m_templatePressed;
	}
}
