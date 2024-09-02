using System;
using System.IO;

namespace DocGen.Pdf.Parsing;

public class PdfDocumentSplitEventArgs : EventArgs
{
	private Stream m_stream;

	public Stream PdfDocumentData
	{
		get
		{
			return m_stream;
		}
		set
		{
			m_stream = value;
		}
	}

	internal PdfDocumentSplitEventArgs(Stream doc)
	{
		m_stream = doc;
	}
}
