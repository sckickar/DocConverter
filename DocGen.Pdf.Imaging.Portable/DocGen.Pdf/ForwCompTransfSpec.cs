using System;

namespace DocGen.Pdf;

internal class ForwCompTransfSpec : CompTransfSpec
{
	internal ForwCompTransfSpec(int nt, int nc, byte type, AnWTFilterSpec wfs, JPXParameters pl)
		: base(nt, nc, type)
	{
		string parameter = pl.getParameter("Mct");
		if (parameter == null)
		{
			if (nc < 3)
			{
				setDefault("none");
				return;
			}
			if (pl.getBooleanParameter("lossless"))
			{
				setDefault("rct");
				return;
			}
			int[] array = new int[nComp];
			for (int i = 0; i < 3; i++)
			{
				AnWTFilter[][] array2 = (AnWTFilter[][])wfs.getCompDef(i);
				array[i] = array2[0][0].FilterType;
			}
			bool flag = false;
			for (int j = 1; j < 3; j++)
			{
				if (array[j] != array[0])
				{
					flag = true;
				}
			}
			if (flag)
			{
				setDefault("none");
			}
			else
			{
				AnWTFilter[][] array2 = (AnWTFilter[][])wfs.getCompDef(0);
				if (array2[0][0].FilterType == 0)
				{
					setDefault("ict");
				}
				else
				{
					setDefault("rct");
				}
			}
			for (int k = 0; k < nt; k++)
			{
				int[] array3 = new int[nComp];
				AnWTFilter[][] array4;
				for (int l = 0; l < 3; l++)
				{
					array4 = (AnWTFilter[][])wfs.getTileCompVal(k, l);
					array3[l] = array4[0][0].FilterType;
				}
				bool flag2 = false;
				for (int m = 1; m < nComp; m++)
				{
					if (array3[m] != array3[0])
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					setTileDef(k, "none");
					continue;
				}
				array4 = (AnWTFilter[][])wfs.getTileCompVal(k, 0);
				if (array4[0][0].FilterType == 0)
				{
					setTileDef(k, "ict");
				}
				else
				{
					setTileDef(k, "rct");
				}
			}
			return;
		}
		SupportClass.Tokenizer tokenizer = new SupportClass.Tokenizer(parameter);
		byte b = 0;
		bool[] array5 = null;
		while (tokenizer.HasMoreTokens())
		{
			string text = tokenizer.NextToken();
			switch (text[0])
			{
			case 't':
				array5 = ModuleSpec.parseIdx(text, nTiles);
				b = (byte)((b != 1) ? 2 : 3);
				continue;
			case 'c':
				throw new ArgumentException("Component specific  parameters not allowed with '-Mct' option");
			}
			if (text.Equals("off"))
			{
				switch (b)
				{
				case 0:
					setDefault("none");
					break;
				case 2:
				{
					for (int num = array5.Length - 1; num >= 0; num--)
					{
						if (array5[num])
						{
							setTileDef(num, "none");
						}
					}
					break;
				}
				}
			}
			else
			{
				if (!text.Equals("on"))
				{
					throw new ArgumentException("Default parameter of option Mct not recognized: " + parameter);
				}
				if (nc < 3)
				{
					throw new ArgumentException("Cannot use component transformation on a image with less than three components");
				}
				switch (b)
				{
				case 0:
					setDefault("rct");
					break;
				case 2:
				{
					for (int num2 = array5.Length - 1; num2 >= 0; num2--)
					{
						if (array5[num2])
						{
							if (getFilterType(num2, wfs) == 1)
							{
								setTileDef(num2, "rct");
							}
							else
							{
								setTileDef(num2, "ict");
							}
						}
					}
					break;
				}
				}
			}
			b = 0;
			array5 = null;
		}
		if (getDefault() == null)
		{
			setDefault("none");
			for (int n = 0; n < nt; n++)
			{
				if (isTileSpecified(n))
				{
					continue;
				}
				int[] array6 = new int[nComp];
				AnWTFilter[][] array7;
				for (int num3 = 0; num3 < 3; num3++)
				{
					array7 = (AnWTFilter[][])wfs.getTileCompVal(n, num3);
					array6[num3] = array7[0][0].FilterType;
				}
				bool flag3 = false;
				for (int num4 = 1; num4 < nComp; num4++)
				{
					if (array6[num4] != array6[0])
					{
						flag3 = true;
					}
				}
				if (flag3)
				{
					setTileDef(n, "none");
					continue;
				}
				array7 = (AnWTFilter[][])wfs.getTileCompVal(n, 0);
				if (array7[0][0].FilterType == 0)
				{
					setTileDef(n, "ict");
				}
				else
				{
					setTileDef(n, "rct");
				}
			}
		}
		for (int num5 = nt - 1; num5 >= 0; num5--)
		{
			if (!((string)getTileDef(num5)).Equals("none"))
			{
				if (((string)getTileDef(num5)).Equals("rct"))
				{
					switch (getFilterType(num5, wfs))
					{
					case 0:
						if (isTileSpecified(num5))
						{
							throw new ArgumentException("Cannot use RCT with 9x7 filter in tile " + num5);
						}
						setTileDef(num5, "ict");
						break;
					default:
						throw new ArgumentException("Default filter is not JPEG 2000 part I compliant");
					case 1:
						break;
					}
				}
				else
				{
					switch (getFilterType(num5, wfs))
					{
					case 1:
						if (isTileSpecified(num5))
						{
							throw new ArgumentException("Cannot use ICT with filter 5x3 in tile " + num5);
						}
						setTileDef(num5, "rct");
						break;
					default:
						throw new ArgumentException("Default filter is not JPEG 2000 part I compliant");
					case 0:
						break;
					}
				}
			}
		}
	}

	private int getFilterType(int t, AnWTFilterSpec wfs)
	{
		int[] array = new int[nComp];
		for (int i = 0; i < nComp; i++)
		{
			AnWTFilter[][] array2 = ((t != -1) ? ((AnWTFilter[][])wfs.getTileCompVal(t, i)) : ((AnWTFilter[][])wfs.getCompDef(i)));
			array[i] = array2[0][0].FilterType;
		}
		bool flag = false;
		for (int j = 1; j < nComp; j++)
		{
			if (array[j] != array[0])
			{
				flag = true;
			}
		}
		if (flag)
		{
			throw new ArgumentException("Can not use component transformation when components do not use the same filters");
		}
		return array[0];
	}
}
