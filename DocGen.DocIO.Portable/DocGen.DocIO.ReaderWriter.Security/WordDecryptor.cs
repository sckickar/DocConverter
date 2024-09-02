using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class WordDecryptor
{
	private const int DEF_READ_LENGTH = 16;

	private const int DEF_PAS_LEN = 64;

	private const int DEF_BLOCK_SIZE = 512;

	private const int DEF_START_POS = 0;

	private const int DEF_INC_BYTE_MAXVAL = 256;

	private const uint DEF_PASSWORD_CONST = 52811u;

	private static readonly ushort[] initCodeArr = new ushort[15]
	{
		57840, 7439, 52380, 33984, 4364, 3600, 61902, 12606, 6258, 57657,
		54287, 34041, 10252, 43370, 20163
	};

	private static readonly ushort[,] encryptMatrix = new ushort[7, 15]
	{
		{
			44796, 31585, 17763, 885, 55369, 28485, 60195, 18387, 47201, 17824,
			43601, 30388, 14128, 13105, 4129
		},
		{
			19929, 63170, 35526, 1770, 41139, 56970, 50791, 36774, 24803, 35648,
			17539, 60776, 28256, 26210, 8258
		},
		{
			39858, 64933, 1453, 3540, 20807, 44341, 40175, 3949, 49606, 1697,
			35078, 51953, 56512, 52420, 16516
		},
		{
			10053, 60267, 2906, 7080, 41614, 19019, 10751, 7898, 37805, 3394,
			557, 34243, 43425, 35241, 33032
		},
		{
			20106, 50935, 5812, 14160, 21821, 38038, 21502, 15796, 14203, 6788,
			1114, 7079, 17251, 883, 4657
		},
		{
			40212, 40399, 11624, 28320, 43642, 14605, 43004, 31592, 28406, 13576,
			2228, 14158, 34502, 1766, 9314
		},
		{
			10761, 11199, 23248, 56640, 17621, 29210, 24537, 63184, 56812, 27152,
			4456, 28316, 7597, 3532, 18628
		}
	};

	private byte[] m_baDocumentID = new byte[16];

	private byte[] m_baPoint = new byte[64];

	private byte[] m_baHash = new byte[16];

	private byte[] m_baPassword = new byte[64];

	private MD5Context m_valContext = new MD5Context();

	private MemoryStream m_tableStream;

	private MemoryStream m_dataStream;

	private MemoryStream m_mainStream;

	private Fib m_fib;

	private bool m_bIsComplexFile;

	private WordKey m_key;

	internal MemoryStream TableStream => m_tableStream;

	internal MemoryStream MainStream => m_mainStream;

	internal MemoryStream DataStream => m_dataStream;

	internal WordDecryptor()
	{
	}

	internal WordDecryptor(MemoryStream tableStream, MemoryStream mainStream, MemoryStream dataStream, Fib fib)
	{
		m_tableStream = tableStream;
		m_mainStream = mainStream;
		m_dataStream = dataStream;
		m_fib = fib;
		m_bIsComplexFile = fib.FComplex;
	}

	public void TestEncrypt(ref MemoryStream stream, string password, ref byte[] docID, ref byte[] point, ref byte[] hash)
	{
		m_baDocumentID = docID;
		ConvertPassword(password);
		PrepareValContext();
		Buffer.BlockCopy(m_baDocumentID, 0, m_baPoint, 0, 16);
		m_baPoint[16] = 128;
		Array.Clear(m_baPoint, 17, 47);
		m_baPoint[56] = 128;
		MD5Context mD5Context = new MD5Context();
		mD5Context.Update(m_baPoint, 64u);
		mD5Context.StoreDigest();
		Buffer.BlockCopy(mD5Context.Digest, 0, m_baHash, 0, 16);
		MakeKey(0u);
		DecryptBuffer(m_baPoint, 16);
		DecryptBuffer(m_baHash, 16);
		stream = DecryptStream(stream);
		docID = m_baDocumentID;
		point = m_baPoint;
		hash = m_baHash;
	}

	public void TestDecrypt(ref MemoryStream stream, string password, ref byte[] docid, ref byte[] point, ref byte[] hash)
	{
		Buffer.BlockCopy(docid, 0, m_baDocumentID, 0, 16);
		Buffer.BlockCopy(point, 0, m_baPoint, 0, 16);
		Buffer.BlockCopy(hash, 0, m_baHash, 0, 16);
		ConvertPassword(password);
		VerifyPassword();
		stream = DecryptStream(stream);
	}

	internal bool CheckPassword(string password)
	{
		if (m_tableStream == null)
		{
			throw new ArgumentNullException("Table Stream is null referenced.");
		}
		m_tableStream.Position = 4L;
		m_tableStream.Read(m_baDocumentID, 0, 16);
		m_tableStream.Read(m_baPoint, 0, 16);
		m_tableStream.Read(m_baHash, 0, 16);
		ConvertPassword(password);
		return VerifyPassword();
	}

	internal void Decrypt()
	{
		m_tableStream = DecryptStream(m_tableStream);
		m_mainStream = DecryptStream(m_mainStream);
		if (m_dataStream != null)
		{
			m_dataStream = DecryptStream(m_dataStream);
		}
	}

	internal void Encrypt(string password)
	{
		m_baDocumentID = Guid.NewGuid().ToByteArray();
		ConvertPassword(password);
		PrepareValContext();
		Buffer.BlockCopy(m_baDocumentID, 0, m_baPoint, 0, 16);
		m_baPoint[16] = 128;
		Array.Clear(m_baPoint, 17, 47);
		m_baPoint[56] = 128;
		MD5Context mD5Context = new MD5Context();
		mD5Context.Update(m_baPoint, 64u);
		mD5Context.StoreDigest();
		Buffer.BlockCopy(mD5Context.Digest, 0, m_baHash, 0, 16);
		MakeKey(0u);
		DecryptBuffer(m_baPoint, 16);
		DecryptBuffer(m_baHash, 16);
		m_tableStream = DecryptStream(m_tableStream);
		m_tableStream.Position = 0L;
		m_tableStream.WriteByte(1);
		m_tableStream.WriteByte(0);
		m_tableStream.WriteByte(1);
		m_tableStream.WriteByte(0);
		m_tableStream.Write(m_baDocumentID, 0, 16);
		m_tableStream.Write(m_baPoint, 0, 16);
		m_tableStream.Write(m_baHash, 0, 16);
		m_mainStream = DecryptStream(m_mainStream);
		if (m_dataStream != null)
		{
			m_dataStream = DecryptStream(m_dataStream);
		}
	}

	private MemoryStream DecryptStream(MemoryStream stream)
	{
		byte[] array = new byte[16];
		MemoryStream memoryStream = new MemoryStream();
		long length = stream.Length;
		int num = 0;
		stream.Position = num;
		uint num2 = 0u;
		MakeKey(num2);
		while (num < length)
		{
			for (int i = stream.Read(array, 0, 16); i < 16; i++)
			{
				array[i] = 1;
			}
			DecryptBuffer(array, 16);
			memoryStream.Write(array, 0, 16);
			num += 16;
			if (num % 512 == 0)
			{
				num2++;
				MakeKey(num2);
			}
		}
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private void ConvertPassword(string password)
	{
		int i;
		for (i = 0; i < password.Length; i++)
		{
			m_baPassword[2 * i] = (byte)(password[i] & 0xFFu);
			m_baPassword[2 * i + 1] = (byte)((uint)((int)password[i] >> 8) & 0xFFu);
		}
		m_baPassword[2 * i] = 128;
		m_baPassword[56] = (byte)(i << 4);
	}

	private void PrepareKey(byte[] data)
	{
		m_key = new WordKey();
		byte b = 0;
		byte b2 = 0;
		for (int i = 0; i < 256; i++)
		{
			m_key.status[i] = (byte)i;
		}
		for (int j = 0; j < 256; j++)
		{
			b2 = (byte)((data[b] + m_key.status[j] + b2) % 256);
			byte b3 = m_key.status[j];
			m_key.status[j] = m_key.status[b2];
			m_key.status[b2] = b3;
			b = (byte)((b + 1) % 16);
		}
	}

	private void MakeKey(uint block)
	{
		MD5Context mD5Context = new MD5Context();
		byte[] array = new byte[64];
		Buffer.BlockCopy(m_valContext.Digest, 0, array, 0, 5);
		array[5] = (byte)(block & 0xFFu);
		array[6] = (byte)((block >> 8) & 0xFFu);
		array[7] = (byte)((block >> 16) & 0xFFu);
		array[8] = (byte)((block >> 24) & 0xFFu);
		array[9] = 128;
		array[56] = 72;
		mD5Context.Update(array, 64u);
		mD5Context.StoreDigest();
		PrepareKey(mD5Context.Digest);
	}

	private bool MemoryCompare(byte[] block1, byte[] block2, int length)
	{
		for (int i = 0; i < length; i++)
		{
			if (block1[i] != block2[i])
			{
				return false;
			}
		}
		return true;
	}

	private bool VerifyPassword()
	{
		PrepareValContext();
		MakeKey(0u);
		DecryptBuffer(m_baPoint, 16);
		DecryptBuffer(m_baHash, 16);
		m_baPoint[16] = 128;
		Array.Clear(m_baPoint, 17, 47);
		m_baPoint[56] = 128;
		MD5Context mD5Context = new MD5Context();
		mD5Context.Update(m_baPoint, 64u);
		mD5Context.StoreDigest();
		return MemoryCompare(mD5Context.Digest, m_baHash, 16);
	}

	private void PrepareValContext()
	{
		MD5Context mD5Context = new MD5Context();
		mD5Context.Update(m_baPassword, 64u);
		mD5Context.StoreDigest();
		m_valContext = new MD5Context();
		int num = 0;
		int srcOffset = 0;
		int num2 = 5;
		while (num != 16)
		{
			if (64 - num < 5)
			{
				num2 = 64 - num;
			}
			Buffer.BlockCopy(mD5Context.Digest, srcOffset, m_baPassword, num, num2);
			num += num2;
			if (num == 64)
			{
				m_valContext.Update(m_baPassword, 64u);
				srcOffset = num2;
				num2 = 5 - num2;
				num = 0;
			}
			else
			{
				srcOffset = 0;
				num2 = 5;
				Buffer.BlockCopy(m_baDocumentID, 0, m_baPassword, num, 16);
				num += 16;
			}
		}
		m_baPassword[16] = 128;
		Array.Clear(m_baPassword, 17, 47);
		m_baPassword[56] = 128;
		m_baPassword[57] = 10;
		m_valContext.Update(m_baPassword, 64u);
		m_valContext.StoreDigest();
	}

	private void DecryptBuffer(byte[] data, int length)
	{
		for (int i = 0; i < length; i++)
		{
			m_key.x = (byte)((m_key.x + 1) % 256);
			m_key.y = (byte)((m_key.status[m_key.x] + m_key.y) % 256);
			byte b = m_key.status[m_key.x];
			m_key.status[m_key.x] = m_key.status[m_key.y];
			m_key.status[m_key.y] = b;
			byte b2 = (byte)((m_key.status[m_key.x] + m_key.status[m_key.y]) % 256);
			data[i] ^= m_key.status[b2];
		}
	}

	internal static uint GetPasswordHash(string password)
	{
		if (string.IsNullOrEmpty(password))
		{
			return 0u;
		}
		if (password.Length > 15)
		{
			password = password.Substring(0, 15);
		}
		ushort lowOrderHash = GetLowOrderHash(password);
		return (uint)((GetHighOrderHash(password) << 16) | lowOrderHash);
	}

	private static uint RevertBytes(uint changeVal)
	{
		uint num = 0u;
		for (int i = 0; i < 4; i++)
		{
			num |= changeVal & 0xFFu;
			if (i < 3)
			{
				num <<= 8;
				changeVal >>= 8;
			}
		}
		return num;
	}

	private static ushort GetHighOrderHash(string password)
	{
		ushort num = initCodeArr[password.Length - 1];
		int num2 = 15 - password.Length;
		int i = 0;
		for (int length = password.Length; i < length; i++)
		{
			bool[] charBits = GetCharBits7(password[i]);
			for (int j = 0; j < 7; j++)
			{
				if (charBits[j])
				{
					num ^= encryptMatrix[j, num2 + i];
				}
			}
		}
		return num;
	}

	private static ushort GetLowOrderHash(string password)
	{
		if (password == null)
		{
			return 0;
		}
		ushort num = 0;
		int i = 0;
		for (int length = password.Length; i < length; i++)
		{
			ushort uInt16FromBits = GetUInt16FromBits(RotateBits(GetCharBits15(password[i]), i + 1));
			num ^= uInt16FromBits;
		}
		return (ushort)((ulong)(num ^ password.Length) ^ 0xCE4BuL);
	}

	private static bool[] GetCharBits7(char charToConvert)
	{
		ushort num = 1;
		bool[] array = new bool[7];
		ushort num2 = Convert.ToUInt16(charToConvert);
		if ((num2 & 0xFF) == 0)
		{
			num2 >>= 8;
		}
		for (int i = 0; i < 7; i++)
		{
			array[i] = (num2 & num) == num;
			num <<= 1;
		}
		return array;
	}

	private static bool[] GetCharBits15(char charToConvert)
	{
		bool[] array = new bool[15];
		ushort num = Convert.ToUInt16(charToConvert);
		ushort num2 = 1;
		for (int i = 0; i < 15; i++)
		{
			array[i] = (num & num2) == num2;
			num2 <<= 1;
		}
		return array;
	}

	private static ushort GetUInt16FromBits(bool[] bits)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		if (bits.Length > 16)
		{
			throw new ArgumentOutOfRangeException("There can't be more than 16 bits");
		}
		ushort num = 0;
		ushort num2 = 1;
		int i = 0;
		for (int num3 = bits.Length; i < num3; i++)
		{
			if (bits[i])
			{
				num += num2;
			}
			num2 <<= 1;
		}
		return num;
	}

	private static bool[] RotateBits(bool[] bits, int count)
	{
		if (bits == null)
		{
			throw new ArgumentNullException("bits");
		}
		if (bits.Length == 0)
		{
			return bits;
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count can't be less than zero");
		}
		bool[] array = new bool[bits.Length];
		int i = 0;
		for (int num = bits.Length; i < num; i++)
		{
			int num2 = (i + count) % num;
			array[num2] = bits[i];
		}
		return array;
	}

	public static int Round(int value, int degree)
	{
		if (degree == 0)
		{
			throw new ArgumentOutOfRangeException("degree can't be 0");
		}
		int num = value % degree;
		return value - num + degree;
	}
}
