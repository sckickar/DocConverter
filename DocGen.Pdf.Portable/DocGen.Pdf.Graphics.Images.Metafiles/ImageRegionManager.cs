using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Images.Metafiles;

internal class ImageRegionManager
{
	private List<ImageRegion> m_regions;

	public ImageRegionManager()
	{
		m_regions = new List<ImageRegion>();
	}

	public void Add(ImageRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		ImageRegion[] regions = Intersect(region);
		ImageRegion item = Union(regions, region);
		m_regions.Add(item);
		Remove(regions);
	}

	public float GetTopCoordinate(float y)
	{
		float result = y;
		ImageRegion region = new ImageRegion(y, 1f);
		ImageRegion[] array = Intersect(region);
		if (array != null && array.Length == 1)
		{
			result = array[0].Y;
		}
		else if (array != null && array.Length > 1)
		{
			ImageRegion imageRegion = array[0];
			ImageRegion imageRegion2 = array[1];
			result = ((!(imageRegion.Y < imageRegion2.Y)) ? imageRegion2.Y : imageRegion.Y);
		}
		return result;
	}

	public float GetCoordinate(float y)
	{
		float result = y;
		ImageRegion region = new ImageRegion(y, 1f);
		ImageRegion[] array = Intersect(region);
		if (array != null && array.Length == 1)
		{
			result = array[0].Y;
		}
		else if (array != null && array.Length > 1)
		{
			ImageRegion imageRegion = array[0];
			ImageRegion imageRegion2 = array[1];
			result = ((!(imageRegion.Y < imageRegion2.Y)) ? imageRegion2.Y : imageRegion.Y);
		}
		else if (array == null)
		{
			result = 0f;
		}
		return result;
	}

	public void Clear()
	{
		m_regions.Clear();
	}

	private ImageRegion[] Intersect(ImageRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		List<ImageRegion> list = new List<ImageRegion>();
		int i = 0;
		for (int count = m_regions.Count; i < count; i++)
		{
			ImageRegion imageRegion = m_regions[i];
			if (region.IntersectsWith(imageRegion))
			{
				list.Add(imageRegion);
			}
		}
		return list.ToArray();
	}

	private void Remove(ImageRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		m_regions.Remove(region);
	}

	private void Remove(ImageRegion[] regions)
	{
		if (regions == null)
		{
			throw new ArgumentNullException("regions");
		}
		int i = 0;
		for (int num = regions.Length; i < num; i++)
		{
			ImageRegion item = regions[i];
			m_regions.Remove(item);
		}
	}

	private ImageRegion Union(ImageRegion[] regions, ImageRegion region)
	{
		if (regions == null)
		{
			throw new ArgumentNullException("regions");
		}
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		int i = 0;
		for (int num = regions.Length; i < num; i++)
		{
			ImageRegion imageRegion = regions[i];
			if (region.IntersectsWith(imageRegion))
			{
				region = ImageRegion.Union(region, imageRegion);
			}
		}
		return region;
	}
}
