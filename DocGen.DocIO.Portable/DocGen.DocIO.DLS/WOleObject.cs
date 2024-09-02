using System;
using System.IO;
using System.Text;
using DocGen.CompoundFile.DocIO;
using DocGen.CompoundFile.DocIO.Net;
using DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WOleObject : ParagraphItem, ILeafWidget, IWidget
{
	private const string DEF_OBJECT_POOL_NAME = "ObjectPool";

	private const string DEF_INFO_STREAM_NAME = "\u0003ObjInfo";

	private const int DEF_STRUCT_SIZE = 6;

	internal WPicture m_picture;

	private WField m_field;

	private string m_oleStorageName;

	private string m_linkAddress;

	private string m_strObjType;

	private OleObjectType m_oleObjType;

	internal OleLinkType m_linkType;

	private XmlParagraphItem m_oleXmlItem;

	private OLEObject m_oleObject;

	private static Random m_oleRandomIdGen;

	private string m_packageFileName = string.Empty;

	private byte m_bFlags = 1;

	public bool DisplayAsIcon
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
			if (!base.Document.IsOpening)
			{
				UpdateOleObjInfoStream();
			}
		}
	}

	public WPicture OlePicture => m_picture;

	public override EntityType EntityType => EntityType.OleObject;

	public Stream Container => GetOleContainer();

	internal WField Field
	{
		get
		{
			if (m_field == null)
			{
				m_field = new WField(m_doc);
			}
			if (m_field.FieldType == FieldType.FieldNone)
			{
				SetFieldType();
			}
			if (base.Owner != this)
			{
				m_field.SetOwner(this);
			}
			return m_field;
		}
		set
		{
			m_field = value;
		}
	}

	public string OleStorageName
	{
		get
		{
			return m_oleStorageName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = new Random().Next().ToString();
			}
			if (m_doc != null && !m_doc.IsOpening && m_doc.OleObjectCollection.ContainsKey(m_oleStorageName) && value != m_oleStorageName && m_doc.OleObjectCollection[m_oleStorageName].OccurrenceCount != 0)
			{
				m_doc.OleObjectCollection[m_oleStorageName].OccurrenceCount--;
				OleObject.SetStorage(m_doc.OleObjectCollection[m_oleStorageName].Clone());
				m_doc.OleObjectCollection.Add(value, OleObject.Storage);
				m_doc.OleObjectCollection[value].Guid = Guid;
			}
			m_oleStorageName = value;
		}
	}

	public string LinkPath
	{
		get
		{
			if (string.IsNullOrEmpty(m_linkAddress))
			{
				UpdateProps();
			}
			return m_linkAddress;
		}
		set
		{
			m_linkAddress = value;
		}
	}

	public OleLinkType LinkType => m_linkType;

	internal XmlParagraphItem OleXmlItem
	{
		get
		{
			return m_oleXmlItem;
		}
		set
		{
			m_oleXmlItem = value;
		}
	}

	internal OleObjectType OleObjectType
	{
		get
		{
			m_oleObjType = ((m_oleObjType == OleObjectType.Undefined) ? OleObject.OleType : m_oleObjType);
			if (m_oleObjType == OleObjectType.Undefined)
			{
				UpdateProps();
			}
			return m_oleObjType;
		}
		set
		{
			m_oleObjType = value;
		}
	}

	internal UpdateMode UpdateMode
	{
		get
		{
			if ((m_bFlags & 2) >> 1 == 0)
			{
				return UpdateMode.Always;
			}
			return UpdateMode.OnCall;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | (((value == UpdateMode.OnCall) ? 1u : 0u) << 1));
		}
	}

	public string ObjectType
	{
		get
		{
			if (!string.IsNullOrEmpty(m_strObjType))
			{
				return m_strObjType;
			}
			return OleTypeConvertor.ToString(OleObjectType, isWord2003: false);
		}
		set
		{
			if (!base.Document.IsOpening && OleTypeConvertor.ToOleType(value) != m_oleObjType)
			{
				if (OleObject.Storage.Streams.ContainsKey("\u0001CompObj"))
				{
					OleObject.Storage.Streams.Remove("\u0001CompObj");
				}
				MemoryStream memoryStream = new MemoryStream();
				new CompObjectStream(OleTypeConvertor.ToOleType(value)).SaveTo(memoryStream);
				memoryStream.Flush();
				memoryStream.Position = 0L;
				OleObject.Storage.Streams.Add("\u0001CompObj", memoryStream);
			}
			m_oleObjType = OleTypeConvertor.ToOleType(value);
			m_strObjType = ((m_oleObjType != 0) ? OleTypeConvertor.ToString(m_oleObjType, isWord2003: false) : value);
		}
	}

	public byte[] NativeData
	{
		get
		{
			if (!IsEmpty)
			{
				return (GetOlePartStream(isNativeData: true) as MemoryStream).ToArray();
			}
			return null;
		}
	}

	private OLEObject OleObject
	{
		get
		{
			if (m_oleObject == null)
			{
				m_oleObject = new OLEObject();
			}
			return m_oleObject;
		}
	}

	internal static int NextOleObjId
	{
		get
		{
			if (m_oleRandomIdGen == null)
			{
				m_oleRandomIdGen = new Random(default(DateTime).Millisecond);
			}
			return m_oleRandomIdGen.Next();
		}
	}

	public string PackageFileName => m_packageFileName;

	internal bool IsEmpty => OleObject.Storage.Streams.Count == 0;

	internal Guid Guid
	{
		get
		{
			if (OleObject.Guid == Guid.Empty)
			{
				return OleTypeConvertor.GetGUID(OleObjectType);
			}
			return OleObject.Guid;
		}
	}

	public WOleObject(WordDocument doc)
		: base(doc)
	{
		m_oleStorageName = string.Empty;
		m_linkAddress = string.Empty;
	}

	internal void AddFieldCodeText()
	{
		WTextRange wTextRange = new WTextRange(m_doc);
		if (LinkType == OleLinkType.Embed)
		{
			wTextRange.Text = "EMBED " + ObjectType;
		}
		else
		{
			wTextRange.Text = "LINK " + ObjectType + " \"" + LinkPath.Replace("\\", "\\\\") + "\"";
		}
		base.OwnerParagraph.Items.Add(wTextRange);
	}

	internal void SetLinkPathValue(string path)
	{
		m_linkAddress = path;
	}

	internal void ParseObjectPool(Stream objectPoolStream)
	{
		OleObject.Storage.Streams.Clear();
		using (DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile(objectPoolStream))
		{
			foreach (DirectoryEntry entry in compoundFile.Directory.Entries)
			{
				if ("_" + OleStorageName == entry.Name)
				{
					OleObject.Guid = entry.StorageGuid;
					break;
				}
			}
			if (Array.IndexOf(compoundFile.RootStorage.Storages, "ObjectPool") != -1 && Array.IndexOf(compoundFile.RootStorage.OpenStorage("ObjectPool").Storages, "_" + OleStorageName) != -1)
			{
				ICompoundStorage compoundStorage = compoundFile.RootStorage.OpenStorage("ObjectPool");
				compoundStorage = compoundStorage.OpenStorage("_" + OleStorageName);
				ParseStreams(compoundStorage);
				OleObject.Storage.ParseStorages(compoundStorage);
				CheckObjectInfoStream();
				UpdateStorageName();
			}
		}
		objectPoolStream.Position = 0L;
	}

	internal void UpdateStorageName()
	{
		if (base.Document != null)
		{
			string text = OleObject.AddOleObjectToCollection(base.Document.OleObjectCollection, OleStorageName);
			if (!string.IsNullOrEmpty(text))
			{
				m_oleStorageName = text;
			}
		}
	}

	internal void ParseOlePartStream(Stream stream)
	{
		OleObject.Storage.Streams.Clear();
		if (DocGen.CompoundFile.DocIO.Net.CompoundFile.CheckHeader(stream))
		{
			DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile(stream);
			foreach (DirectoryEntry entry in compoundFile.Directory.Entries)
			{
				if (compoundFile.RootStorage.Name == entry.Name)
				{
					OleObject.Guid = entry.StorageGuid;
					break;
				}
			}
			ParseStreams(compoundFile.RootStorage);
			OleObject.Storage.ParseStorages(compoundFile.RootStorage);
			CheckObjectInfoStream();
			UpdateStorageName();
			compoundFile.Dispose();
		}
		else
		{
			OleObject.Save(new MemoryStream((stream as MemoryStream).ToArray()), this);
			if (m_doc != null && !m_doc.OleObjectCollection.ContainsKey(OleStorageName))
			{
				m_doc.OleObjectCollection.Add(OleStorageName, OleObject.Storage);
				m_doc.OleObjectCollection[OleStorageName].Guid = Guid;
				OleObject.Guid = Guid;
			}
		}
	}

	private void ParseStreams(ICompoundStorage storage)
	{
		string[] streams = storage.Streams;
		foreach (string text in streams)
		{
			CompoundStream compoundStream = storage.OpenStream(text);
			byte[] array = new byte[compoundStream.Length];
			compoundStream.Read(array, 0, array.Length);
			compoundStream.Dispose();
			if (text == "\u0003ObjInfo")
			{
				DisplayAsIcon = (array[0] & 0x40) == 64;
				if ((array[0] & 0x10) == 16)
				{
					UpdateMode = (((array[1] & 1) == 1) ? UpdateMode.OnCall : UpdateMode.Always);
				}
			}
			if (OleObject.Storage.Streams.ContainsKey(text))
			{
				OleObject.Storage.Streams.Remove(text);
			}
			OleObject.Storage.Streams.Add(text, new MemoryStream(array));
		}
	}

	internal void ParseOleStream(Stream stream)
	{
		if (DocGen.CompoundFile.DocIO.Net.CompoundFile.CheckHeader(stream))
		{
			DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile(stream);
			string text = string.Empty;
			if (compoundFile.RootStorage.Storages.Length != 0)
			{
				text = compoundFile.RootStorage.Storages[0].Replace("_", string.Empty);
			}
			if (int.TryParse(text, out var result))
			{
				m_oleStorageName = result.ToString();
				foreach (DirectoryEntry entry in compoundFile.Directory.Entries)
				{
					if (entry.Name == "_" + text)
					{
						OleObject.Guid = entry.StorageGuid;
						break;
					}
				}
				ICompoundStorage compoundStorage = compoundFile.RootStorage.OpenStorage("_" + text);
				ParseStreams(compoundStorage);
				OleObject.Storage.ParseStorages(compoundStorage);
				CheckObjectInfoStream();
				UpdateStorageName();
				compoundStorage.Dispose();
			}
			else
			{
				m_oleStorageName = NextOleObjId.ToString();
				ParseStreams(compoundFile.RootStorage);
				OleObject.Storage.ParseStorages(compoundFile.RootStorage);
				CheckObjectInfoStream();
				UpdateStorageName();
			}
			compoundFile.Dispose();
		}
		else
		{
			m_oleStorageName = NextOleObjId.ToString();
			byte[] buffer = new byte[(int)stream.Length];
			stream.Read(buffer, 0, (int)stream.Length);
			OleObject.Save(new MemoryStream(buffer), this);
			if (m_doc != null && !m_doc.OleObjectCollection.ContainsKey(OleStorageName))
			{
				m_doc.OleObjectCollection.Add(OleStorageName, OleObject.Storage);
				m_doc.OleObjectCollection[OleStorageName].Guid = Guid;
				OleObject.Guid = Guid;
			}
		}
	}

	private void CheckObjectInfoStream()
	{
		if (!OleObject.Storage.Streams.ContainsKey("\u0003ObjInfo"))
		{
			MemoryStream value = new MemoryStream(UpdateObjInfoBytes());
			OleObject.Storage.Streams.Add("\u0003ObjInfo", value);
		}
	}

	internal void CreateOleObjContainer(byte[] nativeData, string dataPath)
	{
		dataPath = ((dataPath == null) ? string.Empty : dataPath);
		m_oleStorageName = NextOleObjId.ToString();
		m_packageFileName = dataPath;
		m_oleObject = new OLEObject();
		MemoryStream stream = new MemoryStream(nativeData);
		if (DocGen.CompoundFile.DocIO.Net.CompoundFile.CheckHeader(stream))
		{
			ParseOleStream(stream);
		}
		else
		{
			m_oleObject.Save(nativeData, dataPath, this);
			if (m_doc != null && !m_doc.OleObjectCollection.ContainsKey(OleStorageName))
			{
				m_doc.OleObjectCollection.Add(OleStorageName, OleObject.Storage);
				m_doc.OleObjectCollection[OleStorageName].Guid = Guid;
				m_oleObject.Guid = Guid;
			}
		}
		OleObjectType = m_oleObject.OleType;
		m_strObjType = OleTypeConvertor.ToString(OleObjectType, isWord2003: false);
		if (!DisplayAsIcon)
		{
			UpdateOleObjInfoStream();
		}
	}

	internal Stream GetOlePartStream(bool isNativeData)
	{
		if (IsNativeItem() && OleObject.Storage.Streams.ContainsKey("Package"))
		{
			return new MemoryStream((OleObject.Storage.Streams["Package"] as MemoryStream).ToArray());
		}
		if (OleObject.OleType == OleObjectType.AdobeAcrobatDocument && isNativeData && OleObject.Storage.Streams.ContainsKey("CONTENTS"))
		{
			return new MemoryStream((OleObject.Storage.Streams["CONTENTS"] as MemoryStream).ToArray());
		}
		DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile();
		UpdateGuid(compoundFile, 0);
		OleObject.Storage.WriteToStorage(compoundFile.RootStorage);
		if (IsNativeItem())
		{
			if (Array.IndexOf(compoundFile.RootStorage.Streams, "\u0003ObjInfo") != -1)
			{
				compoundFile.RootStorage.DeleteStream("\u0003ObjInfo");
			}
			if (Array.IndexOf(compoundFile.RootStorage.Streams, "\u0001Ole") != -1)
			{
				compoundFile.RootStorage.DeleteStream("\u0001Ole");
			}
			if (Array.IndexOf(compoundFile.RootStorage.Streams, "\u0001CompObj") != -1)
			{
				compoundFile.RootStorage.DeleteStream("\u0001CompObj");
			}
			if (Array.IndexOf(compoundFile.RootStorage.Streams, "\u0003LinkInfo") != -1)
			{
				compoundFile.RootStorage.DeleteStream("\u0003LinkInfo");
			}
			if (Array.IndexOf(compoundFile.RootStorage.Streams, "\u0003EPRINT") != -1)
			{
				compoundFile.RootStorage.DeleteStream("\u0003EPRINT");
			}
		}
		compoundFile.Flush();
		MemoryStream memoryStream = new MemoryStream();
		compoundFile.Save(memoryStream);
		compoundFile.Dispose();
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private bool IsNativeItem()
	{
		switch (OleObjectType)
		{
		case OleObjectType.Excel_97_2003_Worksheet:
		case OleObjectType.ExcelBinaryWorksheet:
		case OleObjectType.ExcelChart:
		case OleObjectType.ExcelMacroWorksheet:
		case OleObjectType.ExcelWorksheet:
		case OleObjectType.PowerPoint_97_2003_Presentation:
		case OleObjectType.PowerPointMacroPresentation:
		case OleObjectType.PowerPointMacroSlide:
		case OleObjectType.PowerPointPresentation:
		case OleObjectType.PowerPointSlide:
		case OleObjectType.Word_97_2003_Document:
		case OleObjectType.WordDocument:
		case OleObjectType.WordMacroDocument:
			return true;
		case OleObjectType.Undefined:
			if (m_strObjType == "Visio.Drawing.15")
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	internal void WriteToStorage(ICompoundStorage storage)
	{
		OleObject.Storage.WriteToStorage(storage);
	}

	internal void UpdateGuid(DocGen.CompoundFile.DocIO.Net.CompoundFile cmpFile, int index)
	{
		for (int i = index; i < cmpFile.Directory.Entries.Count; i++)
		{
			DirectoryEntry directoryEntry = cmpFile.Directory.Entries[i];
			if (directoryEntry.Name == "_" + OleStorageName || directoryEntry.Name == "Root Entry")
			{
				directoryEntry.StorageGuid = Guid;
			}
		}
	}

	private Stream GetOleContainer()
	{
		if (OleObject.Storage.Streams.Count == 0)
		{
			return null;
		}
		DocGen.CompoundFile.DocIO.Net.CompoundFile compoundFile = new DocGen.CompoundFile.DocIO.Net.CompoundFile();
		ICompoundStorage storage = compoundFile.RootStorage.CreateStorage("_" + OleStorageName);
		compoundFile.Directory.Entries[1].StorageGuid = OleObject.Guid;
		OleObject.Storage.WriteToStorage(storage);
		compoundFile.Flush();
		MemoryStream memoryStream = new MemoryStream();
		compoundFile.Save(memoryStream);
		compoundFile.Dispose();
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private void UpdateProps()
	{
		if (m_field != null)
		{
			string fieldValue = m_field.FieldValue;
			char[] separator = new char[1] { '"' };
			string[] array = fieldValue.Split(separator);
			if (m_oleObjType == OleObjectType.Undefined)
			{
				m_oleObjType = OleTypeConvertor.ToOleType(array[0].Trim());
			}
			if (string.IsNullOrEmpty(m_linkAddress) && array.Length > 1)
			{
				m_linkAddress = array[1];
			}
		}
	}

	internal void SetOlePicture(WPicture picture)
	{
		m_picture = picture;
	}

	internal void SetLinkType(OleLinkType type)
	{
		m_linkType = type;
	}

	internal void SetFieldType()
	{
		if (m_linkType == OleLinkType.Link)
		{
			m_field.FieldType = FieldType.FieldLink;
		}
		else
		{
			m_field.FieldType = FieldType.FieldEmbed;
		}
	}

	private void UpdateOleObjInfoStream()
	{
		if (m_doc.OleObjectCollection.ContainsKey(m_oleStorageName))
		{
			string oleStorageName = new Random().Next().ToString();
			OleStorageName = oleStorageName;
		}
		if (OleObject.Storage.Streams.ContainsKey("\u0003ObjInfo"))
		{
			byte[] array = UpdateObjInfoBytes();
			Stream stream = OleObject.Storage.Streams["\u0003ObjInfo"];
			stream.Position = 0L;
			stream.Write(array, 0, array.Length);
			stream.Flush();
		}
	}

	private byte[] UpdateObjInfoBytes()
	{
		byte[] result = new byte[6];
		switch (OleObjectType)
		{
		case OleObjectType.PowerPoint_97_2003_Slide:
		case OleObjectType.WordDocument:
			result = ((LinkType != 0) ? new byte[6] { 16, 0, 3, 0, 13, 0 } : new byte[6] { 0, 0, 3, 0, 1, 0 });
			break;
		case OleObjectType.Equation:
			if (LinkType == OleLinkType.Embed)
			{
				result = new byte[6] { 0, 0, 3, 0, 4, 0 };
			}
			break;
		case OleObjectType.GraphChart:
		case OleObjectType.ExcelChart:
			result = ((LinkType != 0) ? new byte[6] { 16, 0, 3, 0, 13, 0 } : new byte[6] { 0, 2, 3, 0, 13, 0 });
			break;
		case OleObjectType.AdobeAcrobatDocument:
		case OleObjectType.Excel_97_2003_Worksheet:
		case OleObjectType.ExcelBinaryWorksheet:
		case OleObjectType.ExcelMacroWorksheet:
		case OleObjectType.ExcelWorksheet:
		case OleObjectType.PowerPoint_97_2003_Presentation:
		case OleObjectType.PowerPointMacroPresentation:
		case OleObjectType.PowerPointMacroSlide:
		case OleObjectType.PowerPointPresentation:
		case OleObjectType.PowerPointSlide:
		case OleObjectType.WordMacroDocument:
		case OleObjectType.VisioDrawing:
		case OleObjectType.OpenDocumentPresentation:
		case OleObjectType.OpenDocumentSpreadsheet:
		case OleObjectType.OpenOfficeSpreadsheet1_1:
		case OleObjectType.OpenOfficeText_1_1:
		case OleObjectType.OpenOfficeSpreadsheet:
		case OleObjectType.OpenOfficeText:
			result = ((LinkType != 0) ? new byte[6] { 16, 0, 3, 0, 13, 0 } : ((!DisplayAsIcon) ? new byte[6] { 0, 0, 3, 0, 13, 0 } : new byte[6] { 64, 0, 3, 0, 4, 0 }));
			break;
		case OleObjectType.BitmapImage:
		case OleObjectType.MIDISequence:
		case OleObjectType.VideoClip:
			result = ((LinkType != 0) ? new byte[6] { 16, 0, 3, 0, 4, 0 } : new byte[6] { 0, 0, 3, 0, 4, 0 });
			break;
		case OleObjectType.MediaClip:
		case OleObjectType.Package:
		case OleObjectType.WaveSound:
			result = new byte[6] { 64, 0, 3, 0, 4, 0 };
			break;
		case OleObjectType.Undefined:
		case OleObjectType.WordPadDocument:
			result = ((LinkType != 0) ? new byte[6] { 16, 2, 3, 0, 13, 0 } : new byte[6] { 0, 0, 3, 0, 4, 0 });
			break;
		case OleObjectType.Word_97_2003_Document:
			result = ((LinkType != 0) ? new byte[6] { 16, 2, 3, 0, 13, 0 } : new byte[6] { 0, 2, 3, 0, 1, 0 });
			break;
		}
		return result;
	}

	internal bool Compare(WOleObject oleObject)
	{
		if (base.SkipDocxItem != oleObject.SkipDocxItem || DisplayAsIcon != oleObject.DisplayAsIcon || LinkType != oleObject.LinkType || OleObjectType != oleObject.OleObjectType || UpdateMode != oleObject.UpdateMode || ObjectType != oleObject.ObjectType)
		{
			return false;
		}
		if (OlePicture != null && oleObject.OlePicture != null && !OlePicture.Compare(oleObject.OlePicture))
		{
			return false;
		}
		if (NativeData != null && oleObject.NativeData != null && !Comparison.CompareBytes(NativeData, oleObject.NativeData))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0015');
		stringBuilder.Append(base.SkipDocxItem ? "1" : "0;");
		stringBuilder.Append(DisplayAsIcon ? "1" : "0;");
		stringBuilder.Append((int)LinkType + ";");
		stringBuilder.Append((int)OleObjectType + ";");
		stringBuilder.Append((int)UpdateMode + ";");
		stringBuilder.Append(ObjectType + ";");
		stringBuilder.Append(OlePicture.GetAsString()?.ToString() + ";");
		stringBuilder.Append(base.Document.Comparison.ConvertBytesAsHash(NativeData) + ";");
		stringBuilder.Append('\u0015');
		return stringBuilder;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		m_layoutInfo.IsSkip = true;
		if (Field.FieldSeparator != null)
		{
			Field.SetSkipForFieldCode(base.NextSibling);
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_picture != null)
		{
			m_picture.InitLayoutInfo(entity, ref isLastTOCEntry);
		}
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	protected override object CloneImpl()
	{
		WOleObject wOleObject = (WOleObject)base.CloneImpl();
		wOleObject.m_oleStorageName = m_oleStorageName;
		if (m_oleObject != null)
		{
			wOleObject.m_oleObject = m_oleObject.Clone();
		}
		wOleObject.m_oleObjType = OleObjectType;
		if (m_field != null)
		{
			wOleObject.Field = m_field.Clone() as WField;
		}
		if (m_oleXmlItem != null)
		{
			wOleObject.OleXmlItem = m_oleXmlItem.Clone() as XmlParagraphItem;
		}
		wOleObject.IsCloned = true;
		return wOleObject;
	}

	internal override void AddSelf()
	{
		if (base.NextSibling is WFieldMark && (base.NextSibling as WFieldMark).Type == FieldMarkType.FieldSeparator && base.NextSibling.NextSibling is WPicture)
		{
			m_picture = base.NextSibling.NextSibling as WPicture;
		}
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		base.AttachToParagraph(owner, itemPos);
		base.Document.FloatingItems.Add(this);
	}

	internal override void Detach()
	{
		if (base.Document == null)
		{
			return;
		}
		if (base.Document.OleObjectCollection.Count != 0 && base.Document.OleObjectCollection.ContainsKey(OleStorageName))
		{
			base.Document.OleObjectCollection[OleStorageName].OccurrenceCount--;
			if (base.Document.OleObjectCollection[OleStorageName].OccurrenceCount <= 0)
			{
				base.Document.OleObjectCollection.Remove(OleStorageName);
			}
		}
		base.Document.FloatingItems.Remove(this);
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		OleObject.Cloned = true;
		if (doc != null)
		{
			string text = OleObject.AddOleObjectToCollection(doc.OleObjectCollection, OleStorageName);
			if (!string.IsNullOrEmpty(text))
			{
				m_oleStorageName = text;
			}
		}
		UpdateOleItemReferences();
		base.IsCloned = false;
		OleObject.Cloned = false;
	}

	private void UpdateOleItemReferences()
	{
		if (base.OwnerParagraph != null)
		{
			if (Field.FieldSeparator != null && IsValidIndex(Field.FieldSeparator.Index))
			{
				Field.FieldSeparator = base.OwnerParagraph.Items[Field.FieldSeparator.Index] as WFieldMark;
			}
			if (m_picture != null && IsValidIndex(m_picture.Index) && base.OwnerParagraph.Items[m_picture.Index] is WPicture)
			{
				m_picture = base.OwnerParagraph.Items[m_picture.Index] as WPicture;
			}
			if (Field.FieldEnd != null && IsValidIndex(Field.FieldEnd.Index))
			{
				Field.FieldEnd = base.OwnerParagraph.Items[Field.FieldEnd.Index] as WFieldMark;
			}
		}
	}

	private bool IsValidIndex(int index)
	{
		if (index >= 0)
		{
			return index < base.OwnerParagraph.Items.Count;
		}
		return false;
	}

	internal override void Close()
	{
		if (m_picture != null)
		{
			m_picture.Close();
			m_picture = null;
		}
		if (m_oleObject != null)
		{
			m_oleObject.Close();
			m_oleObject = null;
		}
		m_field = null;
		if (m_oleXmlItem != null)
		{
			m_oleXmlItem.Close();
			m_oleXmlItem = null;
		}
		base.Close();
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return SizeF.Empty;
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}
}
