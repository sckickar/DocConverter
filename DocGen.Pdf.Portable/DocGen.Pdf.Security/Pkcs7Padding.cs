using System;

namespace DocGen.Pdf.Security;

internal class Pkcs7Padding : IPadding
{
	public string PaddingName => "PKCS7";

	public void Initialize(SecureRandomAlgorithm random)
	{
	}

	public int AddPadding(byte[] bytes, int offset)
	{
		byte b = (byte)(bytes.Length - offset);
		while (offset < bytes.Length)
		{
			bytes[offset] = b;
			offset++;
		}
		return b;
	}

	public int Count(byte[] input)
	{
		int num = input[^1];
		if (num < 1 || num > input.Length)
		{
			throw new Exception("Invalid pad");
		}
		for (int i = 1; i <= num; i++)
		{
			if (input[^i] != num)
			{
				throw new Exception("Invalid pad");
			}
		}
		return num;
	}
}
