using System;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

public class PdfDataMatrixBarcode : PdfBidimensionalBarcode
{
	private PdfDataMatrixEncoding dataMatrixEncoding;

	private PdfDataMatrixSize size;

	internal byte[,] dataMatrixArray;

	private PdfDataMatrixSymbolAttribute[] symbolAttributes;

	private PdfDataMatrixSymbolAttribute symbolAttribute;

	private int[] log;

	private int[] aLog;

	private byte[] polynomial;

	private int blockLength;

	private const int dpi = 96;

	private int m_quiteZoneLeft = 1;

	private int m_quiteZoneRight = 1;

	private int m_quiteZoneTop = 1;

	private int m_quiteZoneBottom = 1;

	private bool autoTagcheck;

	public PdfDataMatrixEncoding Encoding
	{
		get
		{
			return dataMatrixEncoding;
		}
		set
		{
			dataMatrixEncoding = value;
		}
	}

	public new PdfDataMatrixSize Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
		}
	}

	internal int ActualRows => symbolAttribute.SymbolRow + (m_quiteZoneTop + m_quiteZoneBottom);

	internal int ActualColumns => symbolAttribute.SymbolColumn + (m_quiteZoneLeft + m_quiteZoneRight);

	public PdfDataMatrixBarcode()
	{
		Initialize();
	}

	public PdfDataMatrixBarcode(string text)
		: this()
	{
		base.Text = text;
	}

	private void Initialize()
	{
		base.QuietZone.All = 1f;
		Encoding = PdfDataMatrixEncoding.Auto;
		Size = PdfDataMatrixSize.Auto;
		base.XDimension = 0.86f;
		symbolAttributes = new PdfDataMatrixSymbolAttribute[30]
		{
			new PdfDataMatrixSymbolAttribute(10, 10, 1, 1, 3, 5, 1, 3),
			new PdfDataMatrixSymbolAttribute(12, 12, 1, 1, 5, 7, 1, 5),
			new PdfDataMatrixSymbolAttribute(14, 14, 1, 1, 8, 10, 1, 8),
			new PdfDataMatrixSymbolAttribute(16, 16, 1, 1, 12, 12, 1, 12),
			new PdfDataMatrixSymbolAttribute(18, 18, 1, 1, 18, 14, 1, 18),
			new PdfDataMatrixSymbolAttribute(20, 20, 1, 1, 22, 18, 1, 22),
			new PdfDataMatrixSymbolAttribute(22, 22, 1, 1, 30, 20, 1, 30),
			new PdfDataMatrixSymbolAttribute(24, 24, 1, 1, 36, 24, 1, 36),
			new PdfDataMatrixSymbolAttribute(26, 26, 1, 1, 44, 28, 1, 44),
			new PdfDataMatrixSymbolAttribute(32, 32, 2, 2, 62, 36, 1, 62),
			new PdfDataMatrixSymbolAttribute(36, 36, 2, 2, 86, 42, 1, 86),
			new PdfDataMatrixSymbolAttribute(40, 40, 2, 2, 114, 48, 1, 114),
			new PdfDataMatrixSymbolAttribute(44, 44, 2, 2, 144, 56, 1, 144),
			new PdfDataMatrixSymbolAttribute(48, 48, 2, 2, 174, 68, 1, 174),
			new PdfDataMatrixSymbolAttribute(52, 52, 2, 2, 204, 84, 2, 102),
			new PdfDataMatrixSymbolAttribute(64, 64, 4, 4, 280, 112, 2, 140),
			new PdfDataMatrixSymbolAttribute(72, 72, 4, 4, 368, 144, 4, 92),
			new PdfDataMatrixSymbolAttribute(80, 80, 4, 4, 456, 192, 4, 114),
			new PdfDataMatrixSymbolAttribute(88, 88, 4, 4, 576, 224, 4, 144),
			new PdfDataMatrixSymbolAttribute(96, 96, 4, 4, 696, 272, 4, 174),
			new PdfDataMatrixSymbolAttribute(104, 104, 4, 4, 816, 336, 6, 136),
			new PdfDataMatrixSymbolAttribute(120, 120, 6, 6, 1050, 408, 6, 175),
			new PdfDataMatrixSymbolAttribute(132, 132, 6, 6, 1304, 496, 8, 163),
			new PdfDataMatrixSymbolAttribute(144, 144, 6, 6, 1558, 620, 10, 156),
			new PdfDataMatrixSymbolAttribute(8, 18, 1, 1, 5, 7, 1, 5),
			new PdfDataMatrixSymbolAttribute(8, 32, 2, 1, 10, 11, 1, 10),
			new PdfDataMatrixSymbolAttribute(12, 26, 1, 1, 16, 14, 1, 16),
			new PdfDataMatrixSymbolAttribute(12, 36, 2, 1, 22, 18, 1, 22),
			new PdfDataMatrixSymbolAttribute(16, 36, 2, 1, 32, 24, 1, 32),
			new PdfDataMatrixSymbolAttribute(16, 48, 2, 1, 49, 28, 1, 49)
		};
		CreateLogArrays();
	}

	private void CreateLogArrays()
	{
		log = new int[256];
		aLog = new int[256];
		log[0] = -255;
		aLog[0] = 1;
		for (int i = 1; i <= 255; i++)
		{
			aLog[i] = aLog[i - 1] * 2;
			if (aLog[i] >= 256)
			{
				aLog[i] ^= 0x12D;
			}
			log[aLog[i]] = i;
		}
	}

	private void CreatePolynomial(int step)
	{
		blockLength = 69;
		polynomial = new byte[blockLength];
		int num = symbolAttribute.CorrectionCodewords / step;
		for (int i = 0; i < polynomial.Length; i++)
		{
			polynomial[i] = 1;
		}
		for (int j = 1; j <= num; j++)
		{
			for (int num2 = j - 1; num2 >= 0; num2--)
			{
				polynomial[num2] = ErrorCorrectionCodeDoublify(polynomial[num2], j);
				if (num2 > 0)
				{
					polynomial[num2] = ErrorCorrectionCodeSum(polynomial[num2], polynomial[num2 - 1]);
				}
			}
		}
	}

	private void CreateMatrix(int[] codeword)
	{
		int symbolColumn = symbolAttribute.SymbolColumn;
		int symbolRow = symbolAttribute.SymbolRow;
		int num = symbolColumn / symbolAttribute.HoriDataRegion;
		int num2 = symbolRow / symbolAttribute.VertDataRegion;
		int num3 = symbolColumn - 2 * (symbolColumn / num);
		int num4 = symbolRow - 2 * (symbolRow / num2);
		int[] array = new int[num3 * num4];
		ErrorCorrectingCode200Placement(array, num4, num3);
		byte[] array2 = new byte[symbolColumn * symbolRow];
		for (int i = 0; i < symbolRow; i += num2)
		{
			for (int j = 0; j < symbolColumn; j++)
			{
				array2[i * symbolColumn + j] = 1;
			}
			for (int j = 0; j < symbolColumn; j += 2)
			{
				array2[(i + num2 - 1) * symbolColumn + j] = 1;
			}
		}
		for (int j = 0; j < symbolColumn; j += num)
		{
			for (int i = 0; i < symbolRow; i++)
			{
				array2[i * symbolColumn + j] = 1;
			}
			for (int i = 0; i < symbolRow; i += 2)
			{
				array2[i * symbolColumn + j + num - 1] = 1;
			}
		}
		for (int i = 0; i < num4; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				int num5 = array[(num4 - i - 1) * num3 + j];
				if (num5 == 1 || (num5 > 7 && (codeword[(num5 >> 3) - 1] & (1 << (num5 & 7))) != 0))
				{
					array2[(1 + i + 2 * (i / (num2 - 2))) * symbolColumn + 1 + j + 2 * (j / (num - 2))] = 1;
				}
			}
		}
		int symbolColumn2 = symbolAttribute.SymbolColumn;
		int symbolRow2 = symbolAttribute.SymbolRow;
		byte[,] array3 = new byte[symbolColumn2, symbolRow2];
		for (int k = 0; k < symbolColumn2; k++)
		{
			for (int l = 0; l < symbolRow2; l++)
			{
				array3[k, l] = array2[symbolColumn2 * l + k];
			}
		}
		byte[,] array4 = new byte[symbolRow2, symbolColumn2];
		for (int m = 0; m < symbolRow2; m++)
		{
			for (int n = 0; n < symbolColumn2; n++)
			{
				array4[symbolRow2 - 1 - m, n] = array3[n, m];
			}
		}
		AddQuiteZone(array4);
	}

	private void ErrorCorrectingCode200Placement(int[] array, int numRow, int numColumn)
	{
		int i;
		int j;
		for (i = 0; i < numRow; i++)
		{
			for (j = 0; j < numColumn; j++)
			{
				array[i * numColumn + j] = 0;
			}
		}
		int num = 1;
		i = 4;
		j = 0;
		do
		{
			if (i == numRow && j == 0)
			{
				ErrorCorrectingCode200PlacementCornerA(array, numRow, numColumn, num++);
			}
			if (i == numRow - 2 && j == 0 && numColumn % 4 != 0)
			{
				ErrorCorrectingCode200PlacementCornerB(array, numRow, numColumn, num++);
			}
			if (i == numRow - 2 && j == 0 && numColumn % 8 == 4)
			{
				ErrorCorrectingCode200PlacementCornerC(array, numRow, numColumn, num++);
			}
			if (i == numRow + 4 && j == 2 && numColumn % 8 == 0)
			{
				ErrorCorrectingCode200PlacementCornerD(array, numRow, numColumn, num++);
			}
			do
			{
				if (i < numRow && j >= 0 && array[i * numColumn + j] == 0)
				{
					ErrorCorrectingCode200PlacementBlock(array, numRow, numColumn, i, j, num++);
				}
				i -= 2;
				j += 2;
			}
			while (i >= 0 && j < numColumn);
			i++;
			j += 3;
			do
			{
				if (i >= 0 && j < numColumn && array[i * numColumn + j] == 0)
				{
					ErrorCorrectingCode200PlacementBlock(array, numRow, numColumn, i, j, num++);
				}
				i += 2;
				j -= 2;
			}
			while (i < numRow && j >= 0);
			i += 3;
			j++;
		}
		while (i < numRow || j < numColumn);
		if (array[numRow * numColumn - 1] == 0)
		{
			array[numRow * numColumn - 1] = (array[numRow * numColumn - numColumn - 2] = 1);
		}
	}

	private void ErrorCorrectingCode200PlacementCornerA(int[] array, int numRow, int numColumn, int place)
	{
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 1, 0, place, '\a');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 1, 1, place, '\u0006');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 1, 2, place, '\u0005');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 2, place, '\u0004');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 1, place, '\u0003');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 1, numColumn - 1, place, '\u0002');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 2, numColumn - 1, place, '\u0001');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 3, numColumn - 1, place, '\0');
	}

	private void ErrorCorrectingCode200PlacementCornerB(int[] array, int numRow, int numColumn, int place)
	{
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 3, 0, place, '\a');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 2, 0, place, '\u0006');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 1, 0, place, '\u0005');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 4, place, '\u0004');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 3, place, '\u0003');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 2, place, '\u0002');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 1, place, '\u0001');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 1, numColumn - 1, place, '\0');
	}

	private void ErrorCorrectingCode200PlacementCornerC(int[] array, int numRow, int numColumn, int place)
	{
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 3, 0, place, '\a');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 2, 0, place, '\u0006');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 1, 0, place, '\u0005');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 2, place, '\u0004');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 1, place, '\u0003');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 1, numColumn - 1, place, '\u0002');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 2, numColumn - 1, place, '\u0001');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 3, numColumn - 1, place, '\0');
	}

	private void ErrorCorrectingCode200PlacementCornerD(int[] array, int numRow, int numColumn, int place)
	{
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 1, 0, place, '\a');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, numRow - 1, numColumn - 1, place, '\u0006');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 3, place, '\u0005');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 2, place, '\u0004');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 0, numColumn - 1, place, '\u0003');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 1, numColumn - 3, place, '\u0002');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 1, numColumn - 2, place, '\u0001');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, 1, numColumn - 1, place, '\0');
	}

	private void ErrorCorrectingCode200PlacementBlock(int[] array, int numRow, int numColumn, int row, int column, int place)
	{
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, row - 2, column - 2, place, '\a');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, row - 2, column - 1, place, '\u0006');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, row - 1, column - 2, place, '\u0005');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, row - 1, column - 1, place, '\u0004');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, row - 1, column, place, '\u0003');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, row, column - 2, place, '\u0002');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, row, column - 1, place, '\u0001');
		ErrorCorrectingCode200PlacementBit(array, numRow, numColumn, row, column, place, '\0');
	}

	private void ErrorCorrectingCode200PlacementBit(int[] array, int numRow, int numColumn, int row, int column, int place, char character)
	{
		if (row < 0)
		{
			row += numRow;
			column += 4 - (numRow + 4) % 8;
		}
		if (column < 0)
		{
			column += numColumn;
			row += 4 - (numColumn + 4) % 8;
		}
		array[row * numColumn + column] = (place << 3) + character;
	}

	internal void BuildDataMatrix()
	{
		int[] codeword = PrepareCodeword(GetData());
		CreateMatrix(codeword);
	}

	private int[] PrepareCodeword(byte[] dataCodeword)
	{
		byte[] codeword = PrepareDataCodeword(dataCodeword);
		int[] array = ComputeErrorCorrection(ref codeword);
		int[] array2 = new int[codeword.Length + array.Length];
		codeword.CopyTo(array2, 0);
		array.CopyTo(array2, codeword.Length);
		return array2;
	}

	private byte[] DataMatrixBaseEncoder(byte[] dataCodeword)
	{
		int num = 1;
		if (dataCodeword.Length > 249)
		{
			num++;
		}
		byte[] array = new byte[1 + num + dataCodeword.Length];
		array[0] = 231;
		if (dataCodeword.Length <= 249)
		{
			array[1] = (byte)dataCodeword.Length;
		}
		else
		{
			array[1] = (byte)(dataCodeword.Length / 250 + 249);
			array[2] = (byte)(dataCodeword.Length % 250);
		}
		Array.Copy(dataCodeword, 0, array, 1 + num, dataCodeword.Length);
		for (int i = 1; i < array.Length; i++)
		{
			array[i] = ComputeBase256Codeword(array[i], i);
		}
		return array;
	}

	private byte ComputeBase256Codeword(int codeWordValue, int index)
	{
		int num = 149 * (index + 1) % 255 + 1;
		int num2 = codeWordValue + num;
		if (num2 <= 255)
		{
			return (byte)num2;
		}
		return (byte)(num2 - 256);
	}

	private byte[] DataMatrixASCIINumericEncoder(byte[] dataCodeword)
	{
		byte[] array = dataCodeword;
		bool flag = true;
		if (array.Length % 2 == 1)
		{
			flag = false;
			array = new byte[dataCodeword.Length + 1];
			Array.Copy(dataCodeword, 0, array, 0, dataCodeword.Length);
		}
		byte[] array2 = new byte[array.Length / 2];
		for (int i = 0; i < array2.Length; i++)
		{
			if (!flag && i == array2.Length - 1)
			{
				array2[i] = (byte)(array[2 * i] + 1);
			}
			else
			{
				array2[i] = (byte)((array[2 * i] - 48) * 10 + (array[2 * i + 1] - 48) + 130);
			}
		}
		return array2;
	}

	private byte[] DataMatrixASCIIEncoder(byte[] dataCodeword)
	{
		byte[] array = new byte[dataCodeword.Length];
		int num = 0;
		for (int i = 0; i < dataCodeword.Length; i++)
		{
			if (dataCodeword[i] >= 48 && dataCodeword[i] <= 57)
			{
				int num2 = 0;
				if (i != 0)
				{
					num2 = num - 1;
				}
				byte b = (byte)(array[num2] - 1);
				byte b2 = 0;
				if (i != 0 && num != 1)
				{
					b2 = array[num2 - 1];
				}
				if (b2 != 235 && b >= 48 && b <= 57)
				{
					array[num2] = (byte)(10 * (b - 48) + (dataCodeword[i] - 48) + 130);
				}
				else
				{
					array[num++] = (byte)(dataCodeword[i] + 1);
				}
			}
			else if (dataCodeword[i] < 127)
			{
				array[num++] = (byte)(dataCodeword[i] + 1);
			}
			else
			{
				array[num] = 235;
				array[num++] = (byte)(dataCodeword[i] - 127);
			}
		}
		byte[] array2 = new byte[num];
		Array.Copy(array, array2, num);
		return array2;
	}

	private int[] ComputeErrorCorrection(ref byte[] codeword)
	{
		int num = codeword.Length;
		symbolAttribute = default(PdfDataMatrixSymbolAttribute);
		if (Size == PdfDataMatrixSize.Auto)
		{
			PdfDataMatrixSymbolAttribute[] array = symbolAttributes;
			for (int i = 0; i < array.Length; i++)
			{
				PdfDataMatrixSymbolAttribute pdfDataMatrixSymbolAttribute = array[i];
				if (pdfDataMatrixSymbolAttribute.DataCodewords >= num)
				{
					symbolAttribute = pdfDataMatrixSymbolAttribute;
					break;
				}
			}
		}
		else
		{
			symbolAttribute = symbolAttributes[(int)(Size - 1)];
		}
		if (symbolAttribute.DataCodewords > num)
		{
			PadCodewords(symbolAttribute.DataCodewords, codeword, out byte[] codeword2);
			codeword = new byte[codeword2.Length];
			codeword2.CopyTo(codeword, 0);
			num = codeword.Length;
		}
		else
		{
			if (symbolAttribute.DataCodewords == 0)
			{
				throw new BarcodeException("Data cannot be encoded as barcode");
			}
			if (symbolAttribute.DataCodewords < num)
			{
				string arg = symbolAttribute.SymbolRow.ToString();
				string arg2 = symbolAttribute.SymbolColumn.ToString();
				throw new BarcodeException($"Data too long for {arg}x{arg2} barcode.");
			}
		}
		int correctionCodewords = symbolAttribute.CorrectionCodewords;
		int[] array2 = new int[correctionCodewords + symbolAttribute.DataCodewords];
		int interleavedBlock = symbolAttribute.InterleavedBlock;
		int dataCodewords = symbolAttribute.DataCodewords;
		int num2 = symbolAttribute.CorrectionCodewords / interleavedBlock;
		int num3 = dataCodewords + num2 * interleavedBlock;
		CreatePolynomial(interleavedBlock);
		blockLength = 68;
		byte[] array3 = new byte[blockLength];
		for (int j = 0; j < interleavedBlock; j++)
		{
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k] = 0;
			}
			for (int l = j; l < dataCodewords; l += interleavedBlock)
			{
				int j2 = ErrorCorrectionCodeSum(array3[num2 - 1], codeword[l]);
				for (int num4 = num2 - 1; num4 > 0; num4--)
				{
					array3[num4] = ErrorCorrectionCodeSum(array3[num4 - 1], ErrorCorrectionCodeProduct(polynomial[num4], j2));
				}
				array3[0] = ErrorCorrectionCodeProduct(polynomial[0], j2);
			}
			int num5 = 0;
			num5 = ((j < 8 || Size != PdfDataMatrixSize.Size144x144) ? symbolAttribute.InterleavedDataBlock : (symbolAttribute.DataCodewords / interleavedBlock));
			int num6 = num2;
			for (int m = j + interleavedBlock * num5; m < num3; m += interleavedBlock)
			{
				array2[m] = array3[--num6];
			}
			if (num6 != 0)
			{
				throw new Exception("Error in error correction code generation!");
			}
		}
		if (array2.Length > correctionCodewords)
		{
			int[] array4 = array2;
			array2 = new int[correctionCodewords];
			int num7 = 0;
			for (int num8 = array4.Length; num8 > symbolAttribute.DataCodewords; num8--)
			{
				array2[num7++] = array4[num8 - 1];
			}
		}
		Array.Reverse(array2);
		return array2;
	}

	private byte ErrorCorrectionCodeDoublify(byte i, int j)
	{
		if (i == 0)
		{
			return 0;
		}
		if (j == 0)
		{
			return i;
		}
		return (byte)aLog[(log[i] + j) % 255];
	}

	private byte ErrorCorrectionCodeProduct(byte i, int j)
	{
		if (i == 0 || j == 0)
		{
			return 0;
		}
		return (byte)aLog[(log[i] + log[j]) % 255];
	}

	private static byte ErrorCorrectionCodeSum(byte i, byte j)
	{
		return (byte)(i ^ j);
	}

	private void PadCodewords(int dataCodeWordLength, byte[] temp, out byte[] codeword)
	{
		int num = temp.Length;
		using MemoryStream memoryStream = new MemoryStream();
		for (int i = 0; i < num; i++)
		{
			memoryStream.WriteByte(temp[i]);
		}
		if (num < dataCodeWordLength)
		{
			memoryStream.WriteByte(129);
		}
		for (num = (int)memoryStream.Length; num < dataCodeWordLength; num = (int)memoryStream.Length)
		{
			int num2 = 129 + (num + 1) * 149 % 253 + 1;
			if (num2 > 254)
			{
				num2 -= 254;
			}
			memoryStream.WriteByte((byte)num2);
		}
		codeword = new byte[memoryStream.Length];
		Array.Copy(memoryStream.ToArray(), codeword, num);
	}

	private byte[] PrepareDataCodeword(byte[] dataCodeword)
	{
		if (Encoding == PdfDataMatrixEncoding.Auto || Encoding == PdfDataMatrixEncoding.ASCIINumeric)
		{
			bool flag = true;
			bool flag2 = false;
			int num = 0;
			PdfDataMatrixEncoding pdfDataMatrixEncoding = PdfDataMatrixEncoding.ASCII;
			for (int i = 0; i < dataCodeword.Length; i++)
			{
				if (dataCodeword[i] < 48 || dataCodeword[i] > 57)
				{
					flag = false;
				}
				else if (dataCodeword[i] > 127)
				{
					num++;
					if (num > 3)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (flag)
			{
				pdfDataMatrixEncoding = PdfDataMatrixEncoding.ASCIINumeric;
			}
			if (flag2)
			{
				pdfDataMatrixEncoding = PdfDataMatrixEncoding.Base256;
			}
			if (Encoding == PdfDataMatrixEncoding.ASCIINumeric && Encoding != pdfDataMatrixEncoding)
			{
				throw new BarcodeException("Data contains invalid characters and cannot be encoded as ASCIINumeric.");
			}
			Encoding = pdfDataMatrixEncoding;
		}
		byte[] result = null;
		switch (Encoding)
		{
		case PdfDataMatrixEncoding.ASCII:
			result = DataMatrixASCIIEncoder(dataCodeword);
			break;
		case PdfDataMatrixEncoding.ASCIINumeric:
			result = DataMatrixASCIINumericEncoder(dataCodeword);
			break;
		case PdfDataMatrixEncoding.Base256:
			result = DataMatrixBaseEncoder(dataCodeword);
			break;
		}
		return result;
	}

	private void AddQuiteZone(byte[,] dataMatrix)
	{
		int quiteZone = GetQuiteZone();
		int actualRows = ActualRows;
		int actualColumns = ActualColumns;
		dataMatrixArray = new byte[actualRows, actualColumns];
		for (int i = 0; i < actualColumns; i++)
		{
			dataMatrixArray[0, i] = 0;
		}
		for (int j = quiteZone; j < actualRows - quiteZone; j++)
		{
			dataMatrixArray[j, 0] = 0;
			for (int k = quiteZone; k < actualColumns - quiteZone; k++)
			{
				dataMatrixArray[j, k] = dataMatrix[j - quiteZone, k - quiteZone];
			}
			dataMatrixArray[j, actualColumns - quiteZone] = 0;
		}
		for (int l = 0; l < actualColumns; l++)
		{
			dataMatrixArray[actualRows - quiteZone, l] = 0;
		}
	}

	private int GetQuiteZone()
	{
		int result = 1;
		if (base.QuietZone.IsAll && base.QuietZone.All > 0f)
		{
			m_quiteZoneLeft = (m_quiteZoneRight = (m_quiteZoneTop = (m_quiteZoneBottom = (int)base.QuietZone.All)));
			result = (int)base.QuietZone.All;
		}
		return result;
	}

	private PdfDataMatrixSize FindDataMatrixSize(int width, int height)
	{
		switch (width)
		{
		case 8:
			switch (height)
			{
			case 18:
				return PdfDataMatrixSize.Size8x18;
			case 32:
				return PdfDataMatrixSize.Size8x32;
			}
			break;
		case 10:
			return PdfDataMatrixSize.Size10x10;
		case 14:
			return PdfDataMatrixSize.Size14x14;
		case 12:
			switch (height)
			{
			case 26:
				return PdfDataMatrixSize.Size12x26;
			case 36:
				return PdfDataMatrixSize.Size12x36;
			case 12:
				return PdfDataMatrixSize.Size12x12;
			}
			break;
		case 16:
			switch (height)
			{
			case 16:
				return PdfDataMatrixSize.Size16x16;
			case 36:
				return PdfDataMatrixSize.Size16x36;
			case 48:
				return PdfDataMatrixSize.Size16x48;
			}
			break;
		case 18:
			return PdfDataMatrixSize.Size18x18;
		case 20:
			return PdfDataMatrixSize.Size20x20;
		case 22:
			return PdfDataMatrixSize.Size22x22;
		case 24:
			return PdfDataMatrixSize.Size24x24;
		case 26:
			return PdfDataMatrixSize.Size26x26;
		case 32:
			return PdfDataMatrixSize.Size32x32;
		case 36:
			return PdfDataMatrixSize.Size36x36;
		case 40:
			return PdfDataMatrixSize.Size40x40;
		case 44:
			return PdfDataMatrixSize.Size44x44;
		case 48:
			return PdfDataMatrixSize.Size48x48;
		case 52:
			return PdfDataMatrixSize.Size52x52;
		case 64:
			return PdfDataMatrixSize.Size64x64;
		case 72:
			return PdfDataMatrixSize.Size72x72;
		case 80:
			return PdfDataMatrixSize.Size80x80;
		case 88:
			return PdfDataMatrixSize.Size88x88;
		case 96:
			return PdfDataMatrixSize.Size96x96;
		case 104:
			return PdfDataMatrixSize.Size104x104;
		case 120:
			return PdfDataMatrixSize.Size120x120;
		case 132:
			return PdfDataMatrixSize.Size132x132;
		case 144:
			return PdfDataMatrixSize.Size144x144;
		}
		return PdfDataMatrixSize.Auto;
	}

	public override void Draw(PdfGraphics graphics)
	{
		Draw(graphics, base.Location);
	}

	public override void Draw(PdfGraphics graphics, PointF location)
	{
		BuildDataMatrix();
		PdfBrush pdfBrush = PdfBrushes.Black;
		PdfBrush pdfBrush2 = PdfBrushes.White;
		float x = location.X;
		float y = location.Y;
		int actualRows = ActualRows;
		int actualColumns = ActualColumns;
		y = location.Y + (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Top > 0) ? ((int)base.QuietZone.Top) : 0);
		float num = actualColumns;
		float num2 = actualRows;
		PdfTemplate pdfTemplate = null;
		if (autoTagcheck)
		{
			num = (float)actualColumns * base.XDimension;
			num2 = (float)actualRows * base.XDimension;
			pdfTemplate = new PdfTemplate(new SizeF(num, num2));
		}
		base.Size = new SizeF((int)num, (int)num2);
		size = FindDataMatrixSize((int)num, (int)num2);
		for (int i = 0; i < actualRows; i++)
		{
			x = location.X + (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Left > 0) ? ((int)base.QuietZone.Left) : 0);
			for (int j = 0; j < actualColumns; j++)
			{
				PdfBrush pdfBrush3 = null;
				if (base.BackColor.A != 0)
				{
					Color color = Color.FromArgb(base.BackColor.ToArgb());
					if (color != Color.White)
					{
						pdfBrush2 = new PdfSolidBrush(color);
					}
				}
				if (base.ForeColor.A != 0)
				{
					Color color2 = Color.FromArgb(base.ForeColor.ToArgb());
					if (color2 != Color.Black)
					{
						pdfBrush = new PdfSolidBrush(color2);
					}
				}
				pdfBrush3 = ((dataMatrixArray[i, j] != 1) ? pdfBrush2 : pdfBrush);
				if (autoTagcheck)
				{
					pdfTemplate.Graphics.DrawRectangle(pdfBrush3, x, y, base.XDimension, base.XDimension);
				}
				else
				{
					graphics.DrawRectangle(pdfBrush3, x, y, base.XDimension, base.XDimension);
				}
				x += base.XDimension;
			}
			y += base.XDimension;
		}
		if (autoTagcheck)
		{
			graphics.DrawPdfTemplate(pdfTemplate, location);
		}
	}

	public void Draw(PdfGraphics graphics, PointF location, SizeF Size)
	{
		BuildDataMatrix();
		PdfBrush pdfBrush = PdfBrushes.Black;
		PdfBrush pdfBrush2 = PdfBrushes.White;
		float x = location.X;
		float num = location.Y;
		float num2 = 0f;
		float num3 = 0f;
		int actualRows = ActualRows;
		int actualColumns = ActualColumns;
		base.Size = new SizeF(actualColumns, actualRows);
		size = FindDataMatrixSize(actualColumns, actualRows);
		num2 = Size.Width / (float)actualRows + base.XDimension;
		num3 = Size.Height / (float)actualColumns + base.XDimension;
		PdfTemplate pdfTemplate = null;
		if (autoTagcheck)
		{
			pdfTemplate = new PdfTemplate(Size);
		}
		for (int i = 0; i < actualRows; i++)
		{
			x = location.X;
			for (int j = 0; j < actualColumns; j++)
			{
				PdfBrush pdfBrush3 = null;
				if (base.BackColor.A != 0)
				{
					Color color = Color.FromArgb(base.BackColor.ToArgb());
					if (color != Color.White)
					{
						pdfBrush2 = new PdfSolidBrush(color);
					}
				}
				if (base.ForeColor.A != 0)
				{
					Color color2 = Color.FromArgb(base.ForeColor.ToArgb());
					if (color2 != Color.Black)
					{
						pdfBrush = new PdfSolidBrush(color2);
					}
				}
				pdfBrush3 = ((dataMatrixArray[i, j] != 1) ? pdfBrush2 : pdfBrush);
				if (autoTagcheck)
				{
					pdfTemplate.Graphics.DrawRectangle(pdfBrush3, x, num, num2, num3);
				}
				else
				{
					graphics.DrawRectangle(pdfBrush3, x, num, num2, num3);
				}
				x += num2;
			}
			num += num3;
		}
		if (autoTagcheck)
		{
			graphics.DrawPdfTemplate(pdfTemplate, location);
		}
	}

	public void Draw(PdfGraphics graphics, RectangleF Rectangle)
	{
		BuildDataMatrix();
		PdfBrush pdfBrush = PdfBrushes.Black;
		PdfBrush pdfBrush2 = PdfBrushes.White;
		float x = Rectangle.X;
		float num = Rectangle.Y;
		float num2 = 0f;
		float num3 = 0f;
		int actualRows = ActualRows;
		int actualColumns = ActualColumns;
		base.Size = new SizeF(actualColumns, actualRows);
		size = FindDataMatrixSize(actualColumns, actualRows);
		num2 = Rectangle.Width / (float)actualRows + base.XDimension;
		num3 = Rectangle.Height / (float)actualColumns + base.XDimension;
		PdfTemplate pdfTemplate = null;
		if (autoTagcheck)
		{
			pdfTemplate = new PdfTemplate(new SizeF(Rectangle.Width, Rectangle.Height));
		}
		for (int i = 0; i < actualRows; i++)
		{
			x = Rectangle.X;
			for (int j = 0; j < actualColumns; j++)
			{
				PdfBrush pdfBrush3 = null;
				if (base.BackColor.A != 0)
				{
					Color color = Color.FromArgb(base.BackColor.ToArgb());
					if (color != Color.White)
					{
						pdfBrush2 = new PdfSolidBrush(color);
					}
				}
				if (base.ForeColor.A != 0)
				{
					Color color2 = Color.FromArgb(base.ForeColor.ToArgb());
					if (color2 != Color.Black)
					{
						pdfBrush = new PdfSolidBrush(color2);
					}
				}
				pdfBrush3 = ((dataMatrixArray[i, j] != 1) ? pdfBrush2 : pdfBrush);
				if (autoTagcheck)
				{
					pdfTemplate.Graphics.DrawRectangle(pdfBrush3, x, num, num2, num3);
				}
				else
				{
					graphics.DrawRectangle(pdfBrush3, x, num, num2, num3);
				}
				x += num2;
			}
			num += num3;
		}
		if (autoTagcheck)
		{
			graphics.DrawPdfTemplate(pdfTemplate, new PointF(Rectangle.X, Rectangle.Y));
		}
	}

	public void Draw(PdfGraphics graphics, float x, float y, float Width, float Height)
	{
		BuildDataMatrix();
		PdfBrush pdfBrush = PdfBrushes.Black;
		PdfBrush pdfBrush2 = PdfBrushes.White;
		float num = x;
		float num2 = y;
		float num3 = 0f;
		float num4 = 0f;
		int actualRows = ActualRows;
		int actualColumns = ActualColumns;
		base.Size = new SizeF(actualColumns, actualRows);
		size = FindDataMatrixSize(actualColumns, actualRows);
		num3 = Width / (float)actualRows + base.XDimension;
		num4 = Height / (float)actualColumns + base.XDimension;
		num2 = y + (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Top > 0) ? ((int)base.QuietZone.Top) : 0);
		PdfTemplate pdfTemplate = null;
		if (autoTagcheck)
		{
			pdfTemplate = new PdfTemplate(new SizeF(Width, Height));
		}
		for (int i = 0; i < actualRows; i++)
		{
			num = x + (float)((!base.QuietZone.IsAll && (int)base.QuietZone.Left > 0) ? ((int)base.QuietZone.Left) : 0);
			for (int j = 0; j < actualColumns; j++)
			{
				PdfBrush pdfBrush3 = null;
				if (base.BackColor.A != 0)
				{
					Color color = Color.FromArgb(base.BackColor.ToArgb());
					if (color != Color.White)
					{
						pdfBrush2 = new PdfSolidBrush(color);
					}
				}
				if (base.ForeColor.A != 0)
				{
					Color color2 = Color.FromArgb(base.ForeColor.ToArgb());
					if (color2 != Color.Black)
					{
						pdfBrush = new PdfSolidBrush(color2);
					}
				}
				pdfBrush3 = ((dataMatrixArray[i, j] != 1) ? pdfBrush2 : pdfBrush);
				if (autoTagcheck)
				{
					pdfTemplate.Graphics.DrawRectangle(pdfBrush3, num, num2, num3, num4);
				}
				else
				{
					graphics.DrawRectangle(pdfBrush3, num, num2, num3, num4);
				}
				num += num3;
			}
			num2 += num4;
		}
		if (autoTagcheck)
		{
			graphics.DrawPdfTemplate(pdfTemplate, new PointF(x, y));
		}
	}

	public override void Draw(PdfPageBase page, PointF location)
	{
		if (page is PdfPage && (page as PdfPage).Document != null)
		{
			autoTagcheck = (page as PdfPage).Document.AutoTag;
		}
		Draw(page.Graphics, location);
	}

	public void Draw(PdfPageBase page, PointF location, SizeF size)
	{
		if (page is PdfPage && (page as PdfPage).Document != null)
		{
			autoTagcheck = (page as PdfPage).Document.AutoTag;
		}
		Draw(page.Graphics, location.X, location.Y, size.Width, size.Height);
	}

	public void Draw(PdfPageBase page, RectangleF rectangle)
	{
		if (page is PdfPage && (page as PdfPage).Document != null)
		{
			autoTagcheck = (page as PdfPage).Document.AutoTag;
		}
		Draw(page.Graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void Draw(PdfPageBase page, float x, float y, float width, float height)
	{
		if (page is PdfPage && (page as PdfPage).Document != null)
		{
			autoTagcheck = (page as PdfPage).Document.AutoTag;
		}
		Draw(page.Graphics, x, y, width, height);
	}

	public override void Draw(PdfPageBase page)
	{
		Draw(page, base.Location);
	}
}
