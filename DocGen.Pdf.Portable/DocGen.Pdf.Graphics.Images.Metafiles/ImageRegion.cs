using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics.Images.Metafiles;

internal class ImageRegion
{
	private float m_y;

	private float m_height;

	public float Y
	{
		get
		{
			return m_y;
		}
		set
		{
			m_y = value;
		}
	}

	public float Height
	{
		get
		{
			return m_height;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("value", "Height can not be less 0");
			}
			m_height = value;
		}
	}

	public ImageRegion()
	{
	}

	public ImageRegion(float y, float height)
	{
		if (height < 0f)
		{
			throw new ArgumentOutOfRangeException("height", "Value can not be less 0");
		}
		Y = y;
		Height = height;
	}

	public static ImageRegion Union(ImageRegion region1, ImageRegion region2)
	{
		if (region1 == null)
		{
			throw new ArgumentNullException("region1");
		}
		if (region2 == null)
		{
			throw new ArgumentNullException("region2");
		}
		if (!region1.IntersectsWith(region2))
		{
			throw new ArgumentException("The specified regions don't intersect");
		}
		ImageRegion imageRegion = new ImageRegion();
		float num = Math.Min(region1.Y, region2.Y);
		float height = Math.Max(region1.Y + region1.Height, region2.Y + region2.Height) - num;
		imageRegion.Y = num;
		imageRegion.Height = height;
		return imageRegion;
	}

	public bool IntersectsWith(ImageRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		RectangleF sourceRect = new RectangleF(0f, Y, 1f, Height);
		RectangleF rect = new RectangleF(0f, region.Y, 1f, region.Height);
		return IntersectsWith(sourceRect, rect);
	}

	internal bool IntersectsWith(RectangleF sourceRect, RectangleF rect)
	{
		if (rect.X < sourceRect.X + sourceRect.Width && sourceRect.X < rect.X + rect.Width && rect.Y < sourceRect.Y + sourceRect.Height)
		{
			return sourceRect.Y < rect.Y + rect.Height;
		}
		return false;
	}
}
