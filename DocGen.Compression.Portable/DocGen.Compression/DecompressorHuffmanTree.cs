using System;

namespace DocGen.Compression;

public class DecompressorHuffmanTree
{
	private static int MAX_BITLEN;

	private short[] m_Tree;

	private static DecompressorHuffmanTree m_LengthTree;

	private static DecompressorHuffmanTree m_DistanceTree;

	public static DecompressorHuffmanTree LengthTree => m_LengthTree;

	public static DecompressorHuffmanTree DistanceTree => m_DistanceTree;

	static DecompressorHuffmanTree()
	{
		MAX_BITLEN = 15;
		try
		{
			byte[] array = new byte[288];
			int num = 0;
			while (num < 144)
			{
				array[num++] = 8;
			}
			while (num < 256)
			{
				array[num++] = 9;
			}
			while (num < 280)
			{
				array[num++] = 7;
			}
			while (num < 288)
			{
				array[num++] = 8;
			}
			m_LengthTree = new DecompressorHuffmanTree(array);
			array = new byte[32];
			num = 0;
			while (num < 32)
			{
				array[num++] = 5;
			}
			m_DistanceTree = new DecompressorHuffmanTree(array);
		}
		catch (Exception innerException)
		{
			throw new Exception("DecompressorHuffmanTree: fixed trees generation failed", innerException);
		}
	}

	public DecompressorHuffmanTree(byte[] codeLengths)
	{
		BuildTree(codeLengths);
	}

	private int PrepareData(int[] blCount, int[] nextCode, byte[] lengths, out int treeSize)
	{
		int num = 0;
		treeSize = 512;
		foreach (int num2 in lengths)
		{
			if (num2 > 0)
			{
				blCount[num2]++;
			}
		}
		for (int j = 1; j <= MAX_BITLEN; j++)
		{
			nextCode[j] = num;
			num += blCount[j] << 16 - j;
			if (j >= 10)
			{
				int num3 = nextCode[j] & 0x1FF80;
				int num4 = num & 0x1FF80;
				treeSize += num4 - num3 >> 16 - j;
			}
		}
		return num;
	}

	private short[] TreeFromData(int[] blCount, int[] nextCode, byte[] lengths, int code, int treeSize)
	{
		short[] array = new short[treeSize];
		int num = 512;
		int num2 = 128;
		for (int num3 = MAX_BITLEN; num3 >= 10; num3--)
		{
			int num4 = code & 0x1FF80;
			code -= blCount[num3] << 16 - num3;
			for (int i = code & 0x1FF80; i < num4; i += num2)
			{
				array[Utils.BitReverse(i)] = (short)((-num << 4) | num3);
				num += 1 << num3 - 9;
			}
		}
		for (int j = 0; j < lengths.Length; j++)
		{
			int num5 = lengths[j];
			if (num5 == 0)
			{
				continue;
			}
			code = nextCode[num5];
			int num6 = Utils.BitReverse(code);
			if (num5 <= 9)
			{
				do
				{
					array[num6] = (short)((j << 4) | num5);
					num6 += 1 << num5;
				}
				while (num6 < 512);
			}
			else
			{
				int num7 = array[num6 & 0x1FF];
				int num8 = 1 << (num7 & 0xF);
				num7 = -(num7 >> 4);
				do
				{
					array[num7 | (num6 >> 9)] = (short)((j << 4) | num5);
					num6 += 1 << num5;
				}
				while (num6 < num8);
			}
			nextCode[num5] = code + (1 << 16 - num5);
		}
		return array;
	}

	private void BuildTree(byte[] lengths)
	{
		int[] blCount = new int[MAX_BITLEN + 1];
		int[] nextCode = new int[MAX_BITLEN + 1];
		int treeSize;
		int code = PrepareData(blCount, nextCode, lengths, out treeSize);
		m_Tree = TreeFromData(blCount, nextCode, lengths, code, treeSize);
	}

	public int UnpackSymbol(CompressedStreamReader input)
	{
		int num;
		int num2;
		if ((num = input.PeekBits(9)) >= 0)
		{
			if ((num2 = m_Tree[num]) >= 0)
			{
				input.SkipBits(num2 & 0xF);
				return num2 >> 4;
			}
			int num3 = -(num2 >> 4);
			int count = num2 & 0xF;
			if ((num = input.PeekBits(count)) >= 0)
			{
				num2 = m_Tree[num3 | (num >> 9)];
				input.SkipBits(num2 & 0xF);
				return num2 >> 4;
			}
			int availableBits = input.AvailableBits;
			num = input.PeekBits(availableBits);
			num2 = m_Tree[num3 | (num >> 9)];
			if ((num2 & 0xF) <= availableBits)
			{
				input.SkipBits(num2 & 0xF);
				return num2 >> 4;
			}
			return -1;
		}
		int availableBits2 = input.AvailableBits;
		num = input.PeekBits(availableBits2);
		num2 = m_Tree[num];
		if (num2 >= 0 && (num2 & 0xF) <= availableBits2)
		{
			input.SkipBits(num2 & 0xF);
			return num2 >> 4;
		}
		return -1;
	}
}
