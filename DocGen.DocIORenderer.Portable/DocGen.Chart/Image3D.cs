using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class Image3D : Polygon
{
	private DocGen.Drawing.Image m_image;

	private ImageAttributes m_attributes;

	public ImageAttributes Attributes
	{
		get
		{
			return m_attributes;
		}
		set
		{
			m_attributes = value;
		}
	}

	public Image3D(Vector3D[] vs, DocGen.Drawing.Image img)
		: base(vs)
	{
		m_image = img;
	}

	public override void Draw(Graphics3D g3d)
	{
		Transform3D transform = g3d.Transform;
		PointF[] array = new PointF[m_points.Length];
		for (int i = 0; i < m_points.Length; i++)
		{
			array[i] = transform.ToScreen(m_points[i]);
		}
		if (m_attributes == null)
		{
			g3d.Graphics.DrawImage(m_image, new PointF[3]
			{
				array[0],
				array[1],
				array[2]
			});
		}
		else
		{
			g3d.Graphics.DrawImage(m_image, new PointF[3]
			{
				array[0],
				array[1],
				array[2]
			}, new Rectangle(0, 0, m_image.Width, m_image.Height), g3d.Graphics.PageUnit, m_attributes);
		}
	}

	public static Image3D FromImage(DocGen.Drawing.Image image, RectangleF bounds, float z)
	{
		return new Image3D(new Vector3D[3]
		{
			new Vector3D(bounds.Left, bounds.Top, z),
			new Vector3D(bounds.Right, bounds.Top, z),
			new Vector3D(bounds.Left, bounds.Bottom, z)
		}, image);
	}

	public override Polygon Clone()
	{
		return new Image3D(Points.Clone() as Vector3D[], m_image);
	}
}
