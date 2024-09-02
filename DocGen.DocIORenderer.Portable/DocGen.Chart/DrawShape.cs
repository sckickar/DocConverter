using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class DrawShape
{
	private Size m_size = Size.Empty;

	private Color m_color = Color.White;

	private Color m_textColor = Color.Black;

	private string m_text = "";

	private ChartFontInfo m_font = new ChartFontInfo();

	private ChartLineInfo m_border = new ChartLineInfo();

	private ChartTextOrientation m_position = ChartTextOrientation.Up;

	private ChartCustomShape m_type = ChartCustomShape.Square;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Appearance")]
	public ChartFontInfo Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[DefaultValue("Square")]
	[Description("The style of the shape to be displayed. It will support the limitted shape(Square, Circle, Hexagon, Pentagon) draw around the custom point")]
	public ChartCustomShape Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	[DefaultValue("")]
	[Category("Appearance")]
	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (m_text != value)
			{
				m_text = value;
			}
		}
	}

	[DefaultValue(typeof(Color), "Shape Background color")]
	[Category("Appearance")]
	public Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			if (m_color != value)
			{
				m_color = value;
			}
		}
	}

	[DefaultValue(typeof(Color), "Shape text color")]
	[Category("Appearance")]
	public Color TextColor
	{
		get
		{
			return m_textColor;
		}
		set
		{
			if (m_textColor != value)
			{
				m_textColor = value;
			}
		}
	}

	[DefaultValue(typeof(Size), "Set shape size")]
	[Category("Appearance")]
	public Size Size
	{
		get
		{
			return m_size;
		}
		set
		{
			if (m_size != value)
			{
				m_size = value;
			}
		}
	}

	[DefaultValue(typeof(ChartLineInfo), "Draw border of shape")]
	[Category("Appearance")]
	public ChartLineInfo Border
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

	[DefaultValue(ChartTextOrientation.Up)]
	[Category("Appearance")]
	public ChartTextOrientation Position
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
			}
		}
	}

	public void Dispose()
	{
		m_font.Dispose();
		m_border.Dispose();
		m_font = null;
		m_border = null;
	}
}
