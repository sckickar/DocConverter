using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DocGen.CompoundFile.DocIO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal sealed class SecurityHelper
{
	internal enum EncrytionType
	{
		Standard,
		Agile,
		None
	}

	internal delegate void EncryptionMethod(byte[] buffer1, byte[] buffer2);

	private const int PasswordIterationCount = 50000;

	internal const string EncryptionInfoStream = "EncryptionInfo";

	internal const string DataSpacesStorage = "\u0006DataSpaces";

	internal const string DataSpaceMapStream = "DataSpaceMap";

	internal const string TransformPrimaryStream = "\u0006Primary";

	internal const string DataSpaceInfoStorage = "DataSpaceInfo";

	internal const string TransformInfoStorage = "TransformInfo";

	internal const string EncryptedPackageStream = "EncryptedPackage";

	internal const string StrongEncryptionDataSpaceStream = "StrongEncryptionDataSpace";

	internal const string StrongEncryptionTransformStream = "StrongEncryptionTransform";

	internal const string VersionStream = "Version";

	internal EncrytionType GetEncryptionType(ICompoundStorage storage)
	{
		EncrytionType result = EncrytionType.None;
		if (storage.ContainsStream("EncryptionInfo") && storage.ContainsStorage("\u0006DataSpaces"))
		{
			using Stream stream = storage.OpenStream("EncryptionInfo");
			byte[] buffer = new byte[4];
			int num = ReadInt32(stream, buffer);
			stream.Position = 0L;
			switch (num)
			{
			case 131075:
			case 131076:
				result = EncrytionType.Standard;
				break;
			case 262148:
				result = EncrytionType.Agile;
				break;
			}
		}
		return result;
	}

	internal int ReadInt32(Stream stream, byte[] buffer)
	{
		if (stream.Read(buffer, 0, 4) != 4)
		{
			throw new Exception("Invalid data");
		}
		return BitConverter.ToInt32(buffer, 0);
	}

	internal string ReadUnicodeString(Stream stream)
	{
		byte[] buffer = new byte[4];
		int num = ReadInt32(stream, buffer);
		buffer = new byte[num];
		if (stream.Read(buffer, 0, num) != num)
		{
			throw new Exception("Invalid data");
		}
		string @string = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
		int num2 = num % 4;
		if (num2 != 0)
		{
			stream.Position += 4 - num2;
		}
		return @string;
	}

	internal string ReadUnicodeStringZero(Stream stream)
	{
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = new byte[2];
		while (stream.Read(array, 0, 2) > 0)
		{
			string @string = Encoding.Unicode.GetString(array, 0, array.Length);
			if (@string[0] == '\0')
			{
				break;
			}
			stringBuilder.Append(@string);
		}
		return stringBuilder.ToString();
	}

	internal void WriteInt32(Stream stream, int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, 4);
	}

	internal void WriteUnicodeString(Stream stream, string value)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(value);
		int num = bytes.Length;
		WriteInt32(stream, num);
		stream.Write(bytes, 0, num);
		int num2 = num % 4;
		if (num2 != 0)
		{
			int i = 0;
			for (int num3 = 4 - num2; i < num3; i++)
			{
				stream.WriteByte(0);
			}
		}
	}

	internal void WriteUnicodeStringZero(Stream stream, string value)
	{
		int length = value.Length;
		byte[] bytes = Encoding.Unicode.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
		if (length == 0 || value[length - 1] != 0)
		{
			stream.WriteByte(0);
			stream.WriteByte(0);
		}
	}

	internal byte[] CreateKey(string password, byte[] salt, int keyLength)
	{
		SHA1 sHA = new SHA1Managed();
		byte[] bytes = Encoding.Unicode.GetBytes(password);
		byte[] array = new byte[salt.Length + bytes.Length];
		Buffer.BlockCopy(salt, 0, array, 0, salt.Length);
		Buffer.BlockCopy(bytes, 0, array, salt.Length, bytes.Length);
		byte[] array2 = sHA.ComputeHash(array);
		byte[] array3 = new byte[array2.Length + 4];
		byte[] array4 = array2;
		byte[] bytes2;
		for (int i = 0; i < 50000; i++)
		{
			bytes2 = BitConverter.GetBytes(i);
			Buffer.BlockCopy(bytes2, 0, array3, 0, bytes2.Length);
			Buffer.BlockCopy(array4, 0, array3, bytes2.Length, array4.Length);
			array4 = sHA.ComputeHash(array3);
		}
		bytes2 = BitConverter.GetBytes(0);
		Buffer.BlockCopy(array4, 0, array3, 0, array4.Length);
		Buffer.BlockCopy(bytes2, 0, array3, array4.Length, bytes2.Length);
		byte[] array5 = sHA.ComputeHash(array3);
		byte[] array6 = new byte[64];
		for (int j = 0; j < 64; j++)
		{
			array6[j] = 54;
		}
		int k = 0;
		for (int num = array5.Length; k < num; k++)
		{
			array6[k] ^= array5[k];
		}
		byte[] array7 = sHA.ComputeHash(array6);
		for (int l = 0; l < 64; l++)
		{
			array6[l] = 92;
		}
		int m = 0;
		for (int num2 = array5.Length; m < num2; m++)
		{
			array6[m] ^= array5[m];
		}
		sHA.ComputeHash(array6);
		if (keyLength <= array7.Length)
		{
			byte[] array8 = new byte[keyLength];
			Buffer.BlockCopy(array7, 0, array8, 0, keyLength);
			return array8;
		}
		throw new NotImplementedException();
	}

	internal byte[] CreateAgileEncryptionKey(HashAlgorithm hashAlgorithm, string password, byte[] salt, byte[] blockKey, int keyLength, int iterationCount)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(password);
		byte[] array = hashAlgorithm.ComputeHash(CombineArray(salt, bytes));
		for (int i = 0; i < iterationCount; i++)
		{
			array = hashAlgorithm.ComputeHash(CombineArray(BitConverter.GetBytes(i), array));
		}
		array = hashAlgorithm.ComputeHash(CombineArray(array, blockKey));
		return CorrectSize(array, keyLength, 54);
	}

	internal byte[] EncryptDecrypt(byte[] data, EncryptionMethod method, int blockSize)
	{
		int num = data.Length;
		byte[] array = new byte[num];
		byte[] array2 = new byte[blockSize];
		byte[] array3 = new byte[blockSize];
		for (int i = 0; i < num; i += blockSize)
		{
			int count = Math.Min(num - i, blockSize);
			Buffer.BlockCopy(data, i, array2, 0, count);
			method(array2, array3);
			Buffer.BlockCopy(array3, 0, array, i, count);
		}
		return array;
	}

	internal byte[] CombineArray(byte[] buffer1, byte[] buffer2)
	{
		int num = buffer1.Length;
		int num2 = buffer2.Length;
		byte[] array = new byte[num + num2];
		Buffer.BlockCopy(buffer1, 0, array, 0, num);
		Buffer.BlockCopy(buffer2, 0, array, num, num2);
		return array;
	}

	internal byte[] CorrectSize(byte[] data, int size, byte padding)
	{
		byte[] array = new byte[size];
		if (data.Length < size)
		{
			Buffer.BlockCopy(data, 0, array, 0, data.Length);
			for (int i = data.Length; i < size; i++)
			{
				array[i] = padding;
			}
		}
		else if (data.Length >= size)
		{
			Buffer.BlockCopy(data, 0, array, 0, size);
		}
		return array;
	}

	internal byte[] ConcatenateIV(byte[] data, byte[] IV)
	{
		byte[] array = new byte[data.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (byte)(data[i] ^ IV[i]);
		}
		return array;
	}

	internal bool CompareArray(byte[] buffer1, byte[] buffer2)
	{
		bool result = true;
		for (int i = 0; i < buffer1.Length; i++)
		{
			if (buffer1[i] != buffer2[i])
			{
				result = false;
				break;
			}
		}
		return result;
	}
}
