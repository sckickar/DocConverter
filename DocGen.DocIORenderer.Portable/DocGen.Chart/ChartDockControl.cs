using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartDockControl : Control
{
	protected ChartDock m_position = ChartDock.Top;

	protected ChartAlignment m_alignment = ChartAlignment.Center;

	protected ChartOrientation m_orientation;

	protected bool m_dockingFree;

	private Control m_control;

	private ChartDockingFlags m_behaviour = ChartDockingFlags.All;

	public List<Control> Controls { get; set; }

	[DefaultValue(ChartDock.Top)]
	[Description("Indicates the docking position of the control")]
	public override ChartDock Position
	{
		get
		{
			return m_position;
		}
		set
		{
			if (m_position != value)
			{
				m_position = value;
				SetOrientationByPosition();
				RaiseChartDockChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(ChartAlignment.Center)]
	[Description("Indicates the alignment of control inside the Chart")]
	public override ChartAlignment Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			if (m_alignment != value)
			{
				m_alignment = value;
				RaiseChartAlignmentChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(ChartOrientation.Horizontal)]
	[Description("Indicates the orientation of control")]
	public new virtual ChartOrientation Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			if (m_orientation != value && (m_position == ChartDock.Floating || m_dockingFree))
			{
				m_orientation = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("Indicates if the control should be docked inside the Chart")]
	public bool DockingFree
	{
		get
		{
			return (m_behaviour & ChartDockingFlags.Dockable) != ChartDockingFlags.Dockable;
		}
		set
		{
			bool flag = (m_behaviour & ChartDockingFlags.Dockable) != ChartDockingFlags.Dockable;
			if (value != flag)
			{
				if (value)
				{
					Behavior ^= ChartDockingFlags.Dockable;
				}
				else
				{
					Behavior |= ChartDockingFlags.Dockable;
				}
			}
		}
	}

	[DefaultValue(ChartDockingFlags.All)]
	[Description("Indicates behaviour of the dock control")]
	public override ChartDockingFlags Behavior
	{
		get
		{
			return m_behaviour;
		}
		set
		{
			if (m_behaviour != value)
			{
				m_behaviour = value;
				if ((m_behaviour & ChartDockingFlags.Dockable) == 0)
				{
					SetOrientationByPosition();
				}
				RaiseChartDockChanged(EventArgs.Empty);
			}
		}
	}

	public new event LocationEventHandler LocationChanging;

	public new event EventHandler ChartDockChanged;

	public new event EventHandler ChartAlignmentChanged;

	protected bool IsVisible()
	{
		if (Visible)
		{
			return true;
		}
		MethodInfo method = GetType().GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			return Visible;
		}
		return Convert.ToBoolean(method.Invoke(this, new object[1] { 2 }));
	}

	public ChartDockControl()
	{
	}

	public ChartDockControl(Control control)
	{
		m_control = control;
		base.Size = m_control.Size;
		base.Location = m_control.Location;
		Controls.Add(m_control);
	}

	public virtual SizeF Measure(SizeF size)
	{
		return base.Size;
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		RaiseChartDockChanged(EventArgs.Empty);
		base.OnVisibleChanged(e);
	}

	private void Control_SizeChanged(object sender, EventArgs e)
	{
		base.Size = m_control.Size;
	}

	protected void SetOrientationByPosition()
	{
		switch (Position)
		{
		case ChartDock.Left:
		case ChartDock.Right:
			m_orientation = ChartOrientation.Vertical;
			break;
		case ChartDock.Top:
		case ChartDock.Bottom:
			m_orientation = ChartOrientation.Horizontal;
			break;
		case ChartDock.Floating:
			break;
		}
	}

	private void RaiseChartDockChanged(EventArgs e)
	{
		if (this.ChartDockChanged != null)
		{
			this.ChartDockChanged(this, e);
		}
	}

	private void RaiseChartAlignmentChanged(EventArgs e)
	{
		if (this.ChartAlignmentChanged != null)
		{
			this.ChartAlignmentChanged(this, e);
		}
	}

	protected Point CheckLocation(Point pt)
	{
		if (base.Parent != null)
		{
			if (pt.X > base.Parent.Width - base.Width)
			{
				pt = new Point(base.Parent.Width - base.Width, pt.Y);
			}
			if (pt.Y > base.Parent.Height - base.Height)
			{
				pt = new Point(pt.X, base.Parent.Height - base.Height);
			}
			if (pt.X < 0)
			{
				pt = new Point(0, pt.Y);
			}
			if (pt.Y < 0)
			{
				pt = new Point(pt.X, 0);
			}
		}
		return pt;
	}
}
