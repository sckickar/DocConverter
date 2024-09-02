using System;

namespace DocGen.Pdf;

internal abstract class Subband
{
	public const int WT_ORIENT_LL = 0;

	public const int WT_ORIENT_HL = 1;

	public const int WT_ORIENT_LH = 2;

	public const int WT_ORIENT_HH = 3;

	public bool isNode;

	public int orientation;

	public int level;

	public int resLvl;

	internal JPXImageCoordinates numCb;

	public int anGainExp;

	public int sbandIdx;

	public int ulcx;

	public int ulcy;

	public int ulx;

	public int uly;

	public int w;

	public int h;

	public int nomCBlkW;

	public int nomCBlkH;

	public abstract Subband Parent { get; }

	public abstract Subband LL { get; }

	public abstract Subband HL { get; }

	public abstract Subband LH { get; }

	public abstract Subband HH { get; }

	public virtual Subband NextResLevel
	{
		get
		{
			if (level == 0)
			{
				return null;
			}
			Subband subband = this;
			do
			{
				subband = subband.Parent;
				if (subband == null)
				{
					return null;
				}
			}
			while (subband.resLvl == resLvl);
			subband = subband.HL;
			while (subband.isNode)
			{
				subband = subband.LL;
			}
			return subband;
		}
	}

	internal abstract WaveletFilter HorWFilter { get; }

	internal abstract WaveletFilter VerWFilter { get; }

	internal abstract Subband split(WaveletFilter hfilter, WaveletFilter vfilter);

	internal virtual void initChilds()
	{
		Subband lL = LL;
		Subband hL = HL;
		Subband lH = LH;
		Subband hH = HH;
		lL.level = level + 1;
		lL.ulcx = ulcx + 1 >> 1;
		lL.ulcy = ulcy + 1 >> 1;
		lL.ulx = ulx;
		lL.uly = uly;
		lL.w = (ulcx + w + 1 >> 1) - lL.ulcx;
		lL.h = (ulcy + h + 1 >> 1) - lL.ulcy;
		lL.resLvl = ((orientation == 0) ? (resLvl - 1) : resLvl);
		lL.anGainExp = anGainExp;
		lL.sbandIdx = sbandIdx << 2;
		hL.orientation = 1;
		hL.level = lL.level;
		hL.ulcx = ulcx >> 1;
		hL.ulcy = lL.ulcy;
		hL.ulx = ulx + lL.w;
		hL.uly = uly;
		hL.w = (ulcx + w >> 1) - hL.ulcx;
		hL.h = lL.h;
		hL.resLvl = resLvl;
		hL.anGainExp = anGainExp + 1;
		hL.sbandIdx = (sbandIdx << 2) + 1;
		lH.orientation = 2;
		lH.level = lL.level;
		lH.ulcx = lL.ulcx;
		lH.ulcy = ulcy >> 1;
		lH.ulx = ulx;
		lH.uly = uly + lL.h;
		lH.w = lL.w;
		lH.h = (ulcy + h >> 1) - lH.ulcy;
		lH.resLvl = resLvl;
		lH.anGainExp = anGainExp + 1;
		lH.sbandIdx = (sbandIdx << 2) + 2;
		hH.orientation = 3;
		hH.level = lL.level;
		hH.ulcx = hL.ulcx;
		hH.ulcy = lH.ulcy;
		hH.ulx = hL.ulx;
		hH.uly = lH.uly;
		hH.w = hL.w;
		hH.h = lH.h;
		hH.resLvl = resLvl;
		hH.anGainExp = anGainExp + 2;
		hH.sbandIdx = (sbandIdx << 2) + 3;
	}

	public Subband()
	{
	}

	internal Subband(int w, int h, int ulcx, int ulcy, int lvls, WaveletFilter[] hfilters, WaveletFilter[] vfilters)
	{
		this.w = w;
		this.h = h;
		this.ulcx = ulcx;
		this.ulcy = ulcy;
		resLvl = lvls;
		Subband subband = this;
		for (int i = 0; i < lvls; i++)
		{
			int num = ((subband.resLvl <= hfilters.Length) ? (subband.resLvl - 1) : (hfilters.Length - 1));
			int num2 = ((subband.resLvl <= vfilters.Length) ? (subband.resLvl - 1) : (vfilters.Length - 1));
			subband = subband.split(hfilters[num], vfilters[num2]);
		}
	}

	public virtual Subband nextSubband()
	{
		if (isNode)
		{
			throw new ArgumentException();
		}
		switch (orientation)
		{
		case 0:
		{
			Subband subband = Parent;
			if (subband == null || subband.resLvl != resLvl)
			{
				return null;
			}
			return subband.HL;
		}
		case 1:
			return Parent.LH;
		case 2:
			return Parent.HH;
		case 3:
		{
			Subband subband = this;
			while (subband.orientation == 3)
			{
				subband = subband.Parent;
			}
			switch (subband.orientation)
			{
			case 0:
				subband = subband.Parent;
				if (subband == null || subband.resLvl != resLvl)
				{
					return null;
				}
				subband = subband.HL;
				break;
			case 1:
				subband = subband.Parent.LH;
				break;
			case 2:
				subband = subband.Parent.HH;
				break;
			default:
				throw new ArgumentException();
			}
			while (subband.isNode)
			{
				subband = subband.LL;
			}
			return subband;
		}
		default:
			throw new ArgumentException();
		}
	}

	public virtual Subband getSubbandByIdx(int rl, int sbi)
	{
		Subband subband = this;
		if (rl > subband.resLvl || rl < 0)
		{
			throw new ArgumentException("Resolution level index out of range");
		}
		if (rl == subband.resLvl && sbi == subband.sbandIdx)
		{
			return subband;
		}
		if (subband.sbandIdx != 0)
		{
			subband = subband.Parent;
		}
		while (subband.resLvl > rl)
		{
			subband = subband.LL;
		}
		while (subband.resLvl < rl)
		{
			subband = subband.Parent;
		}
		return sbi switch
		{
			1 => subband.HL, 
			2 => subband.LH, 
			3 => subband.HH, 
			_ => subband, 
		};
	}

	public virtual Subband getSubband(int x, int y)
	{
		if (x < ulx || y < uly || x >= ulx + w || y >= uly + h)
		{
			throw new ArgumentException();
		}
		Subband subband = this;
		while (subband.isNode)
		{
			Subband hH = subband.HH;
			subband = ((x >= hH.ulx) ? ((y >= hH.uly) ? subband.HH : subband.HL) : ((y >= hH.uly) ? subband.LH : subband.LL));
		}
		return subband;
	}

	public override string ToString()
	{
		return "w=" + w + ",h=" + h + ",ulx=" + ulx + ",uly=" + uly + ",ulcx=" + ulcx + ",ulcy=" + ulcy + ",idx=" + sbandIdx + ",orient=" + orientation + ",node=" + isNode + ",level=" + level + ",resLvl=" + resLvl + ",nomCBlkW=" + nomCBlkW + ",nomCBlkH=" + nomCBlkH + ",numCb=" + numCb;
	}
}
