using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

internal sealed class ChartHeatMapConfigItem : ChartConfigItem
{
	private bool m_displayColorSwatch = true;

	private Color m_lowestValueColor = Color.Red;

	private Color m_middleValueColor = Color.Yellow;

	private Color m_highestValueColor = Color.Blue;

	private string m_startText = "";

	private string m_endText = "";

	private float m_labelMargins = 2f;

	private bool m_displayTitle = true;

	private int m_maximumCharacters = -1;

	private bool m_enableLabelsTruncation;

	private bool m_enableLabelRotation = true;

	private bool m_allowLabelsAutoFit = true;

	private float m_minimumFontSize = 6f;

	private bool m_showLargeLabels;

	private ChartHeatMapLayoutStyle m_heatMapStyle;

	[DefaultValue(true)]
	[Description("Indicates whether color swatch is displayed")]
	public bool DisplayColorSwatch
	{
		get
		{
			return m_displayColorSwatch;
		}
		set
		{
			if (m_displayColorSwatch != value)
			{
				m_displayColorSwatch = value;
				RaisePropertyChanged("DisplayColorSwatch");
			}
		}
	}

	[DefaultValue(typeof(Color), "Red")]
	[Description("The color of the lowest value")]
	public Color LowestValueColor
	{
		get
		{
			return m_lowestValueColor;
		}
		set
		{
			if (m_lowestValueColor != value)
			{
				m_lowestValueColor = value;
				RaisePropertyChanged("LowestValueColor");
			}
		}
	}

	[DefaultValue(typeof(Color), "Yellow")]
	[Description("The color of the middle value")]
	public Color MiddleValueColor
	{
		get
		{
			return m_middleValueColor;
		}
		set
		{
			if (m_middleValueColor != value)
			{
				m_middleValueColor = value;
				RaisePropertyChanged("MiddleValueColor");
			}
		}
	}

	[DefaultValue(typeof(Color), "Blue")]
	[Description("The color of the highest value")]
	public Color HighestValueColor
	{
		get
		{
			return m_highestValueColor;
		}
		set
		{
			if (m_highestValueColor != value)
			{
				m_highestValueColor = value;
				RaisePropertyChanged("HighestValueColor");
			}
		}
	}

	[DefaultValue("")]
	[Description("The \"start\" text.")]
	public string StartText
	{
		get
		{
			return m_startText;
		}
		set
		{
			if (m_startText != value)
			{
				m_startText = value;
				RaisePropertyChanged("StartText");
			}
		}
	}

	[DefaultValue("")]
	[Description("The \"end\" text.")]
	public string EndText
	{
		get
		{
			return m_endText;
		}
		set
		{
			if (m_endText != value)
			{
				m_endText = value;
				RaisePropertyChanged("EndText");
			}
		}
	}

	[DefaultValue(2f)]
	[Description("The text margins.")]
	public float LabelMargins
	{
		get
		{
			return m_labelMargins;
		}
		set
		{
			if (m_labelMargins != value)
			{
				m_labelMargins = value;
				RaisePropertyChanged("LabelMargins");
			}
		}
	}

	[DefaultValue(true)]
	[Description("Indicates whether title is displayed")]
	public bool DisplayTitle
	{
		get
		{
			return m_displayTitle;
		}
		set
		{
			if (m_displayTitle != value)
			{
				m_displayTitle = value;
				RaisePropertyChanged("DisplayTitle");
			}
		}
	}

	[DefaultValue(-1)]
	[Description("The maximal number of label characters.")]
	public int MaximumCharacters
	{
		get
		{
			return m_maximumCharacters;
		}
		set
		{
			if (m_maximumCharacters != value)
			{
				m_maximumCharacters = value;
				RaisePropertyChanged("MaximumCharacters");
			}
		}
	}

	[DefaultValue(false)]
	[Description("Indicates whether the large labels should be truncated.")]
	public bool EnableLabelsTruncation
	{
		get
		{
			return m_enableLabelsTruncation;
		}
		set
		{
			if (m_enableLabelsTruncation != value)
			{
				m_enableLabelsTruncation = value;
				RaisePropertyChanged("EnableLabelsTruncation");
			}
		}
	}

	[DefaultValue(true)]
	[Description("Indicates whether is allowed to rotation labels")]
	public bool EnableLabelRotation
	{
		get
		{
			return m_enableLabelRotation;
		}
		set
		{
			if (m_enableLabelRotation != value)
			{
				m_enableLabelRotation = value;
				RaisePropertyChanged("EnableLabelRotation");
			}
		}
	}

	[DefaultValue(true)]
	[Description("Indicates whether labels auto fit is enabled")]
	public bool AllowLabelsAutoFit
	{
		get
		{
			return m_allowLabelsAutoFit;
		}
		set
		{
			if (m_allowLabelsAutoFit != value)
			{
				m_allowLabelsAutoFit = value;
				RaisePropertyChanged("AllowLabelsAutoFit");
			}
		}
	}

	[DefaultValue(6f)]
	[Description("The minimal size of the font")]
	public float MinimumFontSize
	{
		get
		{
			return m_minimumFontSize;
		}
		set
		{
			if (m_minimumFontSize != value)
			{
				m_minimumFontSize = value;
				RaisePropertyChanged("MinimumFontSize");
			}
		}
	}

	[DefaultValue(false)]
	[Description("Indicates whether the large labels should be display")]
	public bool ShowLargeLabels
	{
		get
		{
			return m_showLargeLabels;
		}
		set
		{
			if (m_showLargeLabels != value)
			{
				m_showLargeLabels = value;
				RaisePropertyChanged("ShowLargeLabels");
			}
		}
	}

	[DefaultValue(ChartHeatMapLayoutStyle.Rectangular)]
	[Description("The HeatMap layout style.")]
	public ChartHeatMapLayoutStyle HeatMapStyle
	{
		get
		{
			return m_heatMapStyle;
		}
		set
		{
			if (m_heatMapStyle != value)
			{
				m_heatMapStyle = value;
				RaisePropertyChanged("HeatMapStyle");
			}
		}
	}
}
