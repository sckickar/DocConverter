using System;

namespace DocGen.Pdf;

internal class ArrayUtil
{
	public const int MAX_EL_COPYING = 8;

	public const int INIT_EL_COPYING = 4;

	public static void intArraySet(int[] arr, int val)
	{
		int num = arr.Length;
		int num2;
		if (num < 8)
		{
			for (num2 = num - 1; num2 >= 0; num2--)
			{
				arr[num2] = val;
			}
			return;
		}
		int num3 = num >> 1;
		for (num2 = 0; num2 < 4; num2++)
		{
			arr[num2] = val;
		}
		while (num2 <= num3)
		{
			Array.Copy(arr, 0, arr, num2, num2);
			num2 <<= 1;
		}
		if (num2 < num)
		{
			Array.Copy(arr, 0, arr, num2, num - num2);
		}
	}

	public static void byteArraySet(byte[] arr, byte val)
	{
		int num = arr.Length;
		int num2;
		if (num < 8)
		{
			for (num2 = num - 1; num2 >= 0; num2--)
			{
				arr[num2] = val;
			}
			return;
		}
		int num3 = num >> 1;
		for (num2 = 0; num2 < 4; num2++)
		{
			arr[num2] = val;
		}
		while (num2 <= num3)
		{
			Array.Copy(arr, 0, arr, num2, num2);
			num2 <<= 1;
		}
		if (num2 < num)
		{
			Array.Copy(arr, 0, arr, num2, num - num2);
		}
	}
}
