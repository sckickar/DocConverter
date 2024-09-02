using System.ComponentModel;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartPieConfigItem : ChartConfigItem
{
	private float angleOffset;

	private ChartPieFillMode m_mode;

	private ChartPieType m_type;

	private ColorBlend m_colorGradient;

	private bool m_heightByAreaDepth;

	private bool m_showSeriesTitle;

	private ChartAccumulationLabelStyle m_labelStyle = ChartAccumulationLabelStyle.Outside;

	private float m_heightCoef = 0.2f;

	private float m_doughnutCoef;

	private bool m_pieWithSameRadius;

	private float m_pieRadius;

	private float m_pieTilt = 0.2f;

	private float m_pieHeight = 10f;

	private SizeF m_pieSize = SizeF.Empty;

	private bool m_showDataBindLabels;

	[DefaultValue(0f)]
	public float AngleOffset
	{
		get
		{
			return angleOffset;
		}
		set
		{
			if (angleOffset != value)
			{
				angleOffset = value;
				RaisePropertyChanged("AngleOffset");
			}
		}
	}

	[DefaultValue(false)]
	public bool PieWithSameRadius
	{
		get
		{
			return m_pieWithSameRadius;
		}
		set
		{
			if (m_pieWithSameRadius != value)
			{
				m_pieWithSameRadius = value;
				RaisePropertyChanged("PieWithSameRadius");
			}
		}
	}

	[DefaultValue(0f)]
	public float PieRadius
	{
		get
		{
			return m_pieRadius;
		}
		set
		{
			if (m_pieRadius != value)
			{
				m_pieRadius = value;
				RaisePropertyChanged("PieRadius");
			}
		}
	}

	[DefaultValue(0.2f)]
	public float PieTilt
	{
		get
		{
			return m_pieTilt;
		}
		set
		{
			if (m_pieTilt != value)
			{
				m_pieTilt = value;
				RaisePropertyChanged("PieTilt");
			}
		}
	}

	[DefaultValue(10f)]
	public float PieHeight
	{
		get
		{
			return m_pieHeight;
		}
		set
		{
			if (m_pieHeight != value)
			{
				m_pieHeight = value;
				RaisePropertyChanged("PieHeight");
			}
		}
	}

	public SizeF PieSize
	{
		get
		{
			return m_pieSize;
		}
		set
		{
			if (!(m_pieSize == value))
			{
				m_pieSize = value;
				RaisePropertyChanged("PiesSize");
			}
		}
	}

	[DefaultValue(ChartPieType.None)]
	public ChartPieType PieType
	{
		get
		{
			return m_type;
		}
		set
		{
			if (m_type != value)
			{
				m_type = value;
				RaisePropertyChanged("Type");
			}
		}
	}

	[DefaultValue(ChartPieFillMode.AllPie)]
	public ChartPieFillMode FillMode
	{
		get
		{
			return m_mode;
		}
		set
		{
			if (m_mode != value)
			{
				m_mode = value;
				RaisePropertyChanged("Mode");
			}
		}
	}

	[DefaultValue(null)]
	public ColorBlend Gradient
	{
		get
		{
			return m_colorGradient;
		}
		set
		{
			if (m_colorGradient != value)
			{
				m_colorGradient = value;
				RaisePropertyChanged("Gradient");
			}
		}
	}

	[DefaultValue(false)]
	public bool HeightByAreaDepth
	{
		get
		{
			return m_heightByAreaDepth;
		}
		set
		{
			if (m_heightByAreaDepth != value)
			{
				m_heightByAreaDepth = value;
				RaisePropertyChanged("HeightByAreaDepth");
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

	[DefaultValue(0.2f)]
	public float HeightCoeficient
	{
		get
		{
			return m_heightCoef;
		}
		set
		{
			if (m_heightCoef != value)
			{
				m_heightCoef = value;
				RaisePropertyChanged("HeightCoeficient");
			}
		}
	}

	[DefaultValue(0f)]
	public float DoughnutCoeficient
	{
		get
		{
			return m_doughnutCoef;
		}
		set
		{
			if (m_doughnutCoef != value)
			{
				m_doughnutCoef = value;
				RaisePropertyChanged("DoughnutCoeficient");
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
