using SkiaSharp;

namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class Region : SKRegion
{
	public Region()
	{
	}

	public Region(GraphicsPath path)
	{
		SetPath(path);
	}

	public Region(Rectangle rect)
	{
		SKRectI rect2 = RenderHelper.SKRectI(rect);
		SetRect(rect2);
	}

	~Region()
	{
		Dispose();
	}

	public Region Clone()
	{
		Region region = new Region();
		region.SetRegion(this);
		return region;
	}

	public void Translate(float dx, float dy)
	{
		SKRectI bounds = base.Bounds;
		bounds.Offset((int)dx, (int)dy);
		SetRect(bounds);
	}

	public void Exclude(Region region)
	{
		Op(region, SKRegionOperation.Difference);
	}

	public void Intersect(GraphicsPath path)
	{
		SKRegion sKRegion = new SKRegion();
		sKRegion.SetPath(path);
		Op(sKRegion, SKRegionOperation.Intersect);
	}

	public void Intersect(Rectangle rect)
	{
		SKRegion sKRegion = new SKRegion();
		sKRegion.SetRect(RenderHelper.SKRectI(rect));
		Op(sKRegion, SKRegionOperation.Intersect);
	}

	public void Union(GraphicsPath path)
	{
		SKRegion sKRegion = new SKRegion();
		sKRegion.SetPath(path);
		Op(sKRegion, SKRegionOperation.Union);
	}

	public void Union(Rectangle rect)
	{
		SKRegion sKRegion = new SKRegion();
		sKRegion.SetRect(RenderHelper.SKRectI(rect));
		Op(sKRegion, SKRegionOperation.Union);
	}

	public void Union(RectangleF rect)
	{
		SKRegion sKRegion = new SKRegion();
		sKRegion.SetRect(RenderHelper.SKRectI(rect));
		Op(sKRegion, SKRegionOperation.Union);
	}

	public void Intersect(RectangleF rect)
	{
		SKRegion sKRegion = new SKRegion();
		sKRegion.SetRect(RenderHelper.SKRectI(rect));
		Op(sKRegion, SKRegionOperation.Intersect);
	}
}
