using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Compression;
using DocGen.Pdf.Exporting;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf;

internal class PdfOptimizer
{
	private PdfCompressionOptions m_options;

	private List<PdfReference> m_fontReferenceCollection = new List<PdfReference>();

	private List<PdfReference> m_imageReferenceCollecction = new List<PdfReference>();

	private List<string> TtTableList = new List<string>();

	private List<PdfReference> m_xobjectReferenceCollection = new List<PdfReference>();

	private List<PdfReference> m_compressedXobject = new List<PdfReference>();

	private PdfLoadedDocument m_loadedDocument;

	private List<string> m_usedFontList = new List<string>();

	private Dictionary<string, IPdfPrimitive> m_resourceCollection = new Dictionary<string, IPdfPrimitive>();

	private bool m_identicalResource;

	private List<PdfReference> m_sMaskReference = new List<PdfReference>();

	private Dictionary<string, List<string>> fontUsedText = new Dictionary<string, List<string>>();

	private Dictionary<string, IPdfPrimitive> m_removedFontCollection = new Dictionary<string, IPdfPrimitive>();

	private string baseFont = string.Empty;

	private bool isPdfPage;

	internal PdfOptimizer(PdfLoadedDocument loadedDocument, PdfCompressionOptions options)
	{
		m_options = options;
		TtTableList.Add("OS/2");
		TtTableList.Add("cmap");
		TtTableList.Add("cvt ");
		TtTableList.Add("fpgm");
		TtTableList.Add("glyf");
		TtTableList.Add("head");
		TtTableList.Add("hhea");
		TtTableList.Add("hmtx");
		TtTableList.Add("loca");
		TtTableList.Add("maxp");
		TtTableList.Add("name");
		TtTableList.Add("post");
		TtTableList.Add("prep");
		Optimize(loadedDocument);
	}

	internal PdfOptimizer()
	{
		TtTableList.Add("OS/2");
		TtTableList.Add("cmap");
		TtTableList.Add("cvt ");
		TtTableList.Add("fpgm");
		TtTableList.Add("glyf");
		TtTableList.Add("head");
		TtTableList.Add("hhea");
		TtTableList.Add("hmtx");
		TtTableList.Add("loca");
		TtTableList.Add("maxp");
		TtTableList.Add("name");
		TtTableList.Add("post");
		TtTableList.Add("prep");
	}

	internal void Optimize(PdfLoadedDocument lDoc)
	{
		m_loadedDocument = lDoc;
		lDoc.FileStructure.IncrementalUpdate = false;
		lDoc.isCompressed = true;
		if (m_options.CompressImages || m_options.OptimizeFont || m_options.OptimizePageContents)
		{
			FindIdenticalResoucres(lDoc);
			foreach (PdfPageBase page in lDoc.Pages)
			{
				if (m_options.OptimizePageContents)
				{
					OptimizePageContent(page);
				}
				OptimizePageResources(page);
				if (m_options.OptimizePageContents && page is PdfLoadedPage)
				{
					OptimizeAnnotations(page as PdfLoadedPage);
				}
			}
		}
		if (m_options.RemoveMetadata)
		{
			RemoveMetaData(lDoc.Catalog);
		}
		m_sMaskReference.Clear();
		fontUsedText.Clear();
		m_removedFontCollection.Clear();
		if (lDoc != null && lDoc.Catalog != null && lDoc.Catalog.ContainsKey("AcroForm") && PdfCrossTable.Dereference(lDoc.Catalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Fields") && PdfCrossTable.Dereference(pdfDictionary["Fields"]) is PdfArray { Count: 0 } && !pdfDictionary.ContainsKey("XFA"))
		{
			int num = lDoc.PdfObjects.IndexOf(pdfDictionary);
			pdfDictionary.Clear();
			lDoc.Catalog.Remove("AcroForm");
			if (num != -1)
			{
				lDoc.PdfObjects.Remove(num);
			}
		}
	}

	internal void Close()
	{
		m_fontReferenceCollection.Clear();
		m_imageReferenceCollecction.Clear();
	}

	private void OptimizeAnnotations(PdfLoadedPage lPage)
	{
		PdfArray pdfArray = null;
		if (!lPage.Dictionary.ContainsKey("Annots"))
		{
			return;
		}
		pdfArray = lPage.CrossTable.GetObject(lPage.Dictionary["Annots"]) as PdfArray;
		_ = lPage.Document;
		if (pdfArray != null)
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				PdfDictionary widgetDictionary = lPage.CrossTable.GetObject(pdfArray[i]) as PdfDictionary;
				OptimizeApperance(widgetDictionary);
			}
		}
	}

	private void FindIdenticalResoucres(PdfLoadedDocument pdfLoaded)
	{
		PdfReferenceHolder pdfReferenceHolder = null;
		foreach (PdfPageBase page in pdfLoaded.Pages)
		{
			if (!page.Dictionary.ContainsKey("Resources"))
			{
				continue;
			}
			PdfReferenceHolder pdfReferenceHolder2 = page.Dictionary["Resources"] as PdfReferenceHolder;
			if (pdfReferenceHolder2 != null && pdfReferenceHolder == null)
			{
				pdfReferenceHolder = pdfReferenceHolder2;
				continue;
			}
			if (pdfReferenceHolder2 != null && pdfReferenceHolder != null && pdfReferenceHolder2.Reference == pdfReferenceHolder.Reference)
			{
				m_identicalResource = true;
			}
			break;
		}
	}

	private void OptimizeApperance(PdfDictionary widgetDictionary)
	{
		if (m_options.OptimizePageContents && widgetDictionary != null && widgetDictionary.ContainsKey("AP") && GetObject(widgetDictionary, new PdfName("AP")) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N") && GetObject(pdfDictionary["N"]) is PdfStream contentStream)
		{
			OptimizeContent(contentStream, isPageContent: false);
		}
	}

	private void OptimizePageResources(PdfPageBase lPage)
	{
		PdfResources resources = lPage.GetResources();
		OptimizeResources(resources, lPage);
	}

	private void OptimizePageContent(PdfPageBase lPage)
	{
		if (lPage is PdfPage)
		{
			isPdfPage = true;
		}
		_ = lPage.Contents;
		MemoryStream memoryStream = new MemoryStream();
		lPage.Layers.isOptimizeContent = true;
		lPage.Layers.CombineContent(memoryStream);
		memoryStream = OptimizeContent(memoryStream);
		PdfStream pdfStream = new PdfStream();
		pdfStream.Data = memoryStream.ToArray();
		pdfStream.Compress = true;
		memoryStream.Dispose();
		if (lPage.Dictionary.ContainsKey("Contents"))
		{
			if (lPage.Dictionary["Contents"] is PdfArray pdfArray)
			{
				foreach (IPdfPrimitive content in lPage.Contents)
				{
					PdfDictionary @object = GetObject(content);
					if (@object != null)
					{
						@object.isSkip = true;
					}
				}
				pdfArray.Clear();
				pdfArray.Add(new PdfReferenceHolder(pdfStream));
			}
			else if (lPage.Dictionary["Contents"] is PdfStream pdfStream2)
			{
				pdfStream2.Clear();
				pdfStream2.Items.Remove(new PdfName("Length"));
				pdfStream2.Data = pdfStream.Data;
				pdfStream2.Compress = true;
			}
		}
		if (lPage.Dictionary.ContainsKey("PieceInfo"))
		{
			lPage.Dictionary.Remove("PieceInfo");
		}
		lPage.Layers.isOptimizeContent = false;
	}

	private MemoryStream OptimizeContent(MemoryStream contentStream)
	{
		if (contentStream != null && contentStream.Length > 0 && contentStream.Length > 1)
		{
			PdfRecordCollection pdfRecordCollection = new ContentParser(contentStream.ToArray()).ReadContent();
			PdfStream pdfStream = new PdfStream();
			string currentFont = null;
			int count = pdfRecordCollection.RecordCollection.Count;
			for (int i = 0; i < count; i++)
			{
				PdfRecord pdfRecord = pdfRecordCollection.RecordCollection[i];
				if (pdfRecord.Operands != null && pdfRecord.Operands.Length >= 1)
				{
					if (m_options.OptimizeFont && pdfRecord.OperatorName == "Tf")
					{
						if (!m_usedFontList.Contains(pdfRecord.Operands[0].TrimStart('/')))
						{
							m_usedFontList.Add(pdfRecord.Operands[0].TrimStart('/'));
						}
						currentFont = pdfRecord.Operands[0].TrimStart('/');
					}
					if (m_options.OptimizeFont && (pdfRecord.OperatorName == "Tj" || pdfRecord.OperatorName == "TJ" || pdfRecord.OperatorName == "'"))
					{
						AddUsedFontText(currentFont, pdfRecord);
					}
					if (pdfRecord.OperatorName == "ID")
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int j = 0; j < pdfRecord.Operands.Length; j++)
						{
							if (j + 1 < pdfRecord.Operands.Length && pdfRecord.Operands[j].Contains("/") && pdfRecord.Operands[j + 1].Contains("/"))
							{
								stringBuilder.Append(pdfRecord.Operands[j]);
								stringBuilder.Append(" ");
								stringBuilder.Append(pdfRecord.Operands[j + 1]);
								stringBuilder.Append("\r\n");
								j++;
							}
							else if (j + 1 < pdfRecord.Operands.Length && pdfRecord.Operands[j].Contains("/"))
							{
								stringBuilder.Append(pdfRecord.Operands[j]);
								stringBuilder.Append(" ");
								stringBuilder.Append(pdfRecord.Operands[j + 1]);
								stringBuilder.Append("\r\n");
								j++;
							}
							else
							{
								stringBuilder.Append(pdfRecord.Operands[j]);
								stringBuilder.Append("\r\n");
							}
						}
						string s = stringBuilder.ToString();
						byte[] bytes = Encoding.Default.GetBytes(s);
						pdfStream.Write(bytes);
					}
					else
					{
						for (int k = 0; k < pdfRecord.Operands.Length; k++)
						{
							string text = pdfRecord.Operands[k];
							if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
							{
								text = TrimOperand(text);
							}
							PdfString pdfString = new PdfString(text);
							pdfStream.Write(pdfString.Bytes);
							if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
							{
								pdfStream.Write(" ");
							}
						}
					}
				}
				else if (pdfRecord.Operands == null && pdfRecord.InlineImageBytes != null)
				{
					string @string = Encoding.Default.GetString(pdfRecord.InlineImageBytes);
					byte[] bytes2 = Encoding.Default.GetBytes(@string);
					pdfStream.Write(bytes2);
					pdfStream.Write(" ");
				}
				pdfStream.Write(pdfRecord.OperatorName);
				if (i + 1 < count || isPdfPage)
				{
					if (pdfRecord.OperatorName == "ID")
					{
						pdfStream.Write("\n");
					}
					else if (i + 1 < count && (pdfRecord.OperatorName == "W" || pdfRecord.OperatorName == "W*") && pdfRecordCollection.RecordCollection[i + 1].OperatorName == "n")
					{
						pdfStream.Write(" ");
					}
					else if (pdfRecord.OperatorName == "w" || pdfRecord.OperatorName == "EI")
					{
						pdfStream.Write(" ");
					}
					else
					{
						pdfStream.Write("\r\n");
					}
				}
			}
			if (pdfStream.Data.Length < contentStream.Length)
			{
				contentStream.Close();
				contentStream = new MemoryStream();
				byte[] array = new byte[4096];
				pdfStream.InternalStream.Position = 0L;
				int count2;
				while ((count2 = pdfStream.InternalStream.Read(array, 0, array.Length)) > 0)
				{
					contentStream.Write(array, 0, count2);
				}
				pdfStream.Clear();
				pdfStream.InternalStream.Dispose();
				pdfStream.InternalStream.Close();
				pdfStream.InternalStream = null;
			}
		}
		return contentStream;
	}

	private void OptimizeContent(PdfStream contentStream, bool isPageContent)
	{
		if (contentStream == null || contentStream.Data.Length == 0)
		{
			return;
		}
		contentStream.Decompress();
		if (contentStream.Data.Length <= 1)
		{
			return;
		}
		PdfRecordCollection pdfRecordCollection = new ContentParser(contentStream.InternalStream.ToArray()).ReadContent();
		PdfStream pdfStream = new PdfStream();
		string text = null;
		int count = pdfRecordCollection.RecordCollection.Count;
		for (int i = 0; i < count; i++)
		{
			PdfRecord pdfRecord = pdfRecordCollection.RecordCollection[i];
			if (pdfRecord.Operands != null && pdfRecord.Operands.Length >= 1)
			{
				if (m_options.OptimizeFont && pdfRecord.OperatorName == "Tf")
				{
					if (!m_usedFontList.Contains(pdfRecord.Operands[0].TrimStart('/')))
					{
						m_usedFontList.Add(pdfRecord.Operands[0].TrimStart('/'));
					}
					text = pdfRecord.Operands[0].TrimStart('/');
				}
				if (m_options.OptimizeFont && (pdfRecord.OperatorName == "Tj" || pdfRecord.OperatorName == "TJ" || pdfRecord.OperatorName == "'") && text != null)
				{
					AddUsedFontText(text, pdfRecord);
				}
				if (pdfRecord.OperatorName == "ID")
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int j = 0; j < pdfRecord.Operands.Length; j++)
					{
						if (j + 1 < pdfRecord.Operands.Length && pdfRecord.Operands[j].Contains("/") && pdfRecord.Operands[j + 1].Contains("/"))
						{
							stringBuilder.Append(pdfRecord.Operands[j]);
							stringBuilder.Append(" ");
							stringBuilder.Append(pdfRecord.Operands[j + 1]);
							stringBuilder.Append("\r\n");
							j++;
						}
						else if (j + 1 < pdfRecord.Operands.Length && pdfRecord.Operands[j].Contains("/"))
						{
							stringBuilder.Append(pdfRecord.Operands[j]);
							stringBuilder.Append(" ");
							stringBuilder.Append(pdfRecord.Operands[j + 1]);
							stringBuilder.Append("\r\n");
							j++;
						}
						else
						{
							stringBuilder.Append(pdfRecord.Operands[j]);
							stringBuilder.Append("\r\n");
						}
					}
					string s = stringBuilder.ToString();
					byte[] bytes = Encoding.Default.GetBytes(s);
					pdfStream.Write(bytes);
				}
				else
				{
					for (int k = 0; k < pdfRecord.Operands.Length; k++)
					{
						string text2 = pdfRecord.Operands[k];
						if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
						{
							text2 = TrimOperand(text2);
						}
						PdfString pdfString = new PdfString(text2);
						pdfStream.Write(pdfString.Bytes);
						if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
						{
							pdfStream.Write(" ");
						}
					}
				}
			}
			else if (pdfRecord.Operands == null && pdfRecord.InlineImageBytes != null)
			{
				string @string = Encoding.Default.GetString(pdfRecord.InlineImageBytes);
				byte[] bytes2 = Encoding.Default.GetBytes(@string);
				pdfStream.Write(bytes2);
				pdfStream.Write(" ");
			}
			pdfStream.Write(pdfRecord.OperatorName);
			if (i + 1 < count || isPdfPage)
			{
				if (pdfRecord.OperatorName == "ID")
				{
					pdfStream.Write("\n");
				}
				else if (i + 1 < count && (pdfRecord.OperatorName == "W" || pdfRecord.OperatorName == "W*") && pdfRecordCollection.RecordCollection[i + 1].OperatorName == "n")
				{
					pdfStream.Write(" ");
				}
				else if (pdfRecord.OperatorName == "w" || pdfRecord.OperatorName == "EI")
				{
					pdfStream.Write(" ");
				}
				else
				{
					pdfStream.Write("\r\n");
				}
			}
		}
		if (isPageContent || pdfStream.Data.Length < contentStream.Data.Length)
		{
			contentStream.Clear();
			contentStream.Items.Remove(new PdfName("Length"));
			contentStream.Data = pdfStream.Data;
			pdfStream.Clear();
			pdfStream.InternalStream.Dispose();
			pdfStream.InternalStream.Close();
			pdfStream.InternalStream = null;
		}
		contentStream.Compress = true;
	}

	private string TrimOperand(string operand)
	{
		if (operand == ".00")
		{
			operand = "0";
		}
		if (operand.Contains(".00"))
		{
			string[] array = operand.Split(new char[1] { '.' });
			if (array.Length == 2 && array[1] == "00" && array[0] != "-")
			{
				operand = array[0];
			}
		}
		return operand;
	}

	private void RemoveMetaData(PdfCatalog catalog)
	{
		if (catalog.ContainsKey("Metadata"))
		{
			PdfDictionary pdfDictionary = catalog["Metadata"] as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = (catalog["Metadata"] as PdfReferenceHolder).Object as PdfDictionary;
			}
			if (pdfDictionary != null)
			{
				pdfDictionary.isSkip = true;
			}
			catalog.Remove(new PdfName("Metadata"));
		}
	}

	private void OptimizeResources(PdfDictionary resource, PdfPageBase lPage)
	{
		if (resource.ContainsKey("Font") && m_options.OptimizeFont)
		{
			OptimizeFont(resource, lPage);
		}
		if (resource.ContainsKey("XObject"))
		{
			OptimizeXObect(resource, lPage);
		}
		if (resource.ContainsKey("Resources") && GetObject(resource, new PdfName("Resources")) is PdfDictionary resource2)
		{
			OptimizeResources(resource2, lPage);
		}
	}

	private void OptimizeFont(PdfDictionary resource, PdfPageBase lPage)
	{
		if (!(GetObject(resource, new PdfName("Font")) is PdfDictionary pdfDictionary))
		{
			return;
		}
		Dictionary<PdfName, IPdfPrimitive> dictionary = new Dictionary<PdfName, IPdfPrimitive>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
		{
			dictionary.Add(item.Key, item.Value);
		}
		bool flag = false;
		if (!m_identicalResource && m_removedFontCollection.Count > 0 && m_options.OptimizePageContents && dictionary.Count < m_usedFontList.Count)
		{
			PdfReferenceHolder pdfReferenceHolder = resource["Font"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
			{
				foreach (string usedFont in m_usedFontList)
				{
					string key = usedFont + "_" + pdfReferenceHolder.Reference.ObjNum;
					if (!dictionary.ContainsKey(new PdfName(usedFont)) && m_removedFontCollection.ContainsKey(key))
					{
						IPdfPrimitive value = m_removedFontCollection[key];
						pdfDictionary[new PdfName(usedFont)] = value;
						flag = true;
						m_removedFontCollection.Remove(key);
					}
				}
			}
		}
		if (flag)
		{
			dictionary.Clear();
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary.Items)
			{
				dictionary.Add(item2.Key, item2.Value);
			}
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item3 in dictionary)
		{
			string text = PdfName.DecodeName(item3.Key.Value);
			if (((m_options.OptimizePageContents && m_usedFontList.Contains(item3.Key.Value)) || m_usedFontList.Contains(text) || !m_options.OptimizePageContents) && item3.Value is PdfReferenceHolder)
			{
				PdfReferenceHolder pdfReferenceHolder2 = item3.Value as PdfReferenceHolder;
				if (m_fontReferenceCollection.Contains(pdfReferenceHolder2.Reference))
				{
					continue;
				}
				m_fontReferenceCollection.Add(pdfReferenceHolder2.Reference);
				PdfDictionary pdfDictionary2 = pdfReferenceHolder2.Object as PdfDictionary;
				PdfStream pdfStream = null;
				PdfCrossTable pdfCrossTable = ((lPage is PdfPage) ? (lPage as PdfPage).CrossTable : (lPage as PdfLoadedPage).CrossTable);
				pdfCrossTable.GetObject(pdfDictionary2["BaseFont"]);
				string text2 = string.Empty;
				if (pdfDictionary2.ContainsKey("Subtype"))
				{
					PdfName pdfName = pdfCrossTable.GetObject(pdfDictionary2["Subtype"]) as PdfName;
					if (pdfName.Value == "TrueType")
					{
						text2 = "TrueType";
						PdfReferenceHolder pdfReferenceHolder3 = pdfDictionary2["FontDescriptor"] as PdfReferenceHolder;
						if (pdfReferenceHolder3 != null && pdfReferenceHolder3.Object is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("FontFile2"))
						{
							PdfReferenceHolder pdfReferenceHolder4 = pdfDictionary3["FontFile2"] as PdfReferenceHolder;
							if (pdfReferenceHolder4 != null)
							{
								pdfStream = pdfReferenceHolder4.Object as PdfStream;
							}
						}
					}
					else if (pdfName.Value == "Type0")
					{
						text2 = "Type0";
						PdfArray pdfArray = pdfDictionary2["DescendantFonts"] as PdfArray;
						if (pdfArray == null)
						{
							PdfReferenceHolder pdfReferenceHolder5 = pdfDictionary2["DescendantFonts"] as PdfReferenceHolder;
							if (pdfReferenceHolder5 != null)
							{
								pdfArray = pdfReferenceHolder5.Object as PdfArray;
							}
						}
						PdfDictionary pdfDictionary4 = ((pdfArray[0] is PdfReferenceHolder) ? ((pdfArray[0] as PdfReferenceHolder).Object as PdfDictionary) : (pdfArray[0] as PdfDictionary));
						if (pdfDictionary4 != null && PdfCrossTable.Dereference(pdfDictionary4["FontDescriptor"]) is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey("FontFile2"))
						{
							PdfReferenceHolder pdfReferenceHolder6 = pdfDictionary5["FontFile2"] as PdfReferenceHolder;
							if (pdfReferenceHolder6 != null && lPage is PdfLoadedPage)
							{
								pdfStream = pdfReferenceHolder6.Object as PdfStream;
							}
						}
					}
				}
				if (pdfStream == null)
				{
					continue;
				}
				if (text2 == "Type0")
				{
					if (m_options.OptimizeFont)
					{
						pdfStream.Decompress();
						if (pdfStream.Data.Length != 0)
						{
							FontFile2 ff = new FontFile2(pdfStream.Data);
							OptimizeType0Font(item3.Key.Value, ff, pdfStream, pdfDictionary[item3.Key] as PdfReferenceHolder);
						}
					}
				}
				else if (text2 == "TrueType" && m_options.OptimizeFont)
				{
					pdfStream.Decompress();
					FontFile2 fontFile = new FontFile2(pdfStream.Data);
					if (fontFile.tableList.Contains("glyf") && fontFile.tableList.Contains("loca"))
					{
						MemoryStream memoryStream = OptimizeTrueTypeFont(fontFile) as MemoryStream;
						UpdateFontData(memoryStream, pdfStream);
						memoryStream.Dispose();
					}
					else
					{
						pdfStream.Compress = true;
					}
				}
			}
			else if (!m_identicalResource)
			{
				PdfReferenceHolder pdfReferenceHolder7 = resource["Font"] as PdfReferenceHolder;
				if (pdfReferenceHolder7 != null && pdfReferenceHolder7.Reference != null)
				{
					m_removedFontCollection[item3.Key.Value + "_" + pdfReferenceHolder7.Reference.ObjNum] = pdfDictionary[text];
				}
				pdfDictionary.Remove(item3.Key);
			}
		}
	}

	private void OptimizeXObect(PdfDictionary resource, PdfPageBase lPage)
	{
		if (!(GetObject(resource, new PdfName("XObject")) is PdfDictionary pdfDictionary))
		{
			return;
		}
		Dictionary<PdfName, IPdfPrimitive> dictionary = new Dictionary<PdfName, IPdfPrimitive>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
		{
			dictionary.Add(item.Key, item.Value);
		}
		GetSmaskReference(dictionary, lPage);
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in dictionary)
		{
			PdfDictionary @object = GetObject(item2.Value);
			if (@object == null || !@object.ContainsKey("Subtype"))
			{
				continue;
			}
			PdfName pdfName = @object["Subtype"] as PdfName;
			if (pdfName.Value == "Image" && m_options.CompressImages)
			{
				bool flag = false;
				if (@object is PdfStream)
				{
					flag = (@object as PdfStream).isCustomQuality;
				}
				if (m_options.CompressImages && !flag)
				{
					PdfReference reference = ((lPage is PdfPage) ? (lPage as PdfPage).CrossTable : (lPage as PdfLoadedPage).CrossTable).GetReference(item2.Value);
					ReplaceImageNet(lPage, item2.Key, reference, @object, pdfDictionary);
				}
				if (!(@object is PdfStream pdfStream))
				{
					continue;
				}
				string hashValue = string.Empty;
				if (pdfStream.InternalStream.CanWrite && CompareStream(pdfStream.InternalStream, out hashValue))
				{
					@object = m_resourceCollection[hashValue] as PdfDictionary;
					if (@object != null && @object.ContainsKey("SMask") && pdfStream.ContainsKey("SMask"))
					{
						string text = string.Empty;
						string text2 = string.Empty;
						PdfStream pdfStream2 = PdfCrossTable.Dereference(pdfStream["SMask"]) as PdfStream;
						PdfStream pdfStream3 = PdfCrossTable.Dereference(@object["SMask"]) as PdfStream;
						if (pdfStream2 != null && pdfStream3 != null)
						{
							pdfStream2.InternalStream.Position = 0L;
							text = CreateHashFromStream(pdfStream2.InternalStream.ToArray());
							pdfStream3.InternalStream.Position = 0L;
							text2 = CreateHashFromStream(pdfStream3.InternalStream.ToArray());
						}
						if (text != string.Empty && text2 != string.Empty && text == text2)
						{
							pdfDictionary.Items.Remove(item2.Key);
							pdfDictionary.Items.Add(item2.Key, new PdfReferenceHolder(@object));
						}
					}
					else if (@object != null && !pdfStream.ContainsKey("SMask") && !@object.ContainsKey("SMask"))
					{
						pdfDictionary.Items.Remove(item2.Key);
						pdfDictionary.Items.Add(item2.Key, new PdfReferenceHolder(@object));
					}
				}
				else if (hashValue != string.Empty)
				{
					m_resourceCollection.Add(hashValue, @object);
				}
			}
			else
			{
				if (!(pdfName.Value == "Form"))
				{
					continue;
				}
				if (m_options.OptimizePageContents)
				{
					bool flag2 = true;
					if (item2.Value is PdfReferenceHolder)
					{
						PdfReference reference2 = (item2.Value as PdfReferenceHolder).Reference;
						if (reference2 != null)
						{
							if (!m_compressedXobject.Contains(reference2))
							{
								m_compressedXobject.Add(reference2);
							}
							else
							{
								flag2 = false;
							}
						}
					}
					if (flag2)
					{
						PdfStream contentStream = @object as PdfStream;
						OptimizeContent(contentStream, isPageContent: false);
					}
				}
				bool flag3 = false;
				if (item2.Value is PdfReferenceHolder)
				{
					PdfReference reference3 = (item2.Value as PdfReferenceHolder).Reference;
					if (reference3 != null)
					{
						if (m_xobjectReferenceCollection.Contains(reference3))
						{
							flag3 = true;
						}
						else
						{
							m_xobjectReferenceCollection.Add(reference3);
						}
					}
				}
				if (!flag3)
				{
					OptimizeResources(@object, lPage);
				}
			}
		}
	}

	private FontFile2 UpdateFontStream(string fontKey, PdfReferenceHolder fontList, TtfReader ttfReader)
	{
		FontFile2 result = null;
		List<string> list = fontUsedText[fontKey];
		string text = string.Empty;
		FontStructure fontStructure = new FontStructure(fontList.Object, fontList.Reference.ToString());
		for (int i = 0; i < list.Count; i++)
		{
			text += fontStructure.Decode(list[i], isSameFont: false).Text;
		}
		if (text.Length > 0)
		{
			char[] array = text.ToCharArray();
			Dictionary<char, char> dictionary = new Dictionary<char, char>();
			foreach (char key in array)
			{
				dictionary[key] = '\0';
			}
			byte[] data = ttfReader.ReadFontProgram(dictionary);
			if (fontList.Object is PdfDictionary)
			{
				UpdateFontName(fontList.Object as PdfDictionary, ttfReader);
			}
			result = new FontFile2(data);
		}
		return result;
	}

	private void AddUsedFontText(string currentFont, PdfRecord record)
	{
		if (fontUsedText.ContainsKey(currentFont))
		{
			List<string> list = fontUsedText[currentFont];
			if (!list.Contains(record.Operands[0]))
			{
				list.Add(record.Operands[0]);
			}
		}
		else
		{
			List<string> list2 = new List<string>();
			list2.Add(record.Operands[0]);
			fontUsedText[currentFont] = list2;
		}
	}

	private void ReplaceImageNet(PdfPageBase lPage, PdfName key, PdfReference oldReference, PdfDictionary imgDict, PdfDictionary xObjectDictionary)
	{
		if (m_imageReferenceCollecction.Contains(oldReference) || m_sMaskReference.Contains(oldReference))
		{
			return;
		}
		m_imageReferenceCollecction.Add(oldReference);
		bool flag = false;
		if (imgDict is PdfStream)
		{
			flag = (imgDict as PdfStream).isImageDualFilter;
		}
		if (flag || ImageInterpolated(imgDict))
		{
			return;
		}
		ImageStructureNet imageStructureNet = new ImageStructureNet(imgDict, new PdfMatrix());
		PdfStream pdfStream = imageStructureNet.ImageDictionary as PdfStream;
		imageStructureNet.IsImageForExtraction = true;
		bool flag2 = false;
		PdfArray pdfArray = PdfCrossTable.Dereference(imgDict["Filter"]) as PdfArray;
		if (pdfArray != null && pdfArray.Count > 1)
		{
			flag2 = true;
		}
		else if (pdfArray == null)
		{
			PdfName pdfName = PdfCrossTable.Dereference(imgDict["Filter"]) as PdfName;
			if (pdfName != null)
			{
				flag2 = pdfName.Value == "DCTDecode";
			}
		}
		imageStructureNet.m_compressPDF = !flag2;
		if (!CheckSkipReplace(imageStructureNet, pdfStream))
		{
			MemoryStream embeddedImageStream = imageStructureNet.GetEmbeddedImageStream();
			if (embeddedImageStream != null && pdfStream != null && pdfStream.InternalStream != null && pdfStream.InternalStream.CanWrite)
			{
				PdfArray pdfArray2 = PdfCrossTable.Dereference(imgDict["ColorSpace"]) as PdfArray;
				PdfStream pdfStream2 = null;
				SKEncodedImageFormat encodedImageFormat = GetEncodedImageFormat(imageStructureNet.imageFormat);
				MemoryStream memoryStream = CompressImage(embeddedImageStream, encodedImageFormat);
				if (memoryStream != null)
				{
					memoryStream.Position = 0L;
					if (encodedImageFormat == SKEncodedImageFormat.Png)
					{
						pdfStream2 = GetPdfImageObject(memoryStream);
					}
					if (pdfStream2 == null)
					{
						PdfBitmap pdfBitmap = new PdfBitmap(memoryStream);
						pdfBitmap.Save();
						pdfStream2 = pdfBitmap.ImageStream;
					}
				}
				if (pdfStream2 != null && pdfStream2.Data.Length < (imgDict as PdfStream).Data.Length)
				{
					if (imgDict.ContainsKey("SMask"))
					{
						PdfDictionary parent = GetObject(xObjectDictionary, key) as PdfDictionary;
						if (GetObject(parent, new PdfName("SMask")) is PdfStream pdfStream3)
						{
							pdfStream3.isSkip = true;
						}
					}
					if (pdfArray2 != null)
					{
						PdfName pdfName2 = PdfCrossTable.Dereference(pdfArray2[0]) as PdfName;
						if (pdfStream2.ContainsKey("ColorSpace"))
						{
							PdfName pdfName3 = pdfStream2["ColorSpace"] as PdfName;
							if (pdfName2 != null && pdfName2.Value == "ICCBased" && pdfName3 != null && pdfName3.Value != "DeviceRGB")
							{
								pdfStream2["ColorSpace"] = pdfArray2;
							}
						}
						else if (pdfName2 != null && pdfName2.Value == "ICCBased")
						{
							pdfStream2["ColorSpace"] = pdfArray2;
						}
					}
					PdfReferenceHolder imageReference = new PdfReferenceHolder(pdfStream2);
					lPage.isFlateCompress = true;
					lPage.ReplaceImageStream(oldReference.ObjNum, imageReference, lPage);
					lPage.isFlateCompress = false;
				}
				else if (pdfStream2 != null && imageStructureNet.imageFormat.Equals(ImageFormat.Png) && imgDict.ContainsKey("Filter"))
				{
					bool flag3 = false;
					if (imgDict["Filter"] is PdfName)
					{
						flag3 = (imgDict["Filter"] as PdfName).Value == "FlateDecode" && !imgDict.ContainsKey("Interpolate");
					}
					if (flag3 && pdfStream2.ContainsKey("Filter") && IsDCTDecode(pdfStream2) && pdfStream2.ContainsKey("DecodeParms"))
					{
						PdfCompressionLevel level = m_loadedDocument.Compression;
						if (m_loadedDocument.isCompressed)
						{
							level = PdfCompressionLevel.Best;
						}
						byte[] array = new PdfZlibCompressor(level).Compress(pdfStream2.Data);
						if (array.Length < (imgDict as PdfStream).Data.Length)
						{
							if (imgDict.ContainsKey("SMask"))
							{
								PdfDictionary parent2 = GetObject(xObjectDictionary, key) as PdfDictionary;
								if (GetObject(parent2, new PdfName("SMask")) is PdfStream pdfStream4)
								{
									pdfStream4.isSkip = true;
								}
							}
							PdfReferenceHolder imageReference2 = new PdfReferenceHolder(pdfStream2);
							lPage.isFlateCompress = true;
							lPage.ReplaceImageStream(oldReference.ObjNum, imageReference2, lPage);
							lPage.isFlateCompress = false;
						}
						Array.Clear(array, 0, array.Length);
						array = null;
					}
				}
			}
		}
		imageStructureNet.m_compressPDF = false;
		imageStructureNet.m_decodedOriginalBitmap = null;
		imageStructureNet.m_decodedMaskBitmap = null;
		imageStructureNet.IsImageForExtraction = false;
	}

	private bool IsDCTDecode(PdfStream stream)
	{
		bool result = false;
		if (stream["Filter"] is PdfName)
		{
			result = "DCTDecode" == (stream["Filter"] as PdfName).Value;
		}
		else if (stream["Filter"] is PdfArray)
		{
			PdfArray pdfArray = stream["Filter"] as PdfArray;
			if (pdfArray.Elements.Count > 0 && pdfArray.Elements[0] is PdfName)
			{
				result = (pdfArray.Elements[0] as PdfName).Value == "DCTDecode";
			}
		}
		return result;
	}

	private SKEncodedImageFormat GetEncodedImageFormat(ImageFormat imageFormat)
	{
		SKEncodedImageFormat result = SKEncodedImageFormat.Png;
		switch (imageFormat)
		{
		case ImageFormat.Jpeg:
			result = SKEncodedImageFormat.Jpeg;
			break;
		case ImageFormat.Png:
			result = SKEncodedImageFormat.Png;
			break;
		}
		return result;
	}

	private bool CheckSkipReplace(ImageStructureNet imageStructure, PdfStream imgStream)
	{
		bool result = false;
		string[] imageFilter = imageStructure.ImageFilter;
		if (imageFilter != null)
		{
			string[] array = imageFilter;
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i])
				{
				case "DCTDecode":
					if (!(imageStructure.ColorSpace == "DeviceCMYK"))
					{
						continue;
					}
					break;
				case "CCITTFaxDecode":
				case "JBIG2Decode":
					break;
				default:
					continue;
				}
				result = true;
				break;
			}
			if ((imgStream.ContainsKey("SMask") || imgStream.ContainsKey("Mask")) && imageStructure.MaskStream != null && imageStructure.m_maskFilter != null && imageStructure.m_maskFilter.Length != 0)
			{
				array = imageStructure.m_maskFilter;
				for (int i = 0; i < array.Length; i++)
				{
					switch (array[i])
					{
					case "DCTDecode":
						if (!(imageStructure.ColorSpace == "DeviceCMYK"))
						{
							continue;
						}
						break;
					case "CCITTFaxDecode":
					case "JBIG2Decode":
						break;
					default:
						continue;
					}
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private MemoryStream CompressImage(MemoryStream input, SKEncodedImageFormat format)
	{
		try
		{
			input.Position = 0L;
			SKBitmap sKBitmap = SKBitmap.Decode(input.ToArray());
			if (sKBitmap == null)
			{
				return null;
			}
			int num = sKBitmap.Width * 75 / 100;
			int num2 = sKBitmap.Height * 75 / 100;
			if (num < 1 || num2 < 1)
			{
				return null;
			}
			SKImageInfo info = new SKImageInfo(num, num2, SKColorType.Rgba8888);
			MemoryStream memoryStream = new MemoryStream();
			using (SKSurface sKSurface = SKSurface.Create(info))
			{
				using SKPaint sKPaint = new SKPaint();
				sKPaint.IsAntialias = true;
				sKPaint.FilterQuality = SKFilterQuality.High;
				sKSurface.Canvas.DrawBitmap(sKBitmap, new SKRectI(0, 0, num, num2), sKPaint);
				sKSurface.Canvas.Flush();
				using SKImage sKImage = sKSurface.Snapshot();
				sKImage.Encode(format, m_options.ImageQuality).SaveTo(memoryStream);
			}
			return memoryStream;
		}
		catch
		{
			return null;
		}
	}

	private PdfStream GetPdfImageObject(MemoryStream input)
	{
		PdfStream pdfStream = new PdfStream();
		try
		{
			input.Position = 0L;
			SKBitmap sKBitmap = SKBitmap.Decode(input);
			if (sKBitmap == null)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream();
			sKBitmap.Encode(SKEncodedImageFormat.Jpeg, m_options.ImageQuality).SaveTo(memoryStream);
			pdfStream.Data = memoryStream.ToArray();
			pdfStream["Type"] = new PdfName("XObject");
			pdfStream["Subtype"] = new PdfName("Image");
			pdfStream["Width"] = new PdfNumber(sKBitmap.Width);
			pdfStream["Height"] = new PdfNumber(sKBitmap.Height);
			if (sKBitmap.Info.BitsPerPixel > 8)
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(8);
			}
			else
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(sKBitmap.Info.BitsPerPixel);
			}
			pdfStream["Decode"] = new PdfArray(new float[6] { 0f, 1f, 0f, 1f, 0f, 1f });
			pdfStream["ColorSpace"] = pdfStream.GetName("DeviceRGB");
			PdfArray pdfArray = new PdfArray();
			pdfArray.Add(new PdfName("DCTDecode"));
			pdfStream["Filter"] = pdfArray;
			if (sKBitmap.ColorType == SKColorType.Bgra1010102 || sKBitmap.ColorType == SKColorType.Bgra8888 || sKBitmap.ColorType == SKColorType.Rgba1010102 || sKBitmap.ColorType == SKColorType.Rgba16161616 || sKBitmap.ColorType == SKColorType.Rgba8888 || sKBitmap.ColorType == SKColorType.RgbaF16 || sKBitmap.ColorType == SKColorType.RgbaF16Clamped || sKBitmap.ColorType == SKColorType.RgbaF32 || sKBitmap.ColorType == SKColorType.Argb4444)
			{
				SetMask(pdfStream, sKBitmap);
			}
		}
		catch
		{
			return null;
		}
		return pdfStream;
	}

	private byte[] ExtractAlpha(byte[] data)
	{
		byte[] array = new byte[data.Length / 4];
		int num = 0;
		for (int i = 0; i < data.Length; i += 4)
		{
			array[num++] = data[i + 3];
		}
		return array;
	}

	private void SetMask(PdfStream imageStream, SKBitmap mask)
	{
		if (mask != null)
		{
			PdfStream pdfStream = new PdfStream();
			pdfStream.InternalStream = new MemoryStream(ExtractAlpha(mask.Bytes));
			pdfStream["Type"] = new PdfName("XObject");
			pdfStream["Subtype"] = new PdfName("Image");
			pdfStream["Width"] = new PdfNumber(mask.Width);
			pdfStream["Height"] = new PdfNumber(mask.Height);
			if (mask.Info.BitsPerPixel > 8)
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(8);
			}
			else
			{
				pdfStream["BitsPerComponent"] = new PdfNumber(mask.Info.BitsPerPixel);
			}
			pdfStream["ColorSpace"] = new PdfName("DeviceGray");
			imageStream.SetProperty("SMask", new PdfReferenceHolder(pdfStream));
		}
	}

	private bool ImageInterpolated(PdfDictionary imageDictionary)
	{
		bool flag = false;
		if (imageDictionary != null && imageDictionary.ContainsKey("Interpolate"))
		{
			flag = (imageDictionary["Interpolate"] as PdfBoolean).Value;
		}
		if (flag)
		{
			if (!imageDictionary.ContainsKey("SMask"))
			{
				return false;
			}
			if (PdfCrossTable.Dereference(imageDictionary["SMask"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Interpolate") && PdfCrossTable.Dereference(pdfDictionary["Interpolate"]) is PdfBoolean { Value: not false })
			{
				return false;
			}
		}
		return flag;
	}

	private Stream OptimizeTrueTypeFont(FontFile2 f2)
	{
		List<TableEntry> list = new List<TableEntry>();
		foreach (string table in f2.tableList)
		{
			switch (table)
			{
			case "OS/2":
			case "cmap":
			case "cvt ":
			case "fpgm":
			case "glyf":
			case "head":
			case "hhea":
			case "hmtx":
			case "loca":
			case "maxp":
			case "name":
			case "post":
			case "prep":
			{
				int tableID = f2.getTableID(table);
				byte[] tableBytes = f2.getTableBytes(tableID, isTrueType: true);
				if (tableBytes != null)
				{
					list.Add(GetTableEntry((table == "OS2") ? "OS/2" : table, tableBytes));
				}
				break;
			}
			}
		}
		MemoryStream memoryStream = new FontDecode().CreateFontStream(list);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private void OptimizeType0Font(string fontKey, FontFile2 ff2, PdfStream fontFile2, PdfReferenceHolder fontList)
	{
		FontFile2 fontFile3 = ff2;
		int localTableLength = 0;
		int missedGlyphs = 0;
		int num = 0;
		bool isSkip = false;
		bool flag = false;
		bool flag2 = false;
		bool isSymbol = false;
		bool flag3 = false;
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(fontList) as PdfDictionary;
		bool flag4 = false;
		bool flag5 = false;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("ToUnicode"))
		{
			flag4 = IsOptimizeUsedCharacters(pdfDictionary, fontFile2);
		}
		if (flag4 || ((fontFile3.tableList[0] == "DSIG" || fontFile3.tableList[0] == "GDEF" || fontFile3.tableList[0] == "LTSH") && fontFile3.tableList.Count > 13 && !fontFile3.tableList.Contains("EBLC") && !fontFile3.tableList.Contains("EBDT") && (fontFile3.tableList.Contains("PCLT") || !fontFile3.tableList.Contains("meta") || !fontFile3.tableList.Contains("LTSH"))))
		{
			BinaryReader binaryReader = null;
			binaryReader = ((!fontFile2.InternalStream.CanSeek) ? new BinaryReader(new MemoryStream(fontFile2.Data), TtfReader.Encoding) : new BinaryReader(fontFile2.InternalStream, TtfReader.Encoding));
			TtfReader ttfReader = new TtfReader(binaryReader);
			ttfReader.CreateInternals();
			if (ttfReader.Metrics.PostScriptName != null && !ttfReader.Metrics.IsSymbol && !ttfReader.Metrics.PostScriptName.Contains("Symbol"))
			{
				flag3 = true;
				FontStructure fontStructure = new FontStructure(fontList.Object, fontList.Reference.ToString());
				Dictionary<char, char> dictionary = new Dictionary<char, char>();
				Dictionary<double, string> characterMapTable = fontStructure.CharacterMapTable;
				Dictionary<int, OtfGlyphInfo> dictionary2 = new Dictionary<int, OtfGlyphInfo>();
				flag5 = ttfReader.isOTFFont();
				if (!baseFont.Contains("Arial") || !pdfDictionary.ContainsKey("ToUnicode") || fontFile3.tableList[5] != "LTSH")
				{
					if (characterMapTable.Count > 0)
					{
						if (flag5)
						{
							foreach (KeyValuePair<double, string> item in characterMapTable)
							{
								TtfGlyphInfo ttfGlyphInfo = ttfReader.ReadGlyph((int)item.Key, isOpenType: true);
								if (ttfGlyphInfo.Index > -1)
								{
									if (ttfGlyphInfo.CharCode != -1)
									{
										dictionary2[ttfGlyphInfo.Index] = new OtfGlyphInfo(ttfGlyphInfo.CharCode, ttfGlyphInfo.Index, ttfGlyphInfo.Width);
									}
									else
									{
										dictionary2[ttfGlyphInfo.Index] = new OtfGlyphInfo((int)item.Key, ttfGlyphInfo.Index, ttfGlyphInfo.Width);
									}
								}
							}
						}
						else
						{
							foreach (KeyValuePair<double, string> item2 in characterMapTable)
							{
								char key = item2.Value.ToCharArray()[0];
								dictionary[key] = '\0';
							}
						}
						byte[] array = null;
						array = ((!flag5) ? ttfReader.ReadFontProgram(dictionary) : ttfReader.ReadOpenTypeFontProgram(dictionary2));
						missedGlyphs = ttfReader.m_missedGlyphs;
						if (fontList.Object is PdfDictionary)
						{
							UpdateFontName(fontList.Object as PdfDictionary, ttfReader);
						}
						fontFile3 = new FontFile2(array);
					}
					else
					{
						PdfDictionary @object = GetObject(fontList);
						if (@object != null && !@object.ContainsKey("ToUnicode") && fontUsedText.ContainsKey(fontKey))
						{
							if (IsCIDToGIDMap(@object))
							{
								fontFile3 = UpdateFontStream(fontKey, fontList, ttfReader);
							}
							if (fontFile3 == null)
							{
								fontFile3 = ff2;
							}
						}
						else
						{
							isSymbol = true;
							num = GetLocalTableGlyph(fontList, ttfReader, out missedGlyphs);
						}
					}
				}
			}
			else
			{
				isSymbol = true;
				num = GetLocalTableGlyph(fontList, ttfReader, out missedGlyphs);
			}
			flag2 = ttfReader.m_bIsLocaShort;
			ttfReader.Close();
		}
		else if (fontFile3.tableList[0] == "cvt " || fontFile3.tableList[0] == "OS/2" || (fontFile3.tableList.Contains("EBLC") && fontFile3.tableList.Contains("EBDT")))
		{
			flag2 = fontFile3.Header.IndexToLocFormat == 0;
			BinaryReader binaryReader2 = null;
			binaryReader2 = ((!fontFile2.InternalStream.CanSeek) ? new BinaryReader(new MemoryStream(fontFile2.Data), TtfReader.Encoding) : new BinaryReader(fontFile2.InternalStream, TtfReader.Encoding));
			TtfReader ttfReader2 = new TtfReader(binaryReader2);
			ttfReader2.CreateInternals();
			num = GetLocalTableGlyph(fontList, ttfReader2, out missedGlyphs);
			if (num == 0)
			{
				PdfDictionary object2 = GetObject(fontList);
				if (object2 != null && !object2.ContainsKey("ToUnicode") && fontUsedText.ContainsKey(fontKey))
				{
					if (IsCIDToGIDMap(object2))
					{
						fontFile3 = UpdateFontStream(fontKey, fontList, ttfReader2);
					}
					if (fontFile3 == null)
					{
						fontFile3 = ff2;
					}
				}
				else if (object2 != null && !object2.ContainsKey("ToUnicode"))
				{
					flag = true;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			CalculateLocalTableLength(fontFile3, flag2, num, out localTableLength, out isSkip);
			if (!isSkip && localTableLength > 0)
			{
				if (missedGlyphs != 0)
				{
					localTableLength += missedGlyphs * ((!flag2) ? 1 : 2);
					int tableID = fontFile3.getTableID("loca");
					if (fontFile3.getTableBytes(tableID).Length >= localTableLength)
					{
						MemoryStream memoryStream = ResetFontTables(fontFile3, isSymbol, localTableLength, flag2) as MemoryStream;
						UpdateFontData(memoryStream, fontFile2);
						memoryStream.Dispose();
					}
					else
					{
						fontFile2.Compress = true;
					}
					return;
				}
				int num2 = 0;
				Dictionary<int, PdfName> dictionary3 = new Dictionary<int, PdfName>();
				if (fontFile2.Items.Count > 0)
				{
					foreach (PdfName key2 in fontFile2.Items.Keys)
					{
						dictionary3.Add(num2, key2);
						num2++;
					}
				}
				if ((dictionary3 != null && dictionary3.Count > 0 && dictionary3[0] != null && dictionary3[0].Value == "Length") || flag5)
				{
					MemoryStream memoryStream2 = ResetFontTables(fontFile3, isSymbol, localTableLength, flag2) as MemoryStream;
					UpdateFontData(memoryStream2, fontFile2);
					memoryStream2.Dispose();
				}
				else
				{
					fontFile2.Compress = true;
				}
			}
			else if (isSkip && flag3)
			{
				MemoryStream memoryStream3 = OptimizeTrueTypeFont(fontFile3) as MemoryStream;
				UpdateFontData(memoryStream3, fontFile2);
				memoryStream3.Dispose();
			}
			else
			{
				fontFile2.Compress = true;
			}
		}
		else
		{
			fontFile2.Compress = true;
		}
	}

	private int GetLocalTableGlyph(PdfReferenceHolder fontList, TtfReader ttfReader, out int missedGlyphs)
	{
		missedGlyphs = 0;
		FontStructure fontStructure = null;
		fontStructure = ((!(fontList.Reference != null)) ? new FontStructure(fontList.Object, null) : new FontStructure(fontList.Object, fontList.Reference.ToString()));
		if (fontStructure != null)
		{
			Dictionary<double, string> characterMapTable = fontStructure.CharacterMapTable;
			if (fontStructure.CharacterMapTable.Count > 0)
			{
				Dictionary<char, char> dictionary = new Dictionary<char, char>();
				double[] array = new double[characterMapTable.Count];
				int num = 0;
				foreach (KeyValuePair<double, string> item in characterMapTable)
				{
					array[num++] = item.Key;
					if (!string.IsNullOrEmpty(item.Value))
					{
						char key = item.Value.ToCharArray()[0];
						dictionary[key] = '\0';
					}
				}
				Array.Sort(array);
				if (ttfReader != null)
				{
					Dictionary<int, int> glyphChars = ttfReader.GetGlyphChars(dictionary);
					if (glyphChars.Count < dictionary.Count)
					{
						missedGlyphs = dictionary.Count - glyphChars.Count;
					}
				}
				return (int)array[^1];
			}
			return 0;
		}
		return 0;
	}

	internal byte[] OptimizeType0Font(MemoryStream fontData, Dictionary<char, char> usedChars)
	{
		int localTableGlyph = 0;
		if (usedChars.Count > 0)
		{
			int num = 0;
			double[] array = new double[usedChars.Count];
			foreach (KeyValuePair<char, char> usedChar in usedChars)
			{
				array[num++] = (int)usedChar.Key;
			}
			Array.Sort(array);
			localTableGlyph = (int)array[^1];
		}
		FontFile2 fontFile = new FontFile2(fontData.ToArray());
		int localTableLength = 0;
		bool isSkip = false;
		bool isSymbol = false;
		bool bIsLocaShort = fontFile.Header.IndexToLocFormat == 0;
		CalculateLocalTableLength(fontFile, bIsLocaShort, localTableGlyph, out localTableLength, out isSkip);
		if (!isSkip && localTableLength > 0)
		{
			MemoryStream obj = ResetFontTables(fontFile, isSymbol, localTableLength, bIsLocaShort) as MemoryStream;
			byte[] result = obj.ToArray();
			obj.Dispose();
			return result;
		}
		return fontData.ToArray();
	}

	private void UpdateFontData(MemoryStream fontStream, PdfStream fontFile2)
	{
		fontStream.Position = 0L;
		fontFile2.InternalStream = fontStream;
		fontFile2.Remove("Length1");
		fontFile2.Items.Add(new PdfName("Length1"), new PdfNumber(fontStream.Length));
		fontFile2.Compress = true;
	}

	private Stream ResetFontTables(FontFile2 f2, bool isSymbol, int localTableLength, bool bIsLocaShort)
	{
		List<TableEntry> list = new List<TableEntry>();
		foreach (string table in f2.tableList)
		{
			bool flag = false;
			if (isSymbol && !TtTableList.Contains(table))
			{
				flag = true;
			}
			if (flag)
			{
				continue;
			}
			string text = table;
			int tableID = f2.getTableID(table);
			if (tableID <= -1)
			{
				continue;
			}
			byte[] tableBytes = f2.getTableBytes(tableID);
			if (tableBytes.Length == 0)
			{
				tableBytes = f2.getTableBytes(tableID, isTrueType: true);
			}
			if (table == "OS2")
			{
				text = "OS/2";
			}
			if (text == "loca")
			{
				byte[] array = new byte[localTableLength];
				Array.Copy(tableBytes, array, localTableLength);
				list.Add(GetTableEntry(text, array));
			}
			else if (text == "hmtx")
			{
				byte[] array2 = null;
				if (bIsLocaShort)
				{
					array2 = new byte[localTableLength * 2 - 4];
					if (array2.Length <= tableBytes.Length)
					{
						Array.Copy(tableBytes, array2, localTableLength * 2 - 4);
						list.Add(GetTableEntry(text, array2));
					}
					else
					{
						list.Add(GetTableEntry(text, tableBytes));
					}
				}
				else
				{
					array2 = new byte[localTableLength - 4];
					if (array2.Length <= tableBytes.Length)
					{
						Array.Copy(tableBytes, array2, localTableLength - 4);
						list.Add(GetTableEntry(text, array2));
					}
					else
					{
						list.Add(GetTableEntry(text, array2));
					}
				}
			}
			else
			{
				list.Add(GetTableEntry(text, tableBytes));
			}
		}
		return new FontDecode().CreateFontStream(list);
	}

	private void UpdateFontName(PdfDictionary fontDic, TtfReader ttfReader)
	{
		if (!fontDic.ContainsKey("BaseFont"))
		{
			return;
		}
		PdfName pdfName = fontDic[new PdfName("BaseFont")] as PdfName;
		if (!(pdfName != null) || pdfName.Value.Contains("+"))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		SecureRandomAlgorithm secureRandomAlgorithm = new SecureRandomAlgorithm();
		for (int i = 0; i < 6; i++)
		{
			int index = secureRandomAlgorithm.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Length);
			stringBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ"[index]);
		}
		stringBuilder.Append('+');
		stringBuilder.Append(ttfReader.Metrics.PostScriptName);
		PdfName value = new PdfName(stringBuilder.ToString());
		fontDic.Items[new PdfName("BaseFont")] = value;
		if (!fontDic.ContainsKey("DescendantFonts"))
		{
			return;
		}
		PdfArray pdfArray = fontDic["DescendantFonts"] as PdfArray;
		if (pdfArray == null)
		{
			PdfReferenceHolder pdfReferenceHolder = fontDic["DescendantFonts"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				pdfArray = pdfReferenceHolder.Object as PdfArray;
			}
		}
		PdfDictionary pdfDictionary = ((pdfArray[0] is PdfReferenceHolder) ? ((pdfArray[0] as PdfReferenceHolder).Object as PdfDictionary) : (pdfArray[0] as PdfDictionary));
		if (pdfDictionary != null)
		{
			if (pdfDictionary.ContainsKey("BaseFont"))
			{
				pdfDictionary[new PdfName("BaseFont")] = value;
			}
			PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary["FontDescriptor"] as PdfReferenceHolder;
			if (pdfReferenceHolder2 != null && pdfReferenceHolder2.Object is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("FontName"))
			{
				pdfDictionary2[new PdfName("FontName")] = value;
			}
		}
	}

	private IPdfPrimitive GetObject(PdfDictionary parent, PdfName key)
	{
		PdfDictionary result = null;
		if (parent[key] is PdfDictionary)
		{
			result = parent[key] as PdfDictionary;
		}
		else if (parent[key] is PdfReferenceHolder)
		{
			result = (parent[key] as PdfReferenceHolder).Object as PdfDictionary;
		}
		return result;
	}

	private PdfDictionary GetObject(IPdfPrimitive primitive)
	{
		PdfDictionary result = null;
		if (primitive is PdfDictionary)
		{
			result = primitive as PdfDictionary;
		}
		else if (primitive is PdfReferenceHolder)
		{
			result = (primitive as PdfReferenceHolder).Object as PdfDictionary;
		}
		return result;
	}

	private int CalculateCheckSum(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int i = 0;
		for (int num6 = bytes.Length / 4; i < num6; i++)
		{
			num5 += bytes[num++] & 0xFF;
			num4 += bytes[num++] & 0xFF;
			num3 += bytes[num++] & 0xFF;
			num2 += bytes[num++] & 0xFF;
		}
		return num2 + (num3 << 8) + (num4 << 16) + (num5 << 24);
	}

	private TableEntry GetTableEntry(string tableName, byte[] tableData)
	{
		TableEntry tableEntry = new TableEntry();
		tableEntry.id = tableName;
		tableEntry.bytes = tableData;
		tableEntry.checkSum = CalculateCheckSum(tableEntry.bytes);
		tableEntry.length = tableEntry.bytes.Length;
		return tableEntry;
	}

	private void CalculateLocalTableLength(FontFile2 f2, bool bIsLocaShort, int localTableGlyph, out int localTableLength, out bool isSkip)
	{
		bool flag = false;
		isSkip = false;
		int num = 0;
		localTableLength = 0;
		foreach (string table in f2.tableList)
		{
			string text = table;
			if (!(text == "glyf") && !(text == "loca"))
			{
				continue;
			}
			int tableID = f2.getTableID(table);
			byte[] tableBytes = f2.getTableBytes(tableID);
			if (text == "glyf")
			{
				num = tableBytes.Length;
			}
			else
			{
				if (!(text == "loca"))
				{
					continue;
				}
				byte[] array = null;
				array = ((!bIsLocaShort) ? BitConverter.GetBytes(num) : BitConverter.GetBytes(num / 2));
				Array.Reverse(array);
				bool flag2 = false;
				if (localTableGlyph != 0)
				{
					int num2 = (localTableGlyph + 2) * (bIsLocaShort ? 2 : 4);
					if (num2 < tableBytes.Length && array.Length > 3 && array[2] == tableBytes[num2 - 2] && array[3] == tableBytes[num2 - 1])
					{
						if (num2 == tableBytes.Length)
						{
							isSkip = true;
						}
						localTableLength = num2;
						flag = true;
						break;
					}
					flag2 = true;
				}
				else
				{
					flag2 = true;
				}
				if (flag2)
				{
					for (int i = 0; i < tableBytes.Length; i++)
					{
						if (bIsLocaShort)
						{
							if (i + 1 < tableBytes.Length && array.Length > 3 && tableBytes[i] == array[2] && tableBytes[i + 1] == array[3])
							{
								localTableLength = i + 2;
								if (localTableLength == tableBytes.Length)
								{
									isSkip = true;
								}
								else if (localTableLength - 4 == 0)
								{
									isSkip = true;
								}
								flag = true;
								break;
							}
						}
						else if (i + 3 < tableBytes.Length && array.Length > 3 && tableBytes[i] == array[0] && tableBytes[i + 1] == array[1] && tableBytes[i + 2] == array[2] && tableBytes[i + 3] == array[3])
						{
							localTableLength = i + 4;
							if (localTableLength == tableBytes.Length)
							{
								isSkip = true;
							}
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
	}

	private void GetSmaskReference(Dictionary<PdfName, IPdfPrimitive> xObjectChilds, PdfPageBase lPage)
	{
		List<PdfReference> list = new List<PdfReference>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> xObjectChild in xObjectChilds)
		{
			PdfDictionary @object = GetObject(xObjectChild.Value);
			if (@object == null || !@object.ContainsKey("Subtype") || !((@object["Subtype"] as PdfName).Value == "Image"))
			{
				continue;
			}
			PdfReference reference = ((lPage is PdfPage) ? (lPage as PdfPage).CrossTable : (lPage as PdfLoadedPage).CrossTable).GetReference(xObjectChild.Value);
			list.Add(reference);
			if (@object.ContainsKey("SMask"))
			{
				PdfReferenceHolder pdfReferenceHolder = @object["SMask"] as PdfReferenceHolder;
				if (list.Contains(pdfReferenceHolder.Reference))
				{
					m_sMaskReference.Add(reference);
					m_sMaskReference.Add(pdfReferenceHolder.Reference);
				}
				else if (pdfReferenceHolder != null)
				{
					m_sMaskReference.Add(pdfReferenceHolder.Reference);
				}
			}
		}
		list.Clear();
		list = null;
	}

	private bool CompareStream(MemoryStream stream, out string hashValue)
	{
		hashValue = string.Empty;
		stream.Position = 0L;
		byte[] array = new byte[(int)stream.Length];
		stream.Read(array, 0, array.Length);
		stream.Position = 0L;
		hashValue = CreateHashFromStream(array);
		return m_resourceCollection.ContainsKey(hashValue);
	}

	private string CreateHashFromStream(byte[] streamBytes)
	{
		byte[] array = new MessageDigestAlgorithms().Digest("SHA256", streamBytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		array = null;
		return stringBuilder.ToString();
	}

	private bool IsOptimizeUsedCharacters(PdfDictionary fontDictionary, PdfStream fontFile2)
	{
		bool result = false;
		if (PdfCrossTable.Dereference(fontDictionary["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray && PdfCrossTable.Dereference(pdfArray[0]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("FontDescriptor"))
		{
			baseFont = (PdfCrossTable.Dereference(pdfDictionary["BaseFont"]) as PdfName).Value;
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["FontDescriptor"]) as PdfDictionary;
			if (pdfDictionary2.ContainsKey("Flags") && PdfCrossTable.Dereference(pdfDictionary2["Flags"]) is PdfNumber { IntValue: 6 })
			{
				try
				{
					BinaryReader binaryReader = null;
					binaryReader = ((!fontFile2.InternalStream.CanSeek) ? new BinaryReader(new MemoryStream(fontFile2.Data), TtfReader.Encoding) : new BinaryReader(fontFile2.InternalStream, TtfReader.Encoding));
					TtfReader ttfReader = new TtfReader(binaryReader);
					ttfReader.CreateInternals();
					if (ttfReader.TableDirectory.ContainsKey("cmap"))
					{
						result = true;
					}
				}
				catch
				{
				}
			}
		}
		return result;
	}

	private bool IsCIDToGIDMap(PdfDictionary fontDictionary)
	{
		bool flag = false;
		if (PdfCrossTable.Dereference(fontDictionary["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray)
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfArray[0]) as PdfDictionary;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("FontDescriptor"))
			{
				flag = pdfDictionary.ContainsKey("CIDToGIDMap");
			}
			if (flag && !(PdfCrossTable.Dereference(pdfDictionary["CIDToGIDMap"]) is PdfStream))
			{
				flag = false;
			}
		}
		return flag;
	}
}
