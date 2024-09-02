using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartDockingManager : IDisposable
{
	private class WrapLayouter
	{
		private List<int> m_lines = new List<int>();

		private List<IChartDockControl> m_elements = new List<IChartDockControl>();

		private int m_dimension;

		private int m_spacing;

		private bool m_isVertical;

		public List<IChartDockControl> Elements => m_elements;

		public int Dimension
		{
			get
			{
				return m_dimension;
			}
			set
			{
				m_dimension = value;
			}
		}

		public WrapLayouter(bool isVertical, int spacing)
		{
			m_isVertical = isVertical;
			m_spacing = spacing;
		}

		public int Measure(Graphics g, Size measureSize)
		{
			int num = 0;
			int num2 = 0;
			int num3 = (m_isVertical ? measureSize.Height : measureSize.Width);
			m_dimension = 0;
			m_lines.Clear();
			foreach (IChartDockControl element in m_elements)
			{
				Size size = Size.Round(element.Measure(g, measureSize));
				if (m_isVertical)
				{
					size = new Size(size.Height, size.Width);
				}
				if (num2 + size.Width > num3)
				{
					num2 = m_spacing + size.Width;
					m_dimension += num + m_spacing;
					m_lines.Add(num);
					num = 0;
				}
				else
				{
					num2 += m_spacing + size.Width;
				}
				num = Math.Max(num, size.Height);
			}
			if (num != 0)
			{
				m_lines.Add(num);
				m_dimension += num + m_spacing;
			}
			return m_dimension;
		}

		public void Arrange(Rectangle rect)
		{
			if (m_lines.Count <= 0 || rect.Height <= 0 || rect.Width <= 0)
			{
				return;
			}
			int num = (m_isVertical ? rect.Height : rect.Width);
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < m_elements.Count; i++)
			{
				IChartDockControl chartDockControl = m_elements[i];
				int num5 = (m_isVertical ? chartDockControl.Size.Height : chartDockControl.Size.Width);
				if (num2 + num5 > num)
				{
					num3 += m_lines[num4++] + m_spacing;
					num2 = 0;
				}
				if (m_isVertical)
				{
					chartDockControl.Location = new Point(rect.X + num3, rect.Y + num2);
				}
				else
				{
					chartDockControl.Location = new Point(rect.X + num2, rect.Y + num3);
				}
				num2 += num5 + m_spacing;
			}
		}
	}

	private Control m_host;

	private List<IChartDockControl> m_elements = new List<IChartDockControl>();

	private bool m_dockAlignment;

	private bool m_allIsSet = true;

	private bool m_supressEventFiring;

	private bool m_supressEventProcessing;

	private int m_spacing = 10;

	private Rectangle m_outsideRect = Rectangle.Empty;

	private Rectangle m_insideRect;

	private Size m_mouseOffset = Size.Empty;

	private ChartPlacement m_placement = ChartPlacement.Outside;

	private ChartLayoutMode m_layoutMode;

	private ChartControl chart;

	[DefaultValue(10)]
	public int Spacing
	{
		get
		{
			return m_spacing;
		}
		set
		{
			m_spacing = value;
		}
	}

	[DefaultValue(false)]
	public bool DockAlignment
	{
		get
		{
			return m_dockAlignment;
		}
		set
		{
			if (m_dockAlignment != value)
			{
				m_dockAlignment = value;
			}
		}
	}

	[DefaultValue(ChartPlacement.Outside)]
	public ChartPlacement Placement
	{
		get
		{
			return m_placement;
		}
		set
		{
			if (m_placement != value)
			{
				m_placement = value;
				RaiseSizeChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(ChartLayoutMode.Stack)]
	public ChartLayoutMode LayoutMode
	{
		get
		{
			return m_layoutMode;
		}
		set
		{
			m_layoutMode = value;
		}
	}

	public event EventHandler SizeChanged;

	public ChartDockingManager(ChartControl chart)
	{
		this.chart = chart;
	}

	public ChartDockingManager()
	{
	}

	public ChartDockingManager(Control host)
	{
		m_host = host;
	}

	public void Freeze()
	{
		m_supressEventFiring = true;
		m_supressEventProcessing = true;
	}

	public void Melt()
	{
		m_supressEventFiring = false;
		m_supressEventProcessing = false;
	}

	public void Add(IChartDockControl control)
	{
		if (control != null)
		{
			m_elements.Add(control);
			control.LocationChanging += Control_LocationChanging;
			control.ChartDockChanged += Control_ChartDockChanged;
			control.ChartAlignmentChanged += Control_ChartAlignmentChanged;
			control.SizeChanged += Control_SizeChanged;
		}
		RaiseSizeChanged(EventArgs.Empty);
	}

	public void Remove(IChartDockControl control)
	{
		if (control != null)
		{
			control.LocationChanging -= Control_LocationChanging;
			control.ChartDockChanged -= Control_ChartDockChanged;
			control.ChartAlignmentChanged -= Control_ChartAlignmentChanged;
			control.LocationChanged -= Control_LocationChanged;
			control.SizeChanged -= Control_SizeChanged;
		}
		m_elements.Remove(control);
		RaiseSizeChanged(EventArgs.Empty);
	}

	public Rectangle DoLayout(Rectangle rect)
	{
		Graphics graphics = chart.GetGraphics();
		if (graphics != null)
		{
			return DoLayout(rect, graphics);
		}
		return Rectangle.Empty;
	}

	public Rectangle DoLayout(Rectangle rect, Graphics g)
	{
		m_allIsSet = false;
		Rectangle insideRect = rect;
		insideRect.Inflate(-m_spacing, -m_spacing);
		m_outsideRect = rect;
		m_insideRect = insideRect;
		if (m_layoutMode == ChartLayoutMode.Wrap)
		{
			WrapLayouter wrapLayouter = new WrapLayouter(isVertical: false, m_spacing);
			WrapLayouter wrapLayouter2 = new WrapLayouter(isVertical: true, m_spacing);
			WrapLayouter wrapLayouter3 = new WrapLayouter(isVertical: true, m_spacing);
			WrapLayouter wrapLayouter4 = new WrapLayouter(isVertical: false, m_spacing);
			foreach (IChartDockControl element in m_elements)
			{
				if (element.Visible)
				{
					switch (element.Position)
					{
					case ChartDock.Left:
						wrapLayouter2.Elements.Add(element);
						break;
					case ChartDock.Right:
						wrapLayouter3.Elements.Add(element);
						break;
					case ChartDock.Top:
						wrapLayouter.Elements.Add(element);
						break;
					case ChartDock.Bottom:
						wrapLayouter4.Elements.Add(element);
						break;
					case ChartDock.Floating:
						element.Measure(g, rect.Size);
						break;
					}
				}
			}
			int num = wrapLayouter2.Measure(g, GetMeasureSize(ChartDock.Left, m_insideRect.Size));
			int num2 = wrapLayouter3.Measure(g, GetMeasureSize(ChartDock.Right, m_insideRect.Size));
			m_insideRect.X += num;
			m_insideRect.Width -= num + num2;
			int num3 = wrapLayouter.Measure(g, GetMeasureSize(ChartDock.Top, m_insideRect.Size));
			int num4 = wrapLayouter4.Measure(g, GetMeasureSize(ChartDock.Bottom, m_insideRect.Size));
			m_insideRect.Y += num3;
			m_insideRect.Height -= num3 + num4;
			wrapLayouter2.Arrange(new Rectangle(insideRect.Left, insideRect.Top, num, insideRect.Height));
			wrapLayouter3.Arrange(new Rectangle(insideRect.Right - num2 + m_spacing, insideRect.Top, num2, insideRect.Height));
			wrapLayouter.Arrange(new Rectangle(insideRect.Left + num, insideRect.Top, insideRect.Width - num - num2, num3));
			wrapLayouter4.Arrange(new Rectangle(insideRect.Left + num, insideRect.Bottom - num4 + m_spacing, insideRect.Width - num - num2, num4));
		}
		else
		{
			foreach (IChartDockControl element2 in m_elements)
			{
				Size size = element2.Measure(g, GetMeasureSize(element2.Position, m_insideRect.Size)).ToSize();
				if (element2.Visible && (element2.Behavior & ChartDockingFlags.Dockable) == ChartDockingFlags.Dockable && (!(element2 is ChartLegend) || (element2 as ChartLegend).IsLegendOverlapping))
				{
					switch (element2.Position)
					{
					case ChartDock.Bottom:
						m_insideRect.Height -= size.Height + m_spacing;
						break;
					case ChartDock.Left:
						m_insideRect.X += size.Width + m_spacing;
						m_insideRect.Width -= size.Width + m_spacing;
						break;
					case ChartDock.Right:
						m_insideRect.Width -= size.Width + m_spacing;
						break;
					case ChartDock.Top:
						m_insideRect.Y += (size.Height + m_spacing) * 3 / 4;
						m_insideRect.Height -= size.Height + m_spacing;
						break;
					}
				}
			}
			foreach (IChartDockControl element3 in m_elements)
			{
				Size size2 = element3.Size;
				if (element3.Visible && (element3.Behavior & ChartDockingFlags.Dockable) == ChartDockingFlags.Dockable)
				{
					switch (element3.Position)
					{
					case ChartDock.Left:
						SetToCenter(element3, new Rectangle(rect.Left + m_spacing, m_insideRect.Top, size2.Width, m_insideRect.Height));
						rect.X += size2.Width + m_spacing;
						rect.Width -= size2.Width + m_spacing;
						break;
					case ChartDock.Bottom:
						SetToCenter(element3, new Rectangle(insideRect.Left, rect.Bottom - size2.Height - m_spacing, insideRect.Width, size2.Height));
						rect.Height -= size2.Height + m_spacing;
						break;
					case ChartDock.Right:
						SetToCenter(element3, new Rectangle(rect.Right - size2.Width - m_spacing, m_insideRect.Top, size2.Width, m_insideRect.Height));
						rect.Width -= size2.Width + m_spacing;
						break;
					case ChartDock.Top:
						SetToCenter(element3, new Rectangle(insideRect.Left, rect.Top + m_spacing, insideRect.Width, size2.Height));
						rect.Y += (size2.Height + m_spacing) / 2;
						rect.Height -= size2.Height + m_spacing;
						break;
					}
				}
			}
		}
		m_allIsSet = true;
		if (m_placement != ChartPlacement.Outside)
		{
			return m_outsideRect;
		}
		return m_insideRect;
	}

	public void Clear()
	{
		Freeze();
		foreach (IChartDockControl element in m_elements)
		{
			Remove(element);
		}
		Melt();
		RaiseSizeChanged(EventArgs.Empty);
	}

	public IEnumerable GetAllControls()
	{
		return m_elements;
	}

	public void Dispose()
	{
		if (m_host != null)
		{
			m_host = null;
		}
		foreach (IChartDockControl element in m_elements)
		{
			element.LocationChanging -= Control_LocationChanging;
			element.ChartDockChanged -= Control_ChartDockChanged;
			element.ChartAlignmentChanged -= Control_ChartAlignmentChanged;
			element.SizeChanged -= Control_SizeChanged;
		}
		this.SizeChanged = null;
	}

	private void MoveInWrapDock(IChartDockControl dockElement, Point pt)
	{
		foreach (IChartDockControl element in m_elements)
		{
			if (dockElement != element && new Rectangle(element.Location, element.Size).Contains(pt))
			{
				Move(dockElement, m_elements.IndexOf(element));
				RaiseSizeChanged(EventArgs.Empty);
				break;
			}
		}
	}

	private void MoveInDock(IChartDockControl dockElement, Point pt)
	{
		bool flag = false;
		int num = m_elements.IndexOf(dockElement);
		for (int i = 0; i < m_elements.Count; i++)
		{
			IChartDockControl chartDockControl = m_elements[i];
			if (chartDockControl == dockElement || chartDockControl.Position != dockElement.Position)
			{
				continue;
			}
			int num2 = chartDockControl.Location.X + chartDockControl.Size.Width / 2;
			int num3 = chartDockControl.Location.Y + chartDockControl.Size.Height / 2;
			bool flag2 = num > i;
			bool flag3 = num < i;
			bool flag4 = pt.X < num2;
			bool flag5 = pt.Y < num3;
			switch (dockElement.Position)
			{
			case ChartDock.Left:
				if ((flag4 && flag2) || (!flag4 && flag3))
				{
					Move(dockElement, i);
					flag = true;
				}
				break;
			case ChartDock.Right:
				if ((flag4 && flag3) || (!flag4 && flag2))
				{
					Move(dockElement, i);
					flag = true;
				}
				break;
			case ChartDock.Top:
				if ((!flag5 && flag3) || (flag5 && flag2))
				{
					Move(dockElement, i);
					flag = true;
				}
				break;
			case ChartDock.Bottom:
				if ((!flag5 && flag2) || (flag5 && flag3))
				{
					Move(dockElement, i);
					flag = true;
				}
				break;
			}
		}
		if (m_dockAlignment)
		{
			dockElement.Alignment = GetAlignmentByRect(pt, m_insideRect, dockElement.Orientation);
		}
		if (flag)
		{
			DoLayout(m_outsideRect);
		}
	}

	private bool Dock(IChartDockControl dockElement, Point pt)
	{
		ChartDock chartDock = dockElement.Position;
		if (pt.X < m_insideRect.Left)
		{
			chartDock = ChartDock.Left;
		}
		else if (pt.X > m_insideRect.Right)
		{
			chartDock = ChartDock.Right;
		}
		else if (pt.Y < m_insideRect.Top)
		{
			chartDock = ChartDock.Top;
		}
		else if (pt.Y > m_insideRect.Bottom)
		{
			chartDock = ChartDock.Bottom;
		}
		else if (m_insideRect.Contains(pt))
		{
			chartDock = ChartDock.Floating;
		}
		if (dockElement.Position != chartDock)
		{
			dockElement.Position = chartDock;
			return true;
		}
		return false;
	}

	private void Move(IChartDockControl element, int to)
	{
		m_elements.Remove(element);
		m_elements.Insert(Math.Min(to, m_elements.Count), element);
	}

	private static void SetToCenter(IChartDockControl control, Rectangle rect)
	{
		int num = (rect.Width - control.Size.Width) / 2;
		int num2 = (rect.Height - control.Size.Height) / 2;
		if (control.Orientation == ChartOrientation.Horizontal)
		{
			if (control.Alignment == ChartAlignment.Center)
			{
				control.Location = new Point(rect.Left + num, rect.Top + num2);
			}
			else if (control.Alignment == ChartAlignment.Near)
			{
				control.Location = new Point(rect.Left + num2, rect.Top + num2);
			}
			else if (control.Alignment == ChartAlignment.Far)
			{
				control.Location = new Point(rect.Right - control.Size.Width - num2, rect.Top + num2);
			}
		}
		else if (control.Alignment == ChartAlignment.Center)
		{
			control.Location = new Point(rect.Left + num, rect.Top + num2);
		}
		else if (control.Alignment == ChartAlignment.Near)
		{
			control.Location = new Point(rect.Left + num, rect.Top + num);
		}
		else if (control.Alignment == ChartAlignment.Far)
		{
			control.Location = new Point(rect.Left + num, rect.Bottom - control.Size.Height - num);
		}
	}

	private static Size GetMeasureSize(ChartDock position, Size maxSize)
	{
		switch (position)
		{
		case ChartDock.Left:
		case ChartDock.Right:
			maxSize.Width /= 2;
			break;
		case ChartDock.Top:
		case ChartDock.Bottom:
			maxSize.Height /= 2;
			break;
		}
		return maxSize;
	}

	private ChartAlignment GetAlignmentByRect(Point pt, Rectangle rc, ChartOrientation or)
	{
		ChartAlignment result = ChartAlignment.Center;
		if (or == ChartOrientation.Horizontal)
		{
			int num = rc.Left + rc.Width / 3;
			int num2 = rc.Right - rc.Width / 3;
			if (pt.X < num)
			{
				result = ChartAlignment.Near;
			}
			else if (pt.X > num2)
			{
				result = ChartAlignment.Far;
			}
		}
		else
		{
			int num3 = rc.Top + rc.Height / 3;
			int num4 = rc.Bottom - rc.Height / 3;
			if (pt.Y < num3)
			{
				result = ChartAlignment.Near;
			}
			else if (pt.Y > num4)
			{
				result = ChartAlignment.Far;
			}
		}
		return result;
	}

	private void Control_LocationChanging(object sender, LocationEventArgs e)
	{
		if (m_supressEventProcessing || !m_allIsSet)
		{
			return;
		}
		m_allIsSet = false;
		IChartDockControl chartDockControl = sender as IChartDockControl;
		if ((chartDockControl.Behavior & ChartDockingFlags.Dockable) == ChartDockingFlags.Dockable)
		{
			Point empty = Point.Empty;
			if (!Dock(chartDockControl, empty))
			{
				if (m_layoutMode == ChartLayoutMode.Stack)
				{
					MoveInDock(chartDockControl, empty);
				}
				else
				{
					MoveInWrapDock(chartDockControl, empty);
				}
			}
			e.Allowed = chartDockControl.Position == ChartDock.Floating;
		}
		m_allIsSet = true;
	}

	private void Control_ChartDockChanged(object sender, EventArgs e)
	{
		if (!m_supressEventProcessing)
		{
			RaiseSizeChanged(EventArgs.Empty);
		}
	}

	private void Control_ChartAlignmentChanged(object sender, EventArgs e)
	{
		if (!m_supressEventProcessing)
		{
			DoLayout(m_outsideRect);
		}
	}

	private void RaiseSizeChanged(EventArgs e)
	{
		if (this.SizeChanged != null && !m_supressEventFiring)
		{
			this.SizeChanged(this, e);
		}
	}

	private void Control_LocationChanged(object sender, EventArgs e)
	{
		if (!m_supressEventProcessing && m_allIsSet)
		{
			m_allIsSet = false;
			IChartDockControl chartDockControl = sender as IChartDockControl;
			if ((chartDockControl.Behavior & ChartDockingFlags.Dockable) == ChartDockingFlags.Dockable)
			{
				Dock(chartDockControl, chartDockControl.Location);
			}
			m_allIsSet = true;
		}
	}

	private void Control_SizeChanged(object sender, EventArgs e)
	{
		IChartDockControl chartDockControl = sender as IChartDockControl;
		if (chartDockControl.Position != ChartDock.Floating && (chartDockControl.Behavior & ChartDockingFlags.Dockable) == ChartDockingFlags.Dockable && !m_supressEventProcessing)
		{
			RaiseSizeChanged(e);
		}
	}
}
