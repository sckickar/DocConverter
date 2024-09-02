using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.CompoundFile.DocIO;
using DocGen.CompoundFile.DocIO.Net;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIO.ReaderWriter.Security;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordReader : WordReaderBase, IWordReader, IWordReaderBase, IDisposable
{
	private const int DEF_PID_CODEPAGE = 1;

	protected bool m_bDisposed;

	protected bool m_bDestroyStream;

	private SectionProperties m_secProperties;

	private bool m_bHeaderRead;

	private IWordSubdocumentReader m_lastReader;

	private BuiltinDocumentProperties m_builtinProp = new BuiltinDocumentProperties();

	private CustomDocumentProperties m_custProp = new CustomDocumentProperties();

	private Encoding m_strEncoding;

	private int m_customFnSplittedTextLength = -1;

	private Dictionary<int, string> m_sttbFRAuthorNames;

	public DOPDescriptor DOP => m_docInfo.TablesData.DOP;

	public MainStatePositions StatePositions => (MainStatePositions)m_statePositions;

	public int SectionNumber => StatePositions.SectionIndex + 1;

	public SectionProperties SectionProperties => m_secProperties;

	public BuiltinDocumentProperties BuiltinDocumentProperties => m_builtinProp;

	public CustomDocumentProperties CustomDocumentProperties => m_custProp;

	public MemoryStream MacrosStream => m_streamsManager.MacrosStream;

	public byte[] MacroCommands
	{
		get
		{
			return m_docInfo.TablesData.MacroCommands;
		}
		set
		{
			m_docInfo.TablesData.MacroCommands = value;
		}
	}

	public byte[] Variables
	{
		get
		{
			return m_docInfo.TablesData.Variables;
		}
		set
		{
			m_docInfo.TablesData.Variables = value;
		}
	}

	public GrammarSpelling GrammarSpellingData => m_docInfo.TablesData.GrammarSpellingData;

	public bool IsFootnote
	{
		get
		{
			if (base.TextChunk.Length == 0)
			{
				return false;
			}
			int length = base.TextChunk.Length;
			int num = base.CurrentTextPosition - length;
			int num2 = ((length >= 10) ? 10 : length);
			for (int i = 0; i < num2; i++)
			{
				if (m_docInfo.TablesData.Footnotes.HasReference(num + i))
				{
					return true;
				}
				if (num2 >= 10 && i == num2 - 1)
				{
					int textLength = 0;
					if (m_docInfo.TablesData.Footnotes.HasReference(num, base.CurrentTextPosition, ref textLength))
					{
						m_customFnSplittedTextLength = textLength;
						return true;
					}
				}
			}
			return false;
		}
	}

	internal int CustomFnSplittedTextLength
	{
		get
		{
			return m_customFnSplittedTextLength;
		}
		set
		{
			m_customFnSplittedTextLength = value;
		}
	}

	public bool IsEndnote
	{
		get
		{
			if (base.TextChunk.Length == 0)
			{
				return false;
			}
			int length = base.TextChunk.Length;
			if (base.TextChunk.TrimStart(' ') != string.Empty)
			{
				length = base.TextChunk.TrimStart(' ').Length;
			}
			int num = base.CurrentTextPosition - length;
			int num2 = ((length >= 10) ? 10 : length);
			for (int i = 0; i < num2; i++)
			{
				if (m_docInfo.TablesData.Endnotes.HasReference(num + i))
				{
					return true;
				}
			}
			return false;
		}
	}

	public string StandardAsciiFont => m_docInfo.TablesData.StandardAsciiFont;

	public string StandardFarEastFont => m_docInfo.TablesData.StandardFarEastFont;

	public string StandardNonFarEastFont => m_docInfo.TablesData.StandardNonFarEastFont;

	public string StandardBidiFont => m_docInfo.TablesData.StandardBidiFont;

	public bool IsEncrypted => m_docInfo.Fib.FEncrypted;

	internal byte[] AssociatedStrings
	{
		get
		{
			return m_docInfo.TablesData.AsociatedStrings;
		}
		set
		{
			m_docInfo.TablesData.AsociatedStrings = value;
		}
	}

	internal byte[] SttbfRMark
	{
		get
		{
			return m_docInfo.TablesData.SttbfRMark;
		}
		set
		{
			m_docInfo.TablesData.SttbfRMark = value;
		}
	}

	public new Dictionary<int, string> SttbfRMarkAuthorNames
	{
		get
		{
			if (SttbfRMark != null && m_sttbFRAuthorNames == null)
			{
				m_sttbFRAuthorNames = GetSTTBFRNames(SttbfRMark);
			}
			return m_sttbFRAuthorNames;
		}
	}

	public event NeedPasswordEventHandler NeedPassword;

	public WordReader(Stream stream)
	{
		m_streamsManager = new StreamsManager(stream, createNewStorage: false);
		InitClass();
	}

	public IWordSubdocumentReader GetSubdocumentReader(WordSubdocument subDocumentType)
	{
		if (!m_bHeaderRead)
		{
			throw new InvalidOperationException("Call ReadDocumentHeader() before this method");
		}
		return subDocumentType switch
		{
			WordSubdocument.Footnote => m_lastReader = new WordFootnoteReader(this), 
			WordSubdocument.HeaderFooter => m_lastReader = new WordHeaderFooterReader(this), 
			WordSubdocument.Annotation => m_lastReader = new WordAnnotationReader(this), 
			WordSubdocument.Endnote => m_lastReader = new WordEndnoteReader(this), 
			WordSubdocument.TextBox => m_lastReader = new WordTextBoxReader(this), 
			WordSubdocument.HeaderTextBox => m_lastReader = new WordHFTextBoxReader(this), 
			_ => null, 
		};
	}

	public void ReadDocumentHeader(WordDocument doc)
	{
		if (m_bHeaderRead)
		{
			throw new InvalidOperationException("Method ReadDocumentHeader() already called!");
		}
		m_bHeaderRead = true;
		m_docInfo.Fib.Read(m_streamsManager.MainStream);
		if (m_docInfo.Fib.FComplex && !doc.Settings.SkipIncrementalSaveValidation)
		{
			throw new NotImplementedException("Complex format is not supported");
		}
		m_streamsManager.LoadTableStream(m_docInfo.Fib.FWhichTblStm ? "1Table" : "0Table");
		if (m_docInfo.Fib.FEncrypted)
		{
			if (this.NeedPassword == null)
			{
				throw new ArgumentException("Document is encrypted, password is needed to open the document");
			}
			string text = this.NeedPassword();
			WordDecryptor wordDecryptor = new WordDecryptor(m_streamsManager.TableStream, m_streamsManager.MainStream, m_streamsManager.DataStream, m_docInfo.Fib);
			if (!wordDecryptor.CheckPassword(text))
			{
				throw new Exception("Specified password \"" + text + "\" is incorrect!");
			}
			wordDecryptor.Decrypt();
			m_streamsManager.UpdateStreams(wordDecryptor.MainStream, wordDecryptor.TableStream, wordDecryptor.DataStream);
			m_docInfo.Fib.ReadAfterDecryption(m_streamsManager.MainStream);
		}
		doc.Password = null;
		m_docInfo.TablesData.Read(m_streamsManager.TableStream);
		UpdateBookmarks();
		UpdateStyleSheet();
		m_docInfo.FkpData.Read(m_streamsManager.MainStream);
		UpdateCharacterProperties();
		UpdateParagraphProperties();
		UpdateSectionProperties();
		ReadSummaryManaged();
		if (m_docInfo.Fib.FibRgFcLcb97LcbDggInfo != 0)
		{
			base.Escher = new EscherClass(m_streamsManager.TableStream, m_streamsManager.MainStream, m_docInfo.Fib.FibRgFcLcb97FcDggInfo, m_docInfo.Fib.FibRgFcLcb97LcbDggInfo, doc);
		}
		m_statePositions.InitStartEndPos();
		m_streamsManager.MainStream.Position = m_statePositions.StartText;
	}

	private void ReadSummaryManaged()
	{
		m_streamsManager.LoadSummaryInfoStream();
		if (m_streamsManager.SummaryInfoStream != null && m_streamsManager.SummaryInfoStream.Length > 0)
		{
			DocumentPropertyCollection properties = new DocumentPropertyCollection(m_streamsManager.SummaryInfoStream);
			ReadDocumentProperties(properties);
		}
		m_streamsManager.LoadDocumentSummaryInfoStream();
		if (m_streamsManager.DocumentSummaryInfoStream != null && m_streamsManager.DocumentSummaryInfoStream.Length > 0)
		{
			DocumentPropertyCollection properties2 = new DocumentPropertyCollection(m_streamsManager.DocumentSummaryInfoStream);
			ReadDocumentProperties(properties2);
		}
	}

	private void ReadDocumentProperties(DocumentPropertyCollection properties)
	{
		Guid guid = new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9");
		Guid guid2 = new Guid("D5CDD502-2E9C-101B-9397-08002B2CF9AE");
		Guid guid3 = new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE");
		List<PropertySection> sections = properties.Sections;
		int i = 0;
		for (int count = sections.Count; i < count; i++)
		{
			PropertySection propertySection = sections[i];
			if (propertySection.Id == guid)
			{
				ReadProperties(propertySection, m_builtinProp.SummaryHash, bSummary: true, bBuiltIn: true);
			}
			else if (propertySection.Id == guid2)
			{
				ReadProperties(propertySection, m_builtinProp.DocumentHash, bSummary: false, bBuiltIn: true);
			}
			else if (propertySection.Id == guid3)
			{
				ReadProperties(propertySection, m_custProp.CustomHash, bSummary: true, bBuiltIn: false);
			}
		}
	}

	private void ReadProperties(PropertySection section, IDictionary dicProperties, bool bSummary, bool bBuiltIn)
	{
		Dictionary<int, DocumentProperty> dictionary = null;
		if (!bBuiltIn)
		{
			dictionary = new Dictionary<int, DocumentProperty>();
		}
		List<PropertyData> properties = section.Properties;
		int i = 0;
		for (int count = properties.Count; i < count; i++)
		{
			PropertyData propertyData = properties[i];
			if (!bSummary && bBuiltIn && !propertyData.IsValidProperty())
			{
				continue;
			}
			if (propertyData.IsLinkToSource)
			{
				int parentId = propertyData.ParentId;
				dictionary[parentId].SetLinkSource(propertyData);
			}
			else
			{
				if (propertyData.Data is ClipboardData)
				{
					continue;
				}
				DocumentProperty documentProperty = new DocumentProperty(propertyData, bSummary);
				object key = (bBuiltIn ? ((object)(int)documentProperty.PropertyId) : documentProperty.Name);
				if (!bBuiltIn)
				{
					dictionary.Add(propertyData.Id, documentProperty);
				}
				if (documentProperty.Value != null)
				{
					if (!bSummary && bBuiltIn && !dicProperties.Contains(key))
					{
						dicProperties.Add(key, documentProperty);
					}
					else
					{
						dicProperties[key] = documentProperty;
					}
				}
			}
		}
	}

	public override WordChunkType ReadChunk()
	{
		if (!m_bHeaderRead)
		{
			throw new InvalidOperationException("Call ReadDocumentHeader() before this method");
		}
		return base.ReadChunk();
	}

	public void ReadDocumentEnd()
	{
		m_streamsManager.CloseStg();
	}

	public override FieldDescriptor GetFld()
	{
		int pos = CalcCP(StatePositions.StartText, 1);
		return m_docInfo.TablesData.Fields.FindFld(m_type, pos);
	}

	internal override void Close()
	{
		base.Close();
		m_secProperties = null;
		if (m_builtinProp != null)
		{
			m_builtinProp = null;
		}
		if (m_custProp != null)
		{
			m_custProp = null;
		}
		m_strEncoding = null;
	}

	public override FileShapeAddress GetFSPA()
	{
		int cP = CalcCP(StatePositions.StartText, 1);
		if (m_docInfo.TablesData.FileArtObjects == null)
		{
			return null;
		}
		return m_docInfo.TablesData.FileArtObjects.FindFileShape(m_type, cP);
	}

	public override void FreezeStreamPos()
	{
		if (m_lastReader != null)
		{
			(m_lastReader as WordReaderBase).FreezeStreamPos();
		}
		base.FreezeStreamPos();
	}

	public override void UnfreezeStreamPos()
	{
		if (m_lastReader != null)
		{
			(m_lastReader as WordReaderBase).FreezeStreamPos();
		}
		base.UnfreezeStreamPos();
	}

	protected override void InitClass()
	{
		m_secProperties = new SectionProperties();
		base.InitClass();
		m_docInfo = new DocInfo(m_streamsManager);
		m_statePositions = new MainStatePositions(m_docInfo.FkpData);
		m_type = WordSubdocument.Main;
		m_startTextPos = 0;
		m_endTextPos = 0;
	}

	protected override void UpdateEndPositions(long iEndPos)
	{
		base.UpdateEndPositions(iEndPos);
		if (StatePositions.UpdateSepxEndPos(iEndPos))
		{
			UpdateSectionProperties();
		}
	}

	private void UpdateSectionProperties()
	{
		m_secProperties = new SectionProperties(StatePositions.CurrentSepx);
	}

	private void UpdateStyleSheet()
	{
		WPTablesData tablesData = m_docInfo.TablesData;
		StyleSheetInfoRecord styleSheetInfo = tablesData.StyleSheetInfo;
		FontFamilyNameRecord[] fontFamilyNameRecords = tablesData.FFNStringTable.FontFamilyNameRecords;
		string[] array = new string[fontFamilyNameRecords.Length];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = fontFamilyNameRecords[i].FontName;
			base.StyleSheet.UpdateFontSubstitutionTable(fontFamilyNameRecords[i]);
		}
		base.StyleSheet.ClearFontNames();
		base.StyleSheet.UpdateFontNames(array);
		StyleDefinitionRecord[] styleDefinitions = tablesData.StyleDefinitions;
		int j = 0;
		for (int num2 = styleDefinitions.Length; j < 15 && j < num2; j++)
		{
			StyleDefinitionRecord styleDefinitionRecord = styleDefinitions[j];
			if (styleDefinitionRecord.CharacterProperty != null && styleDefinitionRecord.CharacterProperty.FontAscii == ushort.MaxValue && styleDefinitionRecord.BaseStyle == 4095)
			{
				styleDefinitionRecord.CharacterProperty.FontAscii = styleSheetInfo.StandardChpStsh[0];
				styleDefinitionRecord.CharacterProperty.FontFarEast = styleSheetInfo.StandardChpStsh[1];
				styleDefinitionRecord.CharacterProperty.FontNonFarEast = styleSheetInfo.StandardChpStsh[2];
			}
			if (styleDefinitionRecord.StyleName != null)
			{
				WordStyle wordStyle = base.StyleSheet.UpdateStyle(j, styleDefinitionRecord.StyleName);
				wordStyle.ID = styleDefinitionRecord.StyleId;
				wordStyle.BaseStyleIndex = styleDefinitionRecord.BaseStyle;
				wordStyle.NextStyleIndex = styleDefinitionRecord.NextStyleId;
				wordStyle.LinkStyleIndex = styleDefinitionRecord.LinkStyleId;
				wordStyle.IsCharacterStyle = styleDefinitionRecord.TypeCode == WordStyleType.CharacterStyle;
				wordStyle.IsPrimary = styleDefinitionRecord.IsQFormat;
				wordStyle.IsSemiHidden = styleDefinitionRecord.IsSemiHidden;
				wordStyle.UnhideWhenUsed = styleDefinitionRecord.UnhideWhenUsed;
				wordStyle.TypeCode = styleDefinitionRecord.TypeCode;
				if (styleDefinitionRecord.UpxNumber == 3 && styleDefinitionRecord.Tapx != null)
				{
					wordStyle.TableStyleData = new byte[styleDefinitionRecord.Tapx.Length];
					Buffer.BlockCopy(styleDefinitionRecord.Tapx, 0, wordStyle.TableStyleData, 0, styleDefinitionRecord.Tapx.Length);
				}
				UpdateStyleProperties(wordStyle, styleDefinitionRecord);
			}
		}
		int k = base.StyleSheet.StylesCount;
		for (int num3 = styleDefinitions.Length; k < num3; k++)
		{
			StyleDefinitionRecord styleDefinitionRecord2 = styleDefinitions[k];
			if (styleDefinitionRecord2.StyleName == null)
			{
				base.StyleSheet.AddEmptyStyle();
				continue;
			}
			WordStyle wordStyle2 = base.StyleSheet.CreateStyle(styleDefinitionRecord2.StyleName, characterStyle: false);
			wordStyle2.ID = styleDefinitionRecord2.StyleId;
			wordStyle2.BaseStyleIndex = styleDefinitionRecord2.BaseStyle;
			wordStyle2.NextStyleIndex = styleDefinitionRecord2.NextStyleId;
			wordStyle2.LinkStyleIndex = styleDefinitionRecord2.LinkStyleId;
			wordStyle2.IsPrimary = styleDefinitionRecord2.IsQFormat;
			wordStyle2.IsSemiHidden = styleDefinitionRecord2.IsSemiHidden;
			wordStyle2.UnhideWhenUsed = styleDefinitionRecord2.UnhideWhenUsed;
			wordStyle2.IsCharacterStyle = styleDefinitionRecord2.TypeCode == WordStyleType.CharacterStyle;
			wordStyle2.TypeCode = styleDefinitionRecord2.TypeCode;
			if (styleDefinitionRecord2.UpxNumber == 3 && styleDefinitionRecord2.Tapx != null)
			{
				wordStyle2.TableStyleData = new byte[styleDefinitionRecord2.Tapx.Length];
				Buffer.BlockCopy(styleDefinitionRecord2.Tapx, 0, wordStyle2.TableStyleData, 0, styleDefinitionRecord2.Tapx.Length);
			}
			UpdateStyleProperties(wordStyle2, styleDefinitionRecord2);
		}
		tablesData.StyleDefinitions = null;
		tablesData.StyleSheetInfo = null;
	}

	private void UpdateStyleProperties(WordStyle style, StyleDefinitionRecord record)
	{
		style.CHPX = record.CharacterProperty;
		style.PAPX = record.ParagraphProperty;
	}

	private MemoryStream CopyStream(Stream entryStream)
	{
		try
		{
			byte[] array = new byte[entryStream.Length];
			long position = entryStream.Position;
			entryStream.Position = 0L;
			entryStream.Read(array, 0, array.Length);
			entryStream.Position = position;
			return new MemoryStream(array);
		}
		catch
		{
			throw new ArgumentException("Cannot read data from stream ");
		}
	}

	public void Dispose()
	{
	}
}
