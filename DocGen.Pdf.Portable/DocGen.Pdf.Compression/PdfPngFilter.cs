using System;

namespace DocGen.Pdf.Compression;

internal class PdfPngFilter
{
	internal enum Type
	{
		None,
		Sub,
		Up,
		Average,
		Paeth
	}

	private delegate void RowFilter(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR);

	private static RowFilter s_subFilter = CompressSub;

	private static RowFilter s_upFilter = CompressUp;

	private static RowFilter s_averageFilter = CompressAverage;

	private static RowFilter s_paethFilter = CompressPaeth;

	private static RowFilter s_decompressFilter = Decompress;

	private const byte m_zero = 0;

	public static byte[] Compress(byte[] data, int bpr, Type type)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (bpr <= 0)
		{
			throw new ArgumentException("There can't be less or equal to zero bytes in a line.", "bpr");
		}
		RowFilter filter;
		switch (type)
		{
		case Type.None:
			return data;
		case Type.Sub:
			filter = s_subFilter;
			break;
		case Type.Up:
			filter = s_upFilter;
			break;
		case Type.Average:
			filter = s_averageFilter;
			break;
		case Type.Paeth:
			filter = s_paethFilter;
			break;
		default:
			throw new ArgumentException("Unsupported PNG filter: " + type, "type");
		}
		return Modify(data, bpr, filter, pack: true);
	}

	public static byte[] Decompress(byte[] data, int bpr)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (bpr <= 0)
		{
			throw new ArgumentException("There can't be less or equal to zero bytes in a line.", "bpr");
		}
		return Modify(data, bpr + 1, s_decompressFilter, pack: false);
	}

	private static byte[] Modify(byte[] data, int bpr, RowFilter filter, bool pack)
	{
		long num = 0L;
		long num2 = data.Length;
		long num3 = num2 / bpr;
		int num4 = bpr - ((!pack) ? 1 : (-1));
		byte[] result = new byte[pack ? (num3 * num4) : (num3 * num4)];
		int num5 = 0;
		for (; num + bpr <= num2; num += bpr)
		{
			filter(data, num, bpr, result, num5, num4);
			num5 += num4;
		}
		return result;
	}

	private static void CompressSub(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		result[resIndex] = 1;
		resIndex++;
		for (int i = 0; i < resBPR; i++)
		{
			result[resIndex] = (byte)(data[inIndex] - ((i > 0) ? data[inIndex - 1] : 0));
			resIndex++;
			inIndex++;
		}
	}

	private static void CompressUp(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		long num = inIndex - inBPR;
		result[resIndex] = 2;
		resIndex++;
		for (int i = 0; i < inBPR; i++)
		{
			result[resIndex] = (byte)(data[inIndex] - ((num >= 0) ? data[num] : 0));
			resIndex++;
			inIndex++;
			num++;
		}
	}

	private static void CompressAverage(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		long num = inIndex - inBPR;
		result[resIndex] = 3;
		resIndex++;
		for (int i = 0; i < inBPR; i++)
		{
			result[resIndex] = (byte)(data[inIndex] - (((i > 0) ? data[inIndex - 1] : 0) + ((num >= 0) ? data[num] : 0)) >> 1);
			resIndex++;
			inIndex++;
			num++;
		}
	}

	private static void CompressPaeth(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		long num = inIndex - inBPR;
		result[resIndex] = 3;
		resIndex++;
		for (int i = 0; i < inBPR; i++)
		{
			byte a = (byte)((i > 0) ? data[inIndex - 1] : 0);
			byte b = (byte)((num >= 0) ? data[num] : 0);
			byte c = (byte)((num >= 1) ? data[num - 1] : 0);
			result[resIndex] = (byte)(data[inIndex] - PaethPredictor(a, b, c));
			resIndex++;
			inIndex++;
			num++;
		}
	}

	private static void Decompress(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		switch ((Type)data[inIndex])
		{
		case Type.None:
			DecompressNone(data, inIndex + 1, inBPR, result, resIndex, resBPR);
			break;
		case Type.Sub:
			DeompressSub(data, inIndex + 1, inBPR, result, resIndex, resBPR);
			break;
		case Type.Up:
			DecompressUp(data, inIndex + 1, inBPR, result, resIndex, resBPR);
			break;
		case Type.Average:
			DecompressAverage(data, inIndex + 1, inBPR, result, resIndex, resBPR);
			break;
		case Type.Paeth:
			DecompressPaeth(data, inIndex + 1, inBPR, result, resIndex, resBPR);
			break;
		default:
			throw new ArgumentException("Unsupported PNG filter: " + data[inIndex], "type");
		}
	}

	private static void DecompressNone(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		for (int i = 1; i < inBPR; i++)
		{
			result[resIndex] = data[inIndex];
			resIndex++;
			inIndex++;
		}
	}

	private static void DeompressSub(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		for (int i = 0; i < resBPR; i++)
		{
			result[resIndex] = (byte)(data[inIndex] + ((i > 0) ? result[resIndex - 1] : 0));
			resIndex++;
			inIndex++;
		}
	}

	private static byte[] DecompressUp(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		long num = resIndex - resBPR;
		for (int i = 0; i < resBPR; i++)
		{
			result[resIndex] = (byte)(data[inIndex] + ((num >= 0) ? result[num] : 0));
			resIndex++;
			inIndex++;
			num++;
		}
		return result;
	}

	private static void DecompressAverage(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		int num = 1;
		long num2 = resIndex - resBPR;
		byte[] array = new byte[resBPR];
		for (int i = 0; i < resBPR; i++)
		{
			result[resIndex + i] = data[inIndex + i];
		}
		for (int j = 0; j < 1; j++)
		{
			if (num2 < 0)
			{
				result[resIndex] = (byte)(data[inIndex] + array[resIndex]);
			}
			else
			{
				result[resIndex] = (byte)(data[inIndex] + result[num2] / 2);
			}
			num2++;
			resIndex++;
		}
		for (int k = num; k < resBPR; k++)
		{
			if (num2 < 0)
			{
				result[resIndex] += (byte)(((result[resIndex - num] & 0xFF) + (array[resIndex] & 0xFF)) / 2);
			}
			else
			{
				result[resIndex] += (byte)(((result[resIndex - num] & 0xFF) + (result[num2] & 0xFF)) / 2);
			}
			resIndex++;
			inIndex++;
			num2++;
		}
	}

	private static void DecompressPaeth(byte[] data, long inIndex, int inBPR, byte[] result, long resIndex, int resBPR)
	{
		int num = 1;
		long num2 = resIndex - resBPR;
		for (int i = 0; i < resBPR; i++)
		{
			result[resIndex + i] = data[inIndex + i];
		}
		for (int j = 0; j < num; j++)
		{
			result[resIndex] += result[num2];
			resIndex++;
			num2++;
		}
		for (int k = num; k < resBPR; k++)
		{
			int a = result[resIndex - num] & 0xFF;
			int b = result[num2] & 0xFF;
			int c = result[num2 - num] & 0xFF;
			result[resIndex] += PaethPredictor(a, b, c);
			resIndex++;
			inIndex++;
			num2++;
		}
	}

	private static byte PaethPredictor(int a, int b, int c)
	{
		int num = a + b - c;
		int num2 = Math.Abs(num - a);
		int num3 = Math.Abs(num - b);
		int num4 = Math.Abs(num - c);
		if (num2 <= num3 && num2 <= num4)
		{
			return (byte)a;
		}
		if (num3 <= num4)
		{
			return (byte)b;
		}
		return (byte)c;
	}
}
