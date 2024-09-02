using System;

namespace DocGen.Pdf;

internal abstract class SubbandROIMask
{
	internal SubbandROIMask ll;

	internal SubbandROIMask lh;

	internal SubbandROIMask hl;

	internal SubbandROIMask hh;

	internal bool isNode;

	public int ulx;

	public int uly;

	public int w;

	public int h;

	public SubbandROIMask(int ulx, int uly, int w, int h)
	{
		this.ulx = ulx;
		this.uly = uly;
		this.w = w;
		this.h = h;
	}

	public virtual SubbandROIMask getSubbandRectROIMask(int x, int y)
	{
		if (x < ulx || y < uly || x >= ulx + w || y >= uly + h)
		{
			throw new ArgumentException();
		}
		SubbandROIMask subbandROIMask = this;
		while (subbandROIMask.isNode)
		{
			SubbandROIMask subbandROIMask2 = subbandROIMask.hh;
			subbandROIMask = ((x >= subbandROIMask2.ulx) ? ((y >= subbandROIMask2.uly) ? subbandROIMask.hh : subbandROIMask.hl) : ((y >= subbandROIMask2.uly) ? subbandROIMask.lh : subbandROIMask.ll));
		}
		return subbandROIMask;
	}
}
