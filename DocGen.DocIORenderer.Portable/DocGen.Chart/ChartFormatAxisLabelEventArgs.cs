using System;

namespace DocGen.Chart;

internal class ChartFormatAxisLabelEventArgs : EventArgs
{
	private ChartAxis m_axis;

	private bool m_handled;

	private bool m_isPrimary;

	private string m_label;

	private string m_toolTip;

	private double m_value;

	private ChartPlacement m_axisLabelPlacement;

	public ChartOrientation AxisOrientation => m_axis.Orientation;

	public bool Handled
	{
		get
		{
			return m_handled;
		}
		set
		{
			m_handled = value;
		}
	}

	public bool IsAxisPrimary => m_isPrimary;

	public string Label
	{
		get
		{
			return m_label;
		}
		set
		{
			m_label = value;
		}
	}

	public string ToolTip
	{
		get
		{
			return m_toolTip;
		}
		set
		{
			m_toolTip = value;
		}
	}

	public double Value => m_value;

	public DateTime ValueAsDate => DateTime.FromOADate(m_value);

	public ChartAxis Axis => m_axis;

	public ChartPlacement AxisLabelPlacement
	{
		get
		{
			return m_axisLabelPlacement;
		}
		set
		{
			m_axisLabelPlacement = value;
		}
	}

	public ChartFormatAxisLabelEventArgs(string label, double value, ChartAxis axis)
	{
		m_axis = axis;
		m_label = label;
		m_value = value;
		m_isPrimary = axis.Primary;
		m_axisLabelPlacement = axis.AxisLabelPlacement;
		m_handled = false;
	}
}
