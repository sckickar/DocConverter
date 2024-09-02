using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class Pseudo3DText : Polygon
{
	private Font m_font;

	private string m_text;

	private Matrix m_matrix;

	private RectangleF m_Bounds = RectangleF.Empty;

	private ContentAlignment m_alignment = ContentAlignment.BottomRight;

	public Font Font => m_font;

	public RectangleF Bounds
	{
		get
		{
			return m_Bounds;
		}
		set
		{
			m_Bounds = value;
		}
	}

	public string Text => m_text;

	public Vector3D Location => m_points[0];

	public ContentAlignment Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}

	public Matrix Matrix
	{
		get
		{
			return m_matrix;
		}
		set
		{
			m_matrix = value;
		}
	}

	public Pseudo3DText(string text, Font font, Brush br, Vector3D loc)
		: base(new Vector3D[3]
		{
			loc,
			loc + new Vector3D(1.0, 0.0, 0.0),
			loc + new Vector3D(0.0, 1.0, 0.0)
		})
	{
		m_text = text;
		m_font = font;
		m_brush = br;
		m_points = new Vector3D[3]
		{
			loc,
			loc + new Vector3D(1.0, 0.0, 0.0),
			loc + new Vector3D(0.0, 1.0, 0.0)
		};
	}

	public Pseudo3DText(string text, Font font, Brush br, Vector3D loc, RectangleF bounds)
		: base(new Vector3D[3]
		{
			loc,
			loc + new Vector3D(1.0, 0.0, 0.0),
			loc + new Vector3D(0.0, 1.0, 0.0)
		})
	{
		m_text = text;
		m_font = font;
		m_Bounds = bounds;
		m_brush = br;
		m_points = new Vector3D[3]
		{
			loc,
			loc + new Vector3D(1.0, 0.0, 0.0),
			loc + new Vector3D(0.0, 1.0, 0.0)
		};
	}

	public override void Draw(Graphics3D g3d)
	{
		Transform3D transform = g3d.Transform;
		PointF[] array = new PointF[m_points.Length];
		for (int i = 0; i < m_points.Length; i++)
		{
			array[i] = transform.ToScreen(m_points[i]);
		}
		PointF location = array[0];
		if (m_brush != null)
		{
			SizeF sizeF = (m_Bounds.Equals(RectangleF.Empty) ? g3d.Graphics.MeasureString(m_text, m_font) : m_Bounds.Size);
			switch (m_alignment)
			{
			case ContentAlignment.BottomCenter:
				location = new PointF(location.X - sizeF.Width / 2f, location.Y);
				break;
			case ContentAlignment.BottomLeft:
				location = new PointF(location.X - sizeF.Width, location.Y);
				break;
			case ContentAlignment.BottomRight:
				location = new PointF(location.X, location.Y);
				break;
			case ContentAlignment.MiddleCenter:
				location = new PointF(location.X - sizeF.Width / 2f, location.Y - sizeF.Height / 2f);
				break;
			case ContentAlignment.MiddleLeft:
				location = new PointF(location.X - sizeF.Width, location.Y - sizeF.Height / 2f);
				break;
			case ContentAlignment.MiddleRight:
				location = new PointF(location.X, location.Y - sizeF.Height / 2f);
				break;
			case ContentAlignment.TopCenter:
				location = new PointF(location.X - sizeF.Width / 2f, location.Y - sizeF.Height);
				break;
			case ContentAlignment.TopLeft:
				location = new PointF(location.X - sizeF.Width, location.Y - sizeF.Height);
				break;
			case ContentAlignment.TopRight:
				location = new PointF(location.X, location.Y - sizeF.Height);
				break;
			}
			GraphicsContainer cont = DrawingHelper.BeginTransform(g3d.Graphics);
			if (m_matrix != null)
			{
				g3d.Graphics.Transform = m_matrix;
			}
			if (m_Bounds.Equals(RectangleF.Empty))
			{
				g3d.Graphics.DrawString(m_text, m_font, m_brush, location.X, location.Y);
			}
			else
			{
				g3d.Graphics.DrawString(m_text, m_font, m_brush, new RectangleF(location, m_Bounds.Size));
			}
			DrawingHelper.EndTransform(g3d.Graphics, cont);
		}
	}

	public override Polygon Clone()
	{
		return new Pseudo3DText(Text, Font, base.Brush, Location)
		{
			Alignment = Alignment
		};
	}
}
