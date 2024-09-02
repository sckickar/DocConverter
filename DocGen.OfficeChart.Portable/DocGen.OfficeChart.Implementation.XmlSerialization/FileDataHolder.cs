using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DocGen.Compression;
using DocGen.Compression.Zip;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class FileDataHolder : IWorkbookSerializator, IDisposable
{
	private const string ContentTypesItemName = "[Content_Types].xml";

	internal const string RelationsDirectory = "_rels";

	internal const string RelationExtension = ".rels";

	private const string TopRelationsPath = "_rels/.rels";

	private const string XmlExtension = "xml";

	private const string RelsExtension = "rels";

	public const string BinaryExtension = "bin";

	private const string WorkbookPartName = "xl/workbook.xml";

	private const string CustomXmlPartName = "customXml/item{0}.xml";

	private const string SSTPartName = "/xl/sharedStrings.xml";

	private const string StylesPartName = "xl/styles.xml";

	private const string ThemesPartName = "xl/theme/theme1.xml";

	private const string DefaultWorksheetPathFormat = "xl/worksheets/sheet{0}.xml";

	private const string DefaultChartsheetPathFormat = "xl/chartsheets/sheet{0}.xml";

	public const string DefaultPicturePathFormat = "xl/media/image{0}.";

	public const string ExtendedPropertiesPartName = "docProps/app.xml";

	public const string CorePropertiesPartName = "docProps/core.xml";

	public const string CustomPropertiesPartName = "docProps/custom.xml";

	private const string RelationIdFormat = "rId{0}";

	public const string ExternLinksPathFormat = "xl/externalLinks/externalLink{0}.xml";

	private const string ExtenalLinksPathStart = "xl/externalLinks/externalLink";

	public const string CustomPropertyPathStart = "xl/customProperty";

	public const string PivotCacheDefinitionPathFormat = "xl/pivotCache/pivotCacheDefinition{0}.xml";

	public const string PivotCacheRecordsPathFormat = "xl/pivotCache/pivotCacheRecords{0}.xml";

	public const string PivotTablePathFormat = "xl/pivotTables/pivotTable{0}.xml";

	private const string TablePathFormat = "xl/tables/table{0}.xml";

	private const string ConnectionPathFormat = "xl/connections.xml";

	private const string QueryTablePathFormat = "/xl/queryTables/queryTable{0}.xml";

	private Dictionary<string, MemoryStream> m_metafileStream = new Dictionary<string, MemoryStream>();

	private ZipArchive m_archive = new ZipArchive();

	private WorkbookImpl m_book;

	private Excel2007Parser m_parser;

	private IDictionary<string, string> m_dicDefaultTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private IDictionary<string, string> m_dicOverriddenTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private RelationCollection m_topRelations;

	private string m_strWorkbookPartName = "xl/workbook.xml";

	private string m_strSSTPartName = "/xl/sharedStrings.xml";

	private string m_strStylesPartName = "xl/styles.xml";

	private string m_connectionPartName = "xl/connections.xml";

	private string m_queryTablePartName = "/xl/queryTables/queryTable{0}.xml";

	private string m_strThemesPartName = "xl/theme/theme1.xml";

	private Excel2007Serializator m_serializator;

	private List<int> m_arrCellFormats;

	private RelationCollection m_workbookRelations;

	private string m_strStylesRelationId;

	private string m_strSSTRelationId;

	private string m_strThemeRelationId;

	private string m_strWorkbookContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml";

	private Stream m_streamEnd = new MemoryStream();

	private Stream m_streamStart = new MemoryStream();

	private Stream m_streamDxfs;

	private int m_iCommentIndex;

	private int m_iVmlIndex;

	private int m_iDrawingIndex;

	private int m_iImageIndex;

	private int m_iImageId;

	private int m_iLastChartIndex;

	private int m_iLastPivotCacheIndex;

	private int m_iLastPivotCacheRecordsIndex;

	private int m_iExternLinkIndex;

	private string[] m_arrImageItemNames;

	private List<DxfImpl> m_lstParsedDxfs;

	private List<Dictionary<string, string>> m_lstBookViews;

	private Dictionary<string, object> m_dictItemsToRemove = new Dictionary<string, object>();

	private Stream m_functionGroups;

	private FileVersion m_fileVersion = new FileVersion();

	private string m_strCalculationId = "125725";

	private Dictionary<string, string> m_preservedCaches = new Dictionary<string, string>();

	private Stream m_extensions;

	private string m_strConnectionId;

	private int m_queryTableCount = 1;

	private int m_iLastChartExIndex;

	internal int LastChartExIndex
	{
		get
		{
			return m_iLastChartExIndex;
		}
		set
		{
			m_iLastChartExIndex = value;
		}
	}

	public WorkbookImpl Workbook => m_book;

	public Excel2007Parser Parser
	{
		get
		{
			if (m_parser == null)
			{
				m_parser = new Excel2007Parser(m_book);
			}
			return m_parser;
		}
	}

	public ZipArchiveItem this[Relation relation, string parentPath]
	{
		get
		{
			if (relation == null)
			{
				return null;
			}
			return m_archive[CombinePath(parentPath, relation.Target)];
		}
	}

	public Excel2007Serializator Serializator
	{
		get
		{
			if (m_serializator == null || m_serializator.Version != m_book.Version)
			{
				switch (m_book.Version)
				{
				case OfficeVersion.Excel2007:
					m_serializator = new Excel2007Serializator(m_book);
					break;
				case OfficeVersion.Excel2010:
					m_serializator = new Excel2010Serializator(m_book);
					break;
				case OfficeVersion.Excel2013:
					m_serializator = new Excel2013Serializator(m_book);
					break;
				default:
					throw new NotImplementedException();
				}
			}
			return m_serializator;
		}
	}

	public List<int> XFIndexes => m_arrCellFormats;

	public ZipArchive Archive => m_archive;

	public int LastCommentIndex
	{
		get
		{
			return m_iCommentIndex;
		}
		set
		{
			m_iCommentIndex = value;
		}
	}

	public int LastVmlIndex
	{
		get
		{
			return m_iVmlIndex;
		}
		set
		{
			m_iVmlIndex = value;
		}
	}

	public int LastDrawingIndex
	{
		get
		{
			return m_iDrawingIndex;
		}
		set
		{
			m_iDrawingIndex = value;
		}
	}

	public int LastImageIndex
	{
		get
		{
			return m_iImageIndex;
		}
		set
		{
			m_iImageIndex = value;
		}
	}

	public int LastImageId
	{
		get
		{
			return m_iImageId;
		}
		set
		{
			m_iImageId = value;
		}
	}

	public int LastChartIndex
	{
		get
		{
			return m_iLastChartIndex;
		}
		set
		{
			m_iLastChartIndex = value;
		}
	}

	internal int LastPivotCacheIndex
	{
		get
		{
			return m_iLastPivotCacheIndex;
		}
		set
		{
			m_iLastPivotCacheIndex = value;
		}
	}

	internal int LastPivotCacheRecordsIndex
	{
		get
		{
			return m_iLastPivotCacheRecordsIndex;
		}
		set
		{
			m_iLastPivotCacheRecordsIndex = value;
		}
	}

	public IDictionary<string, string> DefaultContentTypes => m_dicDefaultTypes;

	public IDictionary<string, string> OverriddenContentTypes => m_dicOverriddenTypes;

	public int ParsedDxfsCount
	{
		get
		{
			if (m_lstParsedDxfs == null)
			{
				return int.MinValue;
			}
			return m_lstParsedDxfs.Count;
		}
	}

	public Dictionary<string, object> ItemsToRemove => m_dictItemsToRemove;

	public string CalculationId
	{
		get
		{
			return m_strCalculationId;
		}
		set
		{
			m_strCalculationId = value;
		}
	}

	public FileVersion FileVersion => m_fileVersion;

	public Dictionary<string, string> PreservedCaches => m_preservedCaches;

	public Stream ExtensionStream
	{
		get
		{
			return m_extensions;
		}
		set
		{
			m_extensions = value;
		}
	}

	private FileDataHolder()
	{
	}

	public FileDataHolder(WorkbookImpl book)
	{
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		m_book = book;
	}

	private void RerequestPassword(ref string password, ApplicationImpl excel)
	{
		PasswordRequiredEventArgs passwordRequiredEventArgs = new PasswordRequiredEventArgs();
		if (excel.RaiseOnWrongPassword(this, passwordRequiredEventArgs))
		{
			password = passwordRequiredEventArgs.NewPassword;
		}
		else
		{
			passwordRequiredEventArgs = null;
		}
		if (password == null || passwordRequiredEventArgs == null || passwordRequiredEventArgs.StopParsing)
		{
			throw new ArgumentException("Workbook is protected and password wasn't specified.");
		}
	}

	private void RequestPassword(ref string password, ApplicationImpl excel)
	{
		PasswordRequiredEventArgs passwordRequiredEventArgs = new PasswordRequiredEventArgs();
		if (password == null)
		{
			if (excel.RaiseOnPasswordRequired(this, passwordRequiredEventArgs))
			{
				password = passwordRequiredEventArgs.NewPassword;
			}
			else
			{
				passwordRequiredEventArgs = null;
			}
		}
		if (password == null || passwordRequiredEventArgs == null || passwordRequiredEventArgs.StopParsing)
		{
			throw new ArgumentException("Workbook is protected and password wasn't specified.");
		}
	}

	public void AddOverriddenContentType(string fileName, string contentType)
	{
		if (fileName == null || fileName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("fileName");
		}
		if (fileName[0] != '/')
		{
			fileName = "/" + fileName;
		}
		OverriddenContentTypes[fileName] = contentType;
	}

	public List<DxfImpl> ParseDxfsCollection()
	{
		List<DxfImpl> list = null;
		if (m_streamDxfs == null)
		{
			return m_lstParsedDxfs;
		}
		if (m_streamDxfs.Length == 0L)
		{
			return m_lstParsedDxfs;
		}
		if (m_lstParsedDxfs == null)
		{
			m_streamDxfs.Position = 0L;
			XmlReader xmlReader = UtilityMethods.CreateReader(m_streamDxfs);
			if (xmlReader.LocalName != "dxfs")
			{
				xmlReader.Read();
			}
			list = Parser.ParseDxfCollection(xmlReader);
			m_streamDxfs.Flush();
			m_lstParsedDxfs = list;
		}
		else
		{
			list = m_lstParsedDxfs;
		}
		return list;
	}

	public WorksheetDataHolder GetSheetData(string sheetPath)
	{
		if (sheetPath == null || sheetPath.Length == 0)
		{
			throw new ArgumentOutOfRangeException("sheetPath");
		}
		throw new NotImplementedException();
	}

	public WorksheetBaseImpl GetSheet(string sheetName)
	{
		if (sheetName == null || sheetName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("sheetName");
		}
		return m_book.Objects[sheetName] as WorksheetBaseImpl;
	}

	public void ParseDocument(ref List<Color> themeColors, bool parseOnDemand)
	{
		m_book.IsWorkbookOpening = true;
		bool throwOnUnknownNames = m_book.ThrowOnUnknownNames;
		m_book.ThrowOnUnknownNames = false;
		ParseContentType();
		m_topRelations = ParseRelations("_rels/.rels");
		m_strWorkbookPartName = FindItemByContent("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
		m_dictItemsToRemove.Add("_rels/.rels", null);
		if (m_strWorkbookPartName == null && (FindWorkbookPartName("application/vnd.ms-excel.sheet.macroEnabled.main+xml") || FindWorkbookPartName("application/vnd.ms-excel.template.macroEnabled.main+xml")))
		{
			m_book.HasMacros = true;
		}
		if (m_strWorkbookPartName == null)
		{
			FindWorkbookPartName("application/vnd.openxmlformats-officedocument.spreadsheetml.template.main+xml");
		}
		if (m_strWorkbookPartName == null)
		{
			throw new NotSupportedException("File cannot be opened - format is not supported.");
		}
		if (m_strWorkbookPartName[0] == '/')
		{
			m_strWorkbookPartName = UtilityMethods.RemoveFirstCharUnsafe(m_strWorkbookPartName);
		}
		ParseDocumentProperties();
		ParseWorkbook(ref themeColors, parseOnDemand);
		foreach (string key in m_dictItemsToRemove.Keys)
		{
			m_archive.RemoveItem(key);
		}
		m_dictItemsToRemove.Clear();
		m_book.ThrowOnUnknownNames = throwOnUnknownNames;
		m_book.IsWorkbookOpening = false;
	}

	private bool FindWorkbookPartName(string strContentType)
	{
		m_strWorkbookContentType = strContentType;
		m_strWorkbookPartName = FindItemByContent(strContentType);
		return m_strWorkbookPartName != null;
	}

	internal OfficeSaveType GetWorkbookPartType()
	{
		OfficeSaveType result = OfficeSaveType.SaveAsXLS;
		if (FindWorkbookPartName("application/vnd.ms-excel.template.macroEnabled.main+xml") || FindWorkbookPartName("application/vnd.openxmlformats-officedocument.spreadsheetml.template.main+xml"))
		{
			result = OfficeSaveType.SaveAsTemplate;
		}
		else if (FindWorkbookPartName("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml") || FindWorkbookPartName("application/vnd.ms-excel.sheet.macroEnabled.main+xml"))
		{
			result = OfficeSaveType.SaveAsXLS;
		}
		if (m_strWorkbookPartName[0] == '/')
		{
			m_strWorkbookPartName = UtilityMethods.RemoveFirstCharUnsafe(m_strWorkbookPartName);
		}
		return result;
	}

	public void ParseContentType()
	{
		m_dicDefaultTypes.Clear();
		m_dicOverriddenTypes.Clear();
		XmlReader reader = UtilityMethods.CreateReader((m_archive["[Content_Types].xml"] ?? throw new NotSupportedException("File cannot be opened - format is not supported")).DataStream);
		Parser.ParseContentTypes(reader, m_dicDefaultTypes, m_dicOverriddenTypes);
		string key = "/xl/workbook.xml";
		string value = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml";
		if (!m_dicOverriddenTypes.ContainsKey(key))
		{
			m_dicOverriddenTypes.Add(key, value);
		}
		m_dictItemsToRemove.Add("[Content_Types].xml", null);
	}

	public void ParseDocumentProperties()
	{
		ParseArchiveItemByContentType("application/vnd.openxmlformats-package.core-properties+xml");
		ParseArchiveItemByContentType("application/vnd.openxmlformats-officedocument.extended-properties+xml");
		ParseArchiveItemByContentType("application/vnd.openxmlformats-officedocument.custom-properties+xml");
	}

	public void ParseArchiveItemByContentType(string strContentType)
	{
		if (GetXmlReaderByContentType(strContentType, out var strItemName) != null)
		{
			switch (strContentType)
			{
			default:
				throw new ArgumentException("strContentType");
			case "application/vnd.openxmlformats-package.core-properties+xml":
			case "application/vnd.openxmlformats-officedocument.extended-properties+xml":
			case "application/vnd.openxmlformats-officedocument.custom-properties+xml":
				m_archive.RemoveItem(strItemName);
				break;
			}
		}
	}

	public XmlReader GetXmlReaderByContentType(string strContentType, out string strItemName)
	{
		string text = FindItemByContent(strContentType);
		if (text == null)
		{
			strItemName = string.Empty;
			return null;
		}
		m_dicOverriddenTypes.Remove(text);
		if (text.StartsWith("/"))
		{
			text = UtilityMethods.RemoveFirstCharUnsafe(text);
		}
		ZipArchiveItem zipArchiveItem = m_archive[text];
		strItemName = zipArchiveItem.ItemName;
		Stream dataStream = zipArchiveItem.DataStream;
		dataStream.Position = 0L;
		return UtilityMethods.CreateReader(dataStream);
	}

	public void SaveDocument(OfficeSaveType saveType)
	{
		m_iVmlIndex = 0;
		m_iCommentIndex = 0;
		m_iDrawingIndex = 0;
		m_iImageIndex = 0;
		m_iImageId = 0;
		m_iExternLinkIndex = 0;
		m_iLastChartIndex = 0;
		m_iLastPivotCacheIndex = 0;
		m_iLastPivotCacheRecordsIndex = 0;
		m_book.LastPivotTableIndex = 0;
		SaveWorkbook(saveType);
		SaveDocumentProperties();
		SaveContentTypes();
		SaveTopLevelRelations();
		m_iLastChartExIndex = 0;
	}

	public string RegisterContentTypes(ImageFormat imageFormat)
	{
		string strExtension;
		string pictureContentType = GetPictureContentType(imageFormat, out strExtension);
		m_dicDefaultTypes[strExtension] = pictureContentType;
		return strExtension;
	}

	public static string GetPictureContentType(ImageFormat format, out string strExtension)
	{
		string result;
		if (format.Equals(ImageFormat.Bmp))
		{
			result = "image/bmp";
			strExtension = "bmp";
		}
		else if (format.Equals(ImageFormat.Jpeg))
		{
			result = "image/jpeg";
			strExtension = "jpeg";
		}
		else if (format.Equals(ImageFormat.Png))
		{
			result = "image/png";
			strExtension = "png";
		}
		else if (format.Equals(ImageFormat.Emf))
		{
			result = "image/x-emf";
			strExtension = "emf";
		}
		else if (format.Equals(ImageFormat.Gif))
		{
			result = "image/gif";
			strExtension = "gif";
		}
		else
		{
			result = "image/png";
			strExtension = "png";
		}
		return result;
	}

	public string SaveImage(Image image, string proposedPath)
	{
		return SaveImage(image, image.RawFormat, proposedPath);
	}

	public string SaveImage(Image image, ImageFormat imageFormat, string proposedPath)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		string extension = GetExtension(imageFormat);
		RegisterContentTypes(imageFormat);
		string text = null;
		if (proposedPath == null)
		{
			Regex itemRegex;
			do
			{
				m_iImageIndex++;
				text = $"xl/media/image{m_iImageIndex}.";
				itemRegex = new Regex(text);
			}
			while (m_archive.Find(itemRegex) != -1);
			text += extension;
		}
		else if (m_archive.Find(new Regex(proposedPath.Split(new char[1] { '.' })[0])) != -1)
		{
			m_iImageIndex++;
			text = $"xl/media/image{m_iImageIndex}.";
			text += extension;
		}
		else
		{
			m_iImageIndex++;
			text = proposedPath;
		}
		imageFormat = GetImageFormat(extension);
		ImageFormat rawFormat = image.RawFormat;
		MemoryStream memoryStream2;
		if (rawFormat.Equals(ImageFormat.Emf) || rawFormat.Equals(ImageFormat.Wmf))
		{
			MemoryStream memoryStream = new MemoryStream();
			memoryStream2 = MsoMetafilePicture.SerializeMetafile(image);
			if (m_metafileStream.ContainsKey(text))
			{
				memoryStream = m_metafileStream[text];
				if (memoryStream2.Length != memoryStream.Length)
				{
					memoryStream2 = memoryStream;
				}
			}
		}
		else
		{
			memoryStream2 = new MemoryStream();
			image.Save(memoryStream2, imageFormat);
		}
		m_archive.UpdateItem(text, memoryStream2, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		return text;
	}

	private ImageFormat GetImageFormat(string extension)
	{
		if (extension == null)
		{
			throw new ArgumentNullException("format");
		}
		return extension switch
		{
			"bmp" => ImageFormat.Bmp, 
			"jpeg" => ImageFormat.Jpeg, 
			"png" => ImageFormat.Png, 
			"emf" => ImageFormat.Emf, 
			"icon" => ImageFormat.Jpeg, 
			"wmf" => ImageFormat.Wmf, 
			"gif" => ImageFormat.Gif, 
			_ => ImageFormat.Png, 
		};
	}

	public string GetImageItemName(int i)
	{
		return m_arrImageItemNames[i];
	}

	public string PrepareNewItem(string itemNameStart, string extension, string contentType, RelationCollection relations, string relationType, ref int itemsCounter, out ZipArchiveItem item)
	{
		m_dicDefaultTypes[extension] = contentType;
		string text = GenerateItemName(ref itemsCounter, itemNameStart, extension);
		item = m_archive.AddItem(text, new MemoryStream(), bControlStream: true, DocGen.Compression.FileAttributes.Archive);
		string text2 = relations.GenerateRelationId();
		Relation value = new Relation("/" + text, relationType);
		relations[text2] = value;
		return text2;
	}

	private string GetExtension(ImageFormat format)
	{
		if (format.Equals(ImageFormat.Bmp))
		{
			return "bmp";
		}
		if (format.Equals(ImageFormat.Wmf))
		{
			return "wmf";
		}
		if (format.Equals(ImageFormat.Icon))
		{
			return "icon";
		}
		if (format.Equals(ImageFormat.Jpeg))
		{
			return "jpeg";
		}
		if (format.Equals(ImageFormat.Png))
		{
			return "png";
		}
		if (format.Equals(ImageFormat.Emf))
		{
			return "emf";
		}
		if (format.Equals(ImageFormat.Gif))
		{
			return "gif";
		}
		return "png";
	}

	private void ParseWorkbook(ref List<Color> themeColors, bool parseOnDemand)
	{
		string text = m_strWorkbookPartName;
		if (text == null || text.Length == 0)
		{
			throw new ArgumentOutOfRangeException("workbookItemName");
		}
		if (text[0] == '/')
		{
			text = UtilityMethods.RemoveFirstCharUnsafe(text);
		}
		ZipArchiveItem item = m_archive[text] ?? throw new XmlException("Cannot locate workbook item: " + text);
		string correspondingRelations = GetCorrespondingRelations(text);
		m_workbookRelations = ParseRelations(correspondingRelations);
		m_dictItemsToRemove.Add(correspondingRelations, null);
		Relation relation = m_workbookRelations.FindRelationByContentType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", out m_strStylesRelationId);
		if (relation == null)
		{
			Workbook.InsertDefaultFonts();
			Workbook.InsertDefaultValues();
		}
		Relation relation2 = m_workbookRelations.FindRelationByContentType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings", out m_strSSTRelationId);
		Relation relation3 = m_workbookRelations.FindRelationByContentType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme", out m_strThemeRelationId);
		SeparateItemName(text, out var path);
		MemoryStream memoryStream = new MemoryStream();
		XmlReader reader = CreateReader(item);
		Parser.ParseWorkbook(reader, m_workbookRelations, this, path, m_streamStart, m_streamEnd, ref m_lstBookViews, memoryStream);
		int num = m_book.TabSheets.Count + 4;
		if (relation3 != null)
		{
			m_strThemesPartName = path + relation3.Target;
			reader = CreateReader(relation3, path);
			themeColors = Parser.ParseThemes(reader);
			if (themeColors != null)
			{
				Workbook.m_isThemeColorsParsed = true;
			}
		}
		int num2 = 1;
		ITabSheets objects = m_book.Objects;
		ApplicationImpl appImplementation = m_book.AppImplementation;
		appImplementation.RaiseProgressEvent(num2, objects.Count + 4);
		if (relation != null)
		{
			m_strStylesPartName = path + relation.Target;
			reader = CreateReader(relation, path);
			m_arrCellFormats = Parser.ParseStyles(reader, ref m_streamDxfs);
			m_dictItemsToRemove.Add(m_strStylesPartName, null);
		}
		Relation relation4 = m_workbookRelations.FindRelationByContentType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/connections", out m_strConnectionId);
		if (relation4 != null)
		{
			m_connectionPartName = path + relation4.Target;
			reader = CreateReader(relation4, path);
			m_dictItemsToRemove.Add(m_connectionPartName, null);
			m_workbookRelations.RemoveByContentType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/connections");
		}
		num2++;
		appImplementation.RaiseProgressEvent(num2, num);
		Dictionary<int, int> dictUpdatedSSTIndexes = null;
		if (relation2 != null)
		{
			m_strSSTPartName = ((relation2.Target[0] != '/') ? (path + relation2.Target) : relation2.Target);
			string strItemPath;
			ZipArchiveItem item2 = GetItem(relation2, path, out strItemPath);
			if (item2 != null)
			{
				reader = CreateReader(item2);
				dictUpdatedSSTIndexes = Parser.ParseSST(reader, parseOnDemand);
				m_dictItemsToRemove.Add(m_strSSTPartName, null);
			}
		}
		num2++;
		appImplementation.RaiseProgressEvent(num2, num);
		if (memoryStream.Length != 0L)
		{
			m_functionGroups = memoryStream;
		}
		else
		{
			memoryStream.Dispose();
		}
		Parser.ParseWorksheets(dictUpdatedSSTIndexes, parseOnDemand);
		if (!Workbook.ParseOnDemand)
		{
			Parser.ParsePivotTables();
		}
		RemoveCalcChain();
	}

	private void ParseCustomXmlParts()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		foreach (KeyValuePair<string, Relation> workbookRelation in m_workbookRelations)
		{
			if (workbookRelation.Value.Type == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/customXml" && !list.Contains(workbookRelation.Key))
			{
				list.Add(workbookRelation.Key);
				list3.Add(workbookRelation.Value.Target);
			}
		}
		foreach (string item in list)
		{
			m_workbookRelations.Remove(item);
		}
		foreach (KeyValuePair<string, string> dicOverriddenType in m_dicOverriddenTypes)
		{
			if (dicOverriddenType.Value == "application/vnd.openxmlformats-officedocument.customXmlProperties+xml" && !list2.Contains(dicOverriddenType.Key))
			{
				list2.Add(dicOverriddenType.Key);
			}
		}
		foreach (string item2 in list2)
		{
			m_dicOverriddenTypes.Remove(item2);
		}
		for (int i = 0; i < list3.Count; i++)
		{
			string text = list3[i];
			string text2 = list2[i];
			if (text[0] == '.')
			{
				text = text.Substring(3, text.Length - 3);
			}
			if (text2.StartsWith("/"))
			{
				text2 = UtilityMethods.RemoveFirstCharUnsafe(text2);
			}
			string key = $"{text.Substring(0, text.IndexOf('/'))}/_rels/item{i + 1}.xml.rels";
			m_dictItemsToRemove.Add(key, null);
		}
	}

	internal RelationCollection ParseRelations(string itemPath)
	{
		RelationCollection relationCollection = null;
		ZipArchiveItem zipArchiveItem = m_archive[itemPath];
		if (zipArchiveItem != null)
		{
			XmlReader reader = CreateReader(zipArchiveItem);
			relationCollection = Parser.ParseRelations(reader);
			relationCollection.ItemPath = itemPath;
		}
		return relationCollection;
	}

	private string FindItemByContent(string contentType)
	{
		string text = FindItemByContentInOverride(contentType);
		if (text == null)
		{
			text = FindItemByContentInDefault(contentType);
		}
		return text;
	}

	private string FindItemByContentInDefault(string contentType)
	{
		string result = null;
		foreach (KeyValuePair<string, string> dicDefaultType in m_dicDefaultTypes)
		{
			if (!(dicDefaultType.Value == contentType))
			{
				continue;
			}
			string key = dicDefaultType.Key;
			int i = 0;
			for (int count = m_archive.Count; i < count; i++)
			{
				string text = m_archive[i].ItemName;
				if (text[0] != '/')
				{
					text = "/" + text;
				}
				if (text.EndsWith(key) && !m_dicOverriddenTypes.ContainsKey(text))
				{
					result = text;
					break;
				}
			}
			break;
		}
		return result;
	}

	private string FindItemByContentInOverride(string contentType)
	{
		string result = null;
		foreach (KeyValuePair<string, string> dicOverriddenType in m_dicOverriddenTypes)
		{
			if (dicOverriddenType.Value == contentType)
			{
				result = dicOverriddenType.Key;
				break;
			}
		}
		return result;
	}

	internal static string GetCorrespondingRelations(string itemName)
	{
		if (itemName == null || itemName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("itemName");
		}
		string path;
		string text = SeparateItemName(itemName, out path);
		return path + "_rels/" + text + ".rels";
	}

	internal static string SeparateItemName(string itemName, out string path)
	{
		int num = itemName.LastIndexOf('/');
		path = ((num >= 0) ? itemName.Substring(0, num + 1) : string.Empty);
		return itemName.Substring(num + 1);
	}

	internal Image GetImage(string strFullPath)
	{
		if (strFullPath == null || strFullPath.Length == 0)
		{
			throw new ArgumentOutOfRangeException("strFullPath");
		}
		Image image = null;
		ZipArchiveItem zipArchiveItem = m_archive[strFullPath];
		if (zipArchiveItem != null)
		{
			MemoryStream obj = (MemoryStream)zipArchiveItem.DataStream;
			MemoryStream memoryStream = new MemoryStream((int)obj.Length);
			obj.WriteTo(memoryStream);
			memoryStream.Position = 0L;
			image = ApplicationImpl.CreateImage(memoryStream);
			m_dictItemsToRemove[strFullPath] = null;
			if ((image.RawFormat.Equals(ImageFormat.Emf) || image.RawFormat.Equals(ImageFormat.Wmf)) && !m_metafileStream.ContainsKey(strFullPath))
			{
				m_metafileStream.Add(strFullPath, memoryStream);
			}
		}
		return image;
	}

	private static XmlReader CreateReader(ZipArchiveItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		Stream dataStream = item.DataStream;
		if (dataStream.CanSeek)
		{
			dataStream.Position = 0L;
		}
		return UtilityMethods.CreateReader(dataStream);
	}

	internal XmlReader CreateReader(Relation relation, string parentItemPath)
	{
		string strItemPath;
		return CreateReader(relation, parentItemPath, out strItemPath);
	}

	public XmlReader CreateReaderAndFixBr(Relation relation, string parentItemPath, out string strItemPath)
	{
		string text = new StreamReader(GetItem(relation, parentItemPath, out strItemPath).DataStream).ReadToEnd();
		text = text.Replace("<br></br>", "<br/>");
		text = text.Replace("<br>", "<br/>");
		return UtilityMethods.CreateReader(new MemoryStream(Encoding.UTF8.GetBytes(text)));
	}

	internal XmlReader CreateReader(Relation relation, string parentItemPath, out string strItemPath)
	{
		return CreateReader(GetItem(relation, parentItemPath, out strItemPath));
	}

	internal ZipArchiveItem GetItem(Relation relation, string parentItemPath, out string strItemPath)
	{
		if (relation == null)
		{
			throw new ArgumentNullException("relation");
		}
		string text = relation.Target;
		if (parentItemPath != null)
		{
			text = CombinePath(parentItemPath, text);
			text.Replace('\\', '/');
		}
		ZipArchiveItem result = m_archive[text];
		strItemPath = text;
		return result;
	}

	internal byte[] GetData(Relation relation, string parentItemPath, bool removeItem)
	{
		if (relation == null)
		{
			throw new ArgumentNullException("relation");
		}
		string text = relation.Target;
		if (parentItemPath != null)
		{
			text = CombinePath(parentItemPath, text);
			text.Replace('\\', '/');
		}
		Stream dataStream = m_archive[text].DataStream;
		byte[] array = new byte[dataStream.Length];
		dataStream.Position = 0L;
		dataStream.Read(array, 0, (int)dataStream.Length);
		if (removeItem)
		{
			m_archive.RemoveItem(text);
		}
		return array;
	}

	internal void ParseExternalLink(string relationId)
	{
		Relation relation = m_workbookRelations[relationId];
		SeparateItemName(m_strWorkbookPartName, out var path);
		string strItemPath;
		XmlReader reader = CreateReader(relation, path, out strItemPath);
		string correspondingRelations = GetCorrespondingRelations(strItemPath);
		RelationCollection relations = ParseRelations(correspondingRelations);
		if (Parser.ParseExternalLink(reader, relations))
		{
			m_dictItemsToRemove.Add(strItemPath, null);
			m_workbookRelations.Remove(relationId);
		}
		else
		{
			m_book.PreservedExternalLinks.Add(relationId);
		}
	}

	internal static string CombinePath(string startPath, string endPath)
	{
		if (startPath == null)
		{
			throw new ArgumentOutOfRangeException("startPath");
		}
		if (endPath == null || endPath.Length == 0)
		{
			throw new ArgumentOutOfRangeException("endPath");
		}
		if (startPath.Length > 0 && startPath[startPath.Length - 1] == '/')
		{
			startPath = startPath.Substring(0, startPath.Length - 1);
		}
		while (endPath.StartsWith("../"))
		{
			int num = startPath.LastIndexOf('/');
			if (num < 0)
			{
				break;
			}
			endPath = endPath.Substring(3, endPath.Length - 3);
			startPath = startPath.Substring(0, num);
		}
		if (!endPath.StartsWith("/"))
		{
			if (!(startPath != ""))
			{
				return endPath;
			}
			return startPath + "/" + endPath;
		}
		return UtilityMethods.RemoveFirstCharUnsafe(endPath);
	}

	private void SaveContentTypes()
	{
		FillDefaultContentTypes();
		SaveArchiveItem("[Content_Types].xml");
	}

	private void SaveDocumentProperties()
	{
		SaveArchiveItemRelationContentType("docProps/app.xml", "application/vnd.openxmlformats-officedocument.extended-properties+xml", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties");
		SaveArchiveItemRelationContentType("docProps/core.xml", "application/vnd.openxmlformats-package.core-properties+xml", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties");
		SaveArchiveItemRelationContentType("docProps/custom.xml", "application/vnd.openxmlformats-officedocument.custom-properties+xml", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/custom-properties");
	}

	private void SaveContentTypeProperties()
	{
	}

	private void SaveArchiveItemRelationContentType(string partName, string contentType, string relationType)
	{
		m_dicOverriddenTypes["/" + partName] = contentType;
		m_topRelations.FindRelationByContentType(relationType, out var relationId);
		if (relationId == null)
		{
			relationId = m_topRelations.GenerateRelationId();
		}
		m_topRelations[relationId] = new Relation(partName, relationType);
		SaveArchiveItem(partName);
	}

	private void SaveArchiveItem(string strItemPartName)
	{
		MemoryStream memoryStream = new MemoryStream();
		UtilityMethods.CreateWriter(new StreamWriter(memoryStream)).Flush();
		ZipArchiveItem zipArchiveItem = m_archive[strItemPartName];
		if (zipArchiveItem != null)
		{
			zipArchiveItem.Update(memoryStream, controlStream: true);
		}
		else
		{
			m_archive.AddItem(strItemPartName, memoryStream, bControlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
	}

	private void FillDefaultContentTypes()
	{
		m_dicDefaultTypes["xml"] = "application/xml";
		m_dicDefaultTypes["rels"] = "application/vnd.openxmlformats-package.relationships+xml";
	}

	private void SaveTopLevelRelations()
	{
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(new StreamWriter(memoryStream));
		Serializator.SerializeRelations(xmlWriter, m_topRelations, null);
		xmlWriter.Flush();
		ZipArchiveItem zipArchiveItem = m_archive["_rels/.rels"];
		if (zipArchiveItem != null)
		{
			zipArchiveItem.Update(memoryStream, controlStream: true);
		}
		else
		{
			m_archive.AddItem("_rels/.rels", memoryStream, bControlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
	}

	private static string GetRelationId(int relationIndex)
	{
		return $"rId{relationIndex}";
	}

	private void SaveWorkbook(OfficeSaveType saveAsType)
	{
		m_strWorkbookContentType = SelectWorkbookContentType(saveAsType);
		if (m_topRelations == null)
		{
			m_topRelations = new RelationCollection();
			m_topRelations[GetRelationId(1)] = new Relation(m_strWorkbookPartName, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");
		}
		SaveSST();
		m_arrImageItemNames = SaveWorkbookImages();
		m_arrImageItemNames = null;
	}

	private string SelectWorkbookContentType(OfficeSaveType saveType)
	{
		if (m_book.HasMacros)
		{
			return (saveType == OfficeSaveType.SaveAsTemplate) ? "application/vnd.ms-excel.template.macroEnabled.main+xml" : "application/vnd.ms-excel.sheet.macroEnabled.main+xml";
		}
		return (saveType == OfficeSaveType.SaveAsTemplate) ? "application/vnd.openxmlformats-officedocument.spreadsheetml.template.main+xml" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml";
	}

	private string[] SaveWorkbookImages()
	{
		List<MsofbtBSE> pictures = m_book.ShapesData.Pictures;
		int num = pictures?.Count ?? 0;
		string[] array = new string[num];
		List<int> list = new List<int>();
		int i = 0;
		for (int num2 = num; i < num2; i++)
		{
			MsofbtBSE msofbtBSE = pictures[i];
			if (msofbtBSE.PicturePath != null)
			{
				array[i] = SerializeBSE(msofbtBSE);
			}
			else
			{
				list.Add(i);
			}
		}
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			int num3 = list[j];
			MsofbtBSE bse = pictures[num3];
			array[num3] = SerializeBSE(bse);
		}
		return array;
	}

	private string SerializeBSE(MsofbtBSE bse)
	{
		if (bse == null)
		{
			throw new ArgumentNullException("bse");
		}
		if (bse.BlipType == MsoBlipType.msoblipERROR)
		{
			return null;
		}
		return SaveImage(bse.PictureRecord.Picture, bse.PicturePath);
	}

	internal Dictionary<int, int> SaveStyles(ZipArchive archive, MemoryStream stream)
	{
		AddSlashPreprocessor addSlashPreprocessor = new AddSlashPreprocessor();
		m_dicOverriddenTypes[addSlashPreprocessor.PreprocessName(m_strStylesPartName)] = "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml";
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(new StreamWriter(stream));
		Dictionary<int, int> dictionary = Serializator.SerializeStyles(xmlWriter, ref m_streamDxfs);
		xmlWriter.Flush();
		string itemName = m_strStylesPartName;
		if (m_strStylesPartName[0] == '/')
		{
			itemName = UtilityMethods.RemoveFirstCharUnsafe(m_strStylesPartName);
		}
		int num = 0;
		foreach (KeyValuePair<string, string> dicOverriddenType in m_dicOverriddenTypes)
		{
			if (dicOverriddenType.Value == "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml".ToString())
			{
				num++;
			}
		}
		if (dictionary.Count > 0 && num == 1)
		{
			archive.UpdateItem(itemName, stream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
		return dictionary;
	}

	private void SaveSST()
	{
		if (m_book.InnerSST.ActiveCount > 0)
		{
			AddSlashPreprocessor addSlashPreprocessor = new AddSlashPreprocessor();
			m_dicOverriddenTypes[addSlashPreprocessor.PreprocessName(m_strSSTPartName)] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml";
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(new StreamWriter(memoryStream));
			Serializator.SerializeSST(xmlWriter);
			xmlWriter.Flush();
			string itemName = m_strSSTPartName;
			if (m_strSSTPartName[0] == '/')
			{
				itemName = UtilityMethods.RemoveFirstCharUnsafe(m_strSSTPartName);
			}
			m_archive.UpdateItem(itemName, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
	}

	private string AddRelation(RelationCollection relations, string target, string parentPath, string type, string relationId)
	{
		if (target[0] == '/')
		{
			target = UtilityMethods.RemoveFirstCharUnsafe(target);
		}
		if (target.StartsWith(parentPath))
		{
			target = target.Substring(parentPath.Length);
		}
		Relation relation = new Relation(target, type);
		if (relationId != null)
		{
			relations[relationId] = relation;
		}
		else
		{
			relationId = relations.Add(relation);
		}
		return relationId;
	}

	public void SaveRelations(string parentPartName, RelationCollection relations)
	{
		if (relations != null && relations.Count != 0)
		{
			if (parentPartName == null || parentPartName.Length == 0)
			{
				throw new ArgumentOutOfRangeException("parentPartName");
			}
			string correspondingRelations = GetCorrespondingRelations(parentPartName);
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(new StreamWriter(memoryStream));
			Serializator.SerializeRelations(xmlWriter, relations, null);
			xmlWriter.Flush();
			m_archive.UpdateItem(correspondingRelations, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
	}

	private void ReserveSheetRelations(WorkbookObjectsCollection sheets, RelationCollection relations)
	{
		int i = 0;
		for (int count = sheets.Count; i < count; i++)
		{
			WorksheetDataHolder dataHolder = ((WorksheetBaseImpl)sheets[i]).DataHolder;
			if (dataHolder != null)
			{
				string relationId = dataHolder.RelationId;
				if (relationId != null)
				{
					relations[relationId] = null;
				}
			}
		}
	}

	private void UpdateArchiveItem(WorksheetImpl sheet, string itemName)
	{
		if (sheet.m_dataHolder == null)
		{
			m_archive.RemoveItem(itemName);
			ZipArchiveItem item = m_archive.AddItem(itemName, null, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
			sheet.m_dataHolder = new WorksheetDataHolder(this, item);
			return;
		}
		ZipArchiveItem archiveItem = sheet.m_dataHolder.ArchiveItem;
		if (archiveItem == null || archiveItem.ItemName != itemName)
		{
			if (m_archive.Find(itemName) >= 0)
			{
				m_archive.UpdateItem(itemName, null, controlStream: false, DocGen.Compression.FileAttributes.Archive);
			}
			else
			{
				m_archive.AddItem(itemName, null, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
			}
			sheet.m_dataHolder.ArchiveItem = m_archive[itemName];
		}
	}

	private void SaveChartsheet(ChartImpl chart, string itemName)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (itemName == null || itemName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("itemName");
		}
		string key = itemName;
		if (itemName[0] != '/')
		{
			key = "/" + itemName;
		}
		m_dicOverriddenTypes[key] = "application/vnd.openxmlformats-officedocument.spreadsheetml.chartsheet+xml";
		if (chart.IsSaved && chart.m_dataHolder != null)
		{
			SerializeExistingData(chart, itemName);
			return;
		}
		if (chart.m_dataHolder == null)
		{
			m_archive.RemoveItem(itemName);
			ZipArchiveItem item = m_archive.AddItem(itemName, null, bControlStream: false, DocGen.Compression.FileAttributes.Archive);
			chart.m_dataHolder = new WorksheetDataHolder(this, item);
		}
		chart.m_dataHolder.SerializeChartsheet(chart);
	}

	private void SerializeExistingData(WorksheetBaseImpl sheet, string itemName)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (itemName == null || itemName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("itemName");
		}
		ZipArchiveItem archiveItem = (sheet.m_dataHolder ?? throw new ApplicationException("Cannot serialize sheet " + sheet.Name)).ArchiveItem;
		if (archiveItem.ItemName != itemName)
		{
			m_archive.RemoveItem(archiveItem.ItemName);
			archiveItem.ItemName = itemName;
			m_archive.AddItem(archiveItem);
		}
	}

	private void RemoveCalcChain()
	{
		string relationId;
		Relation relation = m_workbookRelations.FindRelationByContentType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/calcChain", out relationId);
		if (relation != null)
		{
			SeparateItemName(m_strWorkbookPartName, out var path);
			string text = path + relation.Target;
			m_archive.RemoveItem(text);
			m_workbookRelations.Remove(relationId);
			m_dicOverriddenTypes.Remove(text);
		}
	}

	internal void RemoveRelation(string strItemName, string relationID)
	{
		ItemsToRemove.Add(strItemName, null);
		m_workbookRelations.Remove(relationID);
	}

	public string SerializeExternalLink(ExternWorkbookImpl externBook)
	{
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		RelationCollection relations = Serializator.SerializeLinkItem(xmlWriter, externBook);
		xmlWriter.Flush();
		streamWriter.Flush();
		string text = GenerateExternalLinkName();
		m_archive.UpdateItem(text, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		SaveRelations(text, relations);
		m_dicOverriddenTypes["/" + text] = "application/vnd.openxmlformats-officedocument.spreadsheetml.externalLink+xml";
		return text;
	}

	private string GenerateExternalLinkName()
	{
		return GenerateItemName(ref m_iExternLinkIndex, "xl/externalLinks/externalLink", "xml");
	}

	private string GenerateItemName(ref int itemsCount, string pathStart, string extension)
	{
		return string.Format(pathStart + "{0}." + extension, ++itemsCount);
	}

	private string GenerateItemName(ref int itemsCount, string pathFormat)
	{
		string text = null;
		do
		{
			itemsCount++;
			text = string.Format(pathFormat, itemsCount);
		}
		while (m_archive.Find(text) >= 0);
		return text;
	}

	private string GenerateQueryItemName(ref int itemsCount, string pathFormat)
	{
		string text = null;
		do
		{
			itemsCount++;
			text = string.Format(pathFormat, itemsCount);
		}
		while (m_archive.Find(text) >= 0);
		return text;
	}

	internal string GeneratePivotTableName(int lastIndex)
	{
		return $"xl/pivotTables/pivotTable{++lastIndex}.xml";
	}

	internal void CreateDataHolder(WorksheetBaseImpl tabSheet, string fileName)
	{
		if (tabSheet == null)
		{
			throw new ArgumentNullException("tabSheet");
		}
		if (fileName == null || fileName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("fileName");
		}
		if (fileName[0] == '/')
		{
			fileName = UtilityMethods.RemoveFirstCharUnsafe(fileName);
		}
		int num = m_archive.Find(fileName);
		ZipArchiveItem item = ((num >= 0) ? m_archive[num] : m_archive.AddItem(fileName, null, bControlStream: false, DocGen.Compression.FileAttributes.Archive));
		tabSheet.DataHolder = new WorksheetDataHolder(this, item);
	}

	private string GenerateQueryTableFileName()
	{
		int itemsCount = 0;
		return GenerateQueryItemName(ref itemsCount, "/xl/queryTables/queryTable{0}.xml");
	}

	public string GetContentType(string strTarget)
	{
		if (!m_dicOverriddenTypes.TryGetValue(strTarget, out var value))
		{
			strTarget = strTarget.Split('.')[^1];
			return m_dicDefaultTypes[strTarget];
		}
		return value;
	}

	public void SerializeTableRelation(string ItemName, string queryTable)
	{
		int startIndex = ItemName.LastIndexOf('/');
		string itemName = ItemName.Insert(startIndex, "/_rels") + ".rels";
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		Excel2007Serializator excel2007Serializator = new Excel2007Serializator(m_book);
		RelationCollection relationCollection = new RelationCollection();
		Relation relation = new Relation(queryTable, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/queryTable");
		relationCollection.Add(relation);
		excel2007Serializator.SerializeRelations(xmlWriter, relationCollection, null);
		xmlWriter.Flush();
		streamWriter.Flush();
		m_archive.UpdateItem(itemName, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
	}

	public void Serialize(Stream stream, WorkbookImpl book, OfficeSaveType saveType)
	{
		if (book != m_book)
		{
			throw new ArgumentOutOfRangeException("book");
		}
	}

	internal FileDataHolder Clone(WorkbookImpl newParent)
	{
		FileDataHolder fileDataHolder = (FileDataHolder)MemberwiseClone();
		fileDataHolder.m_book = newParent;
		fileDataHolder.m_parser = null;
		fileDataHolder.m_serializator = null;
		if (m_workbookRelations != null)
		{
			fileDataHolder.m_workbookRelations = m_workbookRelations.Clone();
		}
		if (m_topRelations != null)
		{
			fileDataHolder.m_topRelations = m_topRelations.Clone();
		}
		fileDataHolder.m_arrImageItemNames = CloneUtils.CloneStringArray(m_arrImageItemNames);
		fileDataHolder.m_streamEnd = new MemoryStream(CloneUtils.CloneByteArray((m_streamEnd as MemoryStream).ToArray()));
		fileDataHolder.m_streamStart = new MemoryStream(CloneUtils.CloneByteArray((m_streamStart as MemoryStream).ToArray()));
		if (m_streamDxfs != null)
		{
			fileDataHolder.m_streamDxfs = new MemoryStream(CloneUtils.CloneByteArray((m_streamDxfs as MemoryStream).ToArray()));
		}
		if (m_functionGroups != null)
		{
			fileDataHolder.m_functionGroups = new MemoryStream(CloneUtils.CloneByteArray((m_functionGroups as MemoryStream).ToArray()));
		}
		if (m_dictItemsToRemove != null)
		{
			fileDataHolder.m_dictItemsToRemove = new Dictionary<string, object>(m_dictItemsToRemove);
		}
		if (m_dicDefaultTypes != null)
		{
			fileDataHolder.m_dicDefaultTypes = new Dictionary<string, string>(m_dicDefaultTypes);
		}
		if (m_dicOverriddenTypes != null)
		{
			fileDataHolder.m_dicOverriddenTypes = new Dictionary<string, string>(m_dicOverriddenTypes);
		}
		if (m_arrCellFormats != null)
		{
			fileDataHolder.m_arrCellFormats = new List<int>(m_arrCellFormats);
		}
		fileDataHolder.m_lstParsedDxfs = CloneDxfs();
		fileDataHolder.m_lstBookViews = CloneViews();
		fileDataHolder.m_archive = m_archive.Clone();
		if (m_parser != null && m_parser.m_dicThemeColors != null)
		{
			fileDataHolder.Parser.m_dicThemeColors = new Dictionary<string, Color>(m_parser.m_dicThemeColors);
		}
		return fileDataHolder;
	}

	private List<Dictionary<string, string>> CloneViews()
	{
		List<Dictionary<string, string>> list;
		if (m_lstBookViews != null)
		{
			int count = m_lstBookViews.Count;
			list = new List<Dictionary<string, string>>(count);
			for (int i = 0; i < count; i++)
			{
				Dictionary<string, string> dictionary = m_lstBookViews[i];
				list.Add(new Dictionary<string, string>(dictionary));
			}
		}
		else
		{
			list = null;
		}
		return list;
	}

	private List<DxfImpl> CloneDxfs()
	{
		List<DxfImpl> list;
		if (m_lstParsedDxfs != null)
		{
			int count = m_lstParsedDxfs.Count;
			list = new List<DxfImpl>(count);
			for (int i = 0; i < count; i++)
			{
				DxfImpl dxfImpl = m_lstParsedDxfs[i];
				list.Add(dxfImpl.Clone(m_book));
			}
		}
		else
		{
			list = null;
		}
		return list;
	}

	internal void RegisterCache(string cacheId, string relationId)
	{
		m_preservedCaches.Add(cacheId, relationId);
	}

	public void Dispose()
	{
		if (m_serializator != null)
		{
			m_serializator.Dispose();
			m_serializator = null;
		}
		if (m_archive != null)
		{
			m_archive.Dispose();
			m_archive = null;
		}
		if (m_parser != null)
		{
			m_parser.Dispose();
			m_parser = null;
		}
		if (m_workbookRelations != null)
		{
			m_workbookRelations.Dispose();
			m_workbookRelations = null;
		}
		m_functionGroups = null;
		m_extensions = null;
		m_streamDxfs = null;
		m_streamStart = null;
		m_streamEnd = null;
		GC.SuppressFinalize(this);
	}

	internal void ParseDocument(Stream excelStream)
	{
		List<Color> themeColors = new List<Color>();
		m_archive.Open(excelStream, closeStream: false);
		ParseDocument(ref themeColors, parseOnDemand: true);
	}
}
