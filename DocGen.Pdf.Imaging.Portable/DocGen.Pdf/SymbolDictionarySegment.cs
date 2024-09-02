using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class SymbolDictionarySegment : JBIG2Segment
{
	private int m_noOfExportedSymbols;

	private int m_noOfNewSymbols;

	private JBIG2Image[] bitmaps;

	private SymbolDictionaryFlags m_symbolDictionaryFlags = new SymbolDictionaryFlags();

	private ArithmeticDecoderStats m_genericRegionStats;

	private ArithmeticDecoderStats m_refinementRegionStats;

	private HuffmanDecoder m_huffDecoder = new HuffmanDecoder();

	private BitOperation m_bitOperation = new BitOperation();

	private RectangularArrays m_rectangularArrays = new RectangularArrays();

	private short[] m_symbolDictionaryAdaptiveTemplateX = new short[4];

	private short[] m_symbolDictionaryAdaptiveTemplateY = new short[4];

	private short[] m_symbolDictionaryRAdaptiveTemplateX = new short[2];

	private short[] m_symbolDictionaryRAdaptiveTemplateY = new short[2];

	internal int NoOfExportedSymbols
	{
		get
		{
			return m_noOfExportedSymbols;
		}
		set
		{
			m_noOfExportedSymbols = value;
		}
	}

	public SymbolDictionarySegment(JBIG2StreamDecoder streamDecoder)
		: base(streamDecoder)
	{
	}

	public override void readSegment()
	{
		ReadSymbolDictionaryFlags();
		IList list = new List<object>();
		int num = 0;
		int referedToSegCount = m_segmentHeader.ReferedToSegCount;
		int[] referredToSegments = m_segmentHeader.ReferredToSegments;
		for (int i = 0; i < referedToSegCount; i++)
		{
			JBIG2Segment jBIG2Segment = m_decoder.FindSegment(referredToSegments[i]);
			switch (jBIG2Segment.m_segmentHeader.SegmentType)
			{
			case 0:
				num += ((SymbolDictionarySegment)jBIG2Segment).m_noOfExportedSymbols;
				break;
			case 53:
				list.Add(jBIG2Segment);
				break;
			}
		}
		int num2 = 0;
		int num3 = 1;
		while (num3 < num + m_noOfNewSymbols && num3 >= 0)
		{
			num2++;
			num3 <<= 1;
		}
		JBIG2Image[] array = new JBIG2Image[num + m_noOfNewSymbols];
		int num4 = 0;
		SymbolDictionarySegment symbolDictionarySegment = null;
		for (num3 = 0; num3 < referedToSegCount; num3++)
		{
			JBIG2Segment jBIG2Segment2 = m_decoder.FindSegment(referredToSegments[num3]);
			if (jBIG2Segment2.m_segmentHeader.SegmentType == 0)
			{
				symbolDictionarySegment = (SymbolDictionarySegment)jBIG2Segment2;
				for (int j = 0; j < symbolDictionarySegment.m_noOfExportedSymbols; j++)
				{
					array[num4++] = symbolDictionarySegment.bitmaps[j];
				}
			}
		}
		int[][] table = null;
		int[][] table2 = null;
		int[][] table3 = null;
		int[][] table4 = null;
		bool flag = m_symbolDictionaryFlags.GetFlagValue("SD_HUFF") != 0;
		int flagValue = m_symbolDictionaryFlags.GetFlagValue("SD_HUFF_DH");
		int flagValue2 = m_symbolDictionaryFlags.GetFlagValue("SD_HUFF_DW");
		int flagValue3 = m_symbolDictionaryFlags.GetFlagValue("SD_HUFF_BM_SIZE");
		int flagValue4 = m_symbolDictionaryFlags.GetFlagValue("SD_HUFF_AGG_INST");
		num3 = 0;
		if (flag)
		{
			table = flagValue switch
			{
				0 => m_huffDecoder.huffmanTableD, 
				1 => m_huffDecoder.huffmanTableE, 
				_ => null, 
			};
			table2 = flagValue2 switch
			{
				0 => m_huffDecoder.huffmanTableB, 
				1 => m_huffDecoder.huffmanTableC, 
				_ => null, 
			};
			table3 = ((flagValue3 != 0) ? null : m_huffDecoder.huffmanTableA);
			table4 = ((flagValue4 != 0) ? null : m_huffDecoder.huffmanTableA);
		}
		int flagValue5 = m_symbolDictionaryFlags.GetFlagValue("BITMAP_CC_USED");
		int flagValue6 = m_symbolDictionaryFlags.GetFlagValue("SD_TEMPLATE");
		if (!flag)
		{
			if (flagValue5 != 0 && symbolDictionarySegment != null)
			{
				m_arithmeticDecoder.ResetGenericStats(flagValue6, symbolDictionarySegment.m_genericRegionStats);
			}
			else
			{
				m_arithmeticDecoder.ResetGenericStats(flagValue6, null);
			}
			m_arithmeticDecoder.ResetIntegerStats(num2);
			m_arithmeticDecoder.Start();
		}
		int flagValue7 = m_symbolDictionaryFlags.GetFlagValue("SD_REF_AGG");
		int flagValue8 = m_symbolDictionaryFlags.GetFlagValue("SD_R_TEMPLATE");
		if (flagValue7 != 0)
		{
			if (flagValue5 != 0 && symbolDictionarySegment != null)
			{
				m_arithmeticDecoder.ResetRefinementStats(flagValue8, symbolDictionarySegment.m_refinementRegionStats);
			}
			else
			{
				m_arithmeticDecoder.ResetRefinementStats(flagValue8, null);
			}
		}
		int[] array2 = new int[m_noOfNewSymbols];
		int num5 = 0;
		num3 = 0;
		while (num3 < m_noOfNewSymbols)
		{
			int num6 = 0;
			num6 = ((!flag) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IadhStats).IntResult : m_huffmanDecoder.DecodeInt(table).IntResult);
			num5 += num6;
			int num7 = 0;
			int num8 = 0;
			int k = num3;
			while (true)
			{
				int num9 = 0;
				DecodeIntResult decodeIntResult = ((!flag) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IadwStats) : m_huffmanDecoder.DecodeInt(table2));
				if (!decodeIntResult.BooleanResult)
				{
					break;
				}
				num9 = decodeIntResult.IntResult;
				num7 += num9;
				if (flag && flagValue7 == 0)
				{
					array2[num3] = num7;
					num8 += num7;
				}
				else if (flagValue7 == 1)
				{
					int num10 = 0;
					num10 = ((!flag) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IaaiStats).IntResult : m_huffmanDecoder.DecodeInt(table4).IntResult);
					if (num10 == 1)
					{
						int num11 = 0;
						int num12 = 0;
						int num13 = 0;
						if (flag)
						{
							num11 = m_decoder.ReadBits(num2);
							num12 = m_huffmanDecoder.DecodeInt(m_huffDecoder.huffmanTableO).IntResult;
							num13 = m_huffmanDecoder.DecodeInt(m_huffDecoder.huffmanTableO).IntResult;
							m_decoder.ConsumeRemainingBits();
							m_arithmeticDecoder.Start();
						}
						else
						{
							num11 = (int)m_arithmeticDecoder.DecodeIAID(num2, m_arithmeticDecoder.IaidStats);
							num12 = m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IardxStats).IntResult;
							num13 = m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IardyStats).IntResult;
						}
						JBIG2Image referredToBitmap = array[num11];
						JBIG2Image jBIG2Image = new JBIG2Image(num7, num5, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
						jBIG2Image.ReadGenericRefinementRegion(flagValue8, typicalPredictionGenericRefinementOn: false, referredToBitmap, num12, num13, m_symbolDictionaryRAdaptiveTemplateX, m_symbolDictionaryRAdaptiveTemplateY);
						array[num + num3] = jBIG2Image;
					}
					else
					{
						JBIG2Image jBIG2Image2 = new JBIG2Image(num7, num5, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
						jBIG2Image2.ReadTextRegion(flag, symbolRefine: true, num10, 0, num + num3, null, num2, array, 0, 0, transposed: false, 1, 0, m_huffDecoder.huffmanTableF, m_huffDecoder.huffmanTableH, m_huffDecoder.huffmanTableK, m_huffDecoder.huffmanTableO, m_huffDecoder.huffmanTableO, m_huffDecoder.huffmanTableO, m_huffDecoder.huffmanTableO, m_huffDecoder.huffmanTableA, flagValue8, m_symbolDictionaryRAdaptiveTemplateX, m_symbolDictionaryRAdaptiveTemplateY, m_decoder);
						array[num + num3] = jBIG2Image2;
					}
				}
				else
				{
					JBIG2Image jBIG2Image3 = new JBIG2Image(num7, num5, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
					jBIG2Image3.ReadBitmap(useMMR: false, flagValue6, typicalPredictionGenericDecodingOn: false, useSkip: false, null, m_symbolDictionaryAdaptiveTemplateX, m_symbolDictionaryAdaptiveTemplateY, 0);
					array[num + num3] = jBIG2Image3;
				}
				num3++;
			}
			if (!flag || flagValue7 != 0)
			{
				continue;
			}
			int intResult = m_huffmanDecoder.DecodeInt(table3).IntResult;
			m_decoder.ConsumeRemainingBits();
			JBIG2Image jBIG2Image4 = new JBIG2Image(num8, num5, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
			if (intResult == 0)
			{
				int num14 = num8 % 8;
				int num15 = (int)Math.Ceiling((double)num8 / 8.0);
				short[] array3 = new short[num5 * (num8 + 7 >> 3)];
				m_decoder.ReadByte(array3);
				short[][] array4 = m_rectangularArrays.ReturnRectangularShortArray(num5, num15);
				int num16 = 0;
				for (int l = 0; l < num5; l++)
				{
					for (int m = 0; m < num15; m++)
					{
						array4[l][m] = array3[num16];
						num16++;
					}
				}
				int num17 = 0;
				int num18 = 0;
				for (int n = 0; n < num5; n++)
				{
					for (int num19 = 0; num19 < num15; num19++)
					{
						if (num19 == num15 - 1)
						{
							short num20 = array4[n][num19];
							for (int num21 = 7; num21 >= num14; num21--)
							{
								short num22 = (short)(1 << num21);
								int value = (num20 & num22) >> num21;
								jBIG2Image4.SetPixel(num18, num17, value);
								num18++;
							}
							num17++;
							num18 = 0;
						}
						else
						{
							short num23 = array4[n][num19];
							for (int num24 = 7; num24 >= 0; num24--)
							{
								short num25 = (short)(1 << num24);
								int value2 = (num23 & num25) >> num24;
								jBIG2Image4.SetPixel(num18, num17, value2);
								num18++;
							}
						}
					}
				}
			}
			else
			{
				jBIG2Image4.ReadBitmap(useMMR: true, 0, typicalPredictionGenericDecodingOn: false, useSkip: false, null, null, null, intResult);
			}
			int num26 = 0;
			for (; k < num3; k++)
			{
				array[num + k] = jBIG2Image4.GetSlice(num26, 0, array2[k], num5);
				num26 += array2[k];
			}
		}
		bitmaps = new JBIG2Image[m_noOfExportedSymbols];
		int num27 = (num3 = 0);
		bool flag2 = false;
		while (num3 < num + m_noOfNewSymbols)
		{
			int num28 = 0;
			num28 = ((!flag) ? m_arithmeticDecoder.DecodeInt(m_arithmeticDecoder.IaexStats).IntResult : m_huffmanDecoder.DecodeInt(m_huffDecoder.huffmanTableA).IntResult);
			if (flag2)
			{
				for (int num29 = 0; num29 < num28; num29++)
				{
					bitmaps[num27++] = array[num3++];
				}
			}
			else
			{
				num3 += num28;
			}
			flag2 = !flag2;
		}
		int flagValue9 = m_symbolDictionaryFlags.GetFlagValue("BITMAP_CC_RETAINED");
		if (!flag && flagValue9 == 1)
		{
			m_genericRegionStats = m_genericRegionStats.copy();
			if (flagValue7 == 1)
			{
				m_refinementRegionStats = m_refinementRegionStats.copy();
			}
		}
		m_decoder.ConsumeRemainingBits();
	}

	private void ReadSymbolDictionaryFlags()
	{
		short[] array = new short[2];
		m_decoder.ReadByte(array);
		int @int = m_bitOperation.GetInt16(array);
		m_symbolDictionaryFlags.setFlags(@int);
		int flagValue = m_symbolDictionaryFlags.GetFlagValue("SD_HUFF");
		int flagValue2 = m_symbolDictionaryFlags.GetFlagValue("SD_TEMPLATE");
		if (flagValue == 0)
		{
			if (flagValue2 == 0)
			{
				m_symbolDictionaryAdaptiveTemplateX[0] = ReadAtValue();
				m_symbolDictionaryAdaptiveTemplateY[0] = ReadAtValue();
				m_symbolDictionaryAdaptiveTemplateX[1] = ReadAtValue();
				m_symbolDictionaryAdaptiveTemplateY[1] = ReadAtValue();
				m_symbolDictionaryAdaptiveTemplateX[2] = ReadAtValue();
				m_symbolDictionaryAdaptiveTemplateY[2] = ReadAtValue();
				m_symbolDictionaryAdaptiveTemplateX[3] = ReadAtValue();
				m_symbolDictionaryAdaptiveTemplateY[3] = ReadAtValue();
			}
			else
			{
				m_symbolDictionaryAdaptiveTemplateX[0] = ReadAtValue();
				m_symbolDictionaryAdaptiveTemplateY[0] = ReadAtValue();
			}
		}
		int flagValue3 = m_symbolDictionaryFlags.GetFlagValue("SD_REF_AGG");
		int flagValue4 = m_symbolDictionaryFlags.GetFlagValue("SD_R_TEMPLATE");
		if (flagValue3 != 0 && flagValue4 == 0)
		{
			m_symbolDictionaryRAdaptiveTemplateX[0] = ReadAtValue();
			m_symbolDictionaryRAdaptiveTemplateY[0] = ReadAtValue();
			m_symbolDictionaryRAdaptiveTemplateX[1] = ReadAtValue();
			m_symbolDictionaryRAdaptiveTemplateY[1] = ReadAtValue();
		}
		short[] array2 = new short[4];
		m_decoder.ReadByte(array2);
		int int2 = m_bitOperation.GetInt32(array2);
		m_noOfExportedSymbols = int2;
		short[] array3 = new short[4];
		m_decoder.ReadByte(array3);
		int int3 = m_bitOperation.GetInt32(array3);
		m_noOfNewSymbols = int3;
	}

	public JBIG2Image[] getBitmaps()
	{
		return bitmaps;
	}
}
