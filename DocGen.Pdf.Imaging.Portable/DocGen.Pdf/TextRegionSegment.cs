using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class TextRegionSegment : JBIG2BaseSegment
{
	private TextRegionFlags m_textRegionFlags = new TextRegionFlags();

	private TextRegionHuffmanFlags m_textRegionHuffmanFlags = new TextRegionHuffmanFlags();

	private int m_noOfSymbolInstances;

	private bool m_inlineImage;

	private short[] m_symbolRegionAdaptiveTemplateX = new short[2];

	private short[] symbolRegionAdaptiveTemplateY = new short[2];

	private HuffmanDecoder m_huffDecoder = new HuffmanDecoder();

	private BitOperation m_bitOperation = new BitOperation();

	private RectangularArrays m_rectangularArrays = new RectangularArrays();

	public TextRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage)
		: base(streamDecoder)
	{
		m_inlineImage = inlineImage;
	}

	public override void readSegment()
	{
		base.readSegment();
		ReadTextRegionFlags();
		short[] array = new short[4];
		m_decoder.ReadByte(array);
		m_noOfSymbolInstances = m_bitOperation.GetInt32(array);
		int referedToSegCount = m_segmentHeader.ReferedToSegCount;
		int[] referredToSegments = m_segmentHeader.ReferredToSegments;
		IList list = new List<object>();
		IList list2 = new List<object>();
		int num = 0;
		for (int i = 0; i < referedToSegCount; i++)
		{
			JBIG2Segment jBIG2Segment = m_decoder.FindSegment(referredToSegments[i]);
			if (jBIG2Segment != null)
			{
				switch (jBIG2Segment.m_segmentHeader.SegmentType)
				{
				case 0:
					list2.Add(jBIG2Segment);
					num += ((SymbolDictionarySegment)jBIG2Segment).NoOfExportedSymbols;
					break;
				case 53:
					list.Add(jBIG2Segment);
					break;
				}
			}
		}
		int num2 = 0;
		for (int num3 = 1; num3 < num; num3 <<= 1)
		{
			num2++;
		}
		int num4 = 0;
		JBIG2Image[] array2 = new JBIG2Image[num];
		IEnumerator enumerator = list2.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JBIG2Segment jBIG2Segment2 = (JBIG2Segment)enumerator.Current;
			if (jBIG2Segment2.m_segmentHeader.SegmentType == 0)
			{
				JBIG2Image[] bitmaps = ((SymbolDictionarySegment)jBIG2Segment2).getBitmaps();
				for (int j = 0; j < bitmaps.Length; j++)
				{
					array2[num4] = bitmaps[j];
					num4++;
				}
			}
		}
		int[][] huffmanFSTable = null;
		int[][] huffmanDSTable = null;
		int[][] huffmanDTTable = null;
		int[][] huffmanRDWTable = null;
		int[][] huffmanRDHTable = null;
		int[][] huffmanRDXTable = null;
		int[][] huffmanRDYTable = null;
		int[][] huffmanRSizeTable = null;
		bool flag = m_textRegionFlags.GetFlagValue("SB_HUFF") != 0;
		int num5 = 0;
		if (flag)
		{
			switch (m_textRegionHuffmanFlags.GetFlagValue("SB_HUFF_FS"))
			{
			case 0:
				huffmanFSTable = m_huffDecoder.huffmanTableF;
				break;
			case 1:
				huffmanFSTable = m_huffDecoder.huffmanTableG;
				break;
			}
			switch (m_textRegionHuffmanFlags.GetFlagValue("SB_HUFF_DS"))
			{
			case 0:
				huffmanDSTable = m_huffDecoder.huffmanTableH;
				break;
			case 1:
				huffmanDSTable = m_huffDecoder.huffmanTableI;
				break;
			case 2:
				huffmanDSTable = m_huffDecoder.huffmanTableJ;
				break;
			}
			switch (m_textRegionHuffmanFlags.GetFlagValue("SB_HUFF_DT"))
			{
			case 0:
				huffmanDTTable = m_huffDecoder.huffmanTableK;
				break;
			case 1:
				huffmanDTTable = m_huffDecoder.huffmanTableL;
				break;
			case 2:
				huffmanDTTable = m_huffDecoder.huffmanTableM;
				break;
			}
			switch (m_textRegionHuffmanFlags.GetFlagValue("SB_HUFF_RDW"))
			{
			case 0:
				huffmanRDWTable = m_huffDecoder.huffmanTableN;
				break;
			case 1:
				huffmanRDWTable = m_huffDecoder.huffmanTableO;
				break;
			}
			switch (m_textRegionHuffmanFlags.GetFlagValue("SB_HUFF_RDH"))
			{
			case 0:
				huffmanRDHTable = m_huffDecoder.huffmanTableN;
				break;
			case 1:
				huffmanRDHTable = m_huffDecoder.huffmanTableO;
				break;
			}
			switch (m_textRegionHuffmanFlags.GetFlagValue("SB_HUFF_RDX"))
			{
			case 0:
				huffmanRDXTable = m_huffDecoder.huffmanTableN;
				break;
			case 1:
				huffmanRDXTable = m_huffDecoder.huffmanTableO;
				break;
			}
			switch (m_textRegionHuffmanFlags.GetFlagValue("SB_HUFF_RDY"))
			{
			case 0:
				huffmanRDYTable = m_huffDecoder.huffmanTableN;
				break;
			case 1:
				huffmanRDYTable = m_huffDecoder.huffmanTableO;
				break;
			}
			if (m_textRegionHuffmanFlags.GetFlagValue("SB_HUFF_RSIZE") == 0)
			{
				huffmanRSizeTable = m_huffDecoder.huffmanTableA;
			}
		}
		int[][] array3 = m_rectangularArrays.ReturnRectangularIntArray(36, 4);
		int[][] array4 = m_rectangularArrays.ReturnRectangularIntArray(num + 1, 4);
		if (flag)
		{
			m_decoder.ConsumeRemainingBits();
			for (num5 = 0; num5 < 32; num5++)
			{
				array3[num5] = new int[4]
				{
					num5,
					m_decoder.ReadBits(4),
					0,
					0
				};
			}
			array3[32] = new int[4]
			{
				259,
				m_decoder.ReadBits(4),
				2,
				0
			};
			array3[33] = new int[4]
			{
				515,
				m_decoder.ReadBits(4),
				3,
				0
			};
			array3[34] = new int[4]
			{
				523,
				m_decoder.ReadBits(4),
				7,
				0
			};
			array3[35] = new int[3] { 0, 0, m_huffDecoder.jbig2HuffmanEOT };
			array3 = m_huffDecoder.BuildTable(array3, 35);
			for (num5 = 0; num5 < num; num5++)
			{
				array4[num5] = new int[4] { num5, 0, 0, 0 };
			}
			num5 = 0;
			while (num5 < num)
			{
				int intResult = m_huffmanDecoder.DecodeInt(array3).IntResult;
				if (intResult > 512)
				{
					intResult -= 512;
					while (intResult != 0 && num5 < num)
					{
						array4[num5++][1] = 0;
						intResult--;
					}
				}
				else if (intResult > 256)
				{
					intResult -= 256;
					while (intResult != 0 && num5 < num)
					{
						array4[num5][1] = array4[num5 - 1][1];
						num5++;
						intResult--;
					}
				}
				else
				{
					array4[num5++][1] = intResult;
				}
			}
			array4[num][1] = 0;
			array4[num][2] = m_huffDecoder.jbig2HuffmanEOT;
			array4 = m_huffDecoder.BuildTable(array4, num);
			m_decoder.ConsumeRemainingBits();
		}
		else
		{
			array4 = null;
			m_arithmeticDecoder.ResetIntegerStats(num2);
			m_arithmeticDecoder.Start();
		}
		bool flag2 = m_textRegionFlags.GetFlagValue("SB_REFINE") != 0;
		int flagValue = m_textRegionFlags.GetFlagValue("LOG_SB_STRIPES");
		int flagValue2 = m_textRegionFlags.GetFlagValue("SB_DEF_PIXEL");
		int flagValue3 = m_textRegionFlags.GetFlagValue("SB_COMB_OP");
		bool transposed = m_textRegionFlags.GetFlagValue("TRANSPOSED") != 0;
		int flagValue4 = m_textRegionFlags.GetFlagValue("REF_CORNER");
		int flagValue5 = m_textRegionFlags.GetFlagValue("SB_DS_OFFSET");
		int flagValue6 = m_textRegionFlags.GetFlagValue("SB_R_TEMPLATE");
		if (flag2)
		{
			m_arithmeticDecoder.ResetRefinementStats(flagValue6, null);
		}
		JBIG2Image jBIG2Image = new JBIG2Image(regionBitmapWidth, regionBitmapHeight, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
		jBIG2Image.ReadTextRegion(flag, flag2, m_noOfSymbolInstances, flagValue, num, array4, num2, array2, flagValue2, flagValue3, transposed, flagValue4, flagValue5, huffmanFSTable, huffmanDSTable, huffmanDTTable, huffmanRDWTable, huffmanRDHTable, huffmanRDXTable, huffmanRDYTable, huffmanRSizeTable, flagValue6, m_symbolRegionAdaptiveTemplateX, symbolRegionAdaptiveTemplateY, m_decoder);
		if (m_inlineImage)
		{
			m_decoder.FindPageSegement(m_segmentHeader.PageAssociation).pageBitmap.Combine(combOp: regionFlags.GetFlagValue("EXTERNAL_COMBINATION_OPERATOR"), bitmap: jBIG2Image, x: regionBitmapXLocation, y: regionBitmapYLocation);
		}
		else
		{
			jBIG2Image.BitmapNumber = m_segmentHeader.SegmentNumber;
			m_decoder.appendBitmap(jBIG2Image);
		}
		m_decoder.ConsumeRemainingBits();
	}

	private void ReadTextRegionFlags()
	{
		short[] array = new short[2];
		m_decoder.ReadByte(array);
		int @int = m_bitOperation.GetInt16(array);
		m_textRegionFlags.setFlags(@int);
		if (m_textRegionFlags.GetFlagValue("SB_HUFF") != 0)
		{
			short[] array2 = new short[2];
			m_decoder.ReadByte(array2);
			@int = m_bitOperation.GetInt16(array2);
			m_textRegionHuffmanFlags.setFlags(@int);
		}
		bool num = m_textRegionFlags.GetFlagValue("SB_REFINE") != 0;
		int flagValue = m_textRegionFlags.GetFlagValue("SB_R_TEMPLATE");
		if (num && flagValue == 0)
		{
			m_symbolRegionAdaptiveTemplateX[0] = ReadAtValue();
			symbolRegionAdaptiveTemplateY[0] = ReadAtValue();
			m_symbolRegionAdaptiveTemplateX[1] = ReadAtValue();
			symbolRegionAdaptiveTemplateY[1] = ReadAtValue();
		}
	}
}
