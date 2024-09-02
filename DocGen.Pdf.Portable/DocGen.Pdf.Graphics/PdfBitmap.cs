using System;
using System.IO;
using DocGen.Pdf.Graphics.Images.Decoder;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf.Graphics;

public class PdfBitmap : PdfImage, IDisposable
{
	private bool m_bDisposed;

	private bool imageStatus = true;

	internal ImageDecoder Decoder { get; set; }

	internal PdfMask Mask { get; set; }

	internal PdfColorSpace ColorSpace { get; set; }

	internal bool ImageMask { get; set; }

	internal int CheckImageType { get; set; }

	internal int BitsPerComponent { get; set; }

	public PdfBitmap(Stream stream)
	{
		Initialize(stream);
	}

	public PdfBitmap(Stream stream, bool enableMetadata)
	{
		base.EnableMetada = enableMetadata;
		Initialize(stream);
	}

	~PdfBitmap()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal void Close()
	{
		if (Decoder == null)
		{
			return;
		}
		if (Decoder is PngDecoder pngDecoder)
		{
			pngDecoder.Dispose();
		}
		else
		{
			if (Decoder.ImageData != null)
			{
				Decoder.ImageData = null;
			}
			if (Decoder.InternalStream != null)
			{
				if (Decoder.InternalStream.CanRead)
				{
					Decoder.InternalStream.Dispose();
				}
				Decoder.InternalStream = null;
			}
		}
		Decoder = null;
	}

	private void Dispose(bool disposing)
	{
		if (!m_bDisposed)
		{
			m_bDisposed = true;
		}
	}

	private void Initialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanRead || !stream.CanSeek)
		{
			throw new ArgumentException("Unable to process the specified image stream.");
		}
		if (!stream.IsJpeg() && !stream.IsPng())
		{
			throw new PdfException("Only JPEG and PNG images are supported");
		}
		if (ImageDecoder.TryGetDecoder(stream, base.EnableMetada, out ImageDecoder decoder))
		{
			Decoder = decoder;
			Height = Decoder.Height;
			Width = Decoder.Width;
			BitsPerComponent = Decoder.BitsPerComponent;
			base.JpegOrientationAngle = Decoder.JpegDecoderOrientationAngle;
			base.ImageStream = new PdfStream();
			byte[] array = new byte[Decoder.InternalStream.Length];
			Decoder.InternalStream.Position = 0L;
			Decoder.InternalStream.Read(array, 0, array.Length);
			base.ImageStream.InternalStream = new MemoryStream(array);
			imageStatus = false;
			array = null;
			return;
		}
		throw new ArgumentException("Invalid/Unsupported image stream.");
	}

	internal override void Save()
	{
		if (!imageStatus)
		{
			imageStatus = true;
			PngDecoder pngDecoder = null;
			if (Decoder != null && Decoder.Format == ImageType.Png)
			{
				pngDecoder = Decoder as PngDecoder;
				pngDecoder.DecodeImageData();
			}
			base.ImageStream = Decoder.GetImageDictionary();
			if (pngDecoder != null)
			{
				if (pngDecoder.m_isDecode)
				{
					if (pngDecoder.m_colorSpace != null)
					{
						SetColorSpace();
					}
				}
				else
				{
					SetColorSpace();
				}
				pngDecoder?.Dispose();
			}
			else
			{
				SetColorSpace();
			}
		}
		if (base.Metadata != null)
		{
			AddMetadata();
		}
	}

	internal override XmpMetadata GetMetadata()
	{
		return new ImageMetadataParser(Decoder.MetadataStream).TryGetMetadata();
	}

	private void SaveAsJpg()
	{
		ColorSpace = PdfColorSpace.RGB;
		BitsPerComponent = 8;
	}

	private void SaveRequiredItems()
	{
		PdfStream imageStream = base.ImageStream;
		PdfName name = imageStream.GetName("XObject");
		if (imageStream.ContainsKey("Filter"))
		{
			CheckImageType = 1;
		}
		imageStream["Type"] = name;
		PdfName name2 = imageStream.GetName("Image");
		imageStream["Subtype"] = name2;
		imageStream["Width"] = new PdfNumber(Width);
		imageStream["Height"] = new PdfNumber(Height);
		imageStream["BitsPerComponent"] = new PdfNumber(BitsPerComponent);
	}

	private void SaveImage(PdfArray filters)
	{
		if (!ImageMask)
		{
			SaveAsJpg();
			filters.Add(new PdfName("DCTDecode"));
		}
	}

	private void SetColorSpace()
	{
		PdfStream imageStream = base.ImageStream;
		PdfName pdfName = imageStream["ColorSpace"] as PdfName;
		if (pdfName == imageStream.GetName("DeviceCMYK"))
		{
			ColorSpace = PdfColorSpace.CMYK;
		}
		else if (pdfName != null && pdfName.Value == "DeviceGray")
		{
			ColorSpace = PdfColorSpace.GrayScale;
		}
		if (Decoder is PngDecoder && (Decoder as PngDecoder).m_colorSpace != null)
		{
			ColorSpace = PdfColorSpace.Indexed;
		}
		switch (ColorSpace)
		{
		case PdfColorSpace.CMYK:
			imageStream["Decode"] = new PdfArray(new float[8] { 1f, 0f, 1f, 0f, 1f, 0f, 1f, 0f });
			imageStream["ColorSpace"] = imageStream.GetName("DeviceCMYK");
			break;
		case PdfColorSpace.GrayScale:
			imageStream["Decode"] = new PdfArray(new float[2] { 0f, 1f });
			imageStream["ColorSpace"] = imageStream.GetName("DeviceGray");
			break;
		default:
			imageStream["Decode"] = new PdfArray(new float[6] { 0f, 1f, 0f, 1f, 0f, 1f });
			imageStream["ColorSpace"] = imageStream.GetName("DeviceRGB");
			break;
		case PdfColorSpace.Indexed:
			imageStream["ColorSpace"] = (Decoder as PngDecoder).m_colorSpace;
			break;
		}
	}

	private void SaveAddtionalItems()
	{
		PdfStream imageStream = base.ImageStream;
		imageStream["Filter"] = new PdfName("DCTDecode");
		imageStream["DecodeParms"] = new PdfDictionary
		{
			["K"] = new PdfNumber(-1),
			["Columns"] = new PdfNumber(Width),
			["Rows"] = new PdfNumber(Height),
			["BlackIs1"] = new PdfBoolean(value: true)
		};
		imageStream.Compress = false;
	}

	internal override PdfImage Clone()
	{
		return MemberwiseClone() as PdfBitmap;
	}
}
