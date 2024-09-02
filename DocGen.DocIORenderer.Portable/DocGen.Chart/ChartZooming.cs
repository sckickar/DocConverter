using System.ComponentModel;
using DocGen.Chart.Drawing;
using DocGen.Drawing;

namespace DocGen.Chart;

[TypeConverter(typeof(ExpandableObjectConverter))]
internal sealed class ChartZooming
{
	private float m_opacity = 0.5f;

	private bool m_showBorder;

	private LineInfo m_border = new LineInfo();

	private BrushInfo m_interior = new BrushInfo(SystemColors.Highlight);

	[DefaultValue(0.5f)]
	[Description("Indicates the opacity of zooming selection.")]
	public float Opacity
	{
		get
		{
			return m_opacity;
		}
		set
		{
			if (m_opacity != value)
			{
				m_opacity = value;
			}
		}
	}

	[DefaultValue(false)]
	[Description("Indicates the border visibility of zooming selection.")]
	public bool ShowBorder
	{
		get
		{
			return m_showBorder;
		}
		set
		{
			if (m_showBorder != value)
			{
				m_showBorder = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("Contains the border attributes of zooming selection.")]
	public LineInfo Border
	{
		get
		{
			return m_border;
		}
		set
		{
			if (m_border != value)
			{
				m_border = value;
			}
		}
	}

	[Description("Indicates the interior of zooming selection.")]
	public BrushInfo Interior
	{
		get
		{
			return m_interior;
		}
		set
		{
			if (m_interior != value)
			{
				m_interior = value;
			}
		}
	}

	private bool ShouldSerializeInterior()
	{
		if (m_interior.Style == BrushStyle.Solid)
		{
			return m_interior.BackColor != SystemColors.Highlight;
		}
		return true;
	}
}
