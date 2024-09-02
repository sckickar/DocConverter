using System;

namespace DocGen.Pdf.Graphics;

public class PdfCancelEventArgs : EventArgs
{
	private bool m_cancel;

	public bool Cancel
	{
		get
		{
			return m_cancel;
		}
		set
		{
			m_cancel = value;
		}
	}
}
