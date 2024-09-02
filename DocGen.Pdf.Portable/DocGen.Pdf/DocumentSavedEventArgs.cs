using System;
using DocGen.Pdf.IO;

namespace DocGen.Pdf;

internal class DocumentSavedEventArgs : EventArgs
{
	private PdfWriter m_writer;

	internal PdfWriter Writer => m_writer;

	internal DocumentSavedEventArgs(PdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		m_writer = writer;
	}
}
