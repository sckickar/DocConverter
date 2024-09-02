using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Compression;

internal class PdfCcittEncoder
{
	private const int c_G3code_Eol = -1;

	private const int c_G3code_Invalid = -2;

	private const int c_G3code_Eof = -3;

	private const int c_G3code_Incomp = -4;

	private const int c_Length = 0;

	private const int c_Code = 1;

	private const int c_Runlen = 2;

	private const int c_Eol = 1;

	private static byte[] s_tableZeroSpan;

	private static byte[] s_tableOneSpan;

	private static int[][] s_terminatingWhiteCodes;

	private static int[][] s_terminatingBlackCodes;

	private static int[] s_horizontalTabel;

	private static int[] s_passcode;

	private static int[] s_maskTabel;

	private static int[][] s_verticalTable;

	private int m_rowbytes;

	private int m_rowPixels;

	private int m_countBit = 8;

	private int m_data;

	private byte[] m_refline;

	private List<byte> m_outBuf = new List<byte>();

	private byte[] m_imageData;

	private int m_offsetData;

	static PdfCcittEncoder()
	{
		s_horizontalTabel = new int[3] { 3, 1, 0 };
		s_passcode = new int[3] { 4, 1, 0 };
		s_maskTabel = new int[9] { 0, 1, 3, 7, 15, 31, 63, 127, 255 };
		CreteTableZeroSpan();
		CreteTableOneSpan();
		CreateTerminatingWhiteCodes();
		CreateTerminatingBlackCodes();
		CreateVerticalTable();
	}

	public byte[] EncodeData(byte[] data, int width, int height)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_rowPixels = width;
		m_rowbytes = (int)Math.Ceiling((float)m_rowPixels / 8f);
		m_refline = new byte[m_rowbytes];
		m_imageData = data;
		m_offsetData = 0;
		for (int num = m_rowbytes * height; num > 0; num -= m_rowbytes)
		{
			Fax3Encode();
			Array.Copy(m_imageData, m_offsetData, m_refline, 0, m_rowbytes);
			m_offsetData += m_rowbytes;
		}
		Fax4Encode();
		byte[] array = new byte[m_outBuf.Count];
		int i = 0;
		for (int count = m_outBuf.Count; i < count; i++)
		{
			array[i] = m_outBuf[i];
		}
		return array;
	}

	private static void CreateVerticalTable()
	{
		s_verticalTable = new int[7][]
		{
			new int[3] { 7, 3, 0 },
			new int[3] { 6, 3, 0 },
			new int[3] { 3, 3, 0 },
			new int[3] { 1, 1, 0 },
			new int[3] { 3, 2, 0 },
			new int[3] { 6, 2, 0 },
			new int[3] { 7, 2, 0 }
		};
	}

	private static void CreteTableZeroSpan()
	{
		s_tableZeroSpan = new byte[256]
		{
			8, 7, 6, 6, 5, 5, 5, 5, 4, 4,
			4, 4, 4, 4, 4, 4, 3, 3, 3, 3,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			3, 3, 2, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 2, 2, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		};
	}

	private static void CreteTableOneSpan()
	{
		s_tableOneSpan = new byte[256]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 2, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 2, 2, 3, 3, 3, 3, 3, 3,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			4, 4, 4, 4, 4, 4, 4, 4, 5, 5,
			5, 5, 6, 6, 7, 8
		};
	}

	private static void CreateTerminatingWhiteCodes()
	{
		s_terminatingWhiteCodes = new int[109][]
		{
			new int[3] { 8, 53, 0 },
			new int[3] { 6, 7, 1 },
			new int[3] { 4, 7, 2 },
			new int[3] { 4, 8, 3 },
			new int[3] { 4, 11, 4 },
			new int[3] { 4, 12, 5 },
			new int[3] { 4, 14, 6 },
			new int[3] { 4, 15, 7 },
			new int[3] { 5, 19, 8 },
			new int[3] { 5, 20, 9 },
			new int[3] { 5, 7, 10 },
			new int[3] { 5, 8, 11 },
			new int[3] { 6, 8, 12 },
			new int[3] { 6, 3, 13 },
			new int[3] { 6, 52, 14 },
			new int[3] { 6, 53, 15 },
			new int[3] { 6, 42, 16 },
			new int[3] { 6, 43, 17 },
			new int[3] { 7, 39, 18 },
			new int[3] { 7, 12, 19 },
			new int[3] { 7, 8, 20 },
			new int[3] { 7, 23, 21 },
			new int[3] { 7, 3, 22 },
			new int[3] { 7, 4, 23 },
			new int[3] { 7, 40, 24 },
			new int[3] { 7, 43, 25 },
			new int[3] { 7, 19, 26 },
			new int[3] { 7, 36, 27 },
			new int[3] { 7, 24, 28 },
			new int[3] { 8, 2, 29 },
			new int[3] { 8, 3, 30 },
			new int[3] { 8, 26, 31 },
			new int[3] { 8, 27, 32 },
			new int[3] { 8, 18, 33 },
			new int[3] { 8, 19, 34 },
			new int[3] { 8, 20, 35 },
			new int[3] { 8, 21, 36 },
			new int[3] { 8, 22, 37 },
			new int[3] { 8, 23, 38 },
			new int[3] { 8, 40, 39 },
			new int[3] { 8, 41, 40 },
			new int[3] { 8, 42, 41 },
			new int[3] { 8, 43, 42 },
			new int[3] { 8, 44, 43 },
			new int[3] { 8, 45, 44 },
			new int[3] { 8, 4, 45 },
			new int[3] { 8, 5, 46 },
			new int[3] { 8, 10, 47 },
			new int[3] { 8, 11, 48 },
			new int[3] { 8, 82, 49 },
			new int[3] { 8, 83, 50 },
			new int[3] { 8, 84, 51 },
			new int[3] { 8, 85, 52 },
			new int[3] { 8, 36, 53 },
			new int[3] { 8, 37, 54 },
			new int[3] { 8, 88, 55 },
			new int[3] { 8, 89, 56 },
			new int[3] { 8, 90, 57 },
			new int[3] { 8, 91, 58 },
			new int[3] { 8, 74, 59 },
			new int[3] { 8, 75, 60 },
			new int[3] { 8, 50, 61 },
			new int[3] { 8, 51, 62 },
			new int[3] { 8, 52, 63 },
			new int[3] { 5, 27, 64 },
			new int[3] { 5, 18, 128 },
			new int[3] { 6, 23, 192 },
			new int[3] { 7, 55, 256 },
			new int[3] { 8, 54, 320 },
			new int[3] { 8, 55, 384 },
			new int[3] { 8, 100, 448 },
			new int[3] { 8, 101, 512 },
			new int[3] { 8, 104, 576 },
			new int[3] { 8, 103, 640 },
			new int[3] { 9, 204, 704 },
			new int[3] { 9, 205, 768 },
			new int[3] { 9, 210, 832 },
			new int[3] { 9, 211, 896 },
			new int[3] { 9, 212, 960 },
			new int[3] { 9, 213, 1024 },
			new int[3] { 9, 214, 1088 },
			new int[3] { 9, 215, 1152 },
			new int[3] { 9, 216, 1216 },
			new int[3] { 9, 217, 1280 },
			new int[3] { 9, 218, 1344 },
			new int[3] { 9, 219, 1408 },
			new int[3] { 9, 152, 1472 },
			new int[3] { 9, 153, 1536 },
			new int[3] { 9, 154, 1600 },
			new int[3] { 6, 24, 1664 },
			new int[3] { 9, 155, 1728 },
			new int[3] { 11, 8, 1792 },
			new int[3] { 11, 12, 1856 },
			new int[3] { 11, 13, 1920 },
			new int[3] { 12, 18, 1984 },
			new int[3] { 12, 19, 2048 },
			new int[3] { 12, 20, 2112 },
			new int[3] { 12, 21, 2176 },
			new int[3] { 12, 22, 2240 },
			new int[3] { 12, 23, 2304 },
			new int[3] { 12, 28, 2368 },
			new int[3] { 12, 29, 2432 },
			new int[3] { 12, 30, 2496 },
			new int[3] { 12, 31, 2560 },
			new int[3] { 12, 1, -1 },
			new int[3] { 9, 1, -2 },
			new int[3] { 10, 1, -2 },
			new int[3] { 11, 1, -2 },
			new int[3] { 12, 0, -2 }
		};
	}

	private static void CreateTerminatingBlackCodes()
	{
		s_terminatingBlackCodes = new int[109][]
		{
			new int[3] { 10, 55, 0 },
			new int[3] { 3, 2, 1 },
			new int[3] { 2, 3, 2 },
			new int[3] { 2, 2, 3 },
			new int[3] { 3, 3, 4 },
			new int[3] { 4, 3, 5 },
			new int[3] { 4, 2, 6 },
			new int[3] { 5, 3, 7 },
			new int[3] { 6, 5, 8 },
			new int[3] { 6, 4, 9 },
			new int[3] { 7, 4, 10 },
			new int[3] { 7, 5, 11 },
			new int[3] { 7, 7, 12 },
			new int[3] { 8, 4, 13 },
			new int[3] { 8, 7, 14 },
			new int[3] { 9, 24, 15 },
			new int[3] { 10, 23, 16 },
			new int[3] { 10, 24, 17 },
			new int[3] { 10, 8, 18 },
			new int[3] { 11, 103, 19 },
			new int[3] { 11, 104, 20 },
			new int[3] { 11, 108, 21 },
			new int[3] { 11, 55, 22 },
			new int[3] { 11, 40, 23 },
			new int[3] { 11, 23, 24 },
			new int[3] { 11, 24, 25 },
			new int[3] { 12, 202, 26 },
			new int[3] { 12, 203, 27 },
			new int[3] { 12, 204, 28 },
			new int[3] { 12, 205, 29 },
			new int[3] { 12, 104, 30 },
			new int[3] { 12, 105, 31 },
			new int[3] { 12, 106, 32 },
			new int[3] { 12, 107, 33 },
			new int[3] { 12, 210, 34 },
			new int[3] { 12, 211, 35 },
			new int[3] { 12, 212, 36 },
			new int[3] { 12, 213, 37 },
			new int[3] { 12, 214, 38 },
			new int[3] { 12, 215, 39 },
			new int[3] { 12, 108, 40 },
			new int[3] { 12, 109, 41 },
			new int[3] { 12, 218, 42 },
			new int[3] { 12, 219, 43 },
			new int[3] { 12, 84, 44 },
			new int[3] { 12, 85, 45 },
			new int[3] { 12, 86, 46 },
			new int[3] { 12, 87, 47 },
			new int[3] { 12, 100, 48 },
			new int[3] { 12, 101, 49 },
			new int[3] { 12, 82, 50 },
			new int[3] { 12, 83, 51 },
			new int[3] { 12, 36, 52 },
			new int[3] { 12, 55, 53 },
			new int[3] { 12, 56, 54 },
			new int[3] { 12, 39, 55 },
			new int[3] { 12, 40, 56 },
			new int[3] { 12, 88, 57 },
			new int[3] { 12, 89, 58 },
			new int[3] { 12, 43, 59 },
			new int[3] { 12, 44, 60 },
			new int[3] { 12, 90, 61 },
			new int[3] { 12, 102, 62 },
			new int[3] { 12, 103, 63 },
			new int[3] { 10, 15, 64 },
			new int[3] { 12, 200, 128 },
			new int[3] { 12, 201, 192 },
			new int[3] { 12, 91, 256 },
			new int[3] { 12, 51, 320 },
			new int[3] { 12, 52, 384 },
			new int[3] { 12, 53, 448 },
			new int[3] { 13, 108, 512 },
			new int[3] { 13, 109, 576 },
			new int[3] { 13, 74, 640 },
			new int[3] { 13, 75, 704 },
			new int[3] { 13, 76, 768 },
			new int[3] { 13, 77, 832 },
			new int[3] { 13, 114, 896 },
			new int[3] { 13, 115, 960 },
			new int[3] { 13, 116, 1024 },
			new int[3] { 13, 117, 1088 },
			new int[3] { 13, 118, 1152 },
			new int[3] { 13, 119, 1216 },
			new int[3] { 13, 82, 1280 },
			new int[3] { 13, 83, 1344 },
			new int[3] { 13, 84, 1408 },
			new int[3] { 13, 85, 1472 },
			new int[3] { 13, 90, 1536 },
			new int[3] { 13, 91, 1600 },
			new int[3] { 13, 100, 1664 },
			new int[3] { 13, 101, 1728 },
			new int[3] { 11, 8, 1792 },
			new int[3] { 11, 12, 1856 },
			new int[3] { 11, 13, 1920 },
			new int[3] { 12, 18, 1984 },
			new int[3] { 12, 19, 2048 },
			new int[3] { 12, 20, 2112 },
			new int[3] { 12, 21, 2176 },
			new int[3] { 12, 22, 2240 },
			new int[3] { 12, 23, 2304 },
			new int[3] { 12, 28, 2368 },
			new int[3] { 12, 29, 2432 },
			new int[3] { 12, 30, 2496 },
			new int[3] { 12, 31, 2560 },
			new int[3] { 12, 1, -1 },
			new int[3] { 9, 1, -2 },
			new int[3] { 10, 1, -2 },
			new int[3] { 11, 1, -2 },
			new int[3] { 12, 0, -2 }
		};
	}

	private void Putcode(int[] table)
	{
		PutBits(table[1], table[0]);
	}

	private void PutSpan(int span, int[][] tab)
	{
		int bits;
		int length;
		while (span >= 2624)
		{
			int[] array = tab[103];
			bits = array[1];
			length = array[0];
			PutBits(bits, length);
			span -= array[2];
		}
		if (span >= 64)
		{
			int[] array2 = tab[63 + (span >> 6)];
			bits = array2[1];
			length = array2[0];
			PutBits(bits, length);
			span -= array2[2];
		}
		bits = tab[span][1];
		length = tab[span][0];
		PutBits(bits, length);
	}

	private void PutBits(int bits, int length)
	{
		while (length > m_countBit)
		{
			m_data |= bits >> length - m_countBit;
			length -= m_countBit;
			m_outBuf.Add((byte)m_data);
			m_data = 0;
			m_countBit = 8;
		}
		m_data |= (bits & s_maskTabel[length]) << m_countBit - length;
		m_countBit -= length;
		if (m_countBit == 0)
		{
			m_outBuf.Add((byte)m_data);
			m_data = 0;
			m_countBit = 8;
		}
	}

	private void Fax3Encode()
	{
		int num = 0;
		int num2 = ((Pixel(m_imageData, m_offsetData, 0) == 0) ? Finddiff(m_imageData, m_offsetData, 0, m_rowPixels, 0) : 0);
		int num3 = ((Pixel(m_refline, 0, 0) == 0) ? Finddiff(m_refline, 0, 0, m_rowPixels, 0) : 0);
		while (true)
		{
			int num4 = Finddiff2(m_refline, 0, num3, m_rowPixels, Pixel(m_refline, 0, num3));
			if (num4 >= num2)
			{
				int num5 = num3 - num2;
				if (-3 > num5 || num5 > 3)
				{
					int num6 = Finddiff2(m_imageData, m_offsetData, num2, m_rowPixels, Pixel(m_imageData, m_offsetData, num2));
					Putcode(s_horizontalTabel);
					if (num + num2 == 0 || Pixel(m_imageData, m_offsetData, num) == 0)
					{
						PutSpan(num2 - num, s_terminatingWhiteCodes);
						PutSpan(num6 - num2, s_terminatingBlackCodes);
					}
					else
					{
						PutSpan(num2 - num, s_terminatingBlackCodes);
						PutSpan(num6 - num2, s_terminatingWhiteCodes);
					}
					num = num6;
				}
				else
				{
					Putcode(s_verticalTable[num5 + 3]);
					num = num2;
				}
			}
			else
			{
				Putcode(s_passcode);
				num = num4;
			}
			if (num < m_rowPixels)
			{
				num2 = Finddiff(m_imageData, m_offsetData, num, m_rowPixels, Pixel(m_imageData, m_offsetData, num));
				num3 = Finddiff(m_refline, 0, num, m_rowPixels, Pixel(m_imageData, m_offsetData, num) ^ 1);
				num3 = Finddiff(m_refline, 0, num3, m_rowPixels, Pixel(m_imageData, m_offsetData, num));
				continue;
			}
			break;
		}
	}

	private void Fax4Encode()
	{
		PutBits(1, 12);
		PutBits(1, 12);
		if (m_countBit != 8)
		{
			m_outBuf.Add((byte)m_data);
			m_data = 0;
			m_countBit = 8;
		}
	}

	private int Pixel(byte[] data, int offset, int bit)
	{
		int result = 0;
		if (bit < m_rowPixels)
		{
			result = ((data[offset + (bit >> 3)] & 0xFF) >> 7 - (bit & 7)) & 1;
		}
		return result;
	}

	private int FindFirstSpan(byte[] bp, int offset, int bs, int be)
	{
		int num = be - bs;
		int num2 = offset + (bs >> 3);
		int num4;
		int num3;
		if (num > 0 && (num3 = bs & 7) != 0)
		{
			num4 = s_tableOneSpan[(bp[num2] << num3) & 0xFF];
			if (num4 > 8 - num3)
			{
				num4 = 8 - num3;
			}
			if (num4 > num)
			{
				num4 = num;
			}
			if (num3 + num4 < 8)
			{
				return num4;
			}
			num -= num4;
			num2++;
		}
		else
		{
			num4 = 0;
		}
		while (num >= 8)
		{
			if (bp[num2] != byte.MaxValue)
			{
				return num4 + s_tableOneSpan[bp[num2] & 0xFF];
			}
			num4 += 8;
			num -= 8;
			num2++;
		}
		if (num > 0)
		{
			num3 = s_tableOneSpan[bp[num2] & 0xFF];
			num4 += ((num3 > num) ? num : num3);
		}
		return num4;
	}

	private int FindZeroSpan(byte[] bp, int offset, int bs, int be)
	{
		int num = be - bs;
		int num2 = offset + (bs >> 3);
		int num4;
		int num3;
		if (num > 0 && (num3 = bs & 7) != 0)
		{
			num4 = s_tableZeroSpan[(bp[num2] << num3) & 0xFF];
			if (num4 > 8 - num3)
			{
				num4 = 8 - num3;
			}
			if (num4 > num)
			{
				num4 = num;
			}
			if (num3 + num4 < 8)
			{
				return num4;
			}
			num -= num4;
			num2++;
		}
		else
		{
			num4 = 0;
		}
		while (num >= 8)
		{
			if (bp[num2] != 0)
			{
				return num4 + s_tableZeroSpan[bp[num2] & 0xFF];
			}
			num4 += 8;
			num -= 8;
			num2++;
		}
		if (num > 0)
		{
			num3 = s_tableZeroSpan[bp[num2] & 0xFF];
			num4 += ((num3 > num) ? num : num3);
		}
		return num4;
	}

	private int Finddiff(byte[] bp, int offset, int bs, int be, int color)
	{
		return bs + ((color != 0) ? FindFirstSpan(bp, offset, bs, be) : FindZeroSpan(bp, offset, bs, be));
	}

	private int Finddiff2(byte[] bp, int offset, int bs, int be, int color)
	{
		if (bs >= be)
		{
			return be;
		}
		return Finddiff(bp, offset, bs, be, color);
	}
}
