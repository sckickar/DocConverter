using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartManualLayoutImpl : CommonObject, IOfficeChartManualLayout, IParentApplication
{
	protected ChartLayoutImpl m_layout;

	protected object m_Parent;

	private ChartAttachedLabelLayoutRecord m_atachedLabelLayout;

	private ChartPlotAreaLayoutRecord m_plotAreaLayout;

	protected LayoutTargets m_layoutTarget;

	protected LayoutModes m_leftMode;

	protected LayoutModes m_topMode;

	protected double m_left;

	protected double m_top;

	protected double m_dX;

	protected double m_dY;

	protected LayoutModes m_widthMode;

	protected LayoutModes m_heightMode;

	protected double m_width;

	protected double m_height;

	protected int m_xTL;

	protected int m_yTL;

	protected int m_xBR;

	protected int m_yBR;

	private byte m_flagOptions;

	public new object Parent => m_Parent;

	public ChartAttachedLabelLayoutRecord AttachedLabelLayout
	{
		get
		{
			if (m_atachedLabelLayout == null)
			{
				m_atachedLabelLayout = new ChartAttachedLabelLayoutRecord();
			}
			return m_atachedLabelLayout;
		}
		set
		{
			m_atachedLabelLayout = value;
		}
	}

	public ChartPlotAreaLayoutRecord PlotAreaLayout
	{
		get
		{
			if (m_plotAreaLayout == null)
			{
				m_plotAreaLayout = new ChartPlotAreaLayoutRecord();
			}
			return m_plotAreaLayout;
		}
		set
		{
			m_plotAreaLayout = value;
		}
	}

	public LayoutTargets LayoutTarget
	{
		get
		{
			return m_layoutTarget;
		}
		set
		{
			m_layoutTarget = value;
			m_flagOptions |= 16;
		}
	}

	public LayoutModes LeftMode
	{
		get
		{
			return m_leftMode;
		}
		set
		{
			m_leftMode = value;
			if (Parent is ChartLayoutImpl)
			{
				if ((Parent as ChartLayoutImpl).Parent is ChartTextAreaImpl || (Parent as ChartLayoutImpl).Parent is ChartLegendImpl || (Parent as ChartLayoutImpl).Parent is ChartDataLabelsImpl)
				{
					AttachedLabelLayout.WXMode = value;
				}
				else if ((Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
				{
					PlotAreaLayout.WXMode = value;
				}
			}
		}
	}

	public LayoutModes TopMode
	{
		get
		{
			return m_topMode;
		}
		set
		{
			m_topMode = value;
			if (Parent is ChartLayoutImpl)
			{
				if ((Parent as ChartLayoutImpl).Parent is ChartTextAreaImpl || (Parent as ChartLayoutImpl).Parent is ChartLegendImpl || (Parent as ChartLayoutImpl).Parent is ChartDataLabelsImpl)
				{
					AttachedLabelLayout.WYMode = value;
				}
				else if ((Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
				{
					PlotAreaLayout.WYMode = value;
				}
			}
		}
	}

	public double Left
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = value;
			if (Parent is ChartLayoutImpl)
			{
				if ((Parent as ChartLayoutImpl).Parent is ChartTextAreaImpl || (Parent as ChartLayoutImpl).Parent is ChartLegendImpl || (Parent as ChartLayoutImpl).Parent is ChartDataLabelsImpl)
				{
					AttachedLabelLayout.X = value;
				}
				else if ((Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
				{
					PlotAreaLayout.X = value;
				}
			}
			m_flagOptions |= 2;
		}
	}

	public double Top
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = value;
			if (Parent is ChartLayoutImpl)
			{
				if ((Parent as ChartLayoutImpl).Parent is ChartTextAreaImpl || (Parent as ChartLayoutImpl).Parent is ChartLegendImpl || (Parent as ChartLayoutImpl).Parent is ChartDataLabelsImpl)
				{
					AttachedLabelLayout.Y = value;
				}
				else if ((Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
				{
					PlotAreaLayout.Y = value;
				}
			}
			m_flagOptions |= 1;
		}
	}

	public double dX
	{
		get
		{
			return m_dX;
		}
		set
		{
			m_dX = value;
			if (Parent is ChartLayoutImpl)
			{
				if ((Parent as ChartLayoutImpl).Parent is ChartTextAreaImpl || (Parent as ChartLayoutImpl).Parent is ChartLegendImpl)
				{
					AttachedLabelLayout.Dx = value;
				}
				else if ((Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
				{
					PlotAreaLayout.Dx = value;
				}
			}
		}
	}

	public double dY
	{
		get
		{
			return m_dY;
		}
		set
		{
			m_dY = value;
			if (Parent is ChartLayoutImpl)
			{
				if ((Parent as ChartLayoutImpl).Parent is ChartTextAreaImpl || (Parent as ChartLayoutImpl).Parent is ChartLegendImpl)
				{
					AttachedLabelLayout.Dy = value;
				}
				else if ((Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
				{
					PlotAreaLayout.Dy = value;
				}
			}
		}
	}

	public LayoutModes WidthMode
	{
		get
		{
			return m_widthMode;
		}
		set
		{
			m_widthMode = value;
			if (Parent is ChartLayoutImpl)
			{
				if ((Parent as ChartLayoutImpl).Parent is ChartTextAreaImpl || (Parent as ChartLayoutImpl).Parent is ChartLegendImpl || (Parent as ChartLayoutImpl).Parent is ChartDataLabelsImpl)
				{
					AttachedLabelLayout.WWidthMode = value;
				}
				else if ((Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
				{
					PlotAreaLayout.WWidthMode = value;
				}
			}
		}
	}

	public LayoutModes HeightMode
	{
		get
		{
			return m_heightMode;
		}
		set
		{
			m_heightMode = value;
			if (Parent is ChartLayoutImpl)
			{
				if ((Parent as ChartLayoutImpl).Parent is ChartTextAreaImpl || (Parent as ChartLayoutImpl).Parent is ChartLegendImpl || (Parent as ChartLayoutImpl).Parent is ChartDataLabelsImpl)
				{
					AttachedLabelLayout.WHeightMode = value;
				}
				else if ((Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
				{
					PlotAreaLayout.WHeightMode = value;
				}
			}
		}
	}

	public double Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
			m_flagOptions |= 8;
		}
	}

	public double Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
			m_flagOptions |= 4;
		}
	}

	public int xTL
	{
		get
		{
			return m_xTL;
		}
		set
		{
			m_xTL = value;
			if (Parent is ChartLayoutImpl && (Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
			{
				PlotAreaLayout.xTL = value;
			}
		}
	}

	public int yTL
	{
		get
		{
			return m_yTL;
		}
		set
		{
			m_yTL = value;
			if (Parent is ChartLayoutImpl && (Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
			{
				PlotAreaLayout.yTL = value;
			}
		}
	}

	public int xBR
	{
		get
		{
			return m_xBR;
		}
		set
		{
			m_xBR = value;
			if (Parent is ChartLayoutImpl && (Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
			{
				PlotAreaLayout.xBR = value;
			}
		}
	}

	public int yBR
	{
		get
		{
			return m_yBR;
		}
		set
		{
			m_yBR = value;
			if (Parent is ChartLayoutImpl && (Parent as ChartLayoutImpl).Parent is ChartPlotAreaImpl)
			{
				PlotAreaLayout.yBR = value;
			}
		}
	}

	internal byte FlagOptions
	{
		get
		{
			return m_flagOptions;
		}
		set
		{
			m_flagOptions = value;
		}
	}

	public ChartManualLayoutImpl(IApplication application, object parent)
		: this(application, parent, bAutoSize: false, bIsInteriorGrey: false, bSetDefaults: true)
	{
	}

	public ChartManualLayoutImpl(IApplication application, object parent, bool bSetDefaults)
		: this(application, parent, bAutoSize: false, bIsInteriorGrey: false, bSetDefaults)
	{
	}

	public ChartManualLayoutImpl(IApplication application, object parent, bool bAutoSize, bool bIsInteriorGrey, bool bSetDefaults)
		: base(application, parent)
	{
		SetParents(parent);
	}

	public ChartManualLayoutImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent)
	{
		SetParents(parent);
	}

	private void SetParents(object parent)
	{
		m_layout = FindParent(typeof(ChartLayoutImpl)) as ChartLayoutImpl;
		m_Parent = parent;
		if (m_layout == null)
		{
			throw new ArgumentNullException("Can't find parent chart");
		}
	}

	public void SetDefaultValues()
	{
	}
}
