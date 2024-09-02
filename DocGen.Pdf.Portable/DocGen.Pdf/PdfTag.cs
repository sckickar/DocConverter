using DocGen.Drawing;

namespace DocGen.Pdf;

public abstract class PdfTag
{
	private int m_tagOrder;

	private RectangleF bounds;

	public virtual int Order
	{
		get
		{
			return m_tagOrder;
		}
		set
		{
			m_tagOrder = value;
		}
	}

	internal RectangleF Bounds
	{
		get
		{
			return bounds;
		}
		set
		{
			bounds = value;
		}
	}
}
