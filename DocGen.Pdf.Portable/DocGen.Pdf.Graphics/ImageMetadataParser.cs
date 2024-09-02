using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using DocGen.Pdf.Graphics.Images.Decoder;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf.Graphics;

internal class ImageMetadataParser
{
	private Stream m_stream;

	internal ImageMetadataParser(Stream stream, string type)
	{
		stream.Position = 0L;
		switch (type)
		{
		case "Jpeg":
			m_stream = new JpegDecoder(stream, enableMetadata: true).MetadataStream;
			break;
		case "Png":
			m_stream = new PngDecoder(stream, enableMetadata: true).MetadataStream;
			break;
		case "Tiff":
			m_stream = new TiffMetadataParser(stream).GetMetadata();
			break;
		case "Gif":
			m_stream = new GifMetadataParser(stream).GetMetadata();
			break;
		}
	}

	internal ImageMetadataParser(Stream stream)
	{
		m_stream = stream;
	}

	internal XmpMetadata TryGetMetadata()
	{
		if (m_stream != null)
		{
			m_stream.Position = 0L;
			try
			{
				new XDocument();
				return new XmpMetadata(XDocument.Load(XmlReader.Create(m_stream)));
			}
			catch (Exception)
			{
			}
		}
		return null;
	}
}
