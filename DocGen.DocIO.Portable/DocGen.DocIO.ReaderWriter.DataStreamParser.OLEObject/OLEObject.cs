using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.CompoundFile.DocIO;
using DocGen.CompoundFile.DocIO.Net;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

internal class OLEObject
{
	private const string DEF_OLE_STREAM_NAME = "\u0001Ole";

	private const string DEF_CONTENT_STREAM_NAME = "CONTENTS";

	private const string DEF_WP_STREAM_NAME = "Contents";

	private const string DEF_INFO_STREAM_NAME = "\u0003ObjInfo";

	internal const string DEF_COMP_STREAM_NAME = "\u0001CompObj";

	private const string DEF_LINK_INFO_STREAM_NAME = "\u0003LinkInfo";

	private const string DEF_NATIVE_STREAM_NAME = "\u0001Ole10Native";

	private const string DEF_PRINT_STREAM_NAME = "\u0003EPRINT";

	private const string DEF_OLE_PRES000_NAME = "\u0002OlePres000";

	private const string DEF_END_INFO_MARKER = "???";

	private const string DEF_EQUATION_STREAM_NAME = "Equation Native";

	private const string DEF_WORKBOOK_STREAM_NAME = "Workbook";

	private const string DEF_PACKAGE_STREAM_NAME = "Package";

	private const string DEF_PPT_STREAM_NAME = "PowerPoint Document";

	private const string DEF_WORD_STREAM_NAME = "WordDocument";

	private const string DEF_VISIO_STREAM_NAME = "VisioDocument";

	private const string DEF_ODP_STREAM_NAME = "EmbeddedOdf";

	private const string DEF_OOPACKAGE_STREAM_NAME = "package_stream";

	private const string DEF_SUMMARY_STREAM_NAME = "\u0005SummaryInformation";

	private const string DEF_DOC_SUMMARY_STREAM_NAME = "\u0005DocumentSummaryInformation";

	private const string DEF_OBJECT_POOL_NAME = "ObjectPool";

	private OleObjectType m_oleType;

	private Storage m_storage = new Storage("Ole");

	private Guid m_guid;

	private byte m_bFlags;

	internal Guid Guid
	{
		get
		{
			return m_guid;
		}
		set
		{
			m_guid = value;
		}
	}

	internal Storage Storage => m_storage;

	internal OleObjectType OleType
	{
		get
		{
			if (Storage.Streams.ContainsKey("\u0001CompObj"))
			{
				CompObjectStream compObjectStream = new CompObjectStream(Storage.Streams["\u0001CompObj"]);
				m_oleType = OleTypeConvertor.ToOleType(compObjectStream.ObjectType);
			}
			else
			{
				m_oleType = OleObjectType.Undefined;
			}
			return m_oleType;
		}
	}

	internal bool Cloned
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal OLEObject()
	{
	}

	internal void ParseObjectPool(Stream objectPoolStream, string oleStorageName, Dictionary<string, Storage> oleObjectCollection)
	{
		Storage.Streams.Clear();
		Storage.StorageName = oleStorageName;
		using (DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile(objectPoolStream))
		{
			foreach (DirectoryEntry entry in compoundFile.Directory.Entries)
			{
				if ("_" + oleStorageName == entry.Name)
				{
					Guid = entry.StorageGuid;
					break;
				}
			}
			if (Array.IndexOf(compoundFile.RootStorage.Storages, "ObjectPool") != -1 && Array.IndexOf(compoundFile.RootStorage.OpenStorage("ObjectPool").Storages, "_" + oleStorageName) != -1)
			{
				ICompoundStorage compoundStorage = compoundFile.RootStorage.OpenStorage("ObjectPool");
				compoundStorage = compoundStorage.OpenStorage("_" + oleStorageName);
				Storage.ParseStreams(compoundStorage);
				Storage.ParseStorages(compoundStorage);
				string text = AddOleObjectToCollection(oleObjectCollection, oleStorageName);
				if (!string.IsNullOrEmpty(text))
				{
					Storage.StorageName = text;
				}
			}
		}
		objectPoolStream.Position = 0L;
	}

	internal void Save(Stream stream, WOleObject oleObject)
	{
		WriteOleStream(oleObject.LinkType, oleObject.OleObjectType, string.Empty);
		WriteObjInfoStream(oleObject.LinkType, oleObject.OleObjectType);
		WriteCompObjStream(oleObject.OleObjectType);
		if (oleObject.OleObjectType == OleObjectType.Undefined)
		{
			Storage.Streams.Add("Package", stream);
		}
		else
		{
			WriteNativeData((stream as MemoryStream).ToArray(), string.Empty, oleObject.OleObjectType);
		}
	}

	internal void Save(byte[] nativeData, string dataPath, WOleObject oleObject)
	{
		WriteOleStream(oleObject.LinkType, oleObject.OleObjectType, dataPath);
		WriteObjInfoStream(oleObject.LinkType, oleObject.OleObjectType);
		if (oleObject.LinkType == OleLinkType.Embed)
		{
			WriteCompObjStream(oleObject.OleObjectType);
			WriteNativeData(nativeData, dataPath, oleObject.OleObjectType);
		}
		else
		{
			WriteLinkInfoStream(oleObject.OleObjectType, dataPath);
		}
	}

	internal string AddOleObjectToCollection(Dictionary<string, Storage> oleObjectCollection, string oleStorageName)
	{
		if (oleObjectCollection != null && oleObjectCollection.Count == 0)
		{
			oleObjectCollection.Add(oleStorageName, Storage);
			oleObjectCollection[oleStorageName].Guid = Guid;
			oleObjectCollection[oleStorageName].OccurrenceCount++;
		}
		else
		{
			if (Cloned)
			{
				string text = Storage.CompareStorage(oleObjectCollection);
				if (!string.IsNullOrEmpty(text))
				{
					oleStorageName = text;
				}
			}
			if (!oleObjectCollection.ContainsKey(oleStorageName))
			{
				oleObjectCollection.Add(oleStorageName, Storage);
				oleObjectCollection[oleStorageName].OccurrenceCount++;
				oleObjectCollection[oleStorageName].Guid = Guid;
			}
			else
			{
				oleObjectCollection[oleStorageName].OccurrenceCount++;
				SetStorage(oleObjectCollection[oleStorageName]);
			}
		}
		return oleStorageName;
	}

	private void WriteNativeData(byte[] nativeData, string dataPath, OleObjectType objType)
	{
		switch (objType)
		{
		case OleObjectType.ExcelBinaryWorksheet:
		case OleObjectType.ExcelMacroWorksheet:
		case OleObjectType.ExcelWorksheet:
		case OleObjectType.PowerPointMacroPresentation:
		case OleObjectType.PowerPointMacroSlide:
		case OleObjectType.PowerPointPresentation:
		case OleObjectType.PowerPointSlide:
		case OleObjectType.WordDocument:
		case OleObjectType.WordMacroDocument:
			WriteNativeData(nativeData, "Package");
			break;
		case OleObjectType.WordPadDocument:
			WriteNativeData(nativeData, "Contents");
			break;
		case OleObjectType.AdobeAcrobatDocument:
			WriteNativeData(nativeData, "CONTENTS");
			break;
		case OleObjectType.Excel_97_2003_Worksheet:
		case OleObjectType.ExcelChart:
		case OleObjectType.PowerPoint_97_2003_Presentation:
		case OleObjectType.PowerPoint_97_2003_Slide:
		case OleObjectType.Word_97_2003_Document:
		case OleObjectType.VisioDrawing:
		case OleObjectType.OpenOfficeSpreadsheet1_1:
		case OleObjectType.OpenOfficeText_1_1:
		case OleObjectType.OpenOfficeSpreadsheet:
		case OleObjectType.OpenOfficeText:
		{
			MemoryStream stream = new MemoryStream(nativeData);
			WriteNativeStreams(stream);
			break;
		}
		case OleObjectType.Equation:
			WriteNativeData(nativeData, "Equation Native");
			break;
		case OleObjectType.GraphChart:
			WriteNativeData(nativeData, "Workbook");
			break;
		case OleObjectType.OpenDocumentPresentation:
		case OleObjectType.OpenDocumentSpreadsheet:
			WriteNativeData(nativeData, "EmbeddedOdf");
			break;
		case OleObjectType.BitmapImage:
			WritePBrush(nativeData);
			break;
		case OleObjectType.Package:
			WritePackage(nativeData, dataPath);
			break;
		case OleObjectType.MediaClip:
		case OleObjectType.MIDISequence:
		case OleObjectType.OpenDocumentText:
		case OleObjectType.VideoClip:
		case OleObjectType.WaveSound:
			break;
		}
	}

	private void WriteNativeData(byte[] nativeData, string streamName)
	{
		MemoryStream memoryStream = new MemoryStream(nativeData);
		memoryStream.Position = 0L;
		Storage.Streams.Add(streamName, memoryStream);
	}

	private void WritePBrush(byte[] nativeData)
	{
		int iOffset = 0;
		byte[] array = new byte[nativeData.Length + 4];
		ByteConverter.WriteInt32(array, ref iOffset, nativeData.Length);
		ByteConverter.WriteBytes(array, ref iOffset, nativeData);
		MemoryStream memoryStream = new MemoryStream(array);
		memoryStream.Position = 0L;
		Storage.Streams.Add("\u0001Ole10Native", memoryStream);
	}

	private void WriteNativeStreams(Stream stream)
	{
		DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile(stream);
		string[] streams = compoundFile.RootStorage.Streams;
		int i = 0;
		for (int num = streams.Length; i < num; i++)
		{
			CompoundStream compoundStream = compoundFile.RootStorage.OpenStream(streams[i]);
			byte[] array = new byte[compoundStream.Length];
			compoundStream.Read(array, 0, array.Length);
			compoundStream.Dispose();
			Storage.Streams.Add(streams[i], new MemoryStream(array));
		}
		compoundFile.Dispose();
	}

	private void WriteCompObjStream(OleObjectType objType)
	{
		switch (objType)
		{
		case OleObjectType.AdobeAcrobatDocument:
		case OleObjectType.BitmapImage:
		case OleObjectType.Equation:
		case OleObjectType.GraphChart:
		case OleObjectType.Excel_97_2003_Worksheet:
		case OleObjectType.ExcelBinaryWorksheet:
		case OleObjectType.ExcelChart:
		case OleObjectType.ExcelMacroWorksheet:
		case OleObjectType.ExcelWorksheet:
		case OleObjectType.PowerPoint_97_2003_Presentation:
		case OleObjectType.PowerPoint_97_2003_Slide:
		case OleObjectType.PowerPointMacroPresentation:
		case OleObjectType.PowerPointMacroSlide:
		case OleObjectType.PowerPointPresentation:
		case OleObjectType.PowerPointSlide:
		case OleObjectType.Word_97_2003_Document:
		case OleObjectType.WordDocument:
		case OleObjectType.WordMacroDocument:
		case OleObjectType.VisioDrawing:
		case OleObjectType.OpenDocumentPresentation:
		case OleObjectType.OpenDocumentSpreadsheet:
		case OleObjectType.OpenOfficeSpreadsheet1_1:
		case OleObjectType.OpenOfficeText_1_1:
		case OleObjectType.Package:
		case OleObjectType.OpenOfficeSpreadsheet:
		case OleObjectType.OpenOfficeText:
			if (!Storage.Streams.ContainsKey("\u0001CompObj"))
			{
				MemoryStream memoryStream = new MemoryStream();
				new CompObjectStream(objType).SaveTo(memoryStream);
				memoryStream.Flush();
				memoryStream.Position = 0L;
				Storage.Streams.Add("\u0001CompObj", memoryStream);
			}
			break;
		case OleObjectType.MediaClip:
		case OleObjectType.MIDISequence:
		case OleObjectType.OpenDocumentText:
		case OleObjectType.VideoClip:
		case OleObjectType.WaveSound:
		case OleObjectType.WordPadDocument:
			break;
		}
	}

	private void WriteLinkInfoStream(OleObjectType objType, string dataPath)
	{
		MemoryStream memoryStream = new MemoryStream();
		new LinkInfoStream(dataPath).SaveTo(memoryStream);
		memoryStream.Flush();
		memoryStream.Position = 0L;
		Storage.Streams.Add("\u0003LinkInfo", memoryStream);
	}

	private void WriteOleStream(OleLinkType linkType, OleObjectType objType, string dataPath)
	{
		switch (objType)
		{
		case OleObjectType.AdobeAcrobatDocument:
		case OleObjectType.BitmapImage:
		case OleObjectType.Equation:
		case OleObjectType.GraphChart:
		case OleObjectType.Excel_97_2003_Worksheet:
		case OleObjectType.ExcelBinaryWorksheet:
		case OleObjectType.ExcelChart:
		case OleObjectType.ExcelMacroWorksheet:
		case OleObjectType.ExcelWorksheet:
		case OleObjectType.PowerPoint_97_2003_Presentation:
		case OleObjectType.PowerPoint_97_2003_Slide:
		case OleObjectType.PowerPointMacroPresentation:
		case OleObjectType.PowerPointMacroSlide:
		case OleObjectType.PowerPointSlide:
		case OleObjectType.VisioDrawing:
		case OleObjectType.OpenDocumentPresentation:
		case OleObjectType.OpenDocumentSpreadsheet:
		case OleObjectType.OpenOfficeSpreadsheet1_1:
		case OleObjectType.OpenOfficeText_1_1:
		case OleObjectType.Package:
		case OleObjectType.WordPadDocument:
		case OleObjectType.OpenOfficeText:
		{
			MemoryStream memoryStream = new MemoryStream();
			new OLEStream(linkType, dataPath).SaveTo(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			Storage.Streams.Add("\u0001Ole", memoryStream);
			break;
		}
		case OleObjectType.MediaClip:
		case OleObjectType.PowerPointPresentation:
		case OleObjectType.Word_97_2003_Document:
		case OleObjectType.WordDocument:
		case OleObjectType.WordMacroDocument:
		case OleObjectType.MIDISequence:
		case OleObjectType.OpenDocumentText:
		case OleObjectType.VideoClip:
		case OleObjectType.WaveSound:
		case OleObjectType.OpenOfficeSpreadsheet:
			break;
		}
	}

	private void WriteObjInfoStream(OleLinkType linkType, OleObjectType objType)
	{
		MemoryStream memoryStream = new MemoryStream();
		new ObjectInfoStream().SaveTo(memoryStream, linkType, objType);
		memoryStream.Flush();
		memoryStream.Position = 0L;
		Storage.Streams.Add("\u0003ObjInfo", memoryStream);
	}

	private void WritePackage(byte[] nativeData, string dataPath)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		string s = dataPath[(dataPath.LastIndexOf('/') + 1)..];
		byte[] bytes = uTF8Encoding.GetBytes(s);
		byte[] bytes2 = uTF8Encoding.GetBytes(dataPath);
		byte[] array = new byte[2] { 2, 0 };
		byte[] array2 = new byte[4] { 0, 0, 3, 0 };
		int num = 4;
		num += array.Length;
		num += bytes.Length + 1;
		num += bytes2.Length + 1;
		num += array2.Length;
		num += 4;
		num += bytes2.Length + 1;
		num += 4;
		num += nativeData.Length;
		num += 2;
		int iOffset = 0;
		byte[] array3 = new byte[num];
		ByteConverter.WriteInt32(array3, ref iOffset, num - 4);
		ByteConverter.WriteBytes(array3, ref iOffset, array);
		ByteConverter.WriteBytes(array3, ref iOffset, bytes);
		iOffset++;
		ByteConverter.WriteBytes(array3, ref iOffset, bytes2);
		iOffset++;
		ByteConverter.WriteBytes(array3, ref iOffset, array2);
		ByteConverter.WriteInt32(array3, ref iOffset, bytes2.Length + 1);
		ByteConverter.WriteBytes(array3, ref iOffset, bytes2);
		iOffset++;
		ByteConverter.WriteInt32(array3, ref iOffset, nativeData.Length);
		ByteConverter.WriteBytes(array3, ref iOffset, nativeData);
		Storage.Streams.Add("\u0001Ole10Native", new MemoryStream(array3));
	}

	internal OLEObject Clone()
	{
		return new OLEObject
		{
			m_guid = m_guid,
			m_oleType = m_oleType,
			m_storage = m_storage.Clone()
		};
	}

	internal void Close()
	{
		m_storage.Close();
	}

	internal void SetStorage(Storage storage)
	{
		m_storage = storage;
	}
}
