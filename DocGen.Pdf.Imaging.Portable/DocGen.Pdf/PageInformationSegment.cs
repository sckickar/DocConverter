namespace DocGen.Pdf;

internal class PageInformationSegment : JBIG2Segment
{
	private int m_pageBitmapHeight;

	private int m_pageBitmapWidth;

	private int m_yResolution;

	private int m_xResolution;

	private BitOperation m_bitOperation = new BitOperation();

	private int m_pageStriping;

	private JBIG2Image m_pageBitmap;

	private PageInformationFlags m_pageInformationFlags = new PageInformationFlags();

	internal PageInformationFlags pageInformationFlags => m_pageInformationFlags;

	internal JBIG2Image pageBitmap => m_pageBitmap;

	internal int pageBitmapHeight => m_pageBitmapHeight;

	internal PageInformationSegment(JBIG2StreamDecoder streamDecoder)
		: base(streamDecoder)
	{
	}

	public override void readSegment()
	{
		short[] array = new short[4];
		m_decoder.ReadByte(array);
		m_pageBitmapWidth = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		m_pageBitmapHeight = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		m_xResolution = m_bitOperation.GetInt32(array);
		array = new short[4];
		m_decoder.ReadByte(array);
		m_yResolution = m_bitOperation.GetInt32(array);
		short flags = m_decoder.ReadByte();
		m_pageInformationFlags.setFlags(flags);
		array = new short[2];
		m_decoder.ReadByte(array);
		m_pageStriping = m_bitOperation.GetInt16(array);
		int flagValue = m_pageInformationFlags.GetFlagValue("DEFAULT_PIXEL_VALUE");
		m_pageBitmap = new JBIG2Image(height: (m_pageBitmapHeight != -1) ? m_pageBitmapHeight : (m_pageStriping & 0x7FFF), width: m_pageBitmapWidth, arithmeticDecoder: m_arithmeticDecoder, huffmanDecoder: m_huffmanDecoder, mmrDecoder: m_mmrDecoder);
		m_pageBitmap.Clear(flagValue);
	}
}
