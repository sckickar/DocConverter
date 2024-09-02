using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DocGen.Drawing;
using DocGen.Pdf.ColorSpace;
using DocGen.Pdf.Compression;
using DocGen.Pdf.Exporting;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf.Parsing;

internal class PdfA1BConverter
{
	private PdfLoadedDocument ldoc;

	private bool isPdfPage;

	private const float MaxColourChannelValue = 255f;

	private Colorspace colorspace;

	private string appliedSCColorSpace;

	private string appliedSCNColorSpace;

	private float nonStrokingOpacity;

	private float strokingOpacity;

	private bool hasNonStroking;

	private bool isPdfFontFamily = true;

	private bool hasStroking;

	private Dictionary<string, PdfReferenceHolder> formObjects;

	private List<string> m_usedChars;

	private Dictionary<string, PdfTrueTypeFont> m_replaceFonts;

	private Dictionary<string, PdfDictionary> m_oldFonts;

	private Dictionary<string, PdfDictionary> m_replaceFontDictionary;

	private List<string> TtTableList = new List<string>();

	private const string c_cmapPrefix = "/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\r\n/CIDSystemInfo << /Registry (Adobe)/Ordering (UCS)/Supplement 0>> def\n/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\r\n";

	private const string c_cmapEndCodespaceRange = "endcodespacerange\r\n";

	private const string c_cmapSuffix = "endbfrange\nendcmap\nCMapName currentdict /CMap defineresource pop\nend end\r\n";

	private const string c_cmapBeginRange = "beginbfrange\r\n";

	private const string c_cmapEndRange = "endbfrange\r\n";

	private const int c_cmapNextRangeValue = 100;

	private const string c_rdfPdfa = "http://www.aiim.org/pdfa/ns/id/";

	private const string c_rdfUri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

	private const string c_pdfschema = "http://ns.adobe.com/pdf/1.3/";

	private const string c_dublinSchema = "http://purl.org/dc/elements/1.1/";

	private const string c_pdfaExtension = "http://www.aiim.org/pdfa/ns/extension/";

	internal PdfConformanceLevel PdfALevel;

	private List<PdfRecordCollection> recordCollectionList = new List<PdfRecordCollection>();

	private Dictionary<PdfResources, PdfRecordCollection> recordList = new Dictionary<PdfResources, PdfRecordCollection>();

	private Dictionary<PdfResources, List<string>> fontkeys = new Dictionary<PdfResources, List<string>>();

	private PdfAConversionProgressEventArgs args;

	private bool fontEmbedding;

	private List<long> xobjectReference = new List<long>();

	private Dictionary<int, char> m_cidByte2Unicode;

	private PdfICCColorSpace m_pdfICCColorSpace;

	private Dictionary<string, PdfDictionary> pdfFonts;

	private bool constainsColorspace;

	private int[] m_charCodeTable = new int[256]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
		20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
		30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
		40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
		50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
		60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
		70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
		80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
		90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
		100, 101, 102, 103, 104, 105, 106, 107, 108, 109,
		110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
		120, 121, 122, 123, 124, 125, 126, 127, 8364, 65533,
		8218, 402, 8222, 8230, 8224, 8225, 710, 8240, 352, 8249,
		338, 65533, 381, 65533, 65533, 8216, 8217, 8220, 8221, 8226,
		8211, 8212, 732, 8482, 353, 8250, 339, 65533, 382, 376,
		160, 161, 162, 163, 164, 165, 166, 167, 168, 169,
		170, 171, 172, 173, 174, 175, 176, 177, 178, 179,
		180, 181, 182, 183, 184, 185, 186, 187, 188, 189,
		190, 191, 192, 193, 194, 195, 196, 197, 198, 199,
		200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
		210, 211, 212, 213, 214, 215, 216, 217, 218, 219,
		220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
		230, 231, 232, 233, 234, 235, 236, 237, 238, 239,
		240, 241, 242, 243, 244, 245, 246, 247, 248, 249,
		250, 251, 252, 253, 254, 255
	};

	private bool hasNoCa;

	private List<string> UsedFonts
	{
		get
		{
			if (m_usedChars == null)
			{
				m_usedChars = new List<string>();
			}
			return m_usedChars;
		}
	}

	private Dictionary<string, PdfTrueTypeFont> ReplaceFonts
	{
		get
		{
			if (m_replaceFonts == null)
			{
				m_replaceFonts = new Dictionary<string, PdfTrueTypeFont>();
			}
			return m_replaceFonts;
		}
	}

	private Dictionary<string, PdfDictionary> ReplaceFontDictionary
	{
		get
		{
			if (m_replaceFontDictionary == null)
			{
				m_replaceFontDictionary = new Dictionary<string, PdfDictionary>();
			}
			return m_replaceFontDictionary;
		}
	}

	private Dictionary<string, PdfDictionary> OldFonts
	{
		get
		{
			if (m_oldFonts == null)
			{
				m_oldFonts = new Dictionary<string, PdfDictionary>();
			}
			return m_oldFonts;
		}
	}

	private Dictionary<int, char> CIDByte2Unicode
	{
		get
		{
			if (m_cidByte2Unicode == null)
			{
				GetCIDByte2Unicode();
			}
			return m_cidByte2Unicode;
		}
	}

	private PdfICCColorSpace PdfCmykColorSpace
	{
		get
		{
			if (m_pdfICCColorSpace == null)
			{
				m_pdfICCColorSpace = GetCMYKColorSpace();
			}
			return m_pdfICCColorSpace;
		}
	}

	private PdfICCColorSpace GetCMYKColorSpace()
	{
		return new PdfICCColorSpace
		{
			ColorComponents = 4,
			AlternateColorSpace = new PdfDeviceColorSpace(PdfColorSpace.CMYK)
		};
	}

	internal PdfLoadedDocument Convert(PdfLoadedDocument document)
	{
		ldoc = document;
		PdfDocument.EnableCache = false;
		document.FileStructure.CrossReferenceType = PdfCrossReferenceType.CrossReferenceTable;
		document.FileStructure.IncrementalUpdate = false;
		if (PdfALevel != PdfConformanceLevel.Pdf_A3B && PdfALevel != PdfConformanceLevel.Pdf_A3U)
		{
			AttachmentsConsideration(document);
		}
		else
		{
			PdfName key = new PdfName("AFRelationship");
			PdfArray pdfArray = new PdfArray();
			if (document.Attachments != null)
			{
				int num = 0;
				int num2 = 0;
				while (num < document.Attachments.Count)
				{
					if (!document.Attachments[num].Dictionary.Items.ContainsKey(key))
					{
						document.Attachments[num].Dictionary.Items[key] = new PdfName(PdfAttachmentRelationship.Alternative);
					}
					if (document.Attachments.ArrayList.Count > ++num2 && document.Attachments.ArrayList[num2] is PdfReferenceHolder)
					{
						pdfArray.Add(document.Attachments.ArrayList[num2]);
					}
					else
					{
						pdfArray.Add(new PdfReferenceHolder(document.Attachments[num].Dictionary));
					}
					PdfAttachment pdfAttachment = document.Attachments[num];
					if (pdfAttachment != null && !pdfAttachment.Dictionary.ContainsKey("AFRelationship"))
					{
						pdfAttachment.Dictionary.SetProperty("AFRelationship", new PdfName(PdfAttachmentRelationship.Alternative));
					}
					num++;
					num2++;
				}
				if (!document.Catalog.ContainsKey("AF"))
				{
					document.Catalog.Items.Add(new PdfName("AF"), pdfArray);
				}
				SetAttachmentDictionary();
			}
		}
		if (document.RaiseTrackPdfAConversionProgress)
		{
			args = new PdfAConversionProgressEventArgs();
			args.m_progressValue = 0f;
			document.OnPdfAConversionTrackProgress(args);
			ParseForm(document);
			args.m_progressValue = 10f;
			document.OnPdfAConversionTrackProgress(args);
			RetrieveFontData(document);
			if (args.m_progressValue != 35f)
			{
				args.m_progressValue = 35f;
				document.OnPdfAConversionTrackProgress(args);
			}
			ContentStreamParsing(document);
			if (args.m_progressValue != 85f)
			{
				args.m_progressValue = 85f;
				document.OnPdfAConversionTrackProgress(args);
			}
			AddDocumentColorProfile(document);
			args.m_progressValue = 90f;
			document.OnPdfAConversionTrackProgress(args);
			AddMetaDataInfo(document);
			args.m_progressValue = 95f;
			document.OnPdfAConversionTrackProgress(args);
			AddTrailerID(document);
			args.m_progressValue = 100f;
			document.OnPdfAConversionTrackProgress(args);
		}
		else
		{
			RetrieveFontData(document);
			ContentStreamParsing(document);
			if (document.Form != null)
			{
				FormEncoding(document);
			}
			AddDocumentColorProfile(document);
			AddMetaDataInfo(document);
			AddTrailerID(document);
		}
		m_usedChars = null;
		m_replaceFonts = null;
		m_oldFonts = null;
		pdfFonts = null;
		m_replaceFontDictionary = null;
		fontkeys = null;
		if (m_cidByte2Unicode != null)
		{
			m_cidByte2Unicode.Clear();
		}
		m_cidByte2Unicode = null;
		return document;
	}

	private void SetAttachmentDictionary()
	{
		if (!ldoc.Catalog.ContainsKey("Names"))
		{
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(ldoc.Catalog["Names"]);
		if (dictionaryFromRefernceHolder == null || !dictionaryFromRefernceHolder.ContainsKey("EmbeddedFiles"))
		{
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["EmbeddedFiles"]);
		if (dictionaryFromRefernceHolder2 == null)
		{
			return;
		}
		foreach (IPdfPrimitive value in dictionaryFromRefernceHolder2.Items.Values)
		{
			if (!(value is PdfArray))
			{
				continue;
			}
			PdfArray pdfArray = value as PdfArray;
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (!(pdfArray[i] is PdfReferenceHolder))
				{
					continue;
				}
				PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
				if (pdfReferenceHolder.Object == null || !(pdfReferenceHolder.Object is PdfDictionary pdfDictionary) || pdfDictionary.ContainsKey("AFRelationship"))
				{
					continue;
				}
				PdfName key = new PdfName("AFRelationship");
				pdfDictionary.SetProperty(key, new PdfName(PdfAttachmentRelationship.Data));
				if (!pdfDictionary.ContainsKey("EF") || !(PdfCrossTable.Dereference(pdfDictionary["EF"]) is PdfDictionary pdfDictionary2))
				{
					continue;
				}
				foreach (IPdfPrimitive value2 in pdfDictionary2.Items.Values)
				{
					if (value2 is PdfReferenceHolder && (value2 as PdfReferenceHolder).Object is PdfStream pdfStream && !pdfStream.ContainsKey("Subtype"))
					{
						pdfStream["Subtype"] = new PdfName(PdfName.EncodeName("application/octet-stream"));
					}
				}
			}
		}
	}

	internal void EmbedFont(PdfLoadedDocument document)
	{
		ldoc = document;
		fontEmbedding = true;
		PdfDocument.EnableCache = false;
		document.FileStructure.CrossReferenceType = PdfCrossReferenceType.CrossReferenceTable;
		document.FileStructure.IncrementalUpdate = false;
		_ = document.Conformance;
		ParseForm(document);
		RetrieveFontData(document);
		ContentStreamParsing(document);
		m_usedChars = null;
		m_replaceFonts = null;
		m_oldFonts = null;
		m_replaceFontDictionary = null;
		fontkeys = null;
	}

	internal PdfArray GetArrayFromReferenceHolder(IPdfPrimitive primitive)
	{
		if (primitive is PdfReferenceHolder)
		{
			if ((primitive as PdfReferenceHolder).Object is PdfReferenceHolder)
			{
				return GetArrayFromReferenceHolder((primitive as PdfReferenceHolder).Object as PdfReferenceHolder);
			}
			return (primitive as PdfReferenceHolder).Object as PdfArray;
		}
		return primitive as PdfArray;
	}

	internal PdfDictionary GetDictionaryFromRefernceHolder(IPdfPrimitive primitive)
	{
		if (primitive is PdfReferenceHolder)
		{
			if ((primitive as PdfReferenceHolder).Object is PdfReferenceHolder)
			{
				return GetDictionaryFromRefernceHolder((primitive as PdfReferenceHolder).Object as PdfReferenceHolder);
			}
			return (primitive as PdfReferenceHolder).Object as PdfDictionary;
		}
		return primitive as PdfDictionary;
	}

	internal PdfResources GetResourceFromRefernceHolder(IPdfPrimitive primitive)
	{
		if (primitive is PdfReferenceHolder)
		{
			if ((primitive as PdfReferenceHolder).Object is PdfReferenceHolder)
			{
				return GetResourceFromRefernceHolder((primitive as PdfReferenceHolder).Object as PdfResources);
			}
			return (primitive as PdfReferenceHolder).Object as PdfResources;
		}
		return primitive as PdfResources;
	}

	internal PdfStream GetStreamFromRefernceHolder(IPdfPrimitive primitive)
	{
		if (primitive is PdfReferenceHolder)
		{
			if ((primitive as PdfReferenceHolder).Object is PdfReferenceHolder)
			{
				return GetStreamFromRefernceHolder((primitive as PdfReferenceHolder).Object as PdfReferenceHolder);
			}
			return (primitive as PdfReferenceHolder).Object as PdfStream;
		}
		return primitive as PdfStream;
	}

	internal PdfBoolean GetPdfBooleanFromReferenceHolder(IPdfPrimitive primitive)
	{
		if (primitive is PdfReferenceHolder)
		{
			if ((primitive as PdfReferenceHolder).Object is PdfReferenceHolder)
			{
				return GetPdfBooleanFromReferenceHolder((primitive as PdfReferenceHolder).Object as PdfReferenceHolder);
			}
			return (primitive as PdfReferenceHolder).Object as PdfBoolean;
		}
		return primitive as PdfBoolean;
	}

	private bool IsFontHasDifferenceDictiory(PdfDictionary fontDictionary)
	{
		bool result = false;
		if (fontDictionary != null)
		{
			if (fontDictionary.ContainsKey("Encoding"))
			{
				if (PdfCrossTable.Dereference(fontDictionary["Encoding"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Differences") && pdfDictionary.ContainsKey("Differences"))
				{
					result = true;
				}
			}
			else if (fontDictionary.Items.Count > 0)
			{
				foreach (PdfName key in fontDictionary.Items.Keys)
				{
					if (fontDictionary[key] is PdfReferenceHolder)
					{
						PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(fontDictionary[key] as PdfReferenceHolder);
						PdfName pdfName = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["Subtype"]) as PdfName;
						if (dictionaryFromRefernceHolder.ContainsKey("Encoding") && PdfCrossTable.Dereference(dictionaryFromRefernceHolder["Encoding"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Differences") && pdfName != null && pdfName.Value != "Type1")
						{
							result = true;
						}
					}
				}
			}
		}
		return result;
	}

	private void ParseFormStreamFromXObject(PdfDictionary xObjectDictionary, string key)
	{
		if (xObjectDictionary != null && xObjectDictionary is PdfStream && xObjectDictionary.ContainsKey("Type") && (xObjectDictionary["Type"] as PdfName).Value == "XObject" && xObjectDictionary.ContainsKey("Subtype") && (xObjectDictionary["Subtype"] as PdfName).Value == "Form")
		{
			ParseFormStream(xObjectDictionary as PdfStream, key, GetDictionaryFromRefernceHolder(xObjectDictionary["Resources"]), enableRecrusiveCall: true);
		}
	}

	private void ParseFormStream(PdfStream internalStream, string xObjectKey, PdfDictionary resources, bool enableRecrusiveCall)
	{
		bool flag = false;
		if (internalStream.Data.Length != 0)
		{
			internalStream.Decompress();
			PdfDictionary pdfDictionary = null;
			if (resources != null && resources.ContainsKey("Font"))
			{
				pdfDictionary = GetDictionaryFromRefernceHolder(resources["Font"]);
			}
			if (pdfDictionary != null)
			{
				flag = IsFontHasDifferenceDictiory(pdfDictionary);
			}
			string text = null;
			float fontSize = 12f;
			PdfDictionary fontDictionary = null;
			PdfRecordCollection pdfRecordCollection = new XObjectElement(internalStream, xObjectKey).Render(null);
			PdfStream pdfStream = new PdfStream();
			if (pdfRecordCollection != null)
			{
				int count = pdfRecordCollection.RecordCollection.Count;
				bool flag2 = false;
				for (int i = 0; i < count; i++)
				{
					PdfRecord pdfRecord = pdfRecordCollection.RecordCollection[i];
					if (pdfRecord.Operands != null && pdfRecord.Operands.Length >= 1)
					{
						if (pdfRecord.OperatorName == "CS" || pdfRecord.OperatorName == "cs")
						{
							StringBuilder stringBuilder = new StringBuilder();
							string text2 = pdfRecord.Operands[0].Replace("/", "");
							if (text2 == "DeviceCMYK")
							{
								stringBuilder.Append("/DeviceRGB");
								stringBuilder.Append(" ");
								string s = stringBuilder.ToString();
								byte[] bytes = Encoding.Default.GetBytes(s);
								pdfStream.Write(bytes);
								bytes = null;
								flag2 = false;
							}
							else if (text2 == "Pattern")
							{
								stringBuilder.Append(pdfRecord.Operands[0]);
								stringBuilder.Append(" ");
								string s2 = stringBuilder.ToString();
								byte[] bytes2 = Encoding.Default.GetBytes(s2);
								pdfStream.Write(bytes2);
								bytes2 = null;
								flag2 = true;
							}
							else
							{
								stringBuilder.Append(pdfRecord.Operands[0]);
								stringBuilder.Append(" ");
								string s3 = stringBuilder.ToString();
								byte[] bytes3 = Encoding.Default.GetBytes(s3);
								pdfStream.Write(bytes3);
								bytes3 = null;
								flag2 = false;
							}
						}
						else if (flag2 && (pdfRecord.OperatorName == "SCN" || pdfRecord.OperatorName == "scn"))
						{
							StringBuilder stringBuilder2 = new StringBuilder();
							PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(resources["Pattern"]);
							string key = pdfRecord.Operands[0].ToString(CultureInfo.InvariantCulture).Replace("/", "");
							if (dictionaryFromRefernceHolder != null && dictionaryFromRefernceHolder.ContainsKey(key))
							{
								RemoveSMaskFromPattern(GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder[key]));
							}
							stringBuilder2.Append(pdfRecord.Operands[0]);
							stringBuilder2.Append(" ");
							string s4 = stringBuilder2.ToString();
							byte[] bytes4 = Encoding.Default.GetBytes(s4);
							pdfStream.Write(bytes4);
							bytes4 = null;
						}
						else if (pdfRecord.OperatorName == "gs" && resources != null)
						{
							StringBuilder stringBuilder3 = new StringBuilder();
							PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(resources["ExtGState"]);
							string value = pdfRecord.Operands[0].ToString(CultureInfo.InvariantCulture).Replace("/", "");
							if (dictionaryFromRefernceHolder2 != null && dictionaryFromRefernceHolder2.Items.ContainsKey(new PdfName(value)))
							{
								IPdfPrimitive primitive = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2).Items[new PdfName(value)];
								PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(primitive);
								if (dictionaryFromRefernceHolder3.ContainsKey("SMask"))
								{
									PdfDictionary dictionaryFromRefernceHolder4 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder3["SMask"]);
									if (dictionaryFromRefernceHolder4 == null || !dictionaryFromRefernceHolder4.ContainsKey("S") || !((dictionaryFromRefernceHolder4["S"] as PdfName).Value == "Luminosity"))
									{
										dictionaryFromRefernceHolder3.Remove("SMask");
										dictionaryFromRefernceHolder3.Modify();
									}
								}
								if (dictionaryFromRefernceHolder3.ContainsKey("BM") && PdfALevel == PdfConformanceLevel.Pdf_A1B)
								{
									bool flag3 = false;
									if (dictionaryFromRefernceHolder3["BM"] is PdfName)
									{
										if ((dictionaryFromRefernceHolder3["BM"] as PdfName).Value == "Normal" || (dictionaryFromRefernceHolder3["BM"] as PdfName).Value == "Compatible")
										{
											flag3 = true;
										}
										else
										{
											dictionaryFromRefernceHolder3["BM"] = new PdfName("Normal");
											dictionaryFromRefernceHolder3.Modify();
											flag3 = true;
										}
									}
									else if (dictionaryFromRefernceHolder3["BM"] is PdfArray)
									{
										List<IPdfPrimitive> list = new List<IPdfPrimitive>();
										foreach (IPdfPrimitive item in dictionaryFromRefernceHolder3["BM"] as PdfArray)
										{
											list.Add(item);
										}
										for (int j = 0; j < list.Count; j++)
										{
											if (list[j] is PdfName && ((list[j] as PdfName).Value == "Normal" || (list[j] as PdfName).Value == "Compatible"))
											{
												flag3 = true;
											}
											else if (list[j] is PdfName)
											{
												(dictionaryFromRefernceHolder3["BM"] as PdfArray).Elements[j] = new PdfName("Normal");
												(dictionaryFromRefernceHolder3["BM"] as PdfArray).MarkChanged();
											}
										}
									}
									if (flag3 && PdfALevel == PdfConformanceLevel.Pdf_A1B)
									{
										if (dictionaryFromRefernceHolder3.ContainsKey("ca"))
										{
											dictionaryFromRefernceHolder3["ca"] = new PdfNumber(1);
										}
										if (dictionaryFromRefernceHolder3.ContainsKey(new PdfName("CA")))
										{
											dictionaryFromRefernceHolder3["CA"] = new PdfNumber(1);
										}
									}
								}
								else if (PdfALevel == PdfConformanceLevel.Pdf_A1B)
								{
									if (dictionaryFromRefernceHolder3.ContainsKey("ca"))
									{
										dictionaryFromRefernceHolder3["ca"] = new PdfNumber(1);
									}
									if (dictionaryFromRefernceHolder3.ContainsKey(new PdfName("CA")))
									{
										dictionaryFromRefernceHolder3["CA"] = new PdfNumber(1);
									}
								}
							}
							stringBuilder3.Append(pdfRecord.Operands[0]);
							stringBuilder3.Append(" ");
							string s5 = stringBuilder3.ToString();
							pdfStream.Write(Encoding.Default.GetBytes(s5));
						}
						else if (pdfRecord.OperatorName == "Do")
						{
							StringBuilder stringBuilder4 = new StringBuilder();
							if (internalStream.ContainsKey("Resources"))
							{
								IPdfPrimitive pdfPrimitive = GetDictionaryFromRefernceHolder(internalStream["Resources"])["XObject"];
								string value2 = pdfRecord.Operands[0].Replace("/", "");
								if (pdfPrimitive != null && GetDictionaryFromRefernceHolder(pdfPrimitive).Items.ContainsKey(new PdfName(value2)))
								{
									PdfReferenceHolder pdfReferenceHolder = GetDictionaryFromRefernceHolder(pdfPrimitive)[new PdfName(value2)] as PdfReferenceHolder;
									if (pdfReferenceHolder.Object is PdfDictionary pdfDictionary2 && !pdfDictionary2.ContainsKey("Interpolate"))
									{
										RemoveSMask(pdfReferenceHolder, internalStream, pdfRecord.Operands[0]);
									}
								}
							}
							stringBuilder4.Append(pdfRecord.Operands[0]);
							stringBuilder4.Append(" ");
							string s6 = stringBuilder4.ToString();
							pdfStream.Write(Encoding.Default.GetBytes(s6));
						}
						else if (pdfRecord.OperatorName == "K" || pdfRecord.OperatorName == "k")
						{
							StringBuilder stringBuilder5 = new StringBuilder();
							Color color = new DeviceCMYK().GetColor(pdfRecord.Operands);
							float[] array = new float[3]
							{
								(int)color.R,
								(int)color.G,
								(int)color.B
							};
							array[0] /= 255f;
							array[1] /= 255f;
							array[2] /= 255f;
							stringBuilder5.Append(Math.Round(array[0], 2).ToString(CultureInfo.InvariantCulture));
							stringBuilder5.Append(" ");
							stringBuilder5.Append(Math.Round(array[1], 2).ToString(CultureInfo.InvariantCulture));
							stringBuilder5.Append(" ");
							stringBuilder5.Append(Math.Round(array[2], 2).ToString(CultureInfo.InvariantCulture));
							stringBuilder5.Append(" ");
							string s7 = stringBuilder5.ToString();
							byte[] bytes5 = Encoding.Default.GetBytes(s7);
							pdfStream.Write(bytes5);
							array = null;
							bytes5 = null;
						}
						else if (!flag)
						{
							if (pdfRecord.OperatorName == "Tj" || pdfRecord.OperatorName == "TJ")
							{
								for (int k = 0; k < pdfRecord.Operands.Length; k++)
								{
									string operand = pdfRecord.Operands[k];
									operand = TrimOperand(operand);
									PdfString pdfString = new PdfString(operand);
									pdfStream.Write(pdfString.Bytes);
									pdfStream.Write(" ");
								}
							}
							else
							{
								for (int l = 0; l < pdfRecord.Operands.Length; l++)
								{
									string text3 = pdfRecord.Operands[l];
									if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
									{
										text3 = TrimOperand(text3);
									}
									PdfString pdfString2 = new PdfString(text3);
									pdfStream.Write(pdfString2.Bytes);
									if (pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"")
									{
										pdfStream.Write(" ");
									}
								}
							}
						}
						else if (pdfRecord.OperatorName == "Tf" && pdfDictionary != null)
						{
							int m;
							for (m = 0; m < pdfRecord.Operands.Length; m++)
							{
								if (pdfRecord.Operands[m].Contains("/"))
								{
									text = pdfRecord.Operands[m].Replace("/", "");
									break;
								}
							}
							fontSize = float.Parse(pdfRecord.Operands[m + 1], CultureInfo.InvariantCulture);
							if (pdfDictionary.ContainsKey(text))
							{
								fontDictionary = GetDictionaryFromRefernceHolder(pdfDictionary[text]);
							}
							for (int n = 0; n < pdfRecord.Operands.Length; n++)
							{
								string operand2 = pdfRecord.Operands[n];
								operand2 = TrimOperand(operand2);
								PdfString pdfString3 = new PdfString(operand2);
								pdfStream.Write(pdfString3.Bytes);
								pdfStream.Write(" ");
							}
						}
						else if (pdfRecord.OperatorName == "Tj" || pdfRecord.OperatorName == "TJ")
						{
							PdfDictionary fontDescriptor = null;
							PdfDictionary descendantFont = null;
							GetFontInternals(fontDictionary, out fontDescriptor, out descendantFont);
							if (!string.IsNullOrEmpty(text) && OldFonts.ContainsKey(text) && ReplaceFonts.ContainsKey(text) && ReplaceFonts[text] != null)
							{
								PdfDictionary dictionaryFromRefernceHolder5 = GetDictionaryFromRefernceHolder(ReplaceFonts[text].FontInternal);
								if (dictionaryFromRefernceHolder5 != null && dictionaryFromRefernceHolder5.ContainsKey("Subtype"))
								{
									string text4 = new FontStructure(OldFonts[text])
									{
										FontSize = fontSize
									}.DecodeTextExtraction(string.Join("", pdfRecord.Operands), isSameFont: true);
									text4 = ConvertToUnicode(text4, ReplaceFonts[text]);
									PdfString unicodeString = GetUnicodeString(text4);
									pdfDictionary.Remove(text);
									pdfDictionary[new PdfName(text)] = new PdfReferenceHolder(ReplaceFonts[text]);
									pdfStream.Write(unicodeString.PdfEncode(null));
									pdfStream.Write(" ");
								}
							}
							else
							{
								for (int num = 0; num < pdfRecord.Operands.Length; num++)
								{
									string operand3 = pdfRecord.Operands[num];
									operand3 = TrimOperand(operand3);
									PdfString pdfString4 = new PdfString(operand3);
									pdfStream.Write(pdfString4.Bytes);
									pdfStream.Write(" ");
								}
							}
						}
						else
						{
							for (int num2 = 0; num2 < pdfRecord.Operands.Length; num2++)
							{
								string text5 = pdfRecord.Operands[num2];
								if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
								{
									text5 = TrimOperand(text5);
								}
								PdfString pdfString5 = new PdfString(text5);
								pdfStream.Write(pdfString5.Bytes);
								if (pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"")
								{
									pdfStream.Write(" ");
								}
							}
						}
					}
					if (pdfRecord.OperatorName == "K")
					{
						pdfStream.Write("RG");
					}
					else if (pdfRecord.OperatorName == "k")
					{
						pdfStream.Write("rg");
					}
					else
					{
						if (pdfRecord.OperatorName == "Q" || pdfRecord.OperatorName == "E")
						{
							flag2 = false;
						}
						pdfStream.Write(pdfRecord.OperatorName);
					}
					_ = i + 1;
					if (i + 1 < count && (pdfRecord.OperatorName == "W" || pdfRecord.OperatorName == "W*") && pdfRecordCollection.RecordCollection[i + 1].OperatorName == "n")
					{
						pdfStream.Write(" ");
					}
					else if (pdfRecord.OperatorName == "w")
					{
						pdfStream.Write(" ");
					}
					else
					{
						pdfStream.Write("\r\n");
					}
				}
				if (count == 0)
				{
					pdfStream.Write("q");
					pdfStream.Write("\r\n");
					pdfStream.Write("Q");
					pdfStream.Write("\r\n");
				}
				internalStream.Items.Remove(new PdfName("Length"));
				internalStream.Data = pdfStream.Data;
				internalStream.isSkip = false;
				internalStream.Modify();
				pdfStream.Clear();
				pdfStream.InternalStream.Dispose();
				pdfStream.InternalStream.Close();
				pdfStream.InternalStream = null;
			}
		}
		else
		{
			internalStream.Data = new byte[1] { 32 };
			internalStream.isSkip = false;
			internalStream.Modify();
		}
		if (!enableRecrusiveCall || resources == null || !resources.ContainsKey("XObject"))
		{
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder6 = GetDictionaryFromRefernceHolder(resources["XObject"]);
		if (dictionaryFromRefernceHolder6 == null || dictionaryFromRefernceHolder6.Items.Count <= 0)
		{
			return;
		}
		List<PdfName> list2 = new List<PdfName>(dictionaryFromRefernceHolder6.Keys);
		bool flag4 = true;
		foreach (PdfName item2 in list2)
		{
			if (item2.Value == xObjectKey)
			{
				flag4 = false;
				break;
			}
		}
		if (!flag4)
		{
			return;
		}
		foreach (PdfName item3 in list2)
		{
			ParseFormStreamFromXObject(GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder6[item3]), item3.Value);
		}
	}

	private void RemoveSMaskFromPattern(PdfDictionary pattern)
	{
		if (pattern != null && pattern.ContainsKey("Resources"))
		{
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(pattern["Resources"]);
			if (dictionaryFromRefernceHolder == null || !dictionaryFromRefernceHolder.ContainsKey("XObject"))
			{
				return;
			}
			PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["XObject"]);
			if (dictionaryFromRefernceHolder2 == null || dictionaryFromRefernceHolder2.Count <= 0)
			{
				return;
			}
			{
				foreach (IPdfPrimitive value in dictionaryFromRefernceHolder2.Values)
				{
					PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(value);
					if (dictionaryFromRefernceHolder3 == null)
					{
						continue;
					}
					if (dictionaryFromRefernceHolder3.ContainsKey("Subtype") && (dictionaryFromRefernceHolder3["Subtype"] as PdfName).Value == "Image")
					{
						if (dictionaryFromRefernceHolder3.ContainsKey("SMask"))
						{
							dictionaryFromRefernceHolder3.Remove("SMask");
							dictionaryFromRefernceHolder3.Modify();
						}
						if (dictionaryFromRefernceHolder3.ContainsKey("Interpolate"))
						{
							GetPdfBooleanFromReferenceHolder(dictionaryFromRefernceHolder3["Interpolate"]).Value = false;
							dictionaryFromRefernceHolder3.Modify();
						}
					}
					else if (dictionaryFromRefernceHolder3.ContainsKey("Resources"))
					{
						RemoveSMaskFromPattern(dictionaryFromRefernceHolder3);
					}
				}
				return;
			}
		}
		if (pattern != null && pattern.ContainsKey("SMask"))
		{
			pattern.Remove("SMask");
			pattern.Modify();
		}
	}

	private string TrimOperand(string operand)
	{
		if (operand == ".00" || operand == "-.00")
		{
			operand = "0";
		}
		if (operand.Contains(".00"))
		{
			string[] array = operand.Split(new char[1] { '.' });
			if (array.Length == 2 && array[1] == "00")
			{
				operand = array[0];
			}
			array = null;
		}
		return operand;
	}

	private void RemoveSMask(PdfReferenceHolder imageObject, PdfStream internalStream, string key)
	{
		while (imageObject != null && imageObject.Object is PdfReferenceHolder)
		{
			imageObject = imageObject.Object as PdfReferenceHolder;
		}
		if (!(imageObject.Object is PdfStream))
		{
			return;
		}
		PdfStream pdfStream = imageObject.Object as PdfStream;
		if (!((pdfStream["Subtype"] as PdfName).Value == "Image"))
		{
			return;
		}
		if (PdfALevel == PdfConformanceLevel.Pdf_A1B && pdfStream.ContainsKey("SMask"))
		{
			pdfStream.Remove("SMask");
			pdfStream.Modify();
		}
		if (pdfStream.ContainsKey("Mask"))
		{
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(pdfStream["Mask"]);
			if (dictionaryFromRefernceHolder != null && dictionaryFromRefernceHolder.ContainsKey("Interpolate"))
			{
				IPdfPrimitive primitive = dictionaryFromRefernceHolder["Interpolate"];
				GetPdfBooleanFromReferenceHolder(primitive).Value = false;
				dictionaryFromRefernceHolder.Modify();
			}
		}
		if (pdfStream.ContainsKey("Interpolate"))
		{
			IPdfPrimitive primitive2 = pdfStream["Interpolate"];
			GetPdfBooleanFromReferenceHolder(primitive2).Value = false;
			pdfStream.Modify();
		}
	}

	internal void GetFontInternals(PdfDictionary fontDictionary, out PdfDictionary fontDescriptor, out PdfDictionary descendantFont)
	{
		fontDescriptor = null;
		descendantFont = null;
		if (fontDictionary == null)
		{
			return;
		}
		fontDescriptor = GetDictionaryFromRefernceHolder(fontDictionary["FontDescriptor"]);
		if (!fontDictionary.ContainsKey("DescendantFonts"))
		{
			return;
		}
		if (fontDictionary["DescendantFonts"] is PdfArray)
		{
			descendantFont = GetDictionaryFromRefernceHolder((fontDictionary["DescendantFonts"] as PdfArray).Elements[0]);
			if (descendantFont.ContainsKey("FontDescriptor"))
			{
				fontDescriptor = GetDictionaryFromRefernceHolder(descendantFont["FontDescriptor"]);
			}
		}
		else if (fontDictionary["DescendantFonts"] is PdfReferenceHolder)
		{
			PdfArray arrayFromReferenceHolder = GetArrayFromReferenceHolder(fontDictionary["DescendantFonts"]);
			descendantFont = GetDictionaryFromRefernceHolder(arrayFromReferenceHolder.Elements[0]);
			if (descendantFont.ContainsKey("FontDescriptor"))
			{
				fontDescriptor = GetDictionaryFromRefernceHolder(descendantFont["FontDescriptor"]);
			}
		}
	}

	private void RetrieveFontData(PdfLoadedDocument document)
	{
		int num = 0;
		foreach (PdfPageBase page in document.Pages)
		{
			if (page is PdfPage)
			{
				isPdfPage = true;
			}
			MemoryStream memoryStream = new MemoryStream();
			page.Layers.CombineContent(memoryStream);
			ContentParser contentParser = new ContentParser(memoryStream.ToArray())
			{
				ConformanceEnabled = true
			};
			PdfRecordCollection item = contentParser.ReadContent();
			recordCollectionList.Add(item);
			new PdfStream();
			PdfResources resources = page.GetResources();
			if (resources != null && !fontkeys.ContainsKey(resources))
			{
				fontkeys.Add(resources, contentParser.fontkeys);
			}
			PdfDictionary fontResources = null;
			if (resources != null && resources.ContainsKey("Font"))
			{
				fontResources = GetDictionaryFromRefernceHolder(resources["Font"]);
			}
			RetrieveFontData(contentParser.fontkeys, fontResources);
			memoryStream.Dispose();
			contentParser.Dispose();
			num++;
			if (document.RaiseTrackPdfAConversionProgress)
			{
				float num2 = 25f / (float)document.Pages.Count;
				num2 += args.m_progressValue;
				if ((int)args.m_progressValue != (int)num2)
				{
					args.m_progressValue = (int)num2;
					document.OnPdfAConversionTrackProgress(args);
					args.m_progressValue = num2;
				}
				else
				{
					args.m_progressValue = num2;
				}
			}
		}
	}

	private void RetrieveFontData(List<string> fontkeys, PdfDictionary fontResources)
	{
		if (fontResources == null)
		{
			return;
		}
		int count = fontkeys.Count;
		for (int i = 0; i < count; i++)
		{
			string text = fontkeys[i];
			if (!(text != "TJ") || fontResources[text] == null || UsedFonts.Contains(text))
			{
				continue;
			}
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(fontResources[text]);
			PdfDictionary fontDescriptor = null;
			PdfDictionary descendantFont = null;
			GetFontInternals(dictionaryFromRefernceHolder, out fontDescriptor, out descendantFont);
			if (dictionaryFromRefernceHolder.ContainsKey("Subtype") && (dictionaryFromRefernceHolder["Subtype"] as PdfName).Value == "Type1" && dictionaryFromRefernceHolder.ContainsKey("BaseFont"))
			{
				string value = (dictionaryFromRefernceHolder["BaseFont"] as PdfName).Value;
				PdfFontFamily fontFamily = GetFontFamily(value);
				if ((fontFamily == PdfFontFamily.Helvetica || fontFamily == PdfFontFamily.TimesRoman) && (fontDescriptor == null || (!fontDescriptor.ContainsKey("FontFile") && !fontDescriptor.ContainsKey("FontFile2") && !fontDescriptor.ContainsKey("FontFile3"))))
				{
					FontStructure fontStructure = new FontStructure(dictionaryFromRefernceHolder);
					if ((fontStructure.CharacterMapTable == null || fontStructure.CharacterMapTable.Count <= 0) && (fontStructure.DifferencesDictionary == null || fontStructure.DifferencesDictionary.Count <= 0) && (fontDescriptor == null || (fontDescriptor != null && fontDescriptor.ContainsKey("Flags") && (fontDescriptor["Flags"] as PdfNumber).IntValue != 4 && (fontDescriptor["Flags"] as PdfNumber).IntValue != 32)))
					{
						UsedFonts.Add(text);
					}
				}
			}
			else if (fontDescriptor == null || (!fontDescriptor.ContainsKey("FontFile") && !fontDescriptor.ContainsKey("FontFile2") && !fontDescriptor.ContainsKey("FontFile3")))
			{
				FontStructure fontStructure2 = new FontStructure(dictionaryFromRefernceHolder);
				if ((fontStructure2.CharacterMapTable == null || fontStructure2.CharacterMapTable.Count <= 0) && (fontStructure2.DifferencesDictionary == null || fontStructure2.DifferencesDictionary.Count <= 0) && (fontDescriptor == null || (fontDescriptor != null && fontDescriptor.ContainsKey("Flags") && (fontDescriptor["Flags"] as PdfNumber).IntValue != 4 && (fontDescriptor["Flags"] as PdfNumber).IntValue != 32)))
				{
					UsedFonts.Add(text);
				}
			}
			fontDescriptor = null;
			descendantFont = null;
		}
	}

	private PdfStream ParseContentStream(PdfStream stream, PdfPageBase lPage, int pageNo)
	{
		List<string> list = new List<string>();
		PdfResources resources = lPage.GetResources();
		PdfDictionary pdfDictionary = null;
		if (resources != null && resources.ContainsKey("Font"))
		{
			pdfDictionary = GetDictionaryFromRefernceHolder(resources["Font"]);
		}
		string text = null;
		float fontSize = 12f;
		PdfDictionary pdfDictionary2 = null;
		PdfRecordCollection pdfRecordCollection = recordCollectionList[pageNo];
		int count = pdfRecordCollection.RecordCollection.Count;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < count; i++)
		{
			PdfRecord pdfRecord = pdfRecordCollection.RecordCollection[i];
			if (pdfRecord.Operands != null && pdfRecord.Operands.Length >= 1)
			{
				if (pdfRecord.OperatorName == "gs")
				{
					flag = false;
					StringBuilder stringBuilder = new StringBuilder();
					IPdfPrimitive pdfPrimitive = resources["ExtGState"];
					string text2 = pdfRecord.Operands[0].ToString(CultureInfo.InvariantCulture).Replace("/", "");
					if (pdfPrimitive != null && GetDictionaryFromRefernceHolder(pdfPrimitive).Items.ContainsKey(new PdfName(text2)))
					{
						IPdfPrimitive primitive = GetDictionaryFromRefernceHolder(pdfPrimitive).Items[new PdfName(text2)];
						PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(primitive);
						bool flag3 = false;
						if (dictionaryFromRefernceHolder.ContainsKey("BM"))
						{
							if (dictionaryFromRefernceHolder["BM"] is PdfName)
							{
								if ((dictionaryFromRefernceHolder["BM"] as PdfName).Value == "Normal" || (dictionaryFromRefernceHolder["BM"] as PdfName).Value == "Compatible")
								{
									flag3 = true;
								}
								else
								{
									dictionaryFromRefernceHolder["BM"] = new PdfName("Normal");
									dictionaryFromRefernceHolder.Modify();
									flag3 = true;
								}
							}
							else if (dictionaryFromRefernceHolder["BM"] is PdfArray)
							{
								List<IPdfPrimitive> list2 = new List<IPdfPrimitive>();
								foreach (IPdfPrimitive item in dictionaryFromRefernceHolder["BM"] as PdfArray)
								{
									list2.Add(item);
								}
								for (int j = 0; j < list2.Count; j++)
								{
									if (list2[j] is PdfName && ((list2[j] as PdfName).Value == "Normal" || (list2[j] as PdfName).Value == "Compatible"))
									{
										flag3 = true;
									}
									else if (list2[j] is PdfName)
									{
										(dictionaryFromRefernceHolder["BM"] as PdfArray).Elements[j] = new PdfName("Normal");
										(dictionaryFromRefernceHolder["BM"] as PdfArray).MarkChanged();
									}
								}
							}
						}
						if (flag3)
						{
							if (dictionaryFromRefernceHolder.ContainsKey("ca"))
							{
								nonStrokingOpacity = (dictionaryFromRefernceHolder["ca"] as PdfNumber).FloatValue;
								hasNonStroking = true;
							}
							else
							{
								hasNonStroking = false;
							}
							if (dictionaryFromRefernceHolder.ContainsKey("CA"))
							{
								strokingOpacity = (dictionaryFromRefernceHolder["CA"] as PdfNumber).FloatValue;
								hasStroking = true;
							}
							else
							{
								hasStroking = false;
							}
							if (!list.Contains(text2))
							{
								list.Add(text2);
							}
							flag = true;
						}
						else
						{
							if (dictionaryFromRefernceHolder.ContainsKey("ca"))
							{
								if ((dictionaryFromRefernceHolder["ca"] as PdfNumber).FloatValue == 0f)
								{
									hasNoCa = true;
								}
								if (PdfALevel == PdfConformanceLevel.Pdf_A1B)
								{
									dictionaryFromRefernceHolder["ca"] = new PdfNumber(1);
									dictionaryFromRefernceHolder.Modify();
								}
							}
							if (dictionaryFromRefernceHolder.ContainsKey("CA") && PdfALevel == PdfConformanceLevel.Pdf_A1B)
							{
								dictionaryFromRefernceHolder["CA"] = new PdfNumber(1);
								dictionaryFromRefernceHolder.Modify();
							}
						}
						if (dictionaryFromRefernceHolder.ContainsKey("TR"))
						{
							dictionaryFromRefernceHolder.Remove("TR");
						}
						if (dictionaryFromRefernceHolder.ContainsKey("TR2"))
						{
							dictionaryFromRefernceHolder.Remove("TR2");
						}
					}
					if (!flag && pdfPrimitive != null)
					{
						stringBuilder.Append(pdfRecord.Operands[0]);
						stringBuilder.Append(" ");
						string s = stringBuilder.ToString();
						byte[] bytes = Encoding.Default.GetBytes(s);
						stream.Write(bytes);
					}
				}
				else if (pdfRecord.OperatorName == "Tf" && pdfDictionary != null)
				{
					int k;
					for (k = 0; k < pdfRecord.Operands.Length; k++)
					{
						if (pdfRecord.Operands[k].Contains("/"))
						{
							text = pdfRecord.Operands[k].Replace("/", "");
							break;
						}
					}
					fontSize = float.Parse(pdfRecord.Operands[k + 1], CultureInfo.InvariantCulture);
					if (pdfDictionary.ContainsKey(text))
					{
						pdfDictionary2 = GetDictionaryFromRefernceHolder(pdfDictionary[text]);
					}
					for (int l = 0; l < pdfRecord.Operands.Length; l++)
					{
						string operand = pdfRecord.Operands[l];
						operand = TrimOperand(operand);
						PdfString pdfString = new PdfString(operand);
						stream.Write(pdfString.Bytes);
						stream.Write(" ");
					}
				}
				else if (pdfRecord.OperatorName == "Do")
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					IPdfPrimitive pdfPrimitive2 = resources["XObject"];
					string value = pdfRecord.Operands[0].Replace("/", "");
					if (pdfPrimitive2 != null && GetDictionaryFromRefernceHolder(pdfPrimitive2).Items.ContainsKey(new PdfName(value)))
					{
						PdfReferenceHolder imageObject = GetDictionaryFromRefernceHolder(pdfPrimitive2)[new PdfName(value)] as PdfReferenceHolder;
						RemoveSMask(imageObject, resources, pdfRecord.Operands[0]);
					}
					stringBuilder2.Append(pdfRecord.Operands[0]);
					stringBuilder2.Append(" ");
					string s2 = stringBuilder2.ToString();
					byte[] bytes2 = Encoding.Default.GetBytes(s2);
					stream.Write(bytes2);
				}
				else if (pdfRecord.OperatorName == "CS" || pdfRecord.OperatorName == "cs")
				{
					StringBuilder stringBuilder3 = new StringBuilder();
					string text3 = pdfRecord.Operands[0].Replace("/", "");
					Colorspace colorspace = GetColorspace(pdfRecord.Operands, resources);
					switch (text3)
					{
					case "DeviceRGB":
						this.colorspace = new DeviceRGB();
						break;
					case "DeviceCMYK":
					{
						stringBuilder3.Append("/DeviceRGB");
						stringBuilder3.Append(" ");
						string s3 = stringBuilder3.ToString();
						byte[] bytes3 = Encoding.Default.GetBytes(s3);
						stream.Write(bytes3);
						appliedSCColorSpace = "DeviceCMYK";
						this.colorspace = new DeviceCMYK();
						break;
					}
					case "DeviceGray":
						this.colorspace = new DeviceGray();
						break;
					default:
					{
						if (!resources.ContainsKey(new PdfName("ColorSpace")))
						{
							break;
						}
						PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(resources["ColorSpace"]);
						if (dictionaryFromRefernceHolder2 == null || !dictionaryFromRefernceHolder2.ContainsKey(text3))
						{
							break;
						}
						IPdfPrimitive pdfPrimitive3 = dictionaryFromRefernceHolder2[text3];
						List<IPdfPrimitive> list3 = new List<IPdfPrimitive>();
						if (pdfPrimitive3 is PdfReferenceHolder)
						{
							list3 = ((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray).Elements;
						}
						else if (pdfPrimitive3 is PdfArray)
						{
							list3 = (pdfPrimitive3 as PdfArray).Elements;
						}
						if (list3 == null)
						{
							break;
						}
						if (colorspace is Separation)
						{
							this.colorspace = new Separation();
							(this.colorspace as Separation).SetValue((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray);
							appliedSCNColorSpace = "Separation";
							if (list3.Contains(new PdfName("DeviceCMYK")))
							{
								PdfReferenceHolder value2 = new PdfReferenceHolder(new PdfICCColorSpace
								{
									ColorComponents = 4,
									AlternateColorSpace = new PdfDeviceColorSpace(PdfColorSpace.CMYK)
								});
								dictionaryFromRefernceHolder2[new PdfName("DefaultCMYK")] = value2;
							}
						}
						else if (colorspace is Pattern)
						{
							this.colorspace = new Pattern();
							(this.colorspace as Pattern).SetValue((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray);
							appliedSCNColorSpace = "Pattern";
						}
						else if (colorspace is Indexed)
						{
							this.colorspace = new Indexed();
							(this.colorspace as Indexed).SetValue((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray);
							appliedSCColorSpace = "Indexed";
						}
						else if (colorspace is CalRGB)
						{
							this.colorspace = new CalRGB();
							(this.colorspace as CalRGB).SetValue((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray);
							appliedSCColorSpace = "CalRGB";
						}
						else if (colorspace is CalGray)
						{
							this.colorspace = new CalGray();
							(this.colorspace as CalGray).SetValue((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray);
							appliedSCColorSpace = "CalGray";
						}
						else if (colorspace is LabColor)
						{
							this.colorspace = new LabColor();
							(this.colorspace as LabColor).SetValue((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray);
							appliedSCColorSpace = "Lab";
						}
						else if (colorspace is DeviceN)
						{
							this.colorspace = new DeviceN();
							(this.colorspace as DeviceN).SetValue((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray);
							appliedSCNColorSpace = "DeviceN";
						}
						else if (colorspace is ICCBased)
						{
							this.colorspace = new ICCBased();
							(this.colorspace as ICCBased).Profile = new ICCProfile((pdfPrimitive3 as PdfReferenceHolder).Object as PdfArray);
							appliedSCNColorSpace = "ICCBased";
						}
						else if (colorspace is DeviceCMYK)
						{
							this.colorspace = new DeviceCMYK();
						}
						else if (colorspace is DeviceRGB)
						{
							this.colorspace = new DeviceRGB();
						}
						else if (colorspace is DeviceGray)
						{
							this.colorspace = new DeviceGray();
						}
						break;
					}
					}
					if (text3 != "DeviceCMYK")
					{
						stringBuilder3.Append(pdfRecord.Operands[0]);
						stringBuilder3.Append(" ");
						string s4 = stringBuilder3.ToString();
						byte[] bytes4 = Encoding.Default.GetBytes(s4);
						stream.Write(bytes4);
					}
				}
				else if ((pdfRecord.OperatorName == "RG" || pdfRecord.OperatorName == "rg") && flag)
				{
					StringBuilder stringBuilder4 = new StringBuilder();
					Color color = new DeviceRGB().GetColor(pdfRecord.Operands);
					float[] array = new float[3]
					{
						(int)color.R,
						(int)color.G,
						(int)color.B
					};
					array = ((pdfRecord.OperatorName == "RG") ? ((!hasStroking) ? ConvertTransparencyToRGB(array[0], array[1], array[2], -1f) : ConvertTransparencyToRGB(array[0], array[1], array[2], strokingOpacity)) : ((!hasNonStroking) ? ConvertTransparencyToRGB(array[0], array[1], array[2], -1f) : ConvertTransparencyToRGB(array[0], array[1], array[2], nonStrokingOpacity)));
					stringBuilder4.Append(Math.Round(array[0], 2).ToString(CultureInfo.InvariantCulture));
					stringBuilder4.Append(" ");
					stringBuilder4.Append(Math.Round(array[1], 2).ToString(CultureInfo.InvariantCulture));
					stringBuilder4.Append(" ");
					stringBuilder4.Append(Math.Round(array[2], 2).ToString(CultureInfo.InvariantCulture));
					stringBuilder4.Append(" ");
					string s5 = stringBuilder4.ToString();
					byte[] bytes5 = Encoding.Default.GetBytes(s5);
					stream.Write(bytes5);
				}
				else if (pdfRecord.OperatorName == "K" || pdfRecord.OperatorName == "k")
				{
					StringBuilder stringBuilder5 = new StringBuilder();
					Color color2 = new DeviceCMYK().GetColor(pdfRecord.Operands);
					float[] array2 = new float[3]
					{
						(int)color2.R,
						(int)color2.G,
						(int)color2.B
					};
					if (flag)
					{
						array2 = ((pdfRecord.OperatorName == "K") ? ((!hasStroking) ? ConvertTransparencyToRGB(array2[0], array2[1], array2[2], -1f) : ConvertTransparencyToRGB(array2[0], array2[1], array2[2], strokingOpacity)) : ((!hasNonStroking) ? ConvertTransparencyToRGB(array2[0], array2[1], array2[2], -1f) : ConvertTransparencyToRGB(array2[0], array2[1], array2[2], nonStrokingOpacity)));
					}
					else
					{
						array2[0] /= 255f;
						array2[1] /= 255f;
						array2[2] /= 255f;
					}
					stringBuilder5.Append(Math.Round(array2[0], 2).ToString(CultureInfo.InvariantCulture));
					stringBuilder5.Append(" ");
					stringBuilder5.Append(Math.Round(array2[1], 2).ToString(CultureInfo.InvariantCulture));
					stringBuilder5.Append(" ");
					stringBuilder5.Append(Math.Round(array2[2], 2).ToString(CultureInfo.InvariantCulture));
					stringBuilder5.Append(" ");
					string s6 = stringBuilder5.ToString();
					byte[] bytes6 = Encoding.Default.GetBytes(s6);
					stream.Write(bytes6);
				}
				else if ((pdfRecord.OperatorName == "G" || pdfRecord.OperatorName == "g") && flag)
				{
					StringBuilder stringBuilder6 = new StringBuilder();
					float num = (float)System.Convert.ToDouble(pdfRecord.Operands[0], CultureInfo.InvariantCulture);
					if (pdfRecord.OperatorName == "G" && hasStroking)
					{
						num = num * strokingOpacity + (1f - strokingOpacity);
					}
					else if (pdfRecord.OperatorName == "G" && hasNonStroking)
					{
						num = num * nonStrokingOpacity + (1f - nonStrokingOpacity);
					}
					num = (float)Math.Round(num, 2);
					stringBuilder6.Append(num.ToString(CultureInfo.InvariantCulture));
					stringBuilder6.Append(" ");
					stringBuilder6.Append(num.ToString(CultureInfo.InvariantCulture));
					stringBuilder6.Append(" ");
					stringBuilder6.Append(num.ToString(CultureInfo.InvariantCulture));
					stringBuilder6.Append(" ");
					string s7 = stringBuilder6.ToString();
					byte[] bytes7 = Encoding.Default.GetBytes(s7);
					stream.Write(bytes7);
				}
				else if ((pdfRecord.OperatorName == "SCN" || pdfRecord.OperatorName == "scn") && flag)
				{
					StringBuilder stringBuilder7 = new StringBuilder();
					if (this.colorspace == null)
					{
						this.colorspace = new DeviceCMYK();
					}
					bool flag4 = false;
					if (pdfRecord.Operands.Length == 1 && pdfRecord.Operands[0][0] == '/')
					{
						string key = pdfRecord.Operands[0].Substring(1);
						if (resources.ContainsKey("Pattern"))
						{
							PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(resources["Pattern"]);
							if (dictionaryFromRefernceHolder3 != null && dictionaryFromRefernceHolder3.ContainsKey(key))
							{
								flag4 = true;
							}
						}
					}
					if (flag4)
					{
						stringBuilder7.Append(pdfRecord.Operands[0]);
						stringBuilder7.Append(" ");
						stringBuilder7.Append(pdfRecord.OperatorName);
					}
					else
					{
						Color color3 = this.colorspace.GetColor(pdfRecord.Operands);
						float[] array3 = new float[3]
						{
							(int)color3.R,
							(int)color3.G,
							(int)color3.B
						};
						array3 = ((pdfRecord.OperatorName == "SCN") ? ((!hasStroking) ? ConvertTransparencyToRGB(array3[0], array3[1], array3[2], -1f) : ConvertTransparencyToRGB(array3[0], array3[1], array3[2], strokingOpacity)) : ((!hasNonStroking) ? ConvertTransparencyToRGB(array3[0], array3[1], array3[2], -1f) : ConvertTransparencyToRGB(array3[0], array3[1], array3[2], nonStrokingOpacity)));
						stringBuilder7.Append(Math.Round(array3[0], 2).ToString(CultureInfo.InvariantCulture));
						stringBuilder7.Append(" ");
						stringBuilder7.Append(Math.Round(array3[1], 2).ToString(CultureInfo.InvariantCulture));
						stringBuilder7.Append(" ");
						stringBuilder7.Append(Math.Round(array3[2], 2).ToString(CultureInfo.InvariantCulture));
						stringBuilder7.Append(" ");
						if (pdfRecord.OperatorName == "SCN")
						{
							stringBuilder7.Append("RG");
						}
						else
						{
							stringBuilder7.Append("rg");
						}
					}
					stringBuilder7.Append("\r\n");
					string s8 = stringBuilder7.ToString();
					byte[] bytes8 = Encoding.Default.GetBytes(s8);
					stream.Write(bytes8);
				}
				else if ((pdfRecord.OperatorName == "SC" || pdfRecord.OperatorName == "sc") && flag)
				{
					StringBuilder stringBuilder8 = new StringBuilder();
					if (pdfRecord.Operands.Length == 1)
					{
						float num2 = (float)System.Convert.ToDouble(pdfRecord.Operands[0], CultureInfo.InvariantCulture);
						if (pdfRecord.OperatorName == "G" && hasStroking)
						{
							num2 = num2 * strokingOpacity + (1f - strokingOpacity);
						}
						else if (pdfRecord.OperatorName == "G" && hasNonStroking)
						{
							num2 = num2 * nonStrokingOpacity + (1f - nonStrokingOpacity);
						}
						stringBuilder8.Append(((float)Math.Round(num2, 2)).ToString(CultureInfo.InvariantCulture));
						stringBuilder8.Append(" ");
						if (pdfRecord.OperatorName == "SC")
						{
							stringBuilder8.Append("G");
						}
						else
						{
							stringBuilder8.Append("g");
						}
					}
					else if (pdfRecord.Operands.Length == 3)
					{
						Color color4 = this.colorspace.GetColor(pdfRecord.Operands);
						float[] array4 = new float[3]
						{
							(int)color4.R,
							(int)color4.G,
							(int)color4.B
						};
						array4 = ((pdfRecord.OperatorName == "SC") ? ((!hasStroking) ? ConvertTransparencyToRGB(array4[0], array4[1], array4[2], -1f) : ConvertTransparencyToRGB(array4[0], array4[1], array4[2], strokingOpacity)) : ((!hasNonStroking) ? ConvertTransparencyToRGB(array4[0], array4[1], array4[2], -1f) : ConvertTransparencyToRGB(array4[0], array4[1], array4[2], nonStrokingOpacity)));
						stringBuilder8.Append(Math.Round(array4[0], 2).ToString(CultureInfo.InvariantCulture));
						stringBuilder8.Append(" ");
						stringBuilder8.Append(Math.Round(array4[1], 2).ToString(CultureInfo.InvariantCulture));
						stringBuilder8.Append(" ");
						stringBuilder8.Append(Math.Round(array4[2], 2).ToString(CultureInfo.InvariantCulture));
						stringBuilder8.Append(" ");
						if (pdfRecord.OperatorName == "SC")
						{
							stringBuilder8.Append("RG");
						}
						else
						{
							stringBuilder8.Append("rg");
						}
					}
					stringBuilder8.Append("\r\n");
					string s9 = stringBuilder8.ToString();
					byte[] bytes9 = Encoding.Default.GetBytes(s9);
					stream.Write(bytes9);
				}
				else if (pdfRecord.OperatorName == "Tj" || pdfRecord.OperatorName == "'")
				{
					if (hasNoCa)
					{
						StringBuilder stringBuilder9 = new StringBuilder();
						stringBuilder9.Append("3 Tr");
						string s10 = stringBuilder9.ToString();
						byte[] bytes10 = Encoding.Default.GetBytes(s10);
						stream.Write(bytes10);
						stream.Write("\r\n");
					}
					PdfDictionary fontDescriptor = null;
					PdfDictionary descendantFont = null;
					GetFontInternals(pdfDictionary2, out fontDescriptor, out descendantFont);
					bool flag5 = false;
					if (!string.IsNullOrEmpty(text) && OldFonts.ContainsKey(text) && ReplaceFonts.ContainsKey(text) && ReplaceFonts[text] != null)
					{
						PdfDictionary dictionaryFromRefernceHolder4 = GetDictionaryFromRefernceHolder(ReplaceFonts[text].FontInternal);
						if (dictionaryFromRefernceHolder4 != null && dictionaryFromRefernceHolder4.ContainsKey("Subtype") && (dictionaryFromRefernceHolder4["Subtype"] as PdfName).Value == "Type0")
						{
							FontStructure obj = new FontStructure(OldFonts[text])
							{
								FontSize = fontSize,
								isDecodingConformance = true
							};
							string text4 = obj.DecodeTextExtraction(string.Join("", pdfRecord.Operands), isSameFont: true);
							obj.isDecodingConformance = false;
							text4 = ConvertToUnicode(text4, ReplaceFonts[text]);
							PdfString unicodeString = GetUnicodeString(text4);
							resources.Remove(text);
							resources.Add(ReplaceFonts[text], new PdfName(text));
							stream.Write(unicodeString.PdfEncode(null));
							stream.Write(" ");
							flag5 = true;
						}
					}
					else if (!string.IsNullOrEmpty(text) && ReplaceFontDictionary.ContainsKey(text) && OldFonts.ContainsKey(text))
					{
						pdfDictionary2 = OldFonts[text];
						if (pdfDictionary2.ContainsKey("Encoding") && PdfCrossTable.Dereference(pdfDictionary2["Encoding"]) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Differences"))
						{
							resources.Remove(text);
							PdfDictionary pdfDictionary4 = null;
							IPdfPrimitive pdfPrimitive4 = resources["Font"];
							if (pdfPrimitive4 == null)
							{
								pdfDictionary4 = (PdfDictionary)(resources["Font"] = new PdfDictionary());
							}
							else
							{
								PdfReferenceHolder pdfReferenceHolder = pdfPrimitive4 as PdfReferenceHolder;
								pdfDictionary4 = pdfPrimitive4 as PdfDictionary;
								if (pdfReferenceHolder != null)
								{
									pdfDictionary4 = PdfCrossTable.Dereference(pdfPrimitive4) as PdfDictionary;
								}
							}
							pdfDictionary4[new PdfName(text)] = ReplaceFontDictionary[text];
						}
					}
					if (!flag5)
					{
						string text5 = TrimOperand(pdfRecord.Operands[0]);
						if (pdfFonts != null && pdfFonts.ContainsKey(text))
						{
							FontStructure fontStructure = new FontStructure(pdfFonts[text]);
							fontStructure.FontSize = fontSize;
							text5 = ConvertToMapHex(text5, pdfFonts[text], fontStructure);
						}
						PdfString pdfString2 = new PdfString(text5);
						stream.Write(pdfString2.Bytes);
					}
				}
				else if (pdfRecord.OperatorName == "TJ")
				{
					_ = string.Empty;
					PdfDictionary fontDescriptor2 = null;
					PdfDictionary descendantFont2 = null;
					GetFontInternals(pdfDictionary2, out fontDescriptor2, out descendantFont2);
					bool flag6 = false;
					if (fontDescriptor2 == null || (fontDescriptor2 != null && fontDescriptor2.ContainsKey("Flags") && (fontDescriptor2["Flags"] as PdfNumber).IntValue != 4 && (fontDescriptor2["Flags"] as PdfNumber).IntValue != 32))
					{
						if (!string.IsNullOrEmpty(text) && OldFonts.ContainsKey(text) && ReplaceFonts.ContainsKey(text) && ReplaceFonts[text] != null)
						{
							PdfDictionary dictionaryFromRefernceHolder5 = GetDictionaryFromRefernceHolder(ReplaceFonts[text].FontInternal);
							if (dictionaryFromRefernceHolder5 != null && dictionaryFromRefernceHolder5.ContainsKey("Subtype") && (dictionaryFromRefernceHolder5["Subtype"] as PdfName).Value == "Type0")
							{
								string text6 = new FontStructure(OldFonts[text])
								{
									FontSize = fontSize
								}.DecodeTextExtraction(string.Join("", pdfRecord.Operands), isSameFont: true);
								text6 = ConvertToUnicode(text6, ReplaceFonts[text]);
								PdfString unicodeString2 = GetUnicodeString(text6);
								resources.Remove(text);
								resources.Add(ReplaceFonts[text], new PdfName(text));
								stream.Write(unicodeString2.PdfEncode(null));
								stream.Write(" ");
								flag6 = true;
								pdfRecord.OperatorName = "Tj";
							}
							else if (ReplaceFonts.ContainsKey(text) && OldFonts.ContainsKey(text))
							{
								pdfDictionary2 = OldFonts[text];
								if (pdfDictionary2.ContainsKey("Encoding") && PdfCrossTable.Dereference(pdfDictionary2["Encoding"]) is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey("Differences"))
								{
									FontStructure obj2 = new FontStructure(pdfDictionary2)
									{
										FontSize = fontSize,
										isDecodingConformance = true
									};
									string text7 = obj2.DecodeTextExtraction(string.Join("", pdfRecord.Operands), isSameFont: true);
									obj2.isDecodingConformance = false;
									text7 = ConvertToUnicode(text7, ReplaceFonts[text]);
									PdfString unicodeString3 = GetUnicodeString(text7);
									resources.Remove(text);
									resources.Add(ReplaceFonts[text], new PdfName(text));
									stream.Write(unicodeString3.PdfEncode(null));
									stream.Write(" ");
									flag6 = true;
									pdfRecord.OperatorName = "Tj";
								}
							}
						}
					}
					else if (ReplaceFonts.ContainsKey(text) && OldFonts.ContainsKey(text))
					{
						pdfDictionary2 = OldFonts[text];
						if (pdfDictionary2.ContainsKey("Encoding") && PdfCrossTable.Dereference(pdfDictionary2["Encoding"]) is PdfDictionary pdfDictionary6 && pdfDictionary6.ContainsKey("Differences"))
						{
							string text8 = new FontStructure(pdfDictionary2)
							{
								FontSize = fontSize
							}.DecodeTextExtraction(string.Join("", pdfRecord.Operands), isSameFont: true);
							text8 = ConvertToUnicode(text8, ReplaceFonts[text]);
							PdfString unicodeString4 = GetUnicodeString(text8);
							resources.Remove(text);
							resources.Add(ReplaceFonts[text], new PdfName(text));
							stream.Write(unicodeString4.PdfEncode(null));
							stream.Write(" ");
							flag6 = true;
							pdfRecord.OperatorName = "Tj";
						}
					}
					else if (!string.IsNullOrEmpty(text) && ReplaceFontDictionary.ContainsKey(text) && OldFonts.ContainsKey(text))
					{
						pdfDictionary2 = OldFonts[text];
						if (pdfDictionary2.ContainsKey("Encoding") && PdfCrossTable.Dereference(pdfDictionary2["Encoding"]) is PdfDictionary pdfDictionary7 && pdfDictionary7.ContainsKey("Differences"))
						{
							resources.Remove(text);
							PdfDictionary pdfDictionary8 = null;
							IPdfPrimitive pdfPrimitive6 = resources["Font"];
							if (pdfPrimitive6 == null)
							{
								pdfDictionary8 = (PdfDictionary)(resources["Font"] = new PdfDictionary());
							}
							else
							{
								PdfReferenceHolder pdfReferenceHolder2 = pdfPrimitive6 as PdfReferenceHolder;
								pdfDictionary8 = pdfPrimitive6 as PdfDictionary;
								if (pdfReferenceHolder2 != null)
								{
									pdfDictionary8 = PdfCrossTable.Dereference(pdfPrimitive6) as PdfDictionary;
								}
							}
							pdfDictionary8[new PdfName(text)] = ReplaceFontDictionary[text];
						}
					}
					if (!flag6)
					{
						PdfString pdfString3 = new PdfString(TrimOperand(pdfRecord.Operands[0]));
						stream.Write(pdfString3.Bytes);
						stream.Write(" ");
					}
				}
				else if (pdfRecord.OperatorName == "\"")
				{
					PdfDictionary fontDescriptor3 = null;
					PdfDictionary descendantFont3 = null;
					GetFontInternals(pdfDictionary2, out fontDescriptor3, out descendantFont3);
					bool flag7 = false;
					if (fontDescriptor3 == null || (fontDescriptor3 != null && fontDescriptor3.ContainsKey("Flags") && (fontDescriptor3["Flags"] as PdfNumber).IntValue != 4 && (fontDescriptor3["Flags"] as PdfNumber).IntValue != 32))
					{
						if (!string.IsNullOrEmpty(text) && OldFonts.ContainsKey(text) && ReplaceFonts.ContainsKey(text) && ReplaceFonts[text] != null)
						{
							PdfDictionary dictionaryFromRefernceHolder6 = GetDictionaryFromRefernceHolder(ReplaceFonts[text].FontInternal);
							if (dictionaryFromRefernceHolder6 != null && dictionaryFromRefernceHolder6.ContainsKey("Subtype") && (dictionaryFromRefernceHolder6["Subtype"] as PdfName).Value == "Type0")
							{
								StringBuilder stringBuilder10 = new StringBuilder();
								stringBuilder10.Append(TrimOperand(pdfRecord.Operands[0]));
								stringBuilder10.Append(" Tw");
								stringBuilder10.Append("\r\n");
								stringBuilder10.Append(TrimOperand(pdfRecord.Operands[1]));
								stringBuilder10.Append(" Tc");
								stringBuilder10.Append("\r\n");
								string text9 = new FontStructure(OldFonts[text])
								{
									FontSize = fontSize
								}.DecodeTextExtraction(pdfRecord.Operands[2], isSameFont: true);
								text9 = ConvertToUnicode(text9, ReplaceFonts[text]);
								PdfString unicodeString5 = GetUnicodeString(text9);
								resources.Remove(text);
								resources.Add(ReplaceFonts[text], new PdfName(text));
								stream.Write(unicodeString5.PdfEncode(null));
								stream.Write(" ");
								flag7 = true;
							}
						}
						else if (!string.IsNullOrEmpty(text) && ReplaceFontDictionary.ContainsKey(text) && OldFonts.ContainsKey(text))
						{
							pdfDictionary2 = OldFonts[text];
							if (pdfDictionary2.ContainsKey("Encoding") && PdfCrossTable.Dereference(pdfDictionary2["Encoding"]) is PdfDictionary pdfDictionary9 && pdfDictionary9.ContainsKey("Differences"))
							{
								resources.Remove(text);
								PdfDictionary pdfDictionary10 = null;
								IPdfPrimitive pdfPrimitive8 = resources["Font"];
								if (pdfPrimitive8 == null)
								{
									pdfDictionary10 = (PdfDictionary)(resources["Font"] = new PdfDictionary());
								}
								else
								{
									PdfReferenceHolder pdfReferenceHolder3 = pdfPrimitive8 as PdfReferenceHolder;
									pdfDictionary10 = pdfPrimitive8 as PdfDictionary;
									if (pdfReferenceHolder3 != null)
									{
										pdfDictionary10 = PdfCrossTable.Dereference(pdfPrimitive8) as PdfDictionary;
									}
								}
								pdfDictionary10[new PdfName(text)] = ReplaceFontDictionary[text];
							}
						}
					}
					if (!flag7)
					{
						for (int m = 0; m < pdfRecord.Operands.Length; m++)
						{
							string operand2 = pdfRecord.Operands[m];
							operand2 = TrimOperand(operand2);
							PdfString pdfString4 = new PdfString(operand2);
							stream.Write(pdfString4.Bytes);
							stream.Write(" ");
						}
					}
				}
				else if (pdfRecord.OperatorName == "ID")
				{
					for (int n = 0; n < pdfRecord.Operands.Length; n++)
					{
						string text10 = pdfRecord.Operands[n];
						if (text10.IndexOf("/") == 0)
						{
							text10 = text10.Replace("/", "");
						}
						if (text10 == "F" || (text10 == "Filter" && pdfRecord.Operands.Length > n + 1))
						{
							string text11 = pdfRecord.Operands[n + 1];
							if (text11.Contains("LZWDecode") || text11.Contains("LZW"))
							{
								pdfRecord.Operands[n + 1] = "[/FlateDecode]";
								flag2 = true;
							}
							else
							{
								flag2 = false;
							}
						}
						else
						{
							flag2 = false;
						}
						if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
						{
							text10 = TrimOperand(pdfRecord.Operands[n]);
						}
						PdfString pdfString5 = new PdfString(text10);
						stream.Write(pdfString5.Bytes);
						if (pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"")
						{
							stream.Write(" ");
						}
					}
				}
				else
				{
					for (int num3 = 0; num3 < pdfRecord.Operands.Length; num3++)
					{
						string text12 = pdfRecord.Operands[num3];
						if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
						{
							text12 = TrimOperand(text12);
						}
						PdfString pdfString6 = new PdfString(text12);
						stream.Write(pdfString6.Bytes);
						if (pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"")
						{
							stream.Write(" ");
						}
					}
				}
			}
			if (pdfRecord.OperatorName == "K" || (pdfRecord.OperatorName == "G" && flag))
			{
				stream.Write("RG");
			}
			else if (pdfRecord.OperatorName == "k" || (pdfRecord.OperatorName == "g" && flag))
			{
				stream.Write("rg");
			}
			else if (pdfRecord.OperatorName == "gs" && flag)
			{
				if (PdfALevel == PdfConformanceLevel.Pdf_A1B)
				{
					continue;
				}
				for (int num4 = 0; num4 < pdfRecord.Operands.Length; num4++)
				{
					string operand3 = pdfRecord.Operands[num4];
					operand3 = TrimOperand(operand3);
					PdfString pdfString7 = new PdfString(operand3);
					stream.Write(pdfString7.Bytes);
					stream.Write(" ");
					stream.Write("gs");
				}
			}
			else
			{
				if ((pdfRecord.OperatorName == "SCN" || pdfRecord.OperatorName == "scn" || pdfRecord.OperatorName == "SC" || pdfRecord.OperatorName == "sc") && flag)
				{
					continue;
				}
				if ((pdfRecord.OperatorName == "SC" || pdfRecord.OperatorName == "sc") && !flag)
				{
					bool flag8 = pdfRecord.OperatorName == "SC";
					if (pdfRecord.Operands.Length == 1)
					{
						stream.Write(flag8 ? "G" : "g");
					}
					if (pdfRecord.Operands.Length == 3)
					{
						stream.Write(flag8 ? "RG" : "rg");
					}
					if (pdfRecord.Operands.Length == 4)
					{
						stream.Write(flag8 ? "K" : "k");
					}
				}
				else if (pdfRecord.OperatorName == "EI")
				{
					if (pdfRecord.InlineImageBytes.Length != 0)
					{
						if (flag2)
						{
							byte[] data = new PdfLzwCompressor().Decompress(pdfRecord.InlineImageBytes);
							PdfZlibCompressor pdfZlibCompressor = new PdfZlibCompressor();
							pdfRecord.InlineImageBytes = pdfZlibCompressor.Compress(data);
						}
						stream.Write(pdfRecord.InlineImageBytes);
						stream.Write(pdfRecord.OperatorName);
					}
					flag2 = false;
				}
				else
				{
					if (pdfRecord.OperatorName == "Q" || pdfRecord.OperatorName == "ET")
					{
						flag = false;
					}
					stream.Write(pdfRecord.OperatorName);
				}
			}
			if (i + 1 < count || isPdfPage)
			{
				if (i + 1 < count && (pdfRecord.OperatorName == "W" || pdfRecord.OperatorName == "W*") && pdfRecordCollection.RecordCollection[i + 1].OperatorName == "n")
				{
					stream.Write(" ");
				}
				else if (pdfRecord.OperatorName == "w" || pdfRecord.OperatorName == "ID")
				{
					stream.Write(" ");
				}
				else
				{
					stream.Write("\r\n");
				}
			}
		}
		if (count == 0)
		{
			stream.Write("q");
			stream.Write("\r\n");
			stream.Write("Q");
			stream.Write("\r\n");
		}
		if (PdfALevel == PdfConformanceLevel.Pdf_A1B && list.Count > 0 && resources.ContainsKey("ExtGState"))
		{
			IPdfPrimitive primitive2 = resources["ExtGState"];
			foreach (string item2 in list)
			{
				if (GetDictionaryFromRefernceHolder(primitive2).Items.Count == 1)
				{
					lPage.GetResources().Remove("ExtGState");
					lPage.GetResources().Modify();
				}
				else
				{
					GetDictionaryFromRefernceHolder(primitive2).Remove(new PdfName(item2));
					GetDictionaryFromRefernceHolder(primitive2).Modify();
				}
			}
		}
		pdfDictionary = null;
		ResetPageResources();
		pdfRecordCollection.RecordCollection.Clear();
		return stream;
	}

	private string ConvertToMapHex(string decodedText, PdfDictionary m_fontDictionary, FontStructure fontStructure)
	{
		string text = decodedText;
		string text2 = null;
		new List<string>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (text[0] == '<')
		{
			string empty = string.Empty;
			string text3 = text.Substring(1, text.Length - 2);
			if (text3.Length == 8)
			{
				if ((ushort)long.Parse(text3.Substring(text3.Length - text3.Length / 2), NumberStyles.HexNumber) != 0)
				{
					return decodedText;
				}
				if (!string.IsNullOrEmpty(text3) && m_fontDictionary != null)
				{
					PdfName pdfName = m_fontDictionary.Items[new PdfName("Subtype")] as PdfName;
					int num = 2;
					if (pdfName != null && pdfName.Value != "Type1" && pdfName.Value != "TrueType" && pdfName.Value != "Type3")
					{
						num = 4;
					}
					decodedText = string.Empty;
					while (text3.Length > 0)
					{
						if (text3.Length % 4 != 0)
						{
							num = 2;
						}
						empty = text3.Substring(0, num);
						char c = (char)long.Parse(empty, NumberStyles.HexNumber);
						if (!dictionary.ContainsKey(c.ToString()))
						{
							dictionary.Add(c.ToString(), empty);
						}
						decodedText += c;
						text3 = text3.Substring(num, text3.Length - num);
						text2 = decodedText.ToString();
					}
				}
				if (decodedText != null)
				{
					decodedText = string.Empty;
					decodedText += "<";
					string text4 = text2;
					for (int i = 0; i < text4.Length; i++)
					{
						char c2 = text4[i];
						if (fontStructure.CharacterMapTable != null && fontStructure.CharacterMapTable.ContainsKey((int)c2) && fontStructure.CharacterMapTable[(int)c2] != "\t" && dictionary.ContainsKey(c2.ToString()))
						{
							decodedText += dictionary[c2.ToString()];
						}
					}
					decodedText += ">";
					return decodedText;
				}
			}
		}
		return decodedText;
	}

	private PdfString GetUnicodeString(string token)
	{
		return new PdfString(token)
		{
			Converted = true,
			Encode = PdfString.ForceEncoding.ASCII
		};
	}

	public string ConvertString(string text, TtfReader ttfReader)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		char[] array = new char[text.Length];
		int length = 0;
		int i = 0;
		for (int length2 = text.Length; i < length2; i++)
		{
			char charCode = text[i];
			TtfGlyphInfo glyph = ttfReader.GetGlyph(charCode);
			if (!glyph.Empty)
			{
				array[length++] = (char)glyph.Index;
			}
		}
		return new string(array, 0, length);
	}

	private string ConvertToUnicode(string text, PdfTrueTypeFont ttfFont)
	{
		string result = string.Empty;
		if (ttfFont.InternalFont is UnicodeTrueTypeFont)
		{
			TtfReader ttfReader = (ttfFont.InternalFont as UnicodeTrueTypeFont).TtfReader;
			UnicodeTrueTypeFont unicodeTrueTypeFont = ttfFont.InternalFont as UnicodeTrueTypeFont;
			List<TtfGlyphInfo> list = new List<TtfGlyphInfo>();
			ttfFont.SetSymbols(text);
			unicodeTrueTypeFont.SetGlyphInfo(unicodeTrueTypeFont.GetGlyphInfo());
			PdfArray descendantWidth = unicodeTrueTypeFont.GetDescendantWidth();
			PdfDictionary pdfDictionary = new PdfDictionary();
			list = ttfReader.GetAllGlyphs();
			if (unicodeTrueTypeFont.GetUsedCharsCount() > list.Count)
			{
				unicodeTrueTypeFont.m_isIncreasedUsedChar = true;
			}
			pdfDictionary["W"] = descendantWidth;
			result = ttfReader.ConvertString(text);
			result = PdfString.ByteToString(PdfString.ToUnicodeArray(result, bAddPrefix: false));
		}
		return result;
	}

	private void ContentStreamParsing(PdfLoadedDocument document)
	{
		int num = 0;
		foreach (PdfPageBase page in document.Pages)
		{
			if (page is PdfPage)
			{
				isPdfPage = true;
			}
			_ = page.Contents;
			if (page.Dictionary.ContainsKey("AA"))
			{
				page.Dictionary.Remove("AA");
				page.Dictionary.Modify();
			}
			AnnotationConsideration(page, document);
			if (PdfALevel == PdfConformanceLevel.Pdf_A1B)
			{
				RemoveTransparencyGroup(page);
			}
			CompressionConsideration(page);
			EmbedCompleteFonts(page, document);
			ParseFormResources(page);
			PdfStream stream = new PdfStream();
			stream = ParseContentStream(stream, page, num);
			num++;
			if (!fontEmbedding)
			{
				ReplaceCMYKColorSpace(page);
			}
			if (page.Dictionary.ContainsKey("Contents"))
			{
				PdfArray arrayFromReferenceHolder = GetArrayFromReferenceHolder(page.Dictionary["Contents"]);
				if (arrayFromReferenceHolder != null)
				{
					foreach (IPdfPrimitive content in page.Contents)
					{
						PdfDictionary @object = GetObject(content);
						if (@object != null)
						{
							@object.isSkip = true;
						}
					}
					arrayFromReferenceHolder.Clear();
					arrayFromReferenceHolder.Add(new PdfReferenceHolder(stream));
				}
				else
				{
					PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(page.Dictionary["Contents"]);
					if (streamFromRefernceHolder != null)
					{
						streamFromRefernceHolder.Clear();
						streamFromRefernceHolder.Items.Remove(new PdfName("Length"));
						streamFromRefernceHolder.Data = stream.Data;
					}
				}
			}
			if (document.RaiseTrackPdfAConversionProgress)
			{
				float num2 = 50f / (float)document.Pages.Count;
				num2 += args.m_progressValue;
				if ((int)args.m_progressValue != (int)num2)
				{
					args.m_progressValue = (int)num2;
					document.OnPdfAConversionTrackProgress(args);
					args.m_progressValue = num2;
				}
				else
				{
					args.m_progressValue = num2;
				}
			}
		}
	}

	private void ParseAnnotations(PdfLoadedDocument document)
	{
		foreach (PdfPageBase page in document.Pages)
		{
			AnnotationConsideration(page, document);
		}
	}

	private void ParseFormResources(PdfPageBase lPage)
	{
		PdfResources resources = lPage.GetResources();
		if (resources.ContainsKey("XObject"))
		{
			ParseXObjectDictionary(GetDictionaryFromRefernceHolder(resources["XObject"]));
		}
	}

	private void ParseXObjectDictionary(PdfDictionary xObjectDictionary)
	{
		if (xObjectDictionary == null || xObjectDictionary.Items.Count <= 0)
		{
			return;
		}
		foreach (PdfName key in xObjectDictionary.Keys)
		{
			PdfReferenceHolder pdfReferenceHolder = xObjectDictionary.Items[key] as PdfReferenceHolder;
			if (!(pdfReferenceHolder != null) || !(pdfReferenceHolder.Reference != null) || (xobjectReference.Count != 0 && xobjectReference.Contains(pdfReferenceHolder.Reference.ObjNum)))
			{
				continue;
			}
			xobjectReference.Add(pdfReferenceHolder.Reference.ObjNum);
			PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(xObjectDictionary[key]);
			if (streamFromRefernceHolder == null || !streamFromRefernceHolder.ContainsKey("Subtype") || !((streamFromRefernceHolder["Subtype"] as PdfName).Value == "Form"))
			{
				continue;
			}
			if (streamFromRefernceHolder.Data.Length == 0)
			{
				streamFromRefernceHolder.Data = new byte[1] { 32 };
				streamFromRefernceHolder.Modify();
			}
			else
			{
				ParseFormStream(streamFromRefernceHolder, key.Value, GetDictionaryFromRefernceHolder(streamFromRefernceHolder["Resources"]), enableRecrusiveCall: false);
			}
			if (!streamFromRefernceHolder.ContainsKey("Resources"))
			{
				continue;
			}
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(streamFromRefernceHolder["Resources"]);
			if (dictionaryFromRefernceHolder == null || dictionaryFromRefernceHolder.Items.Count <= 0 || !dictionaryFromRefernceHolder.ContainsKey("XObject"))
			{
				continue;
			}
			PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["XObject"]);
			bool flag = false;
			if (dictionaryFromRefernceHolder2.Count == xObjectDictionary.Count)
			{
				flag = true;
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionaryFromRefernceHolder2.Items)
				{
					if (xObjectDictionary.Items.TryGetValue(item.Key, out IPdfPrimitive value))
					{
						if ((value as PdfReferenceHolder).Reference != (item.Value as PdfReferenceHolder).Reference)
						{
							flag = false;
							break;
						}
						continue;
					}
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				ParseXObjectDictionary(GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["XObject"]));
			}
			else
			{
				dictionaryFromRefernceHolder.Remove("XObject");
			}
		}
	}

	private bool RemoveEmbeddedFilesReference(PdfDictionary elementDictionary)
	{
		bool result = false;
		if (elementDictionary != null && elementDictionary.ContainsKey("EmbeddedFiles"))
		{
			elementDictionary.Remove("EmbeddedFiles");
			elementDictionary.Modify();
			result = true;
		}
		if (elementDictionary != null && elementDictionary.ContainsKey("JavaScript"))
		{
			elementDictionary.Remove("JavaScript");
			elementDictionary.Modify();
			result = true;
		}
		return result;
	}

	private void AttachmentsConsideration(PdfLoadedDocument document)
	{
		if (document.Catalog.ContainsKey("Names"))
		{
			if (document.Catalog["Names"] is PdfReferenceHolder)
			{
				PdfReferenceHolder primitive = document.Catalog["Names"] as PdfReferenceHolder;
				PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(primitive);
				if (RemoveEmbeddedFilesReference(dictionaryFromRefernceHolder))
				{
					document.Catalog.Remove("Names");
					document.Catalog.Modify();
				}
				else if (dictionaryFromRefernceHolder != null && dictionaryFromRefernceHolder.ContainsKey("AP"))
				{
					PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["AP"]);
					if (dictionaryFromRefernceHolder2 != null && dictionaryFromRefernceHolder2.ContainsKey("Names"))
					{
						IPdfPrimitive pdfPrimitive = dictionaryFromRefernceHolder2["Names"];
						if (pdfPrimitive is PdfArray)
						{
							foreach (IPdfPrimitive element in (pdfPrimitive as PdfArray).Elements)
							{
								if (element is PdfReferenceHolder)
								{
									ParseFormStreamFromXObject(GetDictionaryFromRefernceHolder(element), "XObject");
								}
							}
						}
					}
				}
			}
			else if (document.Catalog["Names"] is PdfDictionary)
			{
				PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(document.Catalog["Names"]);
				if (dictionaryFromRefernceHolder3.ContainsKey("EmbeddedFiles"))
				{
					dictionaryFromRefernceHolder3.Remove("EmbeddedFiles");
				}
				if (RemoveEmbeddedFilesReference(dictionaryFromRefernceHolder3))
				{
					document.Catalog.Remove("Names");
					document.Catalog.Modify();
				}
			}
		}
		PdfAttachmentCollection attachments = document.Attachments;
		if (attachments == null || attachments.Count == 0)
		{
			return;
		}
		for (int i = 0; i < attachments.Count; i++)
		{
			PdfAttachment pdfAttachment = attachments[i];
			if (pdfAttachment.EmbeddedFile != null)
			{
				document.Attachments.Remove(pdfAttachment);
			}
		}
	}

	private bool IsValidAction(string action)
	{
		switch (action)
		{
		default:
			return !(action == "JavaScript");
		case "ResetForm":
		case "SubmitForm":
		case "Sound":
		case "Movie":
		case "Launch":
			return false;
		}
	}

	private void AnnotationConsideration(PdfPageBase lPage, PdfLoadedDocument document)
	{
		PdfLoadedPage pdfLoadedPage = lPage as PdfLoadedPage;
		if (!isPdfPage && pdfLoadedPage != null && pdfLoadedPage.Annotations.Count > 0)
		{
			List<PdfLoadedAnnotation> list = new List<PdfLoadedAnnotation>();
			foreach (PdfAnnotation annotation in pdfLoadedPage.Annotations)
			{
				if (annotation is PdfLoadedAnnotation)
				{
					list.Add(annotation as PdfLoadedAnnotation);
				}
			}
			foreach (PdfLoadedAnnotation item in list)
			{
				if (item is PdfLoadedSoundAnnotation || item is PdfLoadedAttachmentAnnotation)
				{
					pdfLoadedPage.Annotations.Remove(item);
					continue;
				}
				PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(item.Dictionary);
				if (dictionaryFromRefernceHolder == null)
				{
					continue;
				}
				if (dictionaryFromRefernceHolder.ContainsKey(new PdfName("RichMediaContent")) || dictionaryFromRefernceHolder.ContainsKey(new PdfName("RichMediaSettings")))
				{
					pdfLoadedPage.Annotations.Remove(item);
					continue;
				}
				if (item is PdfLoadedWidgetAnnotation)
				{
					if (!dictionaryFromRefernceHolder.ContainsKey("AP"))
					{
						if (dictionaryFromRefernceHolder.ContainsKey("FT"))
						{
							PdfName pdfName = dictionaryFromRefernceHolder["FT"] as PdfName;
							if (pdfName != null && pdfName.Value == "Btn")
							{
								dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
							}
							else
							{
								dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
							}
						}
						else
						{
							dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
						}
					}
				}
				else if (!dictionaryFromRefernceHolder.ContainsKey("AP"))
				{
					item.SetAppearance(appearance: true);
				}
				RemoveInvalidAnnotations(dictionaryFromRefernceHolder, lPage, document);
			}
		}
		if (!isPdfPage && pdfLoadedPage.AnnotsReference != null && pdfLoadedPage.AnnotsReference.Count > 0)
		{
			foreach (PdfReference item2 in pdfLoadedPage.AnnotsReference)
			{
				IPdfPrimitive @object = document.CrossTable.GetObject(item2);
				PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(@object);
				if (dictionaryFromRefernceHolder2 != null)
				{
					if (dictionaryFromRefernceHolder2.ContainsKey(new PdfName("RichMediaContent")) || dictionaryFromRefernceHolder2.ContainsKey(new PdfName("RichMediaSettings")) || (dictionaryFromRefernceHolder2["Subtype"] as PdfName).Value == "3D")
					{
						if (lPage.Dictionary.ContainsKey("Annots"))
						{
							PdfArray arrayFromReferenceHolder = GetArrayFromReferenceHolder(lPage.Dictionary["Annots"]);
							foreach (PdfReferenceHolder item3 in arrayFromReferenceHolder)
							{
								if (item3 != null && item3.Reference.Equals(item2))
								{
									arrayFromReferenceHolder.Remove(item3);
									arrayFromReferenceHolder.MarkChanged();
									break;
								}
							}
						}
					}
					else
					{
						if (dictionaryFromRefernceHolder2.ContainsKey("Subtype") && (dictionaryFromRefernceHolder2["Subtype"] as PdfName).Value == "Widget" && !dictionaryFromRefernceHolder2.ContainsKey("AP"))
						{
							if (dictionaryFromRefernceHolder2.ContainsKey("FT"))
							{
								PdfName pdfName2 = dictionaryFromRefernceHolder2["FT"] as PdfName;
								if (pdfName2 != null && pdfName2.Value == "Btn")
								{
									dictionaryFromRefernceHolder2["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
								}
								else
								{
									dictionaryFromRefernceHolder2["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
								}
							}
							else
							{
								dictionaryFromRefernceHolder2["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
							}
						}
						RemoveInvalidAnnotations(dictionaryFromRefernceHolder2, lPage, document);
					}
				}
			}
			return;
		}
		if (pdfLoadedPage.Dictionary == null)
		{
			return;
		}
		PdfDictionary dictionary = pdfLoadedPage.Dictionary;
		if (dictionary == null || !dictionary.ContainsKey("Annots"))
		{
			return;
		}
		PdfArray pdfArray = null;
		if (dictionary["Annots"] is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder2 = dictionary["Annots"] as PdfReferenceHolder;
			if (pdfReferenceHolder2.Object is PdfArray)
			{
				pdfArray = pdfReferenceHolder2.Object as PdfArray;
			}
		}
		else
		{
			pdfArray = dictionary["Annots"] as PdfArray;
		}
		if (pdfArray == null || pdfArray.Count <= 0)
		{
			return;
		}
		for (int num = pdfArray.Count - 1; num >= 0; num--)
		{
			PdfReferenceHolder pdfReferenceHolder3 = pdfArray[num] as PdfReferenceHolder;
			if (pdfReferenceHolder3.Object is PdfDictionary pdfDictionary)
			{
				if ((pdfDictionary["Subtype"] as PdfName).Value == "3D")
				{
					pdfArray.Remove(pdfReferenceHolder3);
					pdfArray.MarkChanged();
				}
				else if (pdfDictionary.ContainsKey("Subtype") && (pdfDictionary["Subtype"] as PdfName).Value == "Widget" && !pdfDictionary.ContainsKey("AP"))
				{
					if (pdfDictionary.ContainsKey("FT"))
					{
						PdfName pdfName3 = pdfDictionary["FT"] as PdfName;
						if (pdfName3 != null && pdfName3.Value == "Btn")
						{
							pdfDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
						}
						else
						{
							pdfDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
						}
					}
					else
					{
						pdfDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
					}
				}
				RemoveInvalidAnnotations(pdfDictionary, lPage, document);
			}
		}
	}

	internal bool IsDiffFontEncoding(PdfDictionary dictionary)
	{
		bool result = false;
		bool flag = false;
		bool flag2 = false;
		if (dictionary.ContainsKey("AP"))
		{
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(dictionary["AP"]);
			if (dictionaryFromRefernceHolder.ContainsKey("N") && !(dictionaryFromRefernceHolder["N"] is PdfStream))
			{
				PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["N"]);
				if (dictionaryFromRefernceHolder2 is PdfStream && dictionaryFromRefernceHolder2 is PdfStream pdfStream && pdfStream.ContainsKey("Resources"))
				{
					dictionary = GetDictionaryFromRefernceHolder(pdfStream["Resources"]);
					if (dictionary != null && dictionary.ContainsKey("Font"))
					{
						dictionary = GetDictionaryFromRefernceHolder(dictionary["Font"]);
						if (dictionary != null)
						{
							dictionary = GetDictionaryFromRefernceHolder(dictionary);
							foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
							{
								if (!(item.Value is PdfReferenceHolder))
								{
									continue;
								}
								dictionary = (item.Value as PdfReferenceHolder).Object as PdfDictionary;
								if (dictionary.ContainsKey("Encoding"))
								{
									PdfDictionary pdfDictionary = PdfCrossTable.Dereference(dictionary["Encoding"]) as PdfDictionary;
									if (pdfDictionary != null && pdfDictionary.ContainsKey("Differences") && pdfDictionary.ContainsKey("Differences"))
									{
										flag = true;
									}
									if (pdfDictionary != null && pdfDictionary.ContainsKey("BaseEncoding") && (PdfCrossTable.Dereference(pdfDictionary["BaseEncoding"]) as PdfName).Value == "MacRomanEnconding")
									{
										flag2 = true;
									}
								}
							}
						}
					}
				}
			}
		}
		if (flag || flag2)
		{
			result = true;
		}
		return result;
	}

	private void RemoveInvalidAnnotations(PdfDictionary annotationDictionary, PdfPageBase lPage, PdfLoadedDocument document)
	{
		bool flag = false;
		if (annotationDictionary.ContainsKey("Subtype") && (annotationDictionary["Subtype"] as PdfName).Value == "Widget")
		{
			flag = true;
		}
		if (annotationDictionary.ContainsKey("CA"))
		{
			annotationDictionary.Remove("CA");
			annotationDictionary.Modify();
		}
		PdfName pdfName = annotationDictionary["FT"] as PdfName;
		_ = annotationDictionary["DV"];
		PdfNumber pdfNumber = annotationDictionary["F"] as PdfNumber;
		if ((annotationDictionary.ContainsKey("AP") && pdfName != null && pdfName.Value != "Tx") || !annotationDictionary.ContainsKey("FT") || (annotationDictionary.ContainsKey("V") && !annotationDictionary.ContainsKey("DV")))
		{
			if (pdfNumber != null && pdfNumber.IntValue == 6)
			{
				annotationDictionary["AP"] = CreateNewAppearanceDictionary(isScript: true, isButtonField: false);
			}
			else
			{
				PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(annotationDictionary["AP"]);
				if (dictionaryFromRefernceHolder != null)
				{
					if (dictionaryFromRefernceHolder.ContainsKey("N"))
					{
						if (!(dictionaryFromRefernceHolder["N"] is PdfStream))
						{
							PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["N"]);
							if (dictionaryFromRefernceHolder2 is PdfStream)
							{
								ParseAppearanceStreamInAnnotations(dictionaryFromRefernceHolder2 as PdfStream, "N", flag);
							}
							else if (dictionaryFromRefernceHolder2.Keys.Count > 0)
							{
								foreach (PdfName item in new List<PdfName>(dictionaryFromRefernceHolder2.Keys))
								{
									ParseAppearanceStreamInAnnotations(GetStreamFromRefernceHolder(dictionaryFromRefernceHolder2[item]), item.Value, flag);
								}
							}
						}
						else
						{
							PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder["N"]);
							ParseAppearanceStreamInAnnotations(streamFromRefernceHolder, "N", flag);
						}
					}
					if (dictionaryFromRefernceHolder.ContainsKey("D"))
					{
						if (dictionaryFromRefernceHolder.ContainsKey("N"))
						{
							dictionaryFromRefernceHolder.Remove("D");
							dictionaryFromRefernceHolder.Modify();
						}
						else if (!(dictionaryFromRefernceHolder["D"] is PdfStream))
						{
							PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["D"]);
							if (dictionaryFromRefernceHolder3 is PdfStream)
							{
								ParseAppearanceStreamInAnnotations(dictionaryFromRefernceHolder3 as PdfStream, "D", flag);
							}
							else if (dictionaryFromRefernceHolder3.Keys.Count > 0)
							{
								foreach (PdfName item2 in new List<PdfName>(dictionaryFromRefernceHolder3.Keys))
								{
									ParseAppearanceStreamInAnnotations(GetStreamFromRefernceHolder(dictionaryFromRefernceHolder3[item2]), item2.Value, flag);
								}
							}
						}
						else
						{
							PdfStream streamFromRefernceHolder2 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder["D"]);
							ParseAppearanceStreamInAnnotations(streamFromRefernceHolder2, "D", flag);
						}
					}
				}
			}
		}
		else
		{
			annotationDictionary["AP"] = CreateNewAppearanceDictionary(isScript: false, isButtonField: false);
		}
		if (annotationDictionary.ContainsKey("AA") && flag)
		{
			annotationDictionary.Remove("AA");
		}
		annotationDictionary["F"] = new PdfNumber(4);
		annotationDictionary.Modify();
	}

	private void ParseAppearanceStreamInAnnotations(PdfStream appearanceStream, string appearanceKey, bool isWidget)
	{
		if (appearanceStream == null)
		{
			return;
		}
		if (appearanceStream.Data.Length == 0)
		{
			appearanceStream.Data = new byte[1] { 32 };
			appearanceStream.Modify();
		}
		if (!isWidget)
		{
			ParseFormStream(appearanceStream, appearanceKey, GetDictionaryFromRefernceHolder(appearanceStream["Resources"]), enableRecrusiveCall: true);
		}
		if (!appearanceStream.ContainsKey("Resources"))
		{
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(appearanceStream["Resources"]);
		if (dictionaryFromRefernceHolder != null && dictionaryFromRefernceHolder.ContainsKey("ExtGState") && PdfALevel == PdfConformanceLevel.Pdf_A1B)
		{
			PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["ExtGState"]);
			if (dictionaryFromRefernceHolder2 != null && dictionaryFromRefernceHolder2.Keys.Count > 0)
			{
				foreach (PdfName item in new List<PdfName>(dictionaryFromRefernceHolder2.Keys))
				{
					PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2[item]);
					if (dictionaryFromRefernceHolder3 == null)
					{
						continue;
					}
					if (PdfALevel == PdfConformanceLevel.Pdf_A1B)
					{
						if (dictionaryFromRefernceHolder3.ContainsKey("CA"))
						{
							dictionaryFromRefernceHolder3["CA"] = new PdfNumber(1);
							dictionaryFromRefernceHolder3.Modify();
						}
						if (dictionaryFromRefernceHolder3.ContainsKey("ca"))
						{
							dictionaryFromRefernceHolder3["ca"] = new PdfNumber(1);
							dictionaryFromRefernceHolder3.Modify();
						}
					}
					if (dictionaryFromRefernceHolder3.ContainsKey("BM") && dictionaryFromRefernceHolder3["BM"] is PdfName && ((dictionaryFromRefernceHolder3["BM"] as PdfName).Value != "Normal" || (dictionaryFromRefernceHolder3["BM"] as PdfName).Value != "Compatible"))
					{
						dictionaryFromRefernceHolder3["BM"] = new PdfName("Normal");
						dictionaryFromRefernceHolder3.Modify();
					}
				}
			}
		}
		if (dictionaryFromRefernceHolder != null && dictionaryFromRefernceHolder.ContainsKey("XObject"))
		{
			PdfDictionary dictionaryFromRefernceHolder4 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["XObject"]);
			if (dictionaryFromRefernceHolder4 != null && dictionaryFromRefernceHolder4.Keys.Count > 0)
			{
				foreach (PdfName item2 in new List<PdfName>(dictionaryFromRefernceHolder4.Keys))
				{
					PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder4[item2]);
					if (PdfALevel == PdfConformanceLevel.Pdf_A1B && streamFromRefernceHolder != null)
					{
						RemoveSMaskFromPattern(streamFromRefernceHolder);
					}
					if (streamFromRefernceHolder != null && streamFromRefernceHolder.ContainsKey("Group"))
					{
						PdfDictionary dictionaryFromRefernceHolder5 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder["Group"]);
						if (dictionaryFromRefernceHolder5 != null && dictionaryFromRefernceHolder5.ContainsKey("S") && dictionaryFromRefernceHolder5["S"] is PdfName && (dictionaryFromRefernceHolder5["S"] as PdfName).Value == "Transparency" && PdfALevel == PdfConformanceLevel.Pdf_A1B)
						{
							streamFromRefernceHolder.Remove("Group");
							streamFromRefernceHolder.Modify();
						}
					}
				}
			}
		}
		if (dictionaryFromRefernceHolder == null || !dictionaryFromRefernceHolder.ContainsKey("Font"))
		{
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder6 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["Font"]);
		string text = "";
		if (dictionaryFromRefernceHolder6 == null || IsFontHasDifferenceDictiory(dictionaryFromRefernceHolder6))
		{
			return;
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item3 in dictionaryFromRefernceHolder6.Items)
		{
			text = item3.Key.Value;
		}
		if (text != "ZaDb")
		{
			EmbedCompleteFonts(null, dictionaryFromRefernceHolder6, isForm: true);
		}
	}

	private void RemoveLZWCompression(PdfArray lzwCompressionFilterArray, PdfStream lzwStream)
	{
		foreach (IPdfPrimitive element in lzwCompressionFilterArray.Elements)
		{
			if (element is PdfName && ((element as PdfName).Value == "LZWDecode" || (element as PdfName).Value == "JPXDecode"))
			{
				RemoveLZWCompression(lzwStream, (element as PdfName).Value);
			}
		}
	}

	private void RemoveLZWCompression(PdfStream lzwStream, string FilterType)
	{
		byte[] data = ((!(FilterType == "LZWDecode") && !(FilterType == "LZW")) ? new DefaultCompressor().Decompress(lzwStream.Data) : new PdfLzwCompressor().Decompress(lzwStream.Data));
		PdfZlibCompressor pdfZlibCompressor = new PdfZlibCompressor();
		lzwStream.Data = pdfZlibCompressor.Compress(data);
		lzwStream["Filter"] = new PdfName("FlateDecode");
		lzwStream["Length"] = new PdfNumber(lzwStream.Data.Length);
		lzwStream.Modify();
	}

	private void CompressionConsideration(PdfPageBase lPage)
	{
		if (!lPage.Dictionary.ContainsKey("Thumb"))
		{
			return;
		}
		PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(lPage.Dictionary["Thumb"]);
		if (streamFromRefernceHolder == null)
		{
			return;
		}
		if (streamFromRefernceHolder.ContainsKey("Filter"))
		{
			if (streamFromRefernceHolder["Filter"] is PdfName && (streamFromRefernceHolder["Filter"] as PdfName).Value == "LZWDecode")
			{
				RemoveLZWCompression(streamFromRefernceHolder, "LZWDecode");
			}
			else if (streamFromRefernceHolder["Filter"] is PdfArray)
			{
				RemoveLZWCompression(streamFromRefernceHolder["Filter"] as PdfArray, streamFromRefernceHolder);
			}
		}
		if (!streamFromRefernceHolder.ContainsKey("ColorSpace") || (!(streamFromRefernceHolder["ColorSpace"] is PdfReferenceHolder) && GetArrayFromReferenceHolder(streamFromRefernceHolder["ColorSpace"]) == null))
		{
			return;
		}
		PdfArray arrayFromReferenceHolder = GetArrayFromReferenceHolder(streamFromRefernceHolder["ColorSpace"]);
		if (arrayFromReferenceHolder == null || arrayFromReferenceHolder.Elements.Count <= 0 || !(arrayFromReferenceHolder[arrayFromReferenceHolder.Elements.Count - 1] is PdfReferenceHolder))
		{
			return;
		}
		PdfStream streamFromRefernceHolder2 = GetStreamFromRefernceHolder(arrayFromReferenceHolder[arrayFromReferenceHolder.Elements.Count - 1]);
		if (streamFromRefernceHolder2 != null && streamFromRefernceHolder2.ContainsKey("Filter"))
		{
			if (streamFromRefernceHolder2["Filter"] is PdfName && (streamFromRefernceHolder2["Filter"] as PdfName).Value == "LZWDecode")
			{
				RemoveLZWCompression(streamFromRefernceHolder2, "LZWDecode");
			}
			else if (streamFromRefernceHolder2["Filter"] is PdfArray)
			{
				RemoveLZWCompression(streamFromRefernceHolder2["Filter"] as PdfArray, streamFromRefernceHolder2);
			}
		}
	}

	private void RemoveIndirectCMYKReference(List<IPdfPrimitive> elements, IPdfPrimitive color, PdfResources resources, PdfPageBase lPage)
	{
		int num = elements.IndexOf(new PdfName("DeviceCMYK"));
		elements.Remove(new PdfName("DeviceCMYK"));
		elements.Insert(num, new PdfName("DeviceRGB"));
		if (color is PdfReferenceHolder)
		{
			((color as PdfReferenceHolder).Object as PdfArray).MarkChanged();
		}
		else
		{
			(color as PdfArray).MarkChanged();
		}
		PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(elements[num + 1]);
		if (dictionaryFromRefernceHolder.ContainsKey("C0") && dictionaryFromRefernceHolder["C0"] != null)
		{
			List<IPdfPrimitive> elements2 = (dictionaryFromRefernceHolder["C0"] as PdfArray).Elements;
			float floatValue = (elements2[0] as PdfNumber).FloatValue;
			float floatValue2 = (elements2[1] as PdfNumber).FloatValue;
			float floatValue3 = (elements2[2] as PdfNumber).FloatValue;
			float floatValue4 = (elements2[3] as PdfNumber).FloatValue;
			float[] equivalentRGB = GetEquivalentRGB(floatValue, floatValue2, floatValue3, floatValue4);
			dictionaryFromRefernceHolder.Remove("C0");
			dictionaryFromRefernceHolder["C0"] = new PdfArray(equivalentRGB);
			dictionaryFromRefernceHolder.Modify();
		}
		if (dictionaryFromRefernceHolder.ContainsKey("C1") && dictionaryFromRefernceHolder["C1"] != null)
		{
			List<IPdfPrimitive> elements3 = (dictionaryFromRefernceHolder["C1"] as PdfArray).Elements;
			float floatValue5 = (elements3[0] as PdfNumber).FloatValue;
			float floatValue6 = (elements3[1] as PdfNumber).FloatValue;
			float floatValue7 = (elements3[2] as PdfNumber).FloatValue;
			float floatValue8 = (elements3[3] as PdfNumber).FloatValue;
			float[] equivalentRGB2 = GetEquivalentRGB(floatValue5, floatValue6, floatValue7, floatValue8);
			dictionaryFromRefernceHolder.Remove("C1");
			dictionaryFromRefernceHolder["C1"] = new PdfArray(equivalentRGB2);
			dictionaryFromRefernceHolder.Modify();
		}
		if (dictionaryFromRefernceHolder.ContainsKey("Range") && dictionaryFromRefernceHolder["Range"] != null)
		{
			List<IPdfPrimitive> elements4 = (dictionaryFromRefernceHolder["Range"] as PdfArray).Elements;
			float[] array = new float[6]
			{
				(elements4[0] as PdfNumber).FloatValue,
				(elements4[1] as PdfNumber).FloatValue,
				(elements4[2] as PdfNumber).FloatValue,
				(elements4[3] as PdfNumber).FloatValue,
				(elements4[4] as PdfNumber).FloatValue,
				(elements4[5] as PdfNumber).FloatValue
			};
			dictionaryFromRefernceHolder.Remove("Range");
			dictionaryFromRefernceHolder["Range"] = new PdfArray(array);
			dictionaryFromRefernceHolder.Modify();
		}
		if (dictionaryFromRefernceHolder.ContainsKey("Decode") && dictionaryFromRefernceHolder["Decode"] != null)
		{
			List<IPdfPrimitive> elements5 = (dictionaryFromRefernceHolder["Decode"] as PdfArray).Elements;
			float[] array2 = new float[6]
			{
				(elements5[0] as PdfNumber).FloatValue,
				(elements5[1] as PdfNumber).FloatValue,
				(elements5[2] as PdfNumber).FloatValue,
				(elements5[3] as PdfNumber).FloatValue,
				(elements5[4] as PdfNumber).FloatValue,
				(elements5[5] as PdfNumber).FloatValue
			};
			dictionaryFromRefernceHolder.Remove("Decode");
			dictionaryFromRefernceHolder["Decode"] = new PdfArray(array2);
			dictionaryFromRefernceHolder.Modify();
		}
	}

	private void RemoveTransparencyGroup(PdfPageBase lPage)
	{
		if (lPage.Dictionary.ContainsKey("Group"))
		{
			lPage.Dictionary.Remove("Group");
			lPage.Dictionary.Modify();
		}
	}

	private void RemoveSMaskFromPdfDictionary(PdfPageBase lPage)
	{
		PdfImageInfo[] imagesInfo = lPage.GetImagesInfo();
		for (int i = 0; i < imagesInfo.Length; i++)
		{
			if (imagesInfo[i].Image != null)
			{
				Bitmap bitmap = new Bitmap(imagesInfo[i].Image.m_sKBitmap);
				Bitmap bitmap2 = ReplaceTransparency(bitmap, Color.White);
				MemoryStream stream = new MemoryStream();
				bitmap2.Save(stream, ImageFormat.Jpeg);
				PdfBitmap image = new PdfBitmap(stream);
				lPage.ReplaceImage(i, image);
			}
		}
	}

	private void RemoveSMask(PdfReferenceHolder imageObject, PdfResources resources, string key)
	{
		while (imageObject != null && imageObject.Object is PdfReferenceHolder)
		{
			imageObject = imageObject.Object as PdfReferenceHolder;
		}
		if (!(imageObject.Object is PdfStream))
		{
			return;
		}
		PdfStream pdfStream = imageObject.Object as PdfStream;
		if ((pdfStream["Subtype"] as PdfName).Value == "Image")
		{
			if (PdfALevel == PdfConformanceLevel.Pdf_A1B)
			{
				if (pdfStream.ContainsKey("SMask"))
				{
					for (int i = 0; i < ldoc.Pages.Count; i++)
					{
						PdfPageBase lPage = ldoc.Pages[i];
						RemoveSMaskFromPdfDictionary(lPage);
					}
					if (pdfStream.ContainsKey("Filter") && pdfStream["Filter"] is PdfName && (pdfStream["Filter"] as PdfName).Value == "FlateDecode")
					{
						pdfStream["Filter"] = new PdfName("DCTDecode");
					}
				}
				if (pdfStream.ContainsKey("Filter"))
				{
					if (pdfStream["Filter"] is PdfName && ((pdfStream["Filter"] as PdfName).Value == "JPXDecode" || (pdfStream["Filter"] as PdfName).Value == "LZWDecode"))
					{
						RemoveLZWCompression(pdfStream, (pdfStream["Filter"] as PdfName).Value);
					}
					else if (pdfStream["Filter"] is PdfArray)
					{
						RemoveLZWCompression(pdfStream["Filter"] as PdfArray, pdfStream);
					}
				}
				else
				{
					pdfStream.Compress = true;
				}
			}
			if (pdfStream.ContainsKey("ColorSpace"))
			{
				if (!(pdfStream["ColorSpace"] is PdfName))
				{
					constainsColorspace = true;
				}
				if (!resources.ContainsKey("ColorSpace"))
				{
					PdfReferenceHolder value = new PdfReferenceHolder(PdfCmykColorSpace);
					PdfDictionary pdfDictionary = new PdfDictionary();
					pdfDictionary[new PdfName("DefaultCMYK")] = value;
					resources[new PdfName("ColorSpace")] = pdfDictionary;
				}
			}
			if ((PdfALevel == PdfConformanceLevel.Pdf_A2B || PdfALevel == PdfConformanceLevel.Pdf_A3B || PdfALevel == PdfConformanceLevel.Pdf_A2U || PdfALevel == PdfConformanceLevel.Pdf_A3U) && pdfStream.ContainsKey("SMask"))
			{
				PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(pdfStream["SMask"]);
				if (dictionaryFromRefernceHolder != null && dictionaryFromRefernceHolder.ContainsKey("Interpolate"))
				{
					IPdfPrimitive primitive = dictionaryFromRefernceHolder["Interpolate"];
					GetPdfBooleanFromReferenceHolder(primitive).Value = false;
					dictionaryFromRefernceHolder.Modify();
				}
			}
			if (pdfStream.ContainsKey("Interpolate"))
			{
				IPdfPrimitive primitive2 = pdfStream["Interpolate"];
				GetPdfBooleanFromReferenceHolder(primitive2).Value = false;
				pdfStream.Modify();
			}
			if (pdfStream.ContainsKey("Mask"))
			{
				PdfReferenceHolder pdfReferenceHolder = pdfStream["Mask"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Interpolate"))
				{
					IPdfPrimitive primitive3 = pdfDictionary2["Interpolate"];
					GetPdfBooleanFromReferenceHolder(primitive3).Value = false;
					pdfDictionary2.Modify();
					pdfStream.Modify();
				}
			}
		}
		else
		{
			if (!((pdfStream["Subtype"] as PdfName).Value == "Form"))
			{
				return;
			}
			if (pdfStream.ContainsKey("Group"))
			{
				pdfStream.Remove("Group");
				pdfStream.Modify();
			}
			if (!pdfStream.ContainsKey("Resources"))
			{
				return;
			}
			if (pdfStream.ContainsKey("OPI"))
			{
				pdfStream.Remove("OPI");
				pdfStream.Modify();
			}
			PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(pdfStream["Resources"]);
			if (dictionaryFromRefernceHolder2.ContainsKey("Reference"))
			{
				dictionaryFromRefernceHolder2.Remove("Reference");
				dictionaryFromRefernceHolder2.Modify();
			}
			if (dictionaryFromRefernceHolder2.ContainsKey("Ref"))
			{
				dictionaryFromRefernceHolder2.Remove("Ref");
				dictionaryFromRefernceHolder2.Modify();
			}
			if (dictionaryFromRefernceHolder2.ContainsKey("Font"))
			{
				PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2["Font"]);
				foreach (PdfName item in new List<PdfName>(dictionaryFromRefernceHolder3.Keys))
				{
					if (!resources.ContainsKey("Font"))
					{
						continue;
					}
					PdfDictionary dictionaryFromRefernceHolder4 = GetDictionaryFromRefernceHolder(resources["Font"]);
					if (dictionaryFromRefernceHolder4.ContainsKey(item))
					{
						PdfReferenceHolder pdfReferenceHolder2 = dictionaryFromRefernceHolder3[item.Value] as PdfReferenceHolder;
						PdfReferenceHolder pdfReferenceHolder3 = dictionaryFromRefernceHolder4[item.Value] as PdfReferenceHolder;
						if (pdfReferenceHolder2 != null && pdfReferenceHolder3 != null && pdfReferenceHolder2.Reference != null && pdfReferenceHolder3.Reference != null && pdfReferenceHolder2.Reference.ObjNum == pdfReferenceHolder3.Reference.ObjNum)
						{
							dictionaryFromRefernceHolder3.Remove(item);
							dictionaryFromRefernceHolder3.Items.Add(item, dictionaryFromRefernceHolder4[item]);
							dictionaryFromRefernceHolder3.Modify();
						}
					}
				}
				EmbedCompleteFonts(null, dictionaryFromRefernceHolder3, isForm: true);
			}
			if (dictionaryFromRefernceHolder2.ContainsKey("XObject"))
			{
				PdfDictionary dictionaryFromRefernceHolder5 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2["XObject"]);
				foreach (PdfName item2 in new List<PdfName>(dictionaryFromRefernceHolder5.Keys))
				{
					if (dictionaryFromRefernceHolder5[item2] is PdfReferenceHolder)
					{
						RemoveSMask(dictionaryFromRefernceHolder5[item2] as PdfReferenceHolder, resources, item2.Value);
						if (!dictionaryFromRefernceHolder2.ContainsKey("ColorSpace") && constainsColorspace)
						{
							PdfReferenceHolder value2 = new PdfReferenceHolder(PdfCmykColorSpace);
							PdfDictionary pdfDictionary3 = new PdfDictionary();
							pdfDictionary3[new PdfName("DefaultCMYK")] = value2;
							dictionaryFromRefernceHolder2[new PdfName("ColorSpace")] = pdfDictionary3;
						}
					}
				}
			}
			if (dictionaryFromRefernceHolder2.ContainsKey(new PdfName("ColorSpace")))
			{
				PdfDictionary dictionaryFromRefernceHolder6 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2["ColorSpace"]);
				if (dictionaryFromRefernceHolder6 != null)
				{
					PdfReferenceHolder value3 = new PdfReferenceHolder(PdfCmykColorSpace);
					dictionaryFromRefernceHolder6[new PdfName("DefaultCMYK")] = value3;
				}
			}
			if (resources.ContainsKey(new PdfName("ColorSpace")))
			{
				PdfDictionary dictionaryFromRefernceHolder7 = GetDictionaryFromRefernceHolder(resources["ColorSpace"]);
				if (dictionaryFromRefernceHolder7 != null)
				{
					PdfReferenceHolder value4 = new PdfReferenceHolder(PdfCmykColorSpace);
					dictionaryFromRefernceHolder7[new PdfName("DefaultCMYK")] = value4;
				}
			}
			else
			{
				PdfReferenceHolder value5 = new PdfReferenceHolder(PdfCmykColorSpace);
				PdfDictionary pdfDictionary4 = new PdfDictionary();
				pdfDictionary4[new PdfName("DefaultCMYK")] = value5;
				resources[new PdfName("ColorSpace")] = pdfDictionary4;
			}
		}
	}

	public Bitmap ReplaceTransparency(Bitmap bitmap, Color background)
	{
		Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
		GraphicsHelper graphicsHelper = new GraphicsHelper(bitmap2);
		graphicsHelper.Clear(background);
		graphicsHelper.CompositingMode = CompositingMode.SourceCopy;
		graphicsHelper.InterpolationMode = InterpolationMode.High;
		graphicsHelper.DrawImage(bitmap, 0, 0);
		return bitmap2;
	}

	public Stream CopyStream(MemoryStream input)
	{
		byte[] array = new byte[32768];
		Stream stream = new MemoryStream(input.GetBuffer());
		int count;
		while ((count = input.Read(array, 0, array.Length)) > 0)
		{
			stream.Write(array, 0, count);
		}
		return stream;
	}

	private float[] ConvertTransparencyToRGB(float r, float g, float b, float opacity)
	{
		if (opacity != -1f)
		{
			r *= opacity;
			g *= opacity;
			b *= opacity;
		}
		return new float[3]
		{
			r / 255f,
			g / 255f,
			b / 255f
		};
	}

	private Colorspace GetColorspace(string[] colorSpaceElement, PdfResources resources)
	{
		Colorspace colorspace = null;
		if (Colorspace.IsColorSpace(colorSpaceElement[0].Replace("/", "")))
		{
			colorspace = Colorspace.CreateColorSpace(colorSpaceElement[0].Replace("/", ""));
			if (colorSpaceElement[0].Replace("/", "") == "Pattern")
			{
				PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(resources[colorSpaceElement[0].Replace("/", "")]);
				if (dictionaryFromRefernceHolder != null && dictionaryFromRefernceHolder.Values.Count > 0)
				{
					foreach (IPdfPrimitive value in dictionaryFromRefernceHolder.Values)
					{
						RemoveSMaskFromPattern(GetDictionaryFromRefernceHolder(value));
					}
				}
			}
		}
		else if (this.colorspace is Separation && !resources.ContainsKey("ColorSpace"))
		{
			colorspace = new Separation();
		}
		else if (GetDictionaryFromRefernceHolder(resources["ColorSpace"]).ContainsKey(colorSpaceElement[0].Replace("/", "")))
		{
			IPdfPrimitive pdfPrimitive = GetDictionaryFromRefernceHolder(resources["ColorSpace"])[colorSpaceElement[0].Replace("/", "")];
			if (pdfPrimitive is PdfReferenceHolder)
			{
				PdfArray pdfArray = (pdfPrimitive as PdfReferenceHolder).Object as PdfArray;
				if (pdfArray.Elements[0] is PdfName)
				{
					if ((pdfArray.Elements[0] as PdfName).Value == "DeviceRGB")
					{
						colorspace = new DeviceRGB();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "DeviceGray")
					{
						colorspace = new DeviceGray();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "DeviceCMYK")
					{
						colorspace = new DeviceCMYK();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "CalGray")
					{
						colorspace = new CalGray();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "CalRGB")
					{
						colorspace = new CalRGB();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "Lab")
					{
						colorspace = new LabColor();
					}
					if ((pdfArray.Elements[0] as PdfName).Value == "ICCBased")
					{
						colorspace = new ICCBased();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "Separation")
					{
						colorspace = new Separation();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "DeviceN")
					{
						colorspace = new DeviceN();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "Pattern")
					{
						colorspace = new Pattern();
					}
					else if ((pdfArray.Elements[0] as PdfName).Value == "Indexed")
					{
						colorspace = new Indexed();
					}
				}
			}
		}
		if (colorspace == null)
		{
			return null;
		}
		return colorspace;
	}

	private void ResetPageResources()
	{
		isPdfPage = false;
		appliedSCColorSpace = "";
		appliedSCNColorSpace = "";
		nonStrokingOpacity = -1f;
		strokingOpacity = -1f;
		hasNonStroking = false;
		hasStroking = false;
	}

	private float[] GetEquivalentRGB(float cyan, float magenta, float yellow, float black)
	{
		float num = (1f - cyan) * (1f - black);
		float num2 = (1f - magenta) * (1f - black);
		float num3 = (1f - yellow) * (1f - black);
		return new float[3] { num, num2, num3 };
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

	private void ReplaceCMYKColorSpace(PdfPageBase lPage)
	{
		PdfResources resources = lPage.GetResources();
		if (resources != null && resources.ContainsKey("ColorSpace"))
		{
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(resources["ColorSpace"]);
			if (dictionaryFromRefernceHolder != null && dictionaryFromRefernceHolder.Keys.Count > 0)
			{
				foreach (PdfName item in new List<PdfName>(dictionaryFromRefernceHolder.Keys))
				{
					IPdfPrimitive pdfPrimitive = dictionaryFromRefernceHolder[item];
					List<IPdfPrimitive> list = new List<IPdfPrimitive>();
					if (pdfPrimitive is PdfReferenceHolder && (pdfPrimitive as PdfReferenceHolder).Object is PdfArray)
					{
						list = ((pdfPrimitive as PdfReferenceHolder).Object as PdfArray).Elements;
					}
					else if (pdfPrimitive is PdfArray)
					{
						list = (pdfPrimitive as PdfArray).Elements;
					}
					if (list != null && list.Count > 0 && (list[0] as PdfName).Value == "Separation" && list.Contains(new PdfName("DeviceCMYK")))
					{
						PdfReferenceHolder value = new PdfReferenceHolder(new PdfICCColorSpace
						{
							ColorComponents = 4,
							AlternateColorSpace = new PdfDeviceColorSpace(PdfColorSpace.CMYK)
						});
						dictionaryFromRefernceHolder[new PdfName("DefaultCMYK")] = value;
					}
				}
			}
		}
		if (resources.ContainsKey("Pattern"))
		{
			PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(resources["Pattern"]);
			foreach (PdfName key in dictionaryFromRefernceHolder2.Keys)
			{
				PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2[key]);
				if (dictionaryFromRefernceHolder3.ContainsKey("PatternType") && (dictionaryFromRefernceHolder3["PatternType"] as PdfNumber).IntValue == 2)
				{
					OptimizeShading(dictionaryFromRefernceHolder3["Shading"]);
				}
			}
		}
		if (!resources.ContainsKey("Shading"))
		{
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder4 = GetDictionaryFromRefernceHolder(resources["Shading"]);
		foreach (PdfName key2 in dictionaryFromRefernceHolder4.Keys)
		{
			OptimizeShading(dictionaryFromRefernceHolder4[key2]);
		}
	}

	private void OptimizeShading(IPdfPrimitive shade)
	{
		PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(shade);
		if (dictionaryFromRefernceHolder == null)
		{
			return;
		}
		PdfName pdfName = dictionaryFromRefernceHolder["ColorSpace"] as PdfName;
		if (!(pdfName != null) || !(pdfName.Value == "DeviceCMYK") || !ValidateShadingType(dictionaryFromRefernceHolder) || !dictionaryFromRefernceHolder.ContainsKey("Function") || !ValidateFunctionType(dictionaryFromRefernceHolder))
		{
			return;
		}
		(dictionaryFromRefernceHolder["ColorSpace"] as PdfName).Value = "DeviceRGB";
		if (dictionaryFromRefernceHolder.ContainsKey("Background"))
		{
			PdfArray arrayFromReferenceHolder = GetArrayFromReferenceHolder(dictionaryFromRefernceHolder["Background"]);
			float[] rGBFromCMYK = GetRGBFromCMYK(arrayFromReferenceHolder);
			dictionaryFromRefernceHolder["Background"] = new PdfArray(rGBFromCMYK);
		}
		PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder["Function"]);
		if (streamFromRefernceHolder != null && (streamFromRefernceHolder["FunctionType"] as PdfNumber).IntValue == 0)
		{
			ReplaceCMYKFromType0Function(streamFromRefernceHolder);
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["Function"]);
		if (dictionaryFromRefernceHolder2 != null && (dictionaryFromRefernceHolder2["FunctionType"] as PdfNumber).IntValue == 3)
		{
			ReplaceCMYKFromType3Function(GetArrayFromReferenceHolder(dictionaryFromRefernceHolder2["Functions"]));
		}
		else if (dictionaryFromRefernceHolder2 != null && (dictionaryFromRefernceHolder2["FunctionType"] as PdfNumber).IntValue == 2)
		{
			ReplaceCMYKFromType2Function(dictionaryFromRefernceHolder2);
		}
	}

	private void ReplaceCMYKFromType0Function(PdfStream function)
	{
		function.Decompress();
		if (function.Data.Length % 4 != 0)
		{
			return;
		}
		int[] array = new int[function.Data.Length];
		int num = 0;
		byte[] data = function.Data;
		foreach (byte b in data)
		{
			array[num] = b;
			num++;
			if (num == array.Length)
			{
				break;
			}
		}
		byte[] array2 = new byte[array.Length / 4 * 3];
		for (num = 0; num < array.Length / 4; num++)
		{
			array2[num * 3] = (byte)(255 * (1 - array[num * 4] / 255) * (1 - array[num * 4 + 3] / 255));
			array2[num * 3 + 1] = (byte)(255 * (1 - array[num * 4 + 1] / 255) * (1 - array[num * 4 + 3] / 255));
			array2[num * 3 + 2] = (byte)(255 * (1 - array[num * 4 + 2] / 255) * (1 - array[num * 4 + 3] / 255));
		}
		function.Data = array2;
		function.Compress = true;
		function.Modify();
		if (function.ContainsKey("Range"))
		{
			function["Range"] = new PdfArray(new int[6] { 0, 1, 0, 1, 0, 1 });
		}
	}

	private void ReplaceCMYKFromType2Function(PdfDictionary function)
	{
		if (function.ContainsKey("Range"))
		{
			function["Range"] = new PdfArray(new int[6] { 0, 1, 0, 1, 0, 1 });
		}
		if (function.ContainsKey("C0"))
		{
			float[] rGBFromCMYK = GetRGBFromCMYK(GetArrayFromReferenceHolder(function["C0"]));
			function["C0"] = new PdfArray(rGBFromCMYK);
		}
		if (function.ContainsKey("C1"))
		{
			float[] rGBFromCMYK2 = GetRGBFromCMYK(GetArrayFromReferenceHolder(function["C1"]));
			function["C1"] = new PdfArray(rGBFromCMYK2);
		}
	}

	private void ReplaceCMYKFromType3Function(PdfArray functions)
	{
		foreach (IPdfPrimitive function in functions)
		{
			PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(function);
			if (streamFromRefernceHolder != null && (streamFromRefernceHolder["FunctionType"] as PdfNumber).IntValue == 0)
			{
				ReplaceCMYKFromType0Function(streamFromRefernceHolder);
				continue;
			}
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(function);
			if (dictionaryFromRefernceHolder != null && (dictionaryFromRefernceHolder["FunctionType"] as PdfNumber).IntValue == 2)
			{
				ReplaceCMYKFromType2Function(dictionaryFromRefernceHolder);
			}
			else if (dictionaryFromRefernceHolder != null && (dictionaryFromRefernceHolder["FunctionType"] as PdfNumber).IntValue == 3)
			{
				ReplaceCMYKFromType3Function(GetArrayFromReferenceHolder(dictionaryFromRefernceHolder["Functions"]));
			}
		}
	}

	private float[] GetRGBFromCMYK(PdfArray colorArray)
	{
		float[] array = new float[4];
		int num = 0;
		foreach (PdfNumber item in colorArray)
		{
			array[num] = item.FloatValue;
			num++;
			if (num == 4)
			{
				break;
			}
		}
		return new float[3]
		{
			255f * (1f - array[0]) * (1f - array[3]),
			255f * (1f - array[1]) * (1f - array[3]),
			255f * (1f - array[2]) * (1f - array[3])
		};
	}

	private bool ValidateFunctionType(PdfDictionary shadingDictionary)
	{
		PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(shadingDictionary["Function"]);
		if (streamFromRefernceHolder != null && (streamFromRefernceHolder["FunctionType"] as PdfNumber).IntValue == 4)
		{
			return false;
		}
		if (streamFromRefernceHolder == null)
		{
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(shadingDictionary["Function"]);
			if (dictionaryFromRefernceHolder != null && (dictionaryFromRefernceHolder["FunctionType"] as PdfNumber).IntValue == 3)
			{
				PdfStream streamFromRefernceHolder2 = GetStreamFromRefernceHolder(GetArrayFromReferenceHolder(dictionaryFromRefernceHolder["Functions"]).Elements[0]);
				if (streamFromRefernceHolder2 != null && (streamFromRefernceHolder2["FunctionType"] as PdfNumber).IntValue == 4)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool ValidateShadingType(PdfDictionary shadingDictionary)
	{
		if ((shadingDictionary["ShadingType"] as PdfNumber).IntValue != 1 && (shadingDictionary["ShadingType"] as PdfNumber).IntValue != 2)
		{
			return (shadingDictionary["ShadingType"] as PdfNumber).IntValue == 3;
		}
		return true;
	}

	private FontStyle GetDeviceFontStyle(PdfFontStyle style)
	{
		FontStyle fontStyle = FontStyle.Regular;
		if ((style & PdfFontStyle.Bold) > PdfFontStyle.Regular)
		{
			fontStyle |= FontStyle.Bold;
		}
		if ((style & PdfFontStyle.Italic) > PdfFontStyle.Regular)
		{
			fontStyle |= FontStyle.Italic;
		}
		if ((style & PdfFontStyle.Underline) > PdfFontStyle.Regular)
		{
			fontStyle |= FontStyle.Underline;
		}
		if ((style & PdfFontStyle.Strikeout) > PdfFontStyle.Regular)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		return fontStyle;
	}

	private PdfFontStyle GetFontStyle(string fontFamilyString)
	{
		int num = fontFamilyString.IndexOf("-");
		PdfFontStyle result = PdfFontStyle.Regular;
		if (num >= 0)
		{
			switch (fontFamilyString.Substring(num + 1, fontFamilyString.Length - num - 1))
			{
			case "Italic":
			case "Oblique":
				result = PdfFontStyle.Italic;
				break;
			case "Bold":
				result = PdfFontStyle.Bold;
				break;
			case "BoldItalic":
			case "BoldOblique":
				result = PdfFontStyle.Bold | PdfFontStyle.Italic;
				break;
			}
		}
		return result;
	}

	private PdfFontMetrics CreateFont(PdfDictionary fontDictionary, float height, PdfName baseFont)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		if (fontDictionary.ContainsKey("FontDescriptor"))
		{
			PdfDictionary pdfDictionary = (fontDictionary["FontDescriptor"] as PdfReferenceHolder).Object as PdfDictionary;
			pdfFontMetrics.Ascent = (pdfDictionary["Ascent"] as PdfNumber).IntValue;
			pdfFontMetrics.Descent = (pdfDictionary["Descent"] as PdfNumber).IntValue;
			pdfFontMetrics.Size = height;
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
			pdfFontMetrics.PostScriptName = baseFont.Value;
			PdfArray pdfArray = null;
			if (fontDictionary.ContainsKey("Widths"))
			{
				pdfArray = fontDictionary["Widths"] as PdfArray;
				if (pdfArray == null)
				{
					pdfArray = (fontDictionary["Widths"] as PdfReferenceHolder).Object as PdfArray;
				}
				float[] array = new float[pdfArray.Count];
				for (int i = 0; i < pdfArray.Count; i++)
				{
					array[i] = (pdfArray[i] as PdfNumber).FloatValue;
				}
				pdfFontMetrics.WidthTable = new StandardWidthTable(array);
			}
			pdfFontMetrics.Name = baseFont.Value;
		}
		return pdfFontMetrics;
	}

	private PdfFontFamily GetFontFamily(string fontFamilyString)
	{
		int num = fontFamilyString.IndexOf("-");
		PdfFontFamily pdfFontFamily = PdfFontFamily.Helvetica;
		string text = fontFamilyString;
		if (num >= 0)
		{
			text = fontFamilyString.Substring(0, num);
		}
		if (text == "Times")
		{
			return PdfFontFamily.TimesRoman;
		}
		try
		{
			return (PdfFontFamily)Enum.Parse(typeof(PdfFontFamily), text, ignoreCase: true);
		}
		catch (Exception)
		{
			isPdfFontFamily = false;
			return PdfFontFamily.Helvetica;
		}
	}

	internal void EmbedCompleteFonts(PdfPageBase page, PdfLoadedDocument document)
	{
		PdfResources resources = page.GetResources();
		PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(page.GetResources()["Font"]);
		EmbedCompleteFonts(resources, dictionaryFromRefernceHolder, isForm: false);
	}

	internal void EmbedCompleteFonts(PdfResources pageResources, PdfDictionary usedFonts, bool isForm)
	{
		if (usedFonts == null)
		{
			return;
		}
		foreach (PdfName item in new List<PdfName>(usedFonts.Keys))
		{
			RepairFontDictionary(pageResources, usedFonts, isForm, item);
		}
	}

	private void RepairFontDictionary(PdfResources pageResources, PdfDictionary usedFonts, bool isForm, PdfName key)
	{
		PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(usedFonts[key]);
		bool flag = true;
		isPdfFontFamily = true;
		if (pdfFonts != null)
		{
			if (!pdfFonts.ContainsKey(key.Value))
			{
				pdfFonts.Add(key.Value, dictionaryFromRefernceHolder);
			}
		}
		else
		{
			pdfFonts = new Dictionary<string, PdfDictionary>();
			pdfFonts.Add(key.Value, dictionaryFromRefernceHolder);
		}
		if (dictionaryFromRefernceHolder.ContainsKey("Subtype"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["Subtype"]) as PdfName;
			float size = 12f;
			if (pdfName != null && pdfName.Value == "Type1")
			{
				PdfDictionary fontDescriptor = null;
				PdfDictionary descendantFont = null;
				GetFontInternals(dictionaryFromRefernceHolder, out fontDescriptor, out descendantFont);
				if (fontDescriptor == null || (!fontDescriptor.ContainsKey("FontFile") && !fontDescriptor.ContainsKey("FontFile2") && !fontDescriptor.ContainsKey("FontFile3")))
				{
					if (dictionaryFromRefernceHolder.ContainsKey("BaseFont"))
					{
						PdfName pdfName2 = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["BaseFont"]) as PdfName;
						PdfFontStyle pdfFontStyle = GetFontStyle(pdfName2.Value);
						PdfFontFamily fontFamily = GetFontFamily(pdfName2.Value);
						string text = "";
						text = fontFamily switch
						{
							PdfFontFamily.Helvetica => "Helvetica", 
							PdfFontFamily.Courier => "Courier New", 
							PdfFontFamily.TimesRoman => "Times New Roman", 
							PdfFontFamily.Symbol => "Symbol", 
							PdfFontFamily.ZapfDingbats => "ZapfDingbats", 
							_ => "Arial", 
						};
						if (text == "Helvetica" && (pdfName2.Value.Contains("Narrow") || pdfFontStyle == PdfFontStyle.Italic || pdfFontStyle == (PdfFontStyle.Bold | PdfFontStyle.Italic)))
						{
							if (pdfName2.Value.Contains("Narrow"))
							{
								text = "Helvetica Narrow";
								if (pdfName2.Value.Contains("Bold"))
								{
									pdfFontStyle |= PdfFontStyle.Bold;
								}
							}
							else
							{
								text = "Arial";
							}
						}
						if (pdfName2.Value != text && !isPdfFontFamily)
						{
							flag = false;
						}
						if (flag)
						{
							PdfName pdfName3 = null;
							bool flag2 = false;
							bool flag3 = false;
							if (dictionaryFromRefernceHolder.ContainsKey("Encoding"))
							{
								pdfName3 = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["Encoding"]) as PdfName;
							}
							if (pdfName3 != null && pdfName3.Value == "WinAnsiEncoding" && dictionaryFromRefernceHolder.ContainsKey("Widths") && fontDescriptor != null)
							{
								flag2 = true;
							}
							else if (pdfName3 == null && PdfCrossTable.Dereference(dictionaryFromRefernceHolder["Encoding"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Type"))
							{
								PdfName pdfName4 = PdfCrossTable.Dereference(pdfDictionary["Type"]) as PdfName;
								if (pdfName4 != null && pdfName4.Value == "Encoding" && !dictionaryFromRefernceHolder.ContainsKey("Name"))
								{
									flag3 = true;
								}
							}
							int num = int.MinValue;
							if (fontDescriptor != null && fontDescriptor.ContainsKey("Flags") && PdfCrossTable.Dereference(fontDescriptor["Flags"]) is PdfNumber pdfNumber)
							{
								num = pdfNumber.IntValue;
							}
							bool flag4 = false;
							if (pageResources != null && fontkeys.ContainsKey(pageResources))
							{
								flag4 = true;
								string text2 = string.Empty;
								List<string> list = fontkeys[pageResources];
								for (int i = 0; i < list.Count; i++)
								{
									if (list[i] != "TJ")
									{
										text2 = list[i];
									}
									if (list[i] == "TJ" && text2 == key.Value)
									{
										flag4 = false;
										break;
									}
								}
							}
							PdfTrueTypeFont pdfTrueTypeFont = null;
							if ((flag3 || UsedFonts.Contains(key.Value)) && !ReplaceFonts.ContainsKey(key.Value) && num != 4 && num != 32 && flag4 && !flag2 && num != int.MinValue)
							{
								OldFonts.Add(key.Value, dictionaryFromRefernceHolder);
								if (!ldoc.RaisePdfFont)
								{
									throw new PdfException("Unable to embed the fonts, use SubstituteFont event to embed the fonts");
								}
								PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
								pdfFontEventArgs.m_fontName = text;
								pdfFontEventArgs.m_fontStyle = pdfFontStyle;
								ldoc.PdfFontStream(pdfFontEventArgs);
								pdfTrueTypeFont = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, size, string.Empty, useTrueType: false, isEnableEmbedding: true, isConformanceEnabled: true);
								ReplaceFonts.Add(key.Value, pdfTrueTypeFont);
							}
							else
							{
								if (!ldoc.RaisePdfFont)
								{
									throw new PdfException("Unable to embed the fonts, use SubstituteFont event to embed the fonts");
								}
								PdfFontEventArgs pdfFontEventArgs2 = new PdfFontEventArgs();
								pdfFontEventArgs2.m_fontName = text;
								pdfFontEventArgs2.m_fontStyle = pdfFontStyle;
								ldoc.PdfFontStream(pdfFontEventArgs2);
								if (flag4 && dictionaryFromRefernceHolder.ContainsKey("Encoding") && !dictionaryFromRefernceHolder.ContainsKey("FontDescriptor"))
								{
									if (!OldFonts.TryGetValue(key.Value, out var _))
									{
										OldFonts.Add(key.Value, dictionaryFromRefernceHolder);
									}
									if (PdfCrossTable.Dereference(dictionaryFromRefernceHolder["Encoding"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Differences"))
									{
										if (!ReplaceFonts.TryGetValue(key.Value, out var _))
										{
											PdfDocument.FontEmbeddingEnabled = true;
											pdfTrueTypeFont = new PdfTrueTypeFont(pdfFontEventArgs2.FontStream, size, embed: true, subset: true);
											PdfDocument.FontEmbeddingEnabled = false;
										}
										if (ReplaceFonts.ContainsKey(key.Value))
										{
											OldFonts.Clear();
										}
										else
										{
											ReplaceFonts.Add(key.Value, pdfTrueTypeFont);
										}
									}
								}
								if (pdfTrueTypeFont == null)
								{
									if (pdfName3 == null && OldFonts.Count > 0)
									{
										OldFonts.Clear();
									}
									pdfTrueTypeFont = new PdfTrueTypeFont(pdfFontEventArgs2.FontStream, pdfFontEventArgs2.m_fontStyle, size, string.Empty, useTrueType: true, isEnableEmbedding: true, isConformanceEnabled: true);
								}
							}
							usedFonts.Remove(key);
							if (!isForm)
							{
								pageResources.Add(pdfTrueTypeFont, key);
								if (UsedFonts.Contains(key.Value))
								{
									if (ReplaceFonts.ContainsKey(key.Value))
									{
										ReplaceFonts[key.Value] = pdfTrueTypeFont;
									}
									else
									{
										ReplaceFonts.Add(key.Value, pdfTrueTypeFont);
									}
								}
							}
							else
							{
								PdfReferenceHolder value3 = new PdfReferenceHolder(pdfTrueTypeFont);
								usedFonts.Items.Add(key, value3);
								usedFonts.Modify();
							}
						}
					}
					else if (fontDescriptor != null)
					{
						PdfStream pdfStream = null;
						if (fontDescriptor.ContainsKey("FontFile2"))
						{
							pdfStream = GetStreamFromRefernceHolder(fontDescriptor["FontFile2"]);
						}
						else if (fontDescriptor.ContainsKey("FontFile3"))
						{
							pdfStream = GetStreamFromRefernceHolder(fontDescriptor["FontFile3"]);
						}
						else if (fontDescriptor.ContainsKey("FontFile"))
						{
							pdfStream = GetStreamFromRefernceHolder(fontDescriptor["FontFile"]);
						}
						if (pdfStream != null && pdfStream.ContainsKey("Subtype"))
						{
							PdfName pdfName5 = pdfStream["Subtype"] as PdfName;
							if (pdfName5 != null && pdfName5.Value == "OpenType")
							{
								pdfStream["Subtype"] = new PdfName("TrueType");
								pdfStream.Modify();
							}
						}
					}
				}
				else if (dictionaryFromRefernceHolder.ContainsKey("BaseFont"))
				{
					string empty = string.Empty;
					PdfName pdfName6 = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["BaseFont"]) as PdfName;
					if (pdfName6 != null && pdfName6.Value.Contains("+"))
					{
						empty = pdfName6.Value.Substring(pdfName6.Value.IndexOf('+') + 1);
						dictionaryFromRefernceHolder["BaseFont"] = new PdfName(empty);
					}
				}
			}
			if ((pdfName != null && pdfName.Value == "TrueType") || !flag)
			{
				PdfDictionary fontDescriptor2 = null;
				PdfDictionary descendantFont2 = null;
				GetFontInternals(dictionaryFromRefernceHolder, out fontDescriptor2, out descendantFont2);
				if (fontDescriptor2 == null || (!fontDescriptor2.ContainsKey("FontFile") && !fontDescriptor2.ContainsKey("FontFile2") && !fontDescriptor2.ContainsKey("FontFile3")))
				{
					string text3 = string.Empty;
					if (dictionaryFromRefernceHolder.ContainsKey("BaseFont"))
					{
						PdfName pdfName7 = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["BaseFont"]) as PdfName;
						if (pdfName7 != null)
						{
							text3 = pdfName7.Value.Substring(pdfName7.Value.IndexOf('+') + 1);
						}
					}
					FontStyle fontStyle = FontStyle.Regular;
					bool flag5 = false;
					if (text3.Contains("PSMT"))
					{
						text3 = text3.Remove(text3.IndexOf("PSMT"));
						flag5 = true;
					}
					else if (text3.Contains("BoldItalic"))
					{
						fontStyle = (FontStyle)3;
						text3 = text3.Remove(text3.IndexOf("BoldItalic"));
					}
					else if (text3.Contains("Bold"))
					{
						fontStyle = FontStyle.Bold;
						text3 = text3.Remove(text3.IndexOf("Bold"));
					}
					else if (text3.Contains("Italic"))
					{
						fontStyle = FontStyle.Italic;
						text3 = text3.Remove(text3.IndexOf("Italic"));
					}
					if (text3.Contains("PS"))
					{
						text3 = text3.Remove(text3.IndexOf("PS"));
						flag5 = true;
					}
					if (text3.Contains("-"))
					{
						text3 = text3.Remove(text3.IndexOf("-"));
					}
					if (text3.Contains(","))
					{
						text3 = text3.Remove(text3.IndexOf(","));
					}
					if (text3.Contains("#20"))
					{
						text3 = text3.Replace("#20", " ");
					}
					int num2 = int.MinValue;
					if (fontDescriptor2 != null && fontDescriptor2.ContainsKey("Flags") && PdfCrossTable.Dereference(fontDescriptor2["Flags"]) is PdfNumber pdfNumber2)
					{
						num2 = pdfNumber2.IntValue;
					}
					bool flag6 = false;
					if (pageResources != null && num2 != 96 && fontkeys.ContainsKey(pageResources))
					{
						flag6 = true;
						string text4 = string.Empty;
						List<string> list2 = fontkeys[pageResources];
						for (int j = 0; j < list2.Count; j++)
						{
							if (list2[j] != "TJ")
							{
								text4 = list2[j];
							}
							if (list2[j] == "TJ" && text4 == key.Value)
							{
								flag6 = false;
								break;
							}
						}
					}
					if (dictionaryFromRefernceHolder.ContainsKey("FirstChar") && PdfCrossTable.Dereference(dictionaryFromRefernceHolder["FirstChar"]) is PdfNumber pdfNumber3)
					{
						_ = pdfNumber3.IntValue;
					}
					if (dictionaryFromRefernceHolder.ContainsKey("LastChar") && PdfCrossTable.Dereference(dictionaryFromRefernceHolder["LastChar"]) is PdfNumber pdfNumber4)
					{
						_ = pdfNumber4.IntValue;
					}
					PdfName pdfName8 = null;
					if (dictionaryFromRefernceHolder.ContainsKey("Encoding"))
					{
						pdfName8 = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["Encoding"]) as PdfName;
						if (pdfName8 != null)
						{
							_ = pdfName8.Value == "WinAnsiEncoding";
						}
					}
					PdfTrueTypeFont pdfTrueTypeFont2 = null;
					if (UsedFonts.Contains(key.Value) && !ReplaceFonts.ContainsKey(key.Value) && num2 != 4 && num2 != 32 && !flag5 && flag6)
					{
						OldFonts.Add(key.Value, dictionaryFromRefernceHolder);
						if (!ldoc.RaisePdfFont)
						{
							throw new PdfException("Unable to embed the fonts, use SubstituteFont event to embed the fonts");
						}
						PdfFontEventArgs pdfFontEventArgs3 = new PdfFontEventArgs();
						pdfFontEventArgs3.m_fontName = text3;
						pdfFontEventArgs3.m_fontStyle = (PdfFontStyle)fontStyle;
						ldoc.PdfFontStream(pdfFontEventArgs3);
						pdfTrueTypeFont2 = new PdfTrueTypeFont(pdfFontEventArgs3.FontStream, pdfFontEventArgs3.m_fontStyle, size, string.Empty, useTrueType: false, isEnableEmbedding: true, isConformanceEnabled: true);
					}
					else
					{
						if (!ldoc.RaisePdfFont)
						{
							throw new PdfException("Unable to embed the fonts, use SubstituteFont event to embed the fonts");
						}
						PdfFontEventArgs pdfFontEventArgs4 = new PdfFontEventArgs();
						PdfFontStyle textFontStyle = (PdfFontStyle)fontStyle;
						string fontName = FindFontName(dictionaryFromRefernceHolder, text3, out textFontStyle);
						pdfFontEventArgs4.m_fontName = fontName;
						pdfFontEventArgs4.m_fontStyle = (PdfFontStyle)fontStyle;
						ldoc.PdfFontStream(pdfFontEventArgs4);
						pdfTrueTypeFont2 = new PdfTrueTypeFont(pdfFontEventArgs4.FontStream, pdfFontEventArgs4.m_fontStyle, size, string.Empty, useTrueType: true, isEnableEmbedding: true, isConformanceEnabled: true);
					}
					usedFonts.Remove(key);
					if (!isForm)
					{
						pageResources.Add(pdfTrueTypeFont2, key);
						if (UsedFonts.Contains(key.Value))
						{
							if (ReplaceFonts.ContainsKey(key.Value))
							{
								ReplaceFonts[key.Value] = pdfTrueTypeFont2;
							}
							else
							{
								ReplaceFonts.Add(key.Value, pdfTrueTypeFont2);
							}
						}
						else
						{
							PdfReferenceHolder value4 = new PdfReferenceHolder(pdfTrueTypeFont2);
							usedFonts[key] = value4;
							usedFonts.Modify();
							ReplaceFonts[key.Value] = pdfTrueTypeFont2;
						}
					}
					else
					{
						PdfReferenceHolder value5 = new PdfReferenceHolder(pdfTrueTypeFont2);
						usedFonts.Items.Add(key, value5);
						usedFonts.Modify();
					}
				}
				else if (dictionaryFromRefernceHolder.ContainsKey("Encoding") && PdfCrossTable.Dereference(dictionaryFromRefernceHolder["Encoding"]) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Differences"))
				{
					if (!OldFonts.ContainsKey(key.Value))
					{
						OldFonts.Add(key.Value, dictionaryFromRefernceHolder);
					}
					if (PdfALevel != PdfConformanceLevel.Pdf_A1B && !fontEmbedding)
					{
						if (!ReplaceFontDictionary.ContainsKey(key.Value))
						{
							PdfDictionary pdfDictionary4 = new PdfDictionary(dictionaryFromRefernceHolder);
							pdfDictionary4.Remove("Encoding");
							ReplaceFontDictionary.Add(key.Value, pdfDictionary4);
						}
					}
					else
					{
						PdfTrueTypeFont pdfTrueTypeFont3 = null;
						if (!ldoc.RaisePdfFont)
						{
							throw new PdfException("Unable to embed the fonts, use SubstituteFont event to embed the fonts");
						}
						PdfFontEventArgs pdfFontEventArgs5 = new PdfFontEventArgs();
						pdfFontEventArgs5.m_fontName = "Arial";
						pdfFontEventArgs5.m_fontStyle = PdfFontStyle.Regular;
						ldoc.PdfFontStream(pdfFontEventArgs5);
						pdfTrueTypeFont3 = new PdfTrueTypeFont(pdfFontEventArgs5.FontStream, pdfFontEventArgs5.m_fontStyle, size, string.Empty, useTrueType: false, isEnableEmbedding: true, isConformanceEnabled: true);
						if (!ReplaceFonts.ContainsKey(key.Value))
						{
							ReplaceFonts.Add(key.Value, pdfTrueTypeFont3);
						}
					}
				}
			}
			else if (pdfName != null && pdfName.Value == "Type0")
			{
				PdfDictionary pdfDictionary5 = null;
				PdfDictionary pdfDictionary6 = null;
				if (dictionaryFromRefernceHolder.ContainsKey("DescendantFonts"))
				{
					if (dictionaryFromRefernceHolder["DescendantFonts"] is PdfArray)
					{
						pdfDictionary6 = GetDictionaryFromRefernceHolder((dictionaryFromRefernceHolder["DescendantFonts"] as PdfArray).Elements[0]);
						if (pdfDictionary6.ContainsKey("FontDescriptor"))
						{
							pdfDictionary5 = GetDictionaryFromRefernceHolder(pdfDictionary6["FontDescriptor"]);
						}
					}
					else if (dictionaryFromRefernceHolder["DescendantFonts"] is PdfReferenceHolder)
					{
						PdfArray arrayFromReferenceHolder = GetArrayFromReferenceHolder(dictionaryFromRefernceHolder["DescendantFonts"]);
						pdfDictionary6 = GetDictionaryFromRefernceHolder(arrayFromReferenceHolder.Elements[0]);
						if (pdfDictionary6.ContainsKey("FontDescriptor"))
						{
							pdfDictionary5 = GetDictionaryFromRefernceHolder(pdfDictionary6["FontDescriptor"]);
						}
					}
				}
				if (pdfDictionary5 != null)
				{
					if (pdfDictionary5.ContainsKey("FontFile3"))
					{
						PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(pdfDictionary5["FontFile3"]);
						if (dictionaryFromRefernceHolder2 != null && pdfDictionary6 != null && pdfDictionary6.ContainsKey("Subtype") && (pdfDictionary6["Subtype"] as PdfName).Value == "CIDFontType0")
						{
							dictionaryFromRefernceHolder2["Subtype"] = new PdfName("CIDFontType0C");
						}
					}
					FontStructure fontStructure = new FontStructure(dictionaryFromRefernceHolder);
					if (dictionaryFromRefernceHolder.ContainsKey("ToUnicode"))
					{
						byte[] array = new byte[8] { 128, 64, 32, 16, 8, 4, 2, 1 };
						_ = fontStructure.FontGlyphWidths;
						if (fontStructure.CharacterMapTable != null && fontStructure.CharacterMapTable.Count != 0)
						{
							Dictionary<double, string> characterMapTable = fontStructure.CharacterMapTable;
							byte[] array2 = null;
							if (fontStructure.FontGlyphWidths != null && fontStructure.FontGlyphWidths.Count > 0 && fontStructure.FontGlyphWidths.Count >= fontStructure.CharacterMapTable.Count)
							{
								int[] array3 = new int[fontStructure.FontGlyphWidths.Count];
								fontStructure.FontGlyphWidths.Keys.CopyTo(array3, 0);
								Array.Sort(array3);
								array2 = new byte[array3[^1] / 8 + 1];
								foreach (int num3 in array3)
								{
									array2[num3 / 8] |= array[num3 % 8];
								}
								array3 = null;
							}
							if (array2 == null)
							{
								double[] array4 = new double[characterMapTable.Count];
								characterMapTable.Keys.CopyTo(array4, 0);
								Array.Sort(array4);
								array2 = new byte[(int)array4[^1] / 8 + 1];
								for (int l = 0; l < array4.Length; l++)
								{
									int num4 = (int)array4[l];
									array2[num4 / 8] |= array[num4 % 8];
								}
							}
							dictionaryFromRefernceHolder.Modify();
							PdfStream pdfStream2 = new PdfStream();
							pdfStream2.Write(array2);
							if (ldoc.Conformance == PdfConformanceLevel.Pdf_A1B || fontEmbedding)
							{
								pdfDictionary5["CIDSet"] = new PdfReferenceHolder(pdfStream2);
							}
							if (ldoc.Conformance != PdfConformanceLevel.Pdf_A1B && pdfDictionary5.ContainsKey("CIDSet"))
							{
								pdfDictionary5.Remove("CIDSet");
							}
							pdfDictionary5.Modify();
							array2 = null;
						}
						else if (fontStructure.UnicodeCharMapTable != null && fontStructure.UnicodeCharMapTable.Count > 0)
						{
							Dictionary<int, string> unicodeCharMapTable = fontStructure.UnicodeCharMapTable;
							byte[] array5 = null;
							int[] array6 = new int[unicodeCharMapTable.Count];
							unicodeCharMapTable.Keys.CopyTo(array6, 0);
							Array.Sort(array6);
							array5 = new byte[array6[^1] / 8 + 1];
							foreach (int num5 in array6)
							{
								array5[num5 / 8] |= array[num5 % 8];
							}
							PdfStream pdfStream3 = new PdfStream();
							pdfStream3.Write(array5);
							pdfDictionary5["CIDSet"] = new PdfReferenceHolder(pdfStream3);
							pdfDictionary5.Modify();
							array5 = null;
							array6 = null;
						}
						_ = string.Empty;
						if (dictionaryFromRefernceHolder.ContainsKey("BaseFont"))
						{
							PdfName pdfName9 = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["BaseFont"]) as PdfName;
							PdfName pdfName10 = PdfCrossTable.Dereference(pdfDictionary6["CIDToGIDMap"]) as PdfName;
							if (pdfName9 != null && !pdfName9.Value.Contains("+") && pdfName10 != null && pdfName10.Value == "Identity")
							{
								MapWidthTable(pdfDictionary5, pdfDictionary6);
							}
						}
					}
					else
					{
						byte[] array7 = new byte[8] { 128, 64, 32, 16, 8, 4, 2, 1 };
						byte[] array8 = null;
						if (fontStructure.FontGlyphWidths != null && fontStructure.FontGlyphWidths.Count > 0)
						{
							int[] array9 = new int[fontStructure.FontGlyphWidths.Count];
							fontStructure.FontGlyphWidths.Keys.CopyTo(array9, 0);
							Array.Sort(array9);
							array8 = new byte[array9[^1] / 8 + 1];
							foreach (int num6 in array9)
							{
								array8[num6 / 8] |= array7[num6 % 8];
							}
							array9 = null;
						}
						if (pdfDictionary6 != null && !pdfDictionary6.ContainsKey("CIDToGIDMap"))
						{
							pdfDictionary6.Items.Add(new PdfName("CIDToGIDMap"), new PdfName("Identity"));
							pdfDictionary6.Modify();
						}
						if (array8 != null)
						{
							PdfStream pdfStream4 = new PdfStream();
							pdfStream4.Write(array8);
							pdfDictionary5["CIDSet"] = new PdfReferenceHolder(pdfStream4);
							if (ldoc.Conformance == PdfConformanceLevel.Pdf_A2B && pdfDictionary5.ContainsKey("CIDSet"))
							{
								pdfDictionary5.Remove("CIDSet");
							}
							pdfDictionary5.Modify();
							array8 = null;
						}
					}
					if (pdfDictionary5.ContainsKey("Flags"))
					{
						int num7 = int.MinValue;
						if (PdfCrossTable.Dereference(pdfDictionary5["Flags"]) is PdfNumber pdfNumber5)
						{
							num7 = pdfNumber5.IntValue;
						}
						if (((uint)num7 & 4u) != 0)
						{
							pdfDictionary5["Flags"] = new PdfNumber(34);
							dictionaryFromRefernceHolder.Modify();
						}
					}
					if (fontStructure.IsCID && pdfDictionary6 != null && !pdfDictionary6.ContainsKey("CIDToGIDMap"))
					{
						pdfDictionary6["CIDToGIDMap"] = new PdfName("Identity");
						pdfDictionary6.Modify();
					}
				}
			}
		}
		if (dictionaryFromRefernceHolder.ContainsKey("ToUnicode"))
		{
			PdfDictionary pdfDictionary7 = PdfCrossTable.Dereference(dictionaryFromRefernceHolder["ToUnicode"]) as PdfStream;
			if (pdfDictionary7 != null && pdfDictionary7.isSkip)
			{
				pdfDictionary7.isSkip = false;
			}
		}
	}

	private string FindFontName(PdfDictionary fontDictionary, string name, out PdfFontStyle textFontStyle)
	{
		textFontStyle = PdfFontStyle.Regular;
		PdfName pdfName = null;
		fontDictionary = PdfCrossTable.Dereference(fontDictionary[name]) as PdfDictionary;
		if (fontDictionary != null && fontDictionary.ContainsKey("BaseFont"))
		{
			pdfName = PdfCrossTable.Dereference(fontDictionary["BaseFont"]) as PdfName;
			if (pdfName != null)
			{
				name = PdfName.DecodeName(pdfName.Value);
				textFontStyle = GetFontStyle(pdfName.Value);
				if (name.Contains("PSMT"))
				{
					name = name.Remove(name.IndexOf("PSMT"));
				}
				if (name.Contains("PS"))
				{
					name = name.Remove(name.IndexOf("PS"));
				}
				if (name.Contains("-"))
				{
					name = name.Remove(name.IndexOf("-"));
				}
			}
		}
		else if (fontDictionary != null)
		{
			PdfName pdfName2 = null;
			PdfDictionary pdfDictionary = fontDictionary;
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(fontDictionary["FontDescriptor"]) as PdfDictionary;
			if (pdfDictionary2 == null && fontDictionary.ContainsKey("DescendantFonts") && PdfCrossTable.Dereference(fontDictionary["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray)
			{
				if (PdfCrossTable.Dereference(pdfArray[0]) is PdfDictionary pdfDictionary3)
				{
					pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary3["FontDescriptor"]) as PdfDictionary;
				}
				if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("FontName"))
				{
					pdfName2 = pdfDictionary2["FontName"] as PdfName;
					if (pdfName2 != null)
					{
						string text = pdfName2.Value.Substring(pdfName2.Value.IndexOf('+') + 1);
						textFontStyle = GetFontStyle(text);
						if (text.Contains("PSMT"))
						{
							text = text.Remove(text.IndexOf("PSMT"));
						}
						if (text.Contains("PS"))
						{
							text = text.Remove(text.IndexOf("PS"));
						}
						if (text.Contains("-"))
						{
							text = text.Remove(text.IndexOf("-"));
						}
					}
				}
			}
		}
		else if (name != null)
		{
			textFontStyle = GetFontStyle(name);
		}
		string[] array2;
		if (name.Contains("#"))
		{
			string[] array = name.Split('#');
			string text2 = string.Empty;
			array2 = array;
			foreach (string text3 in array2)
			{
				text2 = (text3.Contains("20") ? (text2 + text3.Substring(2)) : (text2 + text3));
			}
			name = text2;
		}
		string text4 = name;
		string[] array3 = new string[1] { "" };
		int num = 0;
		for (int j = 0; j < text4.Length; j++)
		{
			string text5 = text4.Substring(j, 1);
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Contains(text5) && j > 0 && !"ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Contains(text4[j - 1].ToString()))
			{
				num++;
				string[] array4 = new string[num + 1];
				Array.Copy(array3, 0, array4, 0, num);
				array3 = array4;
			}
			if (!text5.Contains(" "))
			{
				array3[num] += text5;
			}
		}
		name = string.Empty;
		array2 = array3;
		foreach (string text6 in array2)
		{
			name = name + text6 + " ";
		}
		name = name.Substring(name.IndexOf('+') + 1);
		name = name.Trim();
		name = name.Split(',')[0];
		if (name == "Arial MT")
		{
			name = "Arial";
		}
		return name;
	}

	private void FormEncoding(PdfLoadedDocument document)
	{
		if (document.Form == null || document.Form.Fields == null || document.Form.Fields.Count <= 0)
		{
			return;
		}
		foreach (PdfField field in document.Form.Fields)
		{
			if (!(field is PdfLoadedField))
			{
				continue;
			}
			PdfLoadedField pdfLoadedField = field as PdfLoadedField;
			PdfDictionary dictionary = pdfLoadedField.Dictionary;
			if (pdfLoadedField.Dictionary.ContainsKey("Kids"))
			{
				if (pdfLoadedField is PdfLoadedRadioButtonListField)
				{
					foreach (PdfLoadedRadioButtonItem item in (pdfLoadedField as PdfLoadedRadioButtonListField).Items)
					{
						RepairFormAppearenaceDirectory(item.Dictionary, isButtonField: false);
					}
				}
				else if (pdfLoadedField is PdfLoadedButtonField)
				{
					foreach (PdfLoadedButtonItem item2 in (pdfLoadedField as PdfLoadedButtonField).Items)
					{
						RepairFormAppearenaceDirectory(item2.Dictionary, isButtonField: true);
					}
				}
				else if (pdfLoadedField is PdfLoadedCheckBoxField)
				{
					foreach (PdfLoadedCheckBoxItem item3 in (pdfLoadedField as PdfLoadedCheckBoxField).Items)
					{
						RepairFormAppearenaceDirectory(item3.Dictionary, isButtonField: true);
					}
				}
				else if (pdfLoadedField is PdfLoadedComboBoxField)
				{
					foreach (PdfLoadedComboBoxItem item4 in (pdfLoadedField as PdfLoadedComboBoxField).Items)
					{
						RepairFormAppearenaceDirectory(item4.Dictionary, isButtonField: false);
					}
				}
				else if (pdfLoadedField is PdfLoadedStateField)
				{
					foreach (PdfLoadedStateItem item5 in (pdfLoadedField as PdfLoadedStateField).Items)
					{
						RepairFormAppearenaceDirectory(item5.Dictionary, isButtonField: false);
					}
				}
				else if (pdfLoadedField is PdfLoadedTextBoxField)
				{
					foreach (PdfLoadedTexBoxItem item6 in (pdfLoadedField as PdfLoadedTextBoxField).Items)
					{
						RepairFormAppearenaceDirectory(item6.Dictionary, isButtonField: false);
					}
				}
				else if (pdfLoadedField is PdfLoadedListBoxField)
				{
					foreach (PdfLoadedListFieldItem item7 in (pdfLoadedField as PdfLoadedListBoxField).Items)
					{
						RepairFormAppearenaceDirectory(item7.Dictionary, isButtonField: false);
					}
				}
			}
			RepairFormAppearenaceDirectory(dictionary, isButtonField: false);
		}
	}

	private void ParseForm(PdfLoadedDocument document)
	{
		if (document.Form != null)
		{
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(document.Form.Dictionary);
			if (dictionaryFromRefernceHolder.ContainsKey("NeedAppearances"))
			{
				dictionaryFromRefernceHolder.Remove("NeedAppearances");
				dictionaryFromRefernceHolder.Modify();
				document.Form.NeedAppearances = false;
			}
			if (document.Form.Resources.ContainsKey("Font"))
			{
				EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(document.Form.Resources["Font"]), isForm: true);
			}
			if (document.Form.Fields != null && document.Form.Fields.Count > 0)
			{
				foreach (PdfField field in document.Form.Fields)
				{
					if (field is PdfLoadedField)
					{
						PdfLoadedField pdfLoadedField = field as PdfLoadedField;
						PdfDictionary dictionary = pdfLoadedField.Dictionary;
						if (dictionary.ContainsKey("DR"))
						{
							if (dictionary["DR"] is PdfResources)
							{
								PdfResources resourceFromRefernceHolder = GetResourceFromRefernceHolder(dictionary["DR"]);
								if (resourceFromRefernceHolder.ContainsKey("Font"))
								{
									EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(resourceFromRefernceHolder["Font"]), isForm: true);
								}
							}
							else if (dictionary["DR"] is PdfDictionary)
							{
								PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionary["DR"]);
								if (dictionaryFromRefernceHolder2.ContainsKey("Font"))
								{
									EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2["Font"]), isForm: true);
								}
							}
						}
						if (pdfLoadedField.Dictionary.ContainsKey("Kids"))
						{
							if (pdfLoadedField is PdfLoadedRadioButtonListField)
							{
								foreach (PdfLoadedRadioButtonItem item in (pdfLoadedField as PdfLoadedRadioButtonListField).Items)
								{
									RepairFormAppearenaceDirectory(item.Dictionary, isButtonField: false);
								}
							}
							else if (pdfLoadedField is PdfLoadedButtonField)
							{
								foreach (PdfLoadedButtonItem item2 in (pdfLoadedField as PdfLoadedButtonField).Items)
								{
									RepairFormAppearenaceDirectory(item2.Dictionary, isButtonField: true);
								}
							}
							else if (pdfLoadedField is PdfLoadedCheckBoxField)
							{
								foreach (PdfLoadedCheckBoxItem item3 in (pdfLoadedField as PdfLoadedCheckBoxField).Items)
								{
									RepairFormAppearenaceDirectory(item3.Dictionary, isButtonField: true);
								}
							}
							else if (pdfLoadedField is PdfLoadedComboBoxField)
							{
								foreach (PdfLoadedComboBoxItem item4 in (pdfLoadedField as PdfLoadedComboBoxField).Items)
								{
									RepairFormAppearenaceDirectory(item4.Dictionary, isButtonField: false);
								}
							}
							else if (pdfLoadedField is PdfLoadedStateField)
							{
								foreach (PdfLoadedStateItem item5 in (pdfLoadedField as PdfLoadedStateField).Items)
								{
									RepairFormAppearenaceDirectory(item5.Dictionary, isButtonField: false);
								}
							}
							else if (pdfLoadedField is PdfLoadedTextBoxField)
							{
								foreach (PdfLoadedTexBoxItem item6 in (pdfLoadedField as PdfLoadedTextBoxField).Items)
								{
									RepairFormAppearenaceDirectory(item6.Dictionary, isButtonField: false);
								}
							}
							else if (pdfLoadedField is PdfLoadedListBoxField)
							{
								foreach (PdfLoadedListFieldItem item7 in (pdfLoadedField as PdfLoadedListBoxField).Items)
								{
									RepairFormAppearenaceDirectory(item7.Dictionary, isButtonField: false);
								}
							}
						}
						RepairFormFontDirectory(dictionary, pdfLoadedField);
					}
				}
				return;
			}
			ParseFormDictionary(document);
		}
		else
		{
			ParseFormDictionary(document);
		}
	}

	private void ParseFormDictionary(PdfLoadedDocument document)
	{
		foreach (PdfPageBase page in document.Pages)
		{
			PdfResources resources = page.GetResources();
			if (resources == null || !resources.ContainsKey("XObject"))
			{
				continue;
			}
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(resources["XObject"]);
			if (dictionaryFromRefernceHolder == null)
			{
				continue;
			}
			foreach (PdfName item in new List<PdfName>(dictionaryFromRefernceHolder.Keys))
			{
				PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder[item]);
				ParseFormStreamFromXObject(dictionaryFromRefernceHolder2, item.Value);
			}
		}
	}

	private void RepairFormAppearenaceDirectory(PdfDictionary loadedFieldDictionary, bool isButtonField)
	{
		bool flag = false;
		if (loadedFieldDictionary.ContainsKey("AA"))
		{
			loadedFieldDictionary.Remove("AA");
			loadedFieldDictionary.Modify();
		}
		if (loadedFieldDictionary.ContainsKey("A"))
		{
			loadedFieldDictionary.Remove("A");
			loadedFieldDictionary.Modify();
		}
		if (loadedFieldDictionary.ContainsKey("AP"))
		{
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(loadedFieldDictionary["AP"]);
			if (isButtonField && dictionaryFromRefernceHolder.Items.Count > 0)
			{
				Dictionary<PdfName, IPdfPrimitive> items = dictionaryFromRefernceHolder.Items;
				KeyValuePair<PdfName, IPdfPrimitive> keyValuePair = default(KeyValuePair<PdfName, IPdfPrimitive>);
				PdfDictionary pdfDictionary = null;
				if (items.Count == 1)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in items)
					{
						keyValuePair = item;
						if (item.Value is PdfDictionary)
						{
							pdfDictionary = item.Value as PdfDictionary;
						}
					}
					if (keyValuePair.Key != null && keyValuePair.Key.Value == "N")
					{
						dictionaryFromRefernceHolder.Remove("N");
						if (pdfDictionary != null)
						{
							flag = true;
							dictionaryFromRefernceHolder["N"] = pdfDictionary;
						}
					}
				}
			}
			if (flag)
			{
				return;
			}
			if (dictionaryFromRefernceHolder.ContainsKey("D") && dictionaryFromRefernceHolder.ContainsKey("N"))
			{
				if (dictionaryFromRefernceHolder["N"] is PdfNull)
				{
					if (isButtonField)
					{
						loadedFieldDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
						loadedFieldDictionary["AS"] = new PdfName("Off");
					}
					else
					{
						loadedFieldDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
					}
					loadedFieldDictionary.Modify();
				}
				else if (isButtonField)
				{
					PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["N"]);
					if (dictionaryFromRefernceHolder2 != null && dictionaryFromRefernceHolder2 is PdfStream)
					{
						PdfReferenceHolder value = new PdfReferenceHolder(dictionaryFromRefernceHolder["N"]);
						dictionaryFromRefernceHolder["N"] = new PdfDictionary();
						(dictionaryFromRefernceHolder["N"] as PdfDictionary)["Off"] = value;
						loadedFieldDictionary["AS"] = new PdfName("Off");
						dictionaryFromRefernceHolder.Modify();
						loadedFieldDictionary.Modify();
					}
				}
				dictionaryFromRefernceHolder.Remove("D");
				dictionaryFromRefernceHolder.Modify();
			}
			else
			{
				if (!dictionaryFromRefernceHolder.ContainsKey("N"))
				{
					return;
				}
				if (dictionaryFromRefernceHolder["N"] is PdfNull)
				{
					if (isButtonField)
					{
						loadedFieldDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
						loadedFieldDictionary["AS"] = new PdfName("Off");
					}
					else
					{
						loadedFieldDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
					}
				}
				else if (isButtonField)
				{
					PdfReferenceHolder value2 = new PdfReferenceHolder(dictionaryFromRefernceHolder["N"]);
					dictionaryFromRefernceHolder["N"] = new PdfDictionary();
					(dictionaryFromRefernceHolder["N"] as PdfDictionary)["Off"] = value2;
					loadedFieldDictionary["AS"] = new PdfName("Off");
					dictionaryFromRefernceHolder.Modify();
					loadedFieldDictionary.Modify();
				}
				loadedFieldDictionary.Modify();
			}
		}
		else
		{
			if (isButtonField)
			{
				loadedFieldDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
				loadedFieldDictionary["AS"] = new PdfName("Off");
			}
			else
			{
				loadedFieldDictionary["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
			}
			loadedFieldDictionary.Modify();
		}
	}

	private void RepairFormFontDirectory(IPdfPrimitive formField, PdfLoadedField loadedField)
	{
		PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(formField);
		if (dictionaryFromRefernceHolder.ContainsKey("AA"))
		{
			dictionaryFromRefernceHolder.Remove("AA");
			dictionaryFromRefernceHolder.Modify();
		}
		if (dictionaryFromRefernceHolder.ContainsKey("A"))
		{
			dictionaryFromRefernceHolder.Remove("A");
			dictionaryFromRefernceHolder.Modify();
		}
		if (dictionaryFromRefernceHolder.ContainsKey("NeedAppearances"))
		{
			dictionaryFromRefernceHolder["NeedAppearances"] = new PdfBoolean(value: false);
		}
		if (dictionaryFromRefernceHolder.ContainsKey("AP"))
		{
			PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["AP"]);
			if (dictionaryFromRefernceHolder2.ContainsKey("D"))
			{
				if (dictionaryFromRefernceHolder2.ContainsKey("N"))
				{
					dictionaryFromRefernceHolder2.Remove("D");
					dictionaryFromRefernceHolder2.Modify();
				}
				else
				{
					PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2["D"]);
					if (dictionaryFromRefernceHolder3.ContainsKey(new PdfName("Home")))
					{
						PdfStream streamFromRefernceHolder = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder3[new PdfName("Home")]);
						if (streamFromRefernceHolder.ContainsKey("Resources"))
						{
							PdfDictionary dictionaryFromRefernceHolder4 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder["Resources"]);
							if (dictionaryFromRefernceHolder4.ContainsKey("Font"))
							{
								EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder4["Font"]), isForm: true);
							}
						}
					}
					if (dictionaryFromRefernceHolder3.ContainsKey(new PdfName("Off")))
					{
						PdfStream streamFromRefernceHolder2 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder3[new PdfName("Off")]);
						if (streamFromRefernceHolder2.ContainsKey("Resources"))
						{
							PdfDictionary dictionaryFromRefernceHolder5 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder2["Resources"]);
							if (dictionaryFromRefernceHolder5.ContainsKey("Font"))
							{
								EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder5["Font"]), isForm: true);
							}
						}
					}
				}
			}
			if (dictionaryFromRefernceHolder2.ContainsKey("N"))
			{
				PdfStream streamFromRefernceHolder3 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder2["N"]);
				if (streamFromRefernceHolder3 != null)
				{
					if (loadedField is PdfLoadedButtonField)
					{
						PdfReferenceHolder value = new PdfReferenceHolder(dictionaryFromRefernceHolder2["N"]);
						dictionaryFromRefernceHolder2["N"] = new PdfDictionary();
						(dictionaryFromRefernceHolder2["N"] as PdfDictionary)["Off"] = value;
						dictionaryFromRefernceHolder["AS"] = new PdfName("Off");
						dictionaryFromRefernceHolder2.Modify();
					}
					if (streamFromRefernceHolder3.ContainsKey("Resources"))
					{
						PdfDictionary dictionaryFromRefernceHolder6 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder3["Resources"]);
						if (dictionaryFromRefernceHolder6 != null && dictionaryFromRefernceHolder6.ContainsKey("Font"))
						{
							PdfDictionary dictionaryFromRefernceHolder7 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder6["Font"]);
							EmbedCompleteFonts(null, dictionaryFromRefernceHolder7, isForm: true);
						}
						dictionaryFromRefernceHolder6 = null;
					}
					ParseFormStream(streamFromRefernceHolder3, "", null, enableRecrusiveCall: true);
				}
				else
				{
					PdfDictionary dictionaryFromRefernceHolder8 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder2["N"]);
					if (dictionaryFromRefernceHolder8 != null)
					{
						if (loadedField is PdfLoadedButtonField)
						{
							PdfReferenceHolder value2 = new PdfReferenceHolder(dictionaryFromRefernceHolder2["N"]);
							dictionaryFromRefernceHolder2["N"] = new PdfDictionary();
							(dictionaryFromRefernceHolder2["N"] as PdfDictionary)["Off"] = value2;
							dictionaryFromRefernceHolder["AS"] = new PdfName("Off");
							dictionaryFromRefernceHolder2.Modify();
						}
						else
						{
							foreach (PdfName item in new List<PdfName>(dictionaryFromRefernceHolder8.Keys))
							{
								PdfStream streamFromRefernceHolder4 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder8[item]);
								if (streamFromRefernceHolder4 != null)
								{
									if (streamFromRefernceHolder4.ContainsKey("Resources"))
									{
										PdfDictionary dictionaryFromRefernceHolder9 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder4["Resources"]);
										if (dictionaryFromRefernceHolder9.ContainsKey("Font"))
										{
											EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder9["Font"]), isForm: true);
										}
									}
								}
								else
								{
									if (loadedField is PdfLoadedButtonField)
									{
										dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
										dictionaryFromRefernceHolder["AS"] = new PdfName("Off");
									}
									else
									{
										dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
									}
									dictionaryFromRefernceHolder.Modify();
								}
							}
						}
						if (dictionaryFromRefernceHolder8.ContainsKey("Resources"))
						{
							PdfDictionary dictionaryFromRefernceHolder10 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder8["Resources"]);
							if (dictionaryFromRefernceHolder10 != null && dictionaryFromRefernceHolder10.ContainsKey("Font"))
							{
								PdfDictionary dictionaryFromRefernceHolder11 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder10["Font"]);
								EmbedCompleteFonts(null, dictionaryFromRefernceHolder11, isForm: true);
							}
							dictionaryFromRefernceHolder10 = null;
						}
						foreach (PdfName item2 in new List<PdfName>(dictionaryFromRefernceHolder8.Items.Keys))
						{
							PdfStream streamFromRefernceHolder5 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder8.Items[item2]);
							if (streamFromRefernceHolder5 != null)
							{
								ParseFormStream(streamFromRefernceHolder5, item2.Value, null, enableRecrusiveCall: true);
							}
						}
					}
					else if (dictionaryFromRefernceHolder2["N"] is PdfNull)
					{
						if (loadedField is PdfLoadedButtonField)
						{
							dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
							dictionaryFromRefernceHolder["AS"] = new PdfName("Off");
						}
						else
						{
							dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
						}
						dictionaryFromRefernceHolder.Modify();
					}
				}
			}
			if (!dictionaryFromRefernceHolder2.ContainsKey("N") && !dictionaryFromRefernceHolder2.ContainsKey("D"))
			{
				if (loadedField is PdfLoadedButtonField)
				{
					dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
					dictionaryFromRefernceHolder["AS"] = new PdfName("Off");
				}
				else
				{
					dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
				}
				dictionaryFromRefernceHolder.Modify();
			}
		}
		else
		{
			if (loadedField is PdfLoadedButtonField)
			{
				dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: true);
				dictionaryFromRefernceHolder["AS"] = new PdfName("Off");
			}
			else
			{
				dictionaryFromRefernceHolder["AP"] = CreateNewAppearanceDictionary(isButtonField: false);
			}
			dictionaryFromRefernceHolder.Modify();
		}
		if (!dictionaryFromRefernceHolder.ContainsKey("MK"))
		{
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder12 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["MK"]);
		if (dictionaryFromRefernceHolder12 == null || dictionaryFromRefernceHolder12.Items.Count <= 0)
		{
			return;
		}
		if (dictionaryFromRefernceHolder12.ContainsKey("D"))
		{
			PdfDictionary dictionaryFromRefernceHolder13 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder12["D"]);
			if (dictionaryFromRefernceHolder13.ContainsKey(new PdfName("Home")))
			{
				PdfStream streamFromRefernceHolder6 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder13[new PdfName("Home")]);
				if (streamFromRefernceHolder6.ContainsKey("Resources"))
				{
					PdfDictionary dictionaryFromRefernceHolder14 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder6["Resources"]);
					if (dictionaryFromRefernceHolder14.ContainsKey("Font"))
					{
						EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder14["Font"]), isForm: true);
					}
				}
			}
			if (dictionaryFromRefernceHolder13.ContainsKey(new PdfName("Yes")))
			{
				PdfStream streamFromRefernceHolder7 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder13[new PdfName("Yes")]);
				if (streamFromRefernceHolder7.ContainsKey("Resources"))
				{
					PdfDictionary dictionaryFromRefernceHolder15 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder7["Resources"]);
					if (dictionaryFromRefernceHolder15.ContainsKey("Font"))
					{
						EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder15["Font"]), isForm: true);
					}
				}
			}
			if (dictionaryFromRefernceHolder13.ContainsKey(new PdfName("Off")))
			{
				PdfStream streamFromRefernceHolder8 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder13[new PdfName("Off")]);
				if (streamFromRefernceHolder8.ContainsKey("Resources"))
				{
					PdfDictionary dictionaryFromRefernceHolder16 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder8["Resources"]);
					if (dictionaryFromRefernceHolder16.ContainsKey("Font"))
					{
						EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder16["Font"]), isForm: true);
					}
				}
			}
		}
		if (!dictionaryFromRefernceHolder12.ContainsKey("N"))
		{
			return;
		}
		PdfDictionary dictionaryFromRefernceHolder17 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder12["N"]);
		if (dictionaryFromRefernceHolder17.ContainsKey(new PdfName("Home")))
		{
			PdfStream streamFromRefernceHolder9 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder17[new PdfName("Home")]);
			if (streamFromRefernceHolder9.ContainsKey("Resources"))
			{
				PdfDictionary dictionaryFromRefernceHolder18 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder9["Resources"]);
				if (dictionaryFromRefernceHolder18.ContainsKey("Font"))
				{
					EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder18["Font"]), isForm: true);
				}
			}
		}
		if (dictionaryFromRefernceHolder17.ContainsKey(new PdfName("Yes")))
		{
			PdfStream streamFromRefernceHolder10 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder17[new PdfName("Yes")]);
			if (streamFromRefernceHolder10.ContainsKey("Resources"))
			{
				PdfDictionary dictionaryFromRefernceHolder19 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder10["Resources"]);
				if (dictionaryFromRefernceHolder19.ContainsKey("Font"))
				{
					EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder19["Font"]), isForm: true);
				}
			}
		}
		if (!dictionaryFromRefernceHolder17.ContainsKey(new PdfName("Off")))
		{
			return;
		}
		PdfStream streamFromRefernceHolder11 = GetStreamFromRefernceHolder(dictionaryFromRefernceHolder17[new PdfName("Off")]);
		if (streamFromRefernceHolder11.ContainsKey("Resources"))
		{
			PdfDictionary dictionaryFromRefernceHolder20 = GetDictionaryFromRefernceHolder(streamFromRefernceHolder11["Resources"]);
			if (dictionaryFromRefernceHolder20.ContainsKey("Font"))
			{
				EmbedCompleteFonts(null, GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder20["Font"]), isForm: true);
			}
		}
	}

	private PdfDictionary CreateNewAppearanceDictionary(bool isButtonField)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfStream pdfStream = new PdfStream(new PdfDictionary(), Encoding.UTF8.GetBytes(" "));
		pdfStream["Type"] = new PdfName("XObject");
		pdfStream["Resources"] = new PdfReferenceHolder(new PdfNull());
		pdfStream["Filter"] = new PdfName("FlateDecode");
		double[] array = new double[4] { 0.0, 0.0, 139.5, 13.1151 };
		pdfStream["BBox"] = new PdfArray(array);
		pdfStream["Subtype"] = new PdfName("Form");
		if (!isButtonField)
		{
			pdfDictionary["N"] = new PdfReferenceHolder(pdfStream);
		}
		else
		{
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			pdfDictionary2["Off"] = new PdfReferenceHolder(pdfStream);
			pdfDictionary["N"] = pdfDictionary2;
		}
		return pdfDictionary;
	}

	protected void AddDocumentColorProfile(PdfLoadedDocument document)
	{
		if (PdfALevel != PdfConformanceLevel.Pdf_A1B && document.Catalog.ContainsKey("OCProperties"))
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(document.Catalog["OCProperties"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("D"))
			{
				PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["D"]) as PdfDictionary;
				if (pdfDictionary2.ContainsKey("AS"))
				{
					pdfDictionary2.Remove("AS");
				}
				if (PdfALevel != PdfConformanceLevel.Pdf_A2U && PdfALevel != PdfConformanceLevel.Pdf_A3U && pdfDictionary2.ContainsKey("Order"))
				{
					pdfDictionary2.Remove("Order");
				}
				if (!pdfDictionary2.ContainsKey("Name"))
				{
					pdfDictionary2.SetString("Name", "Default");
				}
			}
			document.Catalog.Modify();
		}
		else
		{
			document.Catalog.Remove("OCProperties");
			document.Catalog.Modify();
		}
		if (document.Catalog.ContainsKey("OpenAction") && document.Catalog["OpenAction"] is PdfDictionary && (document.Catalog["OpenAction"] as PdfDictionary).ContainsKey("S"))
		{
			PdfName pdfName = (document.Catalog["OpenAction"] as PdfDictionary)["S"] as PdfName;
			if (pdfName != null && !IsValidAction(pdfName.Value))
			{
				document.Catalog.Remove("OpenAction");
				document.Catalog.Modify();
			}
		}
		if (document.Catalog.ContainsKey("AA"))
		{
			document.Catalog.Remove("AA");
			document.Catalog.Modify();
		}
		if (document.Catalog.ContainsKey("StructTreeRoot"))
		{
			document.Catalog.Remove("StructTreeRoot");
			document.Catalog.Modify();
		}
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		pdfDictionary3["Info"] = new PdfString("sRGB IEC61966-2.1");
		pdfDictionary3["S"] = new PdfName("GTS_PDFA1");
		pdfDictionary3["OutputConditionIdentifier"] = new PdfString("custom");
		pdfDictionary3["Type"] = new PdfName("OutputIntent");
		pdfDictionary3["OutputCondition"] = new PdfString("");
		pdfDictionary3["RegistryName"] = new PdfString("");
		PdfICCColorProfile pdfICCColorProfile = new PdfICCColorProfile();
		(pdfICCColorProfile.Element as PdfStream)["Range"] = new PdfArray(new int[6] { 0, 1, 0, 1, 0, 1 });
		pdfDictionary3["DestOutputProfile"] = new PdfReferenceHolder(pdfICCColorProfile);
		PdfArray pdfArray = new PdfArray();
		pdfArray.Add(pdfDictionary3);
		document.Catalog["OutputIntents"] = pdfArray;
	}

	protected void AddTrailerID(PdfLoadedDocument document)
	{
		if (document.CrossTable.Trailer.ContainsKey("ID"))
		{
			document.CrossTable.Trailer.Items.Remove(new PdfName("ID"));
		}
		if (document.CrossTable.Trailer.ContainsKey("Encrypt"))
		{
			if (document.CrossTable.Trailer["Encrypt"] is PdfReferenceHolder && (object)(document.CrossTable.Trailer["Encrypt"] as PdfReferenceHolder).Reference != null)
			{
				int objectIndex = document.CrossTable.PdfObjects.GetObjectIndex((document.CrossTable.Trailer["Encrypt"] as PdfReferenceHolder).Reference);
				if (objectIndex >= 0)
				{
					document.CrossTable.PdfObjects.Remove(objectIndex);
				}
			}
			document.CrossTable.Trailer.Items.Remove(new PdfName("Encrypt"));
		}
		PdfSecurity security = document.Security;
		security.Encryptor = new PdfEncryptor();
		document.CrossTable.Trailer["ID"] = security.Encryptor.FileID;
		document.CrossTable.Trailer.Modify();
		PdfDocument.ConformanceLevel = PdfALevel;
	}

	protected void AddMetaDataInfo(PdfLoadedDocument document)
	{
		XmpMetadata metadata = document.Catalog.Metadata;
		PdfDocumentInformation documentInformation = document.DocumentInformation;
		documentInformation.ConformanceEnabled = true;
		document.Conformance = PdfConformanceLevel.None;
		if (document.Catalog.ContainsKey("Metadata"))
		{
			document.Catalog.Remove("Metadata");
			document.Catalog.Metadata = null;
		}
		PdfDictionary dictionary = documentInformation.Dictionary;
		List<PdfName> list = new List<PdfName>();
		foreach (PdfName key in dictionary.Keys)
		{
			if (key != "Author" && key != "Title" && key != "Subject" && key != "Trapped" && key != "Keywords" && key != "Producer" && key != "CreationDate" && key != "ModDate" && key != "Creator" && !(documentInformation.Dictionary[key] is PdfString))
			{
				list.Add(key);
			}
		}
		if (!list.Contains(new PdfName("Contents")) || !list.Contains(new PdfName("Parent")))
		{
			foreach (PdfName item in list)
			{
				documentInformation.Dictionary.Remove(item);
			}
		}
		if (documentInformation.Dictionary is PdfStream)
		{
			PdfDictionary pdfDictionary = new PdfDictionary();
			PdfDictionary dictionary2 = documentInformation.Dictionary;
			if (dictionary2 != null)
			{
				foreach (PdfName key2 in dictionary2.Keys)
				{
					pdfDictionary[key2] = dictionary2[key2];
				}
				documentInformation.m_dictionary = pdfDictionary;
			}
		}
		if (documentInformation.Dictionary != null && documentInformation.Dictionary.ContainsKey("CreationDate") && PdfCrossTable.Dereference(documentInformation.Dictionary["CreationDate"]) is PdfString)
		{
			documentInformation.Dictionary.Remove("CreationDate");
			documentInformation.CreationDate = DateTime.Now;
		}
		if (metadata != null)
		{
			if (metadata.BasicSchema != null && !string.IsNullOrEmpty(metadata.BasicSchema.CreatorTool) && string.IsNullOrEmpty(documentInformation.Creator))
			{
				documentInformation.Creator = metadata.BasicSchema.CreatorTool;
			}
			if (metadata.PDFSchema != null && documentInformation.Dictionary.ContainsKey("Producer") && !string.IsNullOrEmpty(metadata.PDFSchema.Producer) && string.IsNullOrEmpty(documentInformation.Producer))
			{
				documentInformation.Producer = metadata.PDFSchema.Producer;
			}
		}
		string title = document.DocumentInformation.Title;
		byte[] bytes = Encoding.UTF8.GetBytes(title);
		string @string = Encoding.UTF8.GetString(bytes);
		if (PdfALevel == PdfConformanceLevel.Pdf_A1B)
		{
			if (!string.IsNullOrEmpty(@string))
			{
				document.DocumentInformation.Title = @string;
			}
		}
		else
		{
			document.DocumentInformation.Title = @string;
		}
		if (documentInformation != null && documentInformation.Dictionary != null && documentInformation.Dictionary.ContainsKey("ModDate"))
		{
			documentInformation.Dictionary.Remove("ModDate");
		}
		documentInformation.ModificationDate = DateTime.Now;
		documentInformation.ResetXmp();
		XmpMetadata xmpMetadata = documentInformation.XmpMetadata;
		xmpMetadata.NamespaceManager.AddNamespace("pdfaid", "http://www.aiim.org/pdfa/ns/id/");
		XNamespace xNamespace = xmpMetadata.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		XNamespace xNamespace2 = xmpMetadata.AddNamespace("pdfaid", "http://www.aiim.org/pdfa/ns/id/");
		XElement xElement = xmpMetadata.CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement.SetAttributeValue(xNamespace + "about", "");
		XElement xElement2 = xmpMetadata.CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		xElement2.SetAttributeValue(xNamespace + "about", "");
		xmpMetadata.CreateAttribute("xmlns", "pdfaid", "http://purl.org/dc/elements/1.1/", "http://www.aiim.org/pdfa/ns/id/");
		xmpMetadata.CreateElement("pdfaid", "part", "");
		xmpMetadata.CreateElement("pdfaid", "conformance", "http://purl.org/dc/elements/1.1/");
		xmpMetadata.CreateElement("pdfaid", "rev", "");
		if (PdfALevel == PdfConformanceLevel.Pdf_A1B)
		{
			xElement.SetElementValue(xNamespace2 + "part", "1");
			xElement.SetElementValue(xNamespace2 + "conformance", "B");
		}
		else if (PdfALevel == PdfConformanceLevel.Pdf_A2B || PdfALevel == PdfConformanceLevel.Pdf_A2U)
		{
			xElement.SetElementValue(xNamespace2 + "part", "2");
			if (PdfALevel == PdfConformanceLevel.Pdf_A2B)
			{
				xElement.SetElementValue(xNamespace2 + "conformance", "B");
			}
			else if (PdfALevel == PdfConformanceLevel.Pdf_A2U)
			{
				xElement.SetElementValue(xNamespace2 + "conformance", "U");
			}
		}
		else if (PdfALevel == PdfConformanceLevel.Pdf_A3B || PdfALevel == PdfConformanceLevel.Pdf_A3U)
		{
			xElement.SetElementValue(xNamespace2 + "part", "3");
			if (PdfALevel == PdfConformanceLevel.Pdf_A3B)
			{
				xElement.SetElementValue(xNamespace2 + "conformance", "B");
			}
			else if (PdfALevel == PdfConformanceLevel.Pdf_A3U)
			{
				xElement.SetElementValue(xNamespace2 + "conformance", "U");
			}
		}
		else if (PdfALevel == PdfConformanceLevel.Pdf_A4 || PdfALevel == PdfConformanceLevel.Pdf_A4E || PdfALevel == PdfConformanceLevel.Pdf_A4F)
		{
			xElement2.SetElementValue(xNamespace2 + "part", "4");
			if (PdfALevel == PdfConformanceLevel.Pdf_A4E)
			{
				xElement2.SetElementValue(xNamespace2 + "conformance", "E");
			}
			else if (PdfALevel == PdfConformanceLevel.Pdf_A4F)
			{
				xElement2.SetElementValue(xNamespace2 + "conformance", "F");
			}
			xElement2.SetElementValue(xNamespace2 + "rev", "2020");
			document.FileStructure.Version = PdfVersion.Version2_0;
			xElement2.SetAttributeValue(XNamespace.Xmlns + "pdfaid", xNamespace2);
			xmpMetadata.Rdf.Add(xElement2);
		}
		if (PdfALevel != PdfConformanceLevel.Pdf_A4 && PdfALevel != PdfConformanceLevel.Pdf_A4E && PdfALevel != PdfConformanceLevel.Pdf_A4F)
		{
			xElement.SetAttributeValue(XNamespace.Xmlns + "pdfaid", xNamespace2);
			xmpMetadata.Rdf.Add(xElement);
		}
		document.Catalog["Metadata"] = new PdfReferenceHolder(xmpMetadata);
		document.Catalog.Modify();
		documentInformation.ConformanceEnabled = false;
		documentInformation.Dictionary.Modify();
		EnsureImageMetadataInfo(document);
	}

	private void EnsureImageMetadataInfo(PdfLoadedDocument document)
	{
		for (int i = 0; i < document.Pages.Count; i++)
		{
			PdfPageBase pdfPageBase = document.Pages[i];
			PdfDictionary dictionaryFromRefernceHolder = GetDictionaryFromRefernceHolder(pdfPageBase.Dictionary["Resources"]);
			if (dictionaryFromRefernceHolder == null)
			{
				continue;
			}
			PdfDictionary dictionaryFromRefernceHolder2 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder["XObject"]);
			if (dictionaryFromRefernceHolder2 == null)
			{
				continue;
			}
			foreach (IPdfPrimitive value in dictionaryFromRefernceHolder2.Values)
			{
				PdfDictionary dictionaryFromRefernceHolder3 = GetDictionaryFromRefernceHolder(value);
				if (dictionaryFromRefernceHolder3 == null || !dictionaryFromRefernceHolder3.ContainsKey("Subtype") || !((dictionaryFromRefernceHolder3["Subtype"] as PdfName).Value == "Image") || !dictionaryFromRefernceHolder3.ContainsKey("Metadata"))
				{
					continue;
				}
				if (dictionaryFromRefernceHolder3 is PdfStream)
				{
					XmpMetadata metadata = GetMetadata(dictionaryFromRefernceHolder3 as PdfStream);
					if (metadata != null)
					{
						foreach (XNode item in metadata.Rdf.Nodes())
						{
							EnsureImageSchemaData(item);
						}
						XDocument xmlData = metadata.XmlData;
						if (xmlData.FirstNode.NodeType != XmlNodeType.ProcessingInstruction || (xmlData.FirstNode.NodeType == XmlNodeType.ProcessingInstruction && (xmlData.FirstNode as XProcessingInstruction).Target != "xpacket"))
						{
							XProcessingInstruction content = new XProcessingInstruction("xpacket", "begin=\"\ufeff\" id=\"W5M0MpCehiHzreSzNTczkc9d\"");
							xmlData.AddFirst(content);
							XProcessingInstruction content2 = new XProcessingInstruction("xpacket", "end=\"r\"");
							xmlData.Add(content2);
						}
						metadata = new XmpMetadata(xmlData);
						PdfReferenceHolder primitive = new PdfReferenceHolder(metadata);
						dictionaryFromRefernceHolder3.SetProperty("Metadata", primitive);
						if (dictionaryFromRefernceHolder3.ContainsKey("SMask"))
						{
							PdfDictionary dictionaryFromRefernceHolder4 = GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder3["SMask"]);
							if (dictionaryFromRefernceHolder4.ContainsKey("Metadata"))
							{
								int index = document.PdfObjects.IndexOf(GetDictionaryFromRefernceHolder(dictionaryFromRefernceHolder4["Metadata"]));
								document.PdfObjects.Remove(index);
								dictionaryFromRefernceHolder4.SetProperty("Metadata", primitive);
							}
						}
					}
					else
					{
						dictionaryFromRefernceHolder3.Remove("Metadata");
					}
				}
				else
				{
					dictionaryFromRefernceHolder3.Remove("Metadata");
				}
			}
		}
	}

	private XmpMetadata GetMetadata(PdfStream imageStream)
	{
		if (imageStream.ContainsKey("Metadata"))
		{
			IPdfPrimitive pdfPrimitive = imageStream["Metadata"];
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				PdfStream stream = pdfReferenceHolder.Object as PdfStream;
				return TryGetMetadata(stream);
			}
			return TryGetMetadata(pdfPrimitive as PdfStream);
		}
		return null;
	}

	private XmpMetadata TryGetMetadata(PdfStream stream)
	{
		if (stream != null)
		{
			byte[] decompressedData = stream.GetDecompressedData();
			if (decompressedData.Length != 0)
			{
				return new ImageMetadataParser(new MemoryStream(decompressedData)).TryGetMetadata();
			}
		}
		return null;
	}

	private void EnsureImageSchemaData(XNode childNode)
	{
		if (childNode == null)
		{
			return;
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>
		{
			{
				"http://ns.adobe.com/xap/1.0/",
				new List<string> { "CreateDate" }
			},
			{
				"http://ns.adobe.com/photoshop/1.0/",
				new List<string> { "LegacyIPTCDigest", "DocumentAncestors", "ColorMode" }
			},
			{
				"http://ns.adobe.com/xap/1.0/mm/",
				new List<string> { "OriginalDocumentID" }
			},
			{
				"http://ns.adobe.com/xap/1.0/sType/ResourceRef#",
				new List<string> { "originalDocumentID" }
			},
			{
				"http://ns.adobe.com/exif/1.0/",
				new List<string> { "DateTimeOriginal" }
			},
			{
				"http://ns.adobe.com/exif/1.0/aux/",
				new List<string> { "LensInfo", "ImageNumber", "FlashCompensation", "OwnerName", "Firmware" }
			},
			{
				"http://ns.adobe.com/camera-raw-settings/1.0/",
				new List<string>
				{
					"FillLight", "Vibrance", "HighlightRecovery", "HueAdjustmentRed", "HueAdjustmentOrange", "HueAdjustmentYellow", "HueAdjustmentGreen", "HueAdjustmentAqua", "HueAdjustmentBlue", "HueAdjustmentPurple",
					"HueAdjustmentMagenta", "SaturationAdjustmentRed", "SaturationAdjustmentOrange", "SaturationAdjustmentYellow", "SaturationAdjustmentGreen", "SaturationAdjustmentAqua", "SaturationAdjustmentBlue", "SaturationAdjustmentPurple", "SaturationAdjustmentMagenta", "LuminanceAdjustmentRed",
					"LuminanceAdjustmentOrange", "LuminanceAdjustmentYellow", "LuminanceAdjustmentGreen", "LuminanceAdjustmentAqua", "LuminanceAdjustmentBlue", "LuminanceAdjustmentPurple", "LuminanceAdjustmentMagenta", "SplitToningShadowHue", "SplitToningShadowSaturation", "SplitToningHighlightHue",
					"SplitToningHighlightSaturation", "SplitToningBalance", "ParametricShadows", "ParametricDarks", "ParametricLights", "ParametricHighlights", "ParametricShadowSplit", "ParametricMidtoneSplit", "ParametricHighlightSplit", "ConvertToGrayscale",
					"AutoGrayscaleWeights", "AlreadyApplied"
				}
			},
			{
				"http://ns.adobe.com/xap/1.0/sType/ResourceEvent#",
				new List<string> { "changed" }
			}
		};
		if (childNode.NodeType == XmlNodeType.Element)
		{
			XElement xElement = childNode as XElement;
			XAttribute xAttribute = xElement.LastAttribute;
			if (xAttribute != null)
			{
				while (xAttribute != null)
				{
					XAttribute? previousAttribute = xAttribute.PreviousAttribute;
					if (dictionary.ContainsKey(xAttribute.Name.NamespaceName) && dictionary[xAttribute.Name.NamespaceName].Contains(xAttribute.Name.LocalName))
					{
						xAttribute.Remove();
					}
					xAttribute = previousAttribute;
				}
			}
			if (xElement.Nodes() != null)
			{
				XNode xNode = xElement.LastNode;
				while (xNode != null)
				{
					XNode? previousNode = xNode.PreviousNode;
					bool flag = true;
					if (xNode.NodeType == XmlNodeType.Element && dictionary.ContainsKey((xNode as XElement).Name.NamespaceName) && dictionary[(xNode as XElement).Name.NamespaceName].Contains((xNode as XElement).Name.LocalName))
					{
						xNode.Remove();
						flag = false;
					}
					xNode = previousNode;
					if (flag)
					{
						EnsureImageSchemaData(xNode);
					}
				}
			}
		}
		dictionary.Clear();
	}

	private PdfDictionary CreateNewAppearanceDictionary(bool isScript, bool isButtonField)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfStream pdfStream = null;
		if (!isButtonField)
		{
			PdfStream pdfStream2 = new PdfStream();
			byte b = 113;
			byte b2 = 81;
			byte b3 = 32;
			pdfStream2.Data = new byte[3] { b, b3, b2 };
			pdfStream = pdfStream2;
			pdfStream["FormType"] = new PdfNumber(1);
		}
		else
		{
			pdfStream = new PdfStream(new PdfDictionary(), Encoding.UTF8.GetBytes(" "));
			pdfStream["Filter"] = new PdfName("FlateDecode");
		}
		pdfStream["Type"] = new PdfName("XObject");
		pdfStream["Resources"] = new PdfReferenceHolder(new PdfNull());
		double[] array = new double[4] { 0.0, 0.0, 139.5, 13.1151 };
		pdfStream["BBox"] = new PdfArray(array);
		pdfStream["Subtype"] = new PdfName("Form");
		if (!isScript && !isButtonField)
		{
			pdfDictionary["N"] = new PdfReferenceHolder(pdfStream);
		}
		else
		{
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			pdfDictionary2["Ja"] = new PdfReferenceHolder(pdfStream);
			pdfDictionary["N"] = pdfDictionary2;
		}
		return pdfDictionary;
	}

	private void MapWidthTable(PdfDictionary fontDescriptor, PdfDictionary descendantFont)
	{
		if (fontDescriptor == null || !fontDescriptor.ContainsKey("FontFile2"))
		{
			return;
		}
		PdfReferenceHolder pdfReferenceHolder = fontDescriptor["FontFile2"] as PdfReferenceHolder;
		if (!(pdfReferenceHolder != null) || !(pdfReferenceHolder.Object is PdfStream pdfStream) || pdfStream.Data.Length == 0)
		{
			return;
		}
		pdfStream.Decompress();
		BinaryReader binaryReader = null;
		binaryReader = ((!pdfStream.InternalStream.CanSeek) ? new BinaryReader(new MemoryStream(pdfStream.Data), TtfReader.Encoding) : new BinaryReader(pdfStream.InternalStream, TtfReader.Encoding));
		TtfReader ttfReader = new TtfReader(binaryReader);
		ttfReader.CreateInternals();
		List<TtfGlyphInfo> list = new List<TtfGlyphInfo>();
		foreach (TtfGlyphInfo value in ttfReader.CompleteGlyph.Values)
		{
			list.Add(value);
		}
		list.Sort();
		PdfArray pdfArray = new PdfArray();
		PdfArray pdfArray2 = new PdfArray();
		List<int> list2 = new List<int>();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			TtfGlyphInfo ttfGlyphInfo = list[i];
			int index = ttfGlyphInfo.Index;
			if (!list2.Contains(ttfGlyphInfo.Index))
			{
				pdfArray.Add(new PdfNumber(ttfGlyphInfo.Width));
				pdfArray2.Add(new PdfNumber(index));
				pdfArray2.Add(pdfArray);
				list2.Add(index);
				pdfArray = new PdfArray();
				if (pdfArray2.Count >= 8190)
				{
					break;
				}
			}
		}
		list2.Clear();
		descendantFont["W"] = pdfArray2;
	}

	private void GetCIDByte2Unicode()
	{
		int num = 256;
		m_cidByte2Unicode = new Dictionary<int, char>();
		for (int i = 0; i < num; i++)
		{
			char value = (char)m_charCodeTable[i];
			m_cidByte2Unicode[i] = value;
		}
	}
}
