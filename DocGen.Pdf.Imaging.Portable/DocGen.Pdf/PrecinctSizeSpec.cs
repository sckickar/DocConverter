using System;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class PrecinctSizeSpec : ModuleSpec
{
	private const string optName = "Cpp";

	private IntegerSpec dls;

	public PrecinctSizeSpec(int nt, int nc, byte type, IntegerSpec dls)
		: base(nt, nc, type)
	{
		this.dls = dls;
	}

	internal PrecinctSizeSpec(int nt, int nc, byte type, BlockImageDataSource imgsrc, IntegerSpec dls, JPXParameters pl)
		: base(nt, nc, type)
	{
		this.dls = dls;
		bool flag = false;
		string parameter = pl.getParameter("Cpp");
		List<object>[] array = new List<object>[2]
		{
			new List<object>(10),
			null
		};
		array[0].Add(65535);
		array[1] = new List<object>(10);
		array[1].Add(65535);
		setDefault(array);
		if (parameter == null)
		{
			return;
		}
		SupportClass.Tokenizer tokenizer = new SupportClass.Tokenizer(parameter);
		byte b = 0;
		bool[] array2 = null;
		bool[] array3 = null;
		bool flag2 = false;
		string text = null;
		while ((tokenizer.HasMoreTokens() || flag) && !flag2)
		{
			List<object>[] array4 = new List<object>[2];
			if (!flag)
			{
				text = tokenizer.NextToken();
			}
			flag = false;
			switch (text[0])
			{
			case 't':
				array2 = ModuleSpec.parseIdx(text, nTiles);
				b = (byte)((b != 1) ? 2 : 3);
				continue;
			case 'c':
				array3 = ModuleSpec.parseIdx(text, nComp);
				b = (byte)((b != 2) ? 1 : 3);
				continue;
			}
			if (!char.IsDigit(text[0]))
			{
				throw new ArgumentException("Bad construction for parameter: " + text);
			}
			array4[0] = new List<object>(10);
			array4[1] = new List<object>(10);
			while (true)
			{
				int num;
				int num2;
				try
				{
					num = int.Parse(text);
					try
					{
						text = tokenizer.NextToken();
					}
					catch (ArgumentOutOfRangeException)
					{
						throw new ArgumentException("'Cpp' option : could not parse the precinct's width");
					}
					num2 = int.Parse(text);
					if (num != 1 << MathUtil.log2(num) || num2 != 1 << MathUtil.log2(num2))
					{
						throw new ArgumentException("Precinct dimensions must be powers of 2");
					}
				}
				catch (FormatException)
				{
					throw new ArgumentException("'Cpp' option : the argument '" + text + "' could not be parsed.");
				}
				array4[0].Add(num);
				array4[1].Add(num2);
				if (tokenizer.HasMoreTokens())
				{
					text = tokenizer.NextToken();
					if (char.IsDigit(text[0]))
					{
						continue;
					}
					flag = true;
					switch (b)
					{
					case 0:
						setDefault(array4);
						break;
					case 2:
					{
						for (int num3 = array2.Length - 1; num3 >= 0; num3--)
						{
							if (array2[num3])
							{
								setTileDef(num3, array4);
							}
						}
						break;
					}
					case 1:
					{
						for (int num4 = array3.Length - 1; num4 >= 0; num4--)
						{
							if (array3[num4])
							{
								setCompDef(num4, array4);
							}
						}
						break;
					}
					default:
					{
						for (int num3 = array2.Length - 1; num3 >= 0; num3--)
						{
							for (int num4 = array3.Length - 1; num4 >= 0; num4--)
							{
								if (array2[num3] && array3[num4])
								{
									setTileCompVal(num3, num4, array4);
								}
							}
						}
						break;
					}
					}
					b = 0;
					array2 = null;
					array3 = null;
					break;
				}
				switch (b)
				{
				case 0:
					setDefault(array4);
					break;
				case 2:
				{
					for (int num3 = array2.Length - 1; num3 >= 0; num3--)
					{
						if (array2[num3])
						{
							setTileDef(num3, array4);
						}
					}
					break;
				}
				case 1:
				{
					for (int num4 = array3.Length - 1; num4 >= 0; num4--)
					{
						if (array3[num4])
						{
							setCompDef(num4, array4);
						}
					}
					break;
				}
				default:
				{
					for (int num3 = array2.Length - 1; num3 >= 0; num3--)
					{
						for (int num4 = array3.Length - 1; num4 >= 0; num4--)
						{
							if (array2[num3] && array3[num4])
							{
								setTileCompVal(num3, num4, array4);
							}
						}
					}
					break;
				}
				}
				flag2 = true;
				break;
			}
		}
	}

	public virtual int getPPX(int t, int c, int rl)
	{
		List<object>[] array = null;
		bool flag = t != -1;
		bool flag2 = c != -1;
		int num;
		if (flag && flag2)
		{
			num = (int)dls.getTileCompVal(t, c);
			array = (List<object>[])getTileCompVal(t, c);
		}
		else if (flag && !flag2)
		{
			num = (int)dls.getTileDef(t);
			array = (List<object>[])getTileDef(t);
		}
		else if (!flag && flag2)
		{
			num = (int)dls.getCompDef(c);
			array = (List<object>[])getCompDef(c);
		}
		else
		{
			num = (int)dls.getDefault();
			array = (List<object>[])getDefault();
		}
		int num2 = num - rl;
		if (array[0].Count > num2)
		{
			return (int)array[0][num2];
		}
		return (int)array[0][array[0].Count - 1];
	}

	public virtual int getPPY(int t, int c, int rl)
	{
		List<object>[] array = null;
		bool flag = t != -1;
		bool flag2 = c != -1;
		int num;
		if (flag && flag2)
		{
			num = (int)dls.getTileCompVal(t, c);
			array = (List<object>[])getTileCompVal(t, c);
		}
		else if (flag && !flag2)
		{
			num = (int)dls.getTileDef(t);
			array = (List<object>[])getTileDef(t);
		}
		else if (!flag && flag2)
		{
			num = (int)dls.getCompDef(c);
			array = (List<object>[])getCompDef(c);
		}
		else
		{
			num = (int)dls.getDefault();
			array = (List<object>[])getDefault();
		}
		int num2 = num - rl;
		if (array[1].Count > num2)
		{
			return (int)array[1][num2];
		}
		return (int)array[1][array[1].Count - 1];
	}
}
