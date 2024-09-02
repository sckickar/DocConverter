using System;
using System.IO;
using System.Text;
using System.Xml;

namespace DocGen.DocIO.DLS.Convertors;

internal class UtilityMethods
{
	public static DateTime ConvertNumberToDateTime(double dNumber)
	{
		if (dNumber < 61.0)
		{
			dNumber += 1.0;
		}
		return DateTimeFromOADate(dNumber);
	}

	private static DateTime DateTimeFromOADate(double dNumber)
	{
		throw new NotImplementedException();
	}

	public static void CopyStreamTo(Stream source, Stream destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		byte[] buffer = new byte[32768];
		int count;
		while ((count = source.Read(buffer, 0, 32768)) > 0)
		{
			destination.Write(buffer, 0, count);
		}
	}

	public static Stream CloneStream(Stream source)
	{
		Stream stream = new MemoryStream((int)source.Length);
		long position = source.Position;
		source.Position = 0L;
		CopyStreamTo(source, stream);
		long position2 = (source.Position = position);
		stream.Position = position2;
		return stream;
	}

	public static XmlReader CreateReader(Stream data, bool skipToElement)
	{
		data.Position = 0L;
		XmlReader xmlReader = XmlReader.Create(data);
		if (skipToElement)
		{
			while (xmlReader.NodeType != XmlNodeType.Element)
			{
				xmlReader.Read();
			}
		}
		return xmlReader;
	}

	public static XmlReader CreateReader(Stream data)
	{
		return CreateReader(data, skipToElement: true);
	}

	public static XmlWriter CreateWriter(Stream data, Encoding encoding)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = encoding;
		return XmlWriter.Create(data, xmlWriterSettings);
	}

	public static XmlWriter CreateWriter(TextWriter data)
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		return XmlWriter.Create(data, settings);
	}

	public static XmlWriter CreateWriter(TextWriter data, bool indent)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = indent;
		return XmlWriter.Create(data, xmlWriterSettings);
	}
}
