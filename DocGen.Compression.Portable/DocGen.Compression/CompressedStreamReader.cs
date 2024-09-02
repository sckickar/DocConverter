using System;
using System.IO;

namespace DocGen.Compression;

public class CompressedStreamReader
{
	private const int DEF_HEADER_METHOD_MASK = 3840;

	private const int DEF_HEADER_INFO_MASK = 61440;

	private const int DEF_HEADER_FLAGS_FCHECK = 31;

	private const int DEF_HEADER_FLAGS_FDICT = 32;

	private const int DEF_HEADER_FLAGS_FLEVEL = 192;

	private static readonly int[] DEF_HUFFMAN_DYNTREE_REPEAT_MINIMUMS = new int[3] { 3, 3, 11 };

	private static readonly int[] DEF_HUFFMAN_DYNTREE_REPEAT_BITS = new int[3] { 2, 3, 7 };

	private const int DEF_MAX_WINDOW_SIZE = 65535;

	private static readonly int[] DEF_HUFFMAN_REPEAT_LENGTH_BASE = new int[29]
	{
		3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
		15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
		67, 83, 99, 115, 131, 163, 195, 227, 258
	};

	private static readonly int[] DEF_HUFFMAN_REPEAT_LENGTH_EXTENSION = new int[29]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
		1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
		4, 4, 4, 4, 5, 5, 5, 5, 0
	};

	private static readonly int[] DEF_HUFFMAN_REPEAT_DISTANCE_BASE = new int[30]
	{
		1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
		33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
		1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
	};

	private static readonly int[] DEF_HUFFMAN_REPEAT_DISTANCE_EXTENSION = new int[30]
	{
		0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
		4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
		9, 9, 10, 10, 11, 11, 12, 12, 13, 13
	};

	private const int DEF_HUFFMAN_REPEATE_MAX = 258;

	private const int DEF_HUFFMAN_END_BLOCK = 256;

	private const int DEF_HUFFMAN_LENGTH_MINIMUM_CODE = 257;

	private const int DEF_HUFFMAN_LENGTH_MAXIMUM_CODE = 285;

	private const int DEF_HUFFMAN_DISTANCE_MAXIMUM_CODE = 29;

	private Stream m_InputStream;

	private long m_CheckSum = 1L;

	private uint m_Buffer;

	private int m_BufferedBits;

	private byte[] m_temp_buffer = new byte[4];

	private byte[] m_Block_Buffer = new byte[65535];

	private bool m_bNoWrap;

	private int m_WindowSize;

	private long m_CurrentPosition;

	private long m_DataLength;

	private bool m_bReadingUncompressed;

	private int m_UncompressedDataLength;

	private bool m_bCanReadNextBlock = true;

	private bool m_bCanReadMoreData = true;

	private DecompressorHuffmanTree m_CurrentLengthTree;

	private DecompressorHuffmanTree m_CurrentDistanceTree;

	private bool m_bCheckSumRead;

	protected internal int AvailableBits => m_BufferedBits;

	protected internal long AvailableBytes => m_InputStream.Length - m_InputStream.Position + m_BufferedBits >> 3;

	public CompressedStreamReader(Stream stream)
		: this(stream, bNoWrap: false)
	{
	}

	public CompressedStreamReader(Stream stream, bool bNoWrap)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream.Length == 0L)
		{
			throw new ArgumentException("stream - string can not be empty");
		}
		m_InputStream = stream;
		m_bNoWrap = bNoWrap;
		if (!m_bNoWrap)
		{
			ReadZLibHeader();
		}
		DecodeBlockHeader();
	}

	protected void ChecksumReset()
	{
		m_CheckSum = 1L;
	}

	protected void ChecksumUpdate(byte[] buffer, int offset, int length)
	{
		ChecksumCalculator.ChecksumUpdate(ref m_CheckSum, buffer, offset, length);
	}

	protected internal void SkipToBoundary()
	{
		m_Buffer >>= m_BufferedBits & 7;
		m_BufferedBits &= -8;
	}

	protected internal int ReadPackedBytes(byte[] buffer, int offset, int length)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length - 1)
		{
			throw new ArgumentOutOfRangeException("offset", "Offset can not be less than zero or greater than buffer length - 1.");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length can not be less than zero.");
		}
		if (length > buffer.Length - offset)
		{
			throw new ArgumentOutOfRangeException("length", "Length is too large.");
		}
		if (((uint)m_BufferedBits & 7u) != 0)
		{
			throw new NotSupportedException("Reading of unalligned data is not supported.");
		}
		if (length == 0)
		{
			return 0;
		}
		int num = 0;
		while (m_BufferedBits > 0 && length > 0)
		{
			buffer[offset++] = (byte)m_Buffer;
			m_BufferedBits -= 8;
			m_Buffer >>= 8;
			length--;
			num++;
		}
		if (length > 0)
		{
			num += m_InputStream.Read(buffer, offset, length);
		}
		return num;
	}

	protected void FillBuffer()
	{
		int num = 4 - (m_BufferedBits >> 3) - (((m_BufferedBits & 7) != 0) ? 1 : 0);
		if (num != 0)
		{
			int num2 = m_InputStream.Read(m_temp_buffer, 0, num);
			for (int i = 0; i < num2; i++)
			{
				m_Buffer |= (uint)(m_temp_buffer[i] << m_BufferedBits);
				m_BufferedBits += 8;
			}
		}
	}

	protected internal int PeekBits(int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Bits count can not be less than zero.");
		}
		if (count > 32)
		{
			throw new ArgumentOutOfRangeException("count", "Count of bits is too large.");
		}
		if (m_BufferedBits < count)
		{
			FillBuffer();
		}
		if (m_BufferedBits < count)
		{
			return -1;
		}
		uint num = (uint)(~(-1 << count));
		return (int)(m_Buffer & num);
	}

	protected internal void SkipBits(int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Bits count can not be less than zero.");
		}
		if (count == 0)
		{
			return;
		}
		if (count >= m_BufferedBits)
		{
			count -= m_BufferedBits;
			m_BufferedBits = 0;
			m_Buffer = 0u;
			if (count > 0)
			{
				m_InputStream.Position += count >> 3;
				count &= 7;
				if (count > 0)
				{
					FillBuffer();
					m_BufferedBits -= count;
					m_Buffer >>= count;
				}
			}
		}
		else
		{
			m_BufferedBits -= count;
			m_Buffer >>= count;
		}
	}

	protected internal int ReadBits(int count)
	{
		int num = PeekBits(count);
		if (num == -1)
		{
			return -1;
		}
		m_BufferedBits -= count;
		m_Buffer >>= count;
		return num;
	}

	protected internal int ReadInt16()
	{
		return (ReadBits(8) << 8) | ReadBits(8);
	}

	protected internal int ReadInt16Inverted()
	{
		return ReadBits(8) | (ReadBits(8) << 8);
	}

	protected internal long ReadInt32()
	{
		return (long)(uint)(ReadBits(8) << 24) | (long)(uint)(ReadBits(8) << 16) | (uint)(ReadBits(8) << 8) | (uint)ReadBits(8);
	}

	protected void ReadZLibHeader()
	{
		int num = ReadInt16();
		if (num == -1)
		{
			throw new Exception("Header of the stream can not be read.");
		}
		if (num % 31 != 0)
		{
			throw new FormatException("Header checksum illegal");
		}
		if ((num & 0xF00) != 2048)
		{
			throw new FormatException("Unsupported compression method.");
		}
		m_WindowSize = (int)Math.Pow(2.0, ((num & 0xF000) >> 12) + 8);
		if (m_WindowSize > 65535)
		{
			throw new FormatException("Unsupported window size for deflate compression method.");
		}
		if ((num & 0x20) >> 5 == 1)
		{
			throw new NotImplementedException("Custom dictionary is not supported at the moment.");
		}
	}

	protected string BitsToString(int bits, int count)
	{
		string text = "";
		for (int i = 0; i < count; i++)
		{
			if ((i & 7) == 0)
			{
				text = " " + text;
			}
			text = (bits & 1) + text;
			bits >>= 1;
		}
		return text;
	}

	protected void DecodeDynHeader(out DecompressorHuffmanTree lengthTree, out DecompressorHuffmanTree distanceTree)
	{
		byte b = 0;
		int num = ReadBits(5);
		int num2 = ReadBits(5);
		int num3 = ReadBits(4);
		if (num < 0 || num2 < 0 || num3 < 0)
		{
			throw new FormatException("Wrong dynamic huffman codes.");
		}
		num += 257;
		num2++;
		int num4 = num + num2;
		byte[] array = new byte[num4];
		byte[] array2 = new byte[19];
		num3 += 4;
		int num5 = 0;
		while (num5 < num3)
		{
			int num6 = ReadBits(3);
			if (num6 < 0)
			{
				throw new FormatException("Wrong dynamic huffman codes.");
			}
			array2[Utils.DEF_HUFFMAN_DYNTREE_CODELENGTHS_ORDER[num5++]] = (byte)num6;
		}
		DecompressorHuffmanTree decompressorHuffmanTree = new DecompressorHuffmanTree(array2);
		num5 = 0;
		do
		{
			bool flag = false;
			int num7;
			while (((num7 = decompressorHuffmanTree.UnpackSymbol(this)) & -16) == 0)
			{
				b = (array[num5++] = (byte)num7);
				if (num5 == num4)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			if (num7 < 0)
			{
				throw new FormatException("Wrong dynamic huffman codes.");
			}
			if (num7 >= 17)
			{
				b = 0;
			}
			else if (num5 == 0)
			{
				throw new FormatException("Wrong dynamic huffman codes.");
			}
			int num8 = num7 - 16;
			int count = DEF_HUFFMAN_DYNTREE_REPEAT_BITS[num8];
			int num9 = ReadBits(count);
			if (num9 < 0)
			{
				throw new FormatException("Wrong dynamic huffman codes.");
			}
			num9 += DEF_HUFFMAN_DYNTREE_REPEAT_MINIMUMS[num8];
			if (num5 + num9 > num4)
			{
				throw new FormatException("Wrong dynamic huffman codes.");
			}
			while (num9-- > 0)
			{
				array[num5++] = b;
			}
		}
		while (num5 != num4);
		byte[] array3 = new byte[num];
		Array.Copy(array, 0, array3, 0, num);
		lengthTree = new DecompressorHuffmanTree(array3);
		array3 = new byte[num2];
		Array.Copy(array, num, array3, 0, num2);
		distanceTree = new DecompressorHuffmanTree(array3);
	}

	protected bool DecodeBlockHeader()
	{
		if (!m_bCanReadNextBlock)
		{
			return false;
		}
		int num = ReadBits(1);
		if (num == -1)
		{
			return false;
		}
		int num2 = ReadBits(2);
		if (num2 == -1)
		{
			return false;
		}
		m_bCanReadNextBlock = num == 0;
		switch (num2)
		{
		case 0:
		{
			m_bReadingUncompressed = true;
			SkipToBoundary();
			int num3 = ReadInt16Inverted();
			int num4 = ReadInt16Inverted();
			if (num3 != (num4 ^ 0xFFFF))
			{
				throw new FormatException("Wrong block length.");
			}
			if (num3 > 65535)
			{
				throw new FormatException("Uncompressed block length can not be more than 65535.");
			}
			m_UncompressedDataLength = num3;
			m_CurrentLengthTree = null;
			m_CurrentDistanceTree = null;
			break;
		}
		case 1:
			m_bReadingUncompressed = false;
			m_UncompressedDataLength = -1;
			m_CurrentLengthTree = DecompressorHuffmanTree.LengthTree;
			m_CurrentDistanceTree = DecompressorHuffmanTree.DistanceTree;
			break;
		case 2:
			m_bReadingUncompressed = false;
			m_UncompressedDataLength = -1;
			DecodeDynHeader(out m_CurrentLengthTree, out m_CurrentDistanceTree);
			break;
		default:
			throw new FormatException("Wrong block type.");
		}
		return true;
	}

	private bool ReadHuffman()
	{
		int num = 65535 - (int)(m_DataLength - m_CurrentPosition);
		bool flag = false;
		while (num >= 258)
		{
			int num2;
			while (((num2 = m_CurrentLengthTree.UnpackSymbol(this)) & -256) == 0)
			{
				m_Block_Buffer[m_DataLength++ % 65535] = (byte)num2;
				flag = true;
				if (--num < 258)
				{
					return true;
				}
			}
			if (num2 < 257)
			{
				if (num2 < 256)
				{
					throw new FormatException("Illegal code.");
				}
				return flag | (m_bCanReadMoreData = DecodeBlockHeader());
			}
			if (num2 > 285)
			{
				throw new FormatException("Illegal repeat code length.");
			}
			int num3 = DEF_HUFFMAN_REPEAT_LENGTH_BASE[num2 - 257];
			int num4 = DEF_HUFFMAN_REPEAT_LENGTH_EXTENSION[num2 - 257];
			if (num4 > 0)
			{
				int num5 = ReadBits(num4);
				if (num5 < 0)
				{
					throw new FormatException("Wrong data.");
				}
				num3 += num5;
			}
			num2 = m_CurrentDistanceTree.UnpackSymbol(this);
			if (num2 < 0 || num2 > DEF_HUFFMAN_REPEAT_DISTANCE_BASE.Length)
			{
				throw new FormatException("Wrong distance code.");
			}
			int num6 = DEF_HUFFMAN_REPEAT_DISTANCE_BASE[num2];
			num4 = DEF_HUFFMAN_REPEAT_DISTANCE_EXTENSION[num2];
			if (num4 > 0)
			{
				int num7 = ReadBits(num4);
				if (num7 < 0)
				{
					throw new FormatException("Wrong data.");
				}
				num6 += num7;
			}
			for (int i = 0; i < num3; i++)
			{
				m_Block_Buffer[m_DataLength % 65535] = m_Block_Buffer[(m_DataLength - num6) % 65535];
				m_DataLength++;
				num--;
			}
			flag = true;
		}
		return flag;
	}

	public int Read(byte[] buffer, int offset, int length)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length - 1)
		{
			throw new ArgumentOutOfRangeException("offset", "Offset does not belong to specified buffer.");
		}
		if (length < 0 || length > buffer.Length - offset)
		{
			throw new ArgumentOutOfRangeException("length", "Length is illegal.");
		}
		int num = length;
		while (length > 0)
		{
			if (m_CurrentPosition < m_DataLength)
			{
				int num2 = (int)(m_CurrentPosition % 65535);
				int val = Math.Min(65535 - num2, (int)(m_DataLength - m_CurrentPosition));
				val = Math.Min(val, length);
				Array.Copy(m_Block_Buffer, num2, buffer, offset, val);
				m_CurrentPosition += val;
				offset += val;
				length -= val;
				continue;
			}
			if (!m_bCanReadMoreData)
			{
				break;
			}
			long dataLength = m_DataLength;
			if (!m_bReadingUncompressed)
			{
				if (!ReadHuffman())
				{
					break;
				}
			}
			else if (m_UncompressedDataLength == 0)
			{
				if (!(m_bCanReadMoreData = DecodeBlockHeader()))
				{
					break;
				}
			}
			else
			{
				int num3 = (int)(m_DataLength % 65535);
				int num4 = Math.Min(m_UncompressedDataLength, 65535 - num3);
				int num5 = ReadPackedBytes(m_Block_Buffer, num3, num4);
				if (num4 != num5)
				{
					throw new FormatException("Not enough data in stream.");
				}
				m_UncompressedDataLength -= num5;
				m_DataLength += num5;
			}
			if (dataLength >= m_DataLength)
			{
				continue;
			}
			int num6 = (int)(dataLength % 65535);
			int num7 = (int)(m_DataLength % 65535);
			if (num6 < num7)
			{
				ChecksumUpdate(m_Block_Buffer, num6, num7 - num6);
				continue;
			}
			ChecksumUpdate(m_Block_Buffer, num6, 65535 - num6);
			if (num7 > 0)
			{
				ChecksumUpdate(m_Block_Buffer, 0, num7);
			}
		}
		if (!m_bCanReadMoreData && !m_bCheckSumRead && !m_bNoWrap)
		{
			SkipToBoundary();
			if (ReadInt32() != m_CheckSum)
			{
				throw new Exception("Checksum check failed.");
			}
			m_bCheckSumRead = true;
		}
		return num - length;
	}
}
