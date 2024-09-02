using DocGen.Drawing;

namespace DocGen.ChartToImageConverter;

internal class MarkerSetting
{
	private Color m_borderBrush;

	private Color m_fillBrush;

	private int m_markerSize;

	private float m_borderThickness;

	private int m_markerTypeInInt;

	internal Color BorderBrush
	{
		get
		{
			return m_borderBrush;
		}
		set
		{
			m_borderBrush = value;
		}
	}

	internal Color FillBrush
	{
		get
		{
			return m_fillBrush;
		}
		set
		{
			m_fillBrush = value;
		}
	}

	internal int MarkerSize
	{
		get
		{
			return m_markerSize;
		}
		set
		{
			m_markerSize = value;
		}
	}

	internal float BorderThickness
	{
		get
		{
			return m_borderThickness;
		}
		set
		{
			m_borderThickness = value;
		}
	}

	internal int MarkerTypeInInt
	{
		get
		{
			return m_markerTypeInInt;
		}
		set
		{
			m_markerTypeInInt = value;
		}
	}

	internal MarkerSetting()
	{
	}
}
