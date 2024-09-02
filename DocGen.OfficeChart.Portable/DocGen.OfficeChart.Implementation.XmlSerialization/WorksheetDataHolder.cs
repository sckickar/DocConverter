using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml;
using DocGen.Compression;
using DocGen.Compression.Zip;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.XmlReaders;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class WorksheetDataHolder : IDisposable
{
	private const string VmlDrawingItemFormat = "xl/drawings/vmlDrawing{0}.vml";

	private const string CommentItemFormat = "xl/comments{0}.xml";

	private const string DrawingItemFormat = "xl/drawings/drawing{0}.xml";

	private const string VmlExtension = "vml";

	private ZipArchiveItem m_archiveItem;

	private FileDataHolder m_parentHolder;

	private MemoryStream m_startStream = new MemoryStream();

	private MemoryStream m_cfStream = new MemoryStream();

	internal Stream m_cfsStream;

	private string m_strBookRelationId;

	private string m_strSheetId;

	private RelationCollection m_relations;

	private RelationCollection m_drawingsRelation;

	private RelationCollection m_hfDrawingsRelation;

	private string m_strVmlDrawingsId;

	private string m_strVmlHFDrawingsId;

	private string m_strCommentsId;

	private string m_strDrawingsId;

	private Stream m_streamControls;

	private Dictionary<string, RelationCollection> m_preservedPivotTable;

	private Dictionary<ChartStyleElements, ShapeStyle> m_defaultChartStyleElements;

	private double[][] m_defaultColorVariations;

	public FileDataHolder ParentHolder => m_parentHolder;

	public ZipArchiveItem ArchiveItem
	{
		get
		{
			return m_archiveItem;
		}
		set
		{
			m_archiveItem = value;
		}
	}

	public string RelationId
	{
		get
		{
			return m_strBookRelationId;
		}
		set
		{
			m_strBookRelationId = value;
		}
	}

	public string SheetId
	{
		get
		{
			return m_strSheetId;
		}
		set
		{
			m_strSheetId = value;
		}
	}

	public RelationCollection Relations
	{
		get
		{
			if (m_relations == null)
			{
				m_relations = new RelationCollection();
			}
			return m_relations;
		}
	}

	public RelationCollection DrawingsRelations
	{
		get
		{
			if (m_drawingsRelation == null)
			{
				m_drawingsRelation = new RelationCollection();
			}
			return m_drawingsRelation;
		}
	}

	public RelationCollection HFDrawingsRelations
	{
		get
		{
			if (m_hfDrawingsRelation == null)
			{
				m_hfDrawingsRelation = new RelationCollection();
			}
			return m_hfDrawingsRelation;
		}
	}

	public string VmlDrawingsId
	{
		get
		{
			return m_strVmlDrawingsId;
		}
		set
		{
			m_strVmlDrawingsId = value;
		}
	}

	public string VmlHFDrawingsId
	{
		get
		{
			return m_strVmlHFDrawingsId;
		}
		set
		{
			m_strVmlHFDrawingsId = value;
		}
	}

	public string CommentNotesId
	{
		get
		{
			return m_strCommentsId;
		}
		set
		{
			m_strCommentsId = value;
		}
	}

	public string DrawingsId
	{
		get
		{
			return m_strDrawingsId;
		}
		set
		{
			m_strDrawingsId = value;
		}
	}

	public Stream ControlsStream
	{
		get
		{
			return m_streamControls;
		}
		set
		{
			m_streamControls = value;
		}
	}

	internal Dictionary<ChartStyleElements, ShapeStyle> DefaultChartStyleElements => m_defaultChartStyleElements;

	internal double[][] DefaultColorVariationArray => m_defaultColorVariations;

	public WorksheetDataHolder(FileDataHolder holder, Relation relation, string parentPath)
	{
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		if (relation == null)
		{
			throw new ArgumentNullException("relation");
		}
		m_archiveItem = holder[relation, parentPath];
		m_parentHolder = holder;
	}

	public WorksheetDataHolder(FileDataHolder holder, ZipArchiveItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		m_archiveItem = item;
		m_parentHolder = holder;
	}

	public void ParseConditionalFormatting(List<DxfImpl> dxfStyles, WorksheetImpl sheet)
	{
		if (m_cfStream != null && m_cfStream.Length != 0L)
		{
			m_cfStream.Position = 0L;
			_ = m_parentHolder.Parser;
			XmlReader xmlReader = UtilityMethods.CreateReader(m_cfStream);
			if (xmlReader.LocalName == "root")
			{
				xmlReader.Read();
			}
			sheet.m_parseCondtionalFormats = false;
			sheet.m_parseCF = false;
			xmlReader.Dispose();
			m_cfStream.Dispose();
			m_cfStream = null;
		}
		if (m_cfsStream != null && m_cfsStream.Length != 0L)
		{
			m_cfsStream.Position = 0L;
			_ = m_parentHolder.Parser;
			XmlReader xmlReader2 = UtilityMethods.CreateReader(m_cfsStream);
			if (xmlReader2.LocalName == "root")
			{
				xmlReader2.Read();
			}
			_ = xmlReader2.LocalName == "conditionalFormattings";
			sheet.m_parseCondtionalFormats = false;
			m_cfsStream = null;
		}
	}

	public void ParseWorksheetData(WorksheetImpl sheet, Dictionary<int, int> dictUpdateSSTIndexes, bool parseOnDemand)
	{
		if (m_archiveItem == null)
		{
			return;
		}
		XmlReader xmlReader = null;
		Excel2007Parser parser = m_parentHolder.Parser;
		string itemName = m_archiveItem.ItemName;
		int num = itemName.LastIndexOf('/');
		string strParentPath = itemName.Substring(0, num);
		string text = itemName.Insert(num, "/_rels") + ".rels";
		ZipArchiveItem zipArchiveItem = m_parentHolder.Archive[text];
		if (zipArchiveItem != null)
		{
			zipArchiveItem.DataStream.Position = 0L;
			xmlReader = UtilityMethods.CreateReader(zipArchiveItem.DataStream);
			m_relations = parser.ParseRelations(xmlReader);
			m_relations.ItemPath = text;
		}
		if (sheet.ParseDataOnDemand && !sheet.ParseOnDemand)
		{
			sheet.ParseOnDemand = true;
			return;
		}
		if (parseOnDemand)
		{
			sheet.ParseDataOnDemand = false;
			sheet.ParseOnDemand = true;
		}
		xmlReader = UtilityMethods.CreateReader(m_archiveItem.DataStream);
		sheet.ArchiveItemName = m_archiveItem.ItemName;
		bool throwOnUnknownNames = sheet.Workbook.ThrowOnUnknownNames;
		sheet.Workbook.ThrowOnUnknownNames = false;
		parser.ParseSheet(xmlReader, sheet, strParentPath, ref m_startStream, ref m_cfStream, m_parentHolder.XFIndexes, m_parentHolder.ItemsToRemove, dictUpdateSSTIndexes);
		sheet.Workbook.ThrowOnUnknownNames = throwOnUnknownNames;
		if (m_relations != null && m_relations.Count > 0)
		{
			CollectPivotRelations(itemName);
		}
		m_parentHolder.ItemsToRemove.Add(m_archiveItem.ItemName, null);
		m_parentHolder.ItemsToRemove.Add(text, null);
		m_archiveItem = null;
	}

	public void CollectPivotRelations(string itemName)
	{
		m_preservedPivotTable = new Dictionary<string, RelationCollection>();
		RelationCollection relationCollection = new RelationCollection();
		new RelationCollection();
		foreach (KeyValuePair<string, Relation> relation in m_relations)
		{
			if (relation.Value.Type == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/pivotTable")
			{
				relationCollection.Add(relation.Value);
			}
			relationCollection.ItemPath = m_relations.ItemPath;
		}
		m_preservedPivotTable.Add(itemName, relationCollection);
	}

	public void ParsePivotTable(IWorksheet sheet)
	{
		if (m_preservedPivotTable == null || m_preservedPivotTable.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, RelationCollection> item in m_preservedPivotTable)
		{
			string key = item.Key;
			_ = key[..key.LastIndexOf('/')];
		}
	}

	public void ParseChartsheetData(ChartImpl chart)
	{
		Excel2007Parser parser = m_parentHolder.Parser;
		string itemName = m_archiveItem.ItemName;
		_ = itemName[..itemName.LastIndexOf('/')];
		string correspondingRelations = FileDataHolder.GetCorrespondingRelations(itemName);
		m_relations = m_parentHolder.ParseRelations(correspondingRelations);
		XmlReader reader = UtilityMethods.CreateReader(m_archiveItem.DataStream);
		parser.ParseChartsheet(reader, chart);
	}

	[SecurityCritical]
	public void SerializeChartsheet(ChartImpl chart)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		SerializeChartsheetPart(chart);
		SerializeVmlDrawings(chart);
		SerializeHeaderFooterImages(chart, null);
		SerializeWorksheetRelations();
	}

	public RelationCollection ParseVmlShapes(ShapeCollectionBase shapes, string relationId, RelationCollection relations)
	{
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		RelationCollection relationCollection = null;
		if (relations == null)
		{
			relations = m_relations;
		}
		if (relations != null)
		{
			Relation relation = relations[relationId];
			if (relation == null)
			{
				throw new ArgumentException("relationId");
			}
			string itemName = m_archiveItem.ItemName;
			string[] array = itemName.Split('/');
			itemName = array[0] + "\\" + array[1];
			itemName = itemName.Replace('\\', '/');
			XmlReader reader = m_parentHolder.CreateReaderAndFixBr(relation, itemName, out var strItemPath);
			string correspondingRelations = FileDataHolder.GetCorrespondingRelations(strItemPath);
			relationCollection = m_parentHolder.ParseRelations(correspondingRelations);
			int num = strItemPath.LastIndexOf('/');
			if (num >= 0)
			{
				strItemPath = strItemPath.Substring(0, num);
			}
			m_parentHolder.Parser.ParseVmlShapes(reader, shapes, relationCollection, strItemPath);
			WorksheetImpl worksheet = shapes.Worksheet;
			if (worksheet != null)
			{
				Relation relation2 = m_relations.FindRelationByContentType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/comments", out m_strCommentsId);
				if (relation2 != null)
				{
					reader = m_parentHolder.CreateReader(relation2, itemName);
					m_parentHolder.Parser.ParseComments(reader, worksheet);
				}
			}
		}
		return relationCollection;
	}

	public void ParseDrawings(WorksheetBaseImpl sheet, string relationId, Dictionary<string, object> dictItemsToRemove, bool isChartShape)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (m_relations != null)
		{
			Relation drawingRelation = m_relations[relationId];
			ParseDrawings(sheet, drawingRelation, dictItemsToRemove, isChartShape);
		}
	}

	public void ParseDrawings(WorksheetBaseImpl sheet, Relation drawingRelation, Dictionary<string, object> dictItemsToRemove, bool isChartShape)
	{
		if (drawingRelation == null)
		{
			throw new ArgumentException("relationId");
		}
		string text = m_archiveItem.ItemName;
		if (!string.IsNullOrEmpty(text))
		{
			string[] array = text.Split('/');
			text = array[0] + "\\" + array[1];
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Replace('\\', '/');
		}
		XmlReader reader = ((sheet is ChartImpl) ? UtilityMethods.CreateReader((sheet as ChartImpl).ChartIteams[drawingRelation.Target]) : m_parentHolder.CreateReader(drawingRelation, text));
		string itemName = FileDataHolder.CombinePath(text, drawingRelation.Target);
		Excel2007Parser parser = m_parentHolder.Parser;
		string correspondingRelations = FileDataHolder.GetCorrespondingRelations(itemName);
		RelationCollection relationCollection = m_parentHolder.ParseRelations(correspondingRelations);
		bool flag = sheet is ChartImpl;
		RelationCollection relationCollection2 = null;
		if (relationCollection != null)
		{
			if (flag && m_drawingsRelation != null)
			{
				relationCollection2 = m_drawingsRelation;
			}
			m_drawingsRelation = relationCollection;
		}
		FileDataHolder.SeparateItemName(itemName, out var path);
		List<string> list = new List<string>();
		parser.ParseDrawings(reader, sheet, path, list, dictItemsToRemove, isChartShape);
		ZipArchive archive = m_parentHolder.Archive;
		archive.RemoveItem(correspondingRelations);
		archive.RemoveItem(itemName);
		if (m_drawingsRelation != null)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				string id = list[i];
				m_drawingsRelation.Remove(id);
			}
		}
		if (flag && relationCollection2 != null)
		{
			m_drawingsRelation = relationCollection2;
		}
	}

	[SecurityCritical]
	private void SerializeWorksheetPart(WorksheetImpl sheet, Dictionary<int, int> hashNewXFIndexes)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (!sheet.ParseDataOnDemand)
		{
			Excel2007Serializator serializator = m_parentHolder.Serializator;
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(new StreamWriter(memoryStream));
			serializator.SerializeWorksheet(xmlWriter, sheet, m_startStream, m_cfStream, hashNewXFIndexes, m_cfsStream);
			xmlWriter.Flush();
			memoryStream.Flush();
			m_archiveItem.Update(memoryStream, controlStream: true);
		}
	}

	[SecurityCritical]
	private void SerializeChartsheetPart(ChartImpl chart)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		string text = GenerateDrawingsName(chart.Workbook);
		string text2 = Relations.GenerateRelationId();
		m_relations[text2] = new Relation("/" + text, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing");
		m_parentHolder.OverriddenContentTypes["/" + text] = "application/vnd.openxmlformats-officedocument.drawing+xml";
		ChartSerializator chartSerializator = new ChartSerializator();
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(new StreamWriter(memoryStream));
		chartSerializator.SerializeChartsheet(xmlWriter, chart, text2);
		xmlWriter.Flush();
		memoryStream.Flush();
		m_archiveItem.Update(memoryStream, controlStream: true);
		RelationCollection relationCollection = new RelationCollection();
		string dummyChartRelation = null;
		string chartSheetDrawingId = SerializeChartSheetDrawing(chart, text, relationCollection, out dummyChartRelation);
		SerializeChartObject(chart, relationCollection, chartSheetDrawingId, dummyChartRelation);
		SerializeRelations(relationCollection, text, null);
	}

	[SecurityCritical]
	private void SerializeChartObject(ChartImpl chart, RelationCollection drawingRelations, string chartSheetDrawingId, string dummyChartRelationId)
	{
		string text = null;
		string text2 = null;
		bool flag = ChartImpl.IsChartExSerieType(chart.ChartType);
		string type = (flag ? "http://schemas.microsoft.com/office/2014/relationships/chartEx" : "http://schemas.openxmlformats.org/officeDocument/2006/relationships/chart");
		string value = (flag ? "application/vnd.ms-office.chartex+xml" : "application/vnd.openxmlformats-officedocument.drawingml.chart+xml");
		if (flag)
		{
			text = ChartShapeSerializator.GetChartExFileName(this, chart);
			text2 = ChartShapeSerializator.GetChartFileName(this, chart);
			drawingRelations[dummyChartRelationId] = new Relation(text2, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/chart");
			m_parentHolder.OverriddenContentTypes[text2] = "application/vnd.openxmlformats-officedocument.drawingml.chart+xml";
		}
		else
		{
			text = ChartShapeSerializator.GetChartFileName(this, chart);
		}
		drawingRelations[chartSheetDrawingId] = new Relation(text, type);
		m_parentHolder.OverriddenContentTypes[text] = value;
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		if (chart.DataHolder == null)
		{
			m_parentHolder.CreateDataHolder(chart, text);
		}
		if (flag)
		{
			new ChartExSerializator().SerializeChartEx(xmlWriter, chart);
			SerializeDummyChartForChartEx(chart.ParentWorkbook, m_parentHolder, text2, dummyChartRelationId);
		}
		else
		{
			new ChartSerializator().SerializeChart(xmlWriter, chart, text);
		}
		xmlWriter.Flush();
		streamWriter.Flush();
		text = UtilityMethods.RemoveFirstCharUnsafe(text);
		m_parentHolder.Archive.UpdateItem(text, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		SerializeRelations(chart.Relations, text, null, chart);
	}

	[SecurityCritical]
	private void SerializeDummyChartForChartEx(WorkbookImpl workbook, FileDataHolder holder, string chartName, string chartRelationId)
	{
		ChartImpl chartImpl = workbook.Charts.Add() as ChartImpl;
		chartImpl.ChartType = OfficeChartType.Column_Clustered;
		if (chartImpl.DataHolder == null)
		{
			holder.CreateDataHolder(chartImpl, chartName);
		}
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		new ChartSerializator(value: true).SerializeChart(xmlWriter, chartImpl, chartName);
		xmlWriter.Flush();
		streamWriter.Flush();
		chartName = UtilityMethods.RemoveFirstCharUnsafe(chartName);
		holder.Archive.UpdateItem(chartName, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		workbook.Charts.Remove(chartImpl.Name);
	}

	internal void SerializeChartExFallbackShape(WorksheetBaseImpl sheet, RelationCollection relations, ref string id, string chartItemName, string contentType, string relationType)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		_ = sheet.InnerShapes;
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		_ = m_parentHolder.Serializator;
		xmlWriter.WriteStartDocument(standalone: true);
		xmlWriter.WriteStartElement("c", "userShapes", "http://schemas.openxmlformats.org/drawingml/2006/chart");
		xmlWriter.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlWriter.WriteStartElement("cdr", "relSizeAnchor", "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing");
		xmlWriter.WriteAttributeString("xmlns", "cdr", null, "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing");
		xmlWriter.WriteStartElement("from", "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing");
		xmlWriter.WriteElementString("x", "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing", "0");
		xmlWriter.WriteElementString("y", "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing", "0");
		xmlWriter.WriteEndElement();
		xmlWriter.WriteStartElement("to", "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing");
		xmlWriter.WriteElementString("x", "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing", "1");
		xmlWriter.WriteElementString("y", "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing", "1");
		xmlWriter.WriteEndElement();
		SerializeChartExFallBackShapeContent(xmlWriter, isChartSheet: true);
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.Flush();
		streamWriter.Flush();
		memoryStream.Flush();
		string text = GenerateDrawingsName(sheet.Workbook);
		m_parentHolder.Archive.UpdateItem(text, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		string text2 = "/" + text;
		relations[id] = new Relation(text2, relationType);
		m_parentHolder.OverriddenContentTypes[text2] = contentType;
		SerializeRelations(relations, chartItemName, null, null);
	}

	internal void SerializeRelations(RelationCollection relations, string strParentItemName, WorksheetDataHolder holder, WorksheetBaseImpl chart)
	{
		if (strParentItemName == null || strParentItemName.Length == 0)
		{
			throw new ArgumentOutOfRangeException(strParentItemName);
		}
		relations = SerializeChartExStyles(relations, strParentItemName, holder, chart);
		if (relations != null && relations.Count > 0)
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
			m_parentHolder.Serializator.SerializeRelations(xmlWriter, relations, holder);
			xmlWriter.Flush();
			streamWriter.Flush();
			memoryStream.Flush();
			int num = strParentItemName.LastIndexOf('/');
			if (strParentItemName[0] == '/')
			{
				strParentItemName = UtilityMethods.RemoveFirstCharUnsafe(strParentItemName);
				num--;
			}
			string itemName = strParentItemName.Insert(num, "/_rels") + ".rels";
			m_parentHolder.Archive.UpdateItem(itemName, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
		else if (new Regex("^\\/{0,1}xl\\/charts\\/chart\\d+\\.xml").Match(strParentItemName).Success)
		{
			int num2 = strParentItemName.LastIndexOf('/');
			if (strParentItemName[0] == '/')
			{
				strParentItemName = UtilityMethods.RemoveFirstCharUnsafe(strParentItemName);
				num2--;
			}
			string itemName2 = strParentItemName.Insert(num2, "/_rels") + ".rels";
			m_parentHolder.Archive.RemoveItem(itemName2);
		}
	}

	private RelationCollection SerializeChartExStyles(RelationCollection relations, string strParentItemName, WorksheetDataHolder holder, WorksheetBaseImpl chart)
	{
		if (chart is ChartImpl chartImpl && strParentItemName.ToLower().Contains("chartex") && ChartImpl.IsChartExSerieType(chartImpl.ChartType))
		{
			if (!chartImpl.m_isChartStyleSkipped)
			{
				if (relations == null)
				{
					relations = new RelationCollection();
				}
				string id = relations.GenerateRelationId();
				string text = TryAndGetFileName(UtilityMethods.RemoveFirstCharUnsafe("/xl/charts/style{0}.xml"), m_parentHolder.Archive);
				string text2 = "/" + text;
				relations[id] = new Relation(text2, "http://schemas.microsoft.com/office/2011/relationships/chartStyle");
				MemoryStream memoryStream = new MemoryStream();
				StreamWriter streamWriter = new StreamWriter(memoryStream);
				XmlWriter writer = UtilityMethods.CreateWriter(streamWriter);
				SerializeDefaultChartStyles(writer, chartImpl, chartImpl.AppImplementation);
				streamWriter.Flush();
				memoryStream.Flush();
				m_parentHolder.Archive.UpdateItem(text, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
				m_parentHolder.OverriddenContentTypes[text2] = "application/vnd.ms-office.chartstyle+xml";
			}
			if (!chartImpl.m_isChartColorStyleSkipped)
			{
				if (relations == null)
				{
					relations = new RelationCollection();
				}
				string id2 = relations.GenerateRelationId();
				string text3 = TryAndGetFileName(UtilityMethods.RemoveFirstCharUnsafe("/xl/charts/colors{0}.xml"), m_parentHolder.Archive);
				string text4 = "/" + text3;
				relations[id2] = new Relation(text4, "http://schemas.microsoft.com/office/2011/relationships/chartColorStyle");
				MemoryStream memoryStream2 = new MemoryStream();
				StreamWriter streamWriter2 = new StreamWriter(memoryStream2);
				XmlWriter writer2 = UtilityMethods.CreateWriter(streamWriter2);
				SerializeDefaultChartColorStyles(writer2, chartImpl.AppImplementation);
				streamWriter2.Flush();
				memoryStream2.Flush();
				m_parentHolder.Archive.UpdateItem(text3, memoryStream2, controlStream: true, DocGen.Compression.FileAttributes.Archive);
				m_parentHolder.OverriddenContentTypes[text4] = "application/vnd.ms-office.chartcolorstyle+xml";
			}
		}
		return relations;
	}

	private string TryAndGetFileName(string itemFormatName, ZipArchive zipArchive)
	{
		int num = 1;
		string text = string.Format(itemFormatName, num);
		while (zipArchive[text] != null)
		{
			num++;
			text = string.Format(itemFormatName, num);
		}
		return text;
	}

	private void SerializeDefaultChartColorStyles(XmlWriter writer, ApplicationImpl applicationImpl)
	{
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("cs", "colorStyle", "http://schemas.microsoft.com/office/drawing/2012/chartStyle");
		writer.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("xmlns", "cs", null, "http://schemas.microsoft.com/office/drawing/2012/chartStyle");
		writer.WriteAttributeString("id", "10");
		writer.WriteAttributeString("meth", "cycle");
		writer.WriteStartElement("a", "schemeClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", "accent1");
		writer.WriteEndElement();
		writer.WriteStartElement("a", "schemeClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", "accent2");
		writer.WriteEndElement();
		writer.WriteStartElement("a", "schemeClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", "accent3");
		writer.WriteEndElement();
		writer.WriteStartElement("a", "schemeClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", "accent4");
		writer.WriteEndElement();
		writer.WriteStartElement("a", "schemeClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", "accent5");
		writer.WriteEndElement();
		writer.WriteStartElement("a", "schemeClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", "accent6");
		writer.WriteEndElement();
		writer.WriteElementString("cs", "variation", "http://schemas.microsoft.com/office/drawing/2012/chartStyle", "");
		if (m_defaultColorVariations == null)
		{
			InitializeChartColorElements();
		}
		for (int i = 0; i < m_defaultColorVariations.Length; i++)
		{
			writer.WriteStartElement("cs", "variation", "http://schemas.microsoft.com/office/drawing/2012/chartStyle");
			writer.WriteStartElement("a", "lumMod", "http://schemas.openxmlformats.org/drawingml/2006/main");
			writer.WriteAttributeString("val", m_defaultColorVariations[i][0].ToString());
			writer.WriteEndElement();
			if (m_defaultColorVariations[i].Length == 2)
			{
				writer.WriteStartElement("a", "lumOff", "http://schemas.openxmlformats.org/drawingml/2006/main");
				writer.WriteAttributeString("val", m_defaultColorVariations[i][1].ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.Flush();
	}

	private void SerializeDefaultChartStyles(XmlWriter writer, ChartImpl chart, ApplicationImpl applicationImpl)
	{
		writer.WriteStartDocument(standalone: true);
		writer.WriteStartElement("cs", "chartStyle", "http://schemas.microsoft.com/office/drawing/2012/chartStyle");
		writer.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("xmlns", "cs", null, "http://schemas.microsoft.com/office/drawing/2012/chartStyle");
		writer.WriteAttributeString("id", "419");
		if (m_defaultChartStyleElements == null)
		{
			InitializeChartStyleElements();
		}
		for (int i = 0; i < 32; i++)
		{
			if (!m_defaultChartStyleElements.ContainsKey((ChartStyleElements)i))
			{
				continue;
			}
			ChartStyleElements chartStyleElements = ChartStyleElements.extLst;
			ShapeStyle shapeStyle = m_defaultChartStyleElements[(ChartStyleElements)i];
			if (shapeStyle != null)
			{
				if (chart.IsTreeMapOrSunBurst)
				{
					switch ((ChartStyleElements)i)
					{
					case ChartStyleElements.dataPoint:
						shapeStyle.ShapeProperties.BorderWeight = 19050.0;
						shapeStyle.ShapeProperties.BorderFillColorValue = "lt1";
						chartStyleElements = ChartStyleElements.dataPoint;
						break;
					case ChartStyleElements.dataLabel:
						shapeStyle.FontRefstyleEntry.ColorValue = "lt1";
						shapeStyle.FontRefstyleEntry.LumOffValue1 = -1.0;
						shapeStyle.FontRefstyleEntry.LumModValue = -1.0;
						chartStyleElements = ChartStyleElements.dataLabel;
						break;
					case ChartStyleElements.axisTitle:
						shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
						shapeStyle.ShapeProperties.ShapeFillColorModelType = ColorModel.schemeClr;
						shapeStyle.ShapeProperties.ShapeFillColorValue = "bg1";
						shapeStyle.ShapeProperties.ShapeFillLumModValue = 65000.0;
						shapeStyle.ShapeProperties.BorderWeight = 19050.0;
						shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
						shapeStyle.ShapeProperties.BorderFillColorValue = "bg1";
						chartStyleElements = ChartStyleElements.axisTitle;
						break;
					}
				}
				if (chart.ChartType == OfficeChartType.Funnel && i == 1)
				{
					shapeStyle.ShapeProperties.BorderWeight = 0.0;
					chartStyleElements = ChartStyleElements.categoryAxis;
				}
				ChartStyleElements chartStyleElements2 = (ChartStyleElements)i;
				shapeStyle.Write(writer, chartStyleElements2.ToString());
			}
			else if (i == 9)
			{
				writer.WriteStartElement("cs", ChartStyleElements.dataPointMarkerLayout.ToString(), "http://schemas.microsoft.com/office/drawing/2012/chartStyle");
				writer.WriteAttributeString("size", "5");
				writer.WriteAttributeString("symbol", "circle");
				writer.WriteEndElement();
			}
			switch (chartStyleElements)
			{
			case ChartStyleElements.axisTitle:
				shapeStyle.ShapeProperties = null;
				break;
			case ChartStyleElements.dataLabel:
				shapeStyle.FontRefstyleEntry.ColorValue = "tx1";
				shapeStyle.FontRefstyleEntry.LumOffValue1 = 75000.0;
				shapeStyle.FontRefstyleEntry.LumModValue = 25000.0;
				break;
			case ChartStyleElements.dataPoint:
				shapeStyle.ShapeProperties.BorderWeight = -1.0;
				shapeStyle.ShapeProperties.BorderFillColorValue = "phClr";
				break;
			case ChartStyleElements.categoryAxis:
				shapeStyle.ShapeProperties.BorderWeight = 9525.0;
				break;
			}
		}
		writer.WriteEndElement();
		writer.Flush();
	}

	private void InitializeChartStyleElements()
	{
		m_defaultChartStyleElements = new Dictionary<ChartStyleElements, ShapeStyle>(31);
		ShapeStyle shapeStyle = null;
		string nameSpaceValue = "http://schemas.microsoft.com/office/drawing/2012/chartStyle";
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 10f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = 12f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		m_defaultChartStyleElements.Add(ChartStyleElements.axisTitle, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.BorderWeight = 9525.0;
		shapeStyle.ShapeProperties.LineCap = EndLineCap.flat;
		shapeStyle.ShapeProperties.BorderLineStyle = Excel2007ShapeLineStyle.sng;
		shapeStyle.ShapeProperties.IsInsetPenAlignment = false;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "tx1";
		shapeStyle.ShapeProperties.BorderFillLumModValue = 15000.0;
		shapeStyle.ShapeProperties.BorderFillLumOffValue1 = 85000.0;
		shapeStyle.ShapeProperties.BorderIsRound = true;
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 9f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = 12f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		m_defaultChartStyleElements.Add(ChartStyleElements.categoryAxis, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 9f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		m_defaultChartStyleElements.Add(ChartStyleElements.trendlineLabel, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.valueAxis, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, (StyleEntryModifierEnum)3);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.ShapeFillType = OfficeFillType.SolidColor;
		shapeStyle.ShapeProperties.ShapeFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.ShapeFillColorValue = "bg1";
		shapeStyle.ShapeProperties.BorderWeight = 9525.0;
		shapeStyle.ShapeProperties.LineCap = EndLineCap.flat;
		shapeStyle.ShapeProperties.BorderLineStyle = Excel2007ShapeLineStyle.sng;
		shapeStyle.ShapeProperties.IsInsetPenAlignment = false;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "tx1";
		shapeStyle.ShapeProperties.BorderFillLumModValue = 15000.0;
		shapeStyle.ShapeProperties.BorderFillLumOffValue1 = 85000.0;
		shapeStyle.ShapeProperties.BorderIsRound = true;
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 10f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = 12f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		m_defaultChartStyleElements.Add(ChartStyleElements.chartArea, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.BorderWeight = 9525.0;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "tx1";
		shapeStyle.ShapeProperties.BorderFillLumModValue = 15000.0;
		shapeStyle.ShapeProperties.BorderFillLumOffValue1 = 85000.0;
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 9f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		m_defaultChartStyleElements.Add(ChartStyleElements.dataTable, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 75000.0, 25000.0, -1.0, -1.0);
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 9f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = 12f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		shapeStyle.TextBodyProperties = new TextBodyPropertiesHolder();
		shapeStyle.TextBodyProperties.WrapTextInShape = true;
		shapeStyle.TextBodyProperties.SetLeftMargin(38100);
		shapeStyle.TextBodyProperties.SetTopMargin(19050);
		shapeStyle.TextBodyProperties.SetRightMargin(38100);
		shapeStyle.TextBodyProperties.SetBottomMargin(19050);
		shapeStyle.TextBodyProperties.TextDirection = TextDirection.Horizontal;
		shapeStyle.TextBodyProperties.VerticalAlignment = OfficeVerticalAlignment.Middle;
		shapeStyle.TextBodyProperties.IsAutoSize = true;
		m_defaultChartStyleElements.Add(ChartStyleElements.dataLabel, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.ShapeFillType = OfficeFillType.SolidColor;
		shapeStyle.ShapeProperties.ShapeFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.ShapeFillColorValue = "lt1";
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "dk1";
		shapeStyle.ShapeProperties.BorderFillLumModValue = 25000.0;
		shapeStyle.ShapeProperties.BorderFillLumOffValue1 = 75000.0;
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 9f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		shapeStyle.TextBodyProperties = new TextBodyPropertiesHolder();
		shapeStyle.TextBodyProperties.WrapTextInShape = true;
		shapeStyle.TextBodyProperties.SetLeftMargin(36576);
		shapeStyle.TextBodyProperties.SetTopMargin(18288);
		shapeStyle.TextBodyProperties.SetRightMargin(36576);
		shapeStyle.TextBodyProperties.SetBottomMargin(18288);
		shapeStyle.TextBodyProperties.TextDirection = TextDirection.Horizontal;
		shapeStyle.TextBodyProperties.VerticalAlignment = OfficeVerticalAlignment.MiddleCentered;
		shapeStyle.TextBodyProperties.TextVertOverflowType = TextVertOverflowType.Clip;
		shapeStyle.TextBodyProperties.TextHorzOverflowType = TextHorzOverflowType.Clip;
		shapeStyle.TextBodyProperties.IsAutoSize = true;
		m_defaultChartStyleElements.Add(ChartStyleElements.dataLabelCallout, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.styleClr, "auto", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.styleClr, "auto", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.ShapeFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.ShapeFillColorValue = "phClr";
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "phClr";
		m_defaultChartStyleElements.Add(ChartStyleElements.dataPoint, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.styleClr, "auto", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.ShapeFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.ShapeFillColorValue = "phClr";
		m_defaultChartStyleElements.Add(ChartStyleElements.dataPoint3D, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.styleClr, "auto", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.BorderWeight = 28575.0;
		shapeStyle.ShapeProperties.LineCap = EndLineCap.rnd;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "phClr";
		shapeStyle.ShapeProperties.BorderIsRound = true;
		m_defaultChartStyleElements.Add(ChartStyleElements.dataPointLine, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.dataPointWireframe, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.styleClr, "auto", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.ShapeFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.ShapeFillColorValue = "phClr";
		shapeStyle.ShapeProperties.BorderWeight = 9525.0;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "lt1";
		m_defaultChartStyleElements.Add(ChartStyleElements.dataPointMarker, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.dataPointMarkerLayout, null);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none)
		{
			LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0),
			FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0),
			EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0),
			FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0)
		};
		m_defaultChartStyleElements.Add(ChartStyleElements.wall, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.dropLine, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.errorBar, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.floor, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.hiLoLine, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.leaderLine, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "dk1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.ShapeFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.ShapeFillColorValue = "dk1";
		m_defaultChartStyleElements.Add(ChartStyleElements.downBar, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "dk1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.ShapeFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.ShapeFillColorValue = "lt1";
		m_defaultChartStyleElements.Add(ChartStyleElements.upBar, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 9f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = 12f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		m_defaultChartStyleElements.Add(ChartStyleElements.legend, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, (StyleEntryModifierEnum)3)
		{
			LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0),
			FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0),
			EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0),
			FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0)
		};
		m_defaultChartStyleElements.Add(ChartStyleElements.plotArea, shapeStyle);
		m_defaultChartStyleElements.Add(ChartStyleElements.plotArea3D, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.BorderWeight = 9525.0;
		shapeStyle.ShapeProperties.LineCap = EndLineCap.flat;
		shapeStyle.ShapeProperties.BorderLineStyle = Excel2007ShapeLineStyle.sng;
		shapeStyle.ShapeProperties.IsInsetPenAlignment = false;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "tx1";
		shapeStyle.ShapeProperties.BorderFillLumModValue = 15000.0;
		shapeStyle.ShapeProperties.BorderFillLumOffValue1 = 85000.0;
		shapeStyle.ShapeProperties.BorderIsRound = true;
		m_defaultChartStyleElements.Add(ChartStyleElements.gridlineMajor, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.BorderWeight = 9525.0;
		shapeStyle.ShapeProperties.LineCap = EndLineCap.flat;
		shapeStyle.ShapeProperties.BorderLineStyle = Excel2007ShapeLineStyle.sng;
		shapeStyle.ShapeProperties.IsInsetPenAlignment = false;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "tx1";
		shapeStyle.ShapeProperties.BorderFillLumModValue = 15000.0;
		shapeStyle.ShapeProperties.BorderFillLumOffValue1 = 85000.0;
		shapeStyle.ShapeProperties.BorderFillLumOffValue2 = 10000.0;
		shapeStyle.ShapeProperties.BorderIsRound = true;
		m_defaultChartStyleElements.Add(ChartStyleElements.gridlineMinor, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 14f;
		shapeStyle.DefaultRunParagraphProperties.Bold = false;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = 12f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = 0f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = 0;
		m_defaultChartStyleElements.Add(ChartStyleElements.title, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.BorderWeight = 19050.0;
		shapeStyle.ShapeProperties.LineCap = EndLineCap.rnd;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "phClr";
		m_defaultChartStyleElements.Add(ChartStyleElements.trendline, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.BorderWeight = 9525.0;
		shapeStyle.ShapeProperties.LineCap = EndLineCap.flat;
		shapeStyle.ShapeProperties.BorderLineStyle = Excel2007ShapeLineStyle.sng;
		shapeStyle.ShapeProperties.IsInsetPenAlignment = false;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.schemeClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "tx1";
		shapeStyle.ShapeProperties.BorderFillLumModValue = 15000.0;
		shapeStyle.ShapeProperties.BorderFillLumOffValue1 = 85000.0;
		shapeStyle.ShapeProperties.BorderIsRound = true;
		shapeStyle.DefaultRunParagraphProperties = new TextSettings();
		shapeStyle.DefaultRunParagraphProperties.FontSize = 9f;
		shapeStyle.DefaultRunParagraphProperties.KerningValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.SpacingValue = -1f;
		shapeStyle.DefaultRunParagraphProperties.Baseline = -1;
		m_defaultChartStyleElements.Add(ChartStyleElements.seriesAxis, shapeStyle);
		shapeStyle = new ShapeStyle("cs", nameSpaceValue, StyleEntryModifierEnum.none);
		shapeStyle.LineRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FillRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.EffectRefStyleEntry = new StyleOrFontReference(0, ColorModel.none, "", -1.0, -1.0, -1.0, -1.0);
		shapeStyle.FontRefstyleEntry = new StyleOrFontReference(1, ColorModel.schemeClr, "tx1", 65000.0, 35000.0, -1.0, -1.0);
		shapeStyle.ShapeProperties = new StyleEntryShapeProperties();
		shapeStyle.ShapeProperties.BorderWeight = 9525.0;
		shapeStyle.ShapeProperties.LineCap = EndLineCap.flat;
		shapeStyle.ShapeProperties.BorderFillColorModelType = ColorModel.srgbClr;
		shapeStyle.ShapeProperties.BorderFillColorValue = "D9D9D9";
		shapeStyle.ShapeProperties.BorderIsRound = true;
		m_defaultChartStyleElements.Add(ChartStyleElements.seriesLine, shapeStyle);
	}

	private void InitializeChartColorElements()
	{
		m_defaultColorVariations = new double[8][];
		m_defaultColorVariations[0] = new double[1] { 60000.0 };
		m_defaultColorVariations[1] = new double[2] { 80000.0, 20000.0 };
		m_defaultColorVariations[2] = new double[1] { 80000.0 };
		m_defaultColorVariations[3] = new double[2] { 60000.0, 40000.0 };
		m_defaultColorVariations[4] = new double[1] { 50000.0 };
		m_defaultColorVariations[5] = new double[2] { 70000.0, 30000.0 };
		m_defaultColorVariations[6] = new double[1] { 70000.0 };
		m_defaultColorVariations[7] = new double[2] { 50000.0, 50000.0 };
	}

	internal void SerializeChartExFallBackShapeContent(XmlWriter writer, bool isChartSheet)
	{
		string ns = (isChartSheet ? "http://schemas.openxmlformats.org/drawingml/2006/chartDrawing" : "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		writer.WriteStartElement("sp", ns);
		Excel2007Serializator.SerializeAttribute(writer, "macro", "", null);
		Excel2007Serializator.SerializeAttribute(writer, "textlink", "", null);
		writer.WriteStartElement("nvSpPr", ns);
		writer.WriteStartElement("cNvPr", ns);
		writer.WriteAttributeString("id", "0");
		writer.WriteAttributeString("name", "");
		writer.WriteEndElement();
		writer.WriteStartElement("cNvSpPr", ns);
		writer.WriteStartElement("a", "spLocks", "http://schemas.openxmlformats.org/drawingml/2006/main");
		Excel2007Serializator.SerializeAttribute(writer, "noTextEdit", value: true, defaultValue: false);
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteStartElement("spPr", ns);
		if (isChartSheet)
		{
			DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/main", "http://schemas.openxmlformats.org/drawingml/2006/main", 0, 0, 8666049, 6293304);
		}
		else
		{
			DrawingShapeSerializator.SerializeForm(writer, "http://schemas.openxmlformats.org/drawingml/2006/main", "http://schemas.openxmlformats.org/drawingml/2006/main", 0, 0, 0, 0);
		}
		writer.WriteStartElement("prstGeom", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("prst", "rect");
		writer.WriteStartElement("avLst", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteStartElement("solidFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("prstClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", "white");
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteStartElement("ln", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("w", "1");
		writer.WriteStartElement("solidFill", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("prstClr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("val", "green");
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteStartElement("txBody", ns);
		writer.WriteStartElement("bodyPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("vertOverflow", "clip");
		writer.WriteAttributeString("horzOverflow", "clip");
		writer.WriteEndElement();
		writer.WriteStartElement("lstStyle", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteEndElement();
		writer.WriteStartElement("p", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("r", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteStartElement("rPr", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteAttributeString("lang", "en-US");
		writer.WriteAttributeString("sz", "1100");
		writer.WriteEndElement();
		writer.WriteStartElement("t", "http://schemas.openxmlformats.org/drawingml/2006/main");
		writer.WriteString("This chart isn't available in your version of Excel. Editing this shape or saving this workbook into a different file format will permanently break the chart.");
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private string SerializeChartSheetDrawing(ChartImpl chart, string drawingItemName, RelationCollection drawingRelations, out string dummyChartRelation)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		if (drawingRelations == null)
		{
			throw new ArgumentNullException();
		}
		if (drawingItemName == null || drawingItemName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("drawingItemName");
		}
		_ = chart.InnerShapesBase;
		dummyChartRelation = null;
		string text = drawingRelations.GenerateRelationId();
		drawingRelations[text] = null;
		string text2 = "";
		if (ChartImpl.IsChartExSerieType(chart.ChartType))
		{
			dummyChartRelation = drawingRelations.GenerateRelationId();
			text2 = ";" + dummyChartRelation;
			drawingRelations[dummyChartRelation] = null;
		}
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		new ChartSerializator().SerializeChartsheetDrawing(xmlWriter, chart, text + text2);
		xmlWriter.Flush();
		streamWriter.Flush();
		m_parentHolder.Archive.UpdateItem(drawingItemName, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		return text;
	}

	private void SerializeWorksheetRelations()
	{
		if (m_relations != null && m_relations.Count > 0)
		{
			string itemName = m_archiveItem.ItemName;
			int startIndex = itemName.LastIndexOf('/');
			string itemName2 = itemName.Insert(startIndex, "/_rels") + ".rels";
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
			m_parentHolder.Serializator.SerializeRelations(xmlWriter, m_relations, null);
			xmlWriter.Flush();
			streamWriter.Flush();
			m_parentHolder.Archive.UpdateItem(itemName2, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
	}

	[SecurityCritical]
	private void SerializeWorksheetDrawings(WorksheetBaseImpl sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		int count = sheet.Shapes.Count;
		if (count == 0 && sheet is IWorksheet && !sheet.UnknownVmlShapes)
		{
			if (m_strDrawingsId != null)
			{
				m_relations.Remove(m_strDrawingsId);
				m_strDrawingsId = null;
			}
		}
		else if (count != 0 || sheet.UnknownVmlShapes)
		{
			SerializeVmlDrawings(sheet);
			if (!SerializeDrawings(sheet) && m_strDrawingsId != null)
			{
				m_relations.Remove(m_strDrawingsId);
				m_strDrawingsId = null;
			}
		}
	}

	[SecurityCritical]
	public bool SerializeDrawings(WorksheetBaseImpl sheet)
	{
		return SerializeDrawings(sheet, Relations, ref m_strDrawingsId, "application/vnd.openxmlformats-officedocument.drawing+xml", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing");
	}

	[SecurityCritical]
	public bool SerializeDrawings(WorksheetBaseImpl sheet, RelationCollection relations, ref string id, string contentType, string relationType)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		ShapesCollection innerShapes = sheet.InnerShapes;
		int num = 0;
		if (innerShapes.Count - sheet.VmlShapesCount - num <= 0 && !Excel2007Serializator.HasAlternateContent(innerShapes))
		{
			return false;
		}
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		m_parentHolder.Serializator.SerializeDrawings(xmlWriter, innerShapes, this);
		xmlWriter.Flush();
		streamWriter.Flush();
		memoryStream.Flush();
		string text = GenerateDrawingsName(sheet.Workbook);
		m_parentHolder.Archive.UpdateItem(text, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		string text2 = "/" + text;
		relations[id] = new Relation(text2, relationType);
		m_parentHolder.OverriddenContentTypes[text2] = contentType;
		if (m_drawingsRelation != null && m_drawingsRelation.Count > 0)
		{
			SerializeRelations(m_drawingsRelation, text, null);
		}
		return true;
	}

	private void SerializeVmlDrawings(WorksheetBaseImpl sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		m_parentHolder.DefaultContentTypes["vml"] = "application/vnd.openxmlformats-officedocument.vmlDrawing";
		Excel2007Serializator serializator = m_parentHolder.Serializator;
		string text = GenerateVmlDrawingsName();
		RelationCollection relationCollection = new RelationCollection();
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter writer = UtilityMethods.CreateWriter(streamWriter, indent: true);
		serializator.SerializeVmlShapes(writer, sheet.InnerShapes, this, serializator.VmlSerializators, relationCollection);
		streamWriter.Flush();
		memoryStream.Flush();
		m_parentHolder.Archive.UpdateItem(text, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		Relations[m_strVmlDrawingsId] = new Relation("/" + text, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/vmlDrawing");
		SerializeRelations(relationCollection, text, null);
	}

	public void SerializeHeaderFooterImages(WorksheetBaseImpl sheet, RelationCollection relations)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		HeaderFooterShapeCollection innerHeaderFooterShapes = sheet.InnerHeaderFooterShapes;
		if (relations == null)
		{
			relations = Relations;
		}
		if (innerHeaderFooterShapes == null || innerHeaderFooterShapes.Count == 0)
		{
			if (m_strVmlHFDrawingsId != null)
			{
				relations.Remove(m_strVmlHFDrawingsId);
			}
			return;
		}
		m_parentHolder.DefaultContentTypes["vml"] = "application/vnd.openxmlformats-officedocument.vmlDrawing";
		Excel2007Serializator serializator = m_parentHolder.Serializator;
		string text = GenerateVmlDrawingsName();
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter writer = UtilityMethods.CreateWriter(streamWriter, indent: true);
		serializator.SerializeVmlShapes(writer, innerHeaderFooterShapes, this, serializator.HFVmlSerializators, relations);
		streamWriter.Flush();
		memoryStream.Flush();
		m_parentHolder.Archive.UpdateItem(text, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		relations[m_strVmlHFDrawingsId] = new Relation("/" + text, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/vmlDrawing");
		SerializeRelations(m_hfDrawingsRelation, text, null);
	}

	public void SerializeRelations(string strParentItemName)
	{
		if (m_relations != null && m_relations.Count > 0)
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
			m_parentHolder.Serializator.SerializeRelations(xmlWriter, m_relations, null);
			xmlWriter.Flush();
			streamWriter.Flush();
			memoryStream.Flush();
			int startIndex = strParentItemName.LastIndexOf('/');
			string itemName = strParentItemName.Insert(startIndex, "/_rels") + ".rels";
			m_parentHolder.Archive.UpdateItem(itemName, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
	}

	public void SerializeRelations(RelationCollection relations, string strParentItemName, WorksheetDataHolder holder)
	{
		if (strParentItemName == null || strParentItemName.Length == 0)
		{
			throw new ArgumentOutOfRangeException(strParentItemName);
		}
		if (relations != null && relations.Count > 0)
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
			m_parentHolder.Serializator.SerializeRelations(xmlWriter, relations, holder);
			xmlWriter.Flush();
			streamWriter.Flush();
			memoryStream.Flush();
			int num = strParentItemName.LastIndexOf('/');
			if (strParentItemName[0] == '/')
			{
				strParentItemName = UtilityMethods.RemoveFirstCharUnsafe(strParentItemName);
				num--;
			}
			string itemName = strParentItemName.Insert(num, "/_rels") + ".rels";
			m_parentHolder.Archive.UpdateItem(itemName, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		}
	}

	private void SerializeComments(WorksheetImpl sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		string text = GenerateCommentsName();
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream);
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(streamWriter);
		m_parentHolder.Serializator.SerializeCommentNotes(xmlWriter, sheet);
		xmlWriter.Flush();
		streamWriter.Flush();
		memoryStream.Flush();
		m_parentHolder.Archive.UpdateItem(text, memoryStream, controlStream: true, DocGen.Compression.FileAttributes.Archive);
		Relations[m_strCommentsId] = new Relation("/" + text, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/comments");
		m_parentHolder.OverriddenContentTypes["/" + text] = "application/vnd.openxmlformats-officedocument.spreadsheetml.comments+xml";
	}

	private string GenerateDrawingsName(IWorkbook workbook)
	{
		string text;
		if (workbook.Saved)
		{
			text = $"xl/drawings/drawing{++m_parentHolder.LastDrawingIndex}.xml";
		}
		else
		{
			do
			{
				text = $"xl/drawings/drawing{++m_parentHolder.LastDrawingIndex}.xml";
			}
			while (m_parentHolder.Archive.Find(text) != -1);
		}
		return text;
	}

	private string GenerateVmlDrawingsName()
	{
		return $"xl/drawings/vmlDrawing{++m_parentHolder.LastVmlIndex}.vml";
	}

	private string GenerateCommentsName()
	{
		return $"xl/comments{++m_parentHolder.LastCommentIndex}.xml";
	}

	internal void AssignDrawingrelation(RelationCollection relation)
	{
		m_drawingsRelation = relation;
	}

	public WorksheetDataHolder Clone(FileDataHolder dataHolder)
	{
		WorksheetDataHolder worksheetDataHolder = (WorksheetDataHolder)MemberwiseClone();
		worksheetDataHolder.m_parentHolder = dataHolder;
		if (m_archiveItem != null)
		{
			worksheetDataHolder.m_archiveItem = dataHolder.Archive[m_archiveItem.ItemName];
		}
		worksheetDataHolder.m_relations = (RelationCollection)CloneUtils.CloneCloneable(m_relations);
		worksheetDataHolder.m_drawingsRelation = (RelationCollection)CloneUtils.CloneCloneable(m_drawingsRelation);
		worksheetDataHolder.m_hfDrawingsRelation = (RelationCollection)CloneUtils.CloneCloneable(m_hfDrawingsRelation);
		if (m_cfStream != null)
		{
			byte[] array = new byte[m_cfStream.Length];
			m_cfStream.Position = 0L;
			m_cfStream.Read(array, 0, array.Length);
			m_cfStream.Position = 0L;
			worksheetDataHolder.m_cfStream = new MemoryStream(array);
		}
		if (m_cfsStream != null)
		{
			byte[] array2 = new byte[m_cfsStream.Length];
			m_cfsStream.Position = 0L;
			m_cfsStream.Read(array2, 0, array2.Length);
			m_cfsStream.Position = 0L;
			worksheetDataHolder.m_cfsStream = new MemoryStream(array2);
		}
		worksheetDataHolder.m_startStream = new MemoryStream(CloneUtils.CloneByteArray(m_startStream.ToArray()));
		if (m_streamControls != null)
		{
			worksheetDataHolder.m_streamControls = new MemoryStream(CloneUtils.CloneByteArray((m_streamControls as MemoryStream).ToArray()));
		}
		return worksheetDataHolder;
	}

	public void Dispose()
	{
		m_archiveItem = null;
		m_cfStream = null;
		m_cfsStream = null;
		if (m_startStream != null)
		{
			m_startStream.Dispose();
			m_startStream = null;
		}
		if (m_streamControls != null)
		{
			m_streamControls.Dispose();
			m_streamControls = null;
		}
		m_parentHolder.Dispose();
		if (m_drawingsRelation != null)
		{
			m_drawingsRelation.Dispose();
			m_drawingsRelation = null;
		}
		if (m_hfDrawingsRelation != null)
		{
			m_hfDrawingsRelation.Dispose();
			m_hfDrawingsRelation = null;
		}
		if (m_relations != null)
		{
			m_relations.Dispose();
			m_relations = null;
		}
		if (m_preservedPivotTable != null)
		{
			m_preservedPivotTable.Clear();
			m_preservedPivotTable = null;
		}
		GC.SuppressFinalize(this);
	}
}
