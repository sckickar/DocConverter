using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.CompoundFile.DocIO.Net;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;
using DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;
using DocGen.DocIO.ReaderWriter.Security;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordWriter : WordWriterBase, IWordWriter, IWordWriterBase, IDisposable
{
	private bool m_bHeaderWritten;

	private int m_commentID = -1;

	private bool m_bLastParagrapfEnd;

	private SectionProperties m_secProperties;

	private IWordSubdocumentWriter m_lastWriter;

	private BuiltinDocumentProperties m_builtinProp = new BuiltinDocumentProperties();

	private CustomDocumentProperties m_customProp = new CustomDocumentProperties();

	private bool m_isTemplate;

	private bool m_bIsWriteProtected;

	private bool m_bHasPicture;

	public SectionProperties SectionProperties => m_secProperties;

	public BuiltinDocumentProperties BuiltinDocumentProperties
	{
		get
		{
			return m_builtinProp;
		}
		set
		{
			m_builtinProp = value;
		}
	}

	public CustomDocumentProperties CustomDocumentProperties
	{
		get
		{
			return m_customProp;
		}
		set
		{
			m_customProp = value;
		}
	}

	public DOPDescriptor DOP
	{
		get
		{
			return m_docInfo.TablesData.DOP;
		}
		set
		{
			m_docInfo.TablesData.DOP = value;
		}
	}

	public GrammarSpelling GrammarSpellingData
	{
		get
		{
			return m_docInfo.TablesData.GrammarSpellingData;
		}
		set
		{
			m_docInfo.TablesData.GrammarSpellingData = value;
		}
	}

	public MemoryStream MacrosStream
	{
		get
		{
			return m_streamsManager.MacrosStream;
		}
		set
		{
			m_streamsManager.MacrosStream = value;
		}
	}

	public MemoryStream ObjectPoolStream
	{
		get
		{
			return m_streamsManager.ObjectPoolStream;
		}
		set
		{
			m_streamsManager.ObjectPoolStream = value;
		}
	}

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

	public string StandardAsciiFont
	{
		get
		{
			return m_docInfo.TablesData.StandardAsciiFont;
		}
		set
		{
			m_docInfo.TablesData.StandardAsciiFont = value;
		}
	}

	public string StandardFarEastFont
	{
		get
		{
			return m_docInfo.TablesData.StandardFarEastFont;
		}
		set
		{
			m_docInfo.TablesData.StandardFarEastFont = value;
		}
	}

	public string StandardBidiFont
	{
		get
		{
			return m_docInfo.TablesData.StandardBidiFont;
		}
		set
		{
			m_docInfo.TablesData.StandardBidiFont = value;
		}
	}

	public string StandardNonFarEastFont
	{
		get
		{
			return m_docInfo.TablesData.StandardNonFarEastFont;
		}
		set
		{
			m_docInfo.TablesData.StandardNonFarEastFont = value;
		}
	}

	internal bool IsTemplate
	{
		get
		{
			return m_isTemplate;
		}
		set
		{
			m_isTemplate = value;
		}
	}

	public byte[] AssociatedStrings
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

	public byte[] SttbfRMark
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

	internal bool WriteProtected
	{
		get
		{
			return m_bIsWriteProtected;
		}
		set
		{
			m_bIsWriteProtected = value;
		}
	}

	internal bool HasPicture
	{
		get
		{
			return m_bHasPicture;
		}
		set
		{
			m_bHasPicture = value;
		}
	}

	public WordWriter(Stream stream)
	{
		m_streamsManager = new StreamsManager(stream, createNewStorage: true);
		InitClass();
	}

	public void WriteDocumentHeader()
	{
		m_bHeaderWritten = true;
		AddSepxProperties();
	}

	public IWordSubdocumentWriter GetSubdocumentWriter(WordSubdocument subDocumentType)
	{
		if (!m_bHeaderWritten)
		{
			throw new InvalidOperationException("Call WriteDocumentHeader before this method");
		}
		if (!m_bLastParagrapfEnd)
		{
			WriteMarker(WordChunkType.ParagraphEnd);
			m_bLastParagrapfEnd = true;
		}
		return subDocumentType switch
		{
			WordSubdocument.Footnote => m_lastWriter = new WordFootnoteWriter(this), 
			WordSubdocument.HeaderFooter => m_lastWriter = new WordHeaderFooterWriter(this), 
			WordSubdocument.Annotation => m_lastWriter = new WordAnnotationWriter(this), 
			WordSubdocument.Endnote => m_lastWriter = new WordEndnoteWriter(this), 
			WordSubdocument.TextBox => m_lastWriter = new WordTextBoxWriter(this), 
			WordSubdocument.HeaderTextBox => m_lastWriter = new WordHFTextBoxWriter(this), 
			_ => null, 
		};
	}

	public override void WriteChunk(string textChunk)
	{
		if (!m_bHeaderWritten)
		{
			throw new InvalidOperationException("Call WriteDocumentHeader before this method");
		}
		base.WriteChunk(textChunk);
	}

	public override void WriteSafeChunk(string textChunk)
	{
		if (!m_bHeaderWritten)
		{
			throw new InvalidOperationException("Call WriteDocumentHeader before this method");
		}
		base.WriteSafeChunk(textChunk);
	}

	public override void WriteMarker(WordChunkType chunkType)
	{
		if (!m_bHeaderWritten)
		{
			throw new InvalidOperationException("Call WriteDocumentHeader before this method");
		}
		base.WriteMarker(chunkType);
		switch (chunkType)
		{
		case WordChunkType.SectionEnd:
			WriteChar('\f');
			AddSepxProperties();
			break;
		case WordChunkType.ColumnBreak:
			WriteChar('\u000e');
			break;
		}
	}

	public void WriteDocumentEnd(string password, string author, ushort fibVersion, Dictionary<string, Storage> oleObjectCollection)
	{
		m_docInfo.Fib.FDot = m_isTemplate;
		m_docInfo.Fib.FReadOnlyRecommended = WriteProtected;
		m_docInfo.Fib.FHasPic = HasPicture;
		CompleteMainStream();
		if (!string.IsNullOrEmpty(password))
		{
			byte[] array = new byte[52];
			m_streamsManager.TableStream.Position = 0L;
			m_streamsManager.TableStream.Write(array, 0, array.Length);
			m_docInfo.Fib.FEncrypted = true;
		}
		WriteTables(author);
		m_docInfo.Fib.Write(m_streamsManager.MainStream, fibVersion);
		WriteSummary();
		if (!string.IsNullOrEmpty(password))
		{
			WordDecryptor wordDecryptor = new WordDecryptor(m_streamsManager.TableStream, m_streamsManager.MainStream, m_streamsManager.DataStream, m_docInfo.Fib);
			wordDecryptor.Encrypt(password);
			m_streamsManager.UpdateStreams(wordDecryptor.MainStream, wordDecryptor.TableStream, wordDecryptor.DataStream);
			m_docInfo.Fib.WriteAfterEncryption(m_streamsManager.MainStream);
		}
		m_streamsManager.SaveStg(oleObjectCollection);
	}

	public void InsertComment(WCommentFormat format)
	{
		m_commentID++;
		AnnotationDescriptor annotationDescriptor = new AnnotationDescriptor();
		int num = m_docInfo.TablesData.Annotations.AddGXAO(format.User);
		int textPos = GetTextPos();
		annotationDescriptor.UserInitials = format.UserInitials;
		annotationDescriptor.IndexToGrpOwner = (short)num;
		annotationDescriptor.TagBkmk = m_commentID;
		m_docInfo.TablesData.Annotations.AddDescriptor(annotationDescriptor, textPos, textPos - format.BookmarkStartOffset, textPos + format.BookmarkEndOffset);
		WriteMarker(WordChunkType.Annotation);
	}

	public void InsertFootnote(WFootnote footnote)
	{
		int textPos = GetTextPos();
		if (footnote.FootnoteType == FootnoteType.Footnote)
		{
			m_docInfo.TablesData.Footnotes.AddReferense(textPos, footnote.IsAutoNumbered);
		}
		else
		{
			m_docInfo.TablesData.Endnotes.AddReferense(textPos, footnote.IsAutoNumbered);
		}
		if (footnote.IsAutoNumbered)
		{
			WriteMarker(WordChunkType.Footnote);
		}
		else if (footnote.CustomMarkerIsSymbol)
		{
			int num = base.StyleSheet.FontNameToIndex(footnote.SymbolFontName);
			if (num >= 0)
			{
				base.CHPX.PropertyModifiers.SetUShortValue(19023, (ushort)num);
			}
			else
			{
				base.CHPX.PropertyModifiers.SetUShortValue(19023, (ushort)base.StyleSheet.FontNamesList.Count);
				base.StyleSheet.UpdateFontName(footnote.SymbolFontName);
			}
			SymbolDescriptor symbolDescriptor = new SymbolDescriptor();
			symbolDescriptor.CharCode = footnote.SymbolCode;
			symbolDescriptor.FontCode = (short)base.StyleSheet.FontNameToIndex(footnote.SymbolFontName);
			base.CHPX.PropertyModifiers.SetByteArrayValue(27145, symbolDescriptor.Save());
			WriteMarker(WordChunkType.Symbol);
		}
		else
		{
			WriteString(footnote.m_strCustomMarker);
		}
	}

	public void InsertPageBreak()
	{
		MemoryStream mainStream = m_streamsManager.MainStream;
		byte[] bytes = m_docInfo.Fib.Encoding.GetBytes(SpecialCharacters.PageBreakStr);
		mainStream.Write(bytes, 0, bytes.Length);
		AddChpxProperties(isParaBreak: false);
		IncreaseCcp(1);
	}

	private void CompleteMainStream()
	{
		if (m_lastWriter != null)
		{
			m_lastWriter.WriteMarker(WordChunkType.ParagraphEnd);
		}
		if (!m_bLastParagrapfEnd)
		{
			WriteMarker(WordChunkType.ParagraphEnd);
			m_bLastParagrapfEnd = true;
		}
		m_docInfo.Fib.UpdateFcMac();
		while (m_streamsManager.MainStream.Position < m_docInfo.Fib.BaseReserved6)
		{
			m_streamsManager.MainWriter.Write((byte)0);
		}
		uint pos = (uint)(m_iStartText + m_docInfo.Fib.CcpText * m_docInfo.Fib.EncodingCharSize);
		m_docInfo.FkpData.CloneAndAddLastPapx(pos);
		m_docInfo.FkpData.CloneAndAddLastChpx(pos);
		m_docInfo.FkpData.Write(m_streamsManager.MainStream);
		if (base.Escher != null)
		{
			base.Escher.WriteContainersData(m_streamsManager.MainStream);
		}
		m_docInfo.Fib.CbMac = (int)m_streamsManager.MainStream.Position;
	}

	private void WriteZeroBlock(int size)
	{
		byte[] array = new byte[size];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 0;
		}
		m_streamsManager.MainWriter.Write(array, 0, array.Length);
	}

	private void WriteTables(string author)
	{
		m_docInfo.TablesData.AddStyleSheetTable(base.StyleSheet);
		m_docInfo.TablesData.Write(m_streamsManager.TableStream, m_lastWriter != null);
	}

	private void WriteSummary()
	{
		Guid guid = new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9");
		Guid guid2 = new Guid("D5CDD502-2E9C-101B-9397-08002B2CF9AE");
		DocumentPropertyCollection documentPropertyCollection = new DocumentPropertyCollection();
		PropertySection propertySection = new PropertySection(guid, -1);
		documentPropertyCollection.Sections.Add(propertySection);
		DocumentPropertyCollection documentPropertyCollection2 = new DocumentPropertyCollection();
		PropertySection propertySection2 = new PropertySection(guid2, -1);
		documentPropertyCollection2.Sections.Add(propertySection2);
		PropertySection propertySection3 = null;
		if (m_customProp.CustomHash.Values.Count > 0)
		{
			propertySection3 = new PropertySection(new Guid("D5CDD505-2E9C-101B-9397-08002B2CF9AE"), -1);
			documentPropertyCollection2.Sections.Add(propertySection3);
		}
		WriteProps(propertySection, m_builtinProp.SummaryHash.Values);
		WriteProps(propertySection2, m_builtinProp.DocumentHash.Values);
		if (propertySection3 != null)
		{
			WriteProps(propertySection3, m_customProp.CustomHash.Values);
		}
		documentPropertyCollection.Serialize(base.StreamsManager.SummaryInfoStream);
		documentPropertyCollection2.Serialize(base.StreamsManager.DocumentSummaryInfoStream);
	}

	private void WriteProps(PropertySection section, ICollection values)
	{
		int num = 2;
		foreach (DocumentProperty value in values)
		{
			PropertyData item = ConvertToPropertyData(value, num);
			section.Properties.Add(item);
			num++;
		}
	}

	private PropertyData ConvertToPropertyData(DocumentProperty property, int iPropertyId)
	{
		PropertyData propertyData = new PropertyData();
		property.FillPropVariant(propertyData, iPropertyId);
		if (property.InternalName != null)
		{
			propertyData.Name = property.InternalName;
		}
		return propertyData;
	}

	private void AddSepxProperties()
	{
		int textPos = GetTextPos();
		SectionPropertyException sepx = SectionProperties.CloneSepx();
		m_docInfo.FkpData.AddSepxProperties(textPos, sepx);
		m_secProperties = new SectionProperties();
	}

	internal override void Close()
	{
		base.Close();
		m_secProperties = null;
		m_builtinProp = null;
		m_customProp = null;
	}

	protected override void InitClass()
	{
		base.InitClass();
		m_type = WordSubdocument.Main;
		m_secProperties = new SectionProperties();
		m_docInfo = new DocInfo(m_streamsManager);
		m_streamsManager.MainStream.Seek(m_docInfo.Fib.BaseReserved5, SeekOrigin.Begin);
		m_iStartText = (int)m_streamsManager.MainStream.Position;
	}

	protected override void IncreaseCcp(int dataLength)
	{
		m_docInfo.Fib.CcpText += dataLength;
	}

	public void Dispose()
	{
	}
}
