using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Interactive;

public abstract class PdfActionLinkAnnotation : PdfLinkAnnotation
{
	private PdfAction m_action;

	public virtual PdfAction Action
	{
		get
		{
			return m_action;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Action");
			}
			m_action = value;
		}
	}

	public PdfActionLinkAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	public PdfActionLinkAnnotation(RectangleF rectangle, PdfAction action)
		: base(rectangle)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		m_action = action;
	}
}
