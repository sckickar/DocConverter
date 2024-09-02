using System;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

internal class PdfAnnotationBase : PdfAnnotation
{
	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfCrossTable m_crossTable = new PdfCrossTable();

	private List<PdfAnnotation> m_list = new List<PdfAnnotation>();

	public PdfAnnotation this[int index] => List[index];

	internal virtual List<PdfAnnotation> List => m_list;

	internal new PdfDictionary Dictionary => m_dictionary;

	internal PdfCrossTable CrossTable => m_crossTable;

	internal PdfAnnotationBase()
	{
	}

	internal PdfAnnotationBase(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		m_dictionary = dictionary;
		if (crossTable != null)
		{
			m_crossTable = crossTable;
		}
	}

	public PdfAnnotation Add(string title)
	{
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		PdfAnnotation pdfAnnotation = null;
		pdfAnnotation.Text = title;
		List.Add(pdfAnnotation);
		UpdateFields();
		return pdfAnnotation;
	}

	private void UpdateFields()
	{
		if (List.Count > 0)
		{
			m_dictionary.SetNumber("Count", List.Count);
		}
		else
		{
			m_dictionary.Clear();
		}
		m_dictionary.Modify();
	}
}
