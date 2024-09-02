using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class JPXParameters : Dictionary<string, string>
{
	private JPXParameters defaults;

	public JPXParameters DefaultParameterList => defaults;

	public JPXParameters()
	{
	}

	public JPXParameters(JPXParameters def)
	{
		defaults = def;
	}

	public virtual string getParameter(string pname)
	{
		if (ContainsKey(pname))
		{
			return base[pname];
		}
		defaults.TryGetValue(pname, out var value);
		return value;
	}

	public virtual bool getBooleanParameter(string pname)
	{
		string parameter = getParameter(pname);
		if (parameter == null)
		{
			throw new ArgumentException("No parameter with name " + pname);
		}
		if (parameter.Equals("on"))
		{
			return true;
		}
		if (parameter.Equals("off"))
		{
			return false;
		}
		throw new Exception();
	}

	public virtual int getIntParameter(string pname)
	{
		string parameter = getParameter(pname);
		if (parameter == null)
		{
			throw new ArgumentException("No parameter with name " + pname);
		}
		try
		{
			return int.Parse(parameter);
		}
		catch (FormatException ex)
		{
			throw new FormatException("Parameter \"" + pname + "\" is not integer: " + ex.Message);
		}
	}

	public virtual float getFloatParameter(string pname)
	{
		string parameter = getParameter(pname);
		if (parameter == null)
		{
			throw new ArgumentException("No parameter with name " + pname);
		}
		try
		{
			return float.Parse(parameter);
		}
		catch (FormatException ex)
		{
			throw new FormatException("Parameter \"" + pname + "\" is not floating-point: " + ex.Message);
		}
	}

	public virtual void checkList(char prfx, string[] plist)
	{
		IEnumerator enumerator = base.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string text = (string)enumerator.Current;
			if (text.Length <= 0 || text[0] != prfx)
			{
				continue;
			}
			bool flag = false;
			if (plist != null)
			{
				for (int num = plist.Length - 1; num >= 0; num--)
				{
					if (text.Equals(plist[num]))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				throw new ArgumentException("Option '" + text + "' is not a valid one.");
			}
		}
	}

	public static string[] toNameArray(string[][] pinfo)
	{
		if (pinfo == null)
		{
			return null;
		}
		string[] array = new string[pinfo.Length];
		for (int num = pinfo.Length - 1; num >= 0; num--)
		{
			array[num] = pinfo[num][0];
		}
		return array;
	}
}
