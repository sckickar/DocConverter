using System;
using DocGen.DocIO.DLS;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal class LayoutArea
{
	private RectangleF m_area;

	private RectangleF m_clientArea;

	private RectangleF m_clientActiveArea;

	private ILayoutSpacingsInfo m_spacings;

	private bool m_bSkipSubtractWhenInvalidParameter = true;

	public double Width => OuterArea.Width;

	public double Height => OuterArea.Height;

	public bool SkipSubtractWhenInvalidParameter
	{
		get
		{
			return m_bSkipSubtractWhenInvalidParameter;
		}
		set
		{
			m_bSkipSubtractWhenInvalidParameter = value;
		}
	}

	public Spacings Margins => m_spacings.Margins;

	public Spacings Paddings => m_spacings.Paddings;

	public RectangleF OuterArea => m_area;

	public RectangleF ClientArea => m_clientArea;

	public RectangleF ClientActiveArea => m_clientActiveArea;

	public LayoutArea()
		: this(default(RectangleF), null, null)
	{
	}

	public LayoutArea(RectangleF area)
		: this(area, null, null)
	{
	}

	public LayoutArea(RectangleF area, ILayoutSpacingsInfo spacings, IWidget widget)
	{
		m_area = area;
		m_spacings = spacings;
		UpdateClientArea(widget);
	}

	public void CutFromLeft(double x, bool isSkip)
	{
		if (x < (double)m_clientActiveArea.Left || x > (double)m_clientActiveArea.Right)
		{
			if (!SkipSubtractWhenInvalidParameter)
			{
				throw new ArgumentException("x");
			}
			if (x < (double)m_clientActiveArea.Left && !isSkip)
			{
				x = m_clientActiveArea.Left;
			}
		}
		RectangleF clientActiveArea = m_clientActiveArea;
		clientActiveArea.Width = (float)((double)clientActiveArea.Right - x);
		if (clientActiveArea.Width < 0f)
		{
			clientActiveArea.Width = 0f;
		}
		clientActiveArea.X = (float)x;
		m_clientActiveArea = clientActiveArea;
	}

	public void CutFromLeft(double x)
	{
		CutFromLeft(x, isSkip: false);
	}

	internal void UpdateDynamicRelayoutBounds(float x, float y, bool isNeedToUpdateWidth, float width)
	{
		RectangleF clientActiveArea = m_clientActiveArea;
		clientActiveArea.Height += clientActiveArea.Y - y;
		clientActiveArea.X = x;
		clientActiveArea.Y = y;
		if (isNeedToUpdateWidth)
		{
			clientActiveArea.Width = width;
		}
		m_clientActiveArea = clientActiveArea;
	}

	public void CutFromTop(double y)
	{
		CutFromTop(y, 0f);
	}

	public void CutFromTop(double y, float footnoteHeight)
	{
		CutFromTop(y, footnoteHeight, isSkip: false);
	}

	internal void CutFromTop(double y, float footnoteHeight, bool isSkip)
	{
		if (y < (double)m_clientActiveArea.Top || y > (double)m_clientActiveArea.Bottom)
		{
			if (!SkipSubtractWhenInvalidParameter)
			{
				throw new ArgumentException("y");
			}
			if (y < (double)m_clientActiveArea.Top)
			{
				y = m_clientActiveArea.Top;
			}
			else if (y > (double)m_clientActiveArea.Bottom && !isSkip)
			{
				y = m_clientActiveArea.Bottom;
			}
		}
		RectangleF clientActiveArea = m_clientActiveArea;
		float num = (float)((double)clientActiveArea.Bottom - y - (double)footnoteHeight);
		clientActiveArea.Height = ((num > 0f) ? num : 0f);
		clientActiveArea.Y = (float)y;
		m_clientActiveArea = clientActiveArea;
	}

	public void CutFromTop()
	{
		CutFromTop(ClientActiveArea.Bottom);
	}

	internal void UpdateClientActiveArea(RectangleF rectangle)
	{
		m_clientActiveArea = rectangle;
	}

	private void UpdateClientArea(IWidget widget)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		if (m_spacings != null)
		{
			num = Margins.Left + Paddings.Left;
			num2 = Margins.Top + Paddings.Top;
			num3 = Margins.Right + Paddings.Right;
			num4 = ((widget is WTableCell) ? Margins.Bottom : Paddings.Bottom);
		}
		double num5 = m_area.X;
		if (num5 < 0.0)
		{
			num5 = m_area.X;
		}
		if (((widget is WParagraph) ? (widget as WParagraph) : ((widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null)) != null && m_spacings != null)
		{
			num = Margins.Left;
			num3 = Margins.Right;
		}
		double num6 = m_area.Y;
		double num7 = m_area.Width;
		double num8 = m_area.Height;
		WTableCell wTableCell = ((widget is WTableCell) ? (widget as WTableCell) : ((widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) ? ((widget as SplitWidgetContainer).RealWidgetContainer as WTableCell) : null));
		if (wTableCell != null && widget.LayoutInfo.IsVerticalText)
		{
			num7 = m_area.Height;
			num8 = m_area.Width;
			if (wTableCell.CellFormat.TextDirection == TextDirection.VerticalTopToBottom)
			{
				num5 = (double)m_area.X + num2;
				num7 = (double)m_area.Height - num2 - num4;
				if (wTableCell.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 || (widget.LayoutInfo as CellLayoutInfo).VerticalAlignment != VerticalAlignment.Middle)
				{
					num6 = (double)m_area.Y + num3;
					num8 = (double)m_area.Width - num3;
				}
			}
			else
			{
				num5 = (double)m_area.X + num4;
				num7 = (double)m_area.Height - num2 - num4;
				if (wTableCell.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 || (widget.LayoutInfo as CellLayoutInfo).VerticalAlignment != VerticalAlignment.Middle)
				{
					num6 = (double)m_area.Y + num;
					num8 = (double)m_area.Width - num;
				}
			}
		}
		else
		{
			num5 = (double)m_area.X + num;
			num6 = (double)m_area.Y + num2;
			num7 = (double)m_area.Width - num - num3;
			num8 = (double)m_area.Height - num2 - num4;
		}
		if (widget != null && widget.LayoutInfo is ParagraphLayoutInfo && !(widget.LayoutInfo as ParagraphLayoutInfo).IsFirstLine && m_spacings != null)
		{
			num6 -= (double)Margins.Top;
			num8 += (double)Margins.Top;
		}
		if (num7 < 0.0)
		{
			num7 = 0.0;
		}
		if (num8 < 0.0)
		{
			num8 = 0.0;
		}
		num5 = Math.Round(num5, 2);
		num6 = Math.Round(num6, 2);
		num7 = Math.Round(num7, 2);
		num8 = Math.Round(num8, 2);
		m_clientArea = new RectangleF((float)num5, (float)num6, (float)num7, (float)num8);
		m_clientActiveArea = m_clientArea;
		if (wTableCell != null)
		{
			(widget.LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds = m_clientArea;
		}
	}

	internal void UpdateBounds(float topPad)
	{
		float num = Math.Abs(topPad - ((m_spacings != null) ? (Margins.Top + Paddings.Top) : 0f));
		m_clientArea.Y += num;
		m_clientArea.Height -= num;
		m_clientActiveArea = m_clientArea;
	}

	internal void UpdateBoundsBasedOnTextWrap(float bottom)
	{
		float num = bottom - m_clientArea.Y;
		m_clientArea.Y = bottom;
		m_clientArea.Height -= num;
		m_clientActiveArea = m_clientArea;
	}

	internal void UpdateWidth(float previousTabPosition)
	{
		if (previousTabPosition == 0f)
		{
			m_clientArea.Width = 1584f - (m_clientActiveArea.X - m_area.X);
		}
		else
		{
			m_clientArea.Width = (float)Math.Round(1584f - previousTabPosition);
		}
		m_clientActiveArea.Width = m_clientArea.Width;
	}

	internal void UpdateLeftPosition(float x)
	{
		RectangleF clientActiveArea = m_clientActiveArea;
		clientActiveArea.Width += clientActiveArea.X - x;
		clientActiveArea.X = x;
		m_clientActiveArea = clientActiveArea;
	}
}
