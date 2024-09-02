using System.Text;

namespace DocGen.PdfViewer.Base;

internal class PdfViewerExceptions
{
	private static StringBuilder m_exceptions = new StringBuilder();

	public StringBuilder Exceptions
	{
		get
		{
			return m_exceptions;
		}
		set
		{
			m_exceptions.Append(value);
		}
	}
}
