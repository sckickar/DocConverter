namespace DocGen.PdfViewer.Base;

internal static class Helper
{
	public static bool ParseInteger(object number, out int res)
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

	public static bool ParseReal(object number, out double res)
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
