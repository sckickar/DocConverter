using System.Collections;

namespace DocGen.Pdf;

internal class JBIG2Image
{
	private int m_width;

	private int m_height;

	private int m_line;

	private BitArray data;

	private ArithmeticDecoder m_arithmeticDecoder;

	private HuffmanDecoder m_huffmanDecoder;

	private MMRDecoder m_mmrDecoder;

	private BitOperation m_bitOperation = new BitOperation();

	internal int BitmapNumber;

	internal int Width => m_width;

	internal int Height => m_height;

	internal JBIG2Image(int width, int height, ArithmeticDecoder arithmeticDecoder, HuffmanDecoder huffmanDecoder, MMRDecoder mmrDecoder)
	{
		m_width = width;
		m_height = height;
		m_arithmeticDecoder = arithmeticDecoder;
		m_huffmanDecoder = huffmanDecoder;
		m_mmrDecoder = mmrDecoder;
		m_line = width + 7 >> 3;
		data = new BitArray(width * height);
	}

	internal void ReadBitmap(bool useMMR, int template, bool typicalPredictionGenericDecodingOn, bool useSkip, JBIG2Image skipBitmap, short[] adaptiveTemplateX, short[] adaptiveTemplateY, int mmrDataLength)
	{
		if (useMMR)
		{
			m_mmrDecoder.Reset();
			int[] array = new int[m_width + 2];
			int[] array2 = new int[m_width + 2];
			array2[0] = (array2[1] = m_width);
			for (int i = 0; i < m_height; i++)
			{
				int j;
				for (j = 0; array2[j] < m_width; j++)
				{
					array[j] = array2[j];
				}
				array[j] = (array[j + 1] = m_width);
				int k = 0;
				int num = 0;
				int num2 = 0;
				do
				{
					switch (m_mmrDecoder.Get2DCode())
					{
					case 0:
						if (array[k] < m_width)
						{
							num2 = array[k + 1];
							k += 2;
						}
						break;
					case 1:
					{
						int num3;
						int num5;
						if (((uint)num & (true ? 1u : 0u)) != 0)
						{
							num3 = 0;
							int num4;
							do
							{
								num3 += (num4 = m_mmrDecoder.GetblackCode());
							}
							while (num4 >= 64);
							num5 = 0;
							do
							{
								num5 += (num4 = m_mmrDecoder.GetWhiteCode());
							}
							while (num4 >= 64);
						}
						else
						{
							num3 = 0;
							int num4;
							do
							{
								num3 += (num4 = m_mmrDecoder.GetWhiteCode());
							}
							while (num4 >= 64);
							num5 = 0;
							do
							{
								num5 += (num4 = m_mmrDecoder.GetblackCode());
							}
							while (num4 >= 64);
						}
						if (num3 > 0 || num5 > 0)
						{
							num2 = (array2[num++] = num2 + num3);
							for (num2 = (array2[num++] = num2 + num5); array[k] <= num2 && array[k] < m_width; k += 2)
							{
							}
						}
						break;
					}
					case 2:
						num2 = (array2[num++] = array[k]);
						if (array[k] < m_width)
						{
							k++;
						}
						break;
					case 3:
						num2 = (array2[num++] = array[k] + 1);
						if (array[k] < m_width)
						{
							for (k++; array[k] <= num2 && array[k] < m_width; k += 2)
							{
							}
						}
						break;
					case 5:
						num2 = (array2[num++] = array[k] + 2);
						if (array[k] < m_width)
						{
							for (k++; array[k] <= num2 && array[k] < m_width; k += 2)
							{
							}
						}
						break;
					case 7:
						num2 = (array2[num++] = array[k] + 3);
						if (array[k] < m_width)
						{
							for (k++; array[k] <= num2 && array[k] < m_width; k += 2)
							{
							}
						}
						break;
					case 4:
						try
						{
							num2 = (array2[num++] = array[k] - 1);
							for (k = ((k <= 0) ? (k + 1) : (k - 1)); array[k] <= num2 && array[k] < m_width; k += 2)
							{
							}
						}
						catch
						{
						}
						break;
					case 6:
						num2 = (array2[num++] = array[k] - 2);
						for (k = ((k <= 0) ? (k + 1) : (k - 1)); array[k] <= num2 && array[k] < m_width; k += 2)
						{
						}
						break;
					case 8:
						num2 = (array2[num++] = array[k] - 3);
						for (k = ((k <= 0) ? (k + 1) : (k - 1)); array[k] <= num2 && array[k] < m_width; k += 2)
						{
						}
						break;
					}
				}
				while (num2 < m_width);
				array2[num++] = m_width;
				for (int l = 0; array2[l] < m_width; l += 2)
				{
					for (int m = array2[l]; m < array2[l + 1]; m++)
					{
						SetPixel(m, i, 1);
					}
				}
			}
			if (mmrDataLength >= 0)
			{
				m_mmrDecoder.SkipTo(mmrDataLength);
			}
			else
			{
				m_mmrDecoder.Get24Bits();
			}
			return;
		}
		ImagePointer imagePointer = new ImagePointer(this);
		ImagePointer imagePointer2 = new ImagePointer(this);
		ImagePointer imagePointer3 = new ImagePointer(this);
		ImagePointer imagePointer4 = new ImagePointer(this);
		ImagePointer imagePointer5 = new ImagePointer(this);
		ImagePointer imagePointer6 = new ImagePointer(this);
		long context = 0L;
		if (typicalPredictionGenericDecodingOn)
		{
			switch (template)
			{
			case 0:
				context = 14675L;
				break;
			case 1:
				context = 1946L;
				break;
			case 2:
				context = 227L;
				break;
			case 3:
				context = 394L;
				break;
			}
		}
		bool flag = false;
		for (int n = 0; n < m_height; n++)
		{
			if (typicalPredictionGenericDecodingOn)
			{
				if (m_arithmeticDecoder.DecodeBit(context, m_arithmeticDecoder.GenericRegionStats) != 0)
				{
					flag = !flag;
				}
				if (flag)
				{
					DuplicateRow(n, n - 1);
					continue;
				}
			}
			switch (template)
			{
			case 0:
			{
				imagePointer.SetPointer(0, n - 2);
				long number3 = imagePointer.NextPixel();
				number3 = m_bitOperation.Bit32Shift(number3, 1, 0) | imagePointer.NextPixel();
				imagePointer2.SetPointer(0, n - 1);
				long number = imagePointer2.NextPixel();
				number = m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel();
				number = m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel();
				long number2 = 0L;
				imagePointer3.SetPointer(adaptiveTemplateX[0], n + adaptiveTemplateY[0]);
				imagePointer4.SetPointer(adaptiveTemplateX[1], n + adaptiveTemplateY[1]);
				imagePointer5.SetPointer(adaptiveTemplateX[2], n + adaptiveTemplateY[2]);
				imagePointer6.SetPointer(adaptiveTemplateX[3], n + adaptiveTemplateY[3]);
				for (int num10 = 0; num10 < m_width; num10++)
				{
					long context2 = m_bitOperation.Bit32Shift(number3, 13, 0) | m_bitOperation.Bit32Shift(number, 8, 0) | m_bitOperation.Bit32Shift(number2, 4, 0) | (imagePointer3.NextPixel() << 3) | (imagePointer4.NextPixel() << 2) | (imagePointer5.NextPixel() << 1) | imagePointer6.NextPixel();
					int num7;
					if (useSkip && skipBitmap.GetPixel(num10, n) != 0)
					{
						num7 = 0;
					}
					else
					{
						num7 = m_arithmeticDecoder.DecodeBit(context2, m_arithmeticDecoder.GenericRegionStats);
						if (num7 != 0)
						{
							SetPixel(num10, n, 1);
						}
					}
					number3 = (m_bitOperation.Bit32Shift(number3, 1, 0) | imagePointer.NextPixel()) & 7;
					number = (m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel()) & 0x1F;
					number2 = (m_bitOperation.Bit32Shift(number2, 1, 0) | num7) & 0xF;
				}
				break;
			}
			case 1:
			{
				imagePointer.SetPointer(0, n - 2);
				long number3 = imagePointer.NextPixel();
				number3 = m_bitOperation.Bit32Shift(number3, 1, 0) | imagePointer.NextPixel();
				number3 = m_bitOperation.Bit32Shift(number3, 1, 0) | imagePointer.NextPixel();
				imagePointer2.SetPointer(0, n - 1);
				long number = imagePointer2.NextPixel();
				number = m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel();
				number = m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel();
				long number2 = 0L;
				imagePointer3.SetPointer(adaptiveTemplateX[0], n + adaptiveTemplateY[0]);
				for (int num8 = 0; num8 < m_width; num8++)
				{
					long context2 = m_bitOperation.Bit32Shift(number3, 9, 0) | m_bitOperation.Bit32Shift(number, 4, 0) | m_bitOperation.Bit32Shift(number2, 1, 0) | imagePointer3.NextPixel();
					int num7;
					if (useSkip && skipBitmap.GetPixel(num8, n) != 0)
					{
						num7 = 0;
					}
					else
					{
						num7 = m_arithmeticDecoder.DecodeBit(context2, m_arithmeticDecoder.GenericRegionStats);
						if (num7 != 0)
						{
							SetPixel(num8, n, 1);
						}
					}
					number3 = (m_bitOperation.Bit32Shift(number3, 1, 0) | imagePointer.NextPixel()) & 0xF;
					number = (m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel()) & 0x1F;
					number2 = (m_bitOperation.Bit32Shift(number2, 1, 0) | num7) & 7;
				}
				break;
			}
			case 2:
			{
				imagePointer.SetPointer(0, n - 2);
				long number3 = imagePointer.NextPixel();
				number3 = m_bitOperation.Bit32Shift(number3, 1, 0) | imagePointer.NextPixel();
				imagePointer2.SetPointer(0, n - 1);
				long number = imagePointer2.NextPixel();
				number = m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel();
				long number2 = 0L;
				imagePointer3.SetPointer(adaptiveTemplateX[0], n + adaptiveTemplateY[0]);
				for (int num9 = 0; num9 < m_width; num9++)
				{
					long context2 = m_bitOperation.Bit32Shift(number3, 7, 0) | m_bitOperation.Bit32Shift(number, 3, 0) | m_bitOperation.Bit32Shift(number2, 1, 0) | imagePointer3.NextPixel();
					int num7;
					if (useSkip && skipBitmap.GetPixel(num9, n) != 0)
					{
						num7 = 0;
					}
					else
					{
						num7 = m_arithmeticDecoder.DecodeBit(context2, m_arithmeticDecoder.GenericRegionStats);
						if (num7 != 0)
						{
							SetPixel(num9, n, 1);
						}
					}
					number3 = (m_bitOperation.Bit32Shift(number3, 1, 0) | imagePointer.NextPixel()) & 7;
					number = (m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel()) & 0xF;
					number2 = (m_bitOperation.Bit32Shift(number2, 1, 0) | num7) & 3;
				}
				break;
			}
			case 3:
			{
				imagePointer2.SetPointer(0, n - 1);
				long number = imagePointer2.NextPixel();
				number = m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel();
				long number2 = 0L;
				imagePointer3.SetPointer(adaptiveTemplateX[0], n + adaptiveTemplateY[0]);
				for (int num6 = 0; num6 < m_width; num6++)
				{
					long context2 = m_bitOperation.Bit32Shift(number, 5, 0) | m_bitOperation.Bit32Shift(number2, 1, 0) | imagePointer3.NextPixel();
					int num7;
					if (useSkip && skipBitmap.GetPixel(num6, n) != 0)
					{
						num7 = 0;
					}
					else
					{
						num7 = m_arithmeticDecoder.DecodeBit(context2, m_arithmeticDecoder.GenericRegionStats);
						if (num7 != 0)
						{
							SetPixel(num6, n, 1);
						}
					}
					number = (m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer2.NextPixel()) & 0x1F;
					number2 = (m_bitOperation.Bit32Shift(number2, 1, 0) | num7) & 0xF;
				}
				break;
			}
			}
		}
	}

	internal void ReadGenericRefinementRegion(int template, bool typicalPredictionGenericRefinementOn, JBIG2Image referredToBitmap, int referenceDX, int referenceDY, short[] adaptiveTemplateX, short[] adaptiveTemplateY)
	{
		long context;
		ImagePointer imagePointer;
		ImagePointer imagePointer2;
		ImagePointer imagePointer3;
		ImagePointer imagePointer4;
		ImagePointer imagePointer5;
		ImagePointer imagePointer6;
		ImagePointer imagePointer7;
		ImagePointer imagePointer8;
		ImagePointer imagePointer9;
		ImagePointer imagePointer10;
		if (template != 0)
		{
			context = 8L;
			imagePointer = new ImagePointer(this);
			imagePointer2 = new ImagePointer(this);
			imagePointer3 = new ImagePointer(referredToBitmap);
			imagePointer4 = new ImagePointer(referredToBitmap);
			imagePointer5 = new ImagePointer(referredToBitmap);
			imagePointer6 = new ImagePointer(this);
			imagePointer7 = new ImagePointer(this);
			imagePointer8 = new ImagePointer(referredToBitmap);
			imagePointer9 = new ImagePointer(referredToBitmap);
			imagePointer10 = new ImagePointer(referredToBitmap);
		}
		else
		{
			context = 16L;
			imagePointer = new ImagePointer(this);
			imagePointer2 = new ImagePointer(this);
			imagePointer3 = new ImagePointer(referredToBitmap);
			imagePointer4 = new ImagePointer(referredToBitmap);
			imagePointer5 = new ImagePointer(referredToBitmap);
			imagePointer6 = new ImagePointer(this);
			imagePointer7 = new ImagePointer(referredToBitmap);
			imagePointer8 = new ImagePointer(referredToBitmap);
			imagePointer9 = new ImagePointer(referredToBitmap);
			imagePointer10 = new ImagePointer(referredToBitmap);
		}
		bool flag = false;
		for (int i = 0; i < m_height; i++)
		{
			long number;
			long number2;
			long num;
			long num4;
			long num3;
			long num2;
			if (template != 0)
			{
				imagePointer.SetPointer(0, i - 1);
				number = imagePointer.NextPixel();
				imagePointer2.SetPointer(-1, i);
				imagePointer3.SetPointer(-referenceDX, i - 1 - referenceDY);
				imagePointer4.SetPointer(-1 - referenceDX, i - referenceDY);
				number2 = imagePointer4.NextPixel();
				number2 = m_bitOperation.Bit32Shift(number2, 1, 0) | imagePointer4.NextPixel();
				imagePointer5.SetPointer(-referenceDX, i + 1 - referenceDY);
				num = imagePointer5.NextPixel();
				num4 = (num3 = (num2 = 0L));
				if (typicalPredictionGenericRefinementOn)
				{
					imagePointer8.SetPointer(-1 - referenceDX, i - 1 - referenceDY);
					num4 = imagePointer8.NextPixel();
					num4 = m_bitOperation.Bit32Shift(num4, 1, 0) | imagePointer8.NextPixel();
					num4 = m_bitOperation.Bit32Shift(num4, 1, 0) | imagePointer8.NextPixel();
					imagePointer9.SetPointer(-1 - referenceDX, i - referenceDY);
					num3 = imagePointer9.NextPixel();
					num3 = m_bitOperation.Bit32Shift(num3, 1, 0) | imagePointer9.NextPixel();
					num3 = m_bitOperation.Bit32Shift(num3, 1, 0) | imagePointer9.NextPixel();
					imagePointer10.SetPointer(-1 - referenceDX, i + 1 - referenceDY);
					num2 = imagePointer10.NextPixel();
					num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | imagePointer10.NextPixel();
					num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | imagePointer10.NextPixel();
				}
				for (int j = 0; j < m_width; j++)
				{
					number = (m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer.NextPixel()) & 7;
					number2 = (m_bitOperation.Bit32Shift(number2, 1, 0) | imagePointer4.NextPixel()) & 7;
					num = (m_bitOperation.Bit32Shift(num, 1, 0) | imagePointer5.NextPixel()) & 3;
					if (typicalPredictionGenericRefinementOn)
					{
						num4 = (m_bitOperation.Bit32Shift(num4, 1, 0) | imagePointer8.NextPixel()) & 7;
						num3 = (m_bitOperation.Bit32Shift(num3, 1, 0) | imagePointer9.NextPixel()) & 7;
						num2 = (m_bitOperation.Bit32Shift(num2, 1, 0) | imagePointer10.NextPixel()) & 7;
						if (m_arithmeticDecoder.DecodeBit(context, m_arithmeticDecoder.RefinementRegionStats) != 0)
						{
							flag = !flag;
						}
						if (num4 == 0L && num3 == 0L && num2 == 0L)
						{
							SetPixel(j, i, 0);
							continue;
						}
						if (num4 == 7 && num3 == 7 && num2 == 7)
						{
							SetPixel(j, i, 1);
							continue;
						}
					}
					long context2 = m_bitOperation.Bit32Shift(number, 7, 0) | (imagePointer2.NextPixel() << 6) | (imagePointer3.NextPixel() << 5) | m_bitOperation.Bit32Shift(number2, 2, 0) | num;
					if (m_arithmeticDecoder.DecodeBit(context2, m_arithmeticDecoder.RefinementRegionStats) == 1)
					{
						SetPixel(j, i, 1);
					}
				}
				continue;
			}
			imagePointer.SetPointer(0, i - 1);
			number = imagePointer.NextPixel();
			imagePointer2.SetPointer(-1, i);
			imagePointer3.SetPointer(-referenceDX, i - 1 - referenceDY);
			long number3 = imagePointer3.NextPixel();
			imagePointer4.SetPointer(-1 - referenceDX, i - referenceDY);
			number2 = imagePointer4.NextPixel();
			number2 = m_bitOperation.Bit32Shift(number2, 1, 0) | imagePointer4.NextPixel();
			imagePointer5.SetPointer(-1 - referenceDX, i + 1 - referenceDY);
			num = imagePointer5.NextPixel();
			num = m_bitOperation.Bit32Shift(num, 1, 0) | imagePointer5.NextPixel();
			imagePointer6.SetPointer(adaptiveTemplateX[0], i + adaptiveTemplateY[0]);
			imagePointer7.SetPointer(adaptiveTemplateX[1] - referenceDX, i + adaptiveTemplateY[1] - referenceDY);
			num4 = (num3 = (num2 = 0L));
			if (typicalPredictionGenericRefinementOn)
			{
				imagePointer8.SetPointer(-1 - referenceDX, i - 1 - referenceDY);
				num4 = imagePointer8.NextPixel();
				num4 = m_bitOperation.Bit32Shift(num4, 1, 0) | imagePointer8.NextPixel();
				num4 = m_bitOperation.Bit32Shift(num4, 1, 0) | imagePointer8.NextPixel();
				imagePointer9.SetPointer(-1 - referenceDX, i - referenceDY);
				num3 = imagePointer9.NextPixel();
				num3 = m_bitOperation.Bit32Shift(num3, 1, 0) | imagePointer9.NextPixel();
				num3 = m_bitOperation.Bit32Shift(num3, 1, 0) | imagePointer9.NextPixel();
				imagePointer10.SetPointer(-1 - referenceDX, i + 1 - referenceDY);
				num2 = imagePointer10.NextPixel();
				num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | imagePointer10.NextPixel();
				num2 = m_bitOperation.Bit32Shift(num2, 1, 0) | imagePointer10.NextPixel();
			}
			for (int k = 0; k < m_width; k++)
			{
				number = (m_bitOperation.Bit32Shift(number, 1, 0) | imagePointer.NextPixel()) & 3;
				number3 = (m_bitOperation.Bit32Shift(number3, 1, 0) | imagePointer3.NextPixel()) & 3;
				number2 = (m_bitOperation.Bit32Shift(number2, 1, 0) | imagePointer4.NextPixel()) & 7;
				num = (m_bitOperation.Bit32Shift(num, 1, 0) | imagePointer5.NextPixel()) & 7;
				if (typicalPredictionGenericRefinementOn)
				{
					num4 = (m_bitOperation.Bit32Shift(num4, 1, 0) | imagePointer8.NextPixel()) & 7;
					num3 = (m_bitOperation.Bit32Shift(num3, 1, 0) | imagePointer9.NextPixel()) & 7;
					num2 = (m_bitOperation.Bit32Shift(num2, 1, 0) | imagePointer10.NextPixel()) & 7;
					if (m_arithmeticDecoder.DecodeBit(context, m_arithmeticDecoder.RefinementRegionStats) == 1)
					{
						flag = !flag;
					}
					if (num4 == 0L && num3 == 0L && num2 == 0L)
					{
						SetPixel(k, i, 0);
						continue;
					}
					if (num4 == 7 && num3 == 7 && num2 == 7)
					{
						SetPixel(k, i, 1);
						continue;
					}
				}
				long context2 = m_bitOperation.Bit32Shift(number, 11, 0) | (imagePointer2.NextPixel() << 10) | m_bitOperation.Bit32Shift(number3, 8, 0) | m_bitOperation.Bit32Shift(number2, 5, 0) | m_bitOperation.Bit32Shift(num, 2, 0) | (imagePointer6.NextPixel() << 1) | imagePointer7.NextPixel();
				if (m_arithmeticDecoder.DecodeBit(context2, m_arithmeticDecoder.RefinementRegionStats) == 1)
				{
					SetPixel(k, i, 1);
				}
			}
		}
	}

	internal void ReadTextRegion(bool huffman, bool symbolRefine, int noOfSymbolInstances, int logStrips, int noOfSymbols, int[][] symbolCodeTable, int symbolCodeLength, JBIG2Image[] symbols, int defaultPixel, int combinationOperator, bool transposed, int referenceCorner, int sOffset, int[][] huffmanFSTable, int[][] huffmanDSTable, int[][] huffmanDTTable, int[][] huffmanRDWTable, int[][] huffmanRDHTable, int[][] huffmanRDXTable, int[][] huffmanRDYTable, int[][] huffmanRSizeTable, int template, short[] symbolRegionAdaptiveTemplateX, short[] symbolRegionAdaptiveTemplateY, JBIG2StreamDecoder decoder)
	{
		int num = 1 << logStrips;
		Clear(defaultPixel);
		int num2 = ((!huffman) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IadtStats).IntResult : m_huffmanDecoder.DecodeInt(huffmanDTTable).IntResult);
		num2 *= -num;
		int num3 = 0;
		int num4 = 0;
		while (num3 < noOfSymbolInstances)
		{
			int num5 = ((!huffman) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IadtStats).IntResult : m_huffmanDecoder.DecodeInt(huffmanDTTable).IntResult);
			num2 += num5 * num;
			int num6 = ((!huffman) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IafsStats).IntResult : m_huffmanDecoder.DecodeInt(huffmanFSTable).IntResult);
			num4 += num6;
			int num7 = num4;
			while (true)
			{
				num5 = ((num != 1) ? ((!huffman) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IaitStats).IntResult : decoder.ReadBits(logStrips)) : 0);
				int num8 = num2 + num5;
				long num9 = ((!huffman) ? m_arithmeticDecoder.DecodeIAID(symbolCodeLength, m_arithmeticDecoder.IaidStats) : ((symbolCodeTable == null) ? decoder.ReadBits(symbolCodeLength) : m_huffmanDecoder.DecodeInt(symbolCodeTable).IntResult));
				if (num9 < noOfSymbols)
				{
					JBIG2Image jBIG2Image = null;
					if (symbolRefine && ((!huffman) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IariStats).IntResult : decoder.ReadBit()) != 0)
					{
						int intResult;
						int intResult2;
						int intResult3;
						int intResult4;
						if (huffman)
						{
							intResult = m_huffmanDecoder.DecodeInt(huffmanRDWTable).IntResult;
							intResult2 = m_huffmanDecoder.DecodeInt(huffmanRDHTable).IntResult;
							intResult3 = m_huffmanDecoder.DecodeInt(huffmanRDXTable).IntResult;
							intResult4 = m_huffmanDecoder.DecodeInt(huffmanRDYTable).IntResult;
							decoder.ConsumeRemainingBits();
							m_arithmeticDecoder.Start();
						}
						else
						{
							intResult = m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IardwStats).IntResult;
							intResult2 = m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IardhStats).IntResult;
							intResult3 = m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IardxStats).IntResult;
							intResult4 = m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IardyStats).IntResult;
						}
						intResult3 = ((intResult >= 0) ? intResult : (intResult - 1)) / 2 + intResult3;
						intResult4 = ((intResult2 >= 0) ? intResult2 : (intResult2 - 1)) / 2 + intResult4;
						jBIG2Image = new JBIG2Image(intResult + symbols[(int)num9].m_width, intResult2 + symbols[(int)num9].m_height, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
						jBIG2Image.ReadGenericRefinementRegion(template, typicalPredictionGenericRefinementOn: false, symbols[(int)num9], intResult3, intResult4, symbolRegionAdaptiveTemplateX, symbolRegionAdaptiveTemplateY);
					}
					else
					{
						jBIG2Image = symbols[(int)num9];
					}
					int num10 = jBIG2Image.m_width - 1;
					int num11 = jBIG2Image.m_height - 1;
					if (transposed)
					{
						switch (referenceCorner)
						{
						case 0:
							Combine(jBIG2Image, num8, num7, combinationOperator);
							break;
						case 1:
							Combine(jBIG2Image, num8, num7, combinationOperator);
							break;
						case 2:
							Combine(jBIG2Image, num8 - num10, num7, combinationOperator);
							break;
						case 3:
							Combine(jBIG2Image, num8 - num10, num7, combinationOperator);
							break;
						}
						num7 += num11;
					}
					else
					{
						switch (referenceCorner)
						{
						case 0:
							Combine(jBIG2Image, num7, num8 - num11, combinationOperator);
							break;
						case 1:
							Combine(jBIG2Image, num7, num8, combinationOperator);
							break;
						case 2:
							Combine(jBIG2Image, num7, num8 - num11, combinationOperator);
							break;
						case 3:
							Combine(jBIG2Image, num7, num8, combinationOperator);
							break;
						}
						num7 += num10;
					}
				}
				num3++;
				DecodeIntResult decodeIntResult = ((!huffman) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IadsStats) : m_huffmanDecoder.DecodeInt(huffmanDSTable));
				if (!decodeIntResult.BooleanResult)
				{
					break;
				}
				num6 = decodeIntResult.IntResult;
				num7 += sOffset + num6;
			}
		}
	}

	internal void Clear(int defPixel)
	{
		data.Set(0, defPixel == 1);
	}

	internal void Combine(JBIG2Image bitmap, int x, int y, long combOp)
	{
		int width = bitmap.m_width;
		int height = bitmap.m_height;
		int num = 0;
		int num2 = 0;
		for (int i = y; i < y + height; i++)
		{
			if (y + height > m_height)
			{
				break;
			}
			for (int j = x; j < x + width; j++)
			{
				int pixel = bitmap.GetPixel(num2, num);
				switch ((int)combOp)
				{
				case 0:
					SetPixel(j, i, GetPixel(j, i) | pixel);
					break;
				case 1:
					SetPixel(j, i, GetPixel(j, i) & pixel);
					break;
				case 2:
					SetPixel(j, i, GetPixel(j, i) ^ pixel);
					break;
				case 3:
					if ((GetPixel(j, i) == 1 && pixel == 1) || (GetPixel(j, i) == 0 && pixel == 0))
					{
						SetPixel(j, i, 1);
					}
					else
					{
						SetPixel(j, i, 0);
					}
					break;
				case 4:
					SetPixel(j, i, pixel);
					break;
				}
				num2++;
			}
			num2 = 0;
			num++;
		}
	}

	private void DuplicateRow(int yDest, int ySrc)
	{
		for (int i = 0; i < m_width; i++)
		{
			SetPixel(i, yDest, GetPixel(i, ySrc));
		}
	}

	internal byte[] GetData(bool switchPixelColor)
	{
		byte[] array = new byte[m_height * m_line];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < m_height; i++)
		{
			for (int j = 0; j < m_width; j++)
			{
				if (data.Get(num))
				{
					int num3 = (num + num2) / 8;
					int num4 = (num + num2) % 8;
					array[num3] |= (byte)(1 << 7 - num4);
				}
				num++;
			}
			num2 = m_line * 8 * (i + 1) - num;
		}
		if (switchPixelColor)
		{
			for (int k = 0; k < array.Length; k++)
			{
				array[k] ^= byte.MaxValue;
			}
		}
		return array;
	}

	internal JBIG2Image GetSlice(int x, int y, int width, int height)
	{
		JBIG2Image jBIG2Image = new JBIG2Image(width, height, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
		int num = 0;
		int num2 = 0;
		for (int i = y; i < height; i++)
		{
			for (int j = x; j < x + width; j++)
			{
				jBIG2Image.SetPixel(num2, num, GetPixel(j, i));
				num2++;
			}
			num2 = 0;
			num++;
		}
		return jBIG2Image;
	}

	private void SetPixel(int col, int row, BitArray data, int value)
	{
		int index = row * m_width + col;
		data.Set(index, value == 1);
	}

	internal void SetPixel(int col, int row, int value)
	{
		SetPixel(col, row, data, value);
	}

	internal int GetPixel(int col, int row)
	{
		try
		{
			return data.Get(row * m_width + col) ? 1 : 0;
		}
		catch
		{
			return 0;
		}
	}

	internal void Expand(int newHeight, int defaultPixel)
	{
		BitArray bitArray = new BitArray(newHeight * m_width);
		for (int i = 0; i < m_height; i++)
		{
			for (int j = 0; j < m_width; j++)
			{
				SetPixel(j, i, bitArray, GetPixel(j, i));
			}
		}
		m_height = newHeight;
		data = bitArray;
	}
}
