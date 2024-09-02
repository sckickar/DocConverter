using System;

namespace DocGen.Pdf.Security;

internal class DataEncryptionParameter : KeyParameter
{
	internal const int DataEncryptionKeyLength = 8;

	private const int DataEncryptionWeekKeysCount = 16;

	private static readonly byte[] DataEncryptionWeekKeys = new byte[128]
	{
		1, 1, 1, 1, 1, 1, 1, 1, 31, 31,
		31, 31, 14, 14, 14, 14, 224, 224, 224, 224,
		241, 241, 241, 241, 254, 254, 254, 254, 254, 254,
		254, 254, 1, 254, 1, 254, 1, 254, 1, 254,
		31, 224, 31, 224, 14, 241, 14, 241, 1, 224,
		1, 224, 1, 241, 1, 241, 31, 254, 31, 254,
		14, 254, 14, 254, 1, 31, 1, 31, 1, 14,
		1, 14, 224, 254, 224, 254, 241, 254, 241, 254,
		254, 1, 254, 1, 254, 1, 254, 1, 224, 31,
		224, 31, 241, 14, 241, 14, 224, 1, 224, 1,
		241, 1, 241, 1, 254, 31, 254, 31, 254, 14,
		254, 14, 31, 1, 31, 1, 14, 1, 14, 1,
		254, 224, 254, 224, 254, 241, 254, 241
	};

	internal DataEncryptionParameter(byte[] keys)
		: base(keys)
	{
		if (CheckKey(keys, 0))
		{
			throw new ArgumentException("Invalid Data Encryption keys creation");
		}
	}

	internal DataEncryptionParameter(byte[] keys, int offset, int length)
		: base(keys, offset, length)
	{
		if (CheckKey(keys, offset))
		{
			throw new ArgumentException("Invalid Data Encryption keys creation");
		}
	}

	internal static bool CheckKey(byte[] bytes, int offset)
	{
		if (bytes.Length - offset < 8)
		{
			throw new ArgumentException("Invalid length in bytes");
		}
		for (int i = 0; i < 16; i++)
		{
			bool flag = false;
			for (int j = 0; j < 8; j++)
			{
				if (bytes[j + offset] != DataEncryptionWeekKeys[i * 8 + j])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}
}
