using System;
using System.IO;
using System.IO.Compression;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class Indexed : Colorspace
{
	private Colorspace m_baseColorspace;

	private int m_hiVal;

	private LookupParameter m_lookup;

	private string m_filter;

	internal override int Components => 1;

	internal override PdfBrush DefaultBrush => BaseColorspace.DefaultBrush;

	internal Colorspace BaseColorspace
	{
		get
		{
			return m_baseColorspace;
		}
		set
		{
			m_baseColorspace = value;
		}
	}

	internal int HiVal
	{
		get
		{
			return m_hiVal;
		}
		set
		{
			m_hiVal = value;
		}
	}

	internal LookupParameter Lookup
	{
		get
		{
			return m_lookup;
		}
		set
		{
			m_lookup = value;
		}
	}

	private Color GetColor(int index)
	{
		if (BaseColorspace == null || Lookup == null)
		{
			return Color.Transparent;
		}
		if (index < 0)
		{
			index = 0;
		}
		if (index > HiVal)
		{
			index = HiVal;
		}
		Colorspace colorSpace = GetColorSpace();
		if (index * colorSpace.Components == Lookup.Data.Length)
		{
			index = 0;
		}
		return colorSpace.GetColor(Lookup.Data, index * colorSpace.Components);
	}

	internal void SetValue(PdfArray array)
	{
		m_baseColorspace = GetBaseColorspace(array);
		m_hiVal = (array[2] as PdfNumber).IntValue;
		m_lookup = new LookupParameter(GetLookupData(array));
	}

	internal override Color GetColor(string[] pars)
	{
		float.TryParse(pars[0], out var result);
		return GetColor((int)result);
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		throw new NotSupportedException("This method is not supported.");
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}

	private Colorspace GetBaseColorspace(PdfArray array)
	{
		if (array[1] is PdfName)
		{
			return Colorspace.CreateColorSpace((array[1] as PdfName).Value);
		}
		if (array[1] is PdfArray)
		{
			PdfArray pdfArray = array[1] as PdfArray;
			if (pdfArray[0] is PdfName)
			{
				return Colorspace.CreateColorSpace((array[0] as PdfName).Value, pdfArray);
			}
		}
		return Colorspace.CreateColorSpace("DeviceRGB");
	}

	private byte[] GetLookupData(PdfArray array)
	{
		_ = new byte[array.Count];
		PdfReferenceHolder pdfReferenceHolder = array[3] as PdfReferenceHolder;
		if (pdfReferenceHolder == null)
		{
			return (array.Elements[3] as PdfString).Bytes;
		}
		return Load(pdfReferenceHolder.Object as PdfStream);
	}

	private Colorspace GetColorSpace()
	{
		Colorspace colorspace = BaseColorspace;
		if (colorspace is ICCBased iCCBased)
		{
			colorspace = iCCBased.Profile.AlternateColorspace;
		}
		return colorspace;
	}

	private byte[] Load(PdfDictionary dictionary)
	{
		m_filter = (dictionary.Items.ContainsKey(new PdfName("Filter")) ? (dictionary[new PdfName("Filter")] as PdfName).Value : "");
		return GetDecodedStream(dictionary as PdfStream);
	}

	private byte[] GetDecodedStream(PdfStream stream)
	{
		if (m_filter == "FlateDecode")
		{
			return DecodeFlateStream(stream.InternalStream).ToArray();
		}
		return stream.Data;
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
}
