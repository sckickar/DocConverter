using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.XmlReaders.Shapes;

internal abstract class ShapeParser
{
	public abstract ShapeImpl ParseShapeType(XmlReader reader, ShapeCollectionBase shapes);

	public abstract bool ParseShape(XmlReader reader, ShapeImpl defaultShape, RelationCollection relations, string parentItemPath);

	public static Stream ReadNodeAsStream(XmlReader reader)
	{
		return ReadNodeAsStream(reader, writeNamespaces: false);
	}

	public static Stream ReadNodeAsStream(XmlReader reader, bool writeNamespaces)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteNode(reader, writeNamespaces);
		xmlWriter.Flush();
		return memoryStream;
	}

	public static void WriteNodeFromStream(XmlWriter writer, Stream stream)
	{
		WriteNodeFromStream(writer, stream, writeNamespaces: false);
	}

	public static void WriteNodeFromStream(XmlWriter writer, Stream stream, bool writeNamespaces)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		XmlReader reader = UtilityMethods.CreateReader(stream);
		writer.WriteNode(reader, writeNamespaces);
		writer.Flush();
	}

	protected void ParseAnchor(XmlReader reader, ShapeImpl shape)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		string[] array = reader.ReadElementContentAsString().Split(',');
		if (array.Length != 8)
		{
			throw new XmlException("Wrong anchor format");
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = array[i].Trim();
		}
		MsofbtClientAnchor clientAnchor = shape.ClientAnchor;
		int num2 = int.Parse(array[0]);
		int iPixels = int.Parse(array[1]);
		clientAnchor.LeftColumn = num2;
		clientAnchor.LeftOffset = shape.PixelsInOffset(num2 + 1, iPixels, isXSize: true);
		shape.CheckLeftOffset();
		num2 = int.Parse(array[2]);
		iPixels = int.Parse(array[3]);
		clientAnchor.TopRow = num2;
		clientAnchor.TopOffset = shape.PixelsInOffset(num2 + 1, iPixels, isXSize: false);
		num2 = int.Parse(array[4]);
		iPixels = int.Parse(array[5]);
		clientAnchor.RightColumn = num2;
		clientAnchor.RightOffset = shape.PixelsInOffset(num2 + 1, iPixels, isXSize: true);
		num2 = int.Parse(array[6]);
		iPixels = int.Parse(array[7]);
		clientAnchor.BottomRow = num2;
		clientAnchor.BottomOffset = shape.PixelsInOffset(num2 + 1, iPixels, isXSize: false);
		shape.UpdateHeight();
		shape.UpdateWidth();
	}

	protected Dictionary<string, string> SplitStyle(string styleValue)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (styleValue != null)
		{
			string[] array = styleValue.Split(';');
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				string text = array[i];
				int num2 = text.IndexOf(':');
				if (num2 >= 0)
				{
					string key = text.Substring(0, num2).Trim();
					string value = text.Substring(num2 + 1, text.Length - num2 - 1).Trim();
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, value);
					}
				}
			}
		}
		return dictionary;
	}
}
