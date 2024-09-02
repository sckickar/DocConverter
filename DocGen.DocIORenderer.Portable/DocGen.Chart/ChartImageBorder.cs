using System.Resources;
using System.Xml;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartImageBorder
{
	private const float c_byteToFloat = 0.003921569f;

	private Bitmap m_sourceImage;

	private RectangleF m_bounds = RectangleF.Empty;

	private Rectangle m_leftTopRect = Rectangle.Empty;

	private Rectangle m_leftBottomRect = Rectangle.Empty;

	private Rectangle m_rightTopRect = Rectangle.Empty;

	private Rectangle m_rightBottomRect = Rectangle.Empty;

	private Rectangle m_topRect = Rectangle.Empty;

	private Rectangle m_bottomRect = Rectangle.Empty;

	private Rectangle m_leftRect = Rectangle.Empty;

	private Rectangle m_rightRect = Rectangle.Empty;

	private RectangleF m_destLeftTopRect = RectangleF.Empty;

	private Region m_tlRegion;

	private Region m_trRegion;

	private Region m_blRegion;

	private Region m_brRegion;

	private ChartThickness m_thickness;

	private ChartThickness m_padding;

	private bool m_supportBaseColor;

	public ChartThickness Padding => m_padding;

	public ChartImageBorder(ResourceManager resources, string name)
	{
		XmlDocument xmlDocument = new XmlDocument();
		if (name != null)
		{
			xmlDocument.LoadXml(resources.GetString(name));
		}
		XmlElement documentElement = xmlDocument.DocumentElement;
		XmlElement xmlElement = documentElement["LeftTop"];
		XmlElement xmlElement2 = documentElement["RightTop"];
		XmlElement xmlElement3 = documentElement["LeftBottom"];
		XmlElement xmlElement4 = documentElement["RightBottom"];
		XmlElement xmlElement5 = documentElement["Left"];
		XmlElement xmlElement6 = documentElement["Top"];
		XmlElement xmlElement7 = documentElement["Right"];
		XmlElement xmlElement8 = documentElement["Bottom"];
		m_sourceImage = resources.GetObject(documentElement.GetAttribute("image")) as Bitmap;
		m_thickness = ChartThickness.Parse(documentElement.GetAttribute("thickness"));
		m_padding = ChartThickness.Parse(documentElement.GetAttribute("padding"));
		m_leftTopRect = ParseRectangle(xmlElement.GetAttribute("rect"));
		m_leftBottomRect = ParseRectangle(xmlElement3.GetAttribute("rect"));
		m_rightTopRect = ParseRectangle(xmlElement2.GetAttribute("rect"));
		m_rightBottomRect = ParseRectangle(xmlElement4.GetAttribute("rect"));
		m_topRect = ParseRectangle(xmlElement6.GetAttribute("rect"));
		m_leftRect = ParseRectangle(xmlElement5.GetAttribute("rect"));
		m_rightRect = ParseRectangle(xmlElement7.GetAttribute("rect"));
		m_bottomRect = ParseRectangle(xmlElement8.GetAttribute("rect"));
		Color maskColor = ParseColor(documentElement.GetAttribute("maskcolor"));
		m_tlRegion = GetRegion(m_sourceImage, m_leftTopRect, maskColor);
		m_blRegion = GetRegion(m_sourceImage, m_leftBottomRect, maskColor);
		m_trRegion = GetRegion(m_sourceImage, m_rightTopRect, maskColor);
		m_brRegion = GetRegion(m_sourceImage, m_rightBottomRect, maskColor);
	}

	public void Draw(Graphics g, RectangleF rect, Color baseColor)
	{
		ImageAttributes attr = (m_supportBaseColor ? GetRecolorAttributes(baseColor) : null);
		RectangleF destRect = new RectangleF(rect.Left, rect.Top, m_thickness.Left, m_thickness.Top);
		RectangleF destRect2 = new RectangleF(rect.Right - m_thickness.Right, rect.Y, m_thickness.Right, m_thickness.Top);
		RectangleF destRect3 = new RectangleF(rect.Left, rect.Bottom - m_thickness.Bottom, m_thickness.Left, m_thickness.Bottom);
		RectangleF destRect4 = new RectangleF(rect.Right - m_thickness.Right, rect.Bottom - m_thickness.Bottom, m_thickness.Right, m_thickness.Bottom);
		RectangleF destRect5 = new RectangleF(rect.Left, rect.Top + m_thickness.Top, m_thickness.Left, rect.Height - m_thickness.Top - m_thickness.Bottom);
		RectangleF destRect6 = new RectangleF(rect.Left + m_thickness.Left, rect.Top, rect.Width - m_thickness.Left - m_thickness.Right, m_thickness.Top);
		RectangleF destRect7 = new RectangleF(rect.Right - m_thickness.Right, rect.Top + m_thickness.Top, m_thickness.Left, rect.Height - m_thickness.Top - m_thickness.Bottom);
		RectangleF destRect8 = new RectangleF(rect.Left + m_thickness.Left, rect.Bottom - m_thickness.Bottom, rect.Width - m_thickness.Left - m_thickness.Right, m_thickness.Bottom);
		DrawImage(g, m_leftRect, destRect5, attr);
		DrawImage(g, m_topRect, destRect6, attr);
		DrawImage(g, m_rightRect, destRect7, attr);
		DrawImage(g, m_bottomRect, destRect8, attr);
		DrawImage(g, m_leftTopRect, destRect, attr);
		DrawImage(g, m_leftBottomRect, destRect3, attr);
		DrawImage(g, m_rightTopRect, destRect2, attr);
		DrawImage(g, m_rightBottomRect, destRect4, attr);
	}

	public void Build(RectangleF rect)
	{
		if (m_bounds != rect)
		{
			m_bounds = rect;
			m_destLeftTopRect = new RectangleF(rect.Left, rect.Top, m_thickness.Left, m_thickness.Top);
		}
	}

	public Region GetRegion(RectangleF rect)
	{
		RectangleF rectangleF = m_thickness.Deflate(rect);
		Region region = new Region(rect);
		Region region2 = m_tlRegion.Clone();
		Region region3 = m_trRegion.Clone();
		Region region4 = m_blRegion.Clone();
		Region region5 = m_brRegion.Clone();
		region2.Translate(rect.Left, rect.Top);
		region3.Translate(rectangleF.Right, rect.Top);
		region4.Translate(rect.Left, rectangleF.Bottom);
		region5.Translate(rectangleF.Right, rectangleF.Bottom);
		region.Exclude(region2);
		region.Exclude(region3);
		region.Exclude(region4);
		region.Exclude(region5);
		return region;
	}

	private void DrawImage(Graphics g, RectangleF srcRect, RectangleF destRect, ImageAttributes attr)
	{
		if (attr == null)
		{
			g.DrawImage(m_sourceImage, destRect, srcRect, GraphicsUnit.Pixel);
		}
		else
		{
			g.DrawImage(m_sourceImage, Rectangle.Truncate(destRect), srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, g.PageUnit, attr);
		}
	}

	private ImageAttributes GetRecolorAttributes(Color color)
	{
		ImageAttributes imageAttributes = new ImageAttributes();
		ColorMatrix newColorMatrix = new ColorMatrix
		{
			Matrix00 = 0.003921569f * (float)(int)color.R,
			Matrix11 = 0.003921569f * (float)(int)color.G,
			Matrix22 = 0.003921569f * (float)(int)color.B,
			Matrix33 = 0.003921569f * (float)(int)color.A
		};
		imageAttributes.SetWrapMode(WrapMode.Tile);
		imageAttributes.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		return imageAttributes;
	}

	private Region GetRegion(Bitmap bmp, Rectangle rect, Color maskColor)
	{
		Region region = new Region(Rectangle.Empty);
		for (int i = 0; i < rect.Width; i++)
		{
			for (int j = 0; j < rect.Height; j++)
			{
				if (bmp.GetPixel(rect.X + i, rect.Y + j).ToArgb() == maskColor.ToArgb())
				{
					region.Union(new Rectangle(i, j, 1, 1));
				}
			}
		}
		return region;
	}

	private Rectangle ParseRectangle(string text)
	{
		string[] array = text.Split(';');
		return new Rectangle(int.Parse(array[0].Trim()), int.Parse(array[1].Trim()), int.Parse(array[2].Trim()), int.Parse(array[3].Trim()));
	}

	private Color ParseColor(string text)
	{
		string[] array = text.Split(';');
		return Color.FromArgb(int.Parse(array[0].Trim()), int.Parse(array[1].Trim()), int.Parse(array[2].Trim()));
	}
}
