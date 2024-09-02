using System;

namespace DocGen.Pdf.Security;

internal class DESedeAlgorithmParameter : DataEncryptionParameter
{
	internal const int DesEdeKeyLength = 24;

	internal DESedeAlgorithmParameter(byte[] key)
		: base(FixKey(key, 0, key.Length))
	{
	}

	internal DESedeAlgorithmParameter(byte[] key, int keyOff, int keyLen)
		: base(FixKey(key, keyOff, keyLen))
	{
	}

	private static byte[] FixKey(byte[] key, int keyOff, int keyLen)
	{
		byte[] array = new byte[24];
		switch (keyLen)
		{
		case 16:
			Array.Copy(key, keyOff, array, 0, 16);
			Array.Copy(key, keyOff, array, 16, 8);
			break;
		case 24:
			Array.Copy(key, keyOff, array, 0, 24);
			break;
		default:
			throw new ArgumentException("Bad length for DESede key");
		}
		if (CheckKey(array, 0, array.Length))
		{
			throw new ArgumentException("Attempt to create weak DESede key");
		}
		return array;
	}

	internal static bool CheckKey(byte[] key, int offset, int length)
	{
		for (int i = offset; i < length; i += 8)
		{
			if (DataEncryptionParameter.CheckKey(key, i))
			{
				return true;
			}
		}
		return false;
	}
}
