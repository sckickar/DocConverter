using System.Globalization;

namespace DocGen.Pdf;

internal class HuffmanDecoder
{
	private Jbig2StreamReader reader;

	internal int jbig2HuffmanLOW = int.Parse("fffffffd", NumberStyles.HexNumber);

	internal int jbig2HuffmanOOB = int.Parse("fffffffe", NumberStyles.HexNumber);

	internal int jbig2HuffmanEOT = int.Parse("ffffffff", NumberStyles.HexNumber);

	internal int[][] huffmanTableA;

	internal int[][] huffmanTableB;

	internal int[][] huffmanTableC;

	internal int[][] huffmanTableD;

	internal int[][] huffmanTableE;

	internal int[][] huffmanTableF;

	internal int[][] huffmanTableG;

	internal int[][] huffmanTableH;

	internal int[][] huffmanTableI;

	internal int[][] huffmanTableJ;

	internal int[][] huffmanTableK;

	internal int[][] huffmanTableL;

	internal int[][] huffmanTableM;

	internal int[][] huffmanTableN;

	internal int[][] huffmanTableO;

	internal HuffmanDecoder(Jbig2StreamReader reader)
	{
		this.reader = reader;
		Initialize();
	}

	internal HuffmanDecoder()
	{
		Initialize();
	}

	internal void Initialize()
	{
		huffmanTableA = new int[5][]
		{
			new int[4] { 0, 1, 4, 0 },
			new int[4] { 16, 2, 8, 2 },
			new int[4] { 272, 3, 16, 6 },
			new int[4] { 65808, 3, 32, 7 },
			new int[4] { 0, 0, jbig2HuffmanEOT, 0 }
		};
		huffmanTableB = new int[8][]
		{
			new int[4] { 0, 1, 0, 0 },
			new int[4] { 1, 2, 0, 2 },
			new int[4] { 2, 3, 0, 6 },
			new int[4] { 3, 4, 3, 14 },
			new int[4] { 11, 5, 6, 30 },
			new int[4] { 75, 6, 32, 62 },
			new int[4] { 0, 6, jbig2HuffmanOOB, 63 },
			new int[4] { 0, 0, jbig2HuffmanEOT, 0 }
		};
		int[][] obj = new int[10][]
		{
			new int[4] { 0, 1, 0, 0 },
			new int[4] { 1, 2, 0, 2 },
			new int[4] { 2, 3, 0, 6 },
			new int[4] { 3, 4, 3, 14 },
			new int[4] { 11, 5, 6, 30 },
			new int[4] { 0, 6, jbig2HuffmanOOB, 62 },
			new int[4] { 75, 7, 32, 254 },
			new int[4] { -256, 8, 8, 254 },
			null,
			null
		};
		int[] obj2 = new int[4] { -257, 8, 0, 255 };
		obj2[2] = jbig2HuffmanLOW;
		obj[8] = obj2;
		obj[9] = new int[4] { 0, 0, jbig2HuffmanEOT, 0 };
		huffmanTableC = obj;
		huffmanTableD = new int[7][]
		{
			new int[4] { 1, 1, 0, 0 },
			new int[4] { 2, 2, 0, 2 },
			new int[4] { 3, 3, 0, 6 },
			new int[4] { 4, 4, 3, 14 },
			new int[4] { 12, 5, 6, 30 },
			new int[4] { 76, 5, 32, 31 },
			new int[4] { 0, 0, jbig2HuffmanEOT, 0 }
		};
		int[][] obj3 = new int[9][]
		{
			new int[4] { 1, 1, 0, 0 },
			new int[4] { 2, 2, 0, 2 },
			new int[4] { 3, 3, 0, 6 },
			new int[4] { 4, 4, 3, 14 },
			new int[4] { 12, 5, 6, 30 },
			new int[4] { 76, 6, 32, 62 },
			new int[4] { -255, 7, 8, 126 },
			null,
			null
		};
		int[] obj4 = new int[4] { -256, 7, 0, 127 };
		obj4[2] = jbig2HuffmanLOW;
		obj3[7] = obj4;
		obj3[8] = new int[4] { 0, 0, jbig2HuffmanEOT, 0 };
		huffmanTableE = obj3;
		int[][] obj5 = new int[15][]
		{
			new int[4] { 0, 2, 7, 0 },
			new int[4] { 128, 3, 7, 2 },
			new int[4] { 256, 3, 8, 3 },
			new int[4] { -1024, 4, 9, 8 },
			new int[4] { -512, 4, 8, 9 },
			new int[4] { -256, 4, 7, 10 },
			new int[4] { -32, 4, 5, 11 },
			new int[4] { 512, 4, 9, 12 },
			new int[4] { 1024, 4, 10, 13 },
			new int[4] { -2048, 5, 10, 28 },
			new int[4] { -128, 5, 6, 29 },
			new int[4] { -64, 5, 5, 30 },
			null,
			null,
			null
		};
		int[] obj6 = new int[4] { -2049, 6, 0, 62 };
		obj6[2] = jbig2HuffmanLOW;
		obj5[12] = obj6;
		obj5[13] = new int[4] { 2048, 6, 32, 63 };
		obj5[14] = new int[4] { 0, 0, jbig2HuffmanEOT, 0 };
		huffmanTableF = obj5;
		int[][] obj7 = new int[16][]
		{
			new int[4] { -512, 3, 8, 0 },
			new int[4] { 256, 3, 8, 1 },
			new int[4] { 512, 3, 9, 2 },
			new int[4] { 1024, 3, 10, 3 },
			new int[4] { -1024, 4, 9, 8 },
			new int[4] { -256, 4, 7, 9 },
			new int[4] { -32, 4, 5, 10 },
			new int[4] { 0, 4, 5, 11 },
			new int[4] { 128, 4, 7, 12 },
			new int[4] { -128, 5, 6, 26 },
			new int[4] { -64, 5, 5, 27 },
			new int[4] { 32, 5, 5, 28 },
			new int[4] { 64, 5, 6, 29 },
			null,
			null,
			null
		};
		int[] obj8 = new int[4] { -1025, 5, 0, 30 };
		obj8[2] = jbig2HuffmanLOW;
		obj7[13] = obj8;
		obj7[14] = new int[4] { 2048, 5, 32, 31 };
		obj7[15] = new int[4] { 0, 0, jbig2HuffmanEOT, 0 };
		huffmanTableG = obj7;
		int[][] obj9 = new int[22][]
		{
			new int[4] { 0, 2, 1, 0 },
			new int[4] { 0, 2, jbig2HuffmanOOB, 1 },
			new int[4] { 4, 3, 4, 4 },
			new int[4] { -1, 4, 0, 10 },
			new int[4] { 22, 4, 4, 11 },
			new int[4] { 38, 4, 5, 12 },
			new int[4] { 2, 5, 0, 26 },
			new int[4] { 70, 5, 6, 27 },
			new int[4] { 134, 5, 7, 28 },
			new int[4] { 3, 6, 0, 58 },
			new int[4] { 20, 6, 1, 59 },
			new int[4] { 262, 6, 7, 60 },
			new int[4] { 646, 6, 10, 61 },
			new int[4] { -2, 7, 0, 124 },
			new int[4] { 390, 7, 8, 125 },
			new int[4] { -15, 8, 3, 252 },
			new int[4] { -5, 8, 1, 253 },
			new int[4] { -7, 9, 1, 508 },
			new int[4] { -3, 9, 0, 509 },
			null,
			null,
			null
		};
		int[] obj10 = new int[4] { -16, 9, 0, 510 };
		obj10[2] = jbig2HuffmanLOW;
		obj9[19] = obj10;
		obj9[20] = new int[4] { 1670, 9, 32, 511 };
		obj9[21] = new int[4] { 0, 0, jbig2HuffmanEOT, 0 };
		huffmanTableH = obj9;
		int[][] obj11 = new int[23][]
		{
			new int[4] { 0, 2, jbig2HuffmanOOB, 0 },
			new int[4] { -1, 3, 1, 2 },
			new int[4] { 1, 3, 1, 3 },
			new int[4] { 7, 3, 5, 4 },
			new int[4] { -3, 4, 1, 10 },
			new int[4] { 43, 4, 5, 11 },
			new int[4] { 75, 4, 6, 12 },
			new int[4] { 3, 5, 1, 26 },
			new int[4] { 139, 5, 7, 27 },
			new int[4] { 267, 5, 8, 28 },
			new int[4] { 5, 6, 1, 58 },
			new int[4] { 39, 6, 2, 59 },
			new int[4] { 523, 6, 8, 60 },
			new int[4] { 1291, 6, 11, 61 },
			new int[4] { -5, 7, 1, 124 },
			new int[4] { 779, 7, 9, 125 },
			new int[4] { -31, 8, 4, 252 },
			new int[4] { -11, 8, 2, 253 },
			new int[4] { -15, 9, 2, 508 },
			new int[4] { -7, 9, 1, 509 },
			null,
			null,
			null
		};
		int[] obj12 = new int[4] { -32, 9, 0, 510 };
		obj12[2] = jbig2HuffmanLOW;
		obj11[20] = obj12;
		obj11[21] = new int[4] { 3339, 9, 32, 511 };
		obj11[22] = new int[4] { 0, 0, jbig2HuffmanEOT, 0 };
		huffmanTableI = obj11;
		int[][] obj13 = new int[22][]
		{
			new int[4] { -2, 2, 2, 0 },
			new int[4] { 6, 2, 6, 1 },
			new int[4] { 0, 2, jbig2HuffmanOOB, 2 },
			new int[4] { -3, 5, 0, 24 },
			new int[4] { 2, 5, 0, 25 },
			new int[4] { 70, 5, 5, 26 },
			new int[4] { 3, 6, 0, 54 },
			new int[4] { 102, 6, 5, 55 },
			new int[4] { 134, 6, 6, 56 },
			new int[4] { 198, 6, 7, 57 },
			new int[4] { 326, 6, 8, 58 },
			new int[4] { 582, 6, 9, 59 },
			new int[4] { 1094, 6, 10, 60 },
			new int[4] { -21, 7, 4, 122 },
			new int[4] { -4, 7, 0, 123 },
			new int[4] { 4, 7, 0, 124 },
			new int[4] { 2118, 7, 11, 125 },
			new int[4] { -5, 8, 0, 252 },
			new int[4] { 5, 8, 0, 253 },
			null,
			null,
			null
		};
		int[] obj14 = new int[4] { -22, 8, 0, 254 };
		obj14[2] = jbig2HuffmanLOW;
		obj13[19] = obj14;
		obj13[20] = new int[4] { 4166, 8, 32, 255 };
		obj13[21] = new int[4] { 0, 0, jbig2HuffmanEOT, 0 };
		huffmanTableJ = obj13;
		huffmanTableK = new int[14][]
		{
			new int[4] { 1, 1, 0, 0 },
			new int[4] { 2, 2, 1, 2 },
			new int[4] { 4, 4, 0, 12 },
			new int[4] { 5, 4, 1, 13 },
			new int[4] { 7, 5, 1, 28 },
			new int[4] { 9, 5, 2, 29 },
			new int[4] { 13, 6, 2, 60 },
			new int[4] { 17, 7, 2, 122 },
			new int[4] { 21, 7, 3, 123 },
			new int[4] { 29, 7, 4, 124 },
			new int[4] { 45, 7, 5, 125 },
			new int[4] { 77, 7, 6, 126 },
			new int[4] { 141, 7, 32, 127 },
			new int[4] { 0, 0, jbig2HuffmanEOT, 0 }
		};
		huffmanTableL = new int[14][]
		{
			new int[4] { 1, 1, 0, 0 },
			new int[4] { 2, 2, 0, 2 },
			new int[4] { 3, 3, 1, 6 },
			new int[4] { 5, 5, 0, 28 },
			new int[4] { 6, 5, 1, 29 },
			new int[4] { 8, 6, 1, 60 },
			new int[4] { 10, 7, 0, 122 },
			new int[4] { 11, 7, 1, 123 },
			new int[4] { 13, 7, 2, 124 },
			new int[4] { 17, 7, 3, 125 },
			new int[4] { 25, 7, 4, 126 },
			new int[4] { 41, 8, 5, 254 },
			new int[4] { 73, 8, 32, 255 },
			new int[4] { 0, 0, jbig2HuffmanEOT, 0 }
		};
		huffmanTableM = new int[14][]
		{
			new int[4] { 1, 1, 0, 0 },
			new int[4] { 2, 3, 0, 4 },
			new int[4] { 7, 3, 3, 5 },
			new int[4] { 3, 4, 0, 12 },
			new int[4] { 5, 4, 1, 13 },
			new int[4] { 4, 5, 0, 28 },
			new int[4] { 15, 6, 1, 58 },
			new int[4] { 17, 6, 2, 59 },
			new int[4] { 21, 6, 3, 60 },
			new int[4] { 29, 6, 4, 61 },
			new int[4] { 45, 6, 5, 62 },
			new int[4] { 77, 7, 6, 126 },
			new int[4] { 141, 7, 32, 127 },
			new int[4] { 0, 0, jbig2HuffmanEOT, 0 }
		};
		huffmanTableN = new int[6][]
		{
			new int[4] { 0, 1, 0, 0 },
			new int[4] { -2, 3, 0, 4 },
			new int[4] { -1, 3, 0, 5 },
			new int[4] { 1, 3, 0, 6 },
			new int[4] { 2, 3, 0, 7 },
			new int[4] { 0, 0, jbig2HuffmanEOT, 0 }
		};
		int[][] obj15 = new int[14][]
		{
			new int[4] { 0, 1, 0, 0 },
			new int[4] { -1, 3, 0, 4 },
			new int[4] { 1, 3, 0, 5 },
			new int[4] { -2, 4, 0, 12 },
			new int[4] { 2, 4, 0, 13 },
			new int[4] { -4, 5, 1, 28 },
			new int[4] { 3, 5, 1, 29 },
			new int[4] { -8, 6, 2, 60 },
			new int[4] { 5, 6, 2, 61 },
			new int[4] { -24, 7, 4, 124 },
			new int[4] { 9, 7, 4, 125 },
			null,
			null,
			null
		};
		int[] obj16 = new int[4] { -25, 7, 0, 126 };
		obj16[2] = jbig2HuffmanLOW;
		obj15[11] = obj16;
		obj15[12] = new int[4] { 25, 7, 32, 127 };
		obj15[13] = new int[4] { 0, 0, jbig2HuffmanEOT, 0 };
		huffmanTableO = obj15;
	}

	internal DecodeIntResult DecodeInt(int[][] table)
	{
		int i = 0;
		int num = 0;
		for (int j = 0; table[j][2] != jbig2HuffmanEOT; j++)
		{
			for (; i < table[j][1]; i++)
			{
				int num2 = reader.ReadBit();
				num = (num << 1) | num2;
			}
			if (num == table[j][3])
			{
				if (table[j][2] == jbig2HuffmanOOB)
				{
					return new DecodeIntResult(-1, booleanResult: false);
				}
				int intResult;
				if (table[j][2] == jbig2HuffmanLOW)
				{
					int num3 = reader.ReadBits(32);
					intResult = table[j][0] - num3;
				}
				else if (table[j][2] > 0)
				{
					int num4 = reader.ReadBits(table[j][2]);
					intResult = table[j][0] + num4;
				}
				else
				{
					intResult = table[j][0];
				}
				return new DecodeIntResult(intResult, booleanResult: true);
			}
		}
		return new DecodeIntResult(-1, booleanResult: false);
	}

	internal int[][] BuildTable(int[][] table, int length)
	{
		int i;
		for (i = 0; i < length; i++)
		{
			int j;
			for (j = i; j < length && table[j][1] == 0; j++)
			{
			}
			if (j == length)
			{
				break;
			}
			for (int k = j + 1; k < length; k++)
			{
				if (table[k][1] > 0 && table[k][1] < table[j][1])
				{
					j = k;
				}
			}
			if (j != i)
			{
				int[] array = table[j];
				for (int k = j; k > i; k--)
				{
					table[k] = table[k - 1];
				}
				table[i] = array;
			}
		}
		table[i] = table[length];
		i = 0;
		int num = 0;
		table[i++][3] = num++;
		for (; table[i][2] != jbig2HuffmanEOT; i++)
		{
			num <<= table[i][1] - table[i - 1][1];
			table[i][3] = num++;
		}
		return table;
	}
}
