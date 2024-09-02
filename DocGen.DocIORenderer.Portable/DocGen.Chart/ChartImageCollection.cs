using System.Collections;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartImageCollection : CollectionBase
{
	public DocGen.Drawing.Image this[int index] => base.List[index] as DocGen.Drawing.Image;

	public ChartImageCollection()
	{
	}

	public ChartImageCollection(IList source)
	{
		foreach (DocGen.Drawing.Image item in source)
		{
			if (item != null)
			{
				Add(item);
			}
		}
	}

	public void MakeTransparent(Color color)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			DocGen.Drawing.Image image = this[i];
			Bitmap bitmap = new Bitmap((Bitmap)image);
			bitmap.MakeTransparent(color);
			base.List[i] = bitmap;
		}
	}

	public int Add(DocGen.Drawing.Image image)
	{
		return base.List.Add(image);
	}

	public void AddFromList(IList list)
	{
		foreach (DocGen.Drawing.Image item in list)
		{
			if (item != null)
			{
				Add(item);
			}
		}
	}

	public void AddRange(DocGen.Drawing.Image[] image)
	{
		base.InnerList.AddRange(image);
	}

	public void Remove(DocGen.Drawing.Image image)
	{
		base.List.Remove(image);
	}

	public bool Contains(DocGen.Drawing.Image image)
	{
		return IndexOf(image) != -1;
	}

	public int IndexOf(DocGen.Drawing.Image image)
	{
		return base.List.IndexOf(image);
	}

	public void Insert(int index, DocGen.Drawing.Image image)
	{
		base.List.Insert(index, image);
	}

	public void CopyTo(DocGen.Drawing.Image[] images, int index)
	{
		base.List.CopyTo(images, index);
	}
}
