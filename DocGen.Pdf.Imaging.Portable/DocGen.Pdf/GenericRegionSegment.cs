namespace DocGen.Pdf;

internal class GenericRegionSegment : JBIG2BaseSegment
{
	private GenericRegionFlags m_genericRegionFlags = new GenericRegionFlags();

	private bool m_inlineImage;

	private bool m_unknownLength;

	public GenericRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage)
		: base(streamDecoder)
	{
		m_inlineImage = inlineImage;
	}

	public override void readSegment()
	{
		base.readSegment();
		ReadGenericRegionFlags();
		bool flag = m_genericRegionFlags.GetFlagValue("MMR") != 0;
		int flagValue = m_genericRegionFlags.GetFlagValue("GB_TEMPLATE");
		short[] array = new short[4];
		short[] array2 = new short[4];
		if (!flag)
		{
			if (flagValue == 0)
			{
				array[0] = ReadAtValue();
				array2[0] = ReadAtValue();
				array[1] = ReadAtValue();
				array2[1] = ReadAtValue();
				array[2] = ReadAtValue();
				array2[2] = ReadAtValue();
				array[3] = ReadAtValue();
				array2[3] = ReadAtValue();
			}
			else
			{
				array[0] = ReadAtValue();
				array2[0] = ReadAtValue();
			}
			m_arithmeticDecoder.ResetGenericStats(flagValue, null);
			m_arithmeticDecoder.Start();
		}
		bool typicalPredictionGenericDecodingOn = m_genericRegionFlags.GetFlagValue("TPGDON") != 0;
		int num = m_segmentHeader.DataLength;
		if (num == -1)
		{
			m_unknownLength = true;
			short num2;
			short num3;
			if (flag)
			{
				num2 = 0;
				num3 = 0;
			}
			else
			{
				num2 = 255;
				num3 = 172;
			}
			int num4 = 0;
			while (true)
			{
				short num5 = m_decoder.ReadByte();
				num4++;
				if (num5 == num2)
				{
					short num6 = m_decoder.ReadByte();
					num4++;
					if (num6 == num3)
					{
						break;
					}
				}
			}
			num = num4 - 2;
			m_decoder.MovePointer(-num4);
		}
		JBIG2Image jBIG2Image = new JBIG2Image(regionBitmapWidth, regionBitmapHeight, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
		jBIG2Image.Clear(0);
		jBIG2Image.ReadBitmap(flag, flagValue, typicalPredictionGenericDecodingOn, useSkip: false, null, array, array2, (!flag) ? (num - 18) : 0);
		if (m_inlineImage)
		{
			PageInformationSegment pageInformationSegment = m_decoder.FindPageSegement(m_segmentHeader.PageAssociation);
			JBIG2Image pageBitmap = pageInformationSegment.pageBitmap;
			int flagValue2 = regionFlags.GetFlagValue("EXTERNAL_COMBINATION_OPERATOR");
			if (pageInformationSegment.pageBitmapHeight == -1 && regionBitmapYLocation + regionBitmapHeight > pageBitmap.Height)
			{
				pageBitmap.Expand(regionBitmapYLocation + regionBitmapHeight, pageInformationSegment.pageInformationFlags.GetFlagValue("DEFAULT_PIXEL_VALUE"));
			}
			pageBitmap.Combine(jBIG2Image, regionBitmapXLocation, regionBitmapYLocation, flagValue2);
		}
		else
		{
			jBIG2Image.BitmapNumber = m_segmentHeader.SegmentNumber;
			m_decoder.appendBitmap(jBIG2Image);
		}
		if (m_unknownLength)
		{
			m_decoder.MovePointer(4);
		}
	}

	private void ReadGenericRegionFlags()
	{
		short flags = m_decoder.ReadByte();
		m_genericRegionFlags.setFlags(flags);
	}
}
