using System;

namespace DocGen.Pdf;

internal class IntegerSpec : ModuleSpec
{
	internal static int MAX_INT = int.MaxValue;

	internal virtual int Max
	{
		get
		{
			int num = (int)def;
			for (int i = 0; i < nTiles; i++)
			{
				for (int j = 0; j < nComp; j++)
				{
					int num2 = (int)getSpec(i, j);
					if (num < num2)
					{
						num = num2;
					}
				}
			}
			return num;
		}
	}

	internal virtual int Min
	{
		get
		{
			int num = (int)def;
			for (int i = 0; i < nTiles; i++)
			{
				for (int j = 0; j < nComp; j++)
				{
					int num2 = (int)getSpec(i, j);
					if (num > num2)
					{
						num = num2;
					}
				}
			}
			return num;
		}
	}

	internal IntegerSpec(int nt, int nc, byte type)
		: base(nt, nc, type)
	{
	}

	internal IntegerSpec(int nt, int nc, byte type, JPXParameters pl, string optName)
		: base(nt, nc, type)
	{
		string parameter = pl.getParameter(optName);
		if (parameter == null)
		{
			parameter = pl.DefaultParameterList.getParameter(optName);
			try
			{
				setDefault(int.Parse(parameter));
				return;
			}
			catch (FormatException)
			{
				throw new ArgumentException("OptionName not supported");
			}
		}
		SupportClass.Tokenizer tokenizer = new SupportClass.Tokenizer(parameter);
		byte b = 0;
		bool[] array = null;
		bool[] array2 = null;
		while (tokenizer.HasMoreTokens())
		{
			string text = tokenizer.NextToken();
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
			int num;
			try
			{
				num = int.Parse(text);
			}
			catch (FormatException)
			{
				throw new ArgumentException("Non recognized value for option -" + optName + ": " + text);
			}
			switch (b)
			{
			case 0:
				setDefault(num);
				break;
			case 2:
			{
				for (int num4 = array.Length - 1; num4 >= 0; num4--)
				{
					if (array[num4])
					{
						setTileDef(num4, num);
					}
				}
				break;
			}
			case 1:
			{
				for (int num5 = array2.Length - 1; num5 >= 0; num5--)
				{
					if (array2[num5])
					{
						setCompDef(num5, num);
					}
				}
				break;
			}
			default:
			{
				for (int num2 = array.Length - 1; num2 >= 0; num2--)
				{
					for (int num3 = array2.Length - 1; num3 >= 0; num3--)
					{
						if (array[num2] && array2[num3])
						{
							setTileCompVal(num2, num3, num);
						}
					}
				}
				break;
			}
			}
			b = 0;
			array = null;
			array2 = null;
		}
		if (getDefault() != null)
		{
			return;
		}
		int num6 = 0;
		for (int num7 = nt - 1; num7 >= 0; num7--)
		{
			for (int num8 = nc - 1; num8 >= 0; num8--)
			{
				if (specValType[num7][num8] == 0)
				{
					num6++;
				}
			}
		}
		if (num6 != 0)
		{
			parameter = pl.DefaultParameterList.getParameter(optName);
			try
			{
				setDefault(int.Parse(parameter));
				return;
			}
			catch (FormatException)
			{
				throw new ArgumentException("Non recognized value for option -" + optName + ": " + parameter);
			}
		}
		setDefault(getTileCompVal(0, 0));
		switch (specValType[0][0])
		{
		case 2:
		{
			for (int num10 = nc - 1; num10 >= 0; num10--)
			{
				if (specValType[0][num10] == 2)
				{
					specValType[0][num10] = 0;
				}
			}
			tileDef[0] = null;
			break;
		}
		case 1:
		{
			for (int num9 = nt - 1; num9 >= 0; num9--)
			{
				if (specValType[num9][0] == 1)
				{
					specValType[num9][0] = 0;
				}
			}
			compDef[0] = null;
			break;
		}
		case 3:
			specValType[0][0] = 0;
			tileCompVal["t0c0"] = null;
			break;
		}
	}

	internal virtual int getMaxInComp(int c)
	{
		int num = 0;
		for (int i = 0; i < nTiles; i++)
		{
			int num2 = (int)getSpec(i, c);
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}

	internal virtual int getMinInComp(int c)
	{
		int num = MAX_INT;
		for (int i = 0; i < nTiles; i++)
		{
			int num2 = (int)getSpec(i, c);
			if (num > num2)
			{
				num = num2;
			}
		}
		return num;
	}

	internal virtual int getMaxInTile(int t)
	{
		int num = 0;
		for (int i = 0; i < nComp; i++)
		{
			int num2 = (int)getSpec(t, i);
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}

	internal virtual int getMinInTile(int t)
	{
		int num = MAX_INT;
		for (int i = 0; i < nComp; i++)
		{
			int num2 = (int)getSpec(t, i);
			if (num > num2)
			{
				num = num2;
			}
		}
		return num;
	}
}
