using System;
using System.ComponentModel;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartWatermark
{
	private const float c_persentToByte = 2.55f;

	private string m_text = string.Empty;

	private readonly ChartArea m_area;

	private ChartAlignment m_verticalAlignment;

	private ChartAlignment m_horizontalAlignment;

	private Font m_font;

	private Color m_color = Color.Empty;

	private DocGen.Drawing.Image m_image;

	private Size? m_imageSize;

	private ChartWaterMarkOrder m_zOrder;

	private ChartThickness m_margin = new ChartThickness(10f);

	private float m_opacity = 60f;

	[DefaultValue("")]
	[Description("Specifies the title text of watermark")]
	[NotifyParentProperty(true)]
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

	[Description("Specifies the title font of watermark")]
	[NotifyParentProperty(true)]
	public Font Font
	{
		get
		{
			if (m_font != null)
			{
				return m_font;
			}
			return m_area.Chart.Font;
		}
		set
		{
			if (m_font != value)
			{
				m_font = value;
			}
		}
	}

	[DefaultValue(ChartAlignment.Near)]
	[Description("Indicates the vertical alignment of watermark")]
	[NotifyParentProperty(true)]
	public ChartAlignment VerticalAlignment
	{
		get
		{
			return m_verticalAlignment;
		}
		set
		{
			if (m_verticalAlignment != value)
			{
				m_verticalAlignment = value;
			}
		}
	}

	[DefaultValue(ChartAlignment.Near)]
	[Description("Indicates the horizontal alignment of watermark")]
	[NotifyParentProperty(true)]
	public ChartAlignment HorizontalAlignment
	{
		get
		{
			return m_horizontalAlignment;
		}
		set
		{
			if (m_horizontalAlignment != value)
			{
				m_horizontalAlignment = value;
			}
		}
	}

	[Description("Indicates the title color of watermark")]
	[NotifyParentProperty(true)]
	public Color TextColor
	{
		get
		{
			if (!m_color.IsEmpty)
			{
				return m_color;
			}
			return m_area.Chart.ForeColor;
		}
		set
		{
			if (m_color != value)
			{
				m_color = value;
			}
		}
	}

	[DefaultValue(null)]
	[Description("Specifies the image of watermark")]
	[NotifyParentProperty(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public DocGen.Drawing.Image Image
	{
		get
		{
			return m_image;
		}
		set
		{
			if (m_image != value)
			{
				m_image = value;
			}
		}
	}

	[Browsable(false)]
	[DefaultValue(null)]
	[Description("Indicates the image size of watermark")]
	[NotifyParentProperty(true)]
	public Size? ImageSize
	{
		get
		{
			return m_imageSize;
		}
		set
		{
			if (m_imageSize != value)
			{
				m_imageSize = value;
			}
		}
	}

	[DefaultValue(ChartWaterMarkOrder.Over)]
	[Description("Indicates the depth order of watermark")]
	[NotifyParentProperty(true)]
	public ChartWaterMarkOrder ZOrder
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
			}
		}
	}

	[DefaultValue(typeof(ChartThickness), "10; 10; 10; 10")]
	[Description("Indicates the margin of watermark")]
	[NotifyParentProperty(true)]
	public ChartThickness Margin
	{
		get
		{
			return m_margin;
		}
		set
		{
			if (m_margin != value)
			{
				m_margin = value;
			}
		}
	}

	[DefaultValue(60f)]
	[Description("Indicates the title opacity of watermark")]
	[NotifyParentProperty(true)]
	public float Opacity
	{
		get
		{
			return m_opacity;
		}
		set
		{
			if (m_opacity != value)
			{
				m_opacity = value;
			}
		}
	}

	internal bool IsVisible
	{
		get
		{
			if (m_image == null)
			{
				return !string.IsNullOrEmpty(m_text);
			}
			return true;
		}
	}

	internal ChartWatermark(ChartArea chartArea)
	{
		m_area = chartArea;
	}

	internal void Draw(ChartGraph graph, RectangleF bounds)
	{
		ComputeBounds(graph, bounds, out var imageBounds, out var textBounds);
		if (m_image != null)
		{
			graph.DrawImage(m_image, imageBounds);
		}
		if (!string.IsNullOrEmpty(m_text))
		{
			using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(2.55f * m_opacity), TextColor)))
			{
				graph.DrawString(m_text, Font, brush, textBounds);
			}
		}
	}

	internal Polygon Draw(Graphics3D g3d, RectangleF bounds, float z)
	{
		Polygon polygon = null;
		Polygon polygon2 = null;
		ComputeBounds(new ChartGDIGraph(g3d.Graphics), bounds, out var imageBounds, out var textBounds);
		if (m_image != null)
		{
			polygon = Image3D.FromImage(m_image, imageBounds, z);
		}
		if (!string.IsNullOrEmpty(m_text))
		{
			GraphicsPath gp = new GraphicsPath();
			RenderingHelper.AddTextPath(gp, g3d.Graphics, m_text, Font, textBounds);
			polygon2 = Path3D.FromGraphicsPath(gp, z, new SolidBrush(Color.FromArgb((int)(2.55f * m_opacity), TextColor)));
		}
		if (polygon == null)
		{
			return polygon2;
		}
		if (polygon2 == null)
		{
			return polygon;
		}
		return new Path3DCollect(new Polygon[2] { polygon, polygon2 });
	}

	private void ComputeBounds(ChartGraph graph, RectangleF bounds, out RectangleF imageBounds, out RectangleF textBounds)
	{
		bounds = m_margin.Deflate(bounds);
		SizeF sizeF = default(SizeF);
		SizeF size = default(SizeF);
		if (m_image != null)
		{
			size = ((!m_imageSize.HasValue) ? new SizeF(m_image.Width, m_image.Height) : ((SizeF)m_imageSize.Value));
		}
		SizeF size2 = graph.MeasureString(m_text, Font, bounds.Width - size.Width);
		bounds = LayoutHelper.AlignRectangle(size: new SizeF(size.Width + size2.Width, Math.Max(size.Height, size2.Height)), bounds: bounds, horizontal: m_horizontalAlignment, vertical: m_verticalAlignment);
		imageBounds = LayoutHelper.AlignRectangle(bounds, size, ContentAlignment.MiddleLeft);
		textBounds = LayoutHelper.AlignRectangle(bounds, size2, ContentAlignment.MiddleRight);
	}

	private bool ShouldSerializeFont()
	{
		return m_font != null;
	}

	private bool ShouldSerializeTextColor()
	{
		return !m_color.IsEmpty;
	}

	private void ResetFont()
	{
		Font = null;
	}

	private void ResetTextColor()
	{
		m_color = Color.Empty;
	}
}
