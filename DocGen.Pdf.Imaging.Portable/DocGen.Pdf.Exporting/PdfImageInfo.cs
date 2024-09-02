using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf.Exporting;

public class PdfImageInfo : IDisposable
{
	private RectangleF m_bounds;

	private Bitmap m_bitmap;

	private Image m_image;

	private int m_index;

	private string m_name;

	private PdfMatrix m_matrix;

	private bool m_maskImage;

	private bool m_bisImageExtracted;

	internal bool m_isImageMasked;

	internal bool m_isImageInterpolated;

	internal bool m_isSoftMasked;

	private XmpMetadata m_metadata;

	internal ImageStructureNet m_imageStructure;

	private Stream m_imageStream;

	internal bool isRotate;

	internal float internalRotation;

	internal RectangleF bounds;

	internal bool regenerateImage;

	internal ImageExportHelperNet helper;

	internal string[] m_imageFilter;

	private bool m_bDisposed;

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		internal set
		{
			m_bounds = value;
		}
	}

	public Stream ImageStream
	{
		get
		{
			if (m_imageStructure != null && m_imageStream == null)
			{
				m_imageStream = m_imageStructure.GetEmbeddedImageStream();
			}
			return m_imageStream;
		}
		internal set
		{
			m_imageStream = value;
		}
	}

	public string[] ImageFilter
	{
		get
		{
			if (m_imageStructure != null)
			{
				m_imageFilter = m_imageStructure.ImageFilter;
			}
			return m_imageFilter;
		}
	}

	internal Bitmap Image
	{
		get
		{
			if (m_bitmap == null && m_imageStructure != null && !IsImageExtracted)
			{
				IsImageExtracted = true;
				Bitmap bitmap = m_imageStructure.EmbeddedImage;
				if (bitmap != null)
				{
					if (bitmap.m_sKBitmap == null && m_imageStructure.imageFormat != ImageFormat.Tiff)
					{
						bitmap = Bitmap.FromStream(m_imageStructure.DecodedMemoryStream);
					}
					if (internalRotation == 270f)
					{
						SKBitmap sKBitmap = new SKBitmap(bitmap.m_sKBitmap.Height, bitmap.m_sKBitmap.Width);
						using (SKCanvas sKCanvas = new SKCanvas(sKBitmap))
						{
							sKCanvas.RotateDegrees(270f, bitmap.m_sKBitmap.Width / 2, bitmap.m_sKBitmap.Width / 2);
							sKCanvas.DrawBitmap(bitmap.m_sKBitmap, 0f, 0f);
						}
						bitmap.m_sKBitmap = sKBitmap;
						isRotate = true;
					}
					else if (internalRotation == 90f)
					{
						SKBitmap sKBitmap2 = new SKBitmap(bitmap.m_sKBitmap.Height, bitmap.m_sKBitmap.Width);
						using (SKCanvas sKCanvas2 = new SKCanvas(sKBitmap2))
						{
							sKCanvas2.RotateDegrees(90f, bitmap.m_sKBitmap.Width / 2, bitmap.m_sKBitmap.Width / 2);
							sKCanvas2.DrawBitmap(bitmap.m_sKBitmap, 0f, 0f);
						}
						bitmap.m_sKBitmap = sKBitmap2;
						isRotate = true;
					}
					else if (internalRotation == 180f)
					{
						SKBitmap sKBitmap3 = new SKBitmap(bitmap.m_sKBitmap.Height, bitmap.m_sKBitmap.Width);
						using (SKCanvas sKCanvas3 = new SKCanvas(sKBitmap3))
						{
							sKCanvas3.RotateDegrees(180f, bitmap.m_sKBitmap.Width / 2, bitmap.m_sKBitmap.Width / 2);
							sKCanvas3.DrawBitmap(bitmap.m_sKBitmap, 0f, 0f);
						}
						bitmap.m_sKBitmap = sKBitmap3;
						isRotate = true;
					}
					if (regenerateImage)
					{
						helper.RegenerateImages(bitmap, Name);
					}
					if (isRotate)
					{
						PdfName key = new PdfName(Name);
						if (helper.m_roatedImages.ContainsKey(key))
						{
							helper.m_roatedImages[key].Add(internalRotation);
						}
						else
						{
							List<float> list = new List<float>();
							list.Add(internalRotation);
							helper.m_roatedImages[key] = list;
						}
					}
					helper.extractedImages.Add(bitmap);
					m_bitmap = bitmap;
				}
			}
			return m_bitmap;
		}
		set
		{
			m_bitmap = value;
		}
	}

	public int Index
	{
		get
		{
			return m_index;
		}
		internal set
		{
			m_index = value;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal PdfMatrix Matrix
	{
		get
		{
			return m_matrix;
		}
		set
		{
			m_matrix = value;
		}
	}

	internal bool MaskImage
	{
		get
		{
			return m_maskImage;
		}
		set
		{
			m_maskImage = value;
		}
	}

	public bool IsSoftMasked => m_isSoftMasked;

	public bool IsImageMasked => m_isImageMasked;

	public bool IsImageInterpolated => m_isImageInterpolated;

	internal bool IsImageExtracted
	{
		get
		{
			return m_bisImageExtracted;
		}
		set
		{
			m_bisImageExtracted = value;
		}
	}

	public XmpMetadata Metadata
	{
		get
		{
			return m_metadata;
		}
		internal set
		{
			m_metadata = value;
		}
	}

	internal PdfImageInfo()
	{
	}

	~PdfImageInfo()
	{
		Dispose(disposing: false);
	}

	public void SaveImage(Stream stream)
	{
		if (m_imageStructure != null)
		{
			if (ImageStream != null && m_imageStructure.m_isCCITTFaxDecode)
			{
				byte[] array = new byte[ImageStream.Length];
				ImageStream.Position = 0L;
				ImageStream.Read(array, 0, array.Length);
				stream.Write(array, 0, array.Length);
			}
			else if (Image != null && Image.m_imageData != null)
			{
				stream.Write(Image.m_imageData, 0, Image.m_imageData.Length);
			}
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!m_bDisposed)
		{
			if (m_imageStream != null && disposing)
			{
				m_imageStream.Dispose();
				m_imageStream = null;
			}
			if (m_imageFilter != null)
			{
				m_imageFilter = null;
			}
			if (m_image != null)
			{
				m_image.Dispose();
				m_image = null;
			}
			if (m_bitmap != null)
			{
				m_bitmap.Dispose();
				m_bitmap = null;
			}
			if (m_matrix != null)
			{
				m_matrix = null;
			}
			if (m_imageStructure != null)
			{
				m_imageStructure = null;
			}
			m_bDisposed = true;
		}
	}
}
