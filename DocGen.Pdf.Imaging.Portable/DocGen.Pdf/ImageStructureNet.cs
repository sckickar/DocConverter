using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using BitMiracle.LibTiff.Classic;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Compression;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class ImageStructureNet
{
	private byte[] m_twoDimention = new byte[128]
	{
		80, 88, 23, 71, 30, 30, 62, 62, 4, 4,
		4, 4, 4, 4, 4, 4, 11, 11, 11, 11,
		11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
		11, 11, 35, 35, 35, 35, 35, 35, 35, 35,
		35, 35, 35, 35, 35, 35, 35, 35, 51, 51,
		51, 51, 51, 51, 51, 51, 51, 51, 51, 51,
		51, 51, 51, 51, 41, 41, 41, 41, 41, 41,
		41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
		41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
		41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
		41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
		41, 41, 41, 41, 41, 41, 41, 41, 41, 41,
		41, 41, 41, 41, 41, 41, 41, 41
	};

	private int[] m_BlackandWhitePixels = new int[16]
	{
		28679, 28679, 31752, 32777, 33801, 34825, 35849, 36873, 29703, 29703,
		30727, 30727, 37897, 38921, 39945, 40969
	};

	private int[] m_whitePixel = new int[1024]
	{
		6430, 6400, 6400, 6400, 3225, 3225, 3225, 3225, 944, 944,
		944, 944, 976, 976, 976, 976, 1456, 1456, 1456, 1456,
		1488, 1488, 1488, 1488, 718, 718, 718, 718, 718, 718,
		718, 718, 750, 750, 750, 750, 750, 750, 750, 750,
		1520, 1520, 1520, 1520, 1552, 1552, 1552, 1552, 428, 428,
		428, 428, 428, 428, 428, 428, 428, 428, 428, 428,
		428, 428, 428, 428, 654, 654, 654, 654, 654, 654,
		654, 654, 1072, 1072, 1072, 1072, 1104, 1104, 1104, 1104,
		1136, 1136, 1136, 1136, 1168, 1168, 1168, 1168, 1200, 1200,
		1200, 1200, 1232, 1232, 1232, 1232, 622, 622, 622, 622,
		622, 622, 622, 622, 1008, 1008, 1008, 1008, 1040, 1040,
		1040, 1040, 44, 44, 44, 44, 44, 44, 44, 44,
		44, 44, 44, 44, 44, 44, 44, 44, 396, 396,
		396, 396, 396, 396, 396, 396, 396, 396, 396, 396,
		396, 396, 396, 396, 1712, 1712, 1712, 1712, 1744, 1744,
		1744, 1744, 846, 846, 846, 846, 846, 846, 846, 846,
		1264, 1264, 1264, 1264, 1296, 1296, 1296, 1296, 1328, 1328,
		1328, 1328, 1360, 1360, 1360, 1360, 1392, 1392, 1392, 1392,
		1424, 1424, 1424, 1424, 686, 686, 686, 686, 686, 686,
		686, 686, 910, 910, 910, 910, 910, 910, 910, 910,
		1968, 1968, 1968, 1968, 2000, 2000, 2000, 2000, 2032, 2032,
		2032, 2032, 16, 16, 16, 16, 10257, 10257, 10257, 10257,
		12305, 12305, 12305, 12305, 330, 330, 330, 330, 330, 330,
		330, 330, 330, 330, 330, 330, 330, 330, 330, 330,
		330, 330, 330, 330, 330, 330, 330, 330, 330, 330,
		330, 330, 330, 330, 330, 330, 362, 362, 362, 362,
		362, 362, 362, 362, 362, 362, 362, 362, 362, 362,
		362, 362, 362, 362, 362, 362, 362, 362, 362, 362,
		362, 362, 362, 362, 362, 362, 362, 362, 878, 878,
		878, 878, 878, 878, 878, 878, 1904, 1904, 1904, 1904,
		1936, 1936, 1936, 1936, -18413, -18413, -16365, -16365, -14317, -14317,
		-10221, -10221, 590, 590, 590, 590, 590, 590, 590, 590,
		782, 782, 782, 782, 782, 782, 782, 782, 1584, 1584,
		1584, 1584, 1616, 1616, 1616, 1616, 1648, 1648, 1648, 1648,
		1680, 1680, 1680, 1680, 814, 814, 814, 814, 814, 814,
		814, 814, 1776, 1776, 1776, 1776, 1808, 1808, 1808, 1808,
		1840, 1840, 1840, 1840, 1872, 1872, 1872, 1872, 6157, 6157,
		6157, 6157, 6157, 6157, 6157, 6157, 6157, 6157, 6157, 6157,
		6157, 6157, 6157, 6157, -12275, -12275, -12275, -12275, -12275, -12275,
		-12275, -12275, -12275, -12275, -12275, -12275, -12275, -12275, -12275, -12275,
		14353, 14353, 14353, 14353, 16401, 16401, 16401, 16401, 22547, 22547,
		24595, 24595, 20497, 20497, 20497, 20497, 18449, 18449, 18449, 18449,
		26643, 26643, 28691, 28691, 30739, 30739, -32749, -32749, -30701, -30701,
		-28653, -28653, -26605, -26605, -24557, -24557, -22509, -22509, -20461, -20461,
		8207, 8207, 8207, 8207, 8207, 8207, 8207, 8207, 72, 72,
		72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
		72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
		72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
		72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
		72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
		72, 72, 72, 72, 72, 72, 72, 72, 72, 72,
		72, 72, 104, 104, 104, 104, 104, 104, 104, 104,
		104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
		104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
		104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
		104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
		104, 104, 104, 104, 104, 104, 104, 104, 104, 104,
		104, 104, 104, 104, 104, 104, 4107, 4107, 4107, 4107,
		4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107,
		4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107,
		4107, 4107, 4107, 4107, 4107, 4107, 4107, 4107, 266, 266,
		266, 266, 266, 266, 266, 266, 266, 266, 266, 266,
		266, 266, 266, 266, 266, 266, 266, 266, 266, 266,
		266, 266, 266, 266, 266, 266, 266, 266, 266, 266,
		298, 298, 298, 298, 298, 298, 298, 298, 298, 298,
		298, 298, 298, 298, 298, 298, 298, 298, 298, 298,
		298, 298, 298, 298, 298, 298, 298, 298, 298, 298,
		298, 298, 524, 524, 524, 524, 524, 524, 524, 524,
		524, 524, 524, 524, 524, 524, 524, 524, 556, 556,
		556, 556, 556, 556, 556, 556, 556, 556, 556, 556,
		556, 556, 556, 556, 136, 136, 136, 136, 136, 136,
		136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
		136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
		136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
		136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
		136, 136, 136, 136, 136, 136, 136, 136, 136, 136,
		136, 136, 136, 136, 136, 136, 136, 136, 168, 168,
		168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
		168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
		168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
		168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
		168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
		168, 168, 168, 168, 168, 168, 168, 168, 168, 168,
		168, 168, 460, 460, 460, 460, 460, 460, 460, 460,
		460, 460, 460, 460, 460, 460, 460, 460, 492, 492,
		492, 492, 492, 492, 492, 492, 492, 492, 492, 492,
		492, 492, 492, 492, 2059, 2059, 2059, 2059, 2059, 2059,
		2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059,
		2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059, 2059,
		2059, 2059, 2059, 2059, 2059, 2059, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		200, 200, 200, 200, 200, 200, 200, 200, 200, 200,
		232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
		232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
		232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
		232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
		232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
		232, 232, 232, 232, 232, 232, 232, 232, 232, 232,
		232, 232, 232, 232
	};

	private int[] m_originBlack = new int[16]
	{
		3226, 6412, 200, 168, 38, 38, 134, 134, 100, 100,
		100, 100, 68, 68, 68, 68
	};

	private int[] m_blackPixel = new int[512]
	{
		62, 62, 30, 30, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225,
		3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225,
		3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225, 3225,
		3225, 3225, 3225, 3225, 588, 588, 588, 588, 588, 588,
		588, 588, 1680, 1680, 20499, 22547, 24595, 26643, 1776, 1776,
		1808, 1808, -24557, -22509, -20461, -18413, 1904, 1904, 1936, 1936,
		-16365, -14317, 782, 782, 782, 782, 814, 814, 814, 814,
		-12269, -10221, 10257, 10257, 12305, 12305, 14353, 14353, 16403, 18451,
		1712, 1712, 1744, 1744, 28691, 30739, -32749, -30701, -28653, -26605,
		2061, 2061, 2061, 2061, 2061, 2061, 2061, 2061, 424, 424,
		424, 424, 424, 424, 424, 424, 424, 424, 424, 424,
		424, 424, 424, 424, 424, 424, 424, 424, 424, 424,
		424, 424, 424, 424, 424, 424, 424, 424, 424, 424,
		750, 750, 750, 750, 1616, 1616, 1648, 1648, 1424, 1424,
		1456, 1456, 1488, 1488, 1520, 1520, 1840, 1840, 1872, 1872,
		1968, 1968, 8209, 8209, 524, 524, 524, 524, 524, 524,
		524, 524, 556, 556, 556, 556, 556, 556, 556, 556,
		1552, 1552, 1584, 1584, 2000, 2000, 2032, 2032, 976, 976,
		1008, 1008, 1040, 1040, 1072, 1072, 1296, 1296, 1328, 1328,
		718, 718, 718, 718, 456, 456, 456, 456, 456, 456,
		456, 456, 456, 456, 456, 456, 456, 456, 456, 456,
		456, 456, 456, 456, 456, 456, 456, 456, 456, 456,
		456, 456, 456, 456, 456, 456, 326, 326, 326, 326,
		326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
		326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
		326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
		326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
		326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
		326, 326, 326, 326, 326, 326, 326, 326, 326, 326,
		358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
		358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
		358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
		358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
		358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
		358, 358, 358, 358, 358, 358, 358, 358, 358, 358,
		358, 358, 358, 358, 490, 490, 490, 490, 490, 490,
		490, 490, 490, 490, 490, 490, 490, 490, 490, 490,
		4113, 4113, 6161, 6161, 848, 848, 880, 880, 912, 912,
		944, 944, 622, 622, 622, 622, 654, 654, 654, 654,
		1104, 1104, 1136, 1136, 1168, 1168, 1200, 1200, 1232, 1232,
		1264, 1264, 686, 686, 686, 686, 1360, 1360, 1392, 1392,
		12, 12, 12, 12, 12, 12, 12, 12, 390, 390,
		390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
		390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
		390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
		390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
		390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
		390, 390, 390, 390, 390, 390, 390, 390, 390, 390,
		390, 390
	};

	private int[] m_blackBit = new int[4] { 292, 260, 226, 226 };

	internal bool m_isExtGStateContainsSMask;

	private int m_elementChanging;

	internal bool m_isDeviceGrayColorspace;

	internal bool m_isDeviceRGBColorspace;

	internal bool m_isDeviceCMYKColorspace;

	private int m_originalLineLength;

	private int m_relativeAddress;

	private int m_currentIndex = 1;

	private int m_bitArrived;

	internal bool m_isWhitePixel = true;

	internal bool m_is2dimention = true;

	private int m_bytesOnDemand;

	private int m_indexPointer;

	private int m_bitDataCount;

	private DrawImagePixels m_outputStream;

	private DrawImagePixels m_bitData;

	private PdfDictionary m_imageDictionary;

	private string[] m_imageFilter;

	private PdfDictionary[] m_decodeParam;

	private MemoryStream m_imageStream;

	internal bool m_isCCITTFaxDecode;

	internal string[] m_maskFilter;

	private Stream m_maskStream;

	private float m_height;

	private float m_width;

	private float m_maskWidth;

	private float m_maskHeight;

	private float m_maskBitsPerComponent;

	private float m_bitsPerComponent;

	private Bitmap m_embeddedImage;

	private bool m_isImageStreamParsed;

	private PdfMatrix m_imageInfo;

	private string m_colorspace;

	private string m_colorspaceBase;

	private string internalColorSpace;

	private int m_colorspaceHival;

	private PixelFormat m_pixelFormat = PixelFormat.Format24bppRgb;

	private MemoryStream m_colorspaceStream;

	internal bool m_isBlackIs1;

	private bool IsTransparent;

	internal bool m_isMaskImage;

	internal StringBuilder exceptions = new StringBuilder();

	internal bool m_isIccBasedAlternateDeviceGray;

	private Dictionary<string, MemoryStream> colorSpaceResourceDict = new Dictionary<string, MemoryStream>();

	private Dictionary<string, PdfStream> nonIndexedImageColorResource = new Dictionary<string, PdfStream>();

	private bool isIndexedImage;

	private bool m_isImageMask;

	public MemoryStream outStream = new MemoryStream();

	private bool m_isEarlyChange = true;

	private int[] m_pixel;

	private string m_maskColorspace;

	private int numberOfComponents;

	private byte[] inputData;

	private PdfDictionary m_maskDictionary;

	private string indexedColorSpace;

	private bool isDualFilter;

	private MemoryStream imageStreamBackup;

	private bool m_isImageInterpolated;

	private bool m_isImageMasked;

	private bool m_isSoftMasked;

	private byte[] m_indexedRGBvalues;

	private bool m_isImageForExtraction;

	private int[] m_maskedPixels;

	private bool m_isJPXDecode;

	private bool isDeviceN;

	private PdfArray m_decodeArray;

	internal ImageFormat imageFormat;

	internal bool m_compressPDF;

	internal MemoryStream m_decodedOriginalBitmap;

	internal MemoryStream m_decodedMaskBitmap;

	private MemoryStream m_decodedMemoryStream;

	private static readonly object ImageParsingLocker = new object();

	internal bool IsImageMasked => m_isImageMasked;

	internal bool IsSoftMasked => m_isSoftMasked;

	internal bool IsImageMask
	{
		get
		{
			GetIsImageMask();
			return m_isImageMask;
		}
		set
		{
			m_isImageMask = value;
		}
	}

	internal bool IsImageInterpolated => m_isImageInterpolated;

	internal bool IsImageForExtraction
	{
		get
		{
			return m_isImageForExtraction;
		}
		set
		{
			m_isImageForExtraction = value;
		}
	}

	internal bool IsEarlyChange
	{
		get
		{
			GetIsEarlyChange();
			return m_isEarlyChange;
		}
		set
		{
			m_isEarlyChange = value;
		}
	}

	internal PdfDictionary ImageDictionary => m_imageDictionary;

	internal PdfMatrix ImageInfo
	{
		get
		{
			return m_imageInfo;
		}
		set
		{
			m_imageInfo = value;
		}
	}

	internal string[] ImageFilter
	{
		get
		{
			if (m_imageFilter == null)
			{
				m_imageFilter = GetImageFilter();
			}
			return m_imageFilter;
		}
	}

	internal string ColorSpace
	{
		get
		{
			if (m_colorspace == null)
			{
				GetColorSpace();
			}
			return m_colorspace;
		}
		set
		{
			m_colorspace = value;
		}
	}

	internal MemoryStream DecodedMemoryStream => m_decodedMemoryStream;

	internal Bitmap EmbeddedImage
	{
		get
		{
			if (m_embeddedImage == null)
			{
				if (PdfDocument.EnableThreadSafe)
				{
					lock (ImageParsingLocker)
					{
						GetEmbeddedImage();
					}
				}
				else
				{
					GetEmbeddedImage();
				}
			}
			return m_embeddedImage;
		}
	}

	internal PdfDictionary[] DecodeParam
	{
		get
		{
			if (m_decodeParam == null)
			{
				m_decodeParam = GetDecodeParam(m_imageDictionary);
			}
			return m_decodeParam;
		}
	}

	internal PdfArray DecodeArray
	{
		get
		{
			GetDecodeArray();
			return m_decodeArray;
		}
		set
		{
			m_decodeArray = value;
		}
	}

	public MemoryStream ImageStream
	{
		get
		{
			if (m_imageStream == null)
			{
				PdfStream pdfStream = m_imageDictionary as PdfStream;
				m_imageStream = pdfStream.InternalStream;
				inputData = new byte[pdfStream.Data.Length];
				inputData = pdfStream.Data;
			}
			return m_imageStream;
		}
		set
		{
			m_imageStream = value;
		}
	}

	internal int[] Pixels
	{
		get
		{
			return m_pixel;
		}
		set
		{
			m_pixel = value;
		}
	}

	public Stream MaskStream
	{
		get
		{
			if (m_maskStream == null)
			{
				PdfStream pdfStream = null;
				if (m_imageDictionary.ContainsKey("SMask"))
				{
					pdfStream = PdfCrossTable.Dereference(m_imageDictionary["SMask"]) as PdfStream;
				}
				else if (m_imageDictionary.ContainsKey("Mask"))
				{
					pdfStream = PdfCrossTable.Dereference(m_imageDictionary["Mask"]) as PdfStream;
				}
				if (pdfStream != null)
				{
					m_maskStream = pdfStream.InternalStream;
				}
				PdfDictionary pdfDictionary = null;
				if (m_imageDictionary.ContainsKey("SMask"))
				{
					pdfDictionary = PdfCrossTable.Dereference(m_imageDictionary["SMask"]) as PdfDictionary;
				}
				else if (m_imageDictionary.ContainsKey("Mask"))
				{
					pdfDictionary = PdfCrossTable.Dereference(m_imageDictionary["Mask"]) as PdfDictionary;
				}
				if (pdfDictionary != null)
				{
					m_maskDictionary = pdfDictionary;
					if (pdfDictionary.ContainsKey("Width") && PdfCrossTable.Dereference(pdfDictionary["Width"]) is PdfNumber pdfNumber)
					{
						m_maskWidth = pdfNumber.IntValue;
					}
					if (pdfDictionary.ContainsKey("Height") && PdfCrossTable.Dereference(pdfDictionary["Height"]) is PdfNumber pdfNumber2)
					{
						m_maskHeight = pdfNumber2.IntValue;
					}
					if (pdfDictionary.ContainsKey("BitsPerComponent") && PdfCrossTable.Dereference(pdfDictionary["BitsPerComponent"]) is PdfNumber pdfNumber3)
					{
						m_maskBitsPerComponent = pdfNumber3.IntValue;
					}
					m_maskFilter = new string[1];
					if (pdfDictionary.ContainsKey("Filter"))
					{
						if (pdfDictionary["Filter"] is PdfArray)
						{
							if (PdfCrossTable.Dereference(pdfDictionary["Filter"]) is PdfArray pdfArray)
							{
								m_maskFilter = new string[pdfArray.Count];
								for (int i = 0; i < pdfArray.Count; i++)
								{
									PdfName pdfName = PdfCrossTable.Dereference(pdfArray[i]) as PdfName;
									if (pdfName != null)
									{
										m_maskFilter[i] = pdfName.Value;
									}
								}
							}
						}
						else
						{
							PdfName pdfName2 = PdfCrossTable.Dereference(pdfDictionary["Filter"]) as PdfName;
							if (pdfName2 != null)
							{
								m_maskFilter[0] = pdfName2.Value;
							}
						}
					}
					if (pdfDictionary.ContainsKey("ColorSpace"))
					{
						PdfName pdfName3 = PdfCrossTable.Dereference(pdfDictionary["ColorSpace"]) as PdfName;
						if (pdfName3 != null)
						{
							m_maskColorspace = pdfName3.Value;
						}
					}
				}
			}
			return m_maskStream;
		}
		set
		{
			m_maskStream = value;
		}
	}

	internal float Width
	{
		get
		{
			if (m_width == 0f)
			{
				m_width = GetImageWidth();
			}
			return m_width;
		}
	}

	internal float Height
	{
		get
		{
			if (m_height == 0f)
			{
				m_height = GetImageHeight();
			}
			return m_height;
		}
	}

	internal float BitsPerComponent
	{
		get
		{
			if (m_bitsPerComponent == 0f)
			{
				m_bitsPerComponent = GetBitsPerComponent();
			}
			return m_bitsPerComponent;
		}
	}

	internal int WhitePixel
	{
		get
		{
			int num = 0;
			bool flag = true;
			while (flag)
			{
				int num2 = OneDimentionBit(10);
				m_bitArrived += 10;
				int num3 = m_whitePixel[num2];
				int num4 = (num3 >>> 1) & 0xF;
				switch (num4)
				{
				case 12:
				{
					int num6 = OneDimentionBit(2);
					m_bitArrived += 2;
					num2 = ((num2 << 2) & 0xC) | num6;
					num3 = m_BlackandWhitePixels[num2];
					num4 = (num3 >>> 1) & 7;
					int num5 = (num3 >>> 4) & 0xFFF;
					num += num5;
					renewPointer(4 - num4);
					break;
				}
				case 0:
				case 15:
					throw new Exception("CCITT Error in getWhitePixel");
				default:
				{
					int num5 = (num3 >>> 5) & 0x7FF;
					num += num5;
					renewPointer(10 - num4);
					if ((num3 & 1) == 0)
					{
						flag = false;
					}
					break;
				}
				}
			}
			return num;
		}
	}

	internal int BlackPixel
	{
		get
		{
			int num = 0;
			bool flag = true;
			while (flag)
			{
				int num2 = OneDimentionBit(4);
				m_bitArrived += 4;
				int num3 = m_originBlack[num2];
				int num4 = (num3 >>> 1) & 0xF;
				int num5 = (num3 >>> 5) & 0x7FF;
				switch (num5)
				{
				case 100:
					num2 = OneDimentionBit(9);
					m_bitArrived += 9;
					num3 = m_blackPixel[num2];
					num4 = (num3 >>> 1) & 0xF;
					num5 = (num3 >>> 5) & 0x7FF;
					switch (num4)
					{
					case 12:
						renewPointer(5);
						num2 = OneDimentionBit(4);
						m_bitArrived += 4;
						num3 = m_BlackandWhitePixels[num2];
						num4 = (num3 >>> 1) & 7;
						num5 = (num3 >>> 4) & 0xFFF;
						num += num5;
						renewPointer(4 - num4);
						break;
					case 15:
						throw new Exception("CCITT unexpected EOL");
					default:
						num += num5;
						renewPointer(9 - num4);
						if ((num3 & 1) == 0)
						{
							flag = false;
						}
						break;
					}
					break;
				case 200:
					num2 = OneDimentionBit(2);
					m_bitArrived += 2;
					num3 = m_blackBit[num2];
					num5 = (num3 >>> 5) & 0x7FF;
					num += num5;
					num4 = (num3 >>> 1) & 0xF;
					renewPointer(2 - num4);
					flag = false;
					break;
				default:
					num += num5;
					renewPointer(4 - num4);
					flag = false;
					break;
				}
			}
			return num;
		}
	}

	public ImageStructureNet()
	{
	}

	public ImageStructureNet(IPdfPrimitive fontDictionary, PdfMatrix tm)
	{
		m_imageDictionary = fontDictionary as PdfDictionary;
		ImageInfo = tm;
	}

	internal void FixTwoDimention(int[] previousRange, int[] currentRange, int elementChanging, int[] currentElementChange)
	{
		m_isWhitePixel = true;
		m_currentIndex = 0;
		m_relativeAddress = 0;
		int num = 0;
		int renewedBits = -1;
		while ((float)m_relativeAddress < Width)
		{
			GetSucceedingElement(renewedBits, m_isWhitePixel, currentElementChange, previousRange, elementChanging);
			int num2 = OneDimentionBit(7);
			m_bitArrived += 7;
			int num3 = m_twoDimention[num2] & 0xFF;
			int num4 = (num3 & 0x78) >>> 3;
			if (!m_is2dimention)
			{
				num = num3 & 7;
			}
			else if (num4 != 11)
			{
				renewPointer(7 - (num3 & 7));
			}
			if (num4 != 0)
			{
				if (num4 != 1)
				{
					if (num4 == 11)
					{
						int num5 = OneDimentionBit(3);
						m_bitArrived += 3;
						if (num5 != 7)
						{
							throw new Exception("The value of" + num5 + " was Unexpected");
						}
						int num6 = 0;
						bool flag = false;
						while (!flag)
						{
							while (true)
							{
								num2 = OneDimentionBit(1);
								m_bitArrived++;
								if (num2 == 1)
								{
									break;
								}
								num6++;
							}
							if (num6 > 5)
							{
								num6 -= 6;
								if (!m_isWhitePixel && num6 > 0)
								{
									currentRange[m_currentIndex++] = m_relativeAddress;
								}
								m_relativeAddress += num6;
								if (num6 > 0)
								{
									m_isWhitePixel = true;
								}
								num2 = OneDimentionBit(1);
								m_bitArrived++;
								if (num2 == 0)
								{
									if (!m_isWhitePixel)
									{
										currentRange[m_currentIndex++] = m_relativeAddress;
									}
									m_isWhitePixel = true;
								}
								else
								{
									if (m_isWhitePixel)
									{
										currentRange[m_currentIndex++] = m_relativeAddress;
									}
									m_isWhitePixel = false;
								}
								flag = true;
							}
							if (num6 == 5)
							{
								if (!m_isWhitePixel)
								{
									currentRange[m_currentIndex++] = m_relativeAddress;
								}
								m_relativeAddress += num6;
								m_isWhitePixel = true;
							}
							else
							{
								m_relativeAddress += num6;
								currentRange[m_currentIndex++] = m_relativeAddress;
								m_outputStream.SetIndex(m_indexPointer, m_indexPointer + 1);
								m_indexPointer++;
								m_relativeAddress++;
								m_isWhitePixel = false;
							}
						}
					}
					else
					{
						if (num4 > 8)
						{
							throw new Exception("CCITT unexpected value");
						}
						currentRange[m_currentIndex++] = currentElementChange[0] + (num4 - 5);
						int num7 = currentElementChange[0] + (num4 - 5) - m_relativeAddress;
						if (!m_isWhitePixel)
						{
							m_outputStream.SetIndex(m_indexPointer, m_indexPointer + num7);
						}
						m_indexPointer += num7;
						renewedBits = (m_relativeAddress = currentElementChange[0] + (num4 - 5));
						m_isWhitePixel = !m_isWhitePixel;
						if (!m_is2dimention)
						{
							m_bitArrived -= 7 - num;
						}
					}
				}
				else
				{
					if (!m_is2dimention)
					{
						m_bitArrived -= 7 - num;
					}
					int num7;
					if (m_isWhitePixel)
					{
						num7 = WhitePixel;
						m_indexPointer += num7;
						m_relativeAddress += num7;
						currentRange[m_currentIndex++] = m_relativeAddress;
						num7 = BlackPixel;
						m_outputStream.SetIndex(m_indexPointer, m_indexPointer + num7);
						m_indexPointer += num7;
					}
					else
					{
						num7 = BlackPixel;
						m_outputStream.SetIndex(m_indexPointer, m_indexPointer + num7);
						m_indexPointer += num7;
						m_relativeAddress += num7;
						currentRange[m_currentIndex++] = m_relativeAddress;
						num7 = WhitePixel;
						m_indexPointer += num7;
					}
					m_relativeAddress += num7;
					currentRange[m_currentIndex++] = m_relativeAddress;
					renewedBits = m_relativeAddress;
				}
			}
			else
			{
				int num7 = currentElementChange[1] - m_relativeAddress;
				if (!m_isWhitePixel)
				{
					m_outputStream.SetIndex(m_indexPointer, m_indexPointer + num7);
				}
				m_indexPointer += num7;
				m_relativeAddress = currentElementChange[1];
				renewedBits = currentElementChange[1];
				if (!m_is2dimention)
				{
					m_bitArrived -= 7 - num;
				}
			}
		}
	}

	private void GetSucceedingElement(int renewedBits, bool isWhitePixel, int[] currentElementChange, int[] previousElementChange, int elementChanging)
	{
		int num = 0;
		num = ((!isWhitePixel) ? (num | 1) : (num & -2));
		int i;
		for (i = num; i < elementChanging; i += 2)
		{
			int num2 = previousElementChange[i];
			if (num2 > renewedBits)
			{
				currentElementChange[0] = num2;
				break;
			}
		}
		if (i + 1 < elementChanging)
		{
			currentElementChange[1] = previousElementChange[i + 1];
		}
	}

	internal int OneDimentionBit(int bitsToGet)
	{
		return GetOneDimentionBit(bitsToGet, is1dimention: false);
	}

	private int GetOneDimentionBit(int bitsToGet, bool is1dimention)
	{
		int num = 0;
		int num2 = 0;
		if (is1dimention && bitsToGet > 8)
		{
			num2++;
		}
		for (int i = 0; i < bitsToGet; i++)
		{
			if (m_bitData.GetValue(i + m_bitArrived))
			{
				int num3 = 1 << bitsToGet - i - 1 - num2;
				num |= num3;
			}
		}
		return num;
	}

	private void renewPointer(int bitsToMoveBack)
	{
		m_bitArrived -= bitsToMoveBack;
	}

	private DrawImagePixels EstimateBit(MemoryStream imageStream, int bitOnDemand)
	{
		int num = 0;
		DrawImagePixels drawImagePixels = new DrawImagePixels(bitOnDemand);
		byte[] buffer = imageStream.GetBuffer();
		foreach (byte b in buffer)
		{
			for (int num2 = 7; num2 >= 0; num2--)
			{
				if ((b & (1 << num2)) >= 1)
				{
					drawImagePixels.SetBitdata(num);
				}
				num++;
			}
		}
		return drawImagePixels;
	}

	private Bitmap GetEmbeddedImage()
	{
		try
		{
			if (m_decodedMemoryStream == null)
			{
				m_decodedMemoryStream = GetImageStream();
			}
			if (m_decodedMemoryStream != null)
			{
				if (imageFormat != ImageFormat.Tiff)
				{
					m_embeddedImage = Bitmap.FromStream(m_decodedMemoryStream);
				}
				else
				{
					m_decodedMemoryStream.Position = 0L;
					MemoryStream memoryStream = ConvertTifftoPng(m_decodedMemoryStream);
					if (memoryStream != null)
					{
						m_embeddedImage = Bitmap.FromStream(memoryStream);
						imageFormat = ImageFormat.Png;
						m_decodedMemoryStream = memoryStream;
					}
				}
				ImageStream = null;
				return m_embeddedImage;
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	internal byte[] GetEncodedStream()
	{
		byte[] array = new byte[m_bytesOnDemand];
		int num = 7;
		int num2 = 0;
		byte b = 0;
		for (int i = 0; i < m_indexPointer; i++)
		{
			if (m_outputStream.GetValue(i))
			{
				int num3 = 1 << num;
				b |= (byte)num3;
				num--;
			}
			else
			{
				num--;
			}
			if ((float)(i + 1) % Width == 0f && i != 0)
			{
				num = -1;
			}
			if (num < 0 && num2 < array.Length)
			{
				array[num2] = b;
				num2++;
				num = 7;
				b = 0;
			}
		}
		return array;
	}

	private MemoryStream DecodeCCITTFaxDecodeStream(MemoryStream imageStream, PdfDictionary imageDictionary, PdfDictionary decodeParams)
	{
		PdfArray pdfArray = new PdfArray();
		bool flag = false;
		int offset = 1;
		int offset2 = 0;
		int offset3 = 0;
		if (imageDictionary.ContainsKey("Width"))
		{
			if (imageDictionary["Width"] is PdfNumber)
			{
				offset2 = (imageDictionary["Width"] as PdfNumber).IntValue;
			}
			else if (imageDictionary["Width"] as PdfReferenceHolder != null)
			{
				offset2 = ((imageDictionary["Width"] as PdfReferenceHolder).Object as PdfNumber).IntValue;
			}
		}
		if (imageDictionary.ContainsKey("Height"))
		{
			if (imageDictionary["Height"] is PdfNumber)
			{
				offset3 = (imageDictionary["Height"] as PdfNumber).IntValue;
			}
			else if (imageDictionary["Height"] as PdfReferenceHolder != null)
			{
				offset3 = ((imageDictionary["Height"] as PdfReferenceHolder).Object as PdfNumber).IntValue;
			}
		}
		if (imageDictionary.ContainsKey("Decode"))
		{
			pdfArray = imageDictionary[new PdfName("Decode")] as PdfArray;
			offset = (pdfArray[0] as PdfNumber).IntValue;
		}
		TiffDecode tiffDecode = new TiffDecode();
		tiffDecode.m_tiffHeader.m_byteOrder = 18761;
		tiffDecode.m_tiffHeader.m_version = 42;
		tiffDecode.m_tiffHeader.m_dirOffset = (uint)(imageStream.Length + 9);
		tiffDecode.WriteHeader(tiffDecode.m_tiffHeader);
		tiffDecode.m_stream.Seek(8L, SeekOrigin.Begin);
		if (decodeParams.ContainsKey("EncodedByteAlign"))
		{
			PdfBoolean pdfBoolean = decodeParams["EncodedByteAlign"] as PdfBoolean;
			if (pdfBoolean.Value && decodeParams.ContainsKey("K") && (decodeParams["K"] as PdfNumber).IntValue < 0)
			{
				int intValue = (decodeParams["Columns"] as PdfNumber).IntValue;
				m_originalLineLength = intValue + 7 >> 3;
				m_bytesOnDemand = (int)Height * m_originalLineLength;
				m_outputStream = new DrawImagePixels(m_bytesOnDemand << 3);
				m_bitDataCount = (int)imageStream.Length << 3;
				m_bitData = EstimateBit(imageStream, m_bitDataCount);
				int num = 0;
				int[] array = new int[(int)Width + 1];
				int[] array2 = new int[(int)Width + 1];
				m_elementChanging = 2;
				array2[0] = (int)Width;
				array2[1] = (int)Width;
				int[] currentElementChange = new int[2];
				for (int i = 0; i < (int)Height; i++)
				{
					if (pdfBoolean.Value && m_bitArrived > 0)
					{
						int num2 = m_bitArrived % 8;
						int num3 = 8 - num2;
						if (num2 > 0)
						{
							m_bitArrived += num3;
						}
					}
					int[] array3 = array;
					array = array2;
					array2 = array3;
					FixTwoDimention(array, array2, m_elementChanging, currentElementChange);
					if (array2.Length != m_currentIndex)
					{
						m_relativeAddress = intValue;
						array2[m_currentIndex++] = m_relativeAddress;
					}
					m_elementChanging = m_currentIndex;
					num += m_originalLineLength;
				}
				byte[] encodedStream = GetEncodedStream();
				tiffDecode.m_stream.Write(encodedStream, 0, encodedStream.Length);
				flag = true;
			}
		}
		if (decodeParams.ContainsKey("EncodedByteAlign"))
		{
			if (!(decodeParams["EncodedByteAlign"] as PdfBoolean).Value || !flag)
			{
				tiffDecode.m_stream.Write(imageStream.ToArray(), 0, (int)imageStream.Length);
			}
		}
		else
		{
			tiffDecode.m_stream.Write(imageStream.ToArray(), 0, (int)imageStream.Length);
		}
		tiffDecode.SetField(1, offset2, TiffTag.ImageWidth, TiffType.Short);
		tiffDecode.SetField(1, offset3, TiffTag.ImageLength, TiffType.Short);
		tiffDecode.SetField(1, 1, TiffTag.BitsPerSample, TiffType.Short);
		if (decodeParams != null && decodeParams.ContainsKey("K"))
		{
			if ((decodeParams["K"] as PdfNumber).IntValue < 0)
			{
				if (decodeParams.ContainsKey("EncodedByteAlign") && (decodeParams["EncodedByteAlign"] as PdfBoolean).Value)
				{
					tiffDecode.SetField(1, 1, TiffTag.Compression, TiffType.Short);
				}
				else
				{
					tiffDecode.SetField(1, 4, TiffTag.Compression, TiffType.Short);
				}
			}
			else if ((decodeParams["K"] as PdfNumber).IntValue == 0)
			{
				if (decodeParams.ContainsKey("EndOfBlock"))
				{
					if ((decodeParams["EndOfBlock"] as PdfBoolean).Value)
					{
						tiffDecode.SetField(1, 2, TiffTag.Compression, TiffType.Short);
					}
					else
					{
						tiffDecode.SetField(1, 3, TiffTag.Compression, TiffType.Short);
					}
				}
				else
				{
					tiffDecode.SetField(1, 3, TiffTag.Compression, TiffType.Short);
				}
			}
			else
			{
				tiffDecode.SetField(1, 3, TiffTag.Compression, TiffType.Short);
			}
			if (decodeParams.ContainsKey("BlackIs1"))
			{
				if ((decodeParams["BlackIs1"] as PdfBoolean).Value)
				{
					if (pdfArray.Count != 0)
					{
						offset = (pdfArray[1] as PdfNumber).IntValue;
					}
					else if (ColorSpace == "Indexed" && m_colorspaceBase == "DeviceRGB" && decodeParams.ContainsKey("Columns") && decodeParams.ContainsKey("Rows"))
					{
						offset = 0;
					}
					tiffDecode.SetField(1, offset, TiffTag.Photometric, TiffType.Short);
				}
				else
				{
					tiffDecode.SetField(1, 0, TiffTag.Photometric, TiffType.Short);
				}
			}
			else if (pdfArray.Count != 0)
			{
				tiffDecode.SetField(1, offset, TiffTag.Photometric, TiffType.Short);
			}
			else if (IsImageMask && imageDictionary.ContainsKey("Decode"))
			{
				pdfArray = m_imageDictionary[new PdfName("Decode")] as PdfArray;
				tiffDecode.SetField(1, (pdfArray[0] as PdfNumber).IntValue, TiffTag.Photometric, TiffType.Short);
			}
		}
		else
		{
			tiffDecode.SetField(1, 3, TiffTag.Compression, TiffType.Short);
		}
		if (m_isBlackIs1)
		{
			tiffDecode.SetField(1, 1, TiffTag.Photometric, TiffType.Short);
		}
		tiffDecode.SetField(1, 8, TiffTag.StripOffset, TiffType.Long);
		tiffDecode.SetField(1, 1, TiffTag.SamplesPerPixel, TiffType.Short);
		tiffDecode.SetField(1, (int)imageStream.Length, TiffTag.StripByteCounts, TiffType.Long);
		tiffDecode.m_stream.Seek(9 + imageStream.Length, SeekOrigin.Begin);
		tiffDecode.WriteDirEntry(tiffDecode.directoryEntries);
		tiffDecode.m_stream.Position = 0L;
		tiffDecode.m_stream.Capacity = (int)tiffDecode.m_stream.Length;
		if (!imageDictionary.ContainsKey("ImageMask"))
		{
			imageStream = tiffDecode.m_stream;
			imageStream.Position = 0L;
		}
		else
		{
			m_isImageMask = (imageDictionary["ImageMask"] as PdfBoolean).Value;
			if (m_isImageMask)
			{
				m_isImageMasked = true;
				imageStream = tiffDecode.m_stream;
				imageStream.Position = 0L;
				m_isBlackIs1 = false;
				if (decodeParams.ContainsKey("BlackIs1") && imageDictionary.ContainsKey("Decode"))
				{
					if ((decodeParams["BlackIs1"] as PdfBoolean).Value && (pdfArray[0] as PdfNumber).IntValue == 1 && (pdfArray[1] as PdfNumber).IntValue == 0)
					{
						m_isBlackIs1 = false;
						imageStream.Position = 0L;
					}
				}
				else if (!decodeParams.ContainsKey("BlackIs1") || m_isMaskImage)
				{
					m_isBlackIs1 = false;
					imageStream.Position = 0L;
				}
				if (decodeParams.ContainsKey("BlackIs1") && !m_isMaskImage)
				{
					m_isBlackIs1 = true;
				}
			}
			else
			{
				imageStream = tiffDecode.m_stream;
				imageStream.Position = 0L;
			}
		}
		return imageStream;
	}

	private int GetPixelFormatSize(PixelFormat format)
	{
		int result = 1;
		if (PixelFormat.Format8bppIndexed == format)
		{
			result = 8;
		}
		else if (PixelFormat.Format16bppArgb1555 == format || PixelFormat.Format16bppGrayScale == format || PixelFormat.Format16bppRgb555 == format || PixelFormat.Format16bppRgb565 == format)
		{
			result = 16;
		}
		else if (PixelFormat.Format24bppRgb == format)
		{
			result = 24;
		}
		else if (PixelFormat.Format32bppArgb == format || PixelFormat.Format32bppPArgb == format || PixelFormat.Format32bppRgb == format)
		{
			result = 32;
		}
		return result;
	}

	internal MemoryStream GetEmbeddedImageStream()
	{
		try
		{
			if (m_decodedMemoryStream == null)
			{
				m_decodedMemoryStream = GetImageStream();
			}
			return m_decodedMemoryStream;
		}
		catch
		{
			return null;
		}
	}

	public MemoryStream GetImageStream()
	{
		m_isImageStreamParsed = true;
		bool flag = true;
		GetImageInterpolation(ImageDictionary);
		if (ImageFilter == null)
		{
			m_imageFilter = new string[1] { "FlateDecode" };
			flag = false;
		}
		if (ImageFilter != null)
		{
			for (int i = 0; i < ImageFilter.Length; i++)
			{
				if (ImageFilter.Length > 1)
				{
					isDualFilter = true;
				}
				switch (ImageFilter[i])
				{
				case "A85":
				case "ASCII85Decode":
					ImageStream = DecodeASCII85Stream(ImageStream);
					if (isDualFilter)
					{
						imageStreamBackup = ImageStream;
					}
					ImageStream.Position = 0L;
					if (ColorSpace == "DeviceGray")
					{
						MemoryStream memoryStream15 = new MemoryStream();
						ColorConvertor[] array38 = RenderGrayPixels(ImageStream.ToArray());
						int[] array39 = new int[array38.Length];
						for (int num93 = 0; num93 < array39.Length; num93++)
						{
							array39[num93] = array38[num93].PixelConversion();
						}
						m_embeddedImage = RenderImage(array39);
						m_embeddedImage.Save(memoryStream15, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
						ImageStream = memoryStream15;
					}
					break;
				case "ASCIIHex":
				{
					byte[] buffer4 = new ASCIIHex().Decode(ImageStream.GetBuffer());
					ImageStream = new MemoryStream(buffer4);
					imageFormat = ImageFormat.Png;
					ImageStream.Position = 0L;
					break;
				}
				case "RunLengthDecode":
				{
					ImageStream.Position = 0L;
					MemoryStream memoryStream10 = new MemoryStream();
					byte[] array23 = ImageStream.ToArray();
					int num54 = array23.Length;
					int num55;
					for (num55 = 0; num55 < num54; num55++)
					{
						int num56 = array23[num55];
						if (num56 < 0)
						{
							num56 = 256 + num56;
						}
						if (num56 == 128)
						{
							num55 = num54;
						}
						else if (num56 > 128)
						{
							num55++;
							num56 = 257 - num56;
							int num57 = array23[num55];
							byte[] array24 = new byte[num56];
							for (int num58 = 0; num58 < num56; num58++)
							{
								array24[num58] = (byte)num57;
							}
							memoryStream10.Write(array24, 0, array24.Length);
						}
						else
						{
							num55++;
							num56++;
							byte[] array25 = new byte[num56];
							for (int num59 = 0; num59 < num56; num59++)
							{
								int num57 = array23[num55 + num59];
								array25[num59] = (byte)num57;
							}
							memoryStream10.Write(array25, 0, array25.Length);
							num55 = num55 + num56 - 1;
						}
					}
					array23 = memoryStream10.GetBuffer();
					byte[] array26 = array23;
					if (ColorSpace == "DeviceRGB")
					{
						using (Bitmap bitmap10 = RenderImage(RenderRGBPixels(array26)))
						{
							if (ImageStream != null)
							{
								ImageStream.Dispose();
							}
							ImageStream = new MemoryStream();
							bitmap10.Save(ImageStream, ImageFormat.Png);
							imageFormat = ImageFormat.Png;
							ImageStream.Position = 0L;
						}
						break;
					}
					if (ColorSpace == "DeviceGray")
					{
						byte[] array27 = array23;
						array26 = array27;
						int num60 = (int)Height;
						int num61 = (int)Width;
						byte[] array28 = new byte[num61 * num60];
						int[] array29 = new int[8] { 1, 2, 4, 8, 16, 32, 64, 128 };
						int num62 = (int)Width + 7 >> 3;
						int num63 = 1;
						try
						{
							for (int num64 = 0; num64 < num60; num64++)
							{
								for (int num65 = 0; num65 < num61; num65++)
								{
									int num66 = 0;
									num54 = 0;
									int num67 = num63;
									int num68 = num63;
									int num69 = (int)Width - num65;
									int num70 = (int)Height - num64;
									if (num67 > num69)
									{
										num67 = num69;
									}
									if (num68 > num70)
									{
										num68 = num70;
									}
									for (int num71 = 0; num71 < num68; num71++)
									{
										for (int num72 = 0; num72 < num67; num72++)
										{
											if ((array27[(num71 + num64 * num63) * num62 + (num65 * num63 + num72 >> 3)] & array29[7 - ((num65 * num63 + num72) & 7)]) != 0)
											{
												num66++;
											}
											num54++;
										}
									}
									int num73 = num65 + num61 * num64;
									array28[num73] = (byte)(255 * num66 / num54);
								}
							}
						}
						catch
						{
						}
						array26 = array28;
						m_pixelFormat = PixelFormat.Format8bppIndexed;
						int num74 = 0;
						Bitmap bitmap11 = new Bitmap(new SKBitmap((int)Width, (int)Height, SKColorType.Gray8, SKAlphaType.Premul));
						nint num75 = bitmap11.m_sKBitmap.GetPixels();
						int num76 = (int)Width;
						for (int num77 = 0; (float)num77 < Height; num77++)
						{
							Marshal.Copy(array26, num74, num75, num76);
							num74 += num76;
							num75 += bitmap11.m_sKImageInfo.RowBytes;
						}
						MemoryStream memoryStream11 = new MemoryStream();
						bitmap11.Save(memoryStream11, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
						ImageStream = memoryStream11;
						ImageStream.Position = 0L;
						break;
					}
					if (ColorSpace == "Indexed")
					{
						new Bitmap((int)Width, (int)Height);
						MemoryStream memoryStream12 = new MemoryStream();
						byte[] indexedPixelData4 = GetIndexedPixelData(array26);
						RenderImage(indexedPixelData4).Save(memoryStream12, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
						ImageStream = memoryStream12;
						ImageStream.Position = 0L;
						break;
					}
					Bitmap bitmap12 = new Bitmap((int)Width, (int)Height);
					MemoryStream memoryStream13 = new MemoryStream();
					if (m_imageDictionary.ContainsKey("Decode") && ColorSpace == null)
					{
						bitmap12 = RenderImage(GetStencilMaskedPixels(array26, Width, Height));
						bitmap12.Save(memoryStream13, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
					}
					else
					{
						bitmap12 = new Bitmap((int)Width, (int)Height);
						int num78 = GetPixelFormatSize(m_pixelFormat) / 8;
						switch (num78)
						{
						case 3:
						{
							for (int num82 = 0; num82 + 3 < array26.Length; num82 += 3)
							{
								int num83 = num82 + 2;
								byte b2 = array26[num83];
								array26[num83] = array26[num82];
								array26[num82] = b2;
							}
							break;
						}
						case 4:
						{
							byte[] array30 = new byte[(int)(Width * Height * 3f)];
							int num79 = (int)(Width * Height * 4f);
							byte[] array31 = array26;
							int num80 = 0;
							int num81 = 0;
							for (; num80 < num79 + 4; num80 += 4)
							{
								array30[num81 + 2] = array31[num80];
								array30[num81 + 1] = array31[num80 + 1];
								array30[num81] = array31[num80 + 2];
								num81 += 3;
							}
							array26 = array30;
							break;
						}
						}
						if (Math.Abs(bitmap12.m_sKImageInfo.RowBytes) * bitmap12.Height < array26.Length)
						{
							int num84 = 0;
							nint num85 = bitmap12.m_sKBitmap.GetPixels();
							int num86 = (int)Width;
							if (num78 == 3)
							{
								num86 = (int)Width * 3;
							}
							for (int num87 = 0; (float)num87 < Height; num87++)
							{
								Marshal.Copy(array26, num84, num85, num86);
								num84 += num86;
								num85 += bitmap12.m_sKImageInfo.RowBytes;
							}
						}
						else
						{
							Marshal.Copy(array26, 0, bitmap12.m_sKBitmap.GetPixels(), array26.Length);
						}
						bitmap12.Save(memoryStream13, ImageFormat.Bmp);
						imageFormat = ImageFormat.Bmp;
					}
					if (!m_imageDictionary.ContainsKey("SMask"))
					{
						ImageStream = memoryStream13;
						ImageStream.Position = 0L;
						break;
					}
					try
					{
						ImageStream = MergeImages(bitmap12, MaskStream as MemoryStream, DTCdecode: false);
						m_isSoftMasked = true;
						ImageStream.Position = 0L;
					}
					catch (Exception)
					{
						ImageStream = memoryStream13;
						ImageStream.Position = 0L;
					}
					break;
				}
				case "DCTDecode":
					if (!m_imageDictionary.ContainsKey("SMask") && !m_imageDictionary.ContainsKey("Mask"))
					{
						ImageStream.Position = 0L;
						if ((!(ColorSpace == "DeviceCMYK") && !(ColorSpace == "DeviceN") && !(ColorSpace == "DeviceGray") && !(ColorSpace == "Separation") && !(ColorSpace == "DeviceRGB") && (!(ColorSpace == "ICCBased") || numberOfComponents != 4)) || (ColorSpace == "DeviceRGB" && (m_imageDictionary.ContainsKey("DecodeParms") || !m_imageDictionary.ContainsKey("Decode"))))
						{
							break;
						}
						if (m_imageDictionary.ContainsKey("Decode"))
						{
							PdfArray pdfArray7 = m_imageDictionary["Decode"] as PdfArray;
							double[] array32 = ((!(ColorSpace == "DeviceRGB")) ? new double[8] { 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0 } : new double[6] { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 });
							PdfArray pdfArray8 = new PdfArray(array32);
							bool flag5 = true;
							for (int num88 = 0; num88 < pdfArray8.Count; num88++)
							{
								if ((pdfArray7[num88] as PdfNumber).FloatValue != (pdfArray8[num88] as PdfNumber).FloatValue)
								{
									flag5 = false;
								}
							}
							if (flag5)
							{
								break;
							}
						}
						Bitmap bitmap14;
						if (isDualFilter && imageStreamBackup != null)
						{
							bitmap14 = Bitmap.FromStream(imageStreamBackup);
						}
						else
						{
							ImageStream.Position = 0L;
							SKBitmap sKBitmap2 = SKBitmap.Decode(ImageStream.ToArray());
							if (sKBitmap2 == null)
							{
								return ImageStream;
							}
							bitmap14 = new Bitmap(sKBitmap2);
						}
						nint pixels3 = bitmap14.m_sKBitmap.GetPixels();
						int num89 = Math.Abs(bitmap14.m_sKImageInfo.RowBytes) * bitmap14.Height;
						byte[] array33 = new byte[num89];
						Marshal.Copy(pixels3, array33, 0, num89);
						if (ColorSpace != "DeviceGray")
						{
							byte[] array34 = YCCKtoRGB(array33);
							Marshal.Copy(array34, 0, pixels3, array34.Length);
						}
						else
						{
							Marshal.Copy(array33, 0, pixels3, array33.Length);
						}
						ImageStream = new MemoryStream();
						bitmap14.Save(ImageStream, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
						ImageStream.Position = 0L;
						break;
					}
					try
					{
						Bitmap bitmap15 = null;
						try
						{
							if (m_imageDictionary.ContainsKey("Mask"))
							{
								IsTransparent = true;
							}
							ImageStream.Position = 0L;
							if (!(ColorSpace == "DeviceCMYK"))
							{
								goto IL_0d63;
							}
							if (!m_imageDictionary.ContainsKey("Decode"))
							{
								goto IL_0cd3;
							}
							PdfArray pdfArray9 = m_imageDictionary["Decode"] as PdfArray;
							PdfArray pdfArray10 = new PdfArray(new double[8] { 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0 });
							bool flag6 = true;
							for (int num90 = 0; num90 < pdfArray10.Count; num90++)
							{
								if ((pdfArray9[num90] as PdfNumber).FloatValue != (pdfArray10[num90] as PdfNumber).FloatValue)
								{
									flag6 = false;
								}
							}
							if (!flag6)
							{
								goto IL_0cd3;
							}
							goto end_IL_0c0a;
							IL_0d63:
							ImageStream.Position = 0L;
							bitmap15 = Bitmap.FromStream(ImageStream);
							goto IL_0d85;
							IL_0cd3:
							Bitmap bitmap16 = Bitmap.FromStream(ImageStream);
							nint pixels4 = bitmap16.m_sKBitmap.GetPixels();
							int num91 = Math.Abs(bitmap16.m_sKImageInfo.RowBytes) * bitmap16.Height;
							byte[] array35 = new byte[num91];
							Marshal.Copy(pixels4, array35, 0, num91);
							byte[] array36 = YCCKtoRGB(array35);
							Marshal.Copy(array36, 0, pixels4, array36.Length);
							ImageStream = new MemoryStream();
							bitmap16.Save(ImageStream, ImageFormat.Png);
							imageFormat = ImageFormat.Png;
							ImageStream.Position = 0L;
							goto IL_0d63;
							end_IL_0c0a:;
						}
						catch
						{
							bitmap15 = null;
							goto IL_0d85;
						}
						goto end_IL_0c07;
						IL_0d85:
						MaskStream.Position = 0L;
						PdfReferenceHolder pdfReferenceHolder2 = m_imageDictionary["SMask"] as PdfReferenceHolder;
						if (!(pdfReferenceHolder2 != null))
						{
							goto IL_0e8f;
						}
						m_isSoftMasked = true;
						if (!(pdfReferenceHolder2.Object is PdfStream pdfStream4))
						{
							goto IL_0e8f;
						}
						PdfDictionary pdfDictionary10 = pdfStream4;
						if (!pdfDictionary10.ContainsKey("Filter"))
						{
							goto IL_0e8f;
						}
						if (pdfDictionary10["Filter"] is PdfArray)
						{
							PdfArray pdfArray11 = pdfDictionary10["Filter"] as PdfArray;
							string[] array37 = new string[pdfArray11.Count];
							for (int num92 = 0; num92 < array37.Length; num92++)
							{
								array37[num92] = (pdfArray11[num92] as PdfName).Value;
							}
							goto IL_0e8f;
						}
						string value2 = (pdfDictionary10["Filter"] as PdfName).Value;
						if (!pdfDictionary10.ContainsKey("Decode") || !(value2 == "FlateDecode") || pdfDictionary10.ContainsKey("DecodeParms"))
						{
							goto IL_0e8f;
						}
						goto end_IL_0c07;
						IL_0e8f:
						ImageStream = MergeImages(bitmap15, MaskStream as MemoryStream, DTCdecode: true);
						if (!m_isSoftMasked)
						{
							m_isImageMasked = true;
						}
						ImageStream.Position = 0L;
						bitmap15?.Dispose();
						end_IL_0c07:;
					}
					catch
					{
						ImageStream.Position = 0L;
					}
					break;
				case "DCT":
					if (!m_imageDictionary.ContainsKey("SMask") && !m_imageDictionary.ContainsKey("Mask"))
					{
						ImageStream.Position = 0L;
						if ((!(ColorSpace == "DeviceCMYK") && !(ColorSpace == "DeviceGray") && !(ColorSpace == "Separation") && !(ColorSpace == "DeviceRGB") && (!(ColorSpace == "ICCBased") || numberOfComponents != 4)) || (ColorSpace == "DeviceRGB" && (m_imageDictionary.ContainsKey("DecodeParms") || !m_imageDictionary.ContainsKey("Decode"))))
						{
							break;
						}
						if (m_imageDictionary.ContainsKey("Decode"))
						{
							PdfArray pdfArray2 = m_imageDictionary["Decode"] as PdfArray;
							double[] array17 = ((!(ColorSpace == "DeviceRGB")) ? new double[8] { 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0 } : new double[6] { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 });
							PdfArray pdfArray3 = new PdfArray(array17);
							bool flag3 = true;
							for (int num49 = 0; num49 < pdfArray3.Count; num49++)
							{
								if ((pdfArray2[num49] as PdfNumber).FloatValue != (pdfArray3[num49] as PdfNumber).FloatValue)
								{
									flag3 = false;
								}
							}
							if (flag3)
							{
								break;
							}
						}
						Bitmap bitmap7 = Bitmap.FromStream((isDualFilter && imageStreamBackup != null) ? imageStreamBackup : ImageStream);
						nint pixels = bitmap7.m_sKBitmap.GetPixels();
						int num50 = Math.Abs(bitmap7.m_sKImageInfo.RowBytes) * bitmap7.Height;
						byte[] array18 = new byte[num50];
						Marshal.Copy(pixels, array18, 0, num50);
						if (ColorSpace != "DeviceGray")
						{
							byte[] array19 = YCCKtoRGB(array18);
							Marshal.Copy(array19, 0, pixels, array19.Length);
						}
						ImageStream = new MemoryStream();
						bitmap7.Save(ImageStream, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
						ImageStream.Position = 0L;
						break;
					}
					try
					{
						Bitmap bitmap8 = null;
						try
						{
							if (m_imageDictionary.ContainsKey("Mask"))
							{
								IsTransparent = true;
							}
							ImageStream.Position = 0L;
							if (!(ColorSpace == "DeviceCMYK"))
							{
								goto IL_129a;
							}
							if (!m_imageDictionary.ContainsKey("Decode"))
							{
								goto IL_120a;
							}
							PdfArray pdfArray4 = m_imageDictionary["Decode"] as PdfArray;
							PdfArray pdfArray5 = new PdfArray(new double[8] { 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0 });
							bool flag4 = true;
							for (int num51 = 0; num51 < pdfArray5.Count; num51++)
							{
								if ((pdfArray4[num51] as PdfNumber).FloatValue != (pdfArray5[num51] as PdfNumber).FloatValue)
								{
									flag4 = false;
								}
							}
							if (!flag4)
							{
								goto IL_120a;
							}
							goto end_IL_1141;
							IL_129a:
							ImageStream.Position = 0L;
							bitmap8 = Bitmap.FromStream(ImageStream);
							goto IL_12bc;
							IL_120a:
							Bitmap bitmap9 = Bitmap.FromStream(ImageStream);
							nint pixels2 = bitmap9.m_sKBitmap.GetPixels();
							int num52 = Math.Abs(bitmap9.m_sKImageInfo.RowBytes) * bitmap9.Height;
							byte[] array20 = new byte[num52];
							Marshal.Copy(pixels2, array20, 0, num52);
							byte[] array21 = YCCKtoRGB(array20);
							Marshal.Copy(array21, 0, pixels2, array21.Length);
							ImageStream = new MemoryStream();
							bitmap9.Save(ImageStream, ImageFormat.Png);
							imageFormat = ImageFormat.Png;
							ImageStream.Position = 0L;
							goto IL_129a;
							end_IL_1141:;
						}
						catch
						{
							bitmap8 = null;
							goto IL_12bc;
						}
						goto end_IL_113e;
						IL_12bc:
						MaskStream.Position = 0L;
						PdfReferenceHolder pdfReferenceHolder = m_imageDictionary["SMask"] as PdfReferenceHolder;
						if (!(pdfReferenceHolder != null))
						{
							goto IL_13a7;
						}
						m_isSoftMasked = true;
						if (!(pdfReferenceHolder.Object is PdfStream pdfStream3))
						{
							goto IL_13a7;
						}
						PdfDictionary pdfDictionary9 = pdfStream3;
						if (pdfDictionary9["Filter"] is PdfArray)
						{
							PdfArray pdfArray6 = pdfDictionary9["Filter"] as PdfArray;
							string[] array22 = new string[pdfArray6.Count];
							for (int num53 = 0; num53 < array22.Length; num53++)
							{
								array22[num53] = (pdfArray6[num53] as PdfName).Value;
							}
							goto IL_13a7;
						}
						string value = (pdfDictionary9["Filter"] as PdfName).Value;
						if (!pdfDictionary9.ContainsKey("Decode") || !(value == "FlateDecode"))
						{
							goto IL_13a7;
						}
						goto end_IL_113e;
						IL_13a7:
						ImageStream = MergeImages(bitmap8, MaskStream as MemoryStream, DTCdecode: true);
						if (!m_isSoftMasked)
						{
							m_isImageMasked = true;
						}
						ImageStream.Position = 0L;
						bitmap8?.Dispose();
						end_IL_113e:;
					}
					catch
					{
						ImageStream.Position = 0L;
					}
					break;
				case "FlateDecode":
				{
					int num15 = 0;
					int colors = 1;
					int columns = 1;
					int num16 = 0;
					if (flag)
					{
						outStream = DecodeFlateStream(ImageStream);
					}
					else
					{
						outStream = ImageStream;
					}
					_ = ColorSpace;
					byte[] array4 = null;
					if (IsImageMask && ColorSpace == null)
					{
						if (i != 0 || (ImageFilter.Length <= 1 && !IsImageForExtraction))
						{
							return null;
						}
						if ((double)GetBitsPerComponent() != 1.0)
						{
							return null;
						}
						ColorSpace = "DeviceGray";
					}
					if (colorSpaceResourceDict.Count > 0 && ColorSpace != "DeviceGray")
					{
						int num17 = 0;
						int w = 0;
						int h = 0;
						if (m_colorspace != "ICCBased")
						{
							isIndexedImage = true;
						}
						else
						{
							isIndexedImage = false;
						}
						if (m_imageDictionary.ContainsKey("BitsPerComponent"))
						{
							num17 = (m_imageDictionary["BitsPerComponent"] as PdfNumber).IntValue;
						}
						if (m_imageDictionary.ContainsKey("Width"))
						{
							w = (m_imageDictionary["Width"] as PdfNumber).IntValue;
						}
						if (m_imageDictionary.ContainsKey("Height"))
						{
							h = (m_imageDictionary["Height"] as PdfNumber).IntValue;
						}
						if (colorSpaceResourceDict.ContainsKey("DeviceCMYK") && m_colorspaceBase == "DeviceCMYK" && m_colorspace == "DeviceN" && internalColorSpace == string.Empty && m_imageDictionary.ContainsKey("SMask"))
						{
							byte[] index = ConvertIndexCMYKToRGB(colorSpaceResourceDict["DeviceCMYK"].GetBuffer());
							array4 = ConvertIndexedStreamToFlat(num17, w, h, outStream.GetBuffer(), index, isARGB: false, isDownsampled: false);
						}
						else if (colorSpaceResourceDict.ContainsKey("DeviceCMYK") || (m_colorspace == "Indexed" && internalColorSpace == "DeviceN" && m_colorspaceBase == "DeviceCMYK"))
						{
							byte[] index2 = ConvertIndexCMYKToRGB(colorSpaceResourceDict["Indexed"].GetBuffer());
							array4 = ConvertIndexedStreamToFlat(num17, w, h, outStream.GetBuffer(), index2, isARGB: false, isDownsampled: false);
						}
						else if (isDeviceN)
						{
							byte[] buffer2 = colorSpaceResourceDict["Indexed"].GetBuffer();
							byte[] array5 = new byte[768];
							int num18 = 0;
							_ = new float[1];
							_ = Color.Empty;
							int num19 = buffer2.Length;
							int num20 = 2;
							float[] array6 = new float[num20];
							for (int num21 = 0; num21 < num19; num21 += num20)
							{
								for (int num22 = 0; num22 < num20; num22++)
								{
									array6[num22] = (float)(buffer2[num21 + num22] & 0xFF) / 255f;
								}
								float num23 = array6[0];
								float num24 = 0f;
								float num25 = 0f;
								float num26 = array6[1];
								float num27 = 255f * (1f - num23) * (1f - num26);
								float num28 = 255f * (1f - num24) * (1f - num26);
								float num29 = 255f * (1f - num25) * (1f - num26);
								array5[num18] = (byte)num27;
								num18++;
								array5[num18] = (byte)num28;
								num18++;
								array5[num18] = (byte)num29;
								num18++;
							}
							array4 = ConvertIndexedStreamToFlat(num17, w, h, outStream.GetBuffer(), array5, isARGB: false, isDownsampled: false);
						}
						else if (colorSpaceResourceDict.ContainsKey("ICCBased") && m_colorspaceBase == "DeviceGray")
						{
							array4 = ConvertICCBasedStreamToFlat(num17, w, h, outStream.GetBuffer(), colorSpaceResourceDict["ICCBased"].GetBuffer(), isARGB: false, isDownsampled: false);
							if (num17 != 1)
							{
								Pixels = RenderRGBPixels(array4);
								m_embeddedImage = RenderImage(Pixels);
								m_decodedMemoryStream = new MemoryStream();
								m_embeddedImage.Save(m_decodedMemoryStream, ImageFormat.Png);
								imageFormat = ImageFormat.Png;
								ImageStream = m_decodedMemoryStream;
								return ImageStream;
							}
						}
						else if (m_colorspaceBase != "CalRGB")
						{
							array4 = ConvertIndexedStreamToFlat(num17, w, h, outStream.GetBuffer(), colorSpaceResourceDict["Indexed"].GetBuffer(), isARGB: false, isDownsampled: false);
							m_indexedRGBvalues = colorSpaceResourceDict["Indexed"].GetBuffer();
						}
					}
					if (ColorSpace == "DeviceGray" || ColorSpace == "CalGray")
					{
						outStream.Position = 0L;
						if (ImageFilter.Length > 1 && i == 0 && (ImageFilter[i + 1] == "DCTDecode" || ImageFilter[i + 1] == "RunLengthDecode" || ImageFilter[i + 1] == "CCITTFaxDecode"))
						{
							ImageStream = outStream;
							ImageStream.Position = 0L;
							break;
						}
						if (ImageDictionary.ContainsKey("SMask") && ImageDictionary["SMask"] as PdfReferenceHolder != null)
						{
							PdfStream pdfStream = (ImageDictionary["SMask"] as PdfReferenceHolder).Object as PdfStream;
							if (pdfStream.ContainsKey("BitsPerComponent"))
							{
								m_bitsPerComponent = (pdfStream["BitsPerComponent"] as PdfNumber).IntValue;
							}
							if (pdfStream.ContainsKey("Width"))
							{
								m_width = (pdfStream["Width"] as PdfNumber).IntValue;
							}
							if (pdfStream.ContainsKey("Height"))
							{
								m_height = (pdfStream["Height"] as PdfNumber).IntValue;
							}
						}
						ImageStream = DecodeDeviceGrayImage(outStream);
						if (m_imageDictionary.ContainsKey("SMask"))
						{
							Bitmap bitmap2 = new Bitmap(ImageStream);
							try
							{
								ImageStream = MergeImages(bitmap2, MaskStream as MemoryStream, DTCdecode: false);
								m_isSoftMasked = true;
							}
							catch (Exception)
							{
								ImageStream = outStream;
							}
							bitmap2.Dispose();
						}
						bool flag2 = false;
						if (m_imageDictionary.ContainsKey("DecodeParms"))
						{
							PdfDictionary pdfDictionary = new PdfDictionary();
							pdfDictionary = DecodeParam[i];
							if (pdfDictionary == null || pdfDictionary.Count <= 0)
							{
								break;
							}
							if (pdfDictionary.ContainsKey("Predictor"))
							{
								num15 = (pdfDictionary["Predictor"] as PdfNumber).IntValue;
							}
							if (pdfDictionary.ContainsKey("Columns"))
							{
								columns = (pdfDictionary["Columns"] as PdfNumber).IntValue;
							}
							if (pdfDictionary.ContainsKey("Colors"))
							{
								colors = (pdfDictionary["Colors"] as PdfNumber).IntValue;
							}
							if (pdfDictionary.ContainsKey("BitsPerComponent"))
							{
								num16 = (pdfDictionary["BitsPerComponent"] as PdfNumber).IntValue;
							}
							if (pdfDictionary.Count <= 0 || num15 == 0)
							{
								break;
							}
							if (m_colorspaceBase == "CalRGB" && ColorSpace == "Indexed")
							{
								byte[] array7 = CMYKPredictor(outStream.ToArray(), columns, colors, num16);
								MemoryStream memoryStream2 = new MemoryStream();
								array4 = array7;
								byte[] indexedPixelData = GetIndexedPixelData(array4);
								m_embeddedImage = RenderImage(indexedPixelData);
								m_embeddedImage.Save(memoryStream2, ImageFormat.Png);
								imageFormat = ImageFormat.Png;
								ImageStream = memoryStream2;
								break;
							}
							flag2 = true;
							array4 = DecodePredictor(num15, colors, columns, outStream).GetBuffer();
							Bitmap bitmap3 = new Bitmap((int)Width, (int)Height);
							if (!ImageDictionary.ContainsKey("Decode") && flag2 && ColorSpace == "DeviceGray" && m_colorspaceBase == null && !ImageDictionary.ContainsKey("SMask"))
							{
								new MemoryStream();
								ColorConvertor[] array8 = RenderGrayPixels(array4);
								int[] array9 = new int[array8.Length];
								for (int num30 = 0; num30 < array9.Length; num30++)
								{
									array9[num30] = array8[num30].PixelConversion();
								}
								bitmap3 = RenderImage(array9);
								flag2 = false;
							}
							else
							{
								bitmap3 = RenderImage(GetStencilMaskedPixels(array4, Width, Height));
							}
							array4 = null;
							if (IsTransparent)
							{
								bitmap3.MakeTransparent(Color.White);
							}
							MemoryStream memoryStream3 = new MemoryStream();
							bitmap3.Save(memoryStream3, ImageFormat.Png);
							imageFormat = ImageFormat.Png;
							ImageStream = memoryStream3;
							ImageStream.Position = 0L;
							bitmap3.Dispose();
						}
						else
						{
							ImageStream.Position = 0L;
						}
						break;
					}
					if (ImageFilter.Length > 1 && i == 0 && (ImageFilter[i + 1] == "DCTDecode" || ImageFilter[i + 1] == "RunLengthDecode" || (ImageFilter[i + 1] == "CCITTFaxDecode" && m_colorspace != "Indexed")))
					{
						ImageStream = new MemoryStream();
						ImageStream = outStream;
						ImageStream.Position = 0L;
						break;
					}
					if (m_colorspace == "ICCBased" && m_colorspaceBase == "DeviceGray" && nonIndexedImageColorResource != null && nonIndexedImageColorResource.Count > 0)
					{
						PdfDictionary pdfDictionary2 = nonIndexedImageColorResource["ICCBased"];
						PdfName pdfName = pdfDictionary2["Alternate"] as PdfName;
						if (pdfDictionary2["N"] is PdfNumber && (pdfDictionary2["N"] as PdfNumber).IntValue == 1)
						{
							int num31 = 0;
							if (m_imageDictionary.ContainsKey("BitsPerComponent"))
							{
								num31 = (m_imageDictionary["BitsPerComponent"] as PdfNumber).IntValue;
							}
							if (num31 == 8)
							{
								m_pixelFormat = PixelFormat.Format8bppIndexed;
							}
							PdfDictionary pdfDictionary3 = new PdfDictionary();
							if (pdfName == null && m_imageDictionary.ContainsKey("DecodeParms"))
							{
								pdfDictionary3 = DecodeParam[i];
								if (pdfDictionary3 != null)
								{
									if (pdfDictionary3.ContainsKey("Predictor"))
									{
										num15 = (pdfDictionary3["Predictor"] as PdfNumber).IntValue;
									}
									if (pdfDictionary3.ContainsKey("Columns"))
									{
										columns = (pdfDictionary3["Columns"] as PdfNumber).IntValue;
									}
									if (pdfDictionary3.ContainsKey("Colors"))
									{
										colors = (pdfDictionary3["Colors"] as PdfNumber).IntValue;
									}
									if (pdfDictionary3.ContainsKey("BitsPerComponent"))
									{
										num16 = (pdfDictionary3["BitsPerComponent"] as PdfNumber).IntValue;
									}
									if (pdfDictionary3.Count > 0)
									{
										array4 = DecodePredictor(num15, colors, columns, outStream).GetBuffer();
										ColorConvertor[] array10 = RenderGrayPixels(array4);
										int[] array11 = new int[array10.Length];
										for (int num32 = 0; num32 < array11.Length; num32++)
										{
											array11[num32] = array10[num32].PixelConversion();
										}
										m_embeddedImage = RenderImage(array11);
										m_decodedMemoryStream = new MemoryStream();
										m_embeddedImage.Save(m_decodedMemoryStream, ImageFormat.Png);
										imageFormat = ImageFormat.Png;
										ImageStream = m_decodedMemoryStream;
										return ImageStream;
									}
								}
							}
						}
					}
					if (!isIndexedImage && !m_isIccBasedAlternateDeviceGray)
					{
						if (nonIndexedImageColorResource != null && nonIndexedImageColorResource.Count > 0)
						{
							PdfDictionary pdfDictionary4 = nonIndexedImageColorResource["ICCBased"];
							if (pdfDictionary4["N"] is PdfNumber)
							{
								if ((pdfDictionary4["N"] as PdfNumber).IntValue == 1)
								{
									int num33 = 0;
									if (m_imageDictionary.ContainsKey("BitsPerComponent"))
									{
										num33 = (m_imageDictionary["BitsPerComponent"] as PdfNumber).IntValue;
									}
									if (num33 == 8)
									{
										m_pixelFormat = PixelFormat.Format8bppIndexed;
									}
									array4 = outStream.GetBuffer();
									for (int num34 = 0; num34 < array4.Length; num34++)
									{
										if (array4[num34] != 0 && array4[num34] != byte.MaxValue)
										{
											array4[num34] = 0;
										}
									}
								}
								else
								{
									array4 = outStream.GetBuffer();
								}
							}
							else
							{
								array4 = outStream.GetBuffer();
							}
						}
						else if (ColorSpace == "DeviceCMYK")
						{
							PdfDictionary pdfDictionary5 = new PdfDictionary();
							PdfName pdfName2 = null;
							if (m_imageDictionary.ContainsKey("Intent"))
							{
								pdfName2 = PdfCrossTable.Dereference(m_imageDictionary["Intent"]) as PdfName;
							}
							bool relativeColorimetric = pdfName2 != null && pdfName2.Value == "RelativeColorimetric";
							if (m_imageDictionary.ContainsKey("DecodeParms"))
							{
								MemoryStream memoryStream4 = new MemoryStream();
								pdfDictionary5 = DecodeParam[i];
								if (pdfDictionary5.ContainsKey("Predictor"))
								{
									num15 = (pdfDictionary5["Predictor"] as PdfNumber).IntValue;
								}
								if (pdfDictionary5.ContainsKey("Columns"))
								{
									columns = (pdfDictionary5["Columns"] as PdfNumber).IntValue;
								}
								if (pdfDictionary5.ContainsKey("Colors"))
								{
									colors = (pdfDictionary5["Colors"] as PdfNumber).IntValue;
								}
								num16 = ((!pdfDictionary5.ContainsKey("BitsPerComponent")) ? ((int)BitsPerComponent) : (pdfDictionary5["BitsPerComponent"] as PdfNumber).IntValue);
								if (num15 != 0)
								{
									if (m_colorspaceBase == null && num16 == 8)
									{
										array4 = DecodePredictor(num15, colors, columns, outStream).GetBuffer();
										Pixels = CMYKtoRGBPixels(array4);
									}
									else
									{
										byte[] encodedData = CMYKPredictor(outStream.GetBuffer(), columns, colors, num16);
										Pixels = CMYKtoRGBPixels(encodedData);
									}
									m_embeddedImage = RenderImage(Pixels);
									m_embeddedImage.Save(memoryStream4, ImageFormat.Png);
									imageFormat = ImageFormat.Png;
									ImageStream = memoryStream4;
									break;
								}
								array4 = YCCToRGB(outStream.GetBuffer(), relativeColorimetric);
								outStream = new MemoryStream(array4);
							}
							else
							{
								array4 = YCCToRGB(outStream.ToArray(), relativeColorimetric);
								outStream = new MemoryStream(array4);
							}
						}
						else
						{
							if (ColorSpace == "DeviceRGB" && !ImageDictionary.ContainsKey("Type") && !ImageDictionary.ContainsKey("SMask") && !ImageDictionary.ContainsKey("DecodeParms"))
							{
								array4 = outStream.ToArray();
								using (Bitmap bitmap4 = RenderImage(RenderRGBPixels(array4)))
								{
									ImageStream = new MemoryStream();
									bitmap4.Save(ImageStream, ImageFormat.Png);
									imageFormat = ImageFormat.Png;
									ImageStream.Position = 0L;
								}
								break;
							}
							array4 = outStream.ToArray();
							if (ImageDictionary.ContainsKey("SMask") && !ImageDictionary.ContainsKey("DecodeParms"))
							{
								MemoryStream memoryStream5 = new MemoryStream();
								Pixels = RenderRGBPixels(array4);
								m_embeddedImage = RenderImage(Pixels);
								m_embeddedImage.Save(memoryStream5, ImageFormat.Png);
								imageFormat = ImageFormat.Png;
								m_embeddedImage.Dispose();
								ImageStream = memoryStream5;
								m_embeddedImage.m_imageData = memoryStream5.ToArray();
								m_isSoftMasked = true;
								Pixels = null;
								break;
							}
						}
					}
					if (ImageDictionary.ContainsKey("Mask"))
					{
						IsTransparent = true;
						if (ImageDictionary["Mask"] is PdfArray)
						{
							PdfArray pdfArray = ImageDictionary["Mask"] as PdfArray;
							if (pdfArray != null && pdfArray.Count > 2)
							{
								IsTransparent = false;
							}
							int num35 = 0;
							while (num35 < pdfArray.Count)
							{
								int num36 = 0;
								int num37 = 0;
								if (pdfArray[num35] is PdfNumber)
								{
									num37 = (pdfArray[num35] as PdfNumber).IntValue;
									num35++;
								}
								if (pdfArray[num35] is PdfNumber)
								{
									num36 = (pdfArray[num35] as PdfNumber).IntValue;
									num35++;
								}
								for (int num38 = 0; num38 < array4.Length; num38++)
								{
									if (array4[num38] >= num37 && array4[num38] <= num36)
									{
										array4[num38] = byte.MaxValue;
									}
								}
							}
						}
					}
					if (ColorSpace == "Indexed" && !isDeviceN)
					{
						PdfDictionary pdfDictionary6 = new PdfDictionary();
						if (ImageDictionary.ContainsKey("DecodeParms"))
						{
							pdfDictionary6 = DecodeParam[i];
							if (pdfDictionary6 != null)
							{
								if (pdfDictionary6.ContainsKey("Predictor"))
								{
									num15 = (pdfDictionary6["Predictor"] as PdfNumber).IntValue;
								}
								if (pdfDictionary6.ContainsKey("Columns"))
								{
									columns = (pdfDictionary6["Columns"] as PdfNumber).IntValue;
								}
								if (pdfDictionary6.ContainsKey("Colors"))
								{
									colors = (pdfDictionary6["Colors"] as PdfNumber).IntValue;
								}
								num16 = ((!pdfDictionary6.ContainsKey("BitsPerComponent")) ? ((int)BitsPerComponent) : (pdfDictionary6["BitsPerComponent"] as PdfNumber).IntValue);
								if (pdfDictionary6.Count > 0)
								{
									byte[] buffer3 = CMYKPredictor(outStream.ToArray(), columns, colors, num16);
									outStream = new MemoryStream(buffer3);
								}
							}
						}
						if (flag)
						{
							byte[] indexedPixelData2 = GetIndexedPixelData(outStream.ToArray());
							using Bitmap bitmap5 = RenderImage(indexedPixelData2);
							if (!m_imageDictionary.ContainsKey("SMask"))
							{
								if (m_imageDictionary.ContainsKey("Mask"))
								{
									bitmap5.MakeTransparent(Color.White);
								}
								MemoryStream memoryStream6 = new MemoryStream();
								bitmap5.Save(memoryStream6, ImageFormat.Png);
								imageFormat = ImageFormat.Png;
								ImageStream = memoryStream6;
								ImageStream.Position = 0L;
								indexedPixelData2 = null;
								Pixels = null;
								break;
							}
							if (m_imageDictionary.ContainsKey("SMask"))
							{
								ImageStream = MergeImages(bitmap5, MaskStream as MemoryStream, DTCdecode: false);
								m_decodedMemoryStream = ImageStream;
								return ImageStream;
							}
						}
						else if (m_imageDictionary.ContainsKey("SMask"))
						{
							PdfStream pdfStream2 = (ImageDictionary["SMask"] as PdfReferenceHolder).Object as PdfStream;
							if (pdfStream2.ContainsKey("BitsPerComponent"))
							{
								m_bitsPerComponent = (pdfStream2["BitsPerComponent"] as PdfNumber).IntValue;
							}
							if (pdfStream2.ContainsKey("Width"))
							{
								m_width = (pdfStream2["Width"] as PdfNumber).IntValue;
							}
							if (pdfStream2.ContainsKey("Height"))
							{
								m_height = (pdfStream2["Height"] as PdfNumber).IntValue;
							}
							if (pdfStream2.ContainsKey("ColorSpace"))
							{
								m_colorspace = (pdfStream2["ColorSpace"] as PdfName).Value;
							}
							array4 = pdfStream2.Data;
							MemoryStream memoryStream7 = new MemoryStream();
							Pixels = RenderRGBPixels(array4);
							m_embeddedImage = RenderImage(Pixels);
							m_embeddedImage.Save(memoryStream7, ImageFormat.Png);
							imageFormat = ImageFormat.Png;
							ImageStream = memoryStream7;
							m_isSoftMasked = true;
							break;
						}
					}
					if ((ColorSpace == "DeviceN" || ColorSpace == "Separation") && (!colorSpaceResourceDict.ContainsKey("DeviceCMYK") || !(m_colorspaceBase == "DeviceCMYK") || !(m_colorspace == "DeviceN")))
					{
						if (ColorSpace == "Separation" && m_imageDictionary.ContainsKey("DecodeParms"))
						{
							PdfDictionary pdfDictionary7 = DecodeParam[i];
							if (pdfDictionary7 != null)
							{
								if (pdfDictionary7.ContainsKey("Predictor"))
								{
									num15 = (pdfDictionary7["Predictor"] as PdfNumber).IntValue;
								}
								if (pdfDictionary7.ContainsKey("Columns"))
								{
									columns = (pdfDictionary7["Columns"] as PdfNumber).IntValue;
								}
								if (pdfDictionary7.ContainsKey("Colors"))
								{
									colors = (pdfDictionary7["Colors"] as PdfNumber).IntValue;
								}
								if (pdfDictionary7.ContainsKey("BitsPerComponent"))
								{
									num16 = (pdfDictionary7["BitsPerComponent"] as PdfNumber).IntValue;
								}
								if (pdfDictionary7.Count > 0)
								{
									array4 = DecodePredictor(num15, colors, columns, outStream).ToArray();
								}
							}
						}
						array4 = GetDeviceNData(array4);
					}
					PdfDictionary pdfDictionary8 = new PdfDictionary();
					int num39 = GetPixelFormatSize(m_pixelFormat) / 8;
					if (ColorSpace != "Separation" && m_imageDictionary.ContainsKey("DecodeParms"))
					{
						pdfDictionary8 = DecodeParam[i];
						if (pdfDictionary8 != null)
						{
							if (pdfDictionary8.ContainsKey("Predictor"))
							{
								num15 = (pdfDictionary8["Predictor"] as PdfNumber).IntValue;
							}
							if (pdfDictionary8.ContainsKey("Columns"))
							{
								columns = (pdfDictionary8["Columns"] as PdfNumber).IntValue;
							}
							if (pdfDictionary8.ContainsKey("Colors"))
							{
								colors = (pdfDictionary8["Colors"] as PdfNumber).IntValue;
							}
							if (pdfDictionary8.ContainsKey("BitsPerComponent"))
							{
								num16 = (pdfDictionary8["BitsPerComponent"] as PdfNumber).IntValue;
							}
							if (pdfDictionary8.Count > 0)
							{
								if (m_colorspaceBase == "CalRGB" && ColorSpace == "Indexed")
								{
									byte[] array12 = CMYKPredictor(outStream.ToArray(), columns, colors, num16);
									MemoryStream memoryStream8 = new MemoryStream();
									array4 = array12;
									byte[] indexedPixelData3 = GetIndexedPixelData(array4);
									m_embeddedImage = RenderImage(indexedPixelData3);
									m_embeddedImage.Save(memoryStream8, ImageFormat.Png);
									imageFormat = ImageFormat.Png;
									ImageStream = memoryStream8;
									break;
								}
								array4 = DecodePredictor(num15, colors, columns, outStream).GetBuffer();
							}
						}
					}
					Bitmap bitmap6;
					if (m_pixelFormat == PixelFormat.Format24bppRgb)
					{
						if (num16 == 0 && m_imageDictionary.ContainsKey("BitsPerComponent") && m_imageDictionary["BitsPerComponent"] is PdfNumber pdfNumber)
						{
							num16 = pdfNumber.IntValue;
						}
						bitmap6 = ((num16 != 8) ? new Bitmap(new SKBitmap((int)Width, (int)Height, SKColorType.Rgb888x, SKAlphaType.Premul)) : new Bitmap((int)Width, (int)Height));
					}
					else
					{
						bitmap6 = new Bitmap((int)Width, (int)Height);
					}
					switch (num39)
					{
					case 3:
					{
						for (int num43 = 0; num43 + 3 <= array4.Length; num43 += 3)
						{
							int num44 = num43 + 2;
							byte b = array4[num44];
							array4[num44] = array4[num43];
							array4[num43] = b;
						}
						break;
					}
					case 4:
					{
						byte[] array13 = new byte[(int)(Width * Height * 3f)];
						int num40 = (int)(Width * Height * 4f);
						byte[] array14 = array4;
						int num41 = 0;
						int num42 = 0;
						for (; num41 < num40 + 4; num41 += 4)
						{
							array13[num42 + 2] = array14[num41];
							array13[num42 + 1] = array14[num41 + 1];
							array13[num42] = array14[num41 + 2];
							num42 += 3;
						}
						array4 = array13;
						break;
					}
					}
					int num45 = 0;
					int num46 = (int)Width;
					if (num39 == 3)
					{
						num46 = (int)Width * 3;
					}
					nint num47 = bitmap6.m_sKBitmap.GetPixels();
					for (int num48 = 0; (float)num48 < Height; num48++)
					{
						if (m_pixelFormat == PixelFormat.Format24bppRgb)
						{
							byte[] array15 = new byte[num46];
							Buffer.BlockCopy(array4, num45, array15, 0, num46);
							byte[] array16 = ApplyAlpha(array15);
							Marshal.Copy(array16, 0, num47, array16.Length);
						}
						else
						{
							Marshal.Copy(array4, num45, num47, num46);
						}
						num45 += num46;
						num47 += bitmap6.m_sKBitmap.RowBytes;
					}
					if (IsTransparent)
					{
						bitmap6.MakeTransparent(Color.White);
					}
					MemoryStream memoryStream9 = new MemoryStream();
					bitmap6.Save(memoryStream9, ImageFormat.Png);
					imageFormat = ImageFormat.Png;
					if (!m_imageDictionary.ContainsKey("SMask"))
					{
						ImageStream = memoryStream9;
						ImageStream.Position = 0L;
					}
					else
					{
						try
						{
							ImageStream = MergeImages(bitmap6, MaskStream as MemoryStream, DTCdecode: false);
							m_isSoftMasked = true;
							ImageStream.Position = 0L;
						}
						catch (Exception)
						{
							ImageStream = memoryStream9;
							ImageStream.Position = 0L;
						}
					}
					bitmap6.Dispose();
					break;
				}
				case "CCITTFaxDecode":
				{
					m_isCCITTFaxDecode = true;
					if (ColorSpace == "Indexed" && m_colorspaceBase == "DeviceRGB")
					{
						m_isBlackIs1 = true;
					}
					PdfDictionary decodeParams = new PdfDictionary();
					if (m_imageDictionary.ContainsKey("DecodeParms"))
					{
						decodeParams = DecodeParam[i];
					}
					if (isDualFilter && imageStreamBackup != null)
					{
						ImageStream = DecodeCCITTFaxDecodeStream(imageStreamBackup, m_imageDictionary, decodeParams);
					}
					else
					{
						ImageStream = DecodeCCITTFaxDecodeStream(ImageStream, m_imageDictionary, decodeParams);
					}
					imageFormat = ImageFormat.Tiff;
					byte[] buffer = ImageStream.GetBuffer();
					_ = ColorSpace;
					if (!m_isIccBasedAlternateDeviceGray)
					{
						break;
					}
					byte[] array = buffer;
					int num = (int)Height;
					int num2 = (int)Width;
					byte[] array2 = new byte[num2 * num];
					int[] array3 = new int[8] { 1, 2, 4, 8, 16, 32, 64, 128 };
					int num3 = (int)Width + 7 >> 3;
					int num4 = 1;
					try
					{
						for (int j = 0; j < num; j++)
						{
							for (int k = 0; k < num2; k++)
							{
								int num5 = 0;
								int num6 = 0;
								int num7 = num4;
								int num8 = num4;
								int num9 = (int)Width - k;
								int num10 = (int)Height - j;
								if (num7 > num9)
								{
									num7 = num9;
								}
								if (num8 > num10)
								{
									num8 = num10;
								}
								for (int l = 0; l < num8; l++)
								{
									for (int m = 0; m < num7; m++)
									{
										if ((array[(l + j * num4) * num3 + (k * num4 + m >> 3)] & array3[7 - ((k * num4 + m) & 7)]) != 0)
										{
											num5++;
										}
										num6++;
									}
								}
								int num11 = k + num2 * j;
								array2[num11] = (byte)(255 * num5 / num6);
							}
						}
					}
					catch
					{
					}
					buffer = array2;
					int num12 = 0;
					int num13 = (int)Width;
					m_pixelFormat = PixelFormat.Format8bppIndexed;
					SKBitmap sKBitmap = new SKBitmap((int)Width, (int)Height, SKColorType.Gray8, SKAlphaType.Premul);
					Bitmap bitmap = new Bitmap(sKBitmap);
					nint num14 = sKBitmap.GetPixels();
					for (int n = 0; (float)n < Height; n++)
					{
						Marshal.Copy(buffer, num12, num14, num13);
						num12 += num13;
						num14 += sKBitmap.RowBytes;
					}
					MemoryStream memoryStream = new MemoryStream();
					bitmap.Save(memoryStream, ImageFormat.Png);
					ImageStream = memoryStream;
					ImageStream.Position = 0L;
					break;
				}
				case "JBIG2Decode":
				{
					Bitmap bitmap17 = DecodeJBIG2EncodedStream(ImageStream);
					MemoryStream memoryStream16 = new MemoryStream();
					if (ImageDictionary.ContainsKey("ImageMask") && !IsImageForExtraction)
					{
						if (ImageDictionary.ContainsKey("NuanRGB"))
						{
							bitmap17.MakeTransparent(Color.FromArgb(255, 255, 255, 255));
						}
						else
						{
							bitmap17.MakeTransparent(Color.White);
						}
					}
					bitmap17.Save(memoryStream16, ImageFormat.Png);
					imageFormat = ImageFormat.Png;
					memoryStream16.Position = 0L;
					ImageStream = memoryStream16;
					ImageStream.Position = 0L;
					bitmap17.Dispose();
					break;
				}
				case "LZWDecode":
				{
					int num94 = 0;
					int colors2 = 1;
					int columns2 = 1;
					int num95 = 0;
					PdfLzwCompressor pdfLzwCompressor = new PdfLzwCompressor();
					new MemoryStream();
					byte[] array40;
					if (isDualFilter && imageStreamBackup != null)
					{
						array40 = new byte[imageStreamBackup.Length];
						inputData = imageStreamBackup.GetBuffer();
					}
					else
					{
						array40 = new byte[ImageStream.Length];
					}
					if (inputData == null)
					{
						inputData = ImageStream.GetBuffer();
					}
					array40 = pdfLzwCompressor.Decompress(inputData, IsEarlyChange);
					if (isDualFilter)
					{
						MemoryStream memoryStream17 = new MemoryStream();
						memoryStream17.Write(array40, 0, array40.Length);
						imageStreamBackup = memoryStream17;
					}
					if (ColorSpace == "Indexed")
					{
						MemoryStream memoryStream18 = new MemoryStream();
						PdfDictionary pdfDictionary11 = new PdfDictionary();
						if (ImageDictionary.ContainsKey("DecodeParms"))
						{
							pdfDictionary11 = DecodeParam[i];
							if (pdfDictionary11 != null)
							{
								if (pdfDictionary11.ContainsKey("Predictor"))
								{
									num94 = (pdfDictionary11["Predictor"] as PdfNumber).IntValue;
								}
								if (pdfDictionary11.ContainsKey("Columns"))
								{
									columns2 = (pdfDictionary11["Columns"] as PdfNumber).IntValue;
								}
								if (pdfDictionary11.ContainsKey("Colors"))
								{
									colors2 = (pdfDictionary11["Colors"] as PdfNumber).IntValue;
								}
								num95 = ((!pdfDictionary11.ContainsKey("BitsPerComponent")) ? ((int)BitsPerComponent) : (pdfDictionary11["BitsPerComponent"] as PdfNumber).IntValue);
								if (pdfDictionary11.Count > 0)
								{
									byte[] buffer5 = CMYKPredictor(outStream.ToArray(), columns2, colors2, num95);
									outStream = new MemoryStream(buffer5);
								}
							}
						}
						byte[] indexedPixelData5 = GetIndexedPixelData(array40);
						Bitmap bitmap18 = RenderImage(indexedPixelData5);
						if (!m_imageDictionary.ContainsKey("SMask"))
						{
							if (ImageDictionary.ContainsKey("Intent") && (ImageDictionary["Intent"] as PdfName).Value == "Perceptual")
							{
								bitmap18.MakeTransparent(Color.White);
							}
							if (m_imageDictionary.ContainsKey("Mask") && !(m_imageDictionary["Mask"] is PdfArray))
							{
								bitmap18.MakeTransparent(Color.White);
							}
							bitmap18.Save(memoryStream18, ImageFormat.Png);
							imageFormat = ImageFormat.Png;
							ImageStream = memoryStream18;
							ImageStream.Position = 0L;
						}
						else
						{
							try
							{
								ImageStream = MergeImages(bitmap18, MaskStream as MemoryStream, DTCdecode: false);
								m_isSoftMasked = true;
								ImageStream.Position = 0L;
							}
							catch (Exception)
							{
								bitmap18.Save(memoryStream18, ImageFormat.Png);
								imageFormat = ImageFormat.Png;
								ImageStream = memoryStream18;
								ImageStream.Position = 0L;
							}
						}
					}
					else if (ColorSpace == "DeviceRGB")
					{
						PdfDictionary pdfDictionary12 = new PdfDictionary();
						if (m_imageDictionary.ContainsKey("DecodeParms"))
						{
							pdfDictionary12 = DecodeParam[i];
							if (pdfDictionary12.ContainsKey("Predictor"))
							{
								num94 = (pdfDictionary12["Predictor"] as PdfNumber).IntValue;
							}
							if (pdfDictionary12.ContainsKey("Columns"))
							{
								columns2 = (pdfDictionary12["Columns"] as PdfNumber).IntValue;
							}
							if (pdfDictionary12.ContainsKey("Colors"))
							{
								colors2 = (pdfDictionary12["Colors"] as PdfNumber).IntValue;
							}
							num95 = ((!pdfDictionary12.ContainsKey("BitsPerComponent")) ? ((int)BitsPerComponent) : (pdfDictionary12["BitsPerComponent"] as PdfNumber).IntValue);
							if (num94 != 0 && m_colorspaceBase == null && num95 == 8)
							{
								MemoryStream memoryStream19 = new MemoryStream();
								memoryStream19.Write(array40, 0, array40.Length);
								array40 = DecodePredictor(num94, colors2, columns2, memoryStream19).GetBuffer();
							}
						}
						MemoryStream memoryStream20 = new MemoryStream();
						Pixels = RenderRGBPixels(array40);
						m_embeddedImage = RenderImage(Pixels);
						m_embeddedImage.Save(memoryStream20, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
						ImageStream = memoryStream20;
					}
					else if (ColorSpace == "DeviceGray")
					{
						MemoryStream memoryStream21 = new MemoryStream();
						ColorConvertor[] array41 = RenderGrayPixels(array40);
						int[] array42 = new int[array41.Length];
						for (int num96 = 0; num96 < array42.Length; num96++)
						{
							array42[num96] = array41[num96].PixelConversion();
						}
						m_embeddedImage = RenderImage(array42);
						m_embeddedImage.Save(memoryStream21, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
						ImageStream = memoryStream21;
					}
					else if (ColorSpace == "DeviceCMYK")
					{
						MemoryStream memoryStream22 = new MemoryStream();
						Pixels = CMYKtoRGBPixels(array40);
						m_embeddedImage = RenderImage(Pixels);
						m_embeddedImage.Save(memoryStream22, ImageFormat.Png);
						imageFormat = ImageFormat.Png;
						ImageStream = memoryStream22;
					}
					break;
				}
				case "JPXDecode":
				{
					ImageStream.Position = 0L;
					JPXImage jPXImage = new JPXImage();
					MemoryStream memoryStream14 = new MemoryStream();
					Bitmap bitmap13 = jPXImage.FromStreamNet(ImageStream);
					bitmap13.Save(memoryStream14, ImageFormat.Png);
					imageFormat = ImageFormat.Png;
					m_isJPXDecode = true;
					if (m_imageDictionary.ContainsKey("SMask") || m_imageDictionary.ContainsKey("Mask"))
					{
						MaskStream.Position = 0L;
						ImageStream = MergeImages(bitmap13, MaskStream as MemoryStream, DTCdecode: false);
						m_isSoftMasked = true;
						bitmap13.Dispose();
						bitmap13 = null;
						m_decodedMemoryStream = ImageStream;
						return ImageStream;
					}
					bitmap13.Dispose();
					m_decodedMemoryStream = ImageStream;
					return memoryStream14;
				}
				default:
					if (string.IsNullOrEmpty(ImageFilter[i]))
					{
						throw new Exception("Error in identifying ImageFilter");
					}
					throw new Exception(ImageFilter?.ToString() + " does not supported");
				}
			}
			m_imageFilter = null;
			m_decodedMemoryStream = ImageStream;
			return ImageStream;
		}
		m_decodedMemoryStream = null;
		return null;
	}

	private byte[] ApplyAlpha(byte[] input)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < input.Length; i += 3)
		{
			SKColor sKColor = new SKColor(input[i], input[i + 1], input[i + 2]);
			list.Add(sKColor.Red);
			list.Add(sKColor.Green);
			list.Add(sKColor.Blue);
			list.Add(sKColor.Alpha);
		}
		return list.ToArray();
	}

	private byte[] CMYKPredictor(byte[] data, int columns, int colors, int bitsPerComponent)
	{
		List<byte[]> list = new List<byte[]>();
		int num = columns * colors * bitsPerComponent;
		num = (int)Math.Ceiling((double)num / 8.0);
		int sub = (int)Math.Ceiling((double)(bitsPerComponent * colors) / 8.0);
		byte[] prevLine = null;
		ByteStreamRenderer byteStreamRenderer = new ByteStreamRenderer(data);
		while (byteStreamRenderer.Remaining >= num + 1)
		{
			int num2 = byteStreamRenderer.ReadByte() & 0xFF;
			byte[] array = new byte[num];
			byteStreamRenderer.Read(array);
			switch (num2)
			{
			case 1:
				Sub(array, sub);
				break;
			case 2:
				Up(array, prevLine);
				break;
			case 3:
				Average(array, prevLine, sub);
				break;
			case 4:
				Paeth(array, prevLine, sub);
				break;
			}
			list.Add(array);
			prevLine = array;
		}
		List<byte> list2 = new List<byte>();
		foreach (byte[] item in list)
		{
			list2.AddRange(item);
		}
		return list2.ToArray();
	}

	private Bitmap RenderImage(int[] pixelArray)
	{
		SKBitmap sKBitmap = new SKBitmap((int)Width, (int)Height, SKColorType.Rgba8888, SKAlphaType.Premul);
		Bitmap bitmap = new Bitmap(sKBitmap);
		int rowBytes = bitmap.m_sKImageInfo.RowBytes;
		byte[] array = new byte[rowBytes * bitmap.Height];
		for (int i = 0; i < pixelArray.Length; i++)
		{
			Color color = Color.FromArgb(pixelArray[i]);
			int num = i % (int)Width;
			int num2 = i / (int)Width;
			array[rowBytes * num2 + 4 * num] = color.R;
			array[rowBytes * num2 + 4 * num + 1] = color.G;
			array[rowBytes * num2 + 4 * num + 2] = color.B;
			array[rowBytes * num2 + 4 * num + 3] = color.A;
		}
		Marshal.Copy(array, 0, sKBitmap.GetPixels(), array.Length);
		array = null;
		return bitmap;
	}

	private Bitmap RenderImage(byte[] pixelArray)
	{
		SKBitmap sKBitmap = new SKBitmap((int)Width, (int)Height, SKColorType.Rgba8888, SKAlphaType.Premul);
		Bitmap bitmap = new Bitmap(sKBitmap);
		int rowBytes = bitmap.m_sKImageInfo.RowBytes;
		byte[] array = new byte[rowBytes * bitmap.Height];
		long num = 0L;
		for (int i = 0; i < pixelArray.Length / 4; i++)
		{
			int num2 = i % (int)Width;
			int num3 = i / (int)Width;
			array[rowBytes * num3 + 4 * num2] = pixelArray[num + 1];
			array[rowBytes * num3 + 4 * num2 + 1] = pixelArray[num + 2];
			array[rowBytes * num3 + 4 * num2 + 2] = pixelArray[num + 3];
			array[rowBytes * num3 + 4 * num2 + 3] = pixelArray[num];
			num += 4;
		}
		Marshal.Copy(array, 0, sKBitmap.GetPixels(), array.Length);
		array = null;
		return bitmap;
	}

	private byte[] GetDeviceNData(byte[] data)
	{
		byte[] array = new byte[3 * (int)Width * (int)Height];
		int num = 0;
		_ = new float[1];
		_ = Color.Empty;
		int num2 = data.Length;
		int num3 = 1;
		float[] array2 = new float[num3];
		for (int i = 0; i < num2; i += num3)
		{
			for (int j = 0; j < num3; j++)
			{
				array2[j] = (float)(data[i + j] & 0xFF) / 255f;
			}
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = array2[0];
			float num8 = 255f * (1f - num4) * (1f - num7);
			float num9 = 255f * (1f - num5) * (1f - num7);
			float num10 = 255f * (1f - num6) * (1f - num7);
			array[num] = (byte)num8;
			num++;
			array[num] = (byte)num9;
			num++;
			array[num] = (byte)num10;
			num++;
		}
		return array;
	}

	private byte[] ConvertIndexCMYKToRGB(byte[] data)
	{
		byte[] array = null;
		int num = data.Length;
		array = new byte[num * 3 / 4];
		int num2 = 0;
		for (int i = 0; i < num; i += 4)
		{
			float[] array2 = new float[4];
			for (int j = 0; j < 4; j++)
			{
				array2[j] = (float)(data[i + j] & 0xFF) / 255f;
			}
			float[] array3 = ConvertCMYKToRGB(array2);
			array[num2] = (byte)(int)array3[0];
			array[num2 + 1] = (byte)(int)array3[1];
			array[num2 + 2] = (byte)(int)array3[2];
			num2 += 3;
			if (num - 4 - i < 4)
			{
				i = num;
			}
		}
		return array;
	}

	private float[] ConvertCMYKToRGB(float[] values)
	{
		float num = values[0];
		float num2 = values[1];
		float num3 = values[2];
		float num4 = values[3];
		float num5 = 255f * (1f - num) * (1f - num4);
		float num6 = 255f * (1f - num2) * (1f - num4);
		float num7 = 255f * (1f - num3) * (1f - num4);
		return new float[3] { num5, num6, num7 };
	}

	private void GetColorSpace()
	{
		if (!m_imageDictionary.ContainsKey("ColorSpace"))
		{
			return;
		}
		string[] filter = null;
		internalColorSpace = string.Empty;
		PdfDictionary colorspaceDictionary = null;
		PdfArray pdfArray = null;
		PdfArray pdfArray2 = null;
		if (m_imageDictionary["ColorSpace"] is PdfArray)
		{
			pdfArray = m_imageDictionary["ColorSpace"] as PdfArray;
		}
		if (m_imageDictionary["ColorSpace"] is PdfReferenceHolder)
		{
			if ((m_imageDictionary["ColorSpace"] as PdfReferenceHolder).Object is PdfArray)
			{
				pdfArray = (m_imageDictionary["ColorSpace"] as PdfReferenceHolder).Object as PdfArray;
			}
			else if ((m_imageDictionary["ColorSpace"] as PdfReferenceHolder).Object.ToString() != null)
			{
				string text = (m_imageDictionary["ColorSpace"] as PdfReferenceHolder).Object.ToString();
				for (int i = 1; i < text.Length; i++)
				{
					m_colorspace += text[i];
				}
			}
		}
		if (m_imageDictionary["ColorSpace"] is PdfName)
		{
			m_colorspace = (m_imageDictionary["ColorSpace"] as PdfName).Value;
		}
		if (pdfArray == null)
		{
			return;
		}
		m_colorspace = (pdfArray[0] as PdfName).Value;
		pdfArray2 = pdfArray;
		if (pdfArray.Count == 4)
		{
			bool flag = false;
			string[] imageFilter = ImageFilter;
			for (int j = 0; j < imageFilter.Length; j++)
			{
				if (imageFilter[j] == "RunLengthDecode")
				{
					flag = true;
				}
			}
			if ((pdfArray2[0] as PdfName).Value == "Indexed" && pdfArray2[pdfArray2.Count - 1].GetType().Name == "PdfReferenceHolder")
			{
				try
				{
					if (((pdfArray2[pdfArray2.Count - 1] as PdfReferenceHolder).Object as PdfDictionary).Values.Count > 1)
					{
						PdfName pdfName = new PdfName();
						MemoryStream internalStream = ((pdfArray2[pdfArray2.Count - 1] as PdfReferenceHolder).Object as PdfStream).InternalStream;
						if (((pdfArray2[pdfArray2.Count - 1] as PdfReferenceHolder).Object as PdfStream).ContainsKey("Filter"))
						{
							pdfName = ((pdfArray2[pdfArray2.Count - 1] as PdfReferenceHolder).Object as PdfStream)["Filter"] as PdfName;
						}
						internalStream = ((pdfName != null && pdfName.Value == "ASCII85Decode") ? (m_colorspaceStream = DecodeASCII85Stream(internalStream)) : (m_colorspaceStream = DecodeFlateStream(internalStream)));
						colorSpaceResourceDict.Add("Indexed", internalStream);
						isIndexedImage = true;
					}
					else
					{
						MemoryStream internalStream = ((pdfArray2[pdfArray2.Count - 1] as PdfReferenceHolder).Object as PdfStream).InternalStream;
						colorSpaceResourceDict.Add("Indexed", internalStream);
						m_colorspaceStream = internalStream;
						if (ImageDictionary.ContainsKey("DecodeParms") || flag)
						{
							GetIndexedColorSpace(pdfArray, colorspaceDictionary, filter);
						}
						isIndexedImage = true;
					}
					if (pdfArray2[pdfArray2.Count - 3].GetType().Name == "PdfName")
					{
						m_colorspaceBase = (pdfArray2[pdfArray2.Count - 3] as PdfName).Value;
						colorSpaceResourceDict.Add(m_colorspaceBase, new MemoryStream());
						if (m_colorspaceBase == "DeviceGray")
						{
							m_colorspace = m_colorspaceBase;
						}
					}
					else if (pdfArray2[pdfArray2.Count - 3] is PdfReferenceHolder)
					{
						PdfArray pdfArray3 = (pdfArray2[pdfArray2.Count - 3] as PdfReferenceHolder).Object as PdfArray;
						if (pdfArray3[0] is PdfName)
						{
							if ((pdfArray3[0] as PdfName).Value == "DeviceN")
							{
								isDeviceN = true;
							}
							m_colorspaceBase = (pdfArray3[0] as PdfName).Value;
						}
						if (pdfArray3.Count >= 2 && pdfArray3[1] is PdfReferenceHolder && (pdfArray3[1] as PdfReferenceHolder).Object is PdfDictionary)
						{
							PdfDictionary pdfDictionary = (pdfArray3[1] as PdfReferenceHolder).Object as PdfDictionary;
							if (pdfDictionary.ContainsKey("N"))
							{
								numberOfComponents = (pdfDictionary["N"] as PdfNumber).IntValue;
								if (numberOfComponents == 3)
								{
									m_colorspaceBase = "DeviceRGB";
								}
								else if (numberOfComponents == 4)
								{
									m_colorspaceBase = "DeviceCMYK";
								}
								else if (numberOfComponents == 1)
								{
									m_colorspaceBase = "DeviceGray";
								}
							}
						}
						if (pdfArray3.Count > 2 && pdfArray3[2] is PdfName && (pdfArray3[2] as PdfName).Value == "DeviceCMYK")
						{
							m_colorspaceBase = "DeviceCMYK";
						}
					}
					else if (pdfArray2[pdfArray2.Count - 3] is PdfArray)
					{
						PdfArray pdfArray4 = pdfArray2[pdfArray2.Count - 3] as PdfArray;
						if (pdfArray4[0] is PdfName && (pdfArray4[0] as PdfName).Value == "DeviceN")
						{
							isDeviceN = true;
						}
						if ((pdfArray4[1] as PdfReferenceHolder).Object is PdfDictionary)
						{
							PdfDictionary pdfDictionary2 = (pdfArray4[1] as PdfReferenceHolder).Object as PdfDictionary;
							if (pdfDictionary2.ContainsKey("N"))
							{
								numberOfComponents = (pdfDictionary2["N"] as PdfNumber).IntValue;
								if (numberOfComponents == 3)
								{
									m_colorspaceBase = "DeviceRGB";
								}
								else if (numberOfComponents == 4)
								{
									m_colorspaceBase = "DeviceCMYK";
								}
								else if (numberOfComponents == 1)
								{
									m_colorspaceBase = "DeviceGray";
								}
							}
						}
					}
					if (pdfArray2[pdfArray2.Count - 2].GetType().Name == "PdfNumber")
					{
						m_colorspaceHival = (pdfArray2[pdfArray2.Count - 2] as PdfNumber).IntValue;
					}
					return;
				}
				catch
				{
					isIndexedImage = false;
					GetIndexedColorSpace(pdfArray, colorspaceDictionary, filter);
					return;
				}
			}
			isIndexedImage = false;
			GetIndexedColorSpace(pdfArray, colorspaceDictionary, filter);
		}
		else if (pdfArray.Count > 4 && m_imageDictionary.ContainsKey("SMask"))
		{
			if (!((pdfArray2[0] as PdfName).Value == "DeviceN") || !(pdfArray2[pdfArray2.Count - 1].GetType().Name == "PdfReferenceHolder") || !(pdfArray2[pdfArray2.Count - 3] is PdfName) || ((pdfArray2[pdfArray2.Count - 1] as PdfReferenceHolder).Object as PdfDictionary).Values.Count <= 1)
			{
				return;
			}
			PdfName pdfName2 = new PdfName();
			MemoryStream internalStream2 = ((pdfArray2[pdfArray2.Count - 2] as PdfReferenceHolder).Object as PdfStream).InternalStream;
			if (((pdfArray2[pdfArray2.Count - 2] as PdfReferenceHolder).Object as PdfStream).ContainsKey("Filter"))
			{
				pdfName2 = ((pdfArray2[pdfArray2.Count - 2] as PdfReferenceHolder).Object as PdfStream)["Filter"] as PdfName;
			}
			internalStream2 = ((!(pdfName2 != null) || !(pdfName2.Value == "ASCII85Decode")) ? DecodeFlateStream(internalStream2) : DecodeASCII85Stream(internalStream2));
			if ((pdfArray2[pdfArray2.Count - 3] as PdfName).Value == "DeviceCMYK")
			{
				colorSpaceResourceDict.Add("DeviceCMYK", internalStream2);
			}
			isIndexedImage = false;
			if (pdfArray2[pdfArray2.Count - 3] as PdfName != null)
			{
				if (pdfArray2[0] is PdfName && (pdfArray2[0] as PdfName).Value == "DeviceN")
				{
					isDeviceN = true;
				}
				if (pdfArray2[2] is PdfName)
				{
					m_colorspaceBase = (pdfArray2[2] as PdfName).Value;
				}
			}
		}
		else
		{
			isIndexedImage = false;
			GetIndexedColorSpace(pdfArray, colorspaceDictionary, filter);
		}
	}

	private void GetIndexedColorSpace(PdfArray value, PdfDictionary colorspaceDictionary, string[] filter)
	{
		if (m_colorspace == "ICCBased")
		{
			if ((value[1] as PdfReferenceHolder).Object is PdfStream)
			{
				PdfStream value2 = (value[1] as PdfReferenceHolder).Object as PdfStream;
				nonIndexedImageColorResource = new Dictionary<string, PdfStream>();
				nonIndexedImageColorResource.Add(m_colorspace, value2);
				PdfDictionary pdfDictionary = (value[1] as PdfReferenceHolder).Object as PdfDictionary;
				PdfName pdfName = pdfDictionary["Alternate"] as PdfName;
				if (pdfName != null)
				{
					m_colorspaceBase = pdfName.Value;
					if (m_colorspaceBase == "DeviceGray")
					{
						m_isIccBasedAlternateDeviceGray = true;
						PdfStream pdfStream = (value[1] as PdfReferenceHolder).Object as PdfStream;
						PdfDictionary pdfDictionary2 = (value[1] as PdfReferenceHolder).Object as PdfDictionary;
						if (pdfStream != null)
						{
							m_colorspaceStream = pdfStream.InternalStream;
						}
						if (pdfDictionary2 != null)
						{
							colorspaceDictionary = pdfDictionary2;
						}
						colorSpaceResourceDict.Add("ICCBased", m_colorspaceStream);
					}
				}
				if (pdfDictionary.ContainsKey("N"))
				{
					numberOfComponents = (pdfDictionary["N"] as PdfNumber).IntValue;
				}
				if (pdfName == null && numberOfComponents == 1)
				{
					m_colorspaceBase = "DeviceGray";
					PdfReferenceHolder pdfReferenceHolder = value[1] as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder.Object != null)
					{
						PdfStream pdfStream2 = pdfReferenceHolder.Object as PdfStream;
						PdfDictionary pdfDictionary3 = pdfReferenceHolder.Object as PdfDictionary;
						if (pdfStream2 != null)
						{
							m_colorspaceStream = pdfStream2.InternalStream;
						}
						if (pdfDictionary3 != null)
						{
							colorspaceDictionary = pdfDictionary3;
						}
						colorSpaceResourceDict.Add("ICCBased", m_colorspaceStream);
					}
				}
			}
		}
		else if (m_colorspace == "Indexed")
		{
			if (value[1] is PdfName)
			{
				m_colorspaceBase = (value[1] as PdfName).Value;
			}
			else if (value[1] is PdfReferenceHolder)
			{
				if ((value[1] as PdfReferenceHolder).Object is PdfArray)
				{
					PdfArray pdfArray = (value[1] as PdfReferenceHolder).Object as PdfArray;
					if (pdfArray[0] is PdfName)
					{
						internalColorSpace = (pdfArray[0] as PdfName).Value;
					}
					if (pdfArray[1] is PdfReferenceHolder)
					{
						PdfDictionary pdfDictionary4 = (pdfArray[1] as PdfReferenceHolder).Object as PdfDictionary;
						if (pdfDictionary4.ContainsKey("Alternate"))
						{
							m_colorspaceBase = (pdfDictionary4["Alternate"] as PdfName).Value;
						}
					}
					if (pdfArray.Count > 2 && pdfArray[2] is PdfName)
					{
						m_colorspaceBase = (pdfArray[2] as PdfName).Value;
					}
				}
			}
			else if (value[1] is PdfArray)
			{
				PdfArray pdfArray2 = value[1] as PdfArray;
				if (pdfArray2[0] is PdfName)
				{
					internalColorSpace = (pdfArray2[0] as PdfName).Value;
				}
				if (pdfArray2[1] is PdfReferenceHolder)
				{
					PdfDictionary pdfDictionary5 = (pdfArray2[1] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary5.ContainsKey("Alternate"))
					{
						m_colorspaceBase = (pdfDictionary5["Alternate"] as PdfName).Value;
					}
				}
			}
			if (value[2] is PdfNumber)
			{
				m_colorspaceHival = (value[2] as PdfNumber).IntValue;
			}
			if (m_colorspaceBase == "DeviceRGB" || m_colorspaceBase == "DeviceGray")
			{
				if (m_colorspaceBase == "DeviceGray" || indexedColorSpace == "DeviceGray")
				{
					ColorSpace = "DeviceGray";
				}
				m_colorspaceHival = (value[2] as PdfNumber).IntValue;
				if (value[3] is PdfReferenceHolder)
				{
					m_colorspaceStream = ((value[3] as PdfReferenceHolder).Object as PdfStream).InternalStream;
					colorspaceDictionary = (value[3] as PdfReferenceHolder).Object as PdfDictionary;
				}
				else if (value[3] is PdfString)
				{
					string text = (value[3] as PdfString).Value;
					if (text.Contains("ColorFound") && text.IndexOf("ColorFound") == 0)
					{
						text = text.Remove(0, 10);
					}
					text = GetLiteralString(text);
					text = SkipEscapeSequence(text);
					byte[] asciiBytes = GetAsciiBytes(text);
					m_colorspaceStream = new MemoryStream(asciiBytes, 0, asciiBytes.Length, writable: true, publiclyVisible: true);
					colorSpaceResourceDict.Add("Indexed", m_colorspaceStream);
				}
				if (BitsPerComponent == 4f && internalColorSpace != "ICCBased")
				{
					m_pixelFormat = PixelFormat.Format4bppIndexed;
				}
				else if (BitsPerComponent == 8f)
				{
					m_pixelFormat = PixelFormat.Format8bppIndexed;
				}
			}
			else if (m_colorspaceBase == null && internalColorSpace == "CalRGB")
			{
				m_colorspaceBase = internalColorSpace;
				m_colorspaceHival = (value[2] as PdfNumber).IntValue;
				if (value[3] is PdfReferenceHolder)
				{
					m_colorspaceStream = ((value[3] as PdfReferenceHolder).Object as PdfStream).InternalStream;
					colorspaceDictionary = (value[3] as PdfReferenceHolder).Object as PdfDictionary;
				}
				else if (value[3] is PdfString)
				{
					string text2 = (value[3] as PdfString).Value;
					if (text2.Contains("ColorFound") && text2.IndexOf("ColorFound") == 0)
					{
						text2 = text2.Remove(0, 10);
					}
					text2 = GetLiteralString(text2);
					text2 = SkipEscapeSequence(text2);
					byte[] asciiBytes2 = GetAsciiBytes(text2);
					m_colorspaceStream = new MemoryStream(asciiBytes2, 0, asciiBytes2.Length, writable: true, publiclyVisible: true);
					if (BitsPerComponent == 1f)
					{
						colorSpaceResourceDict.Add("Indexed", m_colorspaceStream);
					}
				}
			}
			else if (value[3] is PdfReferenceHolder)
			{
				m_colorspaceStream = ((value[3] as PdfReferenceHolder).Object as PdfStream).InternalStream;
				colorspaceDictionary = (value[3] as PdfReferenceHolder).Object as PdfDictionary;
			}
			else if (value[3] is PdfString)
			{
				string text3 = (value[3] as PdfString).Value;
				if (text3.Contains("ColorFound") && text3.IndexOf("ColorFound") == 0)
				{
					text3 = text3.Remove(0, 10);
				}
				text3 = GetLiteralString(text3);
				text3 = SkipEscapeSequence(text3);
				byte[] asciiBytes3 = GetAsciiBytes(text3);
				m_colorspaceStream = new MemoryStream(asciiBytes3, 0, asciiBytes3.Length, writable: true, publiclyVisible: true);
				colorSpaceResourceDict.Add("Indexed", m_colorspaceStream);
			}
		}
		if (colorspaceDictionary != null && colorspaceDictionary.ContainsKey("Filter"))
		{
			if (colorspaceDictionary["Filter"] is PdfName)
			{
				filter = new string[1] { (colorspaceDictionary["Filter"] as PdfName).Value };
			}
			else if (colorspaceDictionary["Filter"] is PdfArray)
			{
				int count = (colorspaceDictionary["Filter"] as PdfArray).Count;
				filter = new string[count];
				for (int i = 0; i < count; i++)
				{
					filter[i] = ((colorspaceDictionary["Filter"] as PdfArray)[i] as PdfName).Value;
				}
			}
			else if (colorspaceDictionary["Filter"] is PdfReferenceHolder)
			{
				PdfArray pdfArray3 = (colorspaceDictionary["Filter"] as PdfReferenceHolder).Object as PdfArray;
				filter = new string[pdfArray3.Count];
				for (int j = 0; j < pdfArray3.Count; j++)
				{
					filter[j] = (pdfArray3[0] as PdfName).Value;
				}
			}
		}
		if (filter != null)
		{
			for (int k = 0; k < filter.Length; k++)
			{
				switch (filter[k])
				{
				case "FlateDecode":
					m_colorspaceStream = DecodeFlateStream(m_colorspaceStream);
					break;
				case "ASCII85":
				case "ASCII85Decode":
					m_colorspaceStream = DecodeASCII85Stream(m_colorspaceStream);
					break;
				default:
					throw new Exception("Filter to decode colorspace not implemented.");
				case "ASCIIHexDecode":
					break;
				}
			}
		}
		if ((m_colorspace == "Indexed" || m_colorspace == "IndexedDeviceGray" || m_colorspace == "DeviceGray") && m_colorspaceStream != null)
		{
			byte[] buffer = m_colorspaceStream.GetBuffer();
			byte[] array = new byte[786];
			Array.Copy(buffer, array, Math.Min(array.Length, buffer.Length));
			if (colorSpaceResourceDict == null || colorSpaceResourceDict.Count == 0)
			{
				colorSpaceResourceDict.Add(m_colorspace, m_colorspaceStream);
			}
		}
	}

	private byte[] ConvertIndexedStreamToFlat(int d, int w, int h, byte[] data, byte[] index, bool isARGB, bool isDownsampled)
	{
		int[] array = new int[3] { 0, 1, 2 };
		int[] array2 = new int[4] { 0, 1, 2, 3 };
		int components = 3;
		int indexLength = 0;
		if (index != null)
		{
			indexLength = index.Length;
		}
		if (isARGB)
		{
			components = 4;
		}
		return ConvertIndexedStreamToFlat(d, w, h, data, index, isARGB, isDownsampled, components, indexLength);
	}

	private byte[] ConvertICCBasedStreamToFlat(int d, int w, int h, byte[] data, byte[] index, bool isARGB, bool isDownsampled)
	{
		_ = new int[3] { 0, 1, 2 };
		int[] array = new int[4] { 0, 1, 2, 3 };
		byte[] array2 = new byte[6];
		int components = 3;
		int indexLength = 0;
		if (index != null)
		{
			indexLength = index.Length;
		}
		if (isARGB)
		{
			components = 4;
		}
		index = array2;
		return ConvertIndexedStreamToFlat(d, w, h, data, index, isARGB, isDownsampled, components, indexLength);
	}

	private byte[] ConvertIndexedStreamToFlat(int d, int w, int h, byte[] data, byte[] index, bool isARGB, bool isDownsampled, int components, int indexLength)
	{
		int num = 0;
		int num2 = w * h * components;
		byte[] array = new byte[num2];
		int num3 = 0;
		float num4 = 0f;
		indexLength = index.Length;
		switch (d)
		{
		case 8:
		{
			for (int num9 = 0; num9 < data.Length - 1; num9++)
			{
				if (isDownsampled)
				{
					num4 = (float)(data[num9] & 0xFF) / 255f;
				}
				else
				{
					num3 = (data[num9] & 0xFF) * 3;
				}
				if (num >= num2)
				{
					break;
				}
				if (isDownsampled)
				{
					if (num4 > 0f)
					{
						array[num++] = (byte)((float)(255 - index[0]) * num4);
						array[num++] = (byte)((float)(255 - index[1]) * num4);
						array[num++] = (byte)((float)(255 - index[2]) * num4);
					}
					else
					{
						num += 3;
					}
				}
				else if (num3 < indexLength - 2)
				{
					array[num++] = index[num3];
					array[num++] = index[num3 + 1];
					array[num++] = index[num3 + 2];
				}
				else
				{
					array[num++] = data[num9];
					array[num++] = data[num9];
					array[num++] = data[num9];
				}
				if (isARGB)
				{
					if (num3 == 0 && num4 == 0f)
					{
						array[num++] = byte.MaxValue;
					}
					else
					{
						array[num++] = 0;
					}
				}
			}
			break;
		}
		case 4:
		{
			int[] array2 = new int[2] { 4, 0 };
			int num6 = 0;
			for (int k = 0; k < data.Length; k++)
			{
				for (int l = 0; l < 2; l++)
				{
					int num7 = ((data[k] >> array2[l]) & 0xF) * 3;
					if (num >= num2)
					{
						break;
					}
					if (indexLength > num7)
					{
						array[num++] = index[num7];
						array[num++] = index[num7 + 1];
						array[num++] = index[num7 + 2];
					}
					if (isARGB)
					{
						if (num7 == 0)
						{
							array[num++] = 0;
						}
						else
						{
							array[num++] = 0;
						}
					}
					num6++;
					if (num6 == w)
					{
						num6 = 0;
						l = 8;
					}
				}
			}
			break;
		}
		case 2:
		{
			int[] array3 = new int[4] { 6, 4, 2, 0 };
			int num8 = 0;
			for (int m = 0; m < data.Length; m++)
			{
				for (int n = 0; n < 4; n++)
				{
					int num7 = ((data[m] >> array3[n]) & 3) * 3;
					if (num >= num2)
					{
						break;
					}
					if (indexLength > num7)
					{
						array[num++] = index[num7];
						array[num++] = index[num7 + 1];
						array[num++] = index[num7 + 2];
					}
					if (isARGB)
					{
						if (num7 == 0)
						{
							array[num++] = 0;
						}
						else
						{
							array[num++] = 0;
						}
					}
					num8++;
					if (num8 == w)
					{
						num8 = 0;
						n = 8;
					}
				}
			}
			break;
		}
		case 1:
		{
			int num5 = 0;
			for (int i = 0; i < data.Length; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					num3 = ((data[i] >> 7 - j) & 1) * 3;
					if (num >= num2)
					{
						break;
					}
					if (isARGB)
					{
						if (num3 == 0)
						{
							array[num++] = index[num3];
							array[num++] = index[num3 + 1];
							array[num++] = index[num3 + 2];
							array[num++] = byte.MaxValue;
						}
						else
						{
							array[num++] = index[num3];
							array[num++] = index[num3 + 1];
							array[num++] = index[num3 + 2];
							array[num++] = 0;
						}
					}
					else
					{
						array[num++] = index[num3];
						array[num++] = index[num3 + 1];
						array[num++] = index[num3 + 2];
					}
					num5++;
					if (num5 == w)
					{
						num5 = 0;
						j = 8;
					}
				}
			}
			break;
		}
		}
		return array;
	}

	private string GetLiteralString(string encodedText)
	{
		string text = encodedText;
		int num = -1;
		int num2 = 3;
		while (text.Contains("\\") || text.Contains("\0"))
		{
			string text2 = string.Empty;
			if (text.IndexOf('\\', num + 1) >= 0)
			{
				num = text.IndexOf('\\', num + 1);
			}
			else
			{
				num = text.IndexOf('\0', num + 1);
				if (num < 0)
				{
					break;
				}
				num2 = 2;
			}
			for (int i = num + 1; i <= num + num2; i++)
			{
				if (i < text.Length)
				{
					int result = 0;
					if (!int.TryParse(text[i].ToString(), out result))
					{
						text2 = string.Empty;
						break;
					}
					if (result <= 8)
					{
						text2 += text[i];
					}
				}
				else
				{
					text2 = string.Empty;
				}
			}
			if (text2 != string.Empty && num2 == 3)
			{
				int value = (int)Convert.ToUInt64(text2, 8);
				string @string = Encoding.GetEncoding(1252).GetString(new byte[1] { Convert.ToByte(value) });
				text = text.Remove(num, num2 + 1);
				text = text.Insert(num, @string);
			}
		}
		return text;
	}

	private string SkipEscapeSequence(string text)
	{
		string text2 = "";
		string text3 = "";
		string text4 = "escape";
		if (text.Contains("\\\\\\"))
		{
			int num = text.IndexOf('\\');
			if (num - 1 != 0)
			{
				text2 = text.Substring(num - 3, 3);
			}
		}
		text = text.Replace("\\r", "\r");
		text = text.Replace("\\(", "(");
		text = text.Replace("\\)", ")");
		text = text.Replace("\\n", "\n");
		text = text.Replace("\\t", "\t");
		if (text.Contains("\\b"))
		{
			text = text.Replace("\\b", text4);
			int num2 = text.IndexOf(text4);
			if (num2 - 1 != 0)
			{
				text3 = text.Substring(num2 - 1, 1);
			}
		}
		text = ((!(text3 == "\u0094")) ? text.Replace(text4, "\b") : text.Replace(text4, "\\b"));
		if (text2 != "[[[")
		{
			text = text.Replace("\\\\", "\\");
		}
		return text;
	}

	private static byte[] GetAsciiBytes(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		byte[] array = new byte[value.Length];
		int i = 0;
		for (int length = value.Length; i < length; i++)
		{
			if (value[i] > 'ÿ')
			{
				_ = new byte[1];
				Encoding encoding = Encoding.GetEncoding(1252);
				array[i] = encoding.GetBytes(value[i].ToString())[0];
			}
			else
			{
				array[i] = (byte)value[i];
			}
		}
		return array;
	}

	private MemoryStream DecodeASCII85Stream(MemoryStream encodedStream)
	{
		byte[] array = new ASCII85().decode(encodedStream.ToArray());
		return new MemoryStream(array, 0, array.Length, writable: true, publiclyVisible: true)
		{
			Position = 0L
		};
	}

	private MemoryStream DecodeFlateStream(MemoryStream encodedStream)
	{
		encodedStream.Position = 0L;
		encodedStream.ReadByte();
		encodedStream.ReadByte();
		DeflateStream deflateStream = new DeflateStream(encodedStream, CompressionMode.Decompress, leaveOpen: true);
		byte[] buffer = new byte[4096];
		MemoryStream memoryStream = new MemoryStream();
		while (true)
		{
			int num = deflateStream.Read(buffer, 0, 4096);
			if (num <= 0)
			{
				break;
			}
			memoryStream.Write(buffer, 0, num);
		}
		return memoryStream;
	}

	private MemoryStream DecodeMaskStreamWithMultipleFilters(MemoryStream decodableStream)
	{
		MemoryStream memoryStream = null;
		for (int i = 1; i < m_maskFilter.Length; i++)
		{
			if (m_maskFilter[i] == "ASCII85Decode")
			{
				memoryStream = DecodeASCII85Stream(decodableStream);
				continue;
			}
			if (m_maskFilter[i] == "CCITTFaxDecode")
			{
				PdfDictionary decodeParams = new PdfDictionary();
				if (m_maskDictionary.ContainsKey("DecodeParms"))
				{
					decodeParams = GetDecodeParam(m_maskDictionary)[1];
				}
				return DecodeCCITTFaxDecodeStream(decodableStream, m_maskDictionary, decodeParams);
			}
			if (m_maskFilter[i] == "DCTDecode")
			{
				return decodableStream;
			}
			if (m_maskFilter[i] == "FlateDecode")
			{
				return DecodeFlateStream(decodableStream);
			}
			if (m_maskFilter[i] == "JPXDecode")
			{
				new JPXImage().FromStreamNet(decodableStream).Save(memoryStream, ImageFormat.Png);
			}
			else if (m_maskFilter[i] == "JBIG2Decode")
			{
				DecodeJBIG2EncodedStream(decodableStream).Save(memoryStream, ImageFormat.Png);
			}
			else if (m_maskFilter[i] == "LZWDecode")
			{
				memoryStream = new MemoryStream(new PdfLzwCompressor().Decompress(decodableStream.ToArray(), IsEarlyChange));
			}
		}
		return memoryStream;
	}

	private MemoryStream GetJBIG2GlobalsStream()
	{
		MemoryStream memoryStream = new MemoryStream();
		if (m_imageDictionary.ContainsKey("DecodeParms"))
		{
			if (m_imageDictionary["DecodeParms"] is PdfDictionary pdfDictionary)
			{
				if (pdfDictionary.ContainsKey("JBIG2Globals"))
				{
					memoryStream = ((pdfDictionary["JBIG2Globals"] as PdfReferenceHolder).Object as PdfStream).InternalStream;
					if ((pdfDictionary["JBIG2Globals"] as PdfReferenceHolder).Object is PdfDictionary)
					{
						PdfDictionary pdfDictionary2 = (pdfDictionary["JBIG2Globals"] as PdfReferenceHolder).Object as PdfDictionary;
						if (pdfDictionary2.ContainsKey("Filter") && (pdfDictionary2["Filter"] as PdfName).Value == "FlateDecode")
						{
							memoryStream = DecodeFlateStream(memoryStream);
						}
					}
				}
			}
			else
			{
				string text = "";
				PdfArray pdfArray = m_imageDictionary["DecodeParms"] as PdfArray;
				PdfDictionary pdfDictionary3 = pdfArray[0] as PdfDictionary;
				if (pdfDictionary3 == null)
				{
					pdfDictionary3 = PdfCrossTable.Dereference(pdfArray[0]) as PdfDictionary;
				}
				if (pdfDictionary3 != null && pdfDictionary3.ContainsKey("JBIG2Globals"))
				{
					if ((pdfDictionary3["JBIG2Globals"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("Filter"))
					{
						text = (pdfDictionary4["Filter"] as PdfName).Value.ToString();
					}
					memoryStream = ((pdfDictionary3["JBIG2Globals"] as PdfReferenceHolder).Object as PdfStream).InternalStream;
					if (text == "FlateDecode")
					{
						memoryStream.Position = 0L;
						memoryStream.ReadByte();
						memoryStream.ReadByte();
						DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress, leaveOpen: true);
						byte[] buffer = new byte[4096];
						MemoryStream memoryStream2 = new MemoryStream();
						while (true)
						{
							int num = deflateStream.Read(buffer, 0, 4096);
							if (num <= 0)
							{
								break;
							}
							memoryStream2.Write(buffer, 0, num);
						}
						memoryStream = memoryStream2;
					}
				}
			}
			if (memoryStream.Length > 0)
			{
				memoryStream.Capacity = (int)memoryStream.Length;
			}
		}
		return memoryStream;
	}

	private Bitmap DecodeJBIG2EncodedStream(MemoryStream imageStream)
	{
		JBIG2StreamDecoder jBIG2StreamDecoder = new JBIG2StreamDecoder();
		byte[] array = null;
		if (ImageDictionary.ContainsKey("DecodeParms"))
		{
			jBIG2StreamDecoder.GlobalData = GetJBIG2GlobalsStream().GetBuffer();
		}
		byte[] buffer = imageStream.GetBuffer();
		byte[] array2 = new byte[imageStream.Length];
		Buffer.BlockCopy(buffer, 0, array2, 0, array2.Length);
		jBIG2StreamDecoder.DecodeJBIG2(array2);
		Array.Clear(array2, 0, array2.Length);
		JBIG2Image pageAsJBIG2Bitmap = jBIG2StreamDecoder.GetPageAsJBIG2Bitmap(0);
		bool switchPixelColor = true;
		if (ImageDictionary.ContainsKey("Decode"))
		{
			PdfArray pdfArray = null;
			if (ImageDictionary["Decode"] is PdfArray)
			{
				pdfArray = ImageDictionary["Decode"] as PdfArray;
			}
			switchPixelColor = ((pdfArray == null || (pdfArray[0] as PdfNumber).IntValue != 1) ? true : false);
		}
		array = pageAsJBIG2Bitmap.GetData(switchPixelColor);
		SKImageInfo info = new SKImageInfo((int)Width, (int)Height, SKColorType.Gray8);
		int num = info.Width / 8;
		if (info.Width % 8 != 0)
		{
			num++;
		}
		info.Width = num * 8;
		SKBitmap sKBitmap = new SKBitmap(info);
		nint num2 = sKBitmap.GetPixels();
		byte[] array3 = new byte[sKBitmap.Width];
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < sKBitmap.Height; i++)
		{
			for (int j = 0; j < num; j++)
			{
				string text = Convert.ToString(array[num4++], 2).PadLeft(8, '0');
				for (int k = 0; k < text.Length; k++)
				{
					if (text[k] == '1')
					{
						array3[num3++] = byte.MaxValue;
					}
					else
					{
						array3[num3++] = 0;
					}
				}
			}
			Marshal.Copy(array3, 0, num2, array3.Length);
			num2 += num3;
			num3 = 0;
		}
		return new Bitmap(new Bitmap(sKBitmap), (int)Width, (int)Height);
	}

	private void GetImageInterpolation(PdfDictionary imageDictionary)
	{
		if (imageDictionary != null && imageDictionary.ContainsKey("Interpolate"))
		{
			m_isImageInterpolated = (imageDictionary["Interpolate"] as PdfBoolean).Value;
		}
	}

	private MemoryStream MergeImages(Bitmap input, MemoryStream maskStream, bool DTCdecode)
	{
		MemoryStream memoryStream = new MemoryStream();
		int predictor = 0;
		int colors = 1;
		int columns = 1;
		if (input == null)
		{
			input = new Bitmap((int)m_maskWidth, (int)m_maskHeight);
			Color.FromArgb(255, 255, 255, 255);
		}
		else if (input != null && m_maskWidth > (float)input.Width && m_maskHeight > (float)input.Height && m_maskFilter[0] != "JBIG2Decode" && !DTCdecode)
		{
			input = new Bitmap((int)m_maskWidth, (int)m_maskHeight);
			Color.FromArgb(255, 255, 255, 255);
		}
		else
		{
			input.GetPixel(0, 0);
		}
		GetImageInterpolation(m_maskDictionary);
		if (m_maskFilter[0] == "ASCII85Decode")
		{
			maskStream.Position = 0L;
			maskStream = DecodeASCII85Stream(maskStream);
			using (memoryStream = DecodeMaskStreamWithMultipleFilters(maskStream))
			{
				if (memoryStream != null)
				{
					memoryStream.Position = 0L;
					maskStream = memoryStream;
				}
			}
		}
		else if (m_maskFilter[0] == "DCTDecode")
		{
			using (memoryStream = DecodeMaskStreamWithMultipleFilters(maskStream))
			{
				if (memoryStream != null)
				{
					memoryStream.Position = 0L;
					maskStream = memoryStream;
				}
			}
		}
		else if (m_maskFilter[0] == "FlateDecode")
		{
			maskStream = DecodeFlateStream(maskStream);
			memoryStream = DecodeMaskStreamWithMultipleFilters(maskStream);
			if (memoryStream != null)
			{
				if (m_maskFilter[0] == "FlateDecode" && m_maskFilter[1] == "CCITTFaxDecode")
				{
					m_isImageMask = false;
					return memoryStream;
				}
				memoryStream.Position = 0L;
				maskStream = memoryStream;
			}
			if (m_maskDictionary.ContainsKey("DecodeParms"))
			{
				PdfDictionary[] decodeParam = GetDecodeParam(m_maskDictionary);
				if (decodeParam != null && decodeParam.Length != 0)
				{
					PdfDictionary pdfDictionary = decodeParam[0];
					if (pdfDictionary != null)
					{
						if (pdfDictionary.ContainsKey("Predictor") && pdfDictionary["Predictor"] is PdfNumber pdfNumber)
						{
							predictor = pdfNumber.IntValue;
						}
						if (pdfDictionary.ContainsKey("Columns") && pdfDictionary["Columns"] is PdfNumber pdfNumber2)
						{
							columns = pdfNumber2.IntValue;
						}
						if (pdfDictionary.ContainsKey("Colors") && pdfDictionary["Colors"] is PdfNumber pdfNumber3)
						{
							colors = pdfNumber3.IntValue;
						}
						if (pdfDictionary.ContainsKey("BitsPerComponent") && pdfDictionary["BitsPerComponent"] is PdfNumber pdfNumber4)
						{
							m_bitsPerComponent = pdfNumber4.IntValue;
						}
						if (pdfDictionary.Count > 0)
						{
							maskStream = DecodePredictor(predictor, colors, columns, maskStream);
						}
					}
				}
			}
		}
		else if (m_maskFilter[0] == "CCITTFaxDecode")
		{
			PdfDictionary decodeParams = new PdfDictionary();
			if (m_maskDictionary.ContainsKey("DecodeParms"))
			{
				decodeParams = GetDecodeParam(m_maskDictionary)[0];
			}
			if (ImageDictionary.ContainsKey("Mask"))
			{
				m_isMaskImage = true;
			}
			maskStream = DecodeCCITTFaxDecodeStream(maskStream, m_maskDictionary, decodeParams);
			using (memoryStream = DecodeMaskStreamWithMultipleFilters(maskStream))
			{
				if (memoryStream != null)
				{
					memoryStream.Position = 0L;
					maskStream = memoryStream;
				}
			}
			maskStream.Position = 0L;
			maskStream = ConvertTifftoPng(maskStream);
		}
		else if (m_maskFilter[0] == "JPXDecode")
		{
			maskStream.Position = 0L;
			Bitmap bitmap = new JPXImage().FromStreamNet(maskStream);
			maskStream = new MemoryStream();
			bitmap.Save(maskStream, ImageFormat.Png);
			using (memoryStream = DecodeMaskStreamWithMultipleFilters(maskStream))
			{
				if (memoryStream != null)
				{
					memoryStream.Position = 0L;
					maskStream = memoryStream;
				}
			}
		}
		else if (m_maskFilter[0] == "JBIG2Decode")
		{
			_ = ImageStream;
			JBIG2StreamDecoder jBIG2StreamDecoder = new JBIG2StreamDecoder();
			MemoryStream memoryStream3 = new MemoryStream();
			byte[] array = null;
			if (m_maskDictionary.ContainsKey("DecodeParms"))
			{
				if (m_maskDictionary["DecodeParms"] is PdfDictionary pdfDictionary2)
				{
					if (pdfDictionary2.ContainsKey("JBIG2Globals"))
					{
						memoryStream3 = ((pdfDictionary2["JBIG2Globals"] as PdfReferenceHolder).Object as PdfStream).InternalStream;
						if ((pdfDictionary2["JBIG2Globals"] as PdfReferenceHolder).Object is PdfDictionary)
						{
							PdfDictionary pdfDictionary3 = (pdfDictionary2["JBIG2Globals"] as PdfReferenceHolder).Object as PdfDictionary;
							if (pdfDictionary3.ContainsKey("Filter") && (pdfDictionary3["Filter"] as PdfName).Value == "FlateDecode")
							{
								memoryStream3 = DecodeFlateStream(memoryStream3);
							}
						}
					}
				}
				else
				{
					string text = "";
					PdfDictionary pdfDictionary4 = (m_maskDictionary["DecodeParms"] as PdfArray)[0] as PdfDictionary;
					if (pdfDictionary4.ContainsKey("JBIG2Globals"))
					{
						if ((pdfDictionary4["JBIG2Globals"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey("Filter"))
						{
							text = (pdfDictionary5["Filter"] as PdfName).Value.ToString();
						}
						memoryStream3 = ((pdfDictionary4["JBIG2Globals"] as PdfReferenceHolder).Object as PdfStream).InternalStream;
						if (text == "FlateDecode")
						{
							memoryStream3.Position = 0L;
							memoryStream3.ReadByte();
							memoryStream3.ReadByte();
							DeflateStream deflateStream = new DeflateStream(memoryStream3, CompressionMode.Decompress, leaveOpen: true);
							byte[] buffer = new byte[4096];
							MemoryStream memoryStream4 = new MemoryStream();
							while (true)
							{
								int num = deflateStream.Read(buffer, 0, 4096);
								if (num <= 0)
								{
									break;
								}
								memoryStream4.Write(buffer, 0, num);
							}
							memoryStream3 = memoryStream4;
						}
					}
				}
				if (memoryStream3.Length > 0)
				{
					memoryStream3.Capacity = (int)memoryStream3.Length;
					jBIG2StreamDecoder.GlobalData = memoryStream3.GetBuffer();
				}
			}
			jBIG2StreamDecoder.DecodeJBIG2(maskStream.GetBuffer());
			array = jBIG2StreamDecoder.GetPageAsJBIG2Bitmap(0).GetData(switchPixelColor: true);
			SKImageInfo info = new SKImageInfo((int)m_maskWidth, (int)m_maskHeight, SKColorType.Gray8);
			int num2 = info.Width / 8;
			if (info.Width % 8 != 0)
			{
				num2++;
			}
			info.Width = num2 * 8;
			SKBitmap sKBitmap = new SKBitmap(info);
			nint num3 = sKBitmap.GetPixels();
			byte[] array2 = new byte[sKBitmap.Width];
			int num4 = 0;
			int num5 = 0;
			for (int i = 0; i < sKBitmap.Height; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					string text2 = Convert.ToString(array[num5++], 2).PadLeft(8, '0');
					for (int k = 0; k < text2.Length; k++)
					{
						if (text2[k] == '1')
						{
							array2[num4++] = byte.MaxValue;
						}
						else
						{
							array2[num4++] = 0;
						}
					}
				}
				Marshal.Copy(array2, 0, num3, array2.Length);
				num3 += num4;
				num4 = 0;
			}
			Bitmap bitmap2 = new Bitmap(sKBitmap);
			SKBitmap sKBitmap2 = new SKBitmap(input.Width, input.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
			SKCanvas sKCanvas = new SKCanvas(sKBitmap2);
			sKCanvas.DrawBitmap(sKBitmap, new SKRect(0f, 0f, input.Width, input.Height));
			sKCanvas.Dispose();
			bitmap2 = new Bitmap(sKBitmap2);
			if (PdfCrossTable.Dereference(m_maskDictionary["Interpolate"]) is PdfBoolean { Value: not false })
			{
				bitmap2.MakeTransparent(Color.White);
			}
			MemoryStream memoryStream5 = new MemoryStream();
			bitmap2.Save(memoryStream5, ImageFormat.Png);
			bitmap2.Dispose();
			memoryStream5.Position = 0L;
			maskStream = memoryStream5;
			return maskStream;
		}
		maskStream.Position = 0L;
		SKBitmap sKBitmap3 = CreateMaskImage(input.m_sKBitmap, maskStream);
		int num6 = Math.Abs(input.m_sKBitmap.RowBytes) * input.m_sKBitmap.Height;
		byte[] array3 = new byte[num6];
		array3 = input.m_sKBitmap.Bytes;
		num6 = Math.Abs(sKBitmap3.RowBytes) * sKBitmap3.Height;
		byte[] array4 = new byte[num6];
		array4 = ConvertPixelsToIntArray(sKBitmap3.Pixels);
		num6 = Math.Abs(input.m_sKBitmap.RowBytes) * input.m_sKBitmap.Height;
		for (int l = 0; l < num6; l += 4)
		{
			array3[l + 3] = array4[l + 2];
		}
		int[] array5 = new int[array3.Length];
		int num7 = 0;
		for (int m = 0; m < array5.Length; m += 4)
		{
			array5[num7] = (array3[m + 3] << 24) | (array3[m + 2] << 16) | (array3[m + 1] << 8) | array3[m];
			num7++;
		}
		SKBitmap sKBitmap4 = new SKBitmap(input.Width, input.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
		byte[] array6 = new byte[sKBitmap4.RowBytes * sKBitmap4.Height];
		for (int n = 0; n < array5.Length / 4 - 1; n++)
		{
			Color color = Color.FromArgb(array5[n]);
			int num8 = n % input.Width;
			int num9 = n / input.Width;
			if (input.m_sKBitmap.ColorType == SKColorType.Rgba8888 || (input.m_sKBitmap.ColorType == SKColorType.Rgb888x && m_isJPXDecode))
			{
				array6[sKBitmap4.RowBytes * num9 + 4 * num8] = color.R;
				array6[sKBitmap4.RowBytes * num9 + 4 * num8 + 1] = color.G;
				array6[sKBitmap4.RowBytes * num9 + 4 * num8 + 2] = color.B;
				array6[sKBitmap4.RowBytes * num9 + 4 * num8 + 3] = color.A;
			}
			else
			{
				array6[sKBitmap4.RowBytes * num9 + 4 * num8] = color.B;
				array6[sKBitmap4.RowBytes * num9 + 4 * num8 + 1] = color.G;
				array6[sKBitmap4.RowBytes * num9 + 4 * num8 + 2] = color.R;
				array6[sKBitmap4.RowBytes * num9 + 4 * num8 + 3] = color.A;
			}
		}
		Marshal.Copy(array6, 0, sKBitmap4.GetPixels(), array6.Length);
		MemoryStream memoryStream6 = new MemoryStream();
		sKBitmap4.Encode(memoryStream6, SKEncodedImageFormat.Png, 100);
		imageFormat = ImageFormat.Png;
		return memoryStream6;
	}

	private bool CheckIsRelativeColor()
	{
		PdfDictionary pdfDictionary = null;
		if (m_imageDictionary != null && m_imageDictionary.ContainsKey("SMask"))
		{
			pdfDictionary = (m_imageDictionary["SMask"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		else if (m_imageDictionary != null && m_imageDictionary.ContainsKey("Mask"))
		{
			pdfDictionary = (m_imageDictionary["Mask"] as PdfReferenceHolder).Object as PdfDictionary;
		}
		if (pdfDictionary != null && pdfDictionary.ContainsKey("Intent") && (pdfDictionary["Intent"] as PdfName).Value == "RelativeColorimetric")
		{
			return true;
		}
		return false;
	}

	private bool CheckIsImageMask()
	{
		if (m_imageDictionary != null && m_imageDictionary.ContainsKey("Mask"))
		{
			if ((m_imageDictionary["Mask"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("ImageMask"))
			{
				return (pdfDictionary["ImageMask"] as PdfBoolean).Value;
			}
			return false;
		}
		return false;
	}

	private SKBitmap CreateMaskImage(SKBitmap bitmap, MemoryStream maskStream)
	{
		maskStream.Position = 0L;
		if ((m_maskColorspace == null || m_maskColorspace == "DeviceGray") && (int)m_maskBitsPerComponent == 1 && !isDualFilter)
		{
			byte[] array = maskStream.ToArray();
			PixelFormat pixelFormat = PixelFormat.Format24bppRgb;
			Bitmap bitmap2 = new Bitmap(new SKBitmap((int)Width, (int)Height, SKColorType.Rgb888x, SKAlphaType.Premul));
			bitmap2.PixelFormat = pixelFormat;
			int d = (int)m_maskBitsPerComponent;
			if (array.Length == bitmap2.m_sKImageInfo.RowBytes * bitmap2.Height)
			{
				pixelFormat = PixelFormat.Format1bppIndexed;
				bitmap2 = new Bitmap((int)Width, (int)Height);
				Marshal.Copy(array, 0, bitmap2.m_sKBitmap.GetPixels(), array.Length);
			}
			else
			{
				byte[] index = new byte[6] { 0, 0, 0, 255, 255, 255 };
				array = ConvertIndexedStreamToFlat(d, (int)Width, (int)Height, maskStream.GetBuffer(), index, isARGB: false, isDownsampled: false);
				int num = GetPixelFormatSize(pixelFormat) / 8;
				for (int i = 0; i + 3 <= array.Length; i += 3)
				{
					int num2 = i + 2;
					byte b = array[num2];
					array[num2] = array[i];
					array[i] = b;
				}
				int num3 = 0;
				long num4 = ((IntPtr)bitmap2.m_sKBitmap.GetPixels()).ToInt64();
				int num5 = (int)Width;
				if (num == 3)
				{
					num5 = (int)Width * 3;
				}
				for (int j = 0; (float)j < Height; j++)
				{
					if (m_pixelFormat == PixelFormat.Format24bppRgb)
					{
						byte[] array2 = new byte[num5];
						Buffer.BlockCopy(array, num3, array2, 0, num5);
						byte[] array3 = ApplyAlpha(array2);
						Marshal.Copy(array3, 0, new IntPtr(num4), array3.Length);
					}
					else
					{
						Marshal.Copy(array, num3, new IntPtr(num4), num5);
					}
					num3 += num5;
					num4 += bitmap2.m_sKImageInfo.RowBytes;
				}
			}
			return bitmap2.m_sKBitmap;
		}
		SKBitmap sKBitmap = SKBitmap.Decode(SKData.Create(new SKManagedStream(maskStream)));
		if (sKBitmap == null)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;
			if ((float)bitmap.Width < m_maskWidth || (float)bitmap.Height < m_maskHeight)
			{
				width = (int)m_maskWidth;
				height = (int)m_maskHeight;
			}
			SKColorType sKColorType = GetColorType((int)m_maskBitsPerComponent, m_maskColorspace);
			if (sKColorType == SKColorType.Unknown)
			{
				sKColorType = SKColorType.Gray8;
			}
			sKBitmap = new SKBitmap(new SKImageInfo(width, height, sKColorType));
			byte[] source = maskStream.ToArray();
			int num6 = 0;
			nint num7 = sKBitmap.GetPixels();
			int width2 = sKBitmap.Width;
			for (int k = 0; k < sKBitmap.Height; k++)
			{
				Marshal.Copy(source, num6, num7, width2);
				num6 += width2;
				num7 += sKBitmap.RowBytes;
			}
		}
		return sKBitmap;
	}

	private bool IsWhiteImage(int bytes, byte[] maskBytes)
	{
		int num = 0;
		for (int i = 0; i < bytes; i += 4)
		{
			if (maskBytes[i] != 0 || maskBytes[i + 1] != 0 || maskBytes[i + 2] != 0 || maskBytes[i + 3] != byte.MaxValue)
			{
				Color color = Color.FromArgb((byte)(255 - maskBytes[i]), (byte)(255 - maskBytes[i + 1]), (byte)(255 - maskBytes[i + 2]), (byte)(255 - maskBytes[i + 3]));
				if (color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0)
				{
					num += 4;
				}
			}
		}
		if (num == maskBytes.Length)
		{
			return true;
		}
		if (DecodeParam != null)
		{
			for (int j = 0; j < DecodeParam.Length; j++)
			{
				if (DecodeParam[j] != null)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public static byte[] FromArgb(double alpha, double red, double green, double blue)
	{
		return new byte[4]
		{
			(byte)((!(blue < 0.0)) ? ((blue > 1.0) ? 255 : ((byte)(blue * 255.0))) : 0),
			(byte)((!(green < 0.0)) ? ((green > 1.0) ? 255 : ((byte)(green * 255.0))) : 0),
			(byte)((!(red < 0.0)) ? ((red > 1.0) ? 255 : ((byte)(red * 255.0))) : 0),
			(byte)((!(alpha < 0.0)) ? ((alpha > 1.0) ? 255 : ((byte)(alpha * 255.0))) : 0)
		};
	}

	private bool IsMasked(int row, int column, double imageFactorWidth, double imageFactorHeight)
	{
		row = (int)((double)row * imageFactorWidth);
		column = (int)((double)column * imageFactorHeight);
		for (int i = 0; i <= 1; i++)
		{
			for (int j = 0; j <= 1; j++)
			{
				int num = (row + i) * (int)m_maskWidth + (column + j);
				if (0 <= num && num < m_maskedPixels.Length && m_maskedPixels[num] != 1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public int[] GetStencilMaskedPixels(byte[] data, float maskWidth, float maskHeight)
	{
		int num = 0;
		if (m_imageDictionary.ContainsKey("Decode") && m_colorspace == null)
		{
			m_decodeArray = m_imageDictionary["Decode"] as PdfArray;
			num = (m_decodeArray.Elements[num] as PdfNumber).IntValue;
		}
		int num2 = -16777216;
		int[] array = new int[(int)(maskHeight * maskWidth)];
		BitParser bitParser = ((m_maskDictionary == null || !m_maskDictionary.ContainsKey("BitsPerComponent")) ? new BitParser(data, (int)BitsPerComponent) : new BitParser(data, (m_maskDictionary["BitsPerComponent"] as PdfNumber).IntValue));
		int num3 = 0;
		for (int i = 0; (float)i < maskHeight; i++)
		{
			for (int j = 0; (float)j < maskWidth; j++)
			{
				int num4 = bitParser.ReadBits();
				if (num4 == num)
				{
					array[num3] = num2;
				}
				else
				{
					array[num3] = num4;
				}
				num3++;
			}
			bitParser.MoveToNextRow();
		}
		bitParser = null;
		return array;
	}

	public static Bitmap SetOpacity(Bitmap image, float opacity)
	{
		ColorMatrix colorMatrix = new ColorMatrix();
		colorMatrix.Matrix33 = opacity;
		new ImageAttributes().SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		return new Bitmap(image.Width, image.Height);
	}

	private float ConvertToFloat(byte byteValue)
	{
		float num = (float)(int)byteValue / 255f;
		if ((double)num <= 0.0)
		{
			return 0f;
		}
		if ((double)num <= 0.04045)
		{
			return num / 12.92f;
		}
		if (num < 1f)
		{
			return (float)Math.Pow(((double)num + 0.055) / 1.055, 2.4);
		}
		return 1f;
	}

	private byte ConvertToByte(float value)
	{
		if ((double)value <= 0.0)
		{
			return 0;
		}
		if ((double)value <= 0.0031308)
		{
			return (byte)(255f * value * 12.92f + 0.5f);
		}
		if ((double)value < 1.0)
		{
			return (byte)(255f * (1.055f * (float)Math.Pow(value, 5.0 / 12.0) - 0.055f) + 0.5f);
		}
		return byte.MaxValue;
	}

	private Bitmap DecodeMaskImage(MemoryStream mask)
	{
		PixelFormat pixelFormat = PixelFormat.Format8bppIndexed;
		if (m_maskBitsPerComponent == 1f)
		{
			pixelFormat = PixelFormat.Format1bppIndexed;
			byte[] buffer = mask.GetBuffer();
			Bitmap bitmap = new Bitmap(new SKBitmap((int)m_maskWidth, (int)m_maskHeight, SKColorType.Gray8, SKAlphaType.Premul));
			_ = m_maskHeight;
			int rowBytes = bitmap.m_sKImageInfo.RowBytes;
			nint num = bitmap.m_sKBitmap.GetPixels();
			int pixelFormatSize = GetPixelFormatSize(pixelFormat);
			int num2 = (int)m_maskWidth * pixelFormatSize / 8;
			if ((int)m_maskWidth * pixelFormatSize % 8 != 0)
			{
				num2++;
			}
			int num3 = 0;
			for (int i = 0; (float)i < m_maskHeight; i++)
			{
				Marshal.Copy(buffer, num3, num, num2);
				num3 += num2;
				num += rowBytes;
			}
			return bitmap;
		}
		byte[] buffer2 = mask.GetBuffer();
		SKImageInfo sKImageInfo = new SKImageInfo((int)m_maskWidth, (int)m_maskHeight, GetColorType((int)m_maskBitsPerComponent, m_maskColorspace));
		Bitmap bitmap2 = new Bitmap((int)m_maskWidth, (int)m_maskHeight);
		bitmap2.m_sKBitmap = new SKBitmap(sKImageInfo);
		bitmap2.m_sKImageInfo = sKImageInfo;
		Math.Abs(sKImageInfo.RowBytes);
		_ = bitmap2.Height;
		if (pixelFormat == PixelFormat.Format8bppIndexed)
		{
			int num4 = 0;
			long num5 = ((IntPtr)bitmap2.m_sKBitmap.GetPixels()).ToInt64();
			for (int j = 0; (float)j < m_maskHeight; j++)
			{
				Marshal.Copy(buffer2, num4, new IntPtr(num5), (int)m_maskWidth);
				num4 += (int)m_maskWidth;
				num5 += sKImageInfo.RowBytes;
			}
		}
		else
		{
			Marshal.Copy(buffer2, 0, bitmap2.m_sKBitmap.GetPixels(), buffer2.Length);
		}
		return bitmap2;
	}

	internal byte[] ConvertPixelsToIntArray(SKColor[] color)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < color.Length; i++)
		{
			SKColor sKColor = color[i];
			list.Add(sKColor.Red);
			list.Add(sKColor.Green);
			list.Add(sKColor.Blue);
			list.Add(sKColor.Alpha);
		}
		return list.ToArray();
	}

	internal SKColorType GetColorType(int bitsPerComponent, string colorSpace)
	{
		SKColorType result = SKColorType.Unknown;
		if (colorSpace == "DeviceGray" && bitsPerComponent == 8)
		{
			result = SKColorType.Gray8;
		}
		return result;
	}

	private MemoryStream DecodeDeviceGrayImage(MemoryStream imageStr)
	{
		byte[] array = new byte[imageStr.Length];
		array = imageStr.ToArray();
		int num = (int)BitsPerComponent;
		int num2 = 0;
		int num3 = 0;
		byte[] array2 = null;
		bool flag = false;
		Bitmap bitmap = null;
		switch (num)
		{
		case 1:
		{
			PixelFormat pixelFormat = PixelFormat.Format24bppRgb;
			bitmap = new Bitmap(new SKBitmap((int)Width, (int)Height, SKColorType.Rgb888x, SKAlphaType.Premul));
			bitmap.PixelFormat = pixelFormat;
			if (array.Length == bitmap.m_sKImageInfo.RowBytes * bitmap.Height)
			{
				pixelFormat = PixelFormat.Format1bppIndexed;
				bitmap = new Bitmap((int)Width, (int)Height);
				Marshal.Copy(array, 0, bitmap.m_sKBitmap.GetPixels(), array.Length);
				break;
			}
			byte[] index = new byte[6] { 0, 0, 0, 255, 255, 255 };
			array = ConvertIndexedStreamToFlat(num, (int)Width, (int)Height, imageStr.GetBuffer(), index, isARGB: false, isDownsampled: false);
			int num9 = GetPixelFormatSize(pixelFormat) / 8;
			for (int l = 0; l + 3 <= array.Length; l += 3)
			{
				int num10 = l + 2;
				byte b = array[num10];
				array[num10] = array[l];
				array[l] = b;
			}
			int num11 = 0;
			long num12 = ((IntPtr)bitmap.m_sKBitmap.GetPixels()).ToInt64();
			int num13 = (int)Width;
			if (num9 == 3)
			{
				num13 = (int)Width * 3;
			}
			for (int m = 0; (float)m < Height; m++)
			{
				if (m_pixelFormat == PixelFormat.Format24bppRgb)
				{
					byte[] array5 = new byte[num13];
					Buffer.BlockCopy(array, num11, array5, 0, num13);
					byte[] array6 = ApplyAlpha(array5);
					Marshal.Copy(array6, 0, new IntPtr(num12), array6.Length);
				}
				else
				{
					Marshal.Copy(array, num11, new IntPtr(num12), num13);
				}
				num11 += num13;
				num12 += bitmap.m_sKImageInfo.RowBytes;
			}
			break;
		}
		case 4:
		{
			byte[] array7 = null;
			if (m_colorspaceStream != null)
			{
				array7 = m_colorspaceStream.ToArray();
				flag = true;
			}
			if (!flag)
			{
				break;
			}
			num3 = (int)Width * (int)Height * 3;
			array2 = new byte[num3];
			int[] array8 = new int[2] { 4, 0 };
			int num14 = 0;
			for (int n = 0; n < array.Length; n++)
			{
				for (int num15 = 0; num15 < 2; num15++)
				{
					int num5 = (array[n] >> array8[num15]) & 0xF;
					if (num2 >= num3)
					{
						break;
					}
					array2[num2++] = array7[num5];
					array2[num2++] = array7[num5];
					array2[num2++] = array7[num5];
					num14++;
					if (num14 == (int)Width)
					{
						num14 = 0;
						num15 = 8;
					}
				}
			}
			array = array2;
			bitmap = new Bitmap((int)Width, (int)Height);
			int num16 = GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8;
			byte[] array9 = new byte[num3];
			int num17 = num3;
			byte[] array10 = array;
			int num18 = 0;
			int num19 = 0;
			for (; num18 < num17; num18 += 3)
			{
				array9[num19 + 2] = array10[num18];
				array9[num19 + 1] = array10[num18 + 1];
				array9[num19] = array10[num18 + 2];
				num19 += 3;
			}
			array = array9;
			int num20 = 0;
			long num21 = ((IntPtr)bitmap.m_sKBitmap.GetPixels()).ToInt64();
			int num22 = (int)Width;
			if (num16 == 3)
			{
				num22 = (int)Width * 3;
			}
			for (int num23 = 0; (float)num23 < Height; num23++)
			{
				Marshal.Copy(array, num20, new IntPtr(num21), num22);
				num20 += num22;
				num21 += bitmap.m_sKImageInfo.RowBytes;
			}
			break;
		}
		case 8:
		{
			int num4 = 0;
			byte[] array3 = null;
			if (m_colorspaceStream != null)
			{
				array3 = m_colorspaceStream.ToArray();
				flag = true;
			}
			if (flag)
			{
				num3 = (int)Width * (int)Height;
				array2 = new byte[num3];
				foreach (int num5 in array)
				{
					array2[num2++] = array3[num5];
				}
				array = array2;
				bitmap = new Bitmap((int)Width, (int)Height);
				byte[] array4 = new byte[(int)Width * (int)Height * 3];
				for (int j = 0; j < array.Length; j++)
				{
					array4[num4] = array[j];
					array4[num4 + 1] = array[j];
					array4[num4 + 2] = array[j];
					num4 += 3;
				}
				int num6 = 0;
				long num7 = ((IntPtr)bitmap.m_sKBitmap.GetPixels()).ToInt64();
				int num8 = (int)Width * 3;
				for (int k = 0; (float)k < Height; k++)
				{
					Marshal.Copy(array4, num6, new IntPtr(num7), num8);
					num6 += num8;
					num7 += bitmap.m_sKImageInfo.RowBytes;
				}
			}
			break;
		}
		}
		if (!flag && num != 1)
		{
			bitmap = new Bitmap((int)Width, (int)Height);
			for (int num24 = 0; num24 < (int)Height; num24++)
			{
				int num25 = 0;
				int num26 = 0;
				for (int num27 = 0; num27 < (int)Width; num27++)
				{
					if (num26 < num)
					{
						num25 <<= 8;
						num25 |= imageStr.ReadByte();
						num26 += 8;
					}
					byte b2 = (byte)(num25 >> num26 - num);
					num25 ^= b2 << num26 - num;
					num26 -= num;
					Color color = Color.FromArgb(255, b2, b2, b2);
					bitmap.SetPixel(num27, num24, color);
				}
			}
		}
		MemoryStream memoryStream = new MemoryStream();
		if (IsTransparent)
		{
			bitmap.MakeTransparent(Color.White);
		}
		bitmap.Save(memoryStream, ImageFormat.Png);
		return memoryStream;
	}

	private MemoryStream DecodePredictor(int predictor, int colors, int columns, MemoryStream data)
	{
		MemoryStream memoryStream = new MemoryStream();
		_ = new byte[2048];
		if (predictor == 1)
		{
			memoryStream = data;
		}
		else
		{
			int num = (int)(((float)colors * BitsPerComponent + 7f) / 8f);
			int num2 = (int)(((float)(columns * colors) * BitsPerComponent + 7f) / 8f + (float)num);
			byte[] array = new byte[num2];
			byte[] array2 = new byte[num2];
			int num3 = predictor;
			bool flag = false;
			data.Position = 0L;
			while (!flag && data.Position < data.Length)
			{
				if (predictor >= 10)
				{
					byte[] array3 = new byte[1];
					data.Read(array3, 0, 1);
					num3 = array3[0];
					if (num3 == -1)
					{
						flag = true;
						break;
					}
					num3 += 10;
				}
				int num4 = 0;
				int num5 = num;
				while (num5 < num2 && (num4 = data.Read(array, num5, num2 - num5)) != -1)
				{
					num5 += num4;
					if (num4 == 0)
					{
						break;
					}
				}
				switch (num3)
				{
				case 2:
				{
					for (int j = num; j < num2; j++)
					{
						int num14 = array[j] & 0xFF;
						int num15 = array[j - num] & 0xFF;
						array[j] = (byte)(num14 + num15);
					}
					break;
				}
				case 11:
				{
					for (int l = num; l < num2; l++)
					{
						array[l] += array[l - num];
					}
					break;
				}
				case 12:
				{
					for (int m = num; m < num2; m++)
					{
						int num19 = array[m] & 0xFF;
						int num20 = array2[m] & 0xFF;
						array[m] = (byte)(num19 + num20);
					}
					break;
				}
				case 13:
				{
					for (int k = num; k < num2; k++)
					{
						int num16 = array[k] & 0xFF;
						int num17 = array[k - num] & 0xFF;
						int num18 = array2[k] & 0xFF;
						array[k] = (byte)(num16 + (num17 + num18) / 2);
					}
					break;
				}
				case 14:
				{
					for (int i = num; i < num2; i++)
					{
						int num6 = array[i] & 0xFF;
						int num7 = array[i - num] & 0xFF;
						int num8 = array2[i] & 0xFF;
						int num9 = array2[i - num] & 0xFF;
						int num10 = num7 + num8 - num9;
						int num11 = Math.Abs(num10 - num7);
						int num12 = Math.Abs(num10 - num8);
						int num13 = Math.Abs(num10 - num9);
						if (num11 <= num12 && num11 <= num13)
						{
							array[i] = (byte)(num6 + num7);
						}
						else if (num12 <= num13)
						{
							array[i] = (byte)(num6 + num8);
						}
						else
						{
							array[i] = (byte)(num6 + num9);
						}
					}
					break;
				}
				}
				array2 = (byte[])array.Clone();
				memoryStream.Write(array, num, array.Length - num);
			}
		}
		return memoryStream;
	}

	private byte[] YCCKtoRGB(byte[] encodedData)
	{
		byte[] array = new byte[encodedData.Length];
		_ = Width;
		_ = Height;
		int num = 0;
		for (int i = 0; i + 3 < encodedData.Length; i += 3)
		{
			double num2 = encodedData[i] & 0xFF;
			double num3 = encodedData[i + 1] & 0xFF;
			double num4 = encodedData[i + 2] & 0xFF;
			double num5 = 255.0 - num2;
			double num6 = 255.0 - num3;
			double num7 = 255.0 - num4;
			array[num++] = (byte)num5;
			array[num++] = (byte)num6;
			array[num++] = (byte)num7;
		}
		return array;
	}

	private byte[] YCCToRGB(byte[] encodedData, bool relativeColorimetric)
	{
		byte[] array = new byte[(int)Width * (int)Height * 3];
		int num = (int)Width * (int)Height * 4;
		double num2 = -1.0;
		double num3 = -1.12;
		double num4 = -1.12;
		double num5 = -1.21;
		double num6 = 255.0;
		double num7 = 1.0;
		int num8 = 0;
		for (int i = 0; i < num && i <= encodedData.Length; i += 4)
		{
			double num9 = (double)(encodedData[i] & 0xFF) / num6;
			double num10 = (double)(encodedData[i + 1] & 0xFF) / num6;
			double num11 = (double)(encodedData[i + 2] & 0xFF) / num6;
			double num12 = (double)(encodedData[i + 3] & 0xFF) / num6;
			double num13 = 0.0;
			double num14 = 0.0;
			double num15 = 0.0;
			if (num2 != num9 || num3 != num10 || num4 != num11 || num5 != num12)
			{
				double num16 = num9;
				double num17 = num10;
				double num18 = num11;
				num7 = num12;
				if (relativeColorimetric)
				{
					num13 = 255.0 * (1.0 - num16) * (1.0 - num7);
					num14 = 255.0 * (1.0 - num17) * (1.0 - num7);
					num15 = 255.0 * (1.0 - num18) * (1.0 - num7);
				}
				else
				{
					num13 = 255.0 + num16 * (-4.387332384609988 * num16 + 54.48615194189176 * num17 + 18.82290502165302 * num18 + 212.25662451639585 * num7 - 285.2331026137004) + num17 * (1.7149763477362134 * num17 - 5.6096736904047315 * num18 - 17.873870861415444 * num7 - 5.497006427196366) + num18 * (-2.5217340131683033 * num18 - 21.248923337353073 * num7 + 17.5119270841813) - num7 * (21.86122147463605 * num7 + 189.48180835922747);
					num14 = 255.0 + num16 * (8.841041422036149 * num16 + 60.118027045597366 * num17 + 6.871425592049007 * num18 + 31.159100130055922 * num7 - 79.2970844816548) + num17 * (-15.310361306967817 * num17 + 17.575251261109482 * num18 + 131.35250912493976 * num7 - 190.9453302588951) + num18 * (4.444339102852739 * num18 + 9.8632861493405 * num7 - 24.86741582555878) - num7 * (20.737325471181034 * num7 + 187.80453709719578);
					num15 = 255.0 + num16 * (0.8842522430003296 * num16 + 8.078677503112928 * num17 + 30.89978309703729 * num18 - 0.23883238689178934 * num7 - 14.183576799673286) + num17 * (10.49593273432072 * num17 + 63.02378494754052 * num18 + 50.606957656360734 * num7 - 112.23884253719248) + num18 * (0.03296041114873217 * num18 + 115.60384449646641 * num7 - 193.58209356861505) - num7 * (22.33816807309886 * num7 + 180.12613974708367);
				}
			}
			array[num8++] = (byte)num13;
			array[num8++] = (byte)num14;
			array[num8++] = (byte)num15;
		}
		return array;
	}

	private byte[] GetIndexedPixelData(byte[] encodedData)
	{
		int num = (int)BitsPerComponent;
		BitParser bitParser = new BitParser(encodedData, num);
		byte[] array = new byte[(int)Width * (int)Height * 4];
		double[] array2 = DefaultDecodeArrayForColorspace();
		double xMinimum = 0.0;
		double xMaximum = Math.Pow(2.0, num) - 1.0;
		byte[] colorBytes = colorSpaceResourceDict["Indexed"].ToArray();
		byte[] array3 = new byte[4];
		int num2 = 0;
		if (m_colorspaceBase == "DeviceGray" || m_colorspaceBase == "CalGray")
		{
			m_isDeviceGrayColorspace = true;
		}
		else if (m_colorspaceBase == "DeviceRGB" || m_colorspaceBase == "CalRGB")
		{
			m_isDeviceRGBColorspace = true;
		}
		else if (m_colorspaceBase == "DeviceCMYK" || m_colorspaceBase == "CalCMYK")
		{
			m_isDeviceCMYKColorspace = true;
		}
		for (int i = 0; (float)i < Height; i++)
		{
			for (int j = 0; (float)j < Width; j++)
			{
				array3 = GetIndexedColor((int)Interject(bitParser.ReadBits(), xMinimum, xMaximum, array2[0], array2[1]), colorBytes);
				array[num2] = array3[0];
				array[num2 + 1] = array3[1];
				array[num2 + 2] = array3[2];
				array[num2 + 3] = array3[3];
				num2 += 4;
			}
			bitParser.MoveToNextRow();
		}
		return array;
	}

	private byte[] GetIndexedColor(int index, byte[] colorBytes)
	{
		int num = 0;
		byte[] result = new byte[4] { 0, 255, 255, 255 };
		if (m_colorspaceBase == null || m_colorspaceStream == null)
		{
			return result;
		}
		if (index < 0)
		{
			index = 0;
		}
		if (index > m_colorspaceHival)
		{
			index = m_colorspaceHival;
		}
		byte b = byte.MaxValue;
		if (m_isDeviceGrayColorspace)
		{
			num = index;
			Color.FromArgb(b, colorBytes[num], colorBytes[num], colorBytes[num]);
		}
		else
		{
			if (m_isDeviceRGBColorspace)
			{
				byte[] array = new byte[4];
				num = index * 3;
				if (colorBytes.Length >= num + 2)
				{
					array[0] = b;
					array[1] = colorBytes[num];
					array[2] = colorBytes[num + 1];
					if (colorBytes.Length > num + 2)
					{
						array[3] = colorBytes[num + 2];
					}
				}
				return array;
			}
			if (m_isDeviceCMYKColorspace)
			{
				byte[] array2 = new byte[4];
				byte[] array3 = new byte[4];
				num = index * 4;
				array3[0] = colorBytes[num];
				array3[1] = colorBytes[num + 1];
				array3[2] = colorBytes[num + 2];
				array3[3] = colorBytes[num + 3];
				array3 = ConvertIndexCMYKToRGB(array3);
				array2[0] = byte.MaxValue;
				array2[1] = array3[0];
				array2[2] = array3[1];
				array2[3] = array3[2];
				return array2;
			}
		}
		return result;
	}

	private byte[] ConvertCMYKtoRGBColor(byte cyan, byte magenta, byte yellow, byte black)
	{
		return new byte[4]
		{
			255,
			(byte)(255 - Math.Min(255, cyan + black)),
			(byte)(255 - Math.Min(255, magenta + black)),
			(byte)(255 - Math.Min(255, yellow + black))
		};
	}

	private int[] RenderRGBPixels(byte[] encodedData)
	{
		int num = (int)BitsPerComponent;
		BitParser bitParser = new BitParser(encodedData, num);
		List<ColorConvertor> list = new List<ColorConvertor>((int)Width * (int)Height);
		double[] array = DefaultDecodeArrayForColorspace();
		if (m_indexedRGBvalues != null)
		{
			bool flag = true;
			for (int i = 0; i < m_indexedRGBvalues.Length; i++)
			{
				if (m_indexedRGBvalues[i] != byte.MaxValue)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				array = new double[2] { 1.0, 1.0 };
			}
		}
		double xMinimum = 0.0;
		double xMaximum = Math.Pow(2.0, num) - 1.0;
		for (int j = 0; (float)j < Height; j++)
		{
			for (int k = 0; (float)k < Width; k++)
			{
				if (ColorSpace != "DeviceGray" && m_colorspaceBase != "DeviceGray")
				{
					list.Add(ColorConvertor.FromArgb(1.0, Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[0], array[1]), Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[2], array[3]), Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[4], array[5])));
				}
				else
				{
					list.Add(ColorConvertor.FromArgb(1.0, Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[0], array[1]), Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[0], array[1]), Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[0], array[1])));
				}
			}
			bitParser.MoveToNextRow();
		}
		int[] result = RenderImagePixel(list);
		bitParser = null;
		list = null;
		return result;
	}

	private int[] CMYKtoRGBPixels(byte[] encodedData)
	{
		int num = (int)BitsPerComponent;
		BitParser bitParser = new BitParser(encodedData, num);
		List<ColorConvertor> list = new List<ColorConvertor>((int)Width * (int)Height);
		double[] array = DefaultDecodeArrayForColorspace();
		double xMinimum = 0.0;
		double xMaximum = Math.Pow(2.0, num) - 1.0;
		for (int i = 0; (float)i < Height; i++)
		{
			for (int j = 0; (float)j < Width; j++)
			{
				list.Add(ColorConvertor.FromCmyk(Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[0], array[1]), Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[2], array[3]), Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[4], array[5]), Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[6], array[7])));
			}
			bitParser.MoveToNextRow();
		}
		return RenderImagePixel(list);
	}

	private ColorConvertor[] RenderGrayPixels(byte[] encodedData)
	{
		int num = (int)BitsPerComponent;
		BitParser bitParser = new BitParser(encodedData, num);
		List<ColorConvertor> list = new List<ColorConvertor>((int)Width * (int)Height);
		double[] array = DefaultDecodeArrayForColorspace();
		double xMinimum = 0.0;
		double xMaximum = Math.Pow(2.0, num) - 1.0;
		for (int i = 0; (float)i < Height; i++)
		{
			for (int j = 0; (float)j < Width; j++)
			{
				list.Add(ColorConvertor.FromGray(Interject(bitParser.ReadBits(), xMinimum, xMaximum, array[0], array[1])));
			}
			bitParser.MoveToNextRow();
		}
		return list.ToArray();
	}

	private double[] DefaultDecodeArrayForColorspace()
	{
		double[] array = null;
		if (ColorSpace == null)
		{
			PdfReferenceHolder pdfReferenceHolder = ImageDictionary["ColorSpace"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				PdfName pdfName = pdfReferenceHolder.Object as PdfName;
				if (pdfName != null)
				{
					ColorSpace = pdfName.Value;
				}
			}
		}
		switch (m_colorspaceBase)
		{
		case "DeviceCMYK":
			array = new double[8] { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 };
			break;
		case "CalRGB":
		case "DeviceRGB":
			array = new double[6] { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 };
			break;
		case "DeviceGray":
			array = new double[3] { 1.0, 0.0, 0.0 };
			break;
		case "Indexed":
			array = new double[2]
			{
				0.0,
				Math.Pow(2.0, BitsPerComponent) - 1.0
			};
			break;
		}
		if (DecodeArray != null && DecodeArray.Count > 0)
		{
			array = new double[DecodeArray.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (DecodeArray[i] as PdfNumber).FloatValue;
			}
		}
		else
		{
			array = new double[2] { 0.0, 1.0 };
		}
		switch (ColorSpace)
		{
		case "DeviceCMYK":
			array = new double[8] { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 };
			break;
		case "CalRGB":
		case "DeviceRGB":
			array = new double[6] { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 };
			break;
		case "DeviceGray":
			array = new double[2] { 0.0, 1.0 };
			break;
		case "Indexed":
			array = new double[2]
			{
				0.0,
				Math.Pow(2.0, BitsPerComponent) - 1.0
			};
			break;
		}
		return array;
	}

	private int[] RenderImagePixel(List<ColorConvertor> list)
	{
		ColorConvertor[] array = list.ToArray();
		if (m_imageDictionary.ContainsKey("SMask"))
		{
			PdfDictionary pdfDictionary = (m_imageDictionary["SMask"] as PdfReferenceHolder).Object as PdfDictionary;
			PdfStream obj = pdfDictionary as PdfStream;
			obj.Decompress();
			byte[] data = obj.Data;
			if (data != null)
			{
				RenderAlphaMask(array, data, pdfDictionary);
			}
		}
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i].PixelConversion();
		}
		m_isSoftMasked = true;
		return array2;
	}

	private void RenderAlphaMask(ColorConvertor[] pixels, byte[] array, PdfDictionary sMaskDictionary)
	{
		float bitsPerComponent = 0f;
		if (sMaskDictionary.ContainsKey("BitsPerComponent"))
		{
			bitsPerComponent = (sMaskDictionary["BitsPerComponent"] as PdfNumber).IntValue;
		}
		m_bitsPerComponent = bitsPerComponent;
		ColorConvertor[] array2 = RenderGrayPixels(array);
		int num = Math.Min(array2.Length, pixels.Length);
		for (int i = 0; i < num; i++)
		{
			pixels[i].Alpha = array2[i].GetGrayComponent();
		}
	}

	private double Interject(double x, double xMinimum, double xMaximum, double yMinimum, double yMaximum)
	{
		return yMinimum + (x - xMinimum) * ((yMaximum - yMinimum) / (xMaximum - xMinimum));
	}

	private double RoundOff(double value)
	{
		if (value < 0.0)
		{
			value = 0.0;
		}
		if (value > 1.0)
		{
			value = 1.0;
		}
		return value;
	}

	private string[] GetImageFilter()
	{
		string[] array = null;
		if (m_imageDictionary != null && m_imageDictionary.ContainsKey("Filter"))
		{
			if (m_imageDictionary["Filter"] is PdfName)
			{
				array = new string[1] { (m_imageDictionary["Filter"] as PdfName).Value };
			}
			else if (m_imageDictionary["Filter"] is PdfArray)
			{
				PdfArray pdfArray = m_imageDictionary["Filter"] as PdfArray;
				array = new string[pdfArray.Count];
				for (int i = 0; i < pdfArray.Count; i++)
				{
					array[i] = (pdfArray[i] as PdfName).Value;
				}
			}
			else if (m_imageDictionary["Filter"] is PdfReferenceHolder)
			{
				if ((m_imageDictionary["Filter"] as PdfReferenceHolder).Object is PdfArray)
				{
					PdfArray pdfArray2 = (m_imageDictionary["Filter"] as PdfReferenceHolder).Object as PdfArray;
					array = new string[pdfArray2.Count];
					for (int j = 0; j < pdfArray2.Count; j++)
					{
						array[j] = (pdfArray2[j] as PdfName).Value;
					}
				}
				else if ((m_imageDictionary["Filter"] as PdfReferenceHolder).Object is PdfName)
				{
					array = new string[1] { ((m_imageDictionary["Filter"] as PdfReferenceHolder).Object as PdfName).Value };
				}
			}
		}
		return array;
	}

	private void GetIsImageMask()
	{
		if (m_imageDictionary.ContainsKey("ImageMask"))
		{
			m_isImageMask = (m_imageDictionary["ImageMask"] as PdfBoolean).Value;
		}
	}

	private PdfDictionary[] GetDecodeParam(PdfDictionary imageDictionary)
	{
		PdfDictionary[] array = null;
		if (imageDictionary != null && imageDictionary.ContainsKey("DecodeParms"))
		{
			if (PdfCrossTable.Dereference(imageDictionary["DecodeParms"]) is PdfDictionary pdfDictionary)
			{
				array = new PdfDictionary[1] { pdfDictionary };
			}
			else if (PdfCrossTable.Dereference(imageDictionary["DecodeParms"]) is PdfArray pdfArray)
			{
				array = new PdfDictionary[pdfArray.Count];
				for (int i = 0; i < pdfArray.Count; i++)
				{
					array[i] = pdfArray[i] as PdfDictionary;
				}
			}
		}
		return array;
	}

	private float GetImageHeight()
	{
		float result = 0f;
		if (m_imageDictionary != null && m_imageDictionary.ContainsKey("Height"))
		{
			if (m_imageDictionary["Height"] is PdfReferenceHolder)
			{
				return ((m_imageDictionary["Height"] as PdfReferenceHolder).Object as PdfNumber).FloatValue;
			}
			result = (m_imageDictionary["Height"] as PdfNumber).FloatValue;
		}
		return result;
	}

	private float GetBitsPerComponent()
	{
		float result = 0f;
		if (m_imageDictionary != null)
		{
			result = (m_imageDictionary["BitsPerComponent"] as PdfNumber).FloatValue;
		}
		return result;
	}

	private bool GetIsEarlyChange()
	{
		PdfDictionary[] decodeParam = GetDecodeParam(m_imageDictionary);
		if (decodeParam != null)
		{
			if (decodeParam[0] != null && decodeParam[0].ContainsKey(new PdfName("EarlyChange")))
			{
				if ((decodeParam[0][new PdfName("EarlyChange")] as PdfNumber).IntValue == 0)
				{
					m_isEarlyChange = false;
				}
				else
				{
					m_isEarlyChange = true;
				}
			}
		}
		else
		{
			m_isEarlyChange = true;
		}
		return m_isEarlyChange;
	}

	private void GetDecodeArray()
	{
		if (m_imageDictionary.ContainsKey("Decode"))
		{
			m_decodeArray = m_imageDictionary["Decode"] as PdfArray;
		}
	}

	private float GetImageWidth()
	{
		float result = 0f;
		if (m_imageDictionary != null && m_imageDictionary.ContainsKey("Width"))
		{
			result = (m_imageDictionary["Width"] as PdfNumber).FloatValue;
		}
		return result;
	}

	private static void Sub(byte[] currentLine, int sub)
	{
		for (int i = 0; i < currentLine.Length; i++)
		{
			int num = i - sub;
			if (num >= 0)
			{
				currentLine[i] += currentLine[num];
			}
		}
	}

	private void Up(byte[] currentLine, byte[] prevLine)
	{
		if (prevLine != null)
		{
			for (int i = 0; i < currentLine.Length; i++)
			{
				currentLine[i] += prevLine[i];
			}
		}
	}

	private void Average(byte[] currentLine, byte[] prevLine, int sub)
	{
		for (int i = 0; i < currentLine.Length; i++)
		{
			int num = 0;
			int num2 = 0;
			int num3 = i - sub;
			if (num3 >= 0)
			{
				num = currentLine[num3] & 0xFF;
			}
			if (prevLine != null)
			{
				num2 = prevLine[i] & 0xFF;
			}
			currentLine[i] += (byte)Math.Floor((double)(num + num2) / 2.0);
		}
	}

	private void Paeth(byte[] currentLine, byte[] prevLine, int sub)
	{
		for (int i = 0; i < currentLine.Length; i++)
		{
			int left = 0;
			int up = 0;
			int upLeft = 0;
			int num = i - sub;
			if (num >= 0)
			{
				left = currentLine[num] & 0xFF;
			}
			if (prevLine != null)
			{
				up = prevLine[i] & 0xFF;
			}
			if (num > 0 && prevLine != null)
			{
				upLeft = prevLine[num] & 0xFF;
			}
			currentLine[i] += (byte)Paeth(left, up, upLeft);
		}
	}

	private int Paeth(int left, int up, int upLeft)
	{
		int num = left + up - upLeft;
		int num2 = Math.Abs(num - left);
		int num3 = Math.Abs(num - up);
		int num4 = Math.Abs(num - upLeft);
		if (num2 <= num3 && num2 <= num4)
		{
			return left;
		}
		if (num3 <= num4)
		{
			return up;
		}
		return upLeft;
	}

	private MemoryStream ConvertTifftoPng(MemoryStream decodedMemoryStream)
	{
		using Tiff tiff = Tiff.ClientOpen("in-memory", "r", decodedMemoryStream, new TiffStream());
		int num = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH)[0].ToInt();
		int num2 = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH)[0].ToInt();
		SKBitmap sKBitmap = new SKBitmap();
		SKImageInfo info = new SKImageInfo(num, num2);
		int[] array = new int[num * num2];
		GCHandle ptr = GCHandle.Alloc(array, GCHandleType.Pinned);
		sKBitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes, null, delegate
		{
			ptr.Free();
		}, null);
		if (!tiff.ReadRGBAImageOriented(num, num2, array, Orientation.TOPLEFT))
		{
			return null;
		}
		if (SKImageInfo.PlatformColorType == SKColorType.Bgra8888)
		{
			SKSwizzle.SwapRedBlue(ptr.AddrOfPinnedObject(), array.Length);
		}
		m_embeddedImage = new Bitmap(sKBitmap);
		MemoryStream memoryStream = new MemoryStream();
		m_embeddedImage.Save(memoryStream, ImageFormat.Png);
		memoryStream.Position = 0L;
		return memoryStream;
	}
}
