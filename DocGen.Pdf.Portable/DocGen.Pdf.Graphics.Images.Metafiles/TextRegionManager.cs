using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Images.Metafiles;

internal class TextRegionManager
{
	private List<TextRegion> m_regions;

	internal int Count => m_regions.Count;

	public TextRegionManager()
	{
		m_regions = new List<TextRegion>();
	}

	public void Add(TextRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		TextRegion[] regions = Intersect(region);
		TextRegion item = Union(regions, region);
		m_regions.Add(item);
		Remove(regions);
	}

	public float GetTopCoordinate(float y)
	{
		float result = y;
		TextRegion region = new TextRegion(y, 1f);
		TextRegion[] array = Intersect(region);
		if (array != null && array.Length == 1)
		{
			result = array[0].Y;
		}
		else if (array != null && array.Length > 1)
		{
			TextRegion textRegion = array[0];
			TextRegion textRegion2 = array[1];
			result = ((!(textRegion.Y < textRegion2.Y)) ? textRegion2.Y : textRegion.Y);
		}
		return result;
	}

	public float GetCoordinate(float y)
	{
		float result = y;
		TextRegion region = new TextRegion(y, 1f);
		TextRegion[] array = Intersect(region);
		if (array != null && array.Length == 1)
		{
			result = array[0].Y;
		}
		else if (array != null && array.Length > 1)
		{
			TextRegion textRegion = array[0];
			TextRegion textRegion2 = array[1];
			result = ((!(textRegion.Y < textRegion2.Y)) ? textRegion2.Y : textRegion.Y);
		}
		else if (array.Length == 0)
		{
			result = 0f;
		}
		return result;
	}

	public void Clear()
	{
		m_regions.Clear();
	}

	private TextRegion[] Intersect(TextRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		List<TextRegion> list = new List<TextRegion>();
		int i = 0;
		for (int count = m_regions.Count; i < count; i++)
		{
			TextRegion textRegion = m_regions[i];
			if (region.IntersectsWith(textRegion))
			{
				list.Add(textRegion);
			}
		}
		return list.ToArray();
	}

	private void Remove(TextRegion region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		m_regions.Remove(region);
	}

	private void Remove(TextRegion[] regions)
	{
		if (regions == null)
		{
			throw new ArgumentNullException("regions");
		}
		int i = 0;
		for (int num = regions.Length; i < num; i++)
		{
			TextRegion item = regions[i];
			m_regions.Remove(item);
		}
	}

	private TextRegion Union(TextRegion[] regions, TextRegion region)
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
			TextRegion textRegion = regions[i];
			if (region.IntersectsWith(textRegion))
			{
				region = TextRegion.Union(region, textRegion);
			}
		}
		return region;
	}
}
