using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

[Serializable]
internal sealed class ChartStripLine
{
	private double m_start;

	private double m_end;

	private double m_width = 1.0;

	private double m_fixedWidth;

	private double m_period = 2.0;

	private string m_text = "StripLine";

	private Font m_font = new Font("Verdana", 10f);

	private Color m_textColor = SystemColors.ControlText;

	private ContentAlignment m_textAlign = ContentAlignment.MiddleCenter;

	private bool m_vertical;

	private bool m_startAtAxisPosition;

	private double m_offset;

	private BrushInfo m_interior = new BrushInfo(Color.White);

	private DocGen.Drawing.Image m_backImage;

	private bool m_enabled;

	private ChartStripLineZorder m_zOrder = ChartStripLineZorder.Behind;

	public ChartStripLineZorder ZOrder
	{
		get
		{
			return m_zOrder;
		}
		set
		{
			if (m_zOrder != value)
			{
				m_zOrder = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool StartAtAxisPosition
	{
		get
		{
			return m_startAtAxisPosition;
		}
		set
		{
			if (m_startAtAxisPosition != value)
			{
				m_startAtAxisPosition = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool Vertical
	{
		get
		{
			return m_vertical;
		}
		set
		{
			if (m_vertical != value)
			{
				m_vertical = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public ContentAlignment TextAlignment
	{
		get
		{
			return m_textAlign;
		}
		set
		{
			if (m_textAlign != value)
			{
				m_textAlign = value;
				RaiseChanged(this, EventArgs.Empty);
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
				RaiseChanged(this, EventArgs.Empty);
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
				RaiseChanged(this, EventArgs.Empty);
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
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public DocGen.Drawing.Image BackImage
	{
		get
		{
			return m_backImage;
		}
		set
		{
			if (m_backImage != value)
			{
				m_backImage = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public BrushInfo Interior
	{
		get
		{
			return m_interior;
		}
		set
		{
			if (m_interior != value)
			{
				m_interior = new BrushInfo(value);
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			if (m_enabled != value)
			{
				m_enabled = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public TimeSpan DateOffset
	{
		get
		{
			return DateTime.FromOADate(m_offset) - DateTime.FromOADate(0.0);
		}
		set
		{
			if (Offset != DateTime.FromOADate(0.0).Add(value).ToOADate())
			{
				Offset = DateTime.FromOADate(0.0).Add(value).ToOADate();
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			if (m_offset != value)
			{
				m_offset = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public TimeSpan PeriodDate
	{
		get
		{
			return DateTime.FromOADate(m_period) - DateTime.FromOADate(0.0);
		}
		set
		{
			if (m_period != DateTime.FromOADate(0.0).Add(value).ToOADate())
			{
				m_period = DateTime.FromOADate(0.0).Add(value).ToOADate();
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double Period
	{
		get
		{
			return m_period;
		}
		set
		{
			if (m_period != value)
			{
				m_period = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public TimeSpan WidthDate
	{
		get
		{
			return DateTime.FromOADate(m_width) - DateTime.FromOADate(0.0);
		}
		set
		{
			if (m_width != DateTime.FromOADate(0.0).Add(value).ToOADate())
			{
				m_width = DateTime.FromOADate(0.0).Add(value).ToOADate();
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double Width
	{
		get
		{
			return m_width;
		}
		set
		{
			if (m_width != value)
			{
				m_width = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double FixedWidth
	{
		get
		{
			return m_fixedWidth;
		}
		set
		{
			if (m_fixedWidth != value)
			{
				m_fixedWidth = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public DateTime StartDate
	{
		get
		{
			return DateTime.FromOADate(m_start);
		}
		set
		{
			if (m_start != value.ToOADate())
			{
				m_start = value.ToOADate();
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public DateTime EndDate
	{
		get
		{
			return DateTime.FromOADate(m_end);
		}
		set
		{
			if (m_end != value.ToOADate())
			{
				m_end = value.ToOADate();
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double Start
	{
		get
		{
			return m_start;
		}
		set
		{
			if (m_start != value)
			{
				m_start = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public double End
	{
		get
		{
			return m_end;
		}
		set
		{
			if (m_end != value)
			{
				m_end = value;
				RaiseChanged(this, EventArgs.Empty);
			}
		}
	}

	public event EventHandler Changed;

	public void Dispose()
	{
		if (m_font != null)
		{
			m_font.Dispose();
			m_font = null;
		}
		if (m_backImage != null)
		{
			m_backImage.Dispose();
			m_backImage = null;
		}
		m_interior = null;
	}

	internal void Draw(ChartGraph graph, RectangleF[] bounds)
	{
		using StringFormat stringformat = CreateStringFormat();
		using SolidBrush brush = new SolidBrush(m_textColor);
		for (int i = 0; i < bounds.Length; i++)
		{
			RectangleF rect = bounds[i];
			graph.PushTransform();
			if (m_backImage != null)
			{
				graph.DrawImage(m_backImage, rect);
			}
			else
			{
				graph.DrawRect(m_interior, null, rect);
			}
			if (m_vertical)
			{
				graph.MultiplyTransform(CreateVerticalTransform(rect));
				graph.DrawString(m_text, m_font, brush, new RectangleF(0f, 0f, rect.Height, rect.Width), stringformat);
			}
			else
			{
				graph.DrawString(m_text, m_font, brush, rect, stringformat);
			}
			graph.PopTransform();
		}
	}

	internal Polygon Draw(Graphics3D g3d, RectangleF[] bounds, float z)
	{
		ArrayList arrayList = new ArrayList(2 * bounds.Length);
		using (StringFormat strFormat = CreateStringFormat())
		{
			for (int i = 0; i < bounds.Length; i++)
			{
				RectangleF rect = bounds[i];
				if (m_backImage != null)
				{
					arrayList.Add(Image3D.FromImage(m_backImage, Rectangle.Round(rect), z));
				}
				else
				{
					arrayList.Add(new Polygon(new Vector3D[4]
					{
						new Vector3D(rect.Left, rect.Top, z),
						new Vector3D(rect.Right, rect.Top, z),
						new Vector3D(rect.Right, rect.Bottom, z),
						new Vector3D(rect.Left, rect.Bottom, z)
					}, m_interior));
				}
				if (m_text != null)
				{
					GraphicsPath graphicsPath = new GraphicsPath();
					if (m_vertical)
					{
						RenderingHelper.AddTextPath(graphicsPath, g3d.Graphics, m_text, m_font, new RectangleF(0f, 0f, rect.Height, rect.Width), strFormat);
						graphicsPath.Transform(CreateVerticalTransform(rect));
					}
					else
					{
						RenderingHelper.AddTextPath(graphicsPath, g3d.Graphics, m_text, m_font, rect, strFormat);
					}
					arrayList.Add(Path3D.FromGraphicsPath(graphicsPath, z, new SolidBrush(m_textColor)));
				}
			}
		}
		if (arrayList.Count != 0)
		{
			return new Path3DCollect(arrayList.ToArray(typeof(Polygon)) as Polygon[]);
		}
		return null;
	}

	private StringFormat CreateStringFormat()
	{
		StringFormat stringFormat = new StringFormat();
		stringFormat.FormatFlags = StringFormatFlags.NoClip;
		stringFormat.Trimming = StringTrimming.None;
		StringAlignment lineAlignment = StringAlignment.Center;
		StringAlignment alignment = StringAlignment.Center;
		switch (m_textAlign)
		{
		case ContentAlignment.BottomCenter:
			lineAlignment = StringAlignment.Far;
			alignment = StringAlignment.Center;
			break;
		case ContentAlignment.BottomLeft:
			lineAlignment = StringAlignment.Far;
			alignment = StringAlignment.Near;
			break;
		case ContentAlignment.BottomRight:
			lineAlignment = StringAlignment.Far;
			alignment = StringAlignment.Far;
			break;
		case ContentAlignment.MiddleCenter:
			lineAlignment = StringAlignment.Center;
			alignment = StringAlignment.Center;
			break;
		case ContentAlignment.MiddleLeft:
			lineAlignment = StringAlignment.Center;
			alignment = StringAlignment.Near;
			break;
		case ContentAlignment.MiddleRight:
			lineAlignment = StringAlignment.Center;
			alignment = StringAlignment.Far;
			break;
		case ContentAlignment.TopCenter:
			lineAlignment = StringAlignment.Near;
			alignment = StringAlignment.Center;
			break;
		case ContentAlignment.TopLeft:
			lineAlignment = StringAlignment.Near;
			alignment = StringAlignment.Near;
			break;
		case ContentAlignment.TopRight:
			lineAlignment = StringAlignment.Near;
			alignment = StringAlignment.Far;
			break;
		}
		stringFormat.LineAlignment = lineAlignment;
		stringFormat.Alignment = alignment;
		return stringFormat;
	}

	private Matrix CreateVerticalTransform(RectangleF rect)
	{
		Matrix matrix = new Matrix();
		matrix.Translate(rect.Left, rect.Bottom);
		matrix.Rotate(-90);
		return matrix;
	}

	private void RaiseChanged(object sender, EventArgs e)
	{
		if (this.Changed != null)
		{
			this.Changed(sender, e);
		}
	}
}
