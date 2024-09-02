using System;

namespace DocGen.Pdf;

internal class AnWTFilterSpec : ModuleSpec
{
	private const string REV_FILTER_STR = "w5x3";

	private const string NON_REV_FILTER_STR = "w9x7";

	internal AnWTFilterSpec(int nt, int nc, byte type, QuantTypeSpec qts, JPXParameters pl)
		: base(nt, nc, type)
	{
		pl.checkList('F', JPXParameters.toNameArray(AnWTFilter.ParameterInfo));
		string parameter = pl.getParameter("Ffilters");
		bool flag = true;
		if (parameter == null)
		{
			flag = false;
			if (pl.getBooleanParameter("lossless"))
			{
				setDefault(parseFilters("w5x3"));
				return;
			}
			for (int num = nt - 1; num >= 0; num--)
			{
				for (int num2 = nc - 1; num2 >= 0; num2--)
				{
					switch (qts.getSpecValType(num, num2))
					{
					case 0:
						if (getDefault() == null)
						{
							if (pl.getBooleanParameter("lossless"))
							{
								setDefault(parseFilters("w5x3"));
							}
							if (((string)qts.getDefault()).Equals("reversible"))
							{
								setDefault(parseFilters("w5x3"));
							}
							else
							{
								setDefault(parseFilters("w9x7"));
							}
						}
						specValType[num][num2] = 0;
						break;
					case 1:
						if (!isCompSpecified(num2))
						{
							if (((string)qts.getCompDef(num2)).Equals("reversible"))
							{
								setCompDef(num2, parseFilters("w5x3"));
							}
							else
							{
								setCompDef(num2, parseFilters("w9x7"));
							}
						}
						specValType[num][num2] = 1;
						break;
					case 2:
						if (!isTileSpecified(num))
						{
							if (((string)qts.getTileDef(num)).Equals("reversible"))
							{
								setTileDef(num, parseFilters("w5x3"));
							}
							else
							{
								setTileDef(num, parseFilters("w9x7"));
							}
						}
						specValType[num][num2] = 2;
						break;
					case 3:
						if (!isTileCompSpecified(num, num2))
						{
							if (((string)qts.getTileCompVal(num, num2)).Equals("reversible"))
							{
								setTileCompVal(num, num2, parseFilters("w5x3"));
							}
							else
							{
								setTileCompVal(num, num2, parseFilters("w9x7"));
							}
						}
						specValType[num][num2] = 3;
						break;
					default:
						throw new ArgumentException("Unsupported specification type");
					}
				}
			}
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
			case 'T':
			case 't':
				array = ModuleSpec.parseIdx(text, nTiles);
				b = (byte)((b != 1) ? 2 : 3);
				break;
			case 'C':
			case 'c':
				array2 = ModuleSpec.parseIdx(text, nComp);
				b = (byte)((b != 2) ? 1 : 3);
				break;
			case 'W':
			case 'w':
			{
				if (pl.getBooleanParameter("lossless") && text.ToUpper().Equals("w9x7".ToUpper()))
				{
					throw new ArgumentException("Cannot use non reversible wavelet transform with '-lossless' option");
				}
				AnWTFilter[][] array3 = parseFilters(text);
				switch (b)
				{
				case 0:
					setDefault(array3);
					break;
				case 2:
				{
					for (int num5 = array.Length - 1; num5 >= 0; num5--)
					{
						if (array[num5])
						{
							setTileDef(num5, array3);
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
							setCompDef(num6, array3);
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
								setTileCompVal(num3, num4, array3);
							}
						}
					}
					break;
				}
				}
				b = 0;
				array = null;
				array2 = null;
				break;
			}
			default:
				throw new ArgumentException("Bad construction for parameter: " + text);
			}
		}
		if (getDefault() == null)
		{
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
				if (((string)qts.getDefault()).Equals("reversible"))
				{
					setDefault(parseFilters("w5x3"));
				}
				else
				{
					setDefault(parseFilters("w9x7"));
				}
			}
			else
			{
				setDefault(getTileCompVal(0, 0));
				switch (specValType[0][0])
				{
				case 2:
				{
					for (int num11 = nc - 1; num11 >= 0; num11--)
					{
						if (specValType[0][num11] == 2)
						{
							specValType[0][num11] = 0;
						}
					}
					tileDef[0] = null;
					break;
				}
				case 1:
				{
					for (int num10 = nt - 1; num10 >= 0; num10--)
					{
						if (specValType[num10][0] == 1)
						{
							specValType[num10][0] = 0;
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
		for (int num12 = nt - 1; num12 >= 0; num12--)
		{
			for (int num13 = nc - 1; num13 >= 0; num13--)
			{
				if (((string)qts.getTileCompVal(num12, num13)).Equals("reversible"))
				{
					if (!isReversible(num12, num13))
					{
						if (flag)
						{
							throw new ArgumentException("Filter of tile-component (" + num12 + "," + num13 + ") does not allow reversible quantization. Specify '-Qtype expounded' or '-Qtype derived'in the command line.");
						}
						setTileCompVal(num12, num13, parseFilters("w5x3"));
					}
				}
				else if (isReversible(num12, num13))
				{
					if (flag)
					{
						throw new ArgumentException("Filter of tile-component (" + num12 + "," + num13 + ") does not allow non-reversible quantization. Specify '-Qtype reversible' in the command line");
					}
					setTileCompVal(num12, num13, parseFilters("w9x7"));
				}
			}
		}
	}

	private AnWTFilter[][] parseFilters(string word)
	{
		AnWTFilter[][] array = new AnWTFilter[2][];
		for (int i = 0; i < 2; i++)
		{
			array[i] = new AnWTFilter[1];
		}
		if (word.ToUpper().Equals("w5x3".ToUpper()))
		{
			array[0][0] = new AnWTFilterIntLift5x3();
			array[1][0] = new AnWTFilterIntLift5x3();
			return array;
		}
		if (word.ToUpper().Equals("w9x7".ToUpper()))
		{
			array[0][0] = new AnWTFilterFloatLift9x7();
			array[1][0] = new AnWTFilterFloatLift9x7();
			return array;
		}
		throw new ArgumentException("Non JPEG 2000 part I filter: " + word);
	}

	public virtual int getWTDataType(int t, int c)
	{
		return ((AnWTFilter[][])getSpec(t, c))[0][0].DataType;
	}

	public virtual AnWTFilter[] getHFilters(int t, int c)
	{
		return ((AnWTFilter[][])getSpec(t, c))[0];
	}

	public virtual AnWTFilter[] getVFilters(int t, int c)
	{
		return ((AnWTFilter[][])getSpec(t, c))[1];
	}

	public override string ToString()
	{
		string text = "";
		text = text + "nTiles=" + nTiles + "\nnComp=" + nComp + "\n\n";
		for (int i = 0; i < nTiles; i++)
		{
			for (int j = 0; j < nComp; j++)
			{
				AnWTFilter[][] array = (AnWTFilter[][])getSpec(i, j);
				text = text + "(t:" + i + ",c:" + j + ")\n";
				text += "\tH:";
				for (int k = 0; k < array[0].Length; k++)
				{
					text = text + " " + array[0][k];
				}
				text += "\n\tV:";
				for (int l = 0; l < array[1].Length; l++)
				{
					text = text + " " + array[1][l];
				}
				text += "\n";
			}
		}
		return text;
	}

	public virtual bool isReversible(int t, int c)
	{
		AnWTFilter[] hFilters = getHFilters(t, c);
		AnWTFilter[] vFilters = getVFilters(t, c);
		for (int num = hFilters.Length - 1; num >= 0; num--)
		{
			if (!hFilters[num].Reversible || !vFilters[num].Reversible)
			{
				return false;
			}
		}
		return true;
	}
}
