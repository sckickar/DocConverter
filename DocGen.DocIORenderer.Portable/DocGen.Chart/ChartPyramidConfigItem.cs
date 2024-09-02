using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartPyramidConfigItem : ChartConfigItem
{
	private ChartPyramidMode m_pyramidMode;

	private ChartAccumulationLabelPlacement m_labelPlacement = ChartAccumulationLabelPlacement.Right;

	private ChartAccumulationLabelStyle m_labelStyle = ChartAccumulationLabelStyle.Outside;

	private float m_gapRatio;

	private ChartFigureBase m_figureBase;

	private bool m_showSeriesTitle;

	private bool m_showDataBindLabels;

	[DefaultValue(ChartPyramidMode.Linear)]
	public ChartPyramidMode PyramidMode
	{
		get
		{
			return m_pyramidMode;
		}
		set
		{
			if (m_pyramidMode != value)
			{
				m_pyramidMode = value;
				RaisePropertyChanged("PyramidMode");
			}
		}
	}

	[DefaultValue(ChartAccumulationLabelPlacement.Right)]
	public ChartAccumulationLabelPlacement LabelPlacement
	{
		get
		{
			return m_labelPlacement;
		}
		set
		{
			if (m_labelPlacement != value)
			{
				m_labelPlacement = value;
				RaisePropertyChanged("LabelPlacement");
			}
		}
	}

	[DefaultValue(ChartAccumulationLabelStyle.Outside)]
	public ChartAccumulationLabelStyle LabelStyle
	{
		get
		{
			return m_labelStyle;
		}
		set
		{
			if (m_labelStyle != value)
			{
				m_labelStyle = value;
				RaisePropertyChanged("LabelStyle");
			}
		}
	}

	[DefaultValue(0f)]
	public float GapRatio
	{
		get
		{
			return m_gapRatio;
		}
		set
		{
			if (m_gapRatio != value)
			{
				m_gapRatio = value;
				RaisePropertyChanged("GapRatio");
			}
		}
	}

	[DefaultValue(ChartFigureBase.Square)]
	public ChartFigureBase FigureBase
	{
		get
		{
			return m_figureBase;
		}
		set
		{
			if (m_figureBase != value)
			{
				m_figureBase = value;
				RaisePropertyChanged("FigureBase");
			}
		}
	}

	[DefaultValue(false)]
	public bool ShowSeriesTitle
	{
		get
		{
			return m_showSeriesTitle;
		}
		set
		{
			if (m_showSeriesTitle != value)
			{
				m_showSeriesTitle = value;
				RaisePropertyChanged("ShowSeriesTitle");
			}
		}
	}

	[Description("Gets or sets a value indicating whether databind labels are displayed.")]
	[DefaultValue(false)]
	public bool ShowDataBindLabels
	{
		get
		{
			return m_showDataBindLabels;
		}
		set
		{
			if (m_showDataBindLabels != value)
			{
				m_showDataBindLabels = value;
				RaisePropertyChanged("ShowDataBindLabels");
			}
		}
	}
}
