using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal sealed class UtilityMethods
{
	private const int DEF_WRONG_DATE = 61;

	private const int DEF_EXCEL2007_MAX_ROW_COUNT = 1048576;

	private const int DEF_EXCEL2007_MAX_COLUMN_COUNT = 16384;

	private const int DEF_EXCEL97TO03_MAX_ROW_COUNT = 65536;

	private const int DEF_EXCEL97TO03_MAX_COLUMN_COUNT = 256;

	private UtilityMethods()
	{
	}

	public static bool Intersects(Rectangle rect1, Rectangle rect2)
	{
		if (rect1.X <= rect2.X + rect2.Width && rect2.X <= rect1.X + rect1.Width && rect1.Y <= rect2.Y + rect2.Height)
		{
			return rect2.Y <= rect1.Y + rect1.Height;
		}
		return false;
	}

	public static bool Contains(Rectangle rect, int x, int y)
	{
		if (rect.X <= x && x <= rect.X + rect.Width && rect.Y <= y)
		{
			return y < rect.Y + rect.Height;
		}
		return false;
	}

	public static int IndexOf(TBIFFRecord[] array, TBIFFRecord value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOf(int[] array, int value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOf(short[] array, short value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static double ConvertDateTimeToNumber(DateTime dateTime)
	{
		double num = dateTime.ToOADate();
		if (num < 61.0)
		{
			num -= 1.0;
		}
		return num;
	}

	internal static XmlWriter CreateWriter(MemoryStream stream)
	{
		return XmlWriter.Create(stream);
	}

	internal static Stream ReadSingleNodeIntoStream(XmlReader reader)
	{
		MemoryStream memoryStream = new MemoryStream();
		using XmlWriter xmlWriter = CreateWriter(memoryStream);
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement("sld");
		xmlWriter.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlWriter.WriteAttributeString("xmlns", "p", null, "http://schemas.openxmlformats.org/presentationml/2006/main");
		xmlWriter.WriteNode(reader, defattr: true);
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndDocument();
		xmlWriter.Flush();
		return memoryStream;
	}

	public static DateTime ConvertNumberToDateTime(double dNumber, bool is1904DateSystem)
	{
		if (is1904DateSystem)
		{
			dNumber += 1462.0;
		}
		else if (dNumber < 61.0)
		{
			dNumber += 1.0;
		}
		return CalcEngineHelper.FromOADate(dNumber);
	}

	[CLSCompliant(false)]
	public static ICellPositionFormat CreateCell(int iRow, int iColumn, TBIFFRecord recordType)
	{
		ICellPositionFormat obj = (ICellPositionFormat)BiffRecordFactory.GetRecord(recordType);
		obj.Row = iRow;
		obj.Column = iColumn;
		return obj;
	}

	public static string RemoveFirstCharUnsafe(string value)
	{
		return value.Substring(1, value.Length - 1);
	}

	public static string Join(string separator, List<string> value)
	{
		if (separator == null)
		{
			separator = string.Empty;
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int num = 0;
		int count = value.Count;
		string text;
		for (int i = 0; i < count; i++)
		{
			text = value[i];
			if (text != null)
			{
				num += text.Length;
			}
		}
		num += (count - 1) * separator.Length;
		if (num < 0 || num + 1 < 0)
		{
			throw new OutOfMemoryException();
		}
		if (num == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		text = value[0];
		if (value != null)
		{
			stringBuilder.Append(value[0]);
		}
		for (int j = 1; j < count; j++)
		{
			stringBuilder.Append(separator);
			text = value[j];
			if (text == null)
			{
				text = string.Empty;
			}
			stringBuilder.Append(text);
		}
		return stringBuilder.ToString();
	}

	public static void GetMaxRowColumnCount(out int iRows, out int iColumns, OfficeVersion version)
	{
		switch (version)
		{
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			iRows = 1048576;
			iColumns = 16384;
			break;
		case OfficeVersion.Excel97to2003:
			iRows = 65536;
			iColumns = 256;
			break;
		default:
			throw new ArgumentException("Unknown version");
		}
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

	public static MemoryStream CloneStream(MemoryStream source)
	{
		MemoryStream memoryStream = new MemoryStream((int)source.Length);
		long position = source.Position;
		source.Position = 0L;
		CopyStreamTo(source, memoryStream);
		long position2 = (source.Position = position);
		memoryStream.Position = position2;
		return memoryStream;
	}

	internal static XmlReader CreateReader(Stream data, string tag)
	{
		data.Position = 0L;
		XmlReader xmlReader = XmlReader.Create(data);
		while (!xmlReader.EOF && xmlReader.LocalName != tag)
		{
			xmlReader.Read();
		}
		return xmlReader;
	}

	public static XmlReader CreateReader(Stream data, bool skipToElement)
	{
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

	public static XmlReader CreateReaderFromStreamPosition(Stream data)
	{
		return CreateReader(data, skipToElement: true);
	}

	public static XmlReader CreateReader(Stream data)
	{
		if (data.CanSeek && data.Position != 0L)
		{
			data.Position = 0L;
		}
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
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.CheckCharacters = false;
		return XmlWriter.Create(data, xmlWriterSettings);
	}

	public static XmlWriter CreateWriter(TextWriter data, bool indent)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = indent;
		return XmlWriter.Create(data, xmlWriterSettings);
	}
}
