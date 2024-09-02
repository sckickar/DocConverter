using System;

namespace DocGen.Pdf;

internal class GuardBitsSpec : ModuleSpec
{
	public GuardBitsSpec(int nt, int nc, byte type)
		: base(nt, nc, type)
	{
	}

	internal GuardBitsSpec(int nt, int nc, byte type, JPXParameters pl)
		: base(nt, nc, type)
	{
		SupportClass.Tokenizer tokenizer = new SupportClass.Tokenizer(pl.getParameter("Qguard_bits") ?? throw new ArgumentException("Qguard_bits option not specified"));
		byte b = 0;
		bool[] array = null;
		bool[] array2 = null;
		while (tokenizer.HasMoreTokens())
		{
			string text = tokenizer.NextToken().ToLower();
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
				throw new ArgumentException("Bad parameter for -Qguard_bits option : " + text);
			}
			if ((float)num <= 0f)
			{
				throw new ArgumentException("Guard bits value must be positive : " + num);
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
			setDefault(int.Parse(pl.DefaultParameterList.getParameter("Qguard_bits")));
			return;
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
}
