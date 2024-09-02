namespace DocGen.Pdf.Parsing;

internal static class SystemFontHelper
{
	public static bool UnboxInteger(object number, out int res)
	{
		if (number is sbyte)
		{
			res = (sbyte)number;
		}
		else if (number is short)
		{
			res = (short)number;
		}
		else if (number is int)
		{
			res = (int)number;
		}
		else
		{
			if (!(number is double))
			{
				res = 0;
				return false;
			}
			res = (int)(double)number;
		}
		return true;
	}

	public static bool UnboxReal(object number, out double res)
	{
		if (number is sbyte)
		{
			res = (sbyte)number;
		}
		else if (number is short)
		{
			res = (short)number;
		}
		else if (number is int)
		{
			res = (int)number;
		}
		else
		{
			if (!(number is double))
			{
				res = 0.0;
				return false;
			}
			res = (double)number;
		}
		return true;
	}

	public static byte[] CreateByteArray(params byte[] bytes)
	{
		return bytes;
	}
}
