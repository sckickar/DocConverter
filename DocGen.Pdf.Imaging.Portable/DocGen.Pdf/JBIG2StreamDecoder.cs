using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal class JBIG2StreamDecoder
{
	private ArithmeticDecoder m_arithmeticDecoder;

	private IList m_bitmaps = new List<object>();

	private byte[] m_globalData;

	private HuffmanDecoder m_huffmanDecoder;

	private MMRDecoder m_mmrDecoder;

	private int m_noOfPages = -1;

	private bool m_noOfPagesKnown;

	private bool m_randomAccessOrganisation;

	private Jbig2StreamReader m_reader;

	private IList m_segments = new List<object>();

	private BitOperation m_bitOperation = new BitOperation();

	internal ArithmeticDecoder ArithDecoder => m_arithmeticDecoder;

	internal HuffmanDecoder HuffDecoder => m_huffmanDecoder;

	internal MMRDecoder MmrDecoder => m_mmrDecoder;

	internal IList AllSegments => m_segments;

	internal byte[] GlobalData
	{
		set
		{
			m_globalData = value;
		}
	}

	internal int NumberOfPages => m_noOfPages;

	internal bool NumberOfPagesKnown => m_noOfPagesKnown;

	internal bool RandomAccessOrganisationUsed => m_randomAccessOrganisation;

	internal void appendBitmap(JBIG2Image bitmap)
	{
		m_bitmaps.Add(bitmap);
	}

	private bool CheckHeader()
	{
		short[] objA = new short[8] { 151, 74, 66, 50, 13, 10, 26, 10 };
		short[] array = new short[8];
		m_reader.ReadByte(array);
		return object.Equals(objA, array);
	}

	internal void ConsumeRemainingBits()
	{
		m_reader.ConsumeRemainingBits();
	}

	internal void DecodeJBIG2(byte[] data)
	{
		m_reader = new Jbig2StreamReader(data);
		ResetDecoder();
		if (!CheckHeader())
		{
			m_noOfPagesKnown = true;
			m_randomAccessOrganisation = false;
			m_noOfPages = 1;
			if (m_globalData != null)
			{
				m_reader = new Jbig2StreamReader(m_globalData);
				m_huffmanDecoder = new HuffmanDecoder(m_reader);
				m_mmrDecoder = new MMRDecoder(m_reader);
				m_arithmeticDecoder = new ArithmeticDecoder(m_reader);
				ReadSegments();
				m_reader = new Jbig2StreamReader(data);
			}
			else
			{
				m_reader.MovePointer(-8);
			}
		}
		else
		{
			SetFileHeaderFlags();
			if (m_noOfPagesKnown)
			{
				m_noOfPages = m_noOfPages;
			}
		}
		m_huffmanDecoder = new HuffmanDecoder(m_reader);
		m_mmrDecoder = new MMRDecoder(m_reader);
		m_arithmeticDecoder = new ArithmeticDecoder(m_reader);
		ReadSegments();
	}

	internal JBIG2Image FindBitmap(int bitmapNumber)
	{
		IEnumerator enumerator = m_bitmaps.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JBIG2Image jBIG2Image = (JBIG2Image)enumerator.Current;
			if (jBIG2Image.BitmapNumber == bitmapNumber)
			{
				return jBIG2Image;
			}
		}
		return null;
	}

	internal PageInformationSegment FindPageSegement(int page)
	{
		IEnumerator enumerator = m_segments.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JBIG2Segment jBIG2Segment = (JBIG2Segment)enumerator.Current;
			SegmentHeader segmentHeader = jBIG2Segment.m_segmentHeader;
			if (segmentHeader.SegmentType == 48 && segmentHeader.PageAssociation == page)
			{
				return (PageInformationSegment)jBIG2Segment;
			}
		}
		return null;
	}

	internal JBIG2Segment FindSegment(int segmentNumber)
	{
		IEnumerator enumerator = m_segments.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JBIG2Segment jBIG2Segment = (JBIG2Segment)enumerator.Current;
			if (jBIG2Segment.m_segmentHeader.SegmentNumber == segmentNumber)
			{
				return jBIG2Segment;
			}
		}
		return null;
	}

	private int getnoOfPages()
	{
		short[] array = new short[4];
		m_reader.ReadByte(array);
		return m_bitOperation.GetInt32(array);
	}

	internal JBIG2Image GetPageAsJBIG2Bitmap(int i)
	{
		return FindPageSegement(1).pageBitmap;
	}

	private void HandlePageAssociation(SegmentHeader segmentHeader)
	{
		int pageAssociation;
		if (segmentHeader.IsPageAssociationSizeSet)
		{
			short[] array = new short[4];
			m_reader.ReadByte(array);
			pageAssociation = m_bitOperation.GetInt32(array);
		}
		else
		{
			pageAssociation = m_reader.ReadByte();
		}
		segmentHeader.PageAssociation = pageAssociation;
	}

	private void HandleReferedToSegmentNumbers(SegmentHeader segmentHeader)
	{
		int referedToSegCount = segmentHeader.ReferedToSegCount;
		int[] array = new int[referedToSegCount];
		int segmentNumber = segmentHeader.SegmentNumber;
		if (segmentNumber <= 256)
		{
			for (int i = 0; i < referedToSegCount; i++)
			{
				array[i] = m_reader.ReadByte();
			}
		}
		else if (segmentNumber <= 65536)
		{
			short[] array2 = new short[2];
			for (int j = 0; j < referedToSegCount; j++)
			{
				m_reader.ReadByte(array2);
				array[j] = m_bitOperation.GetInt16(array2);
			}
		}
		else
		{
			short[] array3 = new short[4];
			for (int k = 0; k < referedToSegCount; k++)
			{
				m_reader.ReadByte(array3);
				array[k] = m_bitOperation.GetInt32(array3);
			}
		}
		segmentHeader.ReferredToSegments = array;
	}

	private void HandleSegmentDataLength(SegmentHeader segmentHeader)
	{
		short[] array = new short[4];
		m_reader.ReadByte(array);
		int @int = m_bitOperation.GetInt32(array);
		segmentHeader.DataLength = @int;
	}

	private void HandleSegmentHeaderFlags(SegmentHeader segmentHeader)
	{
		short segmentHeaderFlags = m_reader.ReadByte();
		segmentHeader.SetSegmentHeaderFlags(segmentHeaderFlags);
	}

	private void HandleSegmentNumber(SegmentHeader segmentHeader)
	{
		short[] array = new short[4];
		m_reader.ReadByte(array);
		int @int = m_bitOperation.GetInt32(array);
		segmentHeader.SegmentNumber = @int;
	}

	private void HandleSegmentReferredToCountAndRententionFlags(SegmentHeader segmentHeader)
	{
		short num = m_reader.ReadByte();
		int num2 = (num & 0xE0) >> 5;
		short[] array = null;
		short num3 = (short)(num & 0x1F);
		if (num2 <= 4)
		{
			array = new short[1] { num3 };
		}
		else if (num2 == 7)
		{
			short[] array2 = new short[4] { num3, 0, 0, 0 };
			for (int i = 1; i < 4; i++)
			{
				array2[i] = m_reader.ReadByte();
			}
			num2 = m_bitOperation.GetInt32(array2);
			array = new short[(int)Math.Ceiling(4.0 + (double)(num2 + 1) / 8.0) - 4];
			m_reader.ReadByte(array);
		}
		segmentHeader.ReferedToSegCount = num2;
		segmentHeader.RententionFlags = array;
	}

	internal void MovePointer(int i)
	{
		m_reader.MovePointer(i);
	}

	internal int ReadBit()
	{
		return m_reader.ReadBit();
	}

	internal int ReadBits(int num)
	{
		return m_reader.ReadBits(num);
	}

	internal short ReadByte()
	{
		return m_reader.ReadByte();
	}

	internal void ReadByte(short[] buff)
	{
		m_reader.ReadByte(buff);
	}

	private void ReadSegmentHeader(SegmentHeader segmentHeader)
	{
		HandleSegmentNumber(segmentHeader);
		HandleSegmentHeaderFlags(segmentHeader);
		HandleSegmentReferredToCountAndRententionFlags(segmentHeader);
		HandleReferedToSegmentNumbers(segmentHeader);
		HandlePageAssociation(segmentHeader);
		if (segmentHeader.SegmentType != 51)
		{
			HandleSegmentDataLength(segmentHeader);
		}
	}

	private void ReadSegments()
	{
		bool flag = false;
		while (!m_reader.Getfinished() && !flag)
		{
			SegmentHeader segmentHeader = new SegmentHeader();
			ReadSegmentHeader(segmentHeader);
			JBIG2Segment jBIG2Segment = null;
			int segmentType = segmentHeader.SegmentType;
			int[] referredToSegments = segmentHeader.ReferredToSegments;
			int referedToSegCount = segmentHeader.ReferedToSegCount;
			switch (segmentType)
			{
			case 4:
				jBIG2Segment = new TextRegionSegment(this, inlineImage: false);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 6:
				jBIG2Segment = new TextRegionSegment(this, inlineImage: true);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 7:
				jBIG2Segment = new TextRegionSegment(this, inlineImage: true);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 16:
				jBIG2Segment = new PatternDictionarySegment(this);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 0:
				jBIG2Segment = new SymbolDictionarySegment(this);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 20:
				jBIG2Segment = new HalftoneRegionSegment(this, inlineImage: false);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 22:
				jBIG2Segment = new HalftoneRegionSegment(this, inlineImage: true);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 23:
				jBIG2Segment = new HalftoneRegionSegment(this, inlineImage: true);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 36:
				jBIG2Segment = new GenericRegionSegment(this, inlineImage: false);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 38:
				jBIG2Segment = new GenericRegionSegment(this, inlineImage: true);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 39:
				jBIG2Segment = new GenericRegionSegment(this, inlineImage: true);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 40:
				jBIG2Segment = new RefinementRegionSegment(this, inlineImage: false, referredToSegments, referedToSegCount);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 42:
				jBIG2Segment = new RefinementRegionSegment(this, inlineImage: true, referredToSegments, referedToSegCount);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 43:
				jBIG2Segment = new RefinementRegionSegment(this, inlineImage: true, referredToSegments, referedToSegCount);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 48:
				jBIG2Segment = new PageInformationSegment(this);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 50:
				jBIG2Segment = new EndOfStripeSegment(this);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 51:
				flag = true;
				continue;
			case 62:
				jBIG2Segment = new ExtensionSegment(this);
				jBIG2Segment.m_segmentHeader = segmentHeader;
				break;
			case 49:
				continue;
			}
			if (!m_randomAccessOrganisation)
			{
				jBIG2Segment?.readSegment();
			}
			m_segments.Add(jBIG2Segment);
		}
		if (m_randomAccessOrganisation)
		{
			IEnumerator enumerator = m_segments.GetEnumerator();
			while (enumerator.MoveNext())
			{
				((JBIG2Segment)enumerator.Current).readSegment();
			}
		}
	}

	private void ResetDecoder()
	{
		m_noOfPagesKnown = false;
		m_randomAccessOrganisation = false;
		m_noOfPages = -1;
		m_segments.Clear();
		m_bitmaps.Clear();
	}

	private void SetFileHeaderFlags()
	{
		short num = m_reader.ReadByte();
		_ = num & 0xFC;
		int num2 = num & 1;
		m_randomAccessOrganisation = num2 == 0;
		int num3 = num & 2;
		m_noOfPagesKnown = num3 == 0;
	}
}
