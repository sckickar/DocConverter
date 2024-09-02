using System;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartAxisLabel : ICloneable
{
	private static Font c_defualtFont = new Font("Verdana", 10f);

	private string m_labelCustomText;

	private string m_labelToolTip;

	private Font m_labelFont;

	private Color m_labelColor = Color.Black;

	private double m_labelDoubleValue;

	private double m_labelLogBase = 2.0;

	private string m_labelFormat;

	private string m_labelDateTimeFormat;

	private ChartValueType m_labelValueType;

	private int m_labelRoundingPlaces = 2;

	private RectangleF m_bounds = RectangleF.Empty;

	private RectangleF m_clientRect = RectangleF.Empty;

	private float m_angle;

	private bool m_isAuto;

	private string m_text;

	internal ChartPlacement m_axisLabelPlacement = ChartPlacement.Outside;

	public string CustomText
	{
		get
		{
			return m_labelCustomText;
		}
		set
		{
			if (m_labelCustomText != value)
			{
				m_labelCustomText = value;
				m_text = null;
			}
		}
	}

	public string ToolTip
	{
		get
		{
			if (m_labelToolTip != null)
			{
				return m_labelToolTip;
			}
			return Text;
		}
		set
		{
			if (m_labelToolTip != value)
			{
				m_labelToolTip = value;
			}
		}
	}

	public string DateTimeFormat
	{
		get
		{
			return m_labelDateTimeFormat;
		}
		set
		{
			if (m_labelDateTimeFormat != value)
			{
				m_labelDateTimeFormat = value;
				m_text = null;
			}
		}
	}

	public string Format
	{
		get
		{
			return m_labelFormat;
		}
		set
		{
			if (m_labelFormat != value)
			{
				m_labelFormat = value;
				m_text = null;
			}
		}
	}

	public ChartPlacement AxisLabelPlacement
	{
		get
		{
			return m_axisLabelPlacement;
		}
		set
		{
			if (m_axisLabelPlacement != value)
			{
				m_axisLabelPlacement = value;
			}
		}
	}

	public double LogBase
	{
		get
		{
			return m_labelLogBase;
		}
		set
		{
			if (m_labelLogBase != value)
			{
				m_labelLogBase = value;
			}
		}
	}

	public double DoubleValue
	{
		get
		{
			return m_labelDoubleValue;
		}
		set
		{
			if (m_labelDoubleValue != value)
			{
				m_labelDoubleValue = value;
				m_text = null;
			}
		}
	}

	public int RoundingPlaces
	{
		get
		{
			return m_labelRoundingPlaces;
		}
		set
		{
			if (m_labelRoundingPlaces != value)
			{
				m_labelRoundingPlaces = value;
				m_text = null;
			}
		}
	}

	public string Text
	{
		get
		{
			if (m_text == null)
			{
				ComputeText();
				if (m_text != CustomText)
				{
					m_text = CustomText;
				}
			}
			return m_text;
		}
	}

	public Font Font
	{
		get
		{
			if (m_labelFont != null)
			{
				return m_labelFont;
			}
			return c_defualtFont;
		}
		set
		{
			if (m_labelFont != value)
			{
				m_labelFont = value;
				m_text = null;
			}
		}
	}

	public Color Color
	{
		get
		{
			if (!m_labelColor.IsEmpty)
			{
				return m_labelColor;
			}
			return Color.Black;
		}
		set
		{
			if (m_labelColor != value)
			{
				m_labelColor = value;
			}
		}
	}

	public ChartValueType ValueType
	{
		get
		{
			return m_labelValueType;
		}
		set
		{
			if (m_labelValueType != value)
			{
				m_labelValueType = value;
				m_text = null;
			}
		}
	}

	internal RectangleF Bounds => m_bounds;

	internal bool IsAuto
	{
		get
		{
			return m_isAuto;
		}
		set
		{
			m_isAuto = value;
		}
	}

	public ChartAxisLabel()
		: this("")
	{
	}

	public ChartAxisLabel(string customText)
		: this(customText, 0.0)
	{
	}

	public ChartAxisLabel(string customText, string toolTip)
		: this(customText, 0.0, toolTip)
	{
	}

	public ChartAxisLabel(string customText, double value)
		: this(customText, Color.Empty, null, value)
	{
	}

	public ChartAxisLabel(string customText, double value, string toolTip)
		: this(customText, Color.Empty, null, value, toolTip)
	{
	}

	public ChartAxisLabel(string customText, Color color, Font font)
		: this(customText, color, font, 0.0)
	{
	}

	public ChartAxisLabel(string customText, Color color, Font font, string toolTip)
		: this(customText, color, font, 0.0, toolTip)
	{
	}

	public ChartAxisLabel(string customText, Color color, Font font, double dvalue)
	{
		m_labelCustomText = customText;
		m_labelColor = color;
		m_labelFont = font;
		m_labelDoubleValue = dvalue;
		m_labelDateTimeFormat = "";
		m_labelValueType = ChartValueType.Custom;
	}

	public ChartAxisLabel(string customText, Color color, Font font, double dvalue, string toolTip)
	{
		m_labelCustomText = customText;
		m_labelToolTip = toolTip;
		m_labelColor = color;
		m_labelFont = font;
		m_labelDoubleValue = dvalue;
		m_labelDateTimeFormat = "";
		m_labelValueType = ChartValueType.Custom;
	}

	public ChartAxisLabel(double dvalue, string format)
	{
		m_labelCustomText = "";
		m_labelColor = Color.Black;
		m_labelFont = c_defualtFont;
		m_labelDoubleValue = dvalue;
		m_labelFormat = format;
		m_labelValueType = ChartValueType.Double;
	}

	public ChartAxisLabel(double dvalue, string format, string toolTip)
	{
		m_labelCustomText = "";
		m_labelToolTip = toolTip;
		m_labelColor = Color.Black;
		m_labelFont = c_defualtFont;
		m_labelDoubleValue = dvalue;
		m_labelFormat = format;
		m_labelValueType = ChartValueType.Double;
	}

	public ChartAxisLabel(DateTime dt, string dateTimeFormat)
	{
		m_labelCustomText = "";
		m_labelColor = Color.Black;
		m_labelFont = c_defualtFont;
		m_labelDoubleValue = dt.ToOADate();
		m_labelDateTimeFormat = dateTimeFormat;
		m_labelValueType = ChartValueType.DateTime;
	}

	public ChartAxisLabel(DateTime dt, string dateTimeFormat, string toolTip)
	{
		m_labelCustomText = "";
		m_labelToolTip = toolTip;
		m_labelColor = Color.Black;
		m_labelFont = c_defualtFont;
		m_labelDoubleValue = dt.ToOADate();
		m_labelDateTimeFormat = dateTimeFormat;
		m_labelValueType = ChartValueType.DateTime;
	}

	public ChartAxisLabel(string customText, Color color, Font font, double dvalue, string format, ChartValueType valueType)
		: this(customText, color, font, dvalue, format, "", valueType)
	{
	}

	public ChartAxisLabel(string customText, string toolTip, Color color, Font font, double dvalue, string format, ChartValueType valueType)
		: this(customText, toolTip, color, font, dvalue, format, "", valueType)
	{
	}

	public ChartAxisLabel(string customText, Color color, Font font, double dvalue, string format, string dateTimeFormat, ChartValueType valueType)
	{
		m_labelCustomText = customText;
		m_labelColor = color;
		m_labelFont = font;
		m_labelDoubleValue = dvalue;
		m_labelFormat = format;
		m_labelDateTimeFormat = dateTimeFormat;
		m_labelValueType = valueType;
	}

	public ChartAxisLabel(string customText, string toolTip, Color color, Font font, double dvalue, string format, string dateTimeFormat, ChartValueType valueType)
	{
		m_labelCustomText = customText;
		m_labelToolTip = toolTip;
		m_labelColor = color;
		m_labelFont = font;
		m_labelDoubleValue = dvalue;
		m_labelFormat = format;
		m_labelDateTimeFormat = dateTimeFormat;
		m_labelValueType = valueType;
	}

	public ChartAxisLabel(string customText, Color color, Font font, DateTime dateTime, string format, string dateTimeFormat, ChartValueType valueType)
	{
		m_labelCustomText = customText;
		m_labelColor = color;
		m_labelFont = font;
		m_labelDoubleValue = dateTime.ToOADate();
		m_labelFormat = format;
		m_labelDateTimeFormat = dateTimeFormat;
		m_labelValueType = valueType;
	}

	public ChartAxisLabel(string customText, string toolTip, Color color, Font font, DateTime dateTime, string format, string dateTimeFormat, ChartValueType valueType)
	{
		m_labelCustomText = customText;
		m_labelToolTip = toolTip;
		m_labelColor = color;
		m_labelFont = font;
		m_labelDoubleValue = dateTime.ToOADate();
		m_labelFormat = format;
		m_labelDateTimeFormat = dateTimeFormat;
		m_labelValueType = valueType;
	}

	public ChartAxisLabel(double dvalue, string format, ChartValueType valueType)
	{
		m_labelCustomText = "";
		m_labelColor = Color.Black;
		m_labelFont = c_defualtFont;
		m_labelDoubleValue = dvalue;
		m_labelFormat = format;
		m_labelDateTimeFormat = "";
		m_labelValueType = valueType;
	}

	public ChartAxisLabel(double dvalue, string format, string toolTip, ChartValueType valueType)
	{
		m_labelCustomText = "";
		m_labelToolTip = toolTip;
		m_labelColor = Color.Black;
		m_labelFont = c_defualtFont;
		m_labelDoubleValue = dvalue;
		m_labelFormat = format;
		m_labelDateTimeFormat = "";
		m_labelValueType = valueType;
	}

	public static ChartAxisLabel InitializeStaticVariables()
	{
		if (c_defualtFont == null)
		{
			c_defualtFont = new Font("Verdana", 10f);
		}
		return new ChartAxisLabel();
	}

	public ChartAxisLabel Clone()
	{
		return new ChartAxisLabel
		{
			m_labelCustomText = m_labelCustomText,
			m_labelToolTip = m_labelToolTip,
			m_labelFont = m_labelFont,
			m_labelColor = m_labelColor,
			m_labelDoubleValue = m_labelDoubleValue,
			m_labelLogBase = m_labelLogBase,
			m_labelFormat = m_labelFormat,
			m_labelDateTimeFormat = m_labelDateTimeFormat,
			m_labelValueType = m_labelValueType,
			m_labelRoundingPlaces = m_labelRoundingPlaces
		};
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	private void ComputeText()
	{
		switch (m_labelValueType)
		{
		case ChartValueType.Double:
			m_text = Math.Round(m_labelDoubleValue, m_labelRoundingPlaces).ToString(m_labelFormat);
			break;
		case ChartValueType.DateTime:
			if (string.IsNullOrEmpty(m_labelDateTimeFormat))
			{
				m_text = DateTime.FromOADate(m_labelDoubleValue).ToShortDateString();
			}
			else
			{
				m_text = DateTime.FromOADate(m_labelDoubleValue).ToString(m_labelDateTimeFormat);
			}
			break;
		case ChartValueType.Custom:
			m_text = m_labelCustomText;
			break;
		case ChartValueType.Logarithmic:
			m_text = Math.Pow(m_labelLogBase, m_labelDoubleValue).ToString(m_labelFormat);
			break;
		case ChartValueType.Category:
			break;
		}
	}

	internal SizeF Measure(Graphics g, ChartAxis axis)
	{
		Font font = ((m_labelFont == null) ? axis.Font : m_labelFont);
		m_clientRect = new RectangleF(PointF.Empty, g.MeasureString(Text, font));
		return m_clientRect.Size;
	}

	internal SizeF Measure(Graphics g, SizeF size, ChartAxis axis)
	{
		Font font = ((m_labelFont == null) ? axis.Font : m_labelFont);
		size = g.MeasureString(Text, font, size, StringFormat.GenericDefault, out var _, out var linesFilled);
		if (linesFilled > 1 && (float)linesFilled * font.GetHeight(g) < size.Height)
		{
			size.Height = font.GetHeight(g) * (float)linesFilled;
		}
		else if (linesFilled == 1)
		{
			size.Height = font.GetHeight(g);
		}
		m_clientRect = new RectangleF(PointF.Empty, size);
		return m_clientRect.Size;
	}

	internal SizeF Measure(Graphics g, float width, ChartAxis axis)
	{
		Font font = ((m_labelFont == null) ? axis.Font : m_labelFont);
		m_clientRect = new RectangleF(PointF.Empty, g.MeasureString(Text, font, (int)width));
		return m_clientRect.Size;
	}

	internal RectangleF Arrange(PointF connectPoint)
	{
		m_angle = 0f;
		return m_bounds = new RectangleF(connectPoint, m_clientRect.Size);
	}

	internal RectangleF Arrange(PointF connectPoint, ContentAlignment aligment)
	{
		m_angle = 0f;
		PointF location = connectPoint;
		SizeF size = m_clientRect.Size;
		switch (aligment)
		{
		case ContentAlignment.BottomCenter:
			location = new PointF(location.X - size.Width / 2f, location.Y);
			break;
		case ContentAlignment.BottomLeft:
			location = new PointF(location.X - size.Width, location.Y);
			break;
		case ContentAlignment.BottomRight:
			location = new PointF(location.X, location.Y);
			break;
		case ContentAlignment.MiddleCenter:
			location = new PointF(location.X - size.Width / 2f, location.Y - size.Height / 2f);
			break;
		case ContentAlignment.MiddleLeft:
			location = new PointF(location.X - size.Width, location.Y - size.Height / 2f);
			break;
		case ContentAlignment.MiddleRight:
			location = new PointF(location.X, location.Y - size.Height / 2f);
			break;
		case ContentAlignment.TopCenter:
			location = new PointF(location.X - size.Width / 2f, location.Y - size.Height);
			break;
		case ContentAlignment.TopLeft:
			location = new PointF(location.X - size.Width, location.Y - size.Height);
			break;
		case ContentAlignment.TopRight:
			location = new PointF(location.X, location.Y - size.Height);
			break;
		}
		return m_bounds = new RectangleF(location, m_clientRect.Size);
	}

	internal RectangleF Arrange(PointF connectPoint, ContentAlignment aligment, float angle, bool m_rotateFromTicks)
	{
		PointF location = connectPoint;
		SizeF size = m_clientRect.Size;
		m_angle = angle;
		if (m_angle != 0f)
		{
			double num = Math.Abs(Math.Cos((double)m_angle * (Math.PI / 180.0)));
			double num2 = Math.Abs(Math.Sin((double)m_angle * (Math.PI / 180.0)));
			double num3 = num * (double)size.Width + num2 * (double)size.Height;
			double num4 = num2 * (double)size.Width + num * (double)size.Height;
			size = new SizeF((float)num3, (float)num4);
		}
		switch (aligment)
		{
		case ContentAlignment.BottomCenter:
			location = new PointF(location.X - size.Width / 2f, location.Y);
			break;
		case ContentAlignment.BottomLeft:
			location = new PointF(location.X - size.Width, location.Y);
			break;
		case ContentAlignment.BottomRight:
			location = (m_rotateFromTicks ? new PointF(location.X, location.Y) : new PointF(location.X, location.Y));
			break;
		case ContentAlignment.MiddleCenter:
			location = new PointF(location.X - size.Width / 2f, location.Y - size.Height / 2f);
			break;
		case ContentAlignment.MiddleLeft:
			location = new PointF(location.X - size.Width, location.Y - size.Height / 2f);
			break;
		case ContentAlignment.MiddleRight:
			location = new PointF(location.X, location.Y - size.Height / 2f);
			break;
		case ContentAlignment.TopCenter:
			location = new PointF(location.X - size.Width / 2f, location.Y - size.Height);
			break;
		case ContentAlignment.TopLeft:
			location = new PointF(location.X - size.Width, location.Y - size.Height);
			break;
		case ContentAlignment.TopRight:
			location = new PointF(location.X, location.Y - size.Height);
			break;
		}
		return m_bounds = new RectangleF(location, size);
	}

	internal void Draw(Graphics g, ChartAxis axis, ChartArea chartArea, RectangleF bounds)
	{
		bool flag = axis.Orientation == ChartOrientation.Horizontal;
		if (bounds.Width > 0f)
		{
			SizeF size = m_clientRect.Size;
			Measure(g, bounds.Width, axis);
			if (!size.Equals(m_clientRect.Size) && !axis.OpposedPosition && !flag)
			{
				m_clientRect.X += size.Width - m_clientRect.Size.Width;
			}
		}
		Draw(g, axis, chartArea);
	}

	internal void Draw(Graphics g, ChartAxis axis, ChartArea chartArea)
	{
		GraphicsContainer cont = DrawingHelper.BeginTransform(g);
		Font font = ((m_labelFont == null) ? axis.Font : m_labelFont);
		Color color = (m_labelColor.IsEmpty ? axis.ForeColor : m_labelColor);
		StringFormat stringFormat = new StringFormat(axis.LabelStringFormat)
		{
			Alignment = ((axis.Orientation == ChartOrientation.Vertical && !axis.OpposedPosition) ? StringAlignment.Far : StringAlignment.Near)
		};
		if (m_angle == 0f)
		{
			if (AxisLabelPlacement == ChartPlacement.Inside && axis.AxisLabelPlacement != AxisLabelPlacement)
			{
				if (axis.Orientation == ChartOrientation.Vertical)
				{
					if (!axis.OpposedPosition)
					{
						g.TranslateTransform(m_bounds.X + axis.LineType.Width + (float)(2 * axis.TickSize.Width) + m_clientRect.Width, m_bounds.Y);
					}
					else
					{
						g.TranslateTransform(m_bounds.X - axis.LineType.Width - (float)(2 * axis.TickSize.Width) - m_clientRect.Width, m_bounds.Y);
					}
				}
				else if (!axis.OpposedPosition)
				{
					g.TranslateTransform(m_bounds.X, m_bounds.Y + axis.LineType.Width - (float)(2 * axis.TickSize.Height) - m_clientRect.Height);
				}
				else
				{
					g.TranslateTransform(m_bounds.X, m_bounds.Y + axis.LineType.Width + (float)(2 * axis.TickSize.Height) + m_clientRect.Height);
				}
			}
			else if (AxisLabelPlacement == ChartPlacement.Outside && axis.AxisLabelPlacement != AxisLabelPlacement)
			{
				if (axis.Orientation == ChartOrientation.Vertical)
				{
					if (!axis.OpposedPosition)
					{
						g.TranslateTransform(m_bounds.X - axis.LineType.Width - (float)(2 * axis.TickSize.Width) - m_clientRect.Width, m_bounds.Y);
					}
					else
					{
						g.TranslateTransform(m_bounds.X + axis.LineType.Width + (float)(2 * axis.TickSize.Width) + m_clientRect.Width, m_bounds.Y);
					}
				}
				else if (!axis.OpposedPosition)
				{
					g.TranslateTransform(m_bounds.X, m_bounds.Y + axis.LineType.Width + (float)(2 * axis.TickSize.Height) + m_clientRect.Height);
				}
				else
				{
					g.TranslateTransform(m_bounds.X, m_bounds.Y + axis.LineType.Width - (float)(2 * axis.TickSize.Height) - m_clientRect.Height);
				}
			}
			else
			{
				g.TranslateTransform(m_bounds.X, m_bounds.Y);
			}
		}
		else
		{
			g.TranslateTransform(m_bounds.X + m_bounds.Width / 2f, m_bounds.Y + m_bounds.Height / 2f);
			g.RotateTransform(m_angle);
			g.TranslateTransform((0f - m_clientRect.Width) / 2f, (0f - m_clientRect.Height) / 2f);
		}
		if (axis.BackInterior != null)
		{
			BrushPaint.FillRectangle(g, new RectangleF(new PointF(0f, 0f), m_bounds.Size), axis.BackInterior);
		}
		using (SolidBrush brush = new SolidBrush(color))
		{
			g.DrawString(Text, font, brush, m_clientRect, stringFormat);
		}
		stringFormat.Dispose();
		DrawingHelper.EndTransform(g, cont);
	}

	public void Dispose()
	{
		if (c_defualtFont != null)
		{
			c_defualtFont.Dispose();
			c_defualtFont = null;
		}
		if (m_labelFont != null)
		{
			m_labelFont.Dispose();
			m_labelFont = null;
		}
	}

	internal Path3D Draw3D(Graphics3D g, ChartAxis axis, float z, ChartArea chartArea)
	{
		Matrix matrix = new Matrix();
		GraphicsPath graphicsPath = new GraphicsPath();
		Font font = ((m_labelFont == null) ? axis.Font : m_labelFont);
		Color color = (m_labelColor.IsEmpty ? axis.ForeColor : m_labelColor);
		if (m_angle == 0f)
		{
			if (AxisLabelPlacement == ChartPlacement.Inside && axis.AxisLabelPlacement != AxisLabelPlacement)
			{
				if (axis.Orientation == ChartOrientation.Vertical)
				{
					if (!axis.OpposedPosition)
					{
						matrix.Translate(m_bounds.X + axis.LineType.Width + (float)(2 * axis.TickSize.Width) + m_clientRect.Width, m_bounds.Y);
					}
					else
					{
						matrix.Translate(m_bounds.X - axis.LineType.Width - (float)(2 * axis.TickSize.Width) - m_clientRect.Width, m_bounds.Y);
					}
				}
				else if (!axis.OpposedPosition)
				{
					matrix.Translate(m_bounds.X, m_bounds.Y + axis.LineType.Width - (float)(2 * axis.TickSize.Height) - m_clientRect.Height);
				}
				else
				{
					matrix.Translate(m_bounds.X, m_bounds.Y + axis.LineType.Width + (float)(2 * axis.TickSize.Height) + m_clientRect.Height);
				}
			}
			else if (AxisLabelPlacement == ChartPlacement.Outside && axis.AxisLabelPlacement != AxisLabelPlacement)
			{
				if (axis.Orientation == ChartOrientation.Vertical)
				{
					if (!axis.OpposedPosition)
					{
						matrix.Translate(m_bounds.X - axis.LineType.Width - (float)(2 * axis.TickSize.Width) - m_clientRect.Width, m_bounds.Y);
					}
					else
					{
						matrix.Translate(m_bounds.X + axis.LineType.Width + (float)(2 * axis.TickSize.Width) + m_clientRect.Width, m_bounds.Y);
					}
				}
				else if (!axis.OpposedPosition)
				{
					matrix.Translate(m_bounds.X, m_bounds.Y + axis.LineType.Width + (float)(2 * axis.TickSize.Height) + m_clientRect.Height);
				}
				else
				{
					matrix.Translate(m_bounds.X, m_bounds.Y + axis.LineType.Width - (float)(2 * axis.TickSize.Height) - m_clientRect.Height);
				}
			}
			else
			{
				matrix.Translate(m_bounds.X, m_bounds.Y);
			}
		}
		else
		{
			matrix.Translate(m_bounds.X + m_bounds.Width / 2f, m_bounds.Y + m_bounds.Height / 2f);
			matrix.Rotate(m_angle);
			matrix.Translate((0f - m_clientRect.Width) / 2f, (0f - m_clientRect.Height) / 2f);
		}
		RenderingHelper.AddTextPath(graphicsPath, g.Graphics, Text, font, m_clientRect);
		graphicsPath.Transform(matrix);
		return Path3D.FromGraphicsPath(graphicsPath, z, new SolidBrush(color));
	}
}
