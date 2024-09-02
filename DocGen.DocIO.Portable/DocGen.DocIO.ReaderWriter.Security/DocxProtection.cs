using System;
using System.Security.Cryptography;
using System.Text;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class DocxProtection
{
	internal const int SpinCount = 100000;

	internal const string CryptographicType = "rsaAES";

	internal const string CryptographicAlgorithmClass = "hash";

	internal const string CryptographicAlgorithmType = "typeAny";

	internal const int CryptographicAlgorithmId = 14;

	internal DocxProtection()
	{
	}

	internal byte[] ComputeHash(byte[] salt, uint encryptedPassword)
	{
		byte[] array = new byte[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = Convert.ToByte((uint)(encryptedPassword & (255 << i * 8)) >> i * 8);
		}
		string text = string.Empty;
		for (int j = 0; j < 4; j++)
		{
			text += array[j].ToString("X2");
		}
		text = text.Trim().Trim(new char[1] { '\ufeff' });
		text = text.ToUpper();
		byte[] bytes = Encoding.Unicode.GetBytes(text);
		byte[] buffer = CombineByteArrays(salt, bytes);
		SHA512 sHA = SHA512.Create();
		buffer = sHA.ComputeHash(buffer);
		byte[] array2 = new byte[68];
		for (int k = 0; k < 100000; k++)
		{
			buffer.CopyTo(array2, 0);
			int num = k;
			for (int l = 0; l < 4; l++)
			{
				array2[buffer.Length + l] = (byte)num;
				num >>= 8;
			}
			buffer = sHA.ComputeHash(array2);
		}
		return buffer;
	}

	private byte[] CombineByteArrays(byte[] array1, byte[] array2)
	{
		byte[] array3 = new byte[array1.Length + array2.Length];
		Buffer.BlockCopy(array1, 0, array3, 0, array1.Length);
		Buffer.BlockCopy(array2, 0, array3, array1.Length, array2.Length);
		return array3;
	}

	internal byte[] CreateSalt(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		byte[] array = new byte[length];
		Random random = new Random((int)DateTime.Now.Ticks);
		int maxValue = 256;
		for (int i = 0; i < length; i++)
		{
			array[i] = (byte)random.Next(maxValue);
		}
		return array;
	}
}
