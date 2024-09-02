using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Images.Decoder;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

public class PdfXfaImage : PdfXfaField
{
	private PdfStream m_imageStream;

	private SizeF m_size = SizeF.Empty;

	private RectangleF m_bounds;

	private string m_subFormName;

	private PdfXfaRotateAngle m_rotate;

	internal PdfXfaForm parent;

	private PdfBitmap m_image;

	internal string imageBase64String = string.Empty;

	internal string imageFormat = string.Empty;

	internal bool isBase64Type;

	internal PdfStream ImageStream
	{
		get
		{
			return m_imageStream;
		}
		set
		{
			m_imageStream = value;
			List<PdfName> list = new List<PdfName>();
			foreach (PdfName key in m_imageStream.Keys)
			{
				list.Add(key);
			}
			foreach (PdfName item in list)
			{
				m_imageStream.Remove(item);
			}
		}
	}

	public SizeF Size
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	internal RectangleF ImageBounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
			m_size = m_bounds.Size;
		}
	}

	internal string SubFormName
	{
		get
		{
			return m_subFormName;
		}
		set
		{
			m_subFormName = value;
		}
	}

	public PdfXfaRotateAngle Rotate
	{
		get
		{
			return m_rotate;
		}
		set
		{
			m_rotate = value;
		}
	}

	public PdfXfaImage(string name, Stream stream)
	{
		imageBase64String = Convert.ToBase64String(StreamToByteArray(stream));
		isBase64Type = true;
		base.Name = name;
		m_image = new PdfBitmap(stream);
		if (stream.IsPng())
		{
			imageFormat = "png";
		}
		else
		{
			if (!stream.IsJpeg())
			{
				throw new PdfException("Only JPEG and PNG images are supported");
			}
			imageFormat = "jpeg";
		}
		ImageBounds = new RectangleF(default(PointF), new SizeF(m_image.Width, m_image.Height));
	}

	public PdfXfaImage(string name, Stream stream, SizeF size)
	{
		imageBase64String = Convert.ToBase64String(StreamToByteArray(stream));
		isBase64Type = true;
		base.Name = name;
		if (stream.IsPng())
		{
			imageFormat = "png";
		}
		else
		{
			if (!stream.IsJpeg())
			{
				throw new PdfException("Only JPEG and PNG images are supported");
			}
			imageFormat = "jpeg";
		}
		ImageBounds = new RectangleF(default(PointF), size);
	}

	public PdfXfaImage(string name, Stream stream, float width, float height)
	{
		imageBase64String = Convert.ToBase64String(StreamToByteArray(stream));
		isBase64Type = true;
		base.Name = name;
		if (stream.IsPng())
		{
			imageFormat = "png";
		}
		else
		{
			if (!stream.IsJpeg())
			{
				throw new PdfException("Only JPEG and PNG images are supported");
			}
			imageFormat = "jpeg";
		}
		ImageBounds = new RectangleF(default(PointF), new SizeF(width, height));
	}

	internal PdfXfaImage(PdfBitmap image, RectangleF bounds)
	{
		m_image = image;
		image.Save();
		ImageStream = image.ImageStream;
		ImageBounds = bounds;
	}

	private byte[] StreamToByteArray(Stream stream)
	{
		stream.Position = 0L;
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		return array;
	}

	private string GetImageType(ImageFormat imf)
	{
		string result = string.Empty;
		if (imf.Equals(ImageFormat.Jpeg))
		{
			result = "jpg";
		}
		else if (imf.Equals(ImageFormat.Png))
		{
			result = "png";
		}
		else if (imf.Equals(ImageFormat.Tiff))
		{
			result = "tiff";
		}
		else if (imf.Equals(ImageFormat.Bmp))
		{
			result = "bmp";
		}
		else if (imf.Equals(ImageFormat.Gif))
		{
			result = "gif";
		}
		return result;
	}

	internal void Save(int fieldCount, string imageName, XfaWriter xfaWriter)
	{
		xfaWriter.Write.WriteStartElement("draw");
		if (base.Name != string.Empty && base.Name != null)
		{
			xfaWriter.Write.WriteAttributeString("name", base.Name);
		}
		else
		{
			xfaWriter.Write.WriteAttributeString("name", "image" + fieldCount);
		}
		xfaWriter.SetRPR(Rotate, base.Visibility, isReadOnly: false);
		if (Size != SizeF.Empty)
		{
			xfaWriter.SetSize(Size.Height, Size.Width, 0f, 0f);
		}
		else
		{
			xfaWriter.SetSize(ImageBounds.Height, ImageBounds.Width, 0f, 0f);
		}
		xfaWriter.WriteUI("imageEdit", null, null);
		xfaWriter.WriteMargins(base.Margins);
		xfaWriter.Write.WriteStartElement("value");
		xfaWriter.Write.WriteStartElement("image");
		if (!isBase64Type)
		{
			xfaWriter.Write.WriteAttributeString("href", imageName);
			xfaWriter.Write.WriteAttributeString("aspect", "none");
		}
		else
		{
			string text = "image";
			if (imageFormat != string.Empty)
			{
				text = text + "/" + imageFormat;
			}
			xfaWriter.Write.WriteAttributeString("contentType", text);
			xfaWriter.Write.WriteAttributeString("aspect", "none");
			xfaWriter.Write.WriteString(imageBase64String);
		}
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
	}

	internal void SaveAcroForm(PdfPage page, RectangleF bounds)
	{
		RectangleF rectangleF = default(RectangleF);
		SizeF size = GetSize();
		rectangleF = new RectangleF(new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top), new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom)));
		PdfImage pdfImage = null;
		pdfImage = ((!isBase64Type) ? PdfImage.FromStream(m_imageStream.InternalStream) : new PdfBitmap(new MemoryStream(Convert.FromBase64String(imageBase64String))));
		PdfGraphics graphics = page.Graphics;
		graphics.Save();
		graphics.TranslateTransform(rectangleF.X, rectangleF.Y);
		graphics.RotateTransform(-GetRotationAngle());
		RectangleF rectangle = RectangleF.Empty;
		switch (GetRotationAngle())
		{
		case 180:
			rectangle = new RectangleF(0f - rectangleF.Width, 0f - rectangleF.Height, rectangleF.Width, rectangleF.Height);
			break;
		case 90:
			rectangle = new RectangleF(0f - rectangleF.Height, 0f, rectangleF.Height, rectangleF.Width);
			break;
		case 270:
			rectangle = new RectangleF(0f, 0f - rectangleF.Width, rectangleF.Height, rectangleF.Width);
			break;
		case 0:
			rectangle = new RectangleF(0f, 0f, rectangleF.Width, rectangleF.Height);
			break;
		}
		graphics.DrawImage(pdfImage, rectangle);
		graphics.Restore();
	}

	internal SizeF GetSize()
	{
		if (Rotate == PdfXfaRotateAngle.RotateAngle270 || Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(Size.Height, Size.Width);
		}
		return Size;
	}

	private int GetRotationAngle()
	{
		int result = 0;
		if (Rotate != 0)
		{
			switch (Rotate)
			{
			case PdfXfaRotateAngle.RotateAngle180:
				result = 180;
				break;
			case PdfXfaRotateAngle.RotateAngle270:
				result = 270;
				break;
			case PdfXfaRotateAngle.RotateAngle90:
				result = 90;
				break;
			}
		}
		return result;
	}

	internal object Clone()
	{
		return MemberwiseClone();
	}
}
