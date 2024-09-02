using DocGen.OfficeChart;

namespace DocGen.Drawing;

internal class StyleOrFontReference
{
	private int m_index;

	private ColorModel m_colorModelType;

	private string m_colorValue;

	private double m_lumModValue;

	private double m_lumOffValue1;

	private double m_lumOffValue2;

	private double m_shadeValue;

	internal int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	internal ColorModel ColorModelType
	{
		get
		{
			return m_colorModelType;
		}
		set
		{
			m_colorModelType = value;
		}
	}

	internal string ColorValue
	{
		get
		{
			return m_colorValue;
		}
		set
		{
			m_colorValue = value;
		}
	}

	internal double LumModValue
	{
		get
		{
			return m_lumModValue;
		}
		set
		{
			m_lumModValue = value;
		}
	}

	internal double LumOffValue1
	{
		get
		{
			return m_lumOffValue1;
		}
		set
		{
			m_lumOffValue1 = value;
		}
	}

	internal double LumOffValue2
	{
		get
		{
			return m_lumOffValue2;
		}
		set
		{
			m_lumOffValue2 = value;
		}
	}

	internal double ShadeValue
	{
		get
		{
			return m_shadeValue;
		}
		set
		{
			m_shadeValue = value;
		}
	}

	internal StyleOrFontReference(int index, ColorModel colorModel, string colorValue, double lumModValue, double lumOffValue1, double lumOffValue2, double shadeValue)
	{
		m_index = index;
		m_colorModelType = colorModel;
		m_colorValue = colorValue;
		m_lumModValue = lumModValue;
		m_lumOffValue1 = lumOffValue1;
		m_lumOffValue2 = lumOffValue2;
		m_shadeValue = shadeValue;
	}
}
