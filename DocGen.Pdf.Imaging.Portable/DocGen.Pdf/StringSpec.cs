using System;

namespace DocGen.Pdf;

internal class StringSpec : ModuleSpec
{
	internal StringSpec(int nt, int nc, byte type)
		: base(nt, nc, type)
	{
	}

	internal StringSpec(int nt, int nc, byte type, string optName, string[] list, JPXParameters pl)
		: base(nt, nc, type)
	{
		string parameter = pl.getParameter(optName);
		bool flag = false;
		if (parameter == null)
		{
			parameter = pl.DefaultParameterList.getParameter(optName);
			for (int num = list.Length - 1; num >= 0; num--)
			{
				if (parameter.ToUpper().Equals(list[num].ToUpper()))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				throw new ArgumentException("The Option name is not supported");
			}
			setDefault(parameter);
			return;
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
			flag = false;
			for (int num2 = list.Length - 1; num2 >= 0; num2--)
			{
				if (text.ToUpper().Equals(list[num2].ToUpper()))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				throw new ArgumentException("The Option name is not supported");
			}
			switch (b)
			{
			case 0:
				setDefault(text);
				break;
			case 2:
			{
				for (int num5 = array.Length - 1; num5 >= 0; num5--)
				{
					if (array[num5])
					{
						setTileDef(num5, text);
					}
				}
				break;
			}
			case 1:
			{
				for (int num6 = array2.Length - 1; num6 >= 0; num6--)
				{
					if (array2[num6])
					{
						setCompDef(num6, text);
					}
				}
				break;
			}
			default:
			{
				for (int num3 = array.Length - 1; num3 >= 0; num3--)
				{
					for (int num4 = array2.Length - 1; num4 >= 0; num4--)
					{
						if (array[num3] && array2[num4])
						{
							setTileCompVal(num3, num4, text);
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
		int num7 = 0;
		for (int num8 = nt - 1; num8 >= 0; num8--)
		{
			for (int num9 = nc - 1; num9 >= 0; num9--)
			{
				if (specValType[num8][num9] == 0)
				{
					num7++;
				}
			}
		}
		if (num7 != 0)
		{
			parameter = pl.DefaultParameterList.getParameter(optName);
			for (int num10 = list.Length - 1; num10 >= 0; num10--)
			{
				if (parameter.ToUpper().Equals(list[num10].ToUpper()))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				throw new ArgumentException("Default parameter of option -" + optName + " not recognized: " + parameter);
			}
			setDefault(parameter);
			return;
		}
		setDefault(getSpec(0, 0));
		switch (specValType[0][0])
		{
		case 2:
		{
			for (int num12 = nc - 1; num12 >= 0; num12--)
			{
				if (specValType[0][num12] == 2)
				{
					specValType[0][num12] = 0;
				}
			}
			tileDef[0] = null;
			break;
		}
		case 1:
		{
			for (int num11 = nt - 1; num11 >= 0; num11--)
			{
				if (specValType[num11][0] == 1)
				{
					specValType[num11][0] = 0;
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
