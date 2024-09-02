using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Styles;

namespace DocGen.Chart;

[TypeConverter(typeof(ChartInstanceConverter))]
internal class ChartCustomPoint
{
	private ChartCustomPointType m_type = ChartCustomPointType.ChartCoordinates;

	private double m_xvalue;

	private double m_yvalue;

	private string m_text = "";

	private ChartTextOrientation m_alignment = ChartTextOrientation.Center;

	private ChartSymbolInfo m_symbol = new ChartSymbolInfo();

	private bool m_drawMarker;

	private ChartFontInfo m_font = new ChartFontInfo();

	private Color m_color = SystemColors.ControlText;

	private int m_seriesIndex = -1;

	private int m_pointIndex = -1;

	private float m_offset;

	private BrushInfo m_interior = new BrushInfo(Color.Transparent);

	private ChartImageCollection m_images;

	private DrawShape shape;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartImageCollection Images
	{
		get
		{
			return m_images;
		}
		set
		{
			if (m_images != value)
			{
				m_images = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Appearance")]
	public ChartLineInfo Border
	{
		get
		{
			return m_symbol.Border;
		}
		set
		{
			m_symbol.Border = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Appearance")]
	public BrushInfo Interior
	{
		get
		{
			return m_interior;
		}
		set
		{
			m_interior = new BrushInfo(value);
			OnSettingsChanged(EventArgs.Empty);
		}
	}

	[DefaultValue(0f)]
	[Category("Data")]
	public float Offset
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
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(-1)]
	[Category("Data")]
	public int PointIndex
	{
		get
		{
			return m_pointIndex;
		}
		set
		{
			if (m_pointIndex != value)
			{
				m_pointIndex = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(-1)]
	[Category("Data")]
	public int SeriesIndex
	{
		get
		{
			return m_seriesIndex;
		}
		set
		{
			if (m_seriesIndex != value)
			{
				m_seriesIndex = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Category("Data")]
	public DateTime DateXValue
	{
		get
		{
			return DateTime.FromOADate(m_xvalue);
		}
		set
		{
			XValue = value.ToOADate();
			OnSettingsChanged(EventArgs.Empty);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Category("Data")]
	public DateTime DateYValue
	{
		get
		{
			return DateTime.FromOADate(m_yvalue);
		}
		set
		{
			YValue = value.ToOADate();
			OnSettingsChanged(EventArgs.Empty);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Appearance")]
	public ChartSymbolInfo Symbol
	{
		get
		{
			return m_symbol;
		}
		set
		{
			if (m_symbol != value)
			{
				if (m_symbol != null)
				{
					m_symbol.Changed -= OnStyleChanged;
				}
				m_symbol = value;
				if (m_symbol != null)
				{
					m_symbol.Changed += OnStyleChanged;
				}
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Category("Appearance")]
	public bool ShowMarker
	{
		get
		{
			return m_drawMarker;
		}
		set
		{
			if (m_drawMarker != value)
			{
				m_drawMarker = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(typeof(Color), "ControlText")]
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
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

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
			if (m_font != value)
			{
				if (m_font != null)
				{
					m_font.Changed -= OnStyleChanged;
				}
				m_font = value;
				if (m_font != null)
				{
					m_font.Changed += OnStyleChanged;
				}
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(ChartTextOrientation.Center)]
	[Category("Appearance")]
	public ChartTextOrientation Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			if (m_alignment != value)
			{
				m_alignment = value;
				OnSettingsChanged(EventArgs.Empty);
			}
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
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(0.0)]
	[Category("Data")]
	public double YValue
	{
		get
		{
			return m_yvalue;
		}
		set
		{
			if (m_yvalue != value)
			{
				m_yvalue = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(0.0)]
	[Category("Data")]
	public double XValue
	{
		get
		{
			return m_xvalue;
		}
		set
		{
			if (m_xvalue != value)
			{
				m_xvalue = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(ChartCustomPointType.ChartCoordinates)]
	[Category("Data")]
	public ChartCustomPointType CustomType
	{
		get
		{
			return m_type;
		}
		set
		{
			if (m_type != value)
			{
				m_type = value;
				OnSettingsChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler SettingsChanged;

	internal void Dispose()
	{
		m_symbol.Dispose();
		m_font.Dispose();
		m_images = null;
		m_symbol = null;
		m_font = null;
	}

	public DrawShape AddShape(DrawShape shape)
	{
		this.shape = shape;
		return shape;
	}

	protected internal virtual void Draw(ChartArea area, ChartGraph graph, PointF point)
	{
		using (Brush brush = new SolidBrush(m_symbol.Color))
		{
			RenderingHelper.DrawPointSymbol(graph, m_symbol.Shape, m_symbol.Marker, m_symbol.Size, m_symbol.Offset, m_symbol.ImageIndex, brush, m_symbol.Border.GdipPen, m_images, point, m_drawMarker);
		}
		if (!(m_text != "") && shape == null)
		{
			return;
		}
		SizeF sizeF = graph.MeasureString(m_text, m_font.GdipFont);
		if (shape == null)
		{
			point = GetTextPoint(point, sizeF);
			using SolidBrush brush2 = new SolidBrush(m_color);
			if (m_font.Orientation == 0)
			{
				graph.DrawString(m_text, m_font.GdipFont, brush2, new RectangleF(point, sizeF));
				return;
			}
			Matrix matrix = new Matrix();
			matrix.Translate(point.X, point.Y + sizeF.Height / 2f);
			matrix.Rotate(m_font.Orientation);
			graph.PushTransform();
			graph.Transform = matrix;
			graph.DrawString(m_text, m_font.GdipFont, brush2, new RectangleF(0f - m_offset, (0f - sizeF.Height) / 2f, sizeF.Width, sizeF.Height));
			graph.PopTransform();
			return;
		}
		string text = ((shape.Text == null || shape.Text == "") ? m_text : shape.Text);
		if (text == null || !(text != ""))
		{
			return;
		}
		SizeF sizeF2 = Size.Empty;
		string[] array = Regex.Split(text, "<br/>");
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			SizeF sizeF3 = graph.MeasureString(text2, shape.Font.GdipFont);
			if (sizeF2.Width < sizeF3.Width)
			{
				sizeF2.Width = sizeF3.Width;
			}
			sizeF2.Height += sizeF3.Height;
		}
		sizeF2.Width += 5f;
		if (shape.Size.IsEmpty)
		{
			shape.Size = sizeF2.ToSize();
		}
		switch (shape.Type)
		{
		case ChartCustomShape.Circle:
		{
			using (Brush brush9 = new SolidBrush(shape.Color))
			{
				sizeF2 = SizeF.Empty;
				sizeF2.Height = shape.Size.Height + shape.Size.Height / 3;
				sizeF2.Width = shape.Size.Width + shape.Size.Width / 4;
				shape.Size = sizeF2.ToSize();
				point = GetShapePoint(point);
				graph.DrawPath(brush9, shape.Border.GdipPen, ChartSymbolHelper.GetPathCircle(new RectangleF(point, shape.Size)));
			}
			using (Brush brush10 = new SolidBrush(shape.TextColor))
			{
				array2 = array;
				foreach (string text6 in array2)
				{
					SizeF sizeF7 = graph.MeasureString(text6, shape.Font.GdipFont);
					graph.DrawString(text6, shape.Font.GdipFont, brush10, new RectangleF(point.X + (float)(shape.Size.Width / 8), point.Y + (float)(shape.Size.Height / 8), shape.Size.Width, shape.Size.Height));
					point.Y += sizeF7.Height;
				}
			}
			break;
		}
		case ChartCustomShape.Pentagon:
		{
			using (Brush brush5 = new SolidBrush(shape.Color))
			{
				sizeF2 = SizeF.Empty;
				sizeF2.Height = shape.Size.Height + shape.Size.Height / 2;
				sizeF2.Width = shape.Size.Width + shape.Size.Width / 3;
				shape.Size = sizeF2.ToSize();
				point = GetShapePoint(point);
				graph.DrawPath(brush5, shape.Border.GdipPen, ChartSymbolHelper.GetPathPentagon(new RectangleF(point, shape.Size)));
			}
			using (Brush brush6 = new SolidBrush(shape.TextColor))
			{
				array2 = array;
				foreach (string text4 in array2)
				{
					SizeF sizeF5 = graph.MeasureString(text4, shape.Font.GdipFont);
					graph.DrawString(text4, shape.Font.GdipFont, brush6, new RectangleF(point.X + (float)(shape.Size.Width / 7), point.Y + (float)(shape.Size.Height / 8), shape.Size.Width, shape.Size.Height));
					point.Y += sizeF5.Height;
				}
			}
			break;
		}
		case ChartCustomShape.Hexagon:
		{
			using (Brush brush7 = new SolidBrush(shape.Color))
			{
				sizeF2 = SizeF.Empty;
				sizeF2.Height = shape.Size.Height + shape.Size.Height / 2;
				sizeF2.Width = shape.Size.Width + shape.Size.Width / 3;
				shape.Size = sizeF2.ToSize();
				point = GetShapePoint(point);
				graph.DrawPath(brush7, shape.Border.GdipPen, ChartSymbolHelper.GetPathHexagon(new RectangleF(point, shape.Size)));
			}
			using (Brush brush8 = new SolidBrush(shape.TextColor))
			{
				array2 = array;
				foreach (string text5 in array2)
				{
					SizeF sizeF6 = graph.MeasureString(text5, shape.Font.GdipFont);
					graph.DrawString(text5, shape.Font.GdipFont, brush8, new RectangleF(point.X + (float)(shape.Size.Width / 6), point.Y + (float)(shape.Size.Height / 6), shape.Size.Width, shape.Size.Height));
					point.Y += sizeF6.Height;
				}
			}
			break;
		}
		default:
		{
			point = GetShapePoint(point);
			using (Brush brush3 = new SolidBrush(shape.Color))
			{
				graph.DrawPath(brush3, shape.Border.GdipPen, ChartSymbolHelper.GetPathSquare(new RectangleF(point, shape.Size)));
			}
			using (Brush brush4 = new SolidBrush(shape.TextColor))
			{
				array2 = array;
				foreach (string text3 in array2)
				{
					SizeF sizeF4 = graph.MeasureString(text3, shape.Font.GdipFont);
					graph.DrawString(text3, shape.Font.GdipFont, brush4, new RectangleF(point.X, point.Y, shape.Size.Width, shape.Size.Height));
					point.Y += sizeF4.Height;
				}
			}
			break;
		}
		}
		shape.Size = Size.Empty;
	}

	protected internal virtual void Draw(ChartArea area, Graphics3D graph, Vector3D point)
	{
		Rectangle rectangle = new Rectangle((int)(point.X - (double)(m_symbol.Size.Width / 2)), (int)(point.Y - (double)(m_symbol.Size.Height / 2)), m_symbol.Size.Width, m_symbol.Size.Height);
		if (m_symbol.Shape != ChartSymbolShape.Image && m_symbol.Shape != 0)
		{
			GraphicsPath pathSymbol = ChartSymbolHelper.GetPathSymbol(m_symbol.Shape, rectangle);
			graph.AddPolygon(Path3D.FromGraphicsPath(pathSymbol, point.Z, new SolidBrush(m_symbol.Color), m_symbol.Border.GdipPen));
		}
		else if (m_symbol.ImageIndex >= 0 && m_symbol.ImageIndex < m_images.Count)
		{
			graph.AddPolygon(Image3D.FromImage(m_images[m_symbol.ImageIndex], rectangle, (float)point.Z));
		}
		if (!(m_text != ""))
		{
			return;
		}
		PointF basePoint = new PointF((float)point.X, (float)point.Y);
		SizeF sizeF = graph.Graphics.MeasureString(m_text, m_font.GdipFont);
		basePoint = GetTextPoint(basePoint, sizeF);
		if (!basePoint.IsEmpty)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			if (m_font.Orientation == 0)
			{
				graphicsPath.AddString(layoutRect: new RectangleF(basePoint, sizeF), s: m_text, family: m_font.GdipFont.GetFontFamily(), style: (int)m_font.GdipFont.Style, emSize: RenderingHelper.GetFontSizeInPixels(m_font.GdipFont), format: StringFormat.GenericDefault);
			}
			else
			{
				Matrix matrix = new Matrix();
				matrix.Translate(basePoint.X + sizeF.Width / 2f, basePoint.Y + sizeF.Height / 2f);
				matrix.Rotate(m_font.Orientation, MatrixOrder.Prepend);
				graphicsPath.AddString(layoutRect: new RectangleF(new PointF(m_offset, (0f - sizeF.Height) / 2f), sizeF), s: m_text, family: m_font.GdipFont.GetFontFamily(), style: (int)m_font.GdipFont.Style, emSize: RenderingHelper.GetFontSizeInPixels(m_font.GdipFont), format: StringFormat.GenericDefault);
				graphicsPath.Transform(matrix);
			}
			graph.AddPolygon(Path3D.FromGraphicsPath(graphicsPath, point.Z - 1.0, new SolidBrush(m_color)));
		}
	}

	protected virtual void OnSettingsChanged(EventArgs e)
	{
		if (this.SettingsChanged != null)
		{
			this.SettingsChanged(this, e);
		}
	}

	private void OnStyleChanged(object sender, StyleChangedEventArgs e)
	{
		OnSettingsChanged(EventArgs.Empty);
	}

	private PointF GetTextPoint(PointF basePoint, SizeF textSize)
	{
		PointF result = basePoint;
		switch (m_alignment)
		{
		case ChartTextOrientation.Up:
			result.X -= textSize.Width / 2f;
			result.Y -= textSize.Height;
			break;
		case ChartTextOrientation.Down:
			result.X -= textSize.Width / 2f;
			break;
		case ChartTextOrientation.Left:
			result.X -= textSize.Width;
			result.Y -= textSize.Height / 2f;
			break;
		case ChartTextOrientation.Right:
			result.Y -= textSize.Height / 2f;
			break;
		case ChartTextOrientation.UpLeft:
			result.X -= textSize.Width;
			result.Y -= textSize.Height;
			break;
		case ChartTextOrientation.DownLeft:
			result.X -= textSize.Width;
			break;
		case ChartTextOrientation.UpRight:
			result.Y -= textSize.Height;
			break;
		case ChartTextOrientation.Center:
			result.X -= textSize.Width / 2f;
			result.Y -= textSize.Height / 2f;
			break;
		case ChartTextOrientation.Smart:
			result.X -= textSize.Width / 2f;
			result.Y -= textSize.Height / 2f;
			break;
		case ChartTextOrientation.RegionUp:
			result.X -= textSize.Width / 2f;
			result.Y -= (float)(m_symbol.Size.Height / 2) + textSize.Height;
			break;
		case ChartTextOrientation.RegionDown:
			result.X -= textSize.Width / 2f;
			result.Y += m_symbol.Size.Height / 2;
			break;
		case ChartTextOrientation.RegionCenter:
		case ChartTextOrientation.SymbolCenter:
			result.X -= textSize.Width / 2f;
			result.Y -= textSize.Height / 2f;
			break;
		}
		return result;
	}

	private PointF GetShapePoint(PointF basePoint)
	{
		PointF result = basePoint;
		switch (shape.Position)
		{
		case ChartTextOrientation.Up:
			result.X -= shape.Size.Width / 2;
			result.Y -= shape.Size.Height + m_symbol.Size.Height;
			break;
		case ChartTextOrientation.Down:
			result.X -= shape.Size.Width / 2;
			result.Y += m_symbol.Size.Height;
			break;
		case ChartTextOrientation.Left:
			result.X -= shape.Size.Width + m_symbol.Size.Width;
			result.Y -= shape.Size.Height / 2;
			break;
		case ChartTextOrientation.Right:
			result.X += m_symbol.Size.Width;
			result.Y -= shape.Size.Height / 2;
			break;
		case ChartTextOrientation.UpLeft:
			result.X -= shape.Size.Width;
			result.Y -= shape.Size.Height + m_symbol.Size.Height;
			break;
		case ChartTextOrientation.UpRight:
			result.Y -= shape.Size.Height + m_symbol.Size.Height;
			break;
		case ChartTextOrientation.DownLeft:
			result.X -= shape.Size.Width;
			result.Y += m_symbol.Size.Height;
			break;
		case ChartTextOrientation.DownRight:
			result.Y += m_symbol.Size.Height;
			break;
		case ChartTextOrientation.Center:
			result.X -= shape.Size.Width / 2;
			result.Y -= shape.Size.Height / 2;
			break;
		case ChartTextOrientation.Smart:
			result.X -= shape.Size.Width / 2;
			result.Y -= shape.Size.Height / 2;
			break;
		case ChartTextOrientation.RegionUp:
			result.X -= shape.Size.Width / 2;
			result.Y -= shape.Size.Height + m_symbol.Size.Height;
			break;
		case ChartTextOrientation.RegionDown:
			result.X -= shape.Size.Width / 2;
			result.Y += m_symbol.Size.Height;
			break;
		case ChartTextOrientation.RegionCenter:
		case ChartTextOrientation.SymbolCenter:
			result.X -= shape.Size.Width / 2;
			result.Y -= shape.Size.Height / 2;
			break;
		}
		return result;
	}
}
