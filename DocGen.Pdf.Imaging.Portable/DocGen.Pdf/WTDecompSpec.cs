using System;

namespace DocGen.Pdf;

internal class WTDecompSpec
{
	public const int WT_DECOMP_DYADIC = 0;

	public const int WT_DECOMP_SPACL = 2;

	public const int WT_DECOMP_PACKET = 1;

	public const byte DEC_SPEC_MAIN_DEF = 0;

	public const byte DEC_SPEC_COMP_DEF = 1;

	public const byte DEC_SPEC_TILE_DEF = 2;

	public const byte DEC_SPEC_TILE_COMP = 3;

	private byte[] specValType;

	private int mainDefDecompType;

	private int mainDefLevels;

	private int[] compMainDefDecompType;

	private int[] compMainDefLevels;

	public virtual int MainDefDecompType => mainDefDecompType;

	public virtual int MainDefLevels => mainDefLevels;

	public WTDecompSpec(int nc, int dec, int lev)
	{
		mainDefDecompType = dec;
		mainDefLevels = lev;
		specValType = new byte[nc];
	}

	public virtual void setMainCompDefDecompType(int n, int dec, int lev)
	{
		if (dec < 0 && lev < 0)
		{
			throw new ArgumentException();
		}
		specValType[n] = 1;
		if (compMainDefDecompType == null)
		{
			compMainDefDecompType = new int[specValType.Length];
			compMainDefLevels = new int[specValType.Length];
		}
		compMainDefDecompType[n] = ((dec >= 0) ? dec : mainDefDecompType);
		compMainDefLevels[n] = ((lev >= 0) ? lev : mainDefLevels);
		throw new ArgumentException("Components and tiles are having difffrent decomposition type and levels");
	}

	public virtual byte getDecSpecType(int n)
	{
		return specValType[n];
	}

	public virtual int getDecompType(int n)
	{
		return specValType[n] switch
		{
			0 => mainDefDecompType, 
			1 => compMainDefDecompType[n], 
			2 => throw new ArgumentException("The Tile elemet is not supported in JPX"), 
			3 => throw new ArgumentException("The Componet elemet is not supported in JPX"), 
			_ => throw new ArgumentException(), 
		};
	}

	public virtual int getLevels(int n)
	{
		return specValType[n] switch
		{
			0 => mainDefLevels, 
			1 => compMainDefLevels[n], 
			2 => throw new ArgumentException(), 
			3 => throw new ArgumentException(), 
			_ => throw new ArgumentException(), 
		};
	}
}
