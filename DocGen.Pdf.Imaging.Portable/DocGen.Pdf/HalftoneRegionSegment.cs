namespace DocGen.Pdf;

internal class HalftoneRegionSegment : JBIG2BaseSegment
{
	private HalftoneRegionFlags m_halftoneRegionFlags = new HalftoneRegionFlags();

	private BitOperation m_bitOperation = new BitOperation();

	private bool inlineImage;

	internal HalftoneRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage)
		: base(streamDecoder)
	{
		this.inlineImage = inlineImage;
	}

	public override void readSegment()
	{
		base.readSegment();
		ReadHalftoneRegionFlags();
		short[] array = new short[4];
		m_decoder.ReadByte(array);
		int @int = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		int int2 = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		int int3 = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		int int4 = m_bitOperation.GetInt32(array);
		array = new short[2];
		m_decoder.ReadByte(array);
		int int5 = m_bitOperation.GetInt16(array);
		array = new short[2];
		m_decoder.ReadByte(array);
		int int6 = m_bitOperation.GetInt16(array);
		int[] referredToSegments = m_segmentHeader.ReferredToSegments;
		PatternDictionarySegment patternDictionarySegment = (PatternDictionarySegment)m_decoder.FindSegment(referredToSegments[0]);
		int num = 0;
		int num2;
		for (num2 = 1; num2 < patternDictionarySegment.Size; num2 <<= 1)
		{
			num++;
		}
		JBIG2Image jBIG2Image = patternDictionarySegment.GetBitmaps()[0];
		int width = jBIG2Image.Width;
		int height = jBIG2Image.Height;
		bool flag = m_halftoneRegionFlags.GetFlagValue("H_MMR") != 0;
		int flagValue = m_halftoneRegionFlags.GetFlagValue("H_TEMPLATE");
		if (!flag)
		{
			m_arithmeticDecoder.ResetGenericStats(flagValue, null);
			m_arithmeticDecoder.Start();
		}
		int flagValue2 = m_halftoneRegionFlags.GetFlagValue("H_DEF_PIXEL");
		jBIG2Image = new JBIG2Image(regionBitmapWidth, regionBitmapHeight, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
		jBIG2Image.Clear(flagValue2);
		bool flag2 = m_halftoneRegionFlags.GetFlagValue("H_ENABLE_SKIP") != 0;
		JBIG2Image jBIG2Image2 = null;
		if (flag2)
		{
			jBIG2Image2 = new JBIG2Image(@int, int2, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
			jBIG2Image2.Clear(0);
			for (int i = 0; i < int2; i++)
			{
				for (int j = 0; j < @int; j++)
				{
					int num3 = int3 + i * int6 + j * int5;
					int num4 = int4 + i * int5 - j * int6;
					if (num3 + width >> 8 <= 0 || num3 >> 8 >= regionBitmapWidth || num4 + height >> 8 <= 0 || num4 >> 8 >= regionBitmapHeight)
					{
						jBIG2Image2.SetPixel(i, j, 1);
					}
				}
			}
		}
		int[] array2 = new int[@int * int2];
		short[] array3 = new short[4];
		short[] array4 = new short[4];
		array3[0] = (short)((flagValue <= 1) ? 3 : 2);
		array4[0] = -1;
		array3[1] = -3;
		array4[1] = -1;
		array3[2] = 2;
		array4[2] = -2;
		array3[3] = -2;
		array4[3] = -2;
		for (int num5 = num - 1; num5 >= 0; num5--)
		{
			JBIG2Image jBIG2Image3 = new JBIG2Image(@int, int2, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
			jBIG2Image3.ReadBitmap(flag, flagValue, typicalPredictionGenericDecodingOn: false, flag2, jBIG2Image2, array3, array4, -1);
			num2 = 0;
			for (int k = 0; k < int2; k++)
			{
				for (int l = 0; l < @int; l++)
				{
					int num6 = jBIG2Image3.GetPixel(l, k) ^ (array2[num2] & 1);
					array2[num2] = (array2[num2] << 1) | num6;
					num2++;
				}
			}
		}
		int flagValue3 = m_halftoneRegionFlags.GetFlagValue("H_COMB_OP");
		num2 = 0;
		for (int m = 0; m < int2; m++)
		{
			int num7 = int3 + m * int6;
			int num8 = int4 + m * int5;
			for (int n = 0; n < @int; n++)
			{
				if (!flag2 || jBIG2Image2.GetPixel(m, n) != 1)
				{
					JBIG2Image bitmap = patternDictionarySegment.GetBitmaps()[array2[num2]];
					jBIG2Image.Combine(bitmap, num7 >> 8, num8 >> 8, flagValue3);
				}
				num7 += int5;
				num8 -= int6;
				num2++;
			}
		}
		if (inlineImage)
		{
			m_decoder.FindPageSegement(m_segmentHeader.PageAssociation).pageBitmap.Combine(combOp: regionFlags.GetFlagValue("EXTERNAL_COMBINATION_OPERATOR"), bitmap: jBIG2Image, x: regionBitmapXLocation, y: regionBitmapYLocation);
			return;
		}
		jBIG2Image.BitmapNumber = m_segmentHeader.SegmentNumber;
		m_decoder.appendBitmap(jBIG2Image);
	}

	private void ReadHalftoneRegionFlags()
	{
		short flags = m_decoder.ReadByte();
		m_halftoneRegionFlags.setFlags(flags);
	}
}
