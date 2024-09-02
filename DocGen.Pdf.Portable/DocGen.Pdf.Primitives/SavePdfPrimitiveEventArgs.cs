using System;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Primitives;

internal class SavePdfPrimitiveEventArgs : EventArgs
{
	private IPdfWriter m_writer;

	public IPdfWriter Writer => m_writer;

	public SavePdfPrimitiveEventArgs(IPdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		m_writer = writer;
	}
}
