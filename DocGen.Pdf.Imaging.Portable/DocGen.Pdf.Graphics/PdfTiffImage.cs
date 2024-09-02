using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BitMiracle.LibTiff.Classic;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Compression;
using DocGen.Pdf.Graphics.Images.Decoder;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf.Graphics;

public class PdfTiffImage : PdfImage, IDisposable
{
	[Flags]
	private enum TiffColorSpace
	{
		Unknown = 0,
		BitLevel = 1,
		Gray = 2,
		Rgb = 4,
		Cmyk = 8,
		Lab = 0x10,
		Palette = 0x1000,
		CalGray = 0x20,
		CalRgb = 0x40,
		IccBased = 0x80
	}

	[Flags]
	private enum TiffCompresser
	{
		None = 0,
		CompressG4 = 1,
		CompressJpeg = 2,
		CompressZip = 4
	}

	[Flags]
	private enum TiffTranscode
	{
		Unknown = 0,
		Raw = 1,
		Encode = 2
	}

	[Flags]
	private enum TiffToImage
	{
		None = 0,
		AbgrToRgb = 1,
		RgbaToRgb = 2,
		RgbaaToRgb = 4,
		YcbcrToRgb = 8,
		YcbcrToLab = 0x10,
		Palette = 0x20,
		SignToUnsigned = 0x40,
		LabSignToUnsigned = 0x40,
		PlannerToContig = 0x100
	}

	private Stream imageStream;

	private Tiff tiff;

	private bool frameCount;

	private int m_frameCount;

	private int m_activeFrame;

	private SizeF m_phisicalDimension;

	private SKBitmap skBitmap;

	private float m_dpiX = 96f;

	private float m_dpiY = 96f;

	private PdfMask m_mask;

	private int m_tiffWidth;

	private int m_tiffHeight;

	private BitMiracle.LibTiff.Classic.Compression m_tiffCompressed;

	private short m_tiffBitsPerSample;

	private short m_tiffSamplePerPixel;

	private Photometric m_tiffPhotoMetric;

	private FillOrder m_tiffFillOrder;

	private TiffColorSpace m_imageColorSpace;

	private bool m_imageDecode;

	private int m_imagePaletteSize;

	private byte[] m_imagePaletteData;

	private int[] m_imageLabRange = new int[4];

	private PlanarConfig m_tiffPlanar;

	private TiffCompresser m_imageCompression;

	private TiffTranscode m_imageTranscode;

	private TiffToImage m_imageSample;

	private float[] m_tiffWhiteChrom = new float[2];

	private float[] m_tiffPrimaryChrom = new float[6];

	private int m_tiffIccLen;

	private byte[] m_tiffIccData;

	private TiffCompresser m_imageDefaultCompresser;

	private bool m_imageNoPass;

	private int m_tiffDataSize;

	public int FrameCount
	{
		get
		{
			if (tiff != null && !frameCount)
			{
				do
				{
					m_frameCount++;
				}
				while (tiff.ReadDirectory());
				frameCount = true;
			}
			else if (skBitmap != null && !frameCount)
			{
				m_frameCount = 1;
				frameCount = true;
			}
			return m_frameCount;
		}
	}

	public override int Height
	{
		get
		{
			if (tiff != null)
			{
				tiff.SetDirectory((short)ActiveFrame);
				return base.Height = tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH)[0].ToInt();
			}
			if (skBitmap != null)
			{
				base.Height = skBitmap.Height;
				return skBitmap.Height;
			}
			return 0;
		}
	}

	public override int Width
	{
		get
		{
			if (tiff != null)
			{
				tiff.SetDirectory((short)ActiveFrame);
				return tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH)[0].ToInt();
			}
			if (skBitmap != null)
			{
				return skBitmap.Width;
			}
			return 0;
		}
	}

	public int ActiveFrame
	{
		get
		{
			return m_activeFrame;
		}
		set
		{
			if (value < 0 || value >= FrameCount)
			{
				throw new ArgumentOutOfRangeException("ActiveFrame");
			}
			m_activeFrame = value;
		}
	}

	public override float VerticalResolution
	{
		get
		{
			if (tiff != null)
			{
				tiff.SetDirectory((short)ActiveFrame);
				return tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.YRESOLUTION)[0].ToFloat();
			}
			return m_dpiY;
		}
	}

	public override float HorizontalResolution
	{
		get
		{
			if (tiff != null)
			{
				tiff.SetDirectory((short)ActiveFrame);
				return tiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.XRESOLUTION)[0].ToFloat();
			}
			return m_dpiX;
		}
	}

	public override SizeF PhysicalDimension
	{
		get
		{
			m_phisicalDimension = GetPointSize(Width, Height, HorizontalResolution, VerticalResolution);
			return m_phisicalDimension;
		}
	}

	internal Tiff Tiff => tiff;

	internal SKBitmap SKBitmap => skBitmap;

	public PdfMask Mask
	{
		get
		{
			return m_mask;
		}
		set
		{
			if (value is PdfImageMask)
			{
				PdfImageMask pdfImageMask = value as PdfImageMask;
				if (pdfImageMask.Mask.Width != Width || pdfImageMask.Mask.Height != Height)
				{
					throw new ArgumentException("Soft mask must be the same size as drawing image.");
				}
			}
			m_mask = value;
		}
	}

	public PdfTiffImage(Stream stream)
	{
		imageStream = stream;
		StreamExistance(stream);
		stream.Position = 0L;
		tiff = GetTiffImage(stream);
		if (tiff == null)
		{
			MemoryStream memoryStream = new MemoryStream();
			CopyStream(stream, memoryStream);
			stream.Position = 0L;
			UpdateImageResolution(memoryStream);
			skBitmap = SKBitmap.Decode(memoryStream.ToArray());
			memoryStream.Dispose();
			memoryStream = null;
			stream.Position = 0L;
		}
	}

	public PdfTiffImage(Stream stream, bool enableMetadata)
	{
		base.EnableMetada = enableMetadata;
		imageStream = stream;
		StreamExistance(stream);
		stream.Position = 0L;
		tiff = GetTiffImage(stream);
		if (tiff == null)
		{
			MemoryStream memoryStream = new MemoryStream();
			CopyStream(stream, memoryStream);
			stream.Position = 0L;
			UpdateImageResolution(memoryStream);
			skBitmap = SKBitmap.Decode(memoryStream.ToArray());
			memoryStream.Dispose();
			memoryStream = null;
			stream.Position = 0L;
		}
	}

	~PdfTiffImage()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (tiff != null)
			{
				tiff.Dispose();
			}
			if (imageStream != null)
			{
				imageStream.Dispose();
			}
			if (skBitmap != null)
			{
				skBitmap.Dispose();
			}
		}
	}

	internal override void Save()
	{
		SKBitmap sKBitmap = null;
		PdfArray pdfArray = null;
		if (Tiff != null)
		{
			tiff.SetDirectory((short)ActiveFrame);
			bool success = false;
			if (!tiff.IsTiled() && Mask == null)
			{
				ConvertTifftoPdfStream(tiff, out success);
			}
			if (success)
			{
				if (base.Metadata != null)
				{
					AddMetadata();
				}
				return;
			}
			sKBitmap = ConvertTifftoPng(tiff);
		}
		else if (SKBitmap != null)
		{
			sKBitmap = SKBitmap;
		}
		if (sKBitmap != null)
		{
			if (Mask != null && Mask is PdfColorMask)
			{
				pdfArray = CreateColorMask();
			}
			else if (Mask != null && Mask is PdfImageMask)
			{
				sKBitmap = CreateMaskImage(sKBitmap);
			}
			PdfStream pdfImageObject = GetPdfImageObject(sKBitmap);
			if (pdfArray != null)
			{
				pdfImageObject["Mask"] = pdfArray;
			}
			base.ImageStream = pdfImageObject;
			if (base.Metadata != null)
			{
				AddMetadata();
			}
		}
	}

	private SKBitmap CreateMaskImage(SKBitmap kBitmap)
	{
		SKBitmap sKBitmap = (m_mask as PdfImageMask).Mask.SKBitmap;
		int num = Math.Abs(kBitmap.RowBytes) * kBitmap.Height;
		byte[] array = new byte[num];
		array = kBitmap.Bytes;
		num = Math.Abs(sKBitmap.RowBytes) * sKBitmap.Height;
		byte[] array2 = new byte[num];
		array2 = ConvertPixelsToIntArray(sKBitmap.Pixels);
		num = Math.Abs(kBitmap.RowBytes) * kBitmap.Height;
		for (int i = 0; i < num; i += 4)
		{
			array[i + 3] = array2[i + 2];
		}
		int[] array3 = new int[array.Length];
		int num2 = 0;
		for (int j = 0; j < array3.Length; j += 4)
		{
			array3[num2] = (array[j + 3] << 24) | (array[j + 2] << 16) | (array[j + 1] << 8) | array[j];
			num2++;
		}
		SKBitmap sKBitmap2 = new SKBitmap(kBitmap.Width, kBitmap.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
		byte[] array4 = new byte[sKBitmap2.RowBytes * sKBitmap2.Height];
		for (int k = 0; k < array3.Length / 4 - 1; k++)
		{
			Color color = Color.FromArgb(array3[k]);
			int num3 = k % kBitmap.Width;
			int num4 = k / kBitmap.Width;
			if (kBitmap.ColorType == SKColorType.Rgba8888)
			{
				array4[sKBitmap2.RowBytes * num4 + 4 * num3] = color.R;
				array4[sKBitmap2.RowBytes * num4 + 4 * num3 + 1] = color.G;
				array4[sKBitmap2.RowBytes * num4 + 4 * num3 + 2] = color.B;
				array4[sKBitmap2.RowBytes * num4 + 4 * num3 + 3] = color.A;
			}
			else
			{
				array4[sKBitmap2.RowBytes * num4 + 4 * num3] = color.B;
				array4[sKBitmap2.RowBytes * num4 + 4 * num3 + 1] = color.G;
				array4[sKBitmap2.RowBytes * num4 + 4 * num3 + 2] = color.R;
				array4[sKBitmap2.RowBytes * num4 + 4 * num3 + 3] = color.A;
			}
		}
		Marshal.Copy(array4, 0, sKBitmap2.GetPixels(), array4.Length);
		return sKBitmap2;
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

	private PdfArray CreateColorMask()
	{
		PdfArray pdfArray = new PdfArray();
		PdfColorMask obj = m_mask as PdfColorMask;
		PdfColor startColor = obj.StartColor;
		PdfColor endColor = obj.EndColor;
		pdfArray.Add(new PdfNumber(startColor.R));
		pdfArray.Add(new PdfNumber(startColor.G));
		pdfArray.Add(new PdfNumber(startColor.B));
		pdfArray.Add(new PdfNumber(endColor.R));
		pdfArray.Add(new PdfNumber(endColor.G));
		pdfArray.Add(new PdfNumber(endColor.B));
		return pdfArray;
	}

	private Tiff GetTiffImage(Stream stream)
	{
		Tiff result = null;
		try
		{
			if (stream.IsTiff())
			{
				stream.Position = 0L;
				result = Tiff.ClientOpen("in-memory", "r", stream, new TiffStream());
			}
		}
		catch
		{
		}
		return result;
	}

	private SKBitmap ConvertTifftoPng(Tiff tifImg)
	{
		if (tifImg != null)
		{
			int num = tifImg.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH)[0].ToInt();
			int num2 = tifImg.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH)[0].ToInt();
			SKBitmap sKBitmap = new SKBitmap();
			SKImageInfo info = new SKImageInfo(num, num2);
			int[] array = new int[num * num2];
			GCHandle ptr = GCHandle.Alloc(array, GCHandleType.Pinned);
			sKBitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes, null, delegate
			{
				ptr.Free();
			}, null);
			if (!tifImg.ReadRGBAImageOriented(num, num2, array, Orientation.TOPLEFT))
			{
				return null;
			}
			if (SKImageInfo.PlatformColorType == SKColorType.Bgra8888)
			{
				SKSwizzle.SwapRedBlue(ptr.AddrOfPinnedObject(), array.Length);
			}
			return sKBitmap;
		}
		return null;
	}

	internal override XmpMetadata GetMetadata()
	{
		ImageMetadataParser imageMetadataParser = null;
		imageStream.Position = 0L;
		if (imageStream.IsTiff())
		{
			imageMetadataParser = new ImageMetadataParser(imageStream, "Tiff");
		}
		else if (imageStream.IsPng())
		{
			imageMetadataParser = new ImageMetadataParser(imageStream, "Png");
		}
		else if (imageStream.IsJpeg())
		{
			imageMetadataParser = new ImageMetadataParser(imageStream, "Jpeg");
		}
		else if (imageStream.IsGif())
		{
			imageMetadataParser = new ImageMetadataParser(imageStream, "Gif");
		}
		return imageMetadataParser?.TryGetMetadata();
	}

	internal override PdfImage Clone()
	{
		return MemberwiseClone() as PdfTiffImage;
	}

	private new SizeF GetPointSize(float width, float height, float horizontalResolution, float verticalResolution)
	{
		PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor(horizontalResolution);
		PdfUnitConvertor pdfUnitConvertor2 = new PdfUnitConvertor(verticalResolution);
		float width2 = pdfUnitConvertor.ConvertUnits(width, PdfGraphicsUnit.Pixel, PdfGraphicsUnit.Point);
		float height2 = pdfUnitConvertor2.ConvertUnits(height, PdfGraphicsUnit.Pixel, PdfGraphicsUnit.Point);
		return new SizeF(width2, height2);
	}

	private Stream StreamExistance(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("The stream can't be null");
		}
		if (stream.Length <= 0)
		{
			throw new ArgumentException("The stream can't be empty", "stream");
		}
		return stream;
	}

	private void UpdateImageResolution(Stream stream)
	{
		if (stream.IsPng())
		{
			stream.Position = 0L;
			float[] imageResolution = PngDecoder.GetImageResolution(stream);
			m_dpiX = imageResolution[0];
			m_dpiY = imageResolution[1];
		}
		else if (stream.IsBmp())
		{
			stream.Position = 0L;
			float[] imageResolution = BmpDecoder.GetImageResolution(stream);
			m_dpiX = imageResolution[0];
			m_dpiY = imageResolution[1];
		}
		else if (stream.IsJpeg())
		{
			stream.Position = 0L;
			float[] imageResolution = JpegDecoder.GetImageResolution(stream);
			m_dpiX = imageResolution[0];
			m_dpiY = imageResolution[1];
		}
	}

	private void CopyStream(Stream source, Stream destination)
	{
		source.Position = 0L;
		destination.Position = 0L;
		byte[] array = new byte[8190];
		int count;
		while ((count = source.Read(array, 0, array.Length)) != 0)
		{
			destination.Write(array, 0, count);
		}
	}

	private PdfStream GetPdfImageObject(SKBitmap original)
	{
		PdfStream pdfStream = new PdfStream();
		try
		{
			if (original == null)
			{
				return null;
			}
			imageStream.Position = 0L;
			PngDecoder pngDecoder = null;
			if (imageStream.IsPng())
			{
				pngDecoder = new PngDecoder(imageStream);
				pngDecoder?.DecodeImageData();
			}
			if (!imageStream.IsPng() || original.AlphaType == SKAlphaType.Opaque || (pngDecoder != null && pngDecoder.BitsPerComponent != 8) || pngDecoder.ImageData == null)
			{
				if (!imageStream.IsJpeg() || original.AlphaType == SKAlphaType.Opaque)
				{
					MemoryStream memoryStream = new MemoryStream();
					original.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(memoryStream);
					pdfStream.Data = memoryStream.ToArray();
					memoryStream.Dispose();
					memoryStream = null;
				}
				else
				{
					JpegDecoder jpegDecoder = new JpegDecoder(imageStream);
					if (jpegDecoder.ImageData != null)
					{
						pdfStream.Data = jpegDecoder.ImageData;
					}
					else
					{
						MemoryStream memoryStream2 = new MemoryStream();
						original.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(memoryStream2);
						pdfStream.Data = memoryStream2.ToArray();
						memoryStream2.Dispose();
						memoryStream2 = null;
					}
					jpegDecoder = null;
				}
				pdfStream["Decode"] = new PdfArray(new float[6] { 0f, 1f, 0f, 1f, 0f, 1f });
				PdfArray pdfArray = new PdfArray();
				pdfArray.Add(new PdfName("DCTDecode"));
				pdfStream["Filter"] = pdfArray;
			}
			else
			{
				if (pngDecoder.ImageData != null)
				{
					pdfStream.Data = pngDecoder.ImageData;
				}
				pngDecoder = null;
			}
			pdfStream["Type"] = new PdfName("XObject");
			pdfStream["Subtype"] = new PdfName("Image");
			pdfStream["Width"] = new PdfNumber(original.Width);
			pdfStream["Height"] = new PdfNumber(original.Height);
			if (original.Info.BitsPerPixel > 8)
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(8);
			}
			else
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(original.Info.BitsPerPixel);
			}
			if (original.ColorType != SKColorType.Gray8)
			{
				pdfStream["ColorSpace"] = pdfStream.GetName("DeviceRGB");
			}
			else
			{
				pdfStream["ColorSpace"] = pdfStream.GetName("DeviceGray");
			}
			if (original.ColorType == SKColorType.Bgra1010102 || original.ColorType == SKColorType.Bgra8888 || original.ColorType == SKColorType.Rgba1010102 || original.ColorType == SKColorType.Rgba16161616 || original.ColorType == SKColorType.Rgba8888 || original.ColorType == SKColorType.RgbaF16 || original.ColorType == SKColorType.RgbaF16Clamped || original.ColorType == SKColorType.RgbaF32 || original.ColorType == SKColorType.Argb4444)
			{
				SetMask(pdfStream, original);
			}
		}
		catch
		{
			return null;
		}
		return pdfStream;
	}

	private byte[] ExtractAlpha(byte[] data)
	{
		byte[] array = new byte[data.Length / 4];
		int num = 0;
		for (int i = 0; i < data.Length; i += 4)
		{
			array[num++] = data[i + 3];
		}
		return array;
	}

	private void SetMask(PdfStream imageStream, SKBitmap mask)
	{
		if (mask != null)
		{
			PdfStream pdfStream = new PdfStream();
			pdfStream.InternalStream = new MemoryStream(ExtractAlpha(mask.Bytes));
			pdfStream["Type"] = new PdfName("XObject");
			pdfStream["Subtype"] = new PdfName("Image");
			pdfStream["Width"] = new PdfNumber(mask.Width);
			pdfStream["Height"] = new PdfNumber(mask.Height);
			if (mask.Info.BitsPerPixel > 8)
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(8);
			}
			else
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(mask.Info.BitsPerPixel);
			}
			pdfStream["ColorSpace"] = new PdfName("DeviceGray");
			imageStream.SetProperty("SMask", new PdfReferenceHolder(pdfStream));
		}
	}

	private void ConvertTifftoPdfStream(Tiff originalTiff, out bool success)
	{
		success = false;
		try
		{
			FieldValue[] field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH);
			if (field != null)
			{
				m_tiffWidth = field[0].ToInt();
				if (m_tiffWidth == 0)
				{
					return;
				}
			}
			field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH);
			if (field != null)
			{
				m_tiffHeight = field[0].ToInt();
				if (m_tiffHeight == 0)
				{
					return;
				}
			}
			field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.COMPRESSION);
			if (field == null || !(field[0].Value.ToString() != "OJPEG"))
			{
				return;
			}
			m_tiffCompressed = (BitMiracle.LibTiff.Classic.Compression)field[0].ToInt();
			if (!originalTiff.IsCodecConfigured(m_tiffCompressed))
			{
				return;
			}
			if (m_tiffCompressed == BitMiracle.LibTiff.Classic.Compression.JPEG)
			{
				field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.JPEGTABLES);
				if (field == null)
				{
					return;
				}
			}
			field = originalTiff.GetFieldDefaulted(BitMiracle.LibTiff.Classic.TiffTag.BITSPERSAMPLE);
			if (field != null)
			{
				m_tiffBitsPerSample = field[0].ToShort();
			}
			switch (m_tiffBitsPerSample)
			{
			default:
				return;
			case 0:
				m_tiffBitsPerSample = 1;
				break;
			case 3:
			case 5:
			case 6:
			case 7:
				return;
			case 1:
			case 2:
			case 4:
			case 8:
				break;
			}
			field = originalTiff.GetFieldDefaulted(BitMiracle.LibTiff.Classic.TiffTag.SAMPLESPERPIXEL);
			m_tiffSamplePerPixel = field[0].ToShort();
			if (m_tiffSamplePerPixel > 4)
			{
				return;
			}
			if (m_tiffSamplePerPixel == 0)
			{
				m_tiffSamplePerPixel = 1;
			}
			field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.SAMPLEFORMAT);
			if (field != null)
			{
				SampleFormat sampleFormat = (SampleFormat)field[0].ToByte();
				if ((uint)sampleFormat > 1u && sampleFormat != SampleFormat.VOID)
				{
					return;
				}
			}
			field = originalTiff.GetFieldDefaulted(BitMiracle.LibTiff.Classic.TiffTag.FILLORDER);
			if (field != null)
			{
				m_tiffFillOrder = (FillOrder)field[0].ToByte();
			}
			field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.PHOTOMETRIC);
			if (field == null)
			{
				return;
			}
			m_tiffPhotoMetric = (Photometric)field[0].ToInt();
			switch (m_tiffPhotoMetric)
			{
			case Photometric.MINISWHITE:
			case Photometric.MINISBLACK:
				if (m_tiffBitsPerSample == 1)
				{
					m_imageColorSpace = TiffColorSpace.BitLevel;
					if (m_tiffPhotoMetric == Photometric.MINISWHITE)
					{
						m_imageDecode = !m_imageDecode;
					}
				}
				else
				{
					m_imageColorSpace = TiffColorSpace.Gray;
					if (m_tiffPhotoMetric == Photometric.MINISWHITE)
					{
						m_imageDecode = !m_imageDecode;
					}
				}
				break;
			case Photometric.RGB:
			case Photometric.PALETTE:
			{
				bool flag2 = m_tiffPhotoMetric == Photometric.PALETTE;
				if (!flag2)
				{
					m_imageColorSpace = TiffColorSpace.Rgb;
					if (m_tiffSamplePerPixel == 3)
					{
						break;
					}
					field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.INDEXED);
					if (field != null && field[0].ToInt() == 1)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					if (m_tiffSamplePerPixel <= 3 || m_tiffSamplePerPixel != 4)
					{
						return;
					}
					m_imageColorSpace = TiffColorSpace.Rgb;
					field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.EXTRASAMPLES);
					if (field != null && field[0].ToInt() == 1)
					{
						byte[] array5 = field[1].ToByteArray();
						if (array5[0] == 1 || (m_tiffFillOrder == FillOrder.MSB2LSB && array5[0] == 2))
						{
							m_imageSample = TiffToImage.RgbaaToRgb;
							return;
						}
						if (array5[0] == 2)
						{
							m_imageSample = TiffToImage.RgbaToRgb;
						}
					}
					else
					{
						m_imageColorSpace = TiffColorSpace.Cmyk;
						m_imageDecode = !m_imageDecode;
					}
				}
				else if (flag2)
				{
					if (m_tiffSamplePerPixel != 1)
					{
						return;
					}
					m_imageColorSpace = TiffColorSpace.Rgb | TiffColorSpace.Palette;
					m_imagePaletteSize = 1 << (int)m_tiffBitsPerSample;
					field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.COLORMAP);
					if (field == null)
					{
						return;
					}
					short[] array = field[0].ToShortArray();
					short[] array2 = field[1].ToShortArray();
					short[] array3 = field[2].ToShortArray();
					m_imagePaletteData = new byte[m_imagePaletteSize * 3];
					for (int j = 0; j < m_imagePaletteSize; j++)
					{
						m_imagePaletteData[j * 3] = (byte)(array[j] >> 8);
						m_imagePaletteData[j * 3 + 1] = (byte)(array2[j] >> 8);
						m_imagePaletteData[j * 3 + 2] = (byte)(array3[j] >> 8);
					}
					m_imagePaletteSize *= 3;
				}
				break;
			}
			case Photometric.SEPARATED:
			{
				bool flag = false;
				field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.INDEXED);
				if (field != null && field[0].ToInt() == 1)
				{
					flag = true;
				}
				if (!flag)
				{
					field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.INKSET);
					if ((field == null || field[0].ToByte() == 1) && m_tiffSamplePerPixel == 4)
					{
						m_imageColorSpace = TiffColorSpace.Cmyk;
						break;
					}
					return;
				}
				if (m_tiffSamplePerPixel != 1)
				{
					return;
				}
				m_imageColorSpace = TiffColorSpace.Cmyk | TiffColorSpace.Palette;
				m_imagePaletteSize = 1 << (int)m_tiffBitsPerSample;
				field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.COLORMAP);
				if (field == null)
				{
					return;
				}
				short[] array = field[0].ToShortArray();
				short[] array2 = field[1].ToShortArray();
				short[] array3 = field[2].ToShortArray();
				short[] array4 = field[3].ToShortArray();
				m_imagePaletteData = new byte[m_imagePaletteSize * 4];
				for (int i = 0; i < m_imagePaletteSize; i++)
				{
					m_imagePaletteData[i * 4] = (byte)(array[i] >> 8);
					m_imagePaletteData[i * 4 + 1] = (byte)(array2[i] >> 8);
					m_imagePaletteData[i * 4 + 2] = (byte)(array3[i] >> 8);
					m_imagePaletteData[i * 4 + 3] = (byte)(array4[i] >> 8);
				}
				m_imagePaletteSize *= 4;
				break;
			}
			case Photometric.YCBCR:
				m_imageColorSpace = TiffColorSpace.Rgb;
				if (m_tiffSamplePerPixel == 1)
				{
					m_imageColorSpace = TiffColorSpace.Gray;
					m_tiffPhotoMetric = Photometric.MINISBLACK;
					break;
				}
				m_imageSample = TiffToImage.YcbcrToRgb;
				if (m_imageDefaultCompresser == TiffCompresser.CompressJpeg)
				{
					m_imageSample = TiffToImage.None;
				}
				break;
			case Photometric.CIELAB:
				m_imageLabRange[0] = -127;
				m_imageLabRange[1] = 127;
				m_imageLabRange[2] = -127;
				m_imageLabRange[3] = 127;
				m_imageSample = TiffToImage.SignToUnsigned;
				m_imageColorSpace = TiffColorSpace.Lab;
				break;
			case Photometric.ICCLAB:
				m_imageLabRange[0] = 0;
				m_imageLabRange[1] = 255;
				m_imageLabRange[2] = 0;
				m_imageLabRange[3] = 255;
				m_imageColorSpace = TiffColorSpace.Lab;
				break;
			case Photometric.ITULAB:
				m_imageLabRange[0] = -85;
				m_imageLabRange[1] = 85;
				m_imageLabRange[2] = -75;
				m_imageLabRange[3] = 124;
				m_imageSample = TiffToImage.SignToUnsigned;
				m_imageColorSpace = TiffColorSpace.Lab;
				break;
			default:
				return;
			}
			field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.PLANARCONFIG);
			if (field != null)
			{
				m_tiffPlanar = (PlanarConfig)field[0].ToShort();
				switch (m_tiffPlanar)
				{
				default:
					return;
				case PlanarConfig.UNKNOWN:
					m_tiffPlanar = PlanarConfig.CONTIG;
					break;
				case PlanarConfig.SEPARATE:
					m_imageSample = TiffToImage.PlannerToContig;
					if (m_tiffBitsPerSample != 8)
					{
						return;
					}
					break;
				case PlanarConfig.CONTIG:
					break;
				}
			}
			m_imageTranscode = TiffTranscode.Encode;
			if (!m_imageNoPass)
			{
				if (m_tiffCompressed == BitMiracle.LibTiff.Classic.Compression.CCITTFAX4 && (originalTiff.IsTiled() || originalTiff.NumberOfStrips() == 1))
				{
					m_imageTranscode = TiffTranscode.Raw;
					m_imageCompression = TiffCompresser.CompressG4;
				}
				if ((m_tiffCompressed == BitMiracle.LibTiff.Classic.Compression.ADOBE_DEFLATE || m_tiffCompressed == BitMiracle.LibTiff.Classic.Compression.DEFLATE) && (originalTiff.IsTiled() || originalTiff.NumberOfStrips() == 1))
				{
					m_imageTranscode = TiffTranscode.Raw;
					m_imageCompression = TiffCompresser.CompressZip;
				}
				if (m_tiffCompressed == BitMiracle.LibTiff.Classic.Compression.JPEG)
				{
					m_imageTranscode = TiffTranscode.Raw;
					m_imageCompression = TiffCompresser.CompressJpeg;
				}
			}
			if (m_imageTranscode != TiffTranscode.Raw)
			{
				m_imageCompression = m_imageDefaultCompresser;
			}
			if (m_imageDefaultCompresser == TiffCompresser.CompressJpeg && (m_imageColorSpace & TiffColorSpace.Palette) != 0)
			{
				m_imageSample |= TiffToImage.Palette;
				m_imageColorSpace ^= TiffColorSpace.Palette;
			}
			if (m_tiffCompressed == BitMiracle.LibTiff.Classic.Compression.JPEG && m_tiffPlanar == PlanarConfig.SEPARATE)
			{
				return;
			}
			if ((m_imageSample & TiffToImage.Palette) != 0)
			{
				if ((m_imageColorSpace & TiffColorSpace.Cmyk) != 0)
				{
					m_tiffSamplePerPixel = 4;
					m_tiffPhotoMetric = Photometric.SEPARATED;
				}
				else
				{
					m_tiffSamplePerPixel = 3;
					m_tiffPhotoMetric = Photometric.RGB;
				}
			}
			field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.WHITEPOINT);
			if (field != null)
			{
				float[] array6 = field[0].ToFloatArray();
				m_tiffWhiteChrom[0] = array6[0];
				m_tiffWhiteChrom[1] = array6[1];
				if ((m_imageColorSpace & TiffColorSpace.Gray) != 0)
				{
					m_imageColorSpace |= TiffColorSpace.CalGray;
				}
				if ((m_imageColorSpace & TiffColorSpace.Rgb) != 0)
				{
					m_imageColorSpace |= TiffColorSpace.CalRgb;
				}
			}
			field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.PRIMARYCHROMATICITIES);
			if (field != null)
			{
				float[] array7 = field[0].ToFloatArray();
				m_tiffPrimaryChrom[0] = array7[0];
				m_tiffPrimaryChrom[1] = array7[1];
				m_tiffPrimaryChrom[2] = array7[2];
				m_tiffPrimaryChrom[3] = array7[3];
				m_tiffPrimaryChrom[4] = array7[4];
				m_tiffPrimaryChrom[5] = array7[5];
				if ((m_imageColorSpace & TiffColorSpace.Rgb) != 0)
				{
					m_imageColorSpace |= TiffColorSpace.CalRgb;
				}
			}
			if ((m_imageColorSpace & TiffColorSpace.Lab) != 0)
			{
				field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.WHITEPOINT);
				if (field != null)
				{
					float[] array8 = field[0].ToFloatArray();
					m_tiffWhiteChrom[0] = array8[0];
					m_tiffWhiteChrom[1] = array8[1];
				}
				else
				{
					m_tiffWhiteChrom[0] = 0.3457f;
					m_tiffWhiteChrom[1] = 0.3585f;
				}
			}
			field = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.ICCPROFILE);
			if (field != null)
			{
				m_tiffIccLen = field[0].ToInt();
				m_tiffIccData = field[1].ToByteArray();
				m_imageColorSpace |= TiffColorSpace.IccBased;
			}
			else
			{
				m_tiffIccLen = 0;
				m_tiffIccData = null;
			}
			if (m_tiffBitsPerSample == 1 && m_tiffSamplePerPixel == 1)
			{
				m_imageCompression = TiffCompresser.CompressG4;
			}
			GetTiffImageSize(originalTiff);
			bool encode = false;
			byte[] array9 = GetImagePdfData(originalTiff, out encode);
			if (m_imageCompression == TiffCompresser.CompressG4 && encode)
			{
				array9 = new PdfCcittEncoder().EncodeData(array9, m_tiffWidth, m_tiffHeight);
			}
			if (array9 == null)
			{
				return;
			}
			PdfStream pdfStream = new PdfStream();
			pdfStream["Type"] = new PdfName("XObject");
			pdfStream["Subtype"] = new PdfName("Image");
			pdfStream["Width"] = new PdfNumber(m_tiffWidth);
			pdfStream["Height"] = new PdfNumber(m_tiffHeight);
			pdfStream["BitsPerComponent"] = new PdfNumber(m_tiffBitsPerSample);
			GetImageColorSpace(pdfStream);
			pdfStream.Data = array9;
			if ((m_imageColorSpace & TiffColorSpace.IccBased) == 0 && m_imageDecode && (m_imageColorSpace != TiffColorSpace.BitLevel || m_imageCompression != TiffCompresser.CompressG4))
			{
				List<float> list = new List<float>();
				for (int k = 0; k < m_tiffSamplePerPixel; k++)
				{
					list.Add(1f);
					list.Add(0f);
				}
				pdfStream["Decode"] = new PdfArray(list.ToArray());
			}
			UpdateImageFilter(pdfStream);
			base.ImageStream = pdfStream;
			success = true;
		}
		catch
		{
			success = false;
		}
		finally
		{
			m_imageCompression = TiffCompresser.None;
			m_tiffCompressed = BitMiracle.LibTiff.Classic.Compression.NONE;
			m_tiffBitsPerSample = 0;
			m_tiffSamplePerPixel = 0;
			m_tiffPhotoMetric = Photometric.MINISWHITE;
			m_tiffFillOrder = FillOrder.MSB2LSB;
			m_imageColorSpace = TiffColorSpace.Unknown;
			m_imageDecode = false;
			m_imagePaletteSize = 0;
			m_imagePaletteData = null;
			m_imageLabRange = new int[4];
			m_tiffPlanar = PlanarConfig.UNKNOWN;
			m_imageTranscode = TiffTranscode.Unknown;
			m_imageSample = TiffToImage.None;
			m_tiffWhiteChrom = new float[2];
			m_tiffPrimaryChrom = new float[6];
			m_tiffIccLen = 0;
			m_tiffIccData = null;
			m_imageDefaultCompresser = TiffCompresser.None;
			m_imageNoPass = false;
			m_tiffDataSize = 0;
		}
	}

	private void UpdateImageFilter(PdfDictionary imageDictionary)
	{
		if (m_imageCompression == TiffCompresser.None)
		{
			return;
		}
		switch (m_imageCompression)
		{
		case TiffCompresser.CompressG4:
		{
			imageDictionary["Filter"] = new PdfName("CCITTFaxDecode");
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			pdfDictionary2["K"] = new PdfNumber(-1);
			pdfDictionary2["Columns"] = new PdfNumber(m_tiffWidth);
			pdfDictionary2["Rows"] = new PdfNumber(m_tiffHeight);
			imageDictionary["DecodeParms"] = pdfDictionary2;
			if (!m_imageDecode)
			{
				pdfDictionary2["BlackIs1"] = new PdfBoolean(value: true);
			}
			(imageDictionary as PdfStream).Compress = false;
			break;
		}
		case TiffCompresser.CompressJpeg:
			imageDictionary["Filter"] = new PdfName("DCTDecode");
			if (m_tiffPhotoMetric != Photometric.YCBCR)
			{
				PdfDictionary pdfDictionary = new PdfDictionary();
				pdfDictionary["ColorTransform"] = new PdfNumber(0);
				imageDictionary["DecodeParms"] = pdfDictionary;
			}
			(imageDictionary as PdfStream).Compress = false;
			break;
		}
	}

	private void GetImageColorSpace(PdfDictionary imageDictionary)
	{
		if ((m_imageColorSpace & TiffColorSpace.IccBased) != 0)
		{
			PdfStream pdfStream = new PdfStream();
			pdfStream["N"] = new PdfNumber(m_tiffSamplePerPixel);
			m_imageColorSpace ^= TiffColorSpace.IccBased;
			pdfStream["Alternative"] = GetDeviceColor(m_imageColorSpace);
			m_imageColorSpace |= TiffColorSpace.IccBased;
			pdfStream.Data = m_tiffIccData;
			PdfArray pdfArray = new PdfArray();
			pdfArray.Add(new PdfName("ICCBased"));
			pdfArray.Add(new PdfReferenceHolder(pdfStream));
			imageDictionary["ColorSpace"] = pdfArray;
			return;
		}
		if ((m_imageColorSpace & TiffColorSpace.Palette) != 0)
		{
			PdfArray pdfArray2 = new PdfArray();
			pdfArray2.Add(new PdfName("Indexed"));
			m_imageColorSpace ^= TiffColorSpace.Palette;
			PdfName deviceColor = GetDeviceColor(m_imageColorSpace);
			pdfArray2.Add(deviceColor);
			m_imageColorSpace |= TiffColorSpace.Palette;
			pdfArray2.Add(new PdfNumber((1 << (int)m_tiffBitsPerSample) - 1));
			PdfStream pdfStream2 = new PdfStream();
			pdfStream2.Data = m_imagePaletteData;
			pdfStream2.Compress = false;
			pdfArray2.Add(new PdfReferenceHolder(pdfStream2));
			imageDictionary["ColorSpace"] = pdfArray2;
			return;
		}
		if ((m_imageColorSpace & TiffColorSpace.Lab) != 0)
		{
			PdfArray pdfArray3 = new PdfArray();
			pdfArray3.Add(new PdfName("Lab"));
			PdfDictionary pdfDictionary = new PdfDictionary();
			float num = m_tiffWhiteChrom[0];
			float num2 = m_tiffWhiteChrom[1];
			float num3 = 1f - (num + num2);
			num /= num2;
			num3 /= num2;
			num2 = 1f;
			PdfArray pdfArray4 = new PdfArray();
			pdfArray4.Add(new PdfNumber(num));
			pdfArray4.Add(new PdfNumber(num2));
			pdfArray4.Add(new PdfNumber(num3));
			pdfDictionary["WhitePoint"] = pdfArray4;
			PdfArray pdfArray5 = new PdfArray();
			num = 0.3457f;
			num2 = 0.3585f;
			num3 = 1f - (num + num2);
			num /= num2;
			num3 /= num2;
			num2 = 1f;
			pdfArray5.Add(new PdfNumber(num));
			pdfArray5.Add(new PdfNumber(num2));
			pdfArray5.Add(new PdfNumber(num3));
			pdfDictionary["BlackPoint"] = pdfArray5;
			PdfArray pdfArray6 = new PdfArray();
			pdfArray6.Add(new PdfNumber(m_imageLabRange[0]));
			pdfArray6.Add(new PdfNumber(m_imageLabRange[1]));
			pdfArray6.Add(new PdfNumber(m_imageLabRange[2]));
			pdfArray6.Add(new PdfNumber(m_imageLabRange[3]));
			pdfDictionary["Range"] = pdfArray6;
			pdfArray3.Add(pdfDictionary);
			imageDictionary["ColorSpace"] = pdfArray3;
		}
		if ((m_imageColorSpace & TiffColorSpace.BitLevel) != 0 || (m_imageColorSpace & TiffColorSpace.Gray) != 0)
		{
			imageDictionary["ColorSpace"] = new PdfName("DeviceGray");
		}
		if ((m_imageColorSpace & TiffColorSpace.Cmyk) != 0)
		{
			imageDictionary["ColorSpace"] = new PdfName("DeviceCMYK");
		}
		if ((m_imageColorSpace & TiffColorSpace.Rgb) != 0)
		{
			imageDictionary["ColorSpace"] = new PdfName("DeviceRGB");
		}
	}

	private PdfName GetDeviceColor(TiffColorSpace m_imageColorSpace)
	{
		if ((m_imageColorSpace & TiffColorSpace.Cmyk) != 0)
		{
			return new PdfName("DeviceCMYK");
		}
		if ((m_imageColorSpace & TiffColorSpace.Rgb) != 0)
		{
			return new PdfName("DeviceRGB");
		}
		if ((m_imageColorSpace & TiffColorSpace.BitLevel) != 0 || (m_imageColorSpace & TiffColorSpace.Gray) != 0)
		{
			return new PdfName("DeviceGray");
		}
		if ((m_imageColorSpace & TiffColorSpace.Cmyk) != 0)
		{
			return new PdfName("DeviceCMYK");
		}
		return null;
	}

	private void GetTiffImageSize(Tiff originalTiff)
	{
		if (m_imageTranscode == TiffTranscode.Raw)
		{
			FieldValue[] array = null;
			if (m_imageCompression == TiffCompresser.CompressG4)
			{
				array = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.STRIPBYTECOUNTS);
				int[] array2 = array[0].ToIntArray();
				m_tiffDataSize = array2[0];
				return;
			}
			if (m_imageCompression == TiffCompresser.CompressZip)
			{
				array = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.STRIPBYTECOUNTS);
				int[] array3 = array[0].ToIntArray();
				m_tiffDataSize = array3[0];
				return;
			}
			if (m_tiffCompressed == BitMiracle.LibTiff.Classic.Compression.JPEG)
			{
				array = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.JPEGTABLES);
				if (array != null)
				{
					int num = array[0].ToInt();
					if (num > 4)
					{
						m_tiffDataSize += num;
						m_tiffDataSize -= 2;
					}
				}
				else
				{
					m_tiffDataSize = 2;
				}
				int num2 = originalTiff.NumberOfStrips();
				int[] array4 = null;
				array = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.STRIPBYTECOUNTS);
				if (array != null)
				{
					array4 = array[0].ToIntArray();
					for (int i = 0; i < num2; i++)
					{
						m_tiffDataSize += array4[i];
						m_tiffDataSize -= 4;
					}
					m_tiffDataSize += 2;
				}
				return;
			}
		}
		m_tiffDataSize = originalTiff.ScanlineSize() * m_tiffHeight;
		if (m_tiffPlanar == PlanarConfig.SEPARATE)
		{
			m_tiffDataSize *= m_tiffSamplePerPixel;
		}
	}

	private byte[] GetImagePdfData(Tiff originalTiff, out bool encode)
	{
		byte[] array = null;
		int imageOffset = 0;
		int num = 0;
		int num2 = 0;
		FieldValue[] array2 = null;
		encode = true;
		if (m_imageTranscode == TiffTranscode.Raw)
		{
			if (m_imageCompression == TiffCompresser.CompressG4)
			{
				array = new byte[m_tiffDataSize];
				originalTiff.ReadRawStrip(0, array, 0, m_tiffDataSize);
				if (m_tiffFillOrder == FillOrder.LSB2MSB)
				{
					Tiff.ReverseBits(array, m_tiffDataSize);
				}
				encode = false;
				return array;
			}
			if (m_imageCompression == TiffCompresser.CompressZip)
			{
				array = new byte[m_tiffDataSize];
				originalTiff.ReadRawStrip(0, array, 0, m_tiffDataSize);
				if (m_tiffFillOrder == FillOrder.LSB2MSB)
				{
					Tiff.ReverseBits(array, m_tiffDataSize);
				}
				encode = false;
				return array;
			}
			if (m_tiffCompressed == BitMiracle.LibTiff.Classic.Compression.JPEG)
			{
				array = new byte[m_tiffDataSize];
				array2 = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.JPEGTABLES);
				if (array2 != null)
				{
					int num3 = array2[0].ToInt();
					byte[] src = array2[1].ToByteArray();
					if (num3 > 4)
					{
						Buffer.BlockCopy(src, 0, array, 0, num3);
						imageOffset += num3 - 2;
					}
				}
				num = originalTiff.NumberOfStrips();
				array2 = originalTiff.GetField(BitMiracle.LibTiff.Classic.TiffTag.STRIPBYTECOUNTS);
				int[] array3 = array2[0].ToIntArray();
				for (int i = 0; i < num; i++)
				{
					if (array3[i] > num2)
					{
						num2 = array3[i];
					}
				}
				byte[] array4 = new byte[num2];
				for (int j = 0; j < num; j++)
				{
					int striplength = originalTiff.ReadRawStrip(j, array4, 0, -1);
					if (!IsJpegStripData(array4, striplength, array, ref imageOffset, num, j, m_tiffHeight))
					{
						return null;
					}
				}
				array[imageOffset++] = byte.MaxValue;
				array[imageOffset++] = 217;
				encode = false;
				return array;
			}
		}
		int num4 = 0;
		if (m_imageSample == TiffToImage.None)
		{
			array = new byte[m_tiffDataSize];
			num4 = originalTiff.StripSize();
			num = originalTiff.NumberOfStrips();
			for (int k = 0; k < num; k++)
			{
				int num5 = originalTiff.ReadEncodedStrip(k, array, imageOffset, num4);
				imageOffset = ((num5 != -1) ? (imageOffset + num5) : (imageOffset + num4));
			}
		}
		else
		{
			byte[] array5 = null;
			bool flag = false;
			if ((m_imageSample & TiffToImage.PlannerToContig) != 0)
			{
				int num6 = originalTiff.StripSize();
				int num7 = originalTiff.NumberOfStrips();
				num4 = num6 * m_tiffSamplePerPixel;
				num = num7 / m_tiffSamplePerPixel;
				array = new byte[m_tiffDataSize];
				array5 = new byte[num4];
				for (int l = 0; l < num; l++)
				{
					int num8 = 0;
					for (int m = 0; m < m_tiffSamplePerPixel; m++)
					{
						int num9 = originalTiff.ReadEncodedStrip(l + m * num, array5, num8, num6);
						num8 = ((num9 != -1) ? (num8 + num9) : (num8 + num6));
					}
					ConvertSamplePlanerSeparatorToContig(array, imageOffset, array5, num8);
					imageOffset += num8;
				}
				flag = true;
			}
			if (!flag)
			{
				array = new byte[m_tiffDataSize];
				num4 = originalTiff.StripSize();
				num = originalTiff.NumberOfStrips();
				for (int n = 0; n < num; n++)
				{
					int num10 = originalTiff.ReadEncodedStrip(n, array, imageOffset, num4);
					imageOffset = ((num10 != -1) ? (imageOffset + num10) : (imageOffset + num4));
				}
				if ((m_imageSample & TiffToImage.Palette) != 0)
				{
					array5 = Tiff.Realloc(array, m_tiffDataSize * m_tiffSamplePerPixel);
					array = array5;
					m_tiffDataSize *= m_tiffSamplePerPixel;
					ConvertToPalette(array);
				}
				if ((m_imageSample & TiffToImage.RgbaToRgb) != 0)
				{
					m_tiffDataSize = ConvertRgbaToRgb(array, m_tiffWidth * m_tiffHeight);
				}
				if ((m_imageSample & TiffToImage.RgbaaToRgb) != 0)
				{
					m_tiffDataSize = ConvertRgbaaToRgb(array, m_tiffWidth * m_tiffHeight);
				}
				if ((m_imageSample & TiffToImage.YcbcrToRgb) != 0)
				{
					array5 = Tiff.Realloc(array, m_tiffWidth * m_tiffHeight * 4);
					array = array5;
					int[] array6 = Tiff.ByteArrayToInts(array, 0, m_tiffWidth * m_tiffHeight * 4);
					if (!originalTiff.ReadRGBAImageOriented(m_tiffWidth, m_tiffHeight, array6, Orientation.TOPLEFT, stopOnError: false))
					{
						return null;
					}
					Tiff.IntsToByteArray(array6, 0, m_tiffWidth * m_tiffHeight, array, 0);
					m_tiffDataSize = ConvertAbgrToRgb(array, m_tiffWidth * m_tiffHeight);
				}
				if ((m_imageSample & TiffToImage.SignToUnsigned) != 0)
				{
					m_tiffDataSize = ConvertLabSignedToUnsigned(array, m_tiffWidth * m_tiffHeight);
				}
			}
		}
		return array;
	}

	private void ConvertSamplePlanerSeparatorToContig(byte[] imgData, int bufferOffset, byte[] samplebuffer, int samplebuffersize)
	{
		int num = samplebuffersize / m_tiffSamplePerPixel;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < m_tiffSamplePerPixel; j++)
			{
				imgData[bufferOffset + i * m_tiffSamplePerPixel + j] = samplebuffer[i + j * num];
			}
		}
	}

	private void ConvertToPalette(byte[] imgData)
	{
		int num = m_tiffWidth * m_tiffHeight;
		short tiffSamplePerPixel = m_tiffSamplePerPixel;
		for (int num2 = num; num2 > 0; num2--)
		{
			int num3 = imgData[num2 - 1] * tiffSamplePerPixel;
			int num4 = (num2 - 1) * tiffSamplePerPixel;
			for (int i = 0; i < tiffSamplePerPixel; i++)
			{
				imgData[num4 + i] = m_imagePaletteData[num3 + i];
			}
		}
	}

	private bool IsJpegStripData(byte[] strip, int striplength, byte[] imgData, ref int imageOffset, int stripCount, int no, int height)
	{
		int num = 1;
		while (num < striplength)
		{
			switch (strip[num])
			{
			case 216:
				num += 2;
				break;
			case 192:
			case 193:
			case 195:
			case 201:
			case 202:
				if (no == 0)
				{
					Buffer.BlockCopy(strip, num - 1, imgData, imageOffset, strip[num + 2] + 2);
					short num2 = 1;
					short num3 = 1;
					for (int i = 0; i < imgData[imageOffset + 9]; i++)
					{
						if (imgData[imageOffset + 11 + 2 * i] >> 4 > num3)
						{
							num3 = (short)(imgData[imageOffset + 11 + 2 * i] >> 4);
						}
						if ((imgData[imageOffset + 11 + 2 * i] & 0xF) > num2)
						{
							num2 = (short)(imgData[imageOffset + 11 + 2 * i] & 0xF);
						}
					}
					num2 *= 8;
					num3 *= 8;
					short num4 = (short)((((imgData[imageOffset + 5] << 8) | imgData[imageOffset + 6]) + num2 - 1) / num2);
					num4 *= (short)((((imgData[imageOffset + 7] << 8) | imgData[imageOffset + 8]) + num3 - 1) / num3);
					imgData[imageOffset + 5] = (byte)((uint)(height >> 8) & 0xFFu);
					imgData[imageOffset + 6] = (byte)((uint)height & 0xFFu);
					imageOffset += strip[num + 2] + 2;
					num += strip[num + 2] + 2;
					if (stripCount > 1)
					{
						imgData[imageOffset++] = byte.MaxValue;
						imgData[imageOffset++] = 221;
						imgData[imageOffset++] = 0;
						imgData[imageOffset++] = 4;
						imgData[imageOffset++] = (byte)((uint)(num4 >> 8) & 0xFFu);
						imgData[imageOffset++] = (byte)((uint)num4 & 0xFFu);
					}
				}
				else
				{
					num += strip[num + 2] + 2;
				}
				break;
			case 196:
			case 219:
				if (no == 0)
				{
					Buffer.BlockCopy(strip, num - 1, imgData, imageOffset, strip[num + 2] + 2);
					imageOffset += strip[num + 2] + 2;
				}
				num += strip[num + 2] + 2;
				break;
			case 218:
				if (no == 0)
				{
					Buffer.BlockCopy(strip, num - 1, imgData, imageOffset, strip[num + 2] + 2);
					imageOffset += strip[num + 2] + 2;
					num += strip[num + 2] + 2;
				}
				else
				{
					imgData[imageOffset++] = byte.MaxValue;
					imgData[imageOffset++] = (byte)(0xD0u | (uint)((no - 1) % 8));
					num += strip[num + 2] + 2;
				}
				Buffer.BlockCopy(strip, num - 1, imgData, imageOffset, striplength - num - 1);
				imageOffset += striplength - num - 1;
				return true;
			default:
				num += strip[num + 2] + 2;
				break;
			}
		}
		return false;
	}

	private int ConvertRgbaToRgb(byte[] data, int samplecount)
	{
		int[] array = Tiff.ByteArrayToInts(data, 0, samplecount * 4);
		int i;
		for (i = 0; i < samplecount; i++)
		{
			int num = array[i];
			byte b = (byte)(255 - ((num >> 24) & 0xFF));
			data[i * 3] = (byte)(((num >> 16) & 0xFF) + b);
			data[i * 3 + 1] = (byte)(((num >> 8) & 0xFF) + b);
			data[i * 3 + 2] = (byte)((num & 0xFF) + b);
		}
		return i * 3;
	}

	private int ConvertRgbaaToRgb(byte[] data, int samplecount)
	{
		int i;
		for (i = 0; i < samplecount; i++)
		{
			Buffer.BlockCopy(data, i * 4, data, i * 3, 3);
		}
		return i * 3;
	}

	private int ConvertAbgrToRgb(byte[] data, int samplecount)
	{
		int[] array = Tiff.ByteArrayToInts(data, 0, samplecount * 4);
		int i;
		for (i = 0; i < samplecount; i++)
		{
			int num = array[i];
			data[i * 3] = (byte)((uint)num & 0xFFu);
			data[i * 3 + 1] = (byte)((uint)(num >> 8) & 0xFFu);
			data[i * 3 + 2] = (byte)((uint)(num >> 16) & 0xFFu);
		}
		return i * 3;
	}

	private int ConvertLabSignedToUnsigned(byte[] imgData, int samplecount)
	{
		for (int i = 0; i < samplecount; i++)
		{
			if ((imgData[i * 3 + 1] & 0x80u) != 0)
			{
				imgData[i * 3 + 1] = (byte)(128 + (sbyte)imgData[i * 3 + 1]);
			}
			else
			{
				imgData[i * 3 + 1] |= 128;
			}
			if ((imgData[i * 3 + 2] & 0x80u) != 0)
			{
				imgData[i * 3 + 2] = (byte)(128 + (sbyte)imgData[i * 3 + 2]);
			}
			else
			{
				imgData[i * 3 + 2] |= 128;
			}
		}
		return samplecount * 3;
	}
}
