using System;
using System.Collections;

namespace DocGen.Pdf;

internal class ProgressionSpec : ModuleSpec
{
	public ProgressionSpec(int nt, int nc, byte type)
		: base(nt, nc, type)
	{
		if (type != 1)
		{
			throw new ApplicationException("Illegal use of class ProgressionSpec !");
		}
	}

	internal ProgressionSpec(int nt, int nc, int nl, IntegerSpec dls, byte type, JPXParameters pl)
		: base(nt, nc, type)
	{
		string parameter = pl.getParameter("Aptype");
		int num = -1;
		if (parameter == null)
		{
			num = ((pl.getParameter("Rroi") != null) ? checkProgMode("layer") : checkProgMode("res"));
			if (num == -1)
			{
				throw new ArgumentException("Unknown progression type : '" + parameter + "'");
			}
			setDefault(new Progression[1]
			{
				new Progression(num, 0, nc, 0, dls.Max + 1, nl)
			});
			return;
		}
		SupportClass.Tokenizer tokenizer = new SupportClass.Tokenizer(parameter);
		byte b = 0;
		bool[] array = null;
		string text = null;
		bool flag = false;
		int num2 = 0;
		ArrayList arrayList = ArrayList.Synchronized(new ArrayList(10));
		int num3 = 0;
		Progression progression = null;
		Progression[] array2;
		while (tokenizer.HasMoreTokens())
		{
			text = tokenizer.NextToken();
			if (text[0] == 't')
			{
				if (arrayList.Count > 0)
				{
					progression.ce = nc;
					progression.lye = nl;
					progression.re = dls.Max + 1;
					array2 = new Progression[arrayList.Count];
					arrayList.CopyTo(array2);
					switch (b)
					{
					case 0:
						setDefault(array2);
						break;
					case 2:
					{
						for (int num4 = array.Length - 1; num4 >= 0; num4--)
						{
							if (array[num4])
							{
								setTileDef(num4, array2);
							}
						}
						break;
					}
					}
				}
				arrayList.Clear();
				num2 = -1;
				flag = false;
				array = ModuleSpec.parseIdx(text, nTiles);
				b = 2;
			}
			else if (flag)
			{
				try
				{
					num3 = int.Parse(text);
				}
				catch (FormatException)
				{
					throw new ArgumentException("Progression order specification has missing parameters: " + parameter);
				}
				switch (num2)
				{
				case 0:
					if (num3 < 0 || num3 > dls.Max + 1)
					{
						throw new ArgumentException("Invalid res_start in '-Aptype' option: " + num3);
					}
					progression.rs = num3;
					break;
				case 1:
					if (num3 < 0 || num3 > nc)
					{
						throw new ArgumentException("Invalid comp_start in '-Aptype' option: " + num3);
					}
					progression.cs = num3;
					break;
				case 2:
					if (num3 < 0)
					{
						throw new ArgumentException("Invalid layer_end in '-Aptype' option: " + num3);
					}
					if (num3 > nl)
					{
						num3 = nl;
					}
					progression.lye = num3;
					break;
				case 3:
					if (num3 < 0)
					{
						throw new ArgumentException("Invalid res_end in '-Aptype' option: " + num3);
					}
					if (num3 > dls.Max + 1)
					{
						num3 = dls.Max + 1;
					}
					progression.re = num3;
					break;
				case 4:
					if (num3 < 0)
					{
						throw new ArgumentException("Invalid comp_end in '-Aptype' option: " + num3);
					}
					if (num3 > nc)
					{
						num3 = nc;
					}
					progression.ce = num3;
					break;
				}
				if (num2 < 4)
				{
					num2++;
					flag = true;
					continue;
				}
				if (num2 != 4)
				{
					throw new ApplicationException("Error in usage of 'Aptype' option: " + parameter);
				}
				num2 = 0;
				flag = false;
			}
			else if (!flag)
			{
				num = checkProgMode(text);
				if (num == -1)
				{
					throw new ArgumentException("Unknown progression type : '" + text + "'");
				}
				flag = true;
				num2 = 0;
				progression = ((arrayList.Count != 0) ? new Progression(num, 0, nc, 0, dls.Max + 1, nl) : new Progression(num, 0, nc, 0, dls.Max + 1, nl));
				arrayList.Add(progression);
			}
		}
		if (arrayList.Count == 0)
		{
			num = ((pl.getParameter("Rroi") != null) ? checkProgMode("layer") : checkProgMode("res"));
			if (num == -1)
			{
				throw new ArgumentException("Unknown progression type : '" + parameter + "'");
			}
			setDefault(new Progression[1]
			{
				new Progression(num, 0, nc, 0, dls.Max + 1, nl)
			});
			return;
		}
		progression.ce = nc;
		progression.lye = nl;
		progression.re = dls.Max + 1;
		array2 = new Progression[arrayList.Count];
		arrayList.CopyTo(array2);
		switch (b)
		{
		case 0:
			setDefault(array2);
			break;
		case 2:
		{
			for (int num5 = array.Length - 1; num5 >= 0; num5--)
			{
				if (array[num5])
				{
					setTileDef(num5, array2);
				}
			}
			break;
		}
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
			num = ((pl.getParameter("Rroi") != null) ? checkProgMode("layer") : checkProgMode("res"));
			if (num == -1)
			{
				throw new ArgumentException("Unknown progression type : '" + parameter + "'");
			}
			setDefault(new Progression[1]
			{
				new Progression(num, 0, nc, 0, dls.Max + 1, nl)
			});
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

	private int checkProgMode(string mode)
	{
		if (mode.Equals("res"))
		{
			return 1;
		}
		if (mode.Equals("layer"))
		{
			return 0;
		}
		if (mode.Equals("pos-comp"))
		{
			return 3;
		}
		if (mode.Equals("comp-pos"))
		{
			return 4;
		}
		if (mode.Equals("res-pos"))
		{
			return 2;
		}
		return -1;
	}
}
