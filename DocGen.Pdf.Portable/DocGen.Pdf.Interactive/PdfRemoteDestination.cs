using System;

namespace DocGen.Pdf.Interactive;

public class PdfRemoteDestination : PdfDestination
{
	private int m_pageNumber;

	public int RemotePageNumber
	{
		get
		{
			return m_pageNumber;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentNullException("PageNumber cannot be less than 0.");
			}
			m_pageNumber = value;
		}
	}
}
