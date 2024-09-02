using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

[Serializable]
internal class ChartAxisGroupingLabel
{
	private StringFormat m_labelFormat = (StringFormat)StringFormat.GenericDefault.Clone();

	private string m_labelText;

	private string m_description;

	private Font m_labelFont;

	private Color m_labelColor;

	private Color m_backColor = Color.Transparent;

	private Pen m_labelBorderPen = Pens.Black;

	private DoubleRange m_labelDoubleRange;

	private ChartAxisGroupingLabelBorderStyle m_labelBorderStyle;

	private float m_labelBorderPadding = 2f;

	private float m_labelMaxTextWidth = 200f;

	private float m_labelMaxTextHeightToWidthRatio = 2.5f;

	private float m_labelRotateAngle;

	private int m_labelRow;

	private float m_labelGridDimension;

	private RectangleF m_rect = RectangleF.Empty;

	private ChartAxisGroupingLabelTextAlignment m_textAlignment;

	private ChartAxisGroupingLabelTextFitMode m_labelTextFitMode = ChartAxisGroupingLabelTextFitMode.Shrink;

	private object m_tag;

	public DoubleRange Range
	{
		get
		{
			return m_labelDoubleRange;
		}
		set
		{
			if (m_labelDoubleRange != value)
			{
				m_labelDoubleRange = value;
			}
		}
	}

	public string Text
	{
		get
		{
			return m_labelText;
		}
		set
		{
			if (m_labelText != value)
			{
				m_labelText = value;
			}
		}
	}

	public string RegionDescription
	{
		get
		{
			return m_description;
		}
		set
		{
			if (m_description != value)
			{
				m_description = value;
			}
		}
	}

	public string[] Lines
	{
		get
		{
			int num = 0;
			ArrayList arrayList = new ArrayList();
			while (num > -1)
			{
				int num2 = m_labelText.IndexOf(Environment.NewLine, num);
				if (num2 > -1)
				{
					arrayList.Add(m_labelText.Substring(num, num2 - num));
					num = num2 + Environment.NewLine.Length;
				}
				else
				{
					arrayList.Add(m_labelText.Substring(num, m_labelText.Length - num));
					num = num2;
				}
			}
			return (string[])arrayList.ToArray(typeof(string));
		}
		set
		{
			m_labelText = string.Join(Environment.NewLine, value);
		}
	}

	public StringFormat Format
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
			}
		}
	}

	public Font Font
	{
		get
		{
			return m_labelFont;
		}
		set
		{
			if (m_labelFont != value)
			{
				m_labelFont = value;
			}
		}
	}

	public Color Color
	{
		get
		{
			return m_labelColor;
		}
		set
		{
			if (m_labelColor != value)
			{
				m_labelColor = value;
			}
		}
	}

	public Color BackColor
	{
		get
		{
			return m_backColor;
		}
		set
		{
			if (m_backColor != value)
			{
				m_backColor = value;
			}
		}
	}

	public Pen BorderPen
	{
		get
		{
			return m_labelBorderPen;
		}
		set
		{
			if (m_labelBorderPen != value)
			{
				m_labelBorderPen = value;
			}
		}
	}

	public ChartAxisGroupingLabelBorderStyle BorderStyle
	{
		get
		{
			return m_labelBorderStyle;
		}
		set
		{
			if (m_labelBorderStyle != value)
			{
				m_labelBorderStyle = value;
			}
		}
	}

	public float BorderPadding
	{
		get
		{
			return m_labelBorderPadding;
		}
		set
		{
			if (m_labelBorderPadding != value)
			{
				m_labelBorderPadding = value;
			}
		}
	}

	public float MaxTextWidth
	{
		get
		{
			return m_labelMaxTextWidth;
		}
		set
		{
			if (m_labelMaxTextWidth != value)
			{
				m_labelMaxTextWidth = value;
			}
		}
	}

	public float MaxTextHeightToWidthRatio
	{
		get
		{
			return m_labelMaxTextHeightToWidthRatio;
		}
		set
		{
			if (m_labelMaxTextHeightToWidthRatio != value)
			{
				m_labelMaxTextHeightToWidthRatio = value;
			}
		}
	}

	public float RotateAngle
	{
		get
		{
			return m_labelRotateAngle;
		}
		set
		{
			if (m_labelRotateAngle != value)
			{
				m_labelRotateAngle = value;
			}
		}
	}

	public int Row
	{
		get
		{
			return m_labelRow;
		}
		set
		{
			if (m_labelRow != value)
			{
				m_labelRow = value;
			}
		}
	}

	internal float GridDimension
	{
		get
		{
			return m_labelGridDimension;
		}
		set
		{
			if (m_labelGridDimension != value)
			{
				m_labelGridDimension = value;
			}
		}
	}

	public ChartAxisGroupingLabelTextFitMode LabelTextFitMode
	{
		get
		{
			return m_labelTextFitMode;
		}
		set
		{
			if (m_labelTextFitMode != value)
			{
				m_labelTextFitMode = value;
			}
		}
	}

	public ChartAxisGroupingLabelTextAlignment LabelTextAlignment
	{
		get
		{
			return m_textAlignment;
		}
		set
		{
			if (m_textAlignment != value)
			{
				m_textAlignment = value;
			}
		}
	}

	public RectangleF Rect => m_rect;

	public object Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			if (m_tag != value)
			{
				m_tag = value;
			}
		}
	}

	public ChartAxisGroupingLabel(DoubleRange range)
		: this(range, "", Color.Black, new Pen(Color.Black), new Font("Verdana", 10f))
	{
	}

	public ChartAxisGroupingLabel(DoubleRange range, string text)
		: this(range, text, Color.Black, new Pen(Color.Black), new Font("Verdana", 10f))
	{
	}

	public ChartAxisGroupingLabel(DoubleRange range, string text, Color color, Font font)
		: this(range, text, color, new Pen(color), font)
	{
	}

	public ChartAxisGroupingLabel(DoubleRange range, string text, Color color, Color borderColor, Font font)
		: this(range, text, color, new Pen(borderColor), font)
	{
	}

	public ChartAxisGroupingLabel(DoubleRange range, string text, Color color, Pen borderPen, Font font)
	{
		m_labelDoubleRange = range;
		m_labelText = text;
		m_labelColor = color;
		m_labelBorderPen = borderPen;
		m_labelFont = font;
		m_labelFormat.Alignment = StringAlignment.Center;
		m_labelFormat.LineAlignment = StringAlignment.Center;
	}

	private RectangleF Draw(Graphics g, float position, float dimention, ChartAxis axis, bool measureDraw)
	{
		float num = ((!axis.OpposedPosition) ? 1 : (-1));
		float coordinateFromValue = axis.GetCoordinateFromValue(Range.Start);
		float coordinateFromValue2 = axis.GetCoordinateFromValue(Range.End);
		Matrix matrix = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		SizeF textSize = SizeF.Empty;
		SizeF stringSize = SizeF.Empty;
		SizeF labelSize = SizeF.Empty;
		Font font = (string.IsNullOrEmpty(m_labelText) ? null : GetTextFontAndSizes(g, axis, out stringSize, out textSize, out labelSize));
		if (!measureDraw)
		{
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				labelSize.Height = dimention;
			}
			else
			{
				labelSize.Width = dimention;
			}
		}
		switch (BorderStyle)
		{
		case ChartAxisGroupingLabelBorderStyle.Brace:
		{
			RectangleF rect7;
			RectangleF rectangle5;
			PointF pointF7;
			PointF pt5;
			PointF pointF8;
			PointF pt6;
			PointF pt7;
			PointF pointF9;
			PointF pt8;
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				rect7 = new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height, Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height);
				rectangle5 = new RectangleF(rect7.Left + rect7.Width / 2f - stringSize.Width / 2f, rect7.Top + rect7.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF7 = new PointF(rect7.X + rect7.Width / 2f, rect7.Y + rect7.Height / 2f);
				pt5 = new PointF(rect7.Left + rect7.Width / 2f - textSize.Width / 2f, rect7.Top + rect7.Height / 2f);
				pointF8 = new PointF(rect7.Left, rect7.Top + rect7.Height / 2f);
				pt6 = new PointF(rect7.Left, rect7.Top + rect7.Height / 2f - num * rect7.Height / 2f);
				pt7 = new PointF(rect7.Left + rect7.Width / 2f + textSize.Width / 2f, rect7.Top + rect7.Height / 2f);
				pointF9 = new PointF(rect7.Right, rect7.Top + rect7.Height / 2f);
				pt8 = new PointF(rect7.Right, rect7.Top + rect7.Height / 2f - num * rect7.Height / 2f);
			}
			else
			{
				rect7 = new RectangleF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2), labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue));
				rectangle5 = new RectangleF(rect7.Left + rect7.Width / 2f - stringSize.Width / 2f, rect7.Top + rect7.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF7 = new PointF(rect7.X + rect7.Width / 2f, rect7.Y + rect7.Height / 2f);
				pt5 = new PointF(rect7.Left + rect7.Width / 2f, rect7.Top + rect7.Height / 2f - textSize.Height / 2f);
				pointF8 = new PointF(rect7.Left + rect7.Width / 2f, rect7.Top);
				pt6 = new PointF(rect7.Left + rect7.Width / 2f + num * rect7.Width / 2f, rect7.Top);
				pt7 = new PointF(rect7.Left + rect7.Width / 2f, rect7.Top + rect7.Height / 2f + textSize.Height / 2f);
				pointF9 = new PointF(rect7.Left + rect7.Width / 2f, rect7.Bottom);
				pt8 = new PointF(rect7.Left + rect7.Width / 2f + num * rect7.Width / 2f, rect7.Bottom);
			}
			m_rect = rect7;
			if (!measureDraw)
			{
				GraphicsContainer cont5 = DrawingHelper.BeginTransform(g);
				matrix.RotateAt(RotateAngle, pointF7);
				g.Transform = matrix;
				g.DrawString(Text, font, new SolidBrush(Color), rectangle5, m_labelFormat);
				DrawingHelper.EndTransform(g, cont5);
				g.DrawLine(BorderPen, pt5, pointF8);
				g.DrawLine(BorderPen, pointF8, pt6);
				g.DrawLine(BorderPen, pt7, pointF9);
				g.DrawLine(BorderPen, pointF9, pt8);
			}
			break;
		}
		case ChartAxisGroupingLabelBorderStyle.Rectangle:
		{
			RectangleF rect6 = ((axis.Orientation != 0) ? new RectangleF(new PointF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2)), new SizeF(labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue))) : new RectangleF(new PointF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height), new SizeF(Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height)));
			float num2 = Math.Min(textSize.Width, rect6.Width);
			float num3 = Math.Min(textSize.Height, rect6.Height);
			RectangleF rectangle4;
			PointF pointF6;
			switch (LabelTextAlignment)
			{
			case ChartAxisGroupingLabelTextAlignment.Left:
				rectangle4 = new RectangleF(rect6.Left + num2 / 2f - stringSize.Width / 2f, rect6.Top + rect6.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.Left + num2 / 2f, rect6.Y + rect6.Height / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.Right:
				rectangle4 = new RectangleF(rect6.Right - num2 / 2f - stringSize.Width / 2f, rect6.Top + rect6.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.Right - num2 / 2f, rect6.Y + rect6.Height / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.Top:
				rectangle4 = new RectangleF(rect6.Left + rect6.Width / 2f - stringSize.Width / 2f, rect6.Top + num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.X + rect6.Width / 2f, rect6.Y + num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.Bottom:
				rectangle4 = new RectangleF(rect6.Left + rect6.Width / 2f - stringSize.Width / 2f, rect6.Bottom - num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.X + rect6.Width / 2f, rect6.Bottom - num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.TopLeft:
				rectangle4 = new RectangleF(rect6.Left + num2 / 2f - stringSize.Width / 2f, rect6.Top + num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.X + num2 / 2f, rect6.Y + num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.TopRight:
				rectangle4 = new RectangleF(rect6.Right - num2 / 2f - stringSize.Width / 2f, rect6.Top + num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.Right - num2 / 2f, rect6.Y + num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.BottomLeft:
				rectangle4 = new RectangleF(rect6.Left + num2 / 2f - stringSize.Width / 2f, rect6.Bottom - num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.Left + num2 / 2f, rect6.Bottom - num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.BottomRight:
				rectangle4 = new RectangleF(rect6.Right - num2 / 2f - stringSize.Width / 2f, rect6.Bottom - num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.Right - num2 / 2f, rect6.Bottom - num3 / 2f);
				break;
			default:
				rectangle4 = new RectangleF(rect6.Left + rect6.Width / 2f - stringSize.Width / 2f, rect6.Top + rect6.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF6 = new PointF(rect6.X + rect6.Width / 2f, rect6.Y + rect6.Height / 2f);
				break;
			}
			m_rect = rect6;
			if (!measureDraw)
			{
				using (SolidBrush brush = new SolidBrush(m_backColor))
				{
					g.FillRectangle(brush, rect6.X, rect6.Y, rect6.Width, rect6.Height);
				}
				GraphicsContainer cont4 = DrawingHelper.BeginTransform(g);
				matrix.RotateAt(RotateAngle, pointF6);
				g.Transform = matrix;
				g.DrawString(Text, font, new SolidBrush(Color), rectangle4, m_labelFormat);
				DrawingHelper.EndTransform(g, cont4);
				g.DrawRectangle(BorderPen, rect6.X, rect6.Y, rect6.Width, rect6.Height);
			}
			break;
		}
		case ChartAxisGroupingLabelBorderStyle.WithoutBorder:
		{
			RectangleF rect3;
			RectangleF rectangle2;
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				rect3 = new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height, Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height);
				rectangle2 = new RectangleF(rect3.Left + rect3.Width / 2f - stringSize.Width / 2f, rect3.Top + rect3.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				new PointF(rect3.X + rect3.Width / 2f, rect3.Y + rect3.Height / 2f);
			}
			else
			{
				rect3 = new RectangleF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2), labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue));
				rectangle2 = new RectangleF(rect3.Left + rect3.Width / 2f - stringSize.Width / 2f, rect3.Top + rect3.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				new PointF(rect3.X + rect3.Width / 2f, rect3.Y + rect3.Height / 2f);
			}
			m_rect = rect3;
			if (!measureDraw)
			{
				GraphicsContainer cont2 = DrawingHelper.BeginTransform(g);
				g.DrawString(Text, font, new SolidBrush(axis.ForeColor), rectangle2, m_labelFormat);
				DrawingHelper.EndTransform(g, cont2);
			}
			break;
		}
		case ChartAxisGroupingLabelBorderStyle.WithoutTopBorder:
		{
			RectangleF rect4;
			RectangleF rectangle3;
			PointF pointF3;
			PointF pointF4;
			PointF pointF5;
			PointF pt3;
			PointF pt4;
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				rect4 = new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height, Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height);
				rectangle3 = new RectangleF(rect4.Left + rect4.Width / 2f - stringSize.Width / 2f, rect4.Top + rect4.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF3 = new PointF(rect4.X + rect4.Width / 2f, rect4.Y + rect4.Height / 2f);
				new PointF(rect4.Left + rect4.Width - textSize.Width, rect4.Top + rect4.Height);
				pointF4 = new PointF(rect4.Left, rect4.Top + rect4.Height);
				pointF5 = new PointF(rect4.Left, rect4.Top + rect4.Height - num * rect4.Height);
				new PointF(rect4.Left + rect4.Width + textSize.Width, rect4.Top + rect4.Height);
				pt3 = new PointF(rect4.Right, rect4.Top + rect4.Height);
				pt4 = new PointF(rect4.Right, rect4.Top + rect4.Height - num * rect4.Height);
			}
			else
			{
				rect4 = new RectangleF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2), labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue));
				rectangle3 = new RectangleF(rect4.Left + rect4.Width / 2f - stringSize.Width / 2f, rect4.Top + rect4.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF3 = new PointF(rect4.X + rect4.Width / 2f, rect4.Y + rect4.Height / 2f);
				new PointF(rect4.Left + rect4.Width - textSize.Width, rect4.Top + rect4.Height);
				pointF4 = new PointF(rect4.Left, rect4.Top + rect4.Height);
				pointF5 = new PointF(rect4.Left, rect4.Top + rect4.Height - num * rect4.Height);
				new PointF(rect4.Left + rect4.Width + textSize.Width, rect4.Top + rect4.Height);
				pt3 = new PointF(rect4.Right, rect4.Top + rect4.Height);
				pt4 = new PointF(rect4.Right, rect4.Top + rect4.Height - num * rect4.Height);
			}
			m_rect = rect4;
			if (!measureDraw)
			{
				GraphicsContainer cont3 = DrawingHelper.BeginTransform(g);
				matrix.RotateAt(RotateAngle, pointF3);
				g.Transform = matrix;
				g.DrawString(Text, font, new SolidBrush(Color), rectangle3, m_labelFormat);
				DrawingHelper.EndTransform(g, cont3);
				if (axis.Orientation == ChartOrientation.Horizontal)
				{
					g.DrawLine(BorderPen, pt3, pointF4);
					g.DrawLine(BorderPen, pointF4, pointF5);
					g.DrawLine(BorderPen, pt3, pt4);
				}
				else
				{
					g.DrawLine(BorderPen, pt3, pointF4);
					g.DrawLine(BorderPen, pointF5, pt4);
					g.DrawLine(BorderPen, pointF4, pointF5);
				}
			}
			break;
		}
		case ChartAxisGroupingLabelBorderStyle.WithoutTopAndBottomBorder:
		{
			RectangleF rect2;
			RectangleF rectangle;
			PointF pointF;
			PointF pointF2;
			PointF pt;
			PointF pt2;
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				rect2 = new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height, Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height);
				rectangle = new RectangleF(rect2.Left + rect2.Width / 2f - stringSize.Width / 2f, rect2.Top + rect2.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				new PointF(rect2.X + rect2.Width / 2f, rect2.Y + rect2.Height / 2f);
				new PointF(rect2.Left + rect2.Width - textSize.Width, rect2.Top + rect2.Height);
				pointF = new PointF(rect2.Left, rect2.Top + rect2.Height);
				pointF2 = new PointF(rect2.Left, rect2.Top + rect2.Height - num * rect2.Height);
				new PointF(rect2.Left + rect2.Width + textSize.Width, rect2.Top + rect2.Height);
				pt = new PointF(rect2.Right, rect2.Top + rect2.Height);
				pt2 = new PointF(rect2.Right, rect2.Top + rect2.Height - num * rect2.Height);
			}
			else
			{
				rect2 = new RectangleF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2), labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue));
				rectangle = new RectangleF(rect2.Left + rect2.Width / 2f - stringSize.Width / 2f, rect2.Top + rect2.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				new PointF(rect2.X + rect2.Width / 2f, rect2.Y + rect2.Height / 2f);
				new PointF(rect2.Left + rect2.Width - textSize.Width, rect2.Top + rect2.Height);
				pointF = new PointF(rect2.Left, rect2.Top + rect2.Height);
				pointF2 = new PointF(rect2.Left, rect2.Top + rect2.Height - num * rect2.Height);
				new PointF(rect2.Left + rect2.Width + textSize.Width, rect2.Top + rect2.Height);
				pt = new PointF(rect2.Right, rect2.Top + rect2.Height);
				pt2 = new PointF(rect2.Right, rect2.Top + rect2.Height - num * rect2.Height);
			}
			m_rect = rect2;
			if (!measureDraw)
			{
				GraphicsContainer cont = DrawingHelper.BeginTransform(g);
				g.DrawString(Text, font, new SolidBrush(axis.ForeColor), rectangle, m_labelFormat);
				DrawingHelper.EndTransform(g, cont);
				BorderPen.Color = axis.LineType.ForeColor;
				pointF2.Y -= rect2.Height;
				pt2.Y -= rect2.Height;
				if (axis.Orientation == ChartOrientation.Horizontal)
				{
					g.DrawLine(BorderPen, pointF, pointF2);
					g.DrawLine(BorderPen, pt, pt2);
				}
				else
				{
					g.DrawLine(BorderPen, pt, pointF);
					g.DrawLine(BorderPen, pointF2, pt2);
				}
			}
			break;
		}
		case ChartAxisGroupingLabelBorderStyle.RightBorder:
		{
			RectangleF rect5 = ((axis.Orientation != 0) ? new RectangleF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2), labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue)) : new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height, Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height));
			RectangleF textRect2 = new RectangleF(rect5.Left + rect5.Width / 2f - stringSize.Width / 2f, rect5.Top + rect5.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
			PointF borderRectCenter2 = new PointF(rect5.X + rect5.Width / 2f, rect5.Y + rect5.Height / 2f);
			PointF point4 = new PointF(rect5.Left, rect5.Top + rect5.Height - num * rect5.Height);
			PointF point5 = new PointF(rect5.Right, rect5.Top + rect5.Height);
			PointF point6 = new PointF(rect5.Right, rect5.Top + rect5.Height - num * rect5.Height);
			m_rect = rect5;
			drawline(measureDraw, point5, point6, point4, borderRectCenter2, g, font, textRect2, axis, ChartAxisGroupingLabelBorderStyle.RightBorder);
			break;
		}
		case ChartAxisGroupingLabelBorderStyle.LeftBorder:
		{
			RectangleF rect = ((axis.Orientation != 0) ? new RectangleF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2), labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue)) : new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height, Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height));
			RectangleF textRect = new RectangleF(rect.Left + rect.Width / 2f - stringSize.Width / 2f, rect.Top + rect.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
			PointF borderRectCenter = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
			PointF point = new PointF(rect.Left, rect.Top + rect.Height);
			PointF point2 = new PointF(rect.Left, rect.Top + rect.Height - num * rect.Height);
			PointF point3 = new PointF(rect.Right, rect.Top + rect.Height);
			m_rect = rect;
			drawline(measureDraw, point, point2, point3, borderRectCenter, g, font, textRect, axis, ChartAxisGroupingLabelBorderStyle.LeftBorder);
			break;
		}
		}
		return m_rect;
	}

	public void Dispose()
	{
		m_labelFont.Dispose();
		m_labelBorderPen.Dispose();
	}

	private void drawline(bool measureDraw, PointF point1, PointF point2, PointF point3, PointF borderRectCenter, Graphics g, Font font, RectangleF textRect, ChartAxis axis, ChartAxisGroupingLabelBorderStyle BorderType)
	{
		Matrix matrix = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		if (!measureDraw)
		{
			GraphicsContainer cont = DrawingHelper.BeginTransform(g);
			matrix.RotateAt(RotateAngle, borderRectCenter);
			g.Transform = matrix;
			g.DrawString(Text, font, new SolidBrush(Color), textRect, m_labelFormat);
			DrawingHelper.EndTransform(g, cont);
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				g.DrawLine(BorderPen, point1, point2);
			}
			else if (BorderType == ChartAxisGroupingLabelBorderStyle.LeftBorder)
			{
				g.DrawLine(BorderPen, point3, point1);
			}
			else
			{
				g.DrawLine(BorderPen, point3, point2);
			}
		}
	}

	private RectangleF Draw(Graphics3D g, float position, float dimention, ChartAxis axis, bool measureDraw)
	{
		float num = ((!axis.OpposedPosition) ? 1 : (-1));
		float coordinateFromValue = axis.GetCoordinateFromValue(Range.Start);
		float coordinateFromValue2 = axis.GetCoordinateFromValue(Range.End);
		Matrix matrix = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		Math.Abs(Math.Cos((double)RotateAngle * Math.PI / 180.0));
		Math.Abs(Math.Sin((double)RotateAngle * Math.PI / 180.0));
		SizeF textSize = SizeF.Empty;
		SizeF stringSize = SizeF.Empty;
		SizeF labelSize = SizeF.Empty;
		Font font = (string.IsNullOrEmpty(m_labelText) ? null : GetTextFontAndSizes(g.Graphics, axis, out stringSize, out textSize, out labelSize));
		if (!measureDraw)
		{
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				labelSize.Height = dimention;
			}
			else
			{
				labelSize.Width = dimention;
			}
		}
		switch (BorderStyle)
		{
		case ChartAxisGroupingLabelBorderStyle.Brace:
		{
			RectangleF rect;
			RectangleF layoutRect2;
			PointF pointF2;
			PointF pointF3;
			PointF pointF4;
			PointF pointF5;
			PointF pointF6;
			PointF pointF7;
			PointF pointF8;
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				rect = new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height, Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height);
				layoutRect2 = new RectangleF(rect.Left + rect.Width / 2f - stringSize.Width / 2f, rect.Top + rect.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF2 = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
				pointF3 = new PointF(rect.Left + rect.Width / 2f - textSize.Width / 2f, rect.Top + rect.Height / 2f);
				pointF4 = new PointF(rect.Left, rect.Top + rect.Height / 2f);
				pointF5 = new PointF(rect.Left, rect.Top + rect.Height / 2f - num * rect.Height / 2f);
				pointF6 = new PointF(rect.Left + rect.Width / 2f + textSize.Width / 2f, rect.Top + rect.Height / 2f);
				pointF7 = new PointF(rect.Right, rect.Top + rect.Height / 2f);
				pointF8 = new PointF(rect.Right, rect.Top + rect.Height / 2f - num * rect.Height / 2f);
			}
			else
			{
				rect = new RectangleF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2), labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue));
				layoutRect2 = new RectangleF(rect.Left + rect.Width / 2f - stringSize.Width / 2f, rect.Top + rect.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF2 = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
				pointF3 = new PointF(rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f - textSize.Height / 2f);
				pointF4 = new PointF(rect.Left + rect.Width / 2f, rect.Top);
				pointF5 = new PointF(rect.Left + rect.Width / 2f + num * rect.Width / 2f, rect.Top);
				pointF6 = new PointF(rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f + textSize.Height / 2f);
				pointF7 = new PointF(rect.Left + rect.Width / 2f, rect.Bottom);
				pointF8 = new PointF(rect.Left + rect.Width / 2f + num * rect.Width / 2f, rect.Bottom);
			}
			if (!measureDraw)
			{
				GraphicsPath graphicsPath3 = new GraphicsPath();
				matrix.RotateAt(RotateAngle, pointF2);
				graphicsPath3.Transform(matrix);
				graphicsPath3.AddString(Text, font.GetFontFamily(), (int)font.Style, RenderingHelper.GetFontSizeInPixels(font), layoutRect2, StringFormat.GenericDefault);
				GraphicsPath graphicsPath4 = new GraphicsPath();
				graphicsPath4.AddLine(pointF3, pointF4);
				graphicsPath4.AddLine(pointF4, pointF5);
				graphicsPath4.AddLine(pointF5, pointF4);
				graphicsPath4.CloseFigure();
				graphicsPath4.AddLine(pointF6, pointF7);
				graphicsPath4.AddLine(pointF7, pointF8);
				graphicsPath4.AddLine(pointF8, pointF7);
				graphicsPath4.CloseFigure();
				g.AddPolygon(new Path3DCollect(new Polygon[2]
				{
					Path3D.FromGraphicsPath(graphicsPath3, 0.0, new BrushInfo(Color)),
					Path3D.FromGraphicsPath(graphicsPath4, 0.0, BorderPen)
				}));
			}
			m_rect = rect;
			break;
		}
		case ChartAxisGroupingLabelBorderStyle.Rectangle:
		{
			RectangleF rectangleF = ((axis.Orientation != 0) ? new RectangleF(new PointF(position - (1f - num) / 2f * labelSize.Width, Math.Min(coordinateFromValue, coordinateFromValue2)), new SizeF(labelSize.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue))) : new RectangleF(new PointF(Math.Min(coordinateFromValue, coordinateFromValue2), position - (1f + num) / 2f * labelSize.Height), new SizeF(Math.Abs(coordinateFromValue2 - coordinateFromValue), labelSize.Height)));
			float num2 = Math.Min(textSize.Width, rectangleF.Width);
			float num3 = Math.Min(textSize.Height, rectangleF.Height);
			RectangleF layoutRect;
			PointF pointF;
			switch (LabelTextAlignment)
			{
			case ChartAxisGroupingLabelTextAlignment.Left:
				layoutRect = new RectangleF(rectangleF.Left + num2 / 2f - stringSize.Width / 2f, rectangleF.Top + rectangleF.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.Left + num2 / 2f, rectangleF.Y + rectangleF.Height / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.Right:
				layoutRect = new RectangleF(rectangleF.Right - num2 / 2f - stringSize.Width / 2f, rectangleF.Top + rectangleF.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.Right - num2 / 2f, rectangleF.Y + rectangleF.Height / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.Top:
				layoutRect = new RectangleF(rectangleF.Left + rectangleF.Width / 2f - stringSize.Width / 2f, rectangleF.Top + num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.X + rectangleF.Width / 2f, rectangleF.Y + num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.Bottom:
				layoutRect = new RectangleF(rectangleF.Left + rectangleF.Width / 2f - stringSize.Width / 2f, rectangleF.Bottom - num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.X + rectangleF.Width / 2f, rectangleF.Bottom - num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.TopLeft:
				layoutRect = new RectangleF(rectangleF.Left + num2 / 2f - stringSize.Width / 2f, rectangleF.Top + num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.X + num2 / 2f, rectangleF.Y + num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.TopRight:
				layoutRect = new RectangleF(rectangleF.Right - num2 / 2f - stringSize.Width / 2f, rectangleF.Top + num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.Right - num2 / 2f, rectangleF.Y + num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.BottomLeft:
				layoutRect = new RectangleF(rectangleF.Left + num2 / 2f - stringSize.Width / 2f, rectangleF.Bottom - num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.Left + num2 / 2f, rectangleF.Bottom - num3 / 2f);
				break;
			case ChartAxisGroupingLabelTextAlignment.BottomRight:
				layoutRect = new RectangleF(rectangleF.Right - num2 / 2f - stringSize.Width / 2f, rectangleF.Bottom - num3 / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.Right - num2 / 2f, rectangleF.Bottom - num3 / 2f);
				break;
			default:
				layoutRect = new RectangleF(rectangleF.Left + rectangleF.Width / 2f - stringSize.Width / 2f, rectangleF.Top + rectangleF.Height / 2f - stringSize.Height / 2f, stringSize.Width, stringSize.Height);
				pointF = new PointF(rectangleF.X + rectangleF.Width / 2f, rectangleF.Y + rectangleF.Height / 2f);
				break;
			}
			if (!measureDraw)
			{
				GraphicsPath graphicsPath = new GraphicsPath();
				matrix.RotateAt(RotateAngle, pointF);
				graphicsPath.AddString(Text, font.GetFontFamily(), (int)font.Style, RenderingHelper.GetFontSizeInPixels(font), layoutRect, StringFormat.GenericDefault);
				graphicsPath.Transform(matrix);
				GraphicsPath graphicsPath2 = new GraphicsPath();
				graphicsPath2.AddRectangle(rectangleF);
				g.AddPolygon(new Path3DCollect(new Polygon[2]
				{
					Path3D.FromGraphicsPath(graphicsPath, 0.0, new BrushInfo(Color)),
					Path3D.FromGraphicsPath(graphicsPath2, 0.0, BorderPen)
				}));
			}
			m_rect = rectangleF;
			break;
		}
		}
		return m_rect;
	}

	internal SizeF GetSize(Graphics g, ChartAxis axis)
	{
		return Draw(g, 0f, 0f, axis, measureDraw: true).Size;
	}

	internal RectangleF Draw(Graphics g, float position, ChartAxis axis)
	{
		return Draw(g, position, GridDimension, axis, measureDraw: false);
	}

	internal RectangleF Draw(Graphics3D g, float position, ChartAxis axis)
	{
		return Draw(g, position, GridDimension, axis, measureDraw: false);
	}

	internal Font GetTextFontAndSizes(Graphics g, ChartAxis axis, out SizeF stringSize, out SizeF textSize, out SizeF labelSize)
	{
		float coordinateFromValue = axis.GetCoordinateFromValue(Range.Start);
		float coordinateFromValue2 = axis.GetCoordinateFromValue(Range.End);
		float coordinateFromValue3 = axis.GetCoordinateFromValue(Range.Start);
		float coordinateFromValue4 = axis.GetCoordinateFromValue(Range.End);
		float coordinateFromValue5 = axis.GetCoordinateFromValue(Range.Start);
		float coordinateFromValue6 = axis.GetCoordinateFromValue(Range.End);
		new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		float num = (float)Math.Abs(Math.Cos((double)RotateAngle * (Math.PI / 180.0)));
		float num2 = (float)Math.Abs(Math.Sin((double)RotateAngle * (Math.PI / 180.0)));
		_ = (float)Math.Min(Math.Ceiling(Math.Abs(coordinateFromValue6 - coordinateFromValue5)), MaxTextWidth) / num;
		stringSize = SizeF.Empty;
		textSize = SizeF.Empty;
		labelSize = SizeF.Empty;
		float val = 0.01f;
		Font font = Font.Clone() as Font;
		int num3 = 3;
		int num4 = 4;
		if (axis.Orientation == ChartOrientation.Horizontal)
		{
			Font font2 = font;
			float num5 = 1f;
			for (int i = 0; i < num4; i++)
			{
				font2 = new Font(font.GetFontName(), font.Size * Math.Max(num5, val), font.Style, font.Unit, font.GdiCharSet);
				double num6 = 8.988465674311579E+307;
				if (LabelTextFitMode == ChartAxisGroupingLabelTextFitMode.Wrap || LabelTextFitMode == ChartAxisGroupingLabelTextFitMode.WrapAndShrink)
				{
					num6 = (Math.Abs(coordinateFromValue - coordinateFromValue2) - 2f * BorderPadding) / num;
				}
				stringSize = g.MeasureString(Text, font2, (int)Math.Min(Math.Abs(num6), num5 * MaxTextWidth), m_labelFormat);
				for (int j = 0; j < num3; j++)
				{
					stringSize = g.MeasureString(Text, font2, (int)Math.Min(Math.Abs(num6 - (double)(stringSize.Height * (num2 / num))), MaxTextWidth), m_labelFormat);
				}
				textSize = new SizeF(Math.Abs(stringSize.Width * num + stringSize.Height * num2), Math.Abs(stringSize.Width * num2 + stringSize.Height * num));
				labelSize = new SizeF(Math.Abs(coordinateFromValue - coordinateFromValue2), Math.Max(textSize.Height + 2f * BorderPadding, m_labelGridDimension));
				if (LabelTextFitMode == ChartAxisGroupingLabelTextFitMode.None || LabelTextFitMode == ChartAxisGroupingLabelTextFitMode.Wrap || i == num4 - 1 || (textSize.Width < labelSize.Width && stringSize.Height < MaxTextHeightToWidthRatio * stringSize.Width))
				{
					break;
				}
				float val2 = 1f;
				float val3 = 1f;
				if (!(textSize.Width < labelSize.Width))
				{
					val2 = labelSize.Width / textSize.Width;
				}
				if (!(stringSize.Height < MaxTextHeightToWidthRatio * stringSize.Width))
				{
					val3 = MaxTextHeightToWidthRatio / (stringSize.Height / stringSize.Width);
				}
				num5 = Math.Min(val2, val3);
			}
			font = font2;
		}
		else
		{
			Font font3 = font;
			float num7 = 1f;
			for (int k = 0; k < num4; k++)
			{
				font3 = new Font(font.GetFontName(), font.Size * Math.Max(num7, val), font.Style, font.Unit, font.GdiCharSet);
				double num8 = 8.988465674311579E+307;
				if (LabelTextFitMode == ChartAxisGroupingLabelTextFitMode.Wrap || LabelTextFitMode == ChartAxisGroupingLabelTextFitMode.WrapAndShrink)
				{
					num8 = (Math.Abs(coordinateFromValue3 - coordinateFromValue4) - 2f * BorderPadding) / num2;
				}
				stringSize = g.MeasureString(Text, font3, (int)MaxTextWidth, m_labelFormat);
				if (num2 != 0f)
				{
					for (int l = 0; l < num3; l++)
					{
						stringSize = g.MeasureString(Text, font3, (int)Math.Min(Math.Abs(num8 - (double)(stringSize.Height * (num / num2))), num7 * MaxTextWidth), m_labelFormat);
					}
				}
				textSize = new SizeF(Math.Abs(stringSize.Width * num + stringSize.Height * num2), Math.Abs(stringSize.Width * num2 + stringSize.Height * num));
				labelSize = new SizeF(Math.Max(textSize.Width + 2f * BorderPadding, m_labelGridDimension), Math.Abs(coordinateFromValue3 - coordinateFromValue4));
				if (LabelTextFitMode == ChartAxisGroupingLabelTextFitMode.None || LabelTextFitMode == ChartAxisGroupingLabelTextFitMode.Wrap || k == num4 - 1 || ((double)textSize.Height < 1.0 * (double)labelSize.Height && stringSize.Height < MaxTextHeightToWidthRatio * stringSize.Width))
				{
					break;
				}
				float val4 = 1f;
				float val5 = 1f;
				if (!((double)textSize.Height < 1.0 * (double)labelSize.Height))
				{
					val4 = labelSize.Height / textSize.Height;
				}
				if (!(stringSize.Height < MaxTextHeightToWidthRatio * stringSize.Width))
				{
					val5 = MaxTextHeightToWidthRatio / (stringSize.Height / stringSize.Width);
				}
				num7 = Math.Min(val4, val5);
			}
			font = font3;
		}
		labelSize = new SizeF(textSize.Width + 2f * BorderPadding, textSize.Height + 2f * BorderPadding);
		return font;
	}
}
