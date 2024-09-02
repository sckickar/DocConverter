using System;

namespace DocGen.Pdf;

internal class CBlkSizeSpec : ModuleSpec
{
	private const string optName = "Cblksiz";

	private int maxCBlkWidth;

	private int maxCBlkHeight;

	public virtual int MaxCBlkWidth => maxCBlkWidth;

	public virtual int MaxCBlkHeight => maxCBlkHeight;

	public CBlkSizeSpec(int nt, int nc, byte type)
		: base(nt, nc, type)
	{
	}

	internal CBlkSizeSpec(int nt, int nc, byte type, JPXParameters pl)
		: base(nt, nc, type)
	{
		bool flag = true;
		SupportClass.Tokenizer tokenizer = new SupportClass.Tokenizer(pl.getParameter("Cblksiz"));
		byte b = 0;
		bool[] array = null;
		bool[] array2 = null;
		string text = null;
		while (tokenizer.HasMoreTokens())
		{
			text = tokenizer.NextToken();
			switch (text[0])
			{
			case 't':
				array = ModuleSpec.parseIdx(text, nTiles);
				b = (byte)((b != 1) ? 2 : 3);
				continue;
			case 'c':
				array2 = ModuleSpec.parseIdx(text, nComp);
				b = (byte)((b != 2) ? 1 : 3);
				continue;
			}
			if (!char.IsDigit(text[0]))
			{
				throw new ArgumentException("Bad construction for parameter: " + text);
			}
			int[] array3 = new int[2];
			try
			{
				array3[0] = int.Parse(text);
				if (array3[0] > StdEntropyCoderOptions.MAX_CB_DIM)
				{
					throw new ArgumentException("'Cblksiz' option : the code-block's width cannot be greater than " + StdEntropyCoderOptions.MAX_CB_DIM);
				}
				if (array3[0] < StdEntropyCoderOptions.MIN_CB_DIM)
				{
					throw new ArgumentException("'Cblksiz' option : the code-block's width cannot be less than " + StdEntropyCoderOptions.MIN_CB_DIM);
				}
				if (array3[0] != 1 << MathUtil.log2(array3[0]))
				{
					throw new ArgumentException("'Cblksiz' option : the code-block's width must be a power of 2");
				}
			}
			catch (FormatException)
			{
				throw new ArgumentException("'Cblksiz' option : the code-block's width could not be parsed.");
			}
			try
			{
				text = tokenizer.NextToken();
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new ArgumentException("'Cblksiz' option : could not parse the code-block's height");
			}
			try
			{
				array3[1] = int.Parse(text);
				if (array3[1] > StdEntropyCoderOptions.MAX_CB_DIM)
				{
					throw new ArgumentException("'Cblksiz' option : the code-block's height cannot be greater than " + StdEntropyCoderOptions.MAX_CB_DIM);
				}
				if (array3[1] < StdEntropyCoderOptions.MIN_CB_DIM)
				{
					throw new ArgumentException("'Cblksiz' option : the code-block's height cannot be less than " + StdEntropyCoderOptions.MIN_CB_DIM);
				}
				if (array3[1] != 1 << MathUtil.log2(array3[1]))
				{
					throw new ArgumentException("'Cblksiz' option : the code-block's height must be a power of 2");
				}
				if (array3[0] * array3[1] > StdEntropyCoderOptions.MAX_CB_AREA)
				{
					throw new ArgumentException("'Cblksiz' option : The code-block's area (i.e. width*height) cannot be greater than " + StdEntropyCoderOptions.MAX_CB_AREA);
				}
			}
			catch (FormatException)
			{
				throw new ArgumentException("'Cblksiz' option : the code-block's height could not be parsed.");
			}
			if (array3[0] > maxCBlkWidth)
			{
				maxCBlkWidth = array3[0];
			}
			if (array3[1] > maxCBlkHeight)
			{
				maxCBlkHeight = array3[1];
			}
			if (flag)
			{
				setDefault(array3);
				flag = false;
			}
			switch (b)
			{
			case 0:
				setDefault(array3);
				continue;
			case 2:
			{
				for (int num = array.Length - 1; num >= 0; num--)
				{
					if (array[num])
					{
						setTileDef(num, array3);
					}
				}
				continue;
			}
			case 1:
			{
				for (int num2 = array2.Length - 1; num2 >= 0; num2--)
				{
					if (array2[num2])
					{
						setCompDef(num2, array3);
					}
				}
				continue;
			}
			}
			for (int num = array.Length - 1; num >= 0; num--)
			{
				for (int num2 = array2.Length - 1; num2 >= 0; num2--)
				{
					if (array[num] && array2[num2])
					{
						setTileCompVal(num, num2, array3);
					}
				}
			}
		}
	}

	public virtual int getCBlkWidth(byte type, int t, int c)
	{
		int[] array = null;
		switch (type)
		{
		case 0:
			array = (int[])getDefault();
			break;
		case 1:
			array = (int[])getCompDef(c);
			break;
		case 2:
			array = (int[])getTileDef(t);
			break;
		case 3:
			array = (int[])getTileCompVal(t, c);
			break;
		}
		return array[0];
	}

	public virtual int getCBlkHeight(byte type, int t, int c)
	{
		int[] array = null;
		switch (type)
		{
		case 0:
			array = (int[])getDefault();
			break;
		case 1:
			array = (int[])getCompDef(c);
			break;
		case 2:
			array = (int[])getTileDef(t);
			break;
		case 3:
			array = (int[])getTileCompVal(t, c);
			break;
		}
		return array[1];
	}

	public override void setDefault(object value_Renamed)
	{
		base.setDefault(value_Renamed);
		storeHighestDims((int[])value_Renamed);
	}

	public override void setTileDef(int t, object value_Renamed)
	{
		base.setTileDef(t, value_Renamed);
		storeHighestDims((int[])value_Renamed);
	}

	public override void setCompDef(int c, object value_Renamed)
	{
		base.setCompDef(c, value_Renamed);
		storeHighestDims((int[])value_Renamed);
	}

	public override void setTileCompVal(int t, int c, object value_Renamed)
	{
		base.setTileCompVal(t, c, value_Renamed);
		storeHighestDims((int[])value_Renamed);
	}

	private void storeHighestDims(int[] dim)
	{
		if (dim[0] > maxCBlkWidth)
		{
			maxCBlkWidth = dim[0];
		}
		if (dim[1] > maxCBlkHeight)
		{
			maxCBlkHeight = dim[1];
		}
	}
}
