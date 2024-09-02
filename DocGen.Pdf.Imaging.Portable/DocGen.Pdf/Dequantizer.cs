using System;

namespace DocGen.Pdf;

internal abstract class Dequantizer : MultiResImgDataAdapter, CBlkWTDataSrcDec, InvWTData, MultiResImgData
{
	public const char OPT_PREFIX = 'Q';

	private static readonly string[][] pinfo;

	internal CBlkQuantDataSrcDec src;

	internal int[] rb;

	internal int[] utrb;

	private CompTransfSpec cts;

	private SynWTFilterSpec wfs;

	public virtual int CbULX => src.CbULX;

	public virtual int CbULY => src.CbULY;

	public static string[][] ParameterInfo => pinfo;

	internal Dequantizer(CBlkQuantDataSrcDec src, int[] utrb, DecodeHelper decSpec)
		: base(src)
	{
		if (utrb.Length != src.NumComps)
		{
			throw new ArgumentException();
		}
		this.src = src;
		this.utrb = utrb;
		cts = decSpec.cts;
		wfs = decSpec.wfs;
	}

	public virtual int getNomRangeBits(int c)
	{
		return rb[c];
	}

	public override SubbandSyn getSynSubbandTree(int t, int c)
	{
		return src.getSynSubbandTree(t, c);
	}

	public override void setTile(int x, int y)
	{
		src.setTile(x, y);
		tIdx = TileIdx;
		int num = 0;
		if ((int)cts.getTileDef(tIdx) == 0)
		{
			num = 0;
		}
		else
		{
			int num2 = ((src.NumComps > 3) ? 3 : src.NumComps);
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				num3 += (wfs.isReversible(tIdx, i) ? 1 : 0);
			}
			num = num3 switch
			{
				3 => 1, 
				0 => 2, 
				_ => throw new ArgumentException("Wavelet transformation and component transformation not coherent in tile" + tIdx), 
			};
		}
		switch (num)
		{
		case 0:
			rb = utrb;
			break;
		case 1:
			rb = InverseComponetTransformation.calcMixedBitDepths(utrb, 1, null);
			break;
		case 2:
			rb = InverseComponetTransformation.calcMixedBitDepths(utrb, 2, null);
			break;
		default:
			throw new ArgumentException("Non JPEG 2000 part I component transformation for tile: " + tIdx);
		}
	}

	public override void nextTile()
	{
		src.nextTile();
		tIdx = TileIdx;
		switch ((int)cts.getTileDef(tIdx))
		{
		case 0:
			rb = utrb;
			break;
		case 1:
			rb = InverseComponetTransformation.calcMixedBitDepths(utrb, 1, null);
			break;
		case 2:
			rb = InverseComponetTransformation.calcMixedBitDepths(utrb, 2, null);
			break;
		default:
			throw new ArgumentException("Non JPEG 2000 part I component transformation for tile: " + tIdx);
		}
	}

	public abstract DataBlock getCodeBlock(int param1, int param2, int param3, SubbandSyn param4, DataBlock param5);

	public abstract int getFixedPoint(int param1);

	public abstract DataBlock getInternCodeBlock(int param1, int param2, int param3, SubbandSyn param4, DataBlock param5);
}
