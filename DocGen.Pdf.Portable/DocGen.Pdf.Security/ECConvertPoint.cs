using System;

namespace DocGen.Pdf.Security;

internal sealed class ECConvertPoint
{
	private ECConvertPoint()
	{
	}

	public static int GetByteLength(EllipticCurveElements field)
	{
		return (field.ElementSize + 7) / 8;
	}

	public static byte[] ConvetByte(Number numberS, int qLength)
	{
		byte[] array = numberS.ToByteArrayUnsigned();
		if (qLength < array.Length)
		{
			byte[] array2 = new byte[qLength];
			Array.Copy(array, array.Length - array2.Length, array2, 0, array2.Length);
			return array2;
		}
		if (qLength > array.Length)
		{
			byte[] array3 = new byte[qLength];
			Array.Copy(array, 0, array3, array3.Length - array.Length, array.Length);
			return array3;
		}
		return array;
	}
}
