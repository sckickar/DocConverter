using DocGen.Drawing;

namespace DocGen.Layouting;

internal class LayoutedStringWidget
{
	private RectangleF m_bounds;

	private string m_text;

	private Font m_font;

	private bool m_isStretchable;

	internal RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	internal string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal Font Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

	internal bool IsStretchable
	{
		get
		{
			return m_isStretchable;
		}
		set
		{
			m_isStretchable = value;
		}
	}

	internal LayoutedStringWidget()
	{
	}

	internal LayoutedStringWidget(LayoutedStringWidget srcWidget)
	{
		Bounds = srcWidget.Bounds;
		Text = srcWidget.Text;
		Font = srcWidget.Font;
		IsStretchable = srcWidget.IsStretchable;
	}

	public void ShiftXYPosition(float xPosition, float yPosition)
	{
		Bounds = new RectangleF(Bounds.X + xPosition, Bounds.Y + yPosition, Bounds.Width, Bounds.Height);
	}

	public void Dispose()
	{
		m_text = null;
		if (m_font != null)
		{
			m_font = null;
		}
	}
}
