using System;
using System.Globalization;
using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic;

public class TiffRgbaImage
{
	public delegate void PutContigDelegate(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift);

	public delegate void PutSeparateDelegate(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift);

	public delegate bool GetDelegate(TiffRgbaImage img, int[] raster, int offset, int width, int height);

	internal const string photoTag = "PhotometricInterpretation";

	private Tiff tif;

	private bool stoponerr;

	private bool isContig;

	private ExtraSample alpha;

	private int width;

	private int height;

	private short bitspersample;

	private short samplesperpixel;

	private Orientation orientation;

	private Orientation req_orientation;

	private Photometric photometric;

	private ushort[] redcmap;

	private ushort[] greencmap;

	private ushort[] bluecmap;

	private GetDelegate get;

	private PutContigDelegate putContig;

	private PutSeparateDelegate putSeparate;

	private byte[] Map;

	private int[][] BWmap;

	private int[][] PALmap;

	private TiffYCbCrToRGB ycbcr;

	private TiffCIELabToRGB cielab;

	private static readonly TiffDisplay display_sRGB = new TiffDisplay(new float[3] { 3.241f, -1.5374f, -0.4986f }, new float[3] { -0.9692f, 1.876f, 0.0416f }, new float[3] { 0.0556f, -0.204f, 1.057f }, 100f, 100f, 100f, 255, 255, 255, 1f, 1f, 1f, 2.4f, 2.4f, 2.4f);

	private const int A1 = -16777216;

	private const int FLIP_VERTICALLY = 1;

	private const int FLIP_HORIZONTALLY = 2;

	internal int row_offset;

	internal int col_offset;

	public bool IsContig => isContig;

	public ExtraSample Alpha => alpha;

	public int Width => width;

	public int Height => height;

	public short BitsPerSample => bitspersample;

	public short SamplesPerPixel => samplesperpixel;

	public Orientation Orientation => orientation;

	public Orientation ReqOrientation
	{
		get
		{
			return req_orientation;
		}
		set
		{
			req_orientation = value;
		}
	}

	public Photometric Photometric => photometric;

	public GetDelegate Get
	{
		get
		{
			return get;
		}
		set
		{
			get = value;
		}
	}

	public PutContigDelegate PutContig
	{
		get
		{
			return putContig;
		}
		set
		{
			putContig = value;
		}
	}

	public PutSeparateDelegate PutSeparate
	{
		get
		{
			return putSeparate;
		}
		set
		{
			putSeparate = value;
		}
	}

	private TiffRgbaImage()
	{
	}

	public static TiffRgbaImage Create(Tiff tif, bool stopOnError, out string errorMsg)
	{
		errorMsg = null;
		TiffRgbaImage tiffRgbaImage = new TiffRgbaImage();
		tiffRgbaImage.row_offset = 0;
		tiffRgbaImage.col_offset = 0;
		tiffRgbaImage.redcmap = null;
		tiffRgbaImage.greencmap = null;
		tiffRgbaImage.bluecmap = null;
		tiffRgbaImage.req_orientation = Orientation.BOTLEFT;
		tiffRgbaImage.tif = tif;
		tiffRgbaImage.stoponerr = stopOnError;
		FieldValue[] fieldDefaulted = tif.GetFieldDefaulted(TiffTag.BITSPERSAMPLE);
		tiffRgbaImage.bitspersample = fieldDefaulted[0].ToShort();
		switch (tiffRgbaImage.bitspersample)
		{
		default:
			errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle images with {0}-bit samples", tiffRgbaImage.bitspersample);
			return null;
		case 1:
		case 2:
		case 4:
		case 8:
		case 16:
		{
			tiffRgbaImage.alpha = ExtraSample.UNSPECIFIED;
			fieldDefaulted = tif.GetFieldDefaulted(TiffTag.SAMPLESPERPIXEL);
			tiffRgbaImage.samplesperpixel = fieldDefaulted[0].ToShort();
			fieldDefaulted = tif.GetFieldDefaulted(TiffTag.EXTRASAMPLES);
			short num = fieldDefaulted[0].ToShort();
			byte[] array = fieldDefaulted[1].ToByteArray();
			if (num >= 1)
			{
				switch ((ExtraSample)array[0])
				{
				case ExtraSample.UNSPECIFIED:
					if (tiffRgbaImage.samplesperpixel > 3)
					{
						tiffRgbaImage.alpha = ExtraSample.ASSOCALPHA;
					}
					break;
				case ExtraSample.ASSOCALPHA:
				case ExtraSample.UNASSALPHA:
					tiffRgbaImage.alpha = (ExtraSample)array[0];
					break;
				}
			}
			fieldDefaulted = tif.GetField(TiffTag.PHOTOMETRIC);
			if (fieldDefaulted == null)
			{
				tiffRgbaImage.photometric = Photometric.MINISWHITE;
			}
			if (num == 0 && tiffRgbaImage.samplesperpixel == 4 && tiffRgbaImage.photometric == Photometric.RGB)
			{
				tiffRgbaImage.alpha = ExtraSample.ASSOCALPHA;
				num = 1;
			}
			int num2 = tiffRgbaImage.samplesperpixel - num;
			fieldDefaulted = tif.GetFieldDefaulted(TiffTag.COMPRESSION);
			Compression compression = (Compression)fieldDefaulted[0].ToInt();
			fieldDefaulted = tif.GetFieldDefaulted(TiffTag.PLANARCONFIG);
			PlanarConfig planarConfig = (PlanarConfig)fieldDefaulted[0].ToShort();
			fieldDefaulted = tif.GetField(TiffTag.PHOTOMETRIC);
			if (fieldDefaulted == null)
			{
				switch (num2)
				{
				case 1:
					if (tiffRgbaImage.isCCITTCompression())
					{
						tiffRgbaImage.photometric = Photometric.MINISWHITE;
					}
					else
					{
						tiffRgbaImage.photometric = Photometric.MINISBLACK;
					}
					break;
				case 3:
					tiffRgbaImage.photometric = Photometric.RGB;
					break;
				default:
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Missing needed {0} tag", "PhotometricInterpretation");
					return null;
				}
			}
			else
			{
				tiffRgbaImage.photometric = (Photometric)fieldDefaulted[0].ToInt();
			}
			switch (tiffRgbaImage.photometric)
			{
			case Photometric.PALETTE:
			{
				fieldDefaulted = tif.GetField(TiffTag.COLORMAP);
				if (fieldDefaulted == null)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Missing required \"Colormap\" tag");
					return null;
				}
				short[] src = fieldDefaulted[0].ToShortArray();
				short[] src2 = fieldDefaulted[1].ToShortArray();
				short[] src3 = fieldDefaulted[2].ToShortArray();
				int num3 = 1 << (int)tiffRgbaImage.bitspersample;
				tiffRgbaImage.redcmap = new ushort[num3];
				tiffRgbaImage.greencmap = new ushort[num3];
				tiffRgbaImage.bluecmap = new ushort[num3];
				Buffer.BlockCopy(src, 0, tiffRgbaImage.redcmap, 0, num3 * 2);
				Buffer.BlockCopy(src2, 0, tiffRgbaImage.greencmap, 0, num3 * 2);
				Buffer.BlockCopy(src3, 0, tiffRgbaImage.bluecmap, 0, num3 * 2);
				if (planarConfig == PlanarConfig.CONTIG && tiffRgbaImage.samplesperpixel != 1 && tiffRgbaImage.bitspersample < 8)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle contiguous data with {0}={1}, and {2}={3} and Bits/Sample={4}", "PhotometricInterpretation", tiffRgbaImage.photometric, "Samples/pixel", tiffRgbaImage.samplesperpixel, tiffRgbaImage.bitspersample);
					return null;
				}
				break;
			}
			case Photometric.MINISWHITE:
			case Photometric.MINISBLACK:
				if (planarConfig == PlanarConfig.CONTIG && tiffRgbaImage.samplesperpixel != 1 && tiffRgbaImage.bitspersample < 8)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle contiguous data with {0}={1}, and {2}={3} and Bits/Sample={4}", "PhotometricInterpretation", tiffRgbaImage.photometric, "Samples/pixel", tiffRgbaImage.samplesperpixel, tiffRgbaImage.bitspersample);
					return null;
				}
				break;
			case Photometric.YCBCR:
				if (planarConfig == PlanarConfig.CONTIG && compression == Compression.JPEG)
				{
					tif.SetField(TiffTag.JPEGCOLORMODE, JpegColorMode.RGB);
					tiffRgbaImage.photometric = Photometric.RGB;
				}
				break;
			case Photometric.RGB:
				if (num2 < 3)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle RGB image with {0}={1}", "Color channels", num2);
					return null;
				}
				break;
			case Photometric.SEPARATED:
			{
				fieldDefaulted = tif.GetFieldDefaulted(TiffTag.INKSET);
				InkSet inkSet = (InkSet)fieldDefaulted[0].ToByte();
				if (inkSet != InkSet.CMYK)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle separated image with {0}={1}", "InkSet", inkSet);
					return null;
				}
				if (tiffRgbaImage.samplesperpixel < 4)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle separated image with {0}={1}", "Samples/pixel", tiffRgbaImage.samplesperpixel);
					return null;
				}
				break;
			}
			case Photometric.LOGL:
				if (compression != Compression.SGILOG)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, LogL data must have {0}={1}", "Compression", Compression.SGILOG);
					return null;
				}
				tif.SetField(TiffTag.SGILOGDATAFMT, 3);
				tiffRgbaImage.photometric = Photometric.MINISBLACK;
				tiffRgbaImage.bitspersample = 8;
				break;
			case Photometric.LOGLUV:
				if (compression != Compression.SGILOG && compression != Compression.SGILOG24)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, LogLuv data must have {0}={1} or {2}", "Compression", Compression.SGILOG, Compression.SGILOG24);
					return null;
				}
				if (planarConfig != PlanarConfig.CONTIG)
				{
					errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle LogLuv images with {0}={1}", "Planarconfiguration", planarConfig);
					return null;
				}
				tif.SetField(TiffTag.SGILOGDATAFMT, 3);
				tiffRgbaImage.photometric = Photometric.RGB;
				tiffRgbaImage.bitspersample = 8;
				break;
			default:
				errorMsg = string.Format(CultureInfo.InvariantCulture, "Sorry, can not handle image with {0}={1}", "PhotometricInterpretation", tiffRgbaImage.photometric);
				return null;
			case Photometric.CIELAB:
				break;
			}
			tiffRgbaImage.Map = null;
			tiffRgbaImage.BWmap = null;
			tiffRgbaImage.PALmap = null;
			tiffRgbaImage.ycbcr = null;
			tiffRgbaImage.cielab = null;
			fieldDefaulted = tif.GetField(TiffTag.IMAGEWIDTH);
			tiffRgbaImage.width = fieldDefaulted[0].ToInt();
			fieldDefaulted = tif.GetField(TiffTag.IMAGELENGTH);
			tiffRgbaImage.height = fieldDefaulted[0].ToInt();
			fieldDefaulted = tif.GetFieldDefaulted(TiffTag.ORIENTATION);
			tiffRgbaImage.orientation = (Orientation)fieldDefaulted[0].ToByte();
			tiffRgbaImage.isContig = planarConfig != PlanarConfig.SEPARATE || num2 <= 1;
			if (tiffRgbaImage.isContig)
			{
				if (!tiffRgbaImage.pickContigCase())
				{
					errorMsg = "Sorry, can not handle image";
					return null;
				}
			}
			else if (!tiffRgbaImage.pickSeparateCase())
			{
				errorMsg = "Sorry, can not handle image";
				return null;
			}
			return tiffRgbaImage;
		}
		}
	}

	public bool GetRaster(int[] raster, int offset, int width, int height)
	{
		if (get == null)
		{
			Tiff.ErrorExt(tif, tif.m_clientdata, tif.FileName(), "No \"get\" method setup");
			return false;
		}
		return get(this, raster, offset, width, height);
	}

	private static int PACK(int r, int g, int b)
	{
		return r | (g << 8) | (b << 16) | -16777216;
	}

	private static int PACK4(int r, int g, int b, int a)
	{
		return r | (g << 8) | (b << 16) | (a << 24);
	}

	private static int PACK4(int rgb, int a)
	{
		return rgb & (a << 24);
	}

	private static int W2B(short v)
	{
		return (v >> 8) & 0xFF;
	}

	private static int PACKW(short r, short g, short b)
	{
		return W2B(r) | (W2B(g) << 8) | (W2B(b) << 16) | -16777216;
	}

	private static int PACKW4(short r, short g, short b, short a)
	{
		return W2B(r) | (W2B(g) << 8) | (W2B(b) << 16) | (W2B(a) << 24);
	}

	private void CMAP(int x, int i, ref int j)
	{
		PALmap[i][j++] = PACK(redcmap[x] & 0xFF, greencmap[x] & 0xFF, bluecmap[x] & 0xFF);
	}

	private void GREY(int x, int i, ref int j)
	{
		int num = Map[x];
		BWmap[i][j++] = PACK(num, num, num);
	}

	private static bool gtTileContig(TiffRgbaImage img, int[] raster, int offset, int width, int height)
	{
		byte[] buffer = new byte[img.tif.TileSize()];
		int num = img.tif.GetField(TiffTag.TILEWIDTH)[0].ToInt();
		int num2 = img.tif.GetField(TiffTag.TILELENGTH)[0].ToInt();
		int num3 = img.setorientation();
		int num4;
		int num5;
		if (((uint)num3 & (true ? 1u : 0u)) != 0)
		{
			num4 = height - 1;
			num5 = -(num + width);
		}
		else
		{
			num4 = 0;
			num5 = -(num - width);
		}
		bool result = true;
		int num7;
		for (int i = 0; i < height; i += num7)
		{
			int num6 = num2 - (i + img.row_offset) % num2;
			num7 = ((i + num6 > height) ? (height - i) : num6);
			for (int j = 0; j < width; j += num)
			{
				if (img.tif.ReadTile(buffer, 0, j + img.col_offset, i + img.row_offset, 0, 0) < 0 && img.stoponerr)
				{
					result = false;
					break;
				}
				int offset2 = (i + img.row_offset) % num2 * img.tif.TileRowSize();
				if (j + num > width)
				{
					int num8 = width - j;
					int num9 = num - num8;
					img.putContig(img, raster, offset + num4 * width + j, num5 + num9, j, num4, num8, num7, buffer, offset2, num9);
				}
				else
				{
					img.putContig(img, raster, offset + num4 * width + j, num5, j, num4, num, num7, buffer, offset2, 0);
				}
			}
			num4 += ((((uint)num3 & (true ? 1u : 0u)) != 0) ? (-num7) : num7);
		}
		if (((uint)num3 & 2u) != 0)
		{
			for (int k = 0; k < height; k++)
			{
				int num10 = offset + k * width;
				int num11 = num10 + width - 1;
				while (num10 < num11)
				{
					int num12 = raster[num10];
					raster[num10] = raster[num11];
					raster[num11] = num12;
					num10++;
					num11--;
				}
			}
		}
		return result;
	}

	private static bool gtTileSeparate(TiffRgbaImage img, int[] raster, int offset, int width, int height)
	{
		int num = img.tif.TileSize();
		byte[] buffer = new byte[((img.alpha != 0) ? 4 : 3) * num];
		int num2 = 0;
		int num3 = num2 + num;
		int num4 = num3 + num;
		int num5 = ((img.alpha != 0) ? (num4 + num) : (-1));
		int num6 = img.tif.GetField(TiffTag.TILEWIDTH)[0].ToInt();
		int num7 = img.tif.GetField(TiffTag.TILELENGTH)[0].ToInt();
		int num8 = img.setorientation();
		int num9;
		int num10;
		if (((uint)num8 & (true ? 1u : 0u)) != 0)
		{
			num9 = height - 1;
			num10 = -(num6 + width);
		}
		else
		{
			num9 = 0;
			num10 = -(num6 - width);
		}
		bool result = true;
		int num12;
		for (int i = 0; i < height; i += num12)
		{
			int num11 = num7 - (i + img.row_offset) % num7;
			num12 = ((i + num11 > height) ? (height - i) : num11);
			for (int j = 0; j < width; j += num6)
			{
				if (img.tif.ReadTile(buffer, num2, j + img.col_offset, i + img.row_offset, 0, 0) < 0 && img.stoponerr)
				{
					result = false;
					break;
				}
				if (img.tif.ReadTile(buffer, num3, j + img.col_offset, i + img.row_offset, 0, 1) < 0 && img.stoponerr)
				{
					result = false;
					break;
				}
				if (img.tif.ReadTile(buffer, num4, j + img.col_offset, i + img.row_offset, 0, 2) < 0 && img.stoponerr)
				{
					result = false;
					break;
				}
				if (img.alpha != 0 && img.tif.ReadTile(buffer, num5, j + img.col_offset, i + img.row_offset, 0, 3) < 0 && img.stoponerr)
				{
					result = false;
					break;
				}
				int num13 = (i + img.row_offset) % num7 * img.tif.TileRowSize();
				if (j + num6 > width)
				{
					int num14 = width - j;
					int num15 = num6 - num14;
					img.putSeparate(img, raster, offset + num9 * width + j, num10 + num15, j, num9, num14, num12, buffer, num2 + num13, num3 + num13, num4 + num13, (img.alpha != 0) ? (num5 + num13) : (-1), num15);
				}
				else
				{
					img.putSeparate(img, raster, offset + num9 * width + j, num10, j, num9, num6, num12, buffer, num2 + num13, num3 + num13, num4 + num13, (img.alpha != 0) ? (num5 + num13) : (-1), 0);
				}
			}
			num9 += ((((uint)num8 & (true ? 1u : 0u)) != 0) ? (-num12) : num12);
		}
		if (((uint)num8 & 2u) != 0)
		{
			for (int k = 0; k < height; k++)
			{
				int num16 = offset + k * width;
				int num17 = num16 + width - 1;
				while (num16 < num17)
				{
					int num18 = raster[num16];
					raster[num16] = raster[num17];
					raster[num17] = num18;
					num16++;
					num17--;
				}
			}
		}
		return result;
	}

	private static bool gtStripContig(TiffRgbaImage img, int[] raster, int offset, int width, int height)
	{
		byte[] buffer = new byte[img.tif.StripSize()];
		int num = img.setorientation();
		int num2;
		int rasterShift;
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			num2 = height - 1;
			rasterShift = -(width + width);
		}
		else
		{
			num2 = 0;
			rasterShift = -(width - width);
		}
		int num3 = img.tif.GetFieldDefaulted(TiffTag.ROWSPERSTRIP)[0].ToInt();
		if (num3 == -1)
		{
			num3 = int.MaxValue;
		}
		short num4 = img.tif.GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING)[1].ToShort();
		int num5 = img.tif.newScanlineSize();
		int bufferShift = ((width < img.width) ? (img.width - width) : 0);
		bool result = true;
		int num7;
		for (int i = 0; i < height; i += num7)
		{
			int num6 = num3 - (i + img.row_offset) % num3;
			num7 = ((i + num6 > height) ? (height - i) : num6);
			int num8 = num7;
			if (num8 % num4 != 0)
			{
				num8 += num4 - num8 % num4;
			}
			if (img.tif.ReadEncodedStrip(img.tif.ComputeStrip(i + img.row_offset, 0), buffer, 0, ((i + img.row_offset) % num3 + num8) * num5) < 0 && img.stoponerr)
			{
				result = false;
				break;
			}
			int offset2 = (i + img.row_offset) % num3 * num5;
			img.putContig(img, raster, offset + num2 * width, rasterShift, 0, num2, width, num7, buffer, offset2, bufferShift);
			num2 += ((((uint)num & (true ? 1u : 0u)) != 0) ? (-num7) : num7);
		}
		if (((uint)num & 2u) != 0)
		{
			for (int j = 0; j < height; j++)
			{
				int num9 = offset + j * width;
				int num10 = num9 + width - 1;
				while (num9 < num10)
				{
					int num11 = raster[num9];
					raster[num9] = raster[num10];
					raster[num10] = num11;
					num9++;
					num10--;
				}
			}
		}
		return result;
	}

	private static bool gtStripSeparate(TiffRgbaImage img, int[] raster, int offset, int width, int height)
	{
		int num = img.tif.StripSize();
		byte[] buffer = new byte[((img.alpha != 0) ? 4 : 3) * num];
		int num2 = 0;
		int num3 = num2 + num;
		int num4 = num3 + num;
		int num5 = num4 + num;
		num5 = ((img.alpha != 0) ? (num4 + num) : (-1));
		int num6 = img.setorientation();
		int num7;
		int rasterShift;
		if (((uint)num6 & (true ? 1u : 0u)) != 0)
		{
			num7 = height - 1;
			rasterShift = -(width + width);
		}
		else
		{
			num7 = 0;
			rasterShift = -(width - width);
		}
		int num8 = img.tif.GetFieldDefaulted(TiffTag.ROWSPERSTRIP)[0].ToInt();
		int num9 = img.tif.ScanlineSize();
		int bufferShift = ((width < img.width) ? (img.width - width) : 0);
		bool result = true;
		int num11;
		for (int i = 0; i < height; i += num11)
		{
			int num10 = num8 - (i + img.row_offset) % num8;
			num11 = ((i + num10 > height) ? (height - i) : num10);
			int row = i + img.row_offset;
			if (img.tif.ReadEncodedStrip(img.tif.ComputeStrip(row, 0), buffer, num2, ((i + img.row_offset) % num8 + num11) * num9) < 0 && img.stoponerr)
			{
				result = false;
				break;
			}
			if (img.tif.ReadEncodedStrip(img.tif.ComputeStrip(row, 1), buffer, num3, ((i + img.row_offset) % num8 + num11) * num9) < 0 && img.stoponerr)
			{
				result = false;
				break;
			}
			if (img.tif.ReadEncodedStrip(img.tif.ComputeStrip(row, 2), buffer, num4, ((i + img.row_offset) % num8 + num11) * num9) < 0 && img.stoponerr)
			{
				result = false;
				break;
			}
			if (img.alpha != 0 && img.tif.ReadEncodedStrip(img.tif.ComputeStrip(row, 3), buffer, num5, ((i + img.row_offset) % num8 + num11) * num9) < 0 && img.stoponerr)
			{
				result = false;
				break;
			}
			int num12 = (i + img.row_offset) % num8 * num9;
			img.putSeparate(img, raster, offset + num7 * width, rasterShift, 0, num7, width, num11, buffer, num2 + num12, num3 + num12, num4 + num12, (img.alpha != 0) ? (num5 + num12) : (-1), bufferShift);
			num7 += ((((uint)num6 & (true ? 1u : 0u)) != 0) ? (-num11) : num11);
		}
		if (((uint)num6 & 2u) != 0)
		{
			for (int j = 0; j < height; j++)
			{
				int num13 = offset + j * width;
				int num14 = num13 + width - 1;
				while (num13 < num14)
				{
					int num15 = raster[num13];
					raster[num13] = raster[num14];
					raster[num14] = num15;
					num13++;
					num14--;
				}
			}
		}
		return result;
	}

	private bool isCCITTCompression()
	{
		Compression compression = (Compression)tif.GetField(TiffTag.COMPRESSION)[0].ToInt();
		if (compression != Compression.CCITTFAX3 && compression != Compression.CCITTFAX4 && compression != Compression.CCITTRLE)
		{
			return compression == Compression.CCITTRLEW;
		}
		return true;
	}

	private int setorientation()
	{
		switch (orientation)
		{
		case Orientation.TOPLEFT:
		case Orientation.LEFTTOP:
			if (req_orientation == Orientation.TOPRIGHT || req_orientation == Orientation.RIGHTTOP)
			{
				return 2;
			}
			if (req_orientation == Orientation.BOTRIGHT || req_orientation == Orientation.RIGHTBOT)
			{
				return 3;
			}
			if (req_orientation == Orientation.BOTLEFT || req_orientation == Orientation.LEFTBOT)
			{
				return 1;
			}
			return 0;
		case Orientation.TOPRIGHT:
		case Orientation.RIGHTTOP:
			if (req_orientation == Orientation.TOPLEFT || req_orientation == Orientation.LEFTTOP)
			{
				return 2;
			}
			if (req_orientation == Orientation.BOTRIGHT || req_orientation == Orientation.RIGHTBOT)
			{
				return 1;
			}
			if (req_orientation == Orientation.BOTLEFT || req_orientation == Orientation.LEFTBOT)
			{
				return 3;
			}
			return 0;
		case Orientation.BOTRIGHT:
		case Orientation.RIGHTBOT:
			if (req_orientation == Orientation.TOPLEFT || req_orientation == Orientation.LEFTTOP)
			{
				return 3;
			}
			if (req_orientation == Orientation.TOPRIGHT || req_orientation == Orientation.RIGHTTOP)
			{
				return 1;
			}
			if (req_orientation == Orientation.BOTLEFT || req_orientation == Orientation.LEFTBOT)
			{
				return 2;
			}
			return 0;
		case Orientation.BOTLEFT:
		case Orientation.LEFTBOT:
			if (req_orientation == Orientation.TOPLEFT || req_orientation == Orientation.LEFTTOP)
			{
				return 1;
			}
			if (req_orientation == Orientation.TOPRIGHT || req_orientation == Orientation.RIGHTTOP)
			{
				return 3;
			}
			if (req_orientation == Orientation.BOTRIGHT || req_orientation == Orientation.RIGHTBOT)
			{
				return 2;
			}
			return 0;
		default:
			return 0;
		}
	}

	private bool pickContigCase()
	{
		get = (tif.IsTiled() ? new GetDelegate(gtTileContig) : new GetDelegate(gtStripContig));
		putContig = null;
		switch (photometric)
		{
		case Photometric.RGB:
			switch (bitspersample)
			{
			case 8:
				if (alpha == ExtraSample.ASSOCALPHA)
				{
					putContig = putRGBAAcontig8bittile;
				}
				else if (alpha == ExtraSample.UNASSALPHA)
				{
					putContig = putRGBUAcontig8bittile;
				}
				else
				{
					putContig = putRGBcontig8bittile;
				}
				break;
			case 16:
				if (alpha == ExtraSample.ASSOCALPHA)
				{
					putContig = putRGBAAcontig16bittile;
				}
				else if (alpha == ExtraSample.UNASSALPHA)
				{
					putContig = putRGBUAcontig16bittile;
				}
				else
				{
					putContig = putRGBcontig16bittile;
				}
				break;
			}
			break;
		case Photometric.SEPARATED:
			if (!buildMap() || bitspersample != 8)
			{
				break;
			}
			if (Map == null)
			{
				if (alpha == ExtraSample.ASSOCALPHA)
				{
					putContig = putRGBAcontig8bitCMYKAtile;
				}
				else
				{
					putContig = putRGBcontig8bitCMYKtile;
				}
			}
			else
			{
				putContig = putRGBcontig8bitCMYKMaptile;
			}
			break;
		case Photometric.PALETTE:
			if (buildMap())
			{
				switch (bitspersample)
				{
				case 8:
					putContig = put8bitcmaptile;
					break;
				case 4:
					putContig = put4bitcmaptile;
					break;
				case 2:
					putContig = put2bitcmaptile;
					break;
				case 1:
					putContig = put1bitcmaptile;
					break;
				}
			}
			break;
		case Photometric.MINISWHITE:
		case Photometric.MINISBLACK:
			if (!buildMap())
			{
				break;
			}
			switch (bitspersample)
			{
			case 16:
				putContig = put16bitbwtile;
				break;
			case 8:
				if (alpha == ExtraSample.ASSOCALPHA)
				{
					putContig = putgreywithalphatile;
				}
				else
				{
					putContig = putgreytile;
				}
				break;
			case 4:
				putContig = put4bitbwtile;
				break;
			case 2:
				putContig = put2bitbwtile;
				break;
			case 1:
				putContig = put1bitbwtile;
				break;
			}
			break;
		case Photometric.YCBCR:
			if (bitspersample == 8 && initYCbCrConversion())
			{
				FieldValue[] fieldDefaulted = tif.GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING);
				short num = fieldDefaulted[0].ToShort();
				short num2 = fieldDefaulted[1].ToShort();
				switch (((ushort)num << 4) | (ushort)num2)
				{
				case 68:
					putContig = putcontig8bitYCbCr44tile;
					break;
				case 66:
					putContig = putcontig8bitYCbCr42tile;
					break;
				case 65:
					putContig = putcontig8bitYCbCr41tile;
					break;
				case 34:
					putContig = putcontig8bitYCbCr22tile;
					break;
				case 33:
					putContig = putcontig8bitYCbCr21tile;
					break;
				case 18:
					putContig = putcontig8bitYCbCr12tile;
					break;
				case 17:
					putContig = putcontig8bitYCbCr11tile;
					break;
				}
			}
			break;
		case Photometric.CIELAB:
			if (buildMap() && bitspersample == 8)
			{
				putContig = initCIELabConversion();
			}
			break;
		}
		return putContig != null;
	}

	private bool pickSeparateCase()
	{
		get = (tif.IsTiled() ? new GetDelegate(gtTileSeparate) : new GetDelegate(gtStripSeparate));
		putSeparate = null;
		switch (photometric)
		{
		case Photometric.RGB:
			switch (bitspersample)
			{
			case 8:
				if (alpha == ExtraSample.ASSOCALPHA)
				{
					putSeparate = putRGBAAseparate8bittile;
				}
				else if (alpha == ExtraSample.UNASSALPHA)
				{
					putSeparate = putRGBUAseparate8bittile;
				}
				else
				{
					putSeparate = putRGBseparate8bittile;
				}
				break;
			case 16:
				if (alpha == ExtraSample.ASSOCALPHA)
				{
					putSeparate = putRGBAAseparate16bittile;
				}
				else if (alpha == ExtraSample.UNASSALPHA)
				{
					putSeparate = putRGBUAseparate16bittile;
				}
				else
				{
					putSeparate = putRGBseparate16bittile;
				}
				break;
			}
			break;
		case Photometric.YCBCR:
			if (bitspersample == 8 && samplesperpixel == 3 && initYCbCrConversion())
			{
				FieldValue[] fieldDefaulted = tif.GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING);
				short num = fieldDefaulted[0].ToShort();
				short num2 = fieldDefaulted[0].ToShort();
				if ((((ushort)num << 4) | (ushort)num2) == 17)
				{
					putSeparate = putseparate8bitYCbCr11tile;
				}
			}
			break;
		}
		return putSeparate != null;
	}

	private bool initYCbCrConversion()
	{
		if (ycbcr == null)
		{
			ycbcr = new TiffYCbCrToRGB();
		}
		float[] luma = tif.GetFieldDefaulted(TiffTag.YCBCRCOEFFICIENTS)[0].ToFloatArray();
		float[] refBlackWhite = tif.GetFieldDefaulted(TiffTag.REFERENCEBLACKWHITE)[0].ToFloatArray();
		ycbcr.Init(luma, refBlackWhite);
		return true;
	}

	private PutContigDelegate initCIELabConversion()
	{
		if (cielab == null)
		{
			cielab = new TiffCIELabToRGB();
		}
		float[] array = tif.GetFieldDefaulted(TiffTag.WHITEPOINT)[0].ToFloatArray();
		float[] array2 = new float[3];
		array2[1] = 100f;
		array2[0] = array[0] / array[1] * array2[1];
		array2[2] = (1f - array[0] - array[1]) / array[1] * array2[1];
		cielab.Init(display_sRGB, array2);
		return putcontig8bitCIELab;
	}

	private bool buildMap()
	{
		switch (photometric)
		{
		case Photometric.RGB:
		case Photometric.SEPARATED:
		case Photometric.YCBCR:
			if (bitspersample != 8 && !setupMap())
			{
				return false;
			}
			break;
		case Photometric.MINISWHITE:
		case Photometric.MINISBLACK:
			if (!setupMap())
			{
				return false;
			}
			break;
		case Photometric.PALETTE:
			if (checkcmap() == 16)
			{
				cvtcmap();
			}
			else
			{
				Tiff.WarningExt(tif, tif.m_clientdata, tif.FileName(), "Assuming 8-bit colormap");
			}
			if (bitspersample <= 8 && !makecmap())
			{
				return false;
			}
			break;
		}
		return true;
	}

	private bool setupMap()
	{
		int num = (1 << (int)bitspersample) - 1;
		if (bitspersample == 16)
		{
			num = 255;
		}
		Map = new byte[num + 1];
		if (photometric == Photometric.MINISWHITE)
		{
			for (int i = 0; i <= num; i++)
			{
				Map[i] = (byte)((num - i) * 255 / num);
			}
		}
		else
		{
			for (int j = 0; j <= num; j++)
			{
				Map[j] = (byte)(j * 255 / num);
			}
		}
		if (bitspersample <= 16 && (photometric == Photometric.MINISBLACK || photometric == Photometric.MINISWHITE))
		{
			if (!makebwmap())
			{
				return false;
			}
			Map = null;
		}
		return true;
	}

	private int checkcmap()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 1 << (int)bitspersample;
		while (num4-- > 0)
		{
			if (redcmap[num] >= 256 || greencmap[num2] >= 256 || bluecmap[num3] >= 256)
			{
				return 16;
			}
			num++;
			num2++;
			num3++;
		}
		return 8;
	}

	private void cvtcmap()
	{
		for (int num = (1 << (int)bitspersample) - 1; num >= 0; num--)
		{
			redcmap[num] >>= 8;
			greencmap[num] >>= 8;
			bluecmap[num] >>= 8;
		}
	}

	private bool makecmap()
	{
		int num = 8 / bitspersample;
		PALmap = new int[256][];
		for (int i = 0; i < 256; i++)
		{
			PALmap[i] = new int[num];
		}
		for (int j = 0; j < 256; j++)
		{
			int j2 = 0;
			switch (bitspersample)
			{
			case 1:
				CMAP(j >> 7, j, ref j2);
				CMAP((j >> 6) & 1, j, ref j2);
				CMAP((j >> 5) & 1, j, ref j2);
				CMAP((j >> 4) & 1, j, ref j2);
				CMAP((j >> 3) & 1, j, ref j2);
				CMAP((j >> 2) & 1, j, ref j2);
				CMAP((j >> 1) & 1, j, ref j2);
				CMAP(j & 1, j, ref j2);
				break;
			case 2:
				CMAP(j >> 6, j, ref j2);
				CMAP((j >> 4) & 3, j, ref j2);
				CMAP((j >> 2) & 3, j, ref j2);
				CMAP(j & 3, j, ref j2);
				break;
			case 4:
				CMAP(j >> 4, j, ref j2);
				CMAP(j & 0xF, j, ref j2);
				break;
			case 8:
				CMAP(j, j, ref j2);
				break;
			}
		}
		return true;
	}

	private bool makebwmap()
	{
		int num = 8 / bitspersample;
		if (num == 0)
		{
			num = 1;
		}
		BWmap = new int[256][];
		for (int i = 0; i < 256; i++)
		{
			BWmap[i] = new int[num];
		}
		for (int j = 0; j < 256; j++)
		{
			int j2 = 0;
			switch (bitspersample)
			{
			case 1:
				GREY(j >> 7, j, ref j2);
				GREY((j >> 6) & 1, j, ref j2);
				GREY((j >> 5) & 1, j, ref j2);
				GREY((j >> 4) & 1, j, ref j2);
				GREY((j >> 3) & 1, j, ref j2);
				GREY((j >> 2) & 1, j, ref j2);
				GREY((j >> 1) & 1, j, ref j2);
				GREY(j & 1, j, ref j2);
				break;
			case 2:
				GREY(j >> 6, j, ref j2);
				GREY((j >> 4) & 3, j, ref j2);
				GREY((j >> 2) & 3, j, ref j2);
				GREY(j & 3, j, ref j2);
				break;
			case 4:
				GREY(j >> 4, j, ref j2);
				GREY(j & 0xF, j, ref j2);
				break;
			case 8:
			case 16:
				GREY(j, j, ref j2);
				break;
			}
		}
		return true;
	}

	private void YCbCrtoRGB(out int dst, int Y, int Cb, int Cr)
	{
		ycbcr.YCbCrtoRGB(Y, Cb, Cr, out var r, out var g, out var b);
		dst = PACK(r, g, b);
	}

	private static void put8bitcmaptile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int[][] pALmap = img.PALmap;
		int num = img.samplesperpixel;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				raster[rasterOffset] = pALmap[buffer[offset]][0];
				rasterOffset++;
				offset += num;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void put4bitcmaptile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int[][] pALmap = img.PALmap;
		bufferShift /= 2;
		while (height-- > 0)
		{
			int[] array = null;
			int num;
			for (num = width; num >= 2; num -= 2)
			{
				array = pALmap[buffer[offset]];
				offset++;
				for (int i = 0; i < 2; i++)
				{
					raster[rasterOffset] = array[i];
					rasterOffset++;
				}
			}
			if (num != 0)
			{
				array = pALmap[buffer[offset]];
				offset++;
				raster[rasterOffset] = array[0];
				rasterOffset++;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void put2bitcmaptile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int[][] pALmap = img.PALmap;
		bufferShift /= 4;
		while (height-- > 0)
		{
			int[] array = null;
			int num;
			for (num = width; num >= 4; num -= 4)
			{
				array = pALmap[buffer[offset]];
				offset++;
				for (int i = 0; i < 4; i++)
				{
					raster[rasterOffset] = array[i];
					rasterOffset++;
				}
			}
			if (num > 0)
			{
				array = pALmap[buffer[offset]];
				offset++;
				if (num <= 3 && num > 0)
				{
					for (int j = 0; j < num; j++)
					{
						raster[rasterOffset] = array[j];
						rasterOffset++;
					}
				}
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void put1bitcmaptile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int[][] pALmap = img.PALmap;
		bufferShift /= 8;
		while (height-- > 0)
		{
			int num = 0;
			int num2;
			for (num2 = width; num2 >= 8; num2 -= 8)
			{
				int[] array = pALmap[buffer[offset++]];
				num = 0;
				for (int i = 0; i < 8; i++)
				{
					raster[rasterOffset++] = array[num++];
				}
			}
			if (num2 > 0)
			{
				int[] array = pALmap[buffer[offset++]];
				num = 0;
				if (num2 <= 7 && num2 > 0)
				{
					for (int j = 0; j < num2; j++)
					{
						raster[rasterOffset++] = array[num++];
					}
				}
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putgreytile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		int[][] bWmap = img.BWmap;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				raster[rasterOffset] = bWmap[buffer[offset]][0];
				rasterOffset++;
				offset += num;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putgreywithalphatile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		int[][] bWmap = img.BWmap;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				raster[rasterOffset] = PACK4(bWmap[buffer[offset]][0], buffer[offset + 1] & 0xFF);
				rasterOffset++;
				offset += num;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void put16bitbwtile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		int[][] bWmap = img.BWmap;
		while (height-- > 0)
		{
			short[] array = Tiff.ByteArrayToShorts(buffer, offset, buffer.Length - offset);
			int num2 = 0;
			x = width;
			while (x-- > 0)
			{
				raster[rasterOffset] = bWmap[(array[num2] & 0xFFFF) >> 8][0];
				rasterOffset++;
				offset += 2 * num;
				num2 += num;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void put1bitbwtile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int[][] bWmap = img.BWmap;
		bufferShift /= 8;
		while (height-- > 0)
		{
			int[] array = null;
			int num;
			for (num = width; num >= 8; num -= 8)
			{
				array = bWmap[buffer[offset]];
				offset++;
				for (int i = 0; i < 8; i++)
				{
					raster[rasterOffset] = array[i];
					rasterOffset++;
				}
			}
			if (num > 0)
			{
				array = bWmap[buffer[offset]];
				offset++;
				if (num <= 7 && num > 0)
				{
					for (int j = 0; j < num; j++)
					{
						raster[rasterOffset] = array[j];
						rasterOffset++;
					}
				}
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void put2bitbwtile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int[][] bWmap = img.BWmap;
		bufferShift /= 4;
		while (height-- > 0)
		{
			int[] array = null;
			int num;
			for (num = width; num >= 4; num -= 4)
			{
				array = bWmap[buffer[offset]];
				offset++;
				for (int i = 0; i < 4; i++)
				{
					raster[rasterOffset] = array[i];
					rasterOffset++;
				}
			}
			if (num > 0)
			{
				array = bWmap[buffer[offset]];
				offset++;
				if (num <= 3 && num > 0)
				{
					for (int j = 0; j < num; j++)
					{
						raster[rasterOffset] = array[j];
						rasterOffset++;
					}
				}
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void put4bitbwtile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int[][] bWmap = img.BWmap;
		bufferShift /= 2;
		while (height-- > 0)
		{
			int[] array = null;
			int num;
			for (num = width; num >= 2; num -= 2)
			{
				array = bWmap[buffer[offset]];
				offset++;
				for (int i = 0; i < 2; i++)
				{
					raster[rasterOffset] = array[i];
					rasterOffset++;
				}
			}
			if (num != 0)
			{
				array = bWmap[buffer[offset]];
				offset++;
				raster[rasterOffset] = array[0];
				rasterOffset++;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putRGBcontig8bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		bufferShift *= num;
		while (height-- > 0)
		{
			int num2;
			for (num2 = width; num2 >= 8; num2 -= 8)
			{
				for (int i = 0; i < 8; i++)
				{
					raster[rasterOffset] = PACK(buffer[offset], buffer[offset + 1], buffer[offset + 2]);
					rasterOffset++;
					offset += num;
				}
			}
			if (num2 > 0 && num2 <= 7 && num2 > 0)
			{
				for (int num3 = num2; num3 > 0; num3--)
				{
					raster[rasterOffset] = PACK(buffer[offset], buffer[offset + 1], buffer[offset + 2]);
					rasterOffset++;
					offset += num;
				}
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putRGBAAcontig8bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		bufferShift *= num;
		while (height-- > 0)
		{
			int num2;
			for (num2 = width; num2 >= 8; num2 -= 8)
			{
				for (int i = 0; i < 8; i++)
				{
					raster[rasterOffset] = PACK4(buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3]);
					rasterOffset++;
					offset += num;
				}
			}
			if (num2 > 0 && num2 <= 7 && num2 > 0)
			{
				for (int num3 = num2; num3 > 0; num3--)
				{
					raster[rasterOffset] = PACK4(buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3]);
					rasterOffset++;
					offset += num;
				}
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putRGBUAcontig8bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		bufferShift *= num;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				int num2 = buffer[offset + 3];
				int r = (buffer[offset] * num2 + 127) / 255;
				int g = (buffer[offset + 1] * num2 + 127) / 255;
				int b = (buffer[offset + 2] * num2 + 127) / 255;
				raster[rasterOffset] = PACK4(r, g, b, num2);
				rasterOffset++;
				offset += num;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putRGBcontig16bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		bufferShift *= num;
		short[] array = Tiff.ByteArrayToShorts(buffer, offset, buffer.Length);
		int num2 = 0;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				raster[rasterOffset] = PACKW(array[num2], array[num2 + 1], array[num2 + 2]);
				rasterOffset++;
				num2 += num;
			}
			rasterOffset += rasterShift;
			num2 += bufferShift;
		}
	}

	private static void putRGBAAcontig16bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		short[] array = Tiff.ByteArrayToShorts(buffer, offset, buffer.Length);
		int num2 = 0;
		bufferShift *= num;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				raster[rasterOffset] = PACKW4(array[num2], array[num2 + 1], array[num2 + 2], array[num2 + 3]);
				rasterOffset++;
				num2 += num;
			}
			rasterOffset += rasterShift;
			num2 += bufferShift;
		}
	}

	private static void putRGBUAcontig16bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		bufferShift *= num;
		short[] array = Tiff.ByteArrayToShorts(buffer, offset, buffer.Length);
		int num2 = 0;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				int num3 = W2B(array[num2 + 3]);
				int r = (W2B(array[num2]) * num3 + 127) / 255;
				int g = (W2B(array[num2 + 1]) * num3 + 127) / 255;
				int b = (W2B(array[num2 + 2]) * num3 + 127) / 255;
				raster[rasterOffset] = PACK4(r, g, b, num3);
				rasterOffset++;
				num2 += num;
			}
			rasterOffset += rasterShift;
			num2 += bufferShift;
		}
	}

	private static void putRGBAcontig8bitCMYKAtile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		bufferShift *= num;
		while (height-- > 0)
		{
			int num2;
			for (num2 = width; num2 >= 8; num2 -= 8)
			{
				for (int i = 0; i < 8; i++)
				{
					short num3 = (short)(255 - buffer[offset + 3]);
					short r = (short)(num3 * (255 - buffer[offset]) / 255);
					short g = (short)(num3 * (255 - buffer[offset + 1]) / 255);
					short b = (short)(num3 * (255 - buffer[offset + 2]) / 255);
					short a = buffer[offset + 4];
					raster[rasterOffset] = PACK4(r, g, b, a);
					rasterOffset++;
					offset += num;
				}
			}
			if (num2 > 0 && num2 <= 7 && num2 > 0)
			{
				for (int num4 = num2; num4 > 0; num4--)
				{
					short num5 = (short)(255 - buffer[offset + 3]);
					short r2 = (short)(num5 * (255 - buffer[offset]) / 255);
					short g2 = (short)(num5 * (255 - buffer[offset + 1]) / 255);
					short b2 = (short)(num5 * (255 - buffer[offset + 2]) / 255);
					short a2 = buffer[offset + 4];
					raster[rasterOffset] = PACK4(r2, g2, b2, a2);
					rasterOffset++;
					offset += num;
				}
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putRGBcontig8bitCMYKtile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		bufferShift *= num;
		while (height-- > 0)
		{
			int num2;
			for (num2 = width; num2 >= 8; num2 -= 8)
			{
				for (int i = 0; i < 8; i++)
				{
					short num3 = (short)(255 - buffer[offset + 3]);
					short r = (short)(num3 * (255 - buffer[offset]) / 255);
					short g = (short)(num3 * (255 - buffer[offset + 1]) / 255);
					short b = (short)(num3 * (255 - buffer[offset + 2]) / 255);
					raster[rasterOffset] = PACK(r, g, b);
					rasterOffset++;
					offset += num;
				}
			}
			if (num2 > 0 && num2 <= 7 && num2 > 0)
			{
				for (int num4 = num2; num4 > 0; num4--)
				{
					short num5 = (short)(255 - buffer[offset + 3]);
					short r2 = (short)(num5 * (255 - buffer[offset]) / 255);
					short g2 = (short)(num5 * (255 - buffer[offset + 1]) / 255);
					short b2 = (short)(num5 * (255 - buffer[offset + 2]) / 255);
					raster[rasterOffset] = PACK(r2, g2, b2);
					rasterOffset++;
					offset += num;
				}
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putcontig8bitCIELab(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		bufferShift *= 3;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				img.cielab.CIELabToXYZ(buffer[offset], (sbyte)buffer[offset + 1], (sbyte)buffer[offset + 2], out var X, out var Y, out var Z);
				img.cielab.XYZToRGB(X, Y, Z, out var r, out var g, out var b);
				raster[rasterOffset] = PACK(r, g, b);
				rasterOffset++;
				offset += 3;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
	}

	private static void putcontig8bitYCbCr22tile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		bufferShift = bufferShift / 2 * 6;
		int num = rasterOffset + width + rasterShift;
		while (height >= 2)
		{
			for (x = width; x >= 2; x -= 2)
			{
				int cb = buffer[offset + 4];
				int cr = buffer[offset + 5];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb, cr);
				img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb, cr);
				img.YCbCrtoRGB(out raster[num], buffer[offset + 2], cb, cr);
				img.YCbCrtoRGB(out raster[num + 1], buffer[offset + 3], cb, cr);
				rasterOffset += 2;
				num += 2;
				offset += 6;
			}
			if (x == 1)
			{
				int cb2 = buffer[offset + 4];
				int cr2 = buffer[offset + 5];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb2, cr2);
				img.YCbCrtoRGB(out raster[num], buffer[offset + 2], cb2, cr2);
				rasterOffset++;
				num++;
				offset += 6;
			}
			rasterOffset += rasterShift * 2 + width;
			num += rasterShift * 2 + width;
			offset += bufferShift;
			height -= 2;
		}
		if (height == 1)
		{
			for (x = width; x >= 2; x -= 2)
			{
				int cb3 = buffer[offset + 4];
				int cr3 = buffer[offset + 5];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb3, cr3);
				img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb3, cr3);
				rasterOffset += 2;
				num += 2;
				offset += 6;
			}
			if (x == 1)
			{
				int cb4 = buffer[offset + 4];
				int cr4 = buffer[offset + 5];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb4, cr4);
			}
		}
	}

	private static void putcontig8bitYCbCr21tile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		bufferShift = bufferShift * 4 / 2;
		do
		{
			x = width >> 1;
			do
			{
				int cb = buffer[offset + 2];
				int cr = buffer[offset + 3];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb, cr);
				img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb, cr);
				rasterOffset += 2;
				offset += 4;
			}
			while (--x != 0);
			if (((uint)width & (true ? 1u : 0u)) != 0)
			{
				int cb2 = buffer[offset + 2];
				int cr2 = buffer[offset + 3];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb2, cr2);
				rasterOffset++;
				offset += 4;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
		while (--height != 0);
	}

	private static void putcontig8bitYCbCr44tile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = rasterOffset + width + rasterShift;
		int num2 = num + width + rasterShift;
		int num3 = num2 + width + rasterShift;
		int num4 = 3 * width + 4 * rasterShift;
		bufferShift = bufferShift * 18 / 4;
		if ((height & 3) == 0 && (width & 3) == 0)
		{
			while (height >= 4)
			{
				x = width >> 2;
				do
				{
					int cb = buffer[offset + 16];
					int cr = buffer[offset + 17];
					img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb, cr);
					img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb, cr);
					img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], cb, cr);
					img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], cb, cr);
					img.YCbCrtoRGB(out raster[num], buffer[offset + 4], cb, cr);
					img.YCbCrtoRGB(out raster[num + 1], buffer[offset + 5], cb, cr);
					img.YCbCrtoRGB(out raster[num + 2], buffer[offset + 6], cb, cr);
					img.YCbCrtoRGB(out raster[num + 3], buffer[offset + 7], cb, cr);
					img.YCbCrtoRGB(out raster[num2], buffer[offset + 8], cb, cr);
					img.YCbCrtoRGB(out raster[num2 + 1], buffer[offset + 9], cb, cr);
					img.YCbCrtoRGB(out raster[num2 + 2], buffer[offset + 10], cb, cr);
					img.YCbCrtoRGB(out raster[num2 + 3], buffer[offset + 11], cb, cr);
					img.YCbCrtoRGB(out raster[num3], buffer[offset + 12], cb, cr);
					img.YCbCrtoRGB(out raster[num3 + 1], buffer[offset + 13], cb, cr);
					img.YCbCrtoRGB(out raster[num3 + 2], buffer[offset + 14], cb, cr);
					img.YCbCrtoRGB(out raster[num3 + 3], buffer[offset + 15], cb, cr);
					rasterOffset += 4;
					num += 4;
					num2 += 4;
					num3 += 4;
					offset += 18;
				}
				while (--x != 0);
				rasterOffset += num4;
				num += num4;
				num2 += num4;
				num3 += num4;
				offset += bufferShift;
				height -= 4;
			}
			return;
		}
		while (height > 0)
		{
			x = width;
			while (x > 0)
			{
				int cb2 = buffer[offset + 16];
				int cr2 = buffer[offset + 17];
				bool flag = false;
				bool flag2 = false;
				if (x < 1 || x > 3)
				{
					flag = false;
					if (height < 1 || height > 3)
					{
						img.YCbCrtoRGB(out raster[num3 + 3], buffer[offset + 15], cb2, cr2);
						flag = true;
					}
					if (height == 3 || flag)
					{
						img.YCbCrtoRGB(out raster[num2 + 3], buffer[offset + 11], cb2, cr2);
						flag = true;
					}
					if (height == 2 || flag)
					{
						img.YCbCrtoRGB(out raster[num + 3], buffer[offset + 7], cb2, cr2);
						flag = true;
					}
					if (height == 1 || flag)
					{
						img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], cb2, cr2);
					}
					flag2 = true;
				}
				if (x == 3 || flag2)
				{
					flag = false;
					if (height < 1 || height > 3)
					{
						img.YCbCrtoRGB(out raster[num3 + 2], buffer[offset + 14], cb2, cr2);
						flag = true;
					}
					if (height == 3 || flag)
					{
						img.YCbCrtoRGB(out raster[num2 + 2], buffer[offset + 10], cb2, cr2);
						flag = true;
					}
					if (height == 2 || flag)
					{
						img.YCbCrtoRGB(out raster[num + 2], buffer[offset + 6], cb2, cr2);
						flag = true;
					}
					if (height == 1 || flag)
					{
						img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], cb2, cr2);
					}
					flag2 = true;
				}
				if (x == 2 || flag2)
				{
					flag = false;
					if (height < 1 || height > 3)
					{
						img.YCbCrtoRGB(out raster[num3 + 1], buffer[offset + 13], cb2, cr2);
						flag = true;
					}
					if (height == 3 || flag)
					{
						img.YCbCrtoRGB(out raster[num2 + 1], buffer[offset + 9], cb2, cr2);
						flag = true;
					}
					if (height == 2 || flag)
					{
						img.YCbCrtoRGB(out raster[num + 1], buffer[offset + 5], cb2, cr2);
						flag = true;
					}
					if (height == 1 || flag)
					{
						img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb2, cr2);
					}
				}
				if (x == 1 || flag2)
				{
					flag = false;
					if (height < 1 || height > 3)
					{
						img.YCbCrtoRGB(out raster[num3], buffer[offset + 12], cb2, cr2);
						flag = true;
					}
					if (height == 3 || flag)
					{
						img.YCbCrtoRGB(out raster[num2], buffer[offset + 8], cb2, cr2);
						flag = true;
					}
					if (height == 2 || flag)
					{
						img.YCbCrtoRGB(out raster[num], buffer[offset + 4], cb2, cr2);
						flag = true;
					}
					if (height == 1 || flag)
					{
						img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb2, cr2);
					}
				}
				if (x < 4)
				{
					rasterOffset += x;
					num += x;
					num2 += x;
					num3 += x;
					x = 0;
				}
				else
				{
					rasterOffset += 4;
					num += 4;
					num2 += 4;
					num3 += 4;
					x -= 4;
				}
				offset += 18;
			}
			if (height > 4)
			{
				height -= 4;
				rasterOffset += num4;
				num += num4;
				num2 += num4;
				num3 += num4;
				offset += bufferShift;
				continue;
			}
			break;
		}
	}

	private static void putcontig8bitYCbCr42tile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = rasterOffset + width + rasterShift;
		int num2 = 2 * rasterShift + width;
		bufferShift = bufferShift * 10 / 4;
		if ((height & 3) == 0 && (width & 1) == 0)
		{
			while (height >= 2)
			{
				x = width >> 2;
				do
				{
					int cb = buffer[offset + 8];
					int cr = buffer[offset + 9];
					img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb, cr);
					img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb, cr);
					img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], cb, cr);
					img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], cb, cr);
					img.YCbCrtoRGB(out raster[num], buffer[offset + 4], cb, cr);
					img.YCbCrtoRGB(out raster[num + 1], buffer[offset + 5], cb, cr);
					img.YCbCrtoRGB(out raster[num + 2], buffer[offset + 6], cb, cr);
					img.YCbCrtoRGB(out raster[num + 3], buffer[offset + 7], cb, cr);
					rasterOffset += 4;
					num += 4;
					offset += 10;
				}
				while (--x != 0);
				rasterOffset += num2;
				num += num2;
				offset += bufferShift;
				height -= 2;
			}
			return;
		}
		while (height > 0)
		{
			x = width;
			while (x > 0)
			{
				int cb2 = buffer[offset + 8];
				int cr2 = buffer[offset + 9];
				bool flag = false;
				if (x < 1 || x > 3)
				{
					if (height != 1)
					{
						img.YCbCrtoRGB(out raster[num + 3], buffer[offset + 7], cb2, cr2);
					}
					img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], cb2, cr2);
					flag = true;
				}
				if (x == 3 || flag)
				{
					if (height != 1)
					{
						img.YCbCrtoRGB(out raster[num + 2], buffer[offset + 6], cb2, cr2);
					}
					img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], cb2, cr2);
					flag = true;
				}
				if (x == 2 || flag)
				{
					if (height != 1)
					{
						img.YCbCrtoRGB(out raster[num + 1], buffer[offset + 5], cb2, cr2);
					}
					img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb2, cr2);
					flag = true;
				}
				if (x == 1 || flag)
				{
					if (height != 1)
					{
						img.YCbCrtoRGB(out raster[num], buffer[offset + 4], cb2, cr2);
					}
					img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb2, cr2);
				}
				if (x < 4)
				{
					rasterOffset += x;
					num += x;
					x = 0;
				}
				else
				{
					rasterOffset += 4;
					num += 4;
					x -= 4;
				}
				offset += 10;
			}
			if (height > 2)
			{
				height -= 2;
				rasterOffset += num2;
				num += num2;
				offset += bufferShift;
				continue;
			}
			break;
		}
	}

	private static void putcontig8bitYCbCr41tile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		do
		{
			x = width >> 2;
			do
			{
				int cb = buffer[offset + 4];
				int cr = buffer[offset + 5];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb, cr);
				img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb, cr);
				img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], cb, cr);
				img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], cb, cr);
				rasterOffset += 4;
				offset += 6;
			}
			while (--x != 0);
			if (((uint)width & 3u) != 0)
			{
				int cb2 = buffer[offset + 4];
				int cr2 = buffer[offset + 5];
				int num = width & 3;
				if (num == 3)
				{
					img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], cb2, cr2);
				}
				if (num == 3 || num == 2)
				{
					img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], cb2, cr2);
				}
				if (num == 3 || num == 2 || num == 1)
				{
					img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb2, cr2);
				}
				rasterOffset += num;
				offset += 6;
			}
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
		while (--height != 0);
	}

	private static void putcontig8bitYCbCr11tile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		bufferShift *= 3;
		do
		{
			x = width;
			do
			{
				int cb = buffer[offset + 1];
				int cr = buffer[offset + 2];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb, cr);
				rasterOffset++;
				offset += 3;
			}
			while (--x != 0);
			rasterOffset += rasterShift;
			offset += bufferShift;
		}
		while (--height != 0);
	}

	private static void putcontig8bitYCbCr12tile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		bufferShift = bufferShift / 2 * 4;
		int num = rasterOffset + width + rasterShift;
		while (height >= 2)
		{
			x = width;
			do
			{
				int cb = buffer[offset + 2];
				int cr = buffer[offset + 3];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb, cr);
				img.YCbCrtoRGB(out raster[num], buffer[offset + 1], cb, cr);
				rasterOffset++;
				num++;
				offset += 4;
			}
			while (--x != 0);
			rasterOffset += rasterShift * 2 + width;
			num += rasterShift * 2 + width;
			offset += bufferShift;
			height -= 2;
		}
		if (height == 1)
		{
			x = width;
			do
			{
				int cb2 = buffer[offset + 2];
				int cr2 = buffer[offset + 3];
				img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset], cb2, cr2);
				rasterOffset++;
				offset += 4;
			}
			while (--x != 0);
		}
	}

	private static void putRGBseparate8bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
	{
		while (height-- > 0)
		{
			int num;
			for (num = width; num >= 8; num -= 8)
			{
				for (int i = 0; i < 8; i++)
				{
					raster[rasterOffset] = PACK(buffer[offset1], buffer[offset2], buffer[offset3]);
					rasterOffset++;
					offset1++;
					offset2++;
					offset3++;
				}
			}
			if (num > 0 && num <= 7 && num > 0)
			{
				for (int num2 = num; num2 > 0; num2--)
				{
					raster[rasterOffset] = PACK(buffer[offset1], buffer[offset2], buffer[offset3]);
					rasterOffset++;
					offset1++;
					offset2++;
					offset3++;
				}
			}
			offset1 += bufferShift;
			offset2 += bufferShift;
			offset3 += bufferShift;
			rasterOffset += rasterShift;
		}
	}

	private static void putRGBAAseparate8bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
	{
		while (height-- > 0)
		{
			int num;
			for (num = width; num >= 8; num -= 8)
			{
				for (int i = 0; i < 8; i++)
				{
					raster[rasterOffset] = PACK4(buffer[offset1], buffer[offset2], buffer[offset3], buffer[offset4]);
					rasterOffset++;
					offset1++;
					offset2++;
					offset3++;
					offset4++;
				}
			}
			if (num > 0 && num <= 7 && num > 0)
			{
				for (int num2 = num; num2 > 0; num2--)
				{
					raster[rasterOffset] = PACK4(buffer[offset1], buffer[offset2], buffer[offset3], buffer[offset4]);
					rasterOffset++;
					offset1++;
					offset2++;
					offset3++;
					offset4++;
				}
			}
			offset1 += bufferShift;
			offset2 += bufferShift;
			offset3 += bufferShift;
			offset4 += bufferShift;
			rasterOffset += rasterShift;
		}
	}

	private static void putRGBUAseparate8bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
	{
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				int num = buffer[offset4];
				int r = (buffer[offset1] * num + 127) / 255;
				int g = (buffer[offset2] * num + 127) / 255;
				int b = (buffer[offset3] * num + 127) / 255;
				raster[rasterOffset] = PACK4(r, g, b, num);
				rasterOffset++;
				offset1++;
				offset2++;
				offset3++;
				offset4++;
			}
			offset1 += bufferShift;
			offset2 += bufferShift;
			offset3 += bufferShift;
			offset4 += bufferShift;
			rasterOffset += rasterShift;
		}
	}

	private static void putRGBseparate16bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
	{
		short[] array = Tiff.ByteArrayToShorts(buffer, 0, buffer.Length);
		offset1 /= 2;
		offset2 /= 2;
		offset3 /= 2;
		while (height-- > 0)
		{
			for (x = 0; x < width; x++)
			{
				raster[rasterOffset] = PACKW(array[offset1], array[offset2], array[offset3]);
				rasterOffset++;
				offset1++;
				offset2++;
				offset3++;
			}
			offset1 += bufferShift;
			offset2 += bufferShift;
			offset3 += bufferShift;
			rasterOffset += rasterShift;
		}
	}

	private static void putRGBAAseparate16bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
	{
		short[] array = Tiff.ByteArrayToShorts(buffer, 0, buffer.Length);
		offset1 /= 2;
		offset2 /= 2;
		offset3 /= 2;
		offset4 /= 2;
		while (height-- > 0)
		{
			for (x = 0; x < width; x++)
			{
				raster[rasterOffset] = PACKW4(array[offset1], array[offset2], array[offset3], array[offset4]);
				rasterOffset++;
				offset1++;
				offset2++;
				offset3++;
				offset4++;
			}
			offset1 += bufferShift;
			offset2 += bufferShift;
			offset3 += bufferShift;
			offset4 += bufferShift;
			rasterOffset += rasterShift;
		}
	}

	private static void putRGBUAseparate16bittile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
	{
		short[] array = Tiff.ByteArrayToShorts(buffer, 0, buffer.Length);
		offset1 /= 2;
		offset2 /= 2;
		offset3 /= 2;
		offset4 /= 2;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				int num = W2B(array[offset4]);
				int r = (W2B(array[offset1]) * num + 127) / 255;
				int g = (W2B(array[offset2]) * num + 127) / 255;
				int b = (W2B(array[offset3]) * num + 127) / 255;
				raster[rasterOffset] = PACK4(r, g, b, num);
				rasterOffset++;
				offset1++;
				offset2++;
				offset3++;
				offset4++;
			}
			offset1 += bufferShift;
			offset2 += bufferShift;
			offset3 += bufferShift;
			offset4 += bufferShift;
			rasterOffset += rasterShift;
		}
	}

	private static void putseparate8bitYCbCr11tile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
	{
		while (height-- > 0)
		{
			x = width;
			do
			{
				img.ycbcr.YCbCrtoRGB(buffer[offset1], buffer[offset2], buffer[offset3], out var r, out var g, out var b);
				raster[rasterOffset] = PACK(r, g, b);
				rasterOffset++;
				offset1++;
				offset2++;
				offset3++;
			}
			while (--x != 0);
			offset1 += bufferShift;
			offset2 += bufferShift;
			offset3 += bufferShift;
			rasterOffset += rasterShift;
		}
	}

	private static void putRGBcontig8bitCMYKMaptile(TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift, int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
	{
		int num = img.samplesperpixel;
		byte[] map = img.Map;
		bufferShift *= num;
		while (height-- > 0)
		{
			x = width;
			while (x-- > 0)
			{
				short num2 = (short)(255 - buffer[offset + 3]);
				short num3 = (short)(num2 * (255 - buffer[offset]) / 255);
				short num4 = (short)(num2 * (255 - buffer[offset + 1]) / 255);
				short num5 = (short)(num2 * (255 - buffer[offset + 2]) / 255);
				raster[rasterOffset] = PACK(map[num3], map[num4], map[num5]);
				rasterOffset++;
				offset += num;
			}
			offset += bufferShift;
			rasterOffset += rasterShift;
		}
	}
}
