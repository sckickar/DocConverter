using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartAxesInfoBar
{
	private bool m_visible;

	private string m_text = "";

	private Font m_font = new Font("Verdana", 10f);

	private Color m_textColor = Color.Black;

	private bool m_showBorder = true;

	private LineInfo m_border = new LineInfo();

	private Hashtable m_groupingCells = new Hashtable();

	private StringFormat m_stringFormat = (StringFormat)StringFormat.GenericDefault.Clone();

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			if (m_visible != value)
			{
				m_visible = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

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
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public Font Font
	{
		get
		{
			return m_font;
		}
		set
		{
			if (m_font != value)
			{
				m_font = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

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
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public bool ShowBorder
	{
		get
		{
			return m_showBorder;
		}
		set
		{
			if (m_showBorder != value)
			{
				m_showBorder = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public LineInfo Border
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
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public StringFormat TextFormat
	{
		get
		{
			return m_stringFormat;
		}
		set
		{
			if (m_stringFormat != value)
			{
				m_stringFormat = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public string this[int col, int row]
	{
		get
		{
			return (string)m_groupingCells[GetCellKey(col, row)];
		}
		set
		{
			int cellKey = GetCellKey(col, row);
			if (!object.Equals(m_groupingCells[cellKey], value))
			{
				if (value == string.Empty)
				{
					m_groupingCells.Remove(cellKey);
				}
				else
				{
					m_groupingCells[cellKey] = value;
				}
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler Changed;

	internal ChartAxesInfoBar()
	{
		m_stringFormat.Alignment = StringAlignment.Center;
		m_stringFormat.LineAlignment = StringAlignment.Center;
	}

	internal void Draw(Graphics g, ChartAxis xAxis, ChartAxis yAxis)
	{
		float num = (yAxis.OpposedPosition ? 1 : (-1));
		float num2 = ((!xAxis.OpposedPosition) ? 1 : (-1));
		RectangleF rectangle = CorrectRect(new RectangleF(yAxis.Location.X, xAxis.Location.Y, num * yAxis.TickAndLabelsDimension, num2 * xAxis.TickAndLabelsDimension));
		using SolidBrush brush = new SolidBrush(m_textColor);
		if (m_text != null && m_text != "")
		{
			g.DrawString(m_text, m_font, brush, rectangle, m_stringFormat);
			if (m_showBorder)
			{
				g.DrawRectangle(m_border.Pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			}
		}
		float num3 = xAxis.TickAndLabelsDimension;
		for (int i = 0; i < xAxis.GroupingLabelsRowsDimensions.Length; i++)
		{
			float num4 = xAxis.GroupingLabelsRowsDimensions[i];
			if (num4 == num3)
			{
				continue;
			}
			float num5 = yAxis.TickAndLabelsDimension;
			for (int j = 0; j < yAxis.GroupingLabelsRowsDimensions.Length; j++)
			{
				float num6 = yAxis.GroupingLabelsRowsDimensions[j];
				if (num6 == num5)
				{
					continue;
				}
				RectangleF rectangle2 = CalcCellRect(CorrectRect(new RectangleF(yAxis.Location.X, xAxis.Location.Y, num * num5, num2 * num3)), new SizeF(num * (num6 - num5), num2 * (num4 - num3)));
				string text = (string)m_groupingCells[GetCellKey(i, j)];
				if (text != null)
				{
					if (m_showBorder)
					{
						g.DrawRectangle(m_border.Pen, rectangle2.X, rectangle2.Y, rectangle2.Width, rectangle2.Height);
					}
					g.DrawString(text, m_font, brush, rectangle2, m_stringFormat);
				}
				num5 = num6;
			}
			num3 = num4;
		}
	}

	internal void Draw(Graphics3D g, ChartAxis xAxis, ChartAxis yAxis)
	{
		float num = (yAxis.OpposedPosition ? 1 : (-1));
		float num2 = ((!xAxis.OpposedPosition) ? 1 : (-1));
		RectangleF rectangleF = CorrectRect(new RectangleF(yAxis.Location.X, xAxis.Location.Y, num * yAxis.TickAndLabelsDimension, num2 * xAxis.TickAndLabelsDimension));
		ArrayList arrayList = new ArrayList();
		BrushInfo br = new BrushInfo(m_textColor);
		if (m_text != null && m_text != "")
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddString(m_text, m_font.GetFontFamily(), (int)m_font.Style, RenderingHelper.GetFontSizeInPixels(m_font), rectangleF, m_stringFormat);
			arrayList.Add(Path3D.FromGraphicsPath(graphicsPath, 0.0, br));
			if (m_showBorder)
			{
				GraphicsPath graphicsPath2 = new GraphicsPath();
				graphicsPath2.AddRectangle(rectangleF);
				arrayList.Add(Path3D.FromGraphicsPath(graphicsPath2, 0.0, (BrushInfo)null, m_border.Pen));
			}
		}
		float num3 = xAxis.TickAndLabelsDimension;
		for (int i = 0; i < xAxis.GroupingLabelsRowsDimensions.Length; i++)
		{
			float num4 = xAxis.GroupingLabelsRowsDimensions[i];
			if (num4 == num3)
			{
				continue;
			}
			float num5 = yAxis.TickAndLabelsDimension;
			for (int j = 0; j < yAxis.GroupingLabelsRowsDimensions.Length; j++)
			{
				float num6 = yAxis.GroupingLabelsRowsDimensions[j];
				if (num6 == num5)
				{
					continue;
				}
				RectangleF rectangleF2 = CalcCellRect(CorrectRect(new RectangleF(yAxis.Location.X, xAxis.Location.Y, num * num5, num2 * num3)), new SizeF(num * (num6 - num5), num2 * (num4 - num3)));
				string text = (string)m_groupingCells[GetCellKey(i, j)];
				if (text != null)
				{
					GraphicsPath graphicsPath3 = new GraphicsPath();
					graphicsPath3.AddString(text, m_font.GetFontFamily(), (int)m_font.Style, RenderingHelper.GetFontSizeInPixels(m_font), rectangleF2, m_stringFormat);
					arrayList.Add(Path3D.FromGraphicsPath(graphicsPath3, 0.0, br));
					if (m_showBorder)
					{
						GraphicsPath graphicsPath4 = new GraphicsPath();
						graphicsPath4.AddRectangle(rectangleF2);
						arrayList.Add(Path3D.FromGraphicsPath(graphicsPath4, 0.0, (BrushInfo)null, m_border.Pen));
					}
				}
				num5 = num6;
			}
			num3 = num4;
		}
		g.AddPolygon(new Path3DCollect((Polygon[])arrayList.ToArray(typeof(Polygon))));
	}

	private static int GetCellKey(int col, int row)
	{
		return (col << 16) | row;
	}

	private static RectangleF CorrectRect(RectangleF rect)
	{
		return new RectangleF(Math.Min(rect.Left, rect.Right), Math.Min(rect.Top, rect.Bottom), Math.Abs(rect.Width), Math.Abs(rect.Height));
	}

	private static RectangleF CalcCellRect(RectangleF mainRect, SizeF size)
	{
		RectangleF empty = RectangleF.Empty;
		if (size.Width > 0f)
		{
			empty.X = mainRect.Right;
			empty.Width = size.Width;
		}
		else
		{
			empty.X = mainRect.Left + size.Width;
			empty.Width = 0f - size.Width;
		}
		if (size.Height > 0f)
		{
			empty.Y = mainRect.Bottom;
			empty.Height = size.Height;
		}
		else
		{
			empty.Y = mainRect.Top + size.Height;
			empty.Height = 0f - size.Height;
		}
		return empty;
	}

	private void OnChanged(EventArgs e)
	{
		if (this.Changed != null)
		{
			this.Changed(this, e);
		}
	}
}
