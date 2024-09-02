using System.IO;

namespace DocGen.Pdf.Graphics.Images.Decoder;

public static class StreamExtensions
{
	private static byte[] m_jpegSignature = new byte[2] { 255, 216 };

	private static byte[] m_pngSignature = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

	private static byte[] m_bmpSignature = new byte[2] { 66, 77 };

	private static byte[] m_mp3Signature = new byte[2] { 255, 251 };

	private static byte[] m_movSignature = new byte[10] { 0, 0, 0, 20, 102, 116, 121, 112, 113, 116 };

	private static byte[] m_mp4Signature = new byte[11]
	{
		0, 0, 0, 32, 102, 116, 121, 112, 105, 115,
		111
	};

	private static byte[] m_3gpSignature = new byte[10] { 0, 0, 0, 32, 102, 116, 121, 112, 51, 103 };

	private static byte[] m_m4vSignature = new byte[11]
	{
		0, 0, 0, 20, 102, 116, 121, 112, 77, 52,
		86
	};

	private static byte[] m_tiffSignature = new byte[4] { 73, 73, 42, 0 };

	private static byte[] m_gifSignature = new byte[4] { 71, 73, 70, 56 };

	private static byte[] m_tiffSignature2 = new byte[4] { 77, 77, 0, 42 };

	public static int ReadUInt32(this Stream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return (array[0] << 24) + (array[1] << 16) + (array[2] << 8) + array[3];
	}

	public static int ReadInt32(this Stream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return array[0] + (array[1] << 8) + (array[2] << 16) + (array[3] << 24);
	}

	public static int ReadUInt16(this Stream stream)
	{
		byte[] array = new byte[2];
		stream.Read(array, 0, 2);
		return (array[0] << 8) + array[1];
	}

	public static int ReadInt16(this Stream stream)
	{
		byte[] array = new byte[2];
		stream.Read(array, 0, 2);
		return array[0] | (array[1] << 8);
	}

	public static int ReadWord(this Stream stream)
	{
		return (stream.ReadByte() + (stream.ReadByte() << 8)) & 0xFFFF;
	}

	public static int ReadShortLE(this Stream stream)
	{
		int num = stream.ReadWord();
		if (num > 32767)
		{
			num -= 65536;
		}
		return num;
	}

	public static string ReadString(this Stream stream, int len)
	{
		char[] array = new char[len];
		for (int i = 0; i < len; i++)
		{
			array[i] = (char)stream.ReadByte();
		}
		return new string(array);
	}

	public static void Reset(this Stream stream)
	{
		stream.Position = 0L;
	}

	public static void Skip(this Stream stream, int noOfBytes)
	{
		long num = stream.Length - stream.Position;
		byte[] array = null;
		array = ((noOfBytes <= num) ? new byte[noOfBytes] : new byte[num]);
		stream.Read(array, 0, array.Length);
	}

	public static int ReadByte(this Stream stream)
	{
		return stream.ReadByte();
	}

	internal static byte[] ReadByte(this Stream stream, int len)
	{
		byte[] array = new byte[len];
		for (int i = 0; i < len; i++)
		{
			array[i] = (byte)stream.ReadByte();
		}
		stream.Skip(4);
		return array;
	}

	public static bool IsJpeg(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_jpegSignature.Length; i++)
		{
			if (m_jpegSignature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsBmp(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_bmpSignature.Length; i++)
		{
			if (m_bmpSignature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsPng(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_pngSignature.Length; i++)
		{
			if (m_pngSignature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsTiff(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_tiffSignature.Length; i++)
		{
			if (m_tiffSignature[i] != stream.ReadByte())
			{
				if (stream.Position > 0)
				{
					stream.Position--;
				}
				if (m_tiffSignature2[i] != stream.ReadByte())
				{
					return false;
				}
			}
		}
		return true;
	}

	internal static bool IsGif(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_gifSignature.Length; i++)
		{
			if (m_gifSignature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsMP3(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_mp3Signature.Length; i++)
		{
			if (m_mp3Signature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsMov(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_movSignature.Length; i++)
		{
			if (m_movSignature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsM4v(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_m4vSignature.Length; i++)
		{
			if (m_m4vSignature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool Is3Gp(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_3gpSignature.Length; i++)
		{
			if (m_3gpSignature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsMP4(this Stream stream)
	{
		stream.Reset();
		for (int i = 0; i < m_mp4Signature.Length; i++)
		{
			if (m_mp4Signature[i] != stream.ReadByte())
			{
				return false;
			}
		}
		return true;
	}
}
