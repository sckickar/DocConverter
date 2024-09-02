using System;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf;

internal class AnnotationRemovedArgs : EventArgs
{
	private PdfAnnotation m_annotation;

	public PdfAnnotation Annotation
	{
		get
		{
			return m_annotation;
		}
		set
		{
			m_annotation = value;
		}
	}
}
