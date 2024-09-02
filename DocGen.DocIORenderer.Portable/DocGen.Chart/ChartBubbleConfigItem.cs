using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartBubbleConfigItem : ChartConfigItem
{
	private RectangleF minRect;

	private RectangleF maxRect;

	private ChartBubbleType bubbleType;

	private bool m_enablePhongStyle = true;

	public RectangleF MinBounds
	{
		get
		{
			return minRect;
		}
		set
		{
			if (!(minRect == value))
			{
				minRect = value;
				RaisePropertyChanged("MinBounds");
			}
		}
	}

	public RectangleF MaxBounds
	{
		get
		{
			return maxRect;
		}
		set
		{
			if (!(maxRect == value))
			{
				maxRect = value;
				RaisePropertyChanged("MaxBounds");
			}
		}
	}

	[DefaultValue(ChartBubbleType.Circle)]
	public ChartBubbleType BubbleType
	{
		get
		{
			return bubbleType;
		}
		set
		{
			if (bubbleType != value)
			{
				bubbleType = value;
				RaisePropertyChanged("BubbleType");
			}
		}
	}

	[DefaultValue(true)]
	public bool EnablePhongStyle
	{
		get
		{
			return m_enablePhongStyle;
		}
		set
		{
			if (m_enablePhongStyle != value)
			{
				m_enablePhongStyle = value;
				RaisePropertyChanged("BubbleType");
			}
		}
	}

	public ChartBubbleConfigItem()
	{
		minRect = new RectangleF(PointF.Empty, new SizeF(25f, 25f));
		maxRect = new RectangleF(PointF.Empty, new SizeF(50f, 50f));
	}
}
