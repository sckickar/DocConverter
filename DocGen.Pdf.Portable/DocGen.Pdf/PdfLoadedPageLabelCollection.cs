using System;
using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfLoadedPageLabelCollection : IPdfWrapper
{
	private int m_count;

	private List<PdfPageLabel> m_pageLabel = new List<PdfPageLabel>();

	private List<PdfReferenceHolder> m_pageLabelCollection = new List<PdfReferenceHolder>();

	public int Count => m_count;

	public PdfPageLabel this[int index] => m_pageLabel[index];

	IPdfPrimitive IPdfWrapper.Element => null;

	public void Add(PdfPageLabel pageLabel)
	{
		if (pageLabel == null)
		{
			throw new ArgumentNullException("section");
		}
		PdfReferenceHolder item = new PdfReferenceHolder(pageLabel);
		m_pageLabel.Add(pageLabel);
		m_pageLabelCollection.Add(item);
		m_count++;
	}
}
