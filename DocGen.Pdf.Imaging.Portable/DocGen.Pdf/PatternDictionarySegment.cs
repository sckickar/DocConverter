namespace DocGen.Pdf;

internal class PatternDictionarySegment : JBIG2Segment
{
	private PatternDictionaryFlags m_patternDictionaryFlags = new PatternDictionaryFlags();

	private int m_width;

	private int m_height;

	private int m_grayMax;

	private JBIG2Image[] m_bitmaps;

	private int m_size;

	private BitOperation m_bitOperation = new BitOperation();

	internal int Size => m_size;

	public PatternDictionarySegment(JBIG2StreamDecoder streamDecoder)
		: base(streamDecoder)
	{
	}

	public override void readSegment()
	{
		ReadPatternDictionaryFlags();
		m_width = m_decoder.ReadByte();
		m_height = m_decoder.ReadByte();
		short[] array = new short[4];
		m_decoder.ReadByte(array);
		m_grayMax = m_bitOperation.GetInt32(array);
		bool flag = m_patternDictionaryFlags.GetFlagValue("HD_MMR") == 1;
		int flagValue = m_patternDictionaryFlags.GetFlagValue("HD_TEMPLATE");
		if (!flag)
		{
			m_arithmeticDecoder.ResetGenericStats(flagValue, null);
			m_arithmeticDecoder.Start();
		}
		short[] array2 = new short[4];
		short[] array3 = new short[4];
		array2[0] = (short)(-m_width);
		array3[0] = 0;
		array2[1] = -3;
		array3[1] = -1;
		array2[2] = 2;
		array3[2] = -2;
		array2[3] = -2;
		array3[3] = -2;
		m_size = m_grayMax + 1;
		JBIG2Image jBIG2Image = new JBIG2Image(m_size * m_width, m_height, m_arithmeticDecoder, m_huffmanDecoder, m_mmrDecoder);
		jBIG2Image.Clear(0);
		jBIG2Image.ReadBitmap(flag, flagValue, typicalPredictionGenericDecodingOn: false, useSkip: false, null, array2, array3, m_segmentHeader.DataLength - 7);
		JBIG2Image[] array4 = new JBIG2Image[m_size];
		int num = 0;
		for (int i = 0; i < m_size; i++)
		{
			array4[i] = jBIG2Image.GetSlice(num, 0, m_width, m_height);
			num += m_width;
		}
		m_bitmaps = array4;
	}

	internal JBIG2Image[] GetBitmaps()
	{
		return m_bitmaps;
	}

	private void ReadPatternDictionaryFlags()
	{
		short flags = m_decoder.ReadByte();
		m_patternDictionaryFlags.setFlags(flags);
	}
}
