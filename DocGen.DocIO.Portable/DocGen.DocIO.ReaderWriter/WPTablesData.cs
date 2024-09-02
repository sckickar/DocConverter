using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WPTablesData
{
	internal const string DEF_TABLESTREAM_NAME = "1Table";

	private const string EXC_NOTREAD_TABLE_MESS = "You can not get \"table\" object without calling the Read() method!";

	internal EscherClass m_escher;

	private Fib m_fib;

	private SectionExceptionsTable m_sectionTable;

	internal PieceTable m_pieceTable;

	private FontFamilyNameStringTable m_ffnStringTable;

	private BookmarkNameStringTable m_bkmkStringTable;

	private BookmarkDescriptor m_bkmkDescriptor;

	private BinaryTable m_binTableCHPX;

	private BinaryTable m_binTablePAPX;

	private ListInfo m_listInfo;

	private CharPosTableRecord m_charPosTableHF;

	private StyleSheetInfoRecord m_styleSheetInfo = new StyleSheetInfoRecord(18);

	private StyleDefinitionRecord[] m_arrStyleDefinitions;

	private List<uint> m_pieceTablePositions = new List<uint>();

	internal List<Encoding> m_pieceTableEncodings = new List<Encoding>();

	private DOPDescriptor m_dopDescriptor = new DOPDescriptor();

	private ArtObjectsRW m_artObjects;

	private AnnotationsRW m_anotations;

	private FootnotesRW m_footnotes;

	private EndnotesRW m_endnotes;

	private Fields m_fields;

	private byte[] m_macroCommands;

	private byte[] m_variables;

	private byte[] m_assocStrings;

	private byte[] m_sttbfRMark;

	private SinglePropertyModifierArray m_clxModifiers;

	private List<int> m_secPositions = new List<int>();

	private List<int> m_sepxPositions = new List<int>();

	private List<uint> m_papPositions = new List<uint>();

	private List<int> m_papxPositions = new List<int>();

	private List<uint> m_chpPositions = new List<uint>();

	private List<int> m_chpxPositions = new List<int>();

	private int[] m_headerPositions;

	private string m_standardAsciiFont;

	private string m_standardFarEastFont;

	private string m_standardNonFarEastFont;

	private string m_standardBidiFont;

	private GrammarSpelling m_grammarSpellingTablesData;

	internal ArtObjectsRW ArtObj
	{
		get
		{
			if (m_artObjects == null)
			{
				m_artObjects = new ArtObjectsRW();
			}
			return m_artObjects;
		}
	}

	internal AnnotationsRW Annotations
	{
		get
		{
			if (m_anotations == null)
			{
				m_anotations = new AnnotationsRW();
			}
			return m_anotations;
		}
	}

	internal FootnotesRW Footnotes
	{
		get
		{
			if (m_footnotes == null)
			{
				m_footnotes = new FootnotesRW();
			}
			return m_footnotes;
		}
	}

	internal EndnotesRW Endnotes
	{
		get
		{
			if (m_endnotes == null)
			{
				m_endnotes = new EndnotesRW();
			}
			return m_endnotes;
		}
	}

	internal SectionExceptionsTable SectionsTable
	{
		get
		{
			if (m_sectionTable == null)
			{
				throw new InvalidOperationException("You can not get \"table\" object without calling the Read() method!");
			}
			return m_sectionTable;
		}
	}

	internal FontFamilyNameStringTable FFNStringTable
	{
		get
		{
			if (m_ffnStringTable == null)
			{
				throw new InvalidOperationException("You can not get \"table\" object without calling the Read() method!");
			}
			return m_ffnStringTable;
		}
		set
		{
			m_ffnStringTable = value;
		}
	}

	internal StyleSheetInfoRecord StyleSheetInfo
	{
		get
		{
			return m_styleSheetInfo;
		}
		set
		{
			m_styleSheetInfo = value;
		}
	}

	internal StyleDefinitionRecord[] StyleDefinitions
	{
		get
		{
			return m_arrStyleDefinitions;
		}
		set
		{
			m_arrStyleDefinitions = value;
		}
	}

	internal CharPosTableRecord HeaderFooterCharPosTable => m_charPosTableHF;

	internal BinaryTable CHPXBinaryTable => m_binTableCHPX;

	internal BinaryTable PAPXBinaryTable => m_binTablePAPX;

	internal int[] HeaderPositions
	{
		get
		{
			return m_headerPositions;
		}
		set
		{
			m_headerPositions = value;
		}
	}

	internal int SectionCount => m_secPositions.Count + 1;

	internal BookmarkNameStringTable BookmarkStrings
	{
		get
		{
			if (m_bkmkStringTable == null)
			{
				m_bkmkStringTable = new BookmarkNameStringTable();
			}
			return m_bkmkStringTable;
		}
	}

	internal BookmarkDescriptor BookmarkDescriptor
	{
		get
		{
			if (m_bkmkDescriptor == null)
			{
				m_bkmkDescriptor = new BookmarkDescriptor();
			}
			return m_bkmkDescriptor;
		}
	}

	internal DOPDescriptor DOP
	{
		get
		{
			return m_dopDescriptor;
		}
		set
		{
			m_dopDescriptor = value;
		}
	}

	internal ListInfo ListInfo
	{
		get
		{
			if (m_listInfo == null)
			{
				m_listInfo = new ListInfo();
			}
			return m_listInfo;
		}
		set
		{
			m_listInfo = value;
		}
	}

	internal List<uint> PieceTablePositions => m_pieceTablePositions;

	internal ArtObjectsRW FileArtObjects => m_artObjects;

	internal EscherClass Escher
	{
		get
		{
			if (m_escher == null)
			{
				m_escher = new EscherClass(null);
			}
			return m_escher;
		}
		set
		{
			m_escher = value;
		}
	}

	internal Fields Fields
	{
		get
		{
			if (m_fields == null)
			{
				m_fields = new Fields();
			}
			return m_fields;
		}
		set
		{
			m_fields = value;
		}
	}

	internal byte[] MacroCommands
	{
		get
		{
			return m_macroCommands;
		}
		set
		{
			m_macroCommands = value;
		}
	}

	internal Fib Fib => m_fib;

	internal string StandardAsciiFont
	{
		get
		{
			return m_standardAsciiFont;
		}
		set
		{
			m_standardAsciiFont = value;
		}
	}

	internal string StandardFarEastFont
	{
		get
		{
			return m_standardFarEastFont;
		}
		set
		{
			m_standardFarEastFont = value;
		}
	}

	internal string StandardNonFarEastFont
	{
		get
		{
			return m_standardNonFarEastFont;
		}
		set
		{
			m_standardNonFarEastFont = value;
		}
	}

	internal string StandardBidiFont
	{
		get
		{
			return m_standardBidiFont;
		}
		set
		{
			m_standardBidiFont = value;
		}
	}

	internal GrammarSpelling GrammarSpellingData
	{
		get
		{
			return m_grammarSpellingTablesData;
		}
		set
		{
			m_grammarSpellingTablesData = value;
		}
	}

	internal byte[] Variables
	{
		get
		{
			return m_variables;
		}
		set
		{
			m_variables = value;
		}
	}

	internal byte[] AsociatedStrings
	{
		get
		{
			return m_assocStrings;
		}
		set
		{
			m_assocStrings = value;
		}
	}

	internal byte[] SttbfRMark
	{
		get
		{
			return m_sttbfRMark;
		}
		set
		{
			m_sttbfRMark = value;
		}
	}

	internal WPTablesData(Fib fib)
	{
		m_fib = fib;
	}

	internal void Read(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Fib fib = m_fib;
		stream.Position = fib.FibRgFcLcb97FcPlcfSed;
		m_sectionTable = new SectionExceptionsTable(stream, (int)fib.FibRgFcLcb97LcbPlcfSed);
		ReadFFNTable(stream, fib);
		ReadBookmarks(fib, stream);
		stream.Position = fib.FibRgFcLcb97FcPlcftxbxTxt;
		ReadFields(fib, stream);
		stream.Position = fib.FibRgFcLcb97FcPlcfBteChpx;
		m_binTableCHPX = new BinaryTable(stream, (int)fib.FibRgFcLcb97LcbPlcfBteChpx);
		stream.Position = fib.FibRgFcLcb97FcPlcfBtePapx;
		m_binTablePAPX = new BinaryTable(stream, (int)fib.FibRgFcLcb97LcbPlcfBtePapx);
		ReadStyleSheet(stream);
		if (fib.FibRgFcLcb97LcbPlcfHdd != 0)
		{
			stream.Position = fib.FibRgFcLcb97FcPlcfHdd;
			m_charPosTableHF = new CharPosTableRecord(stream, (int)fib.FibRgFcLcb97LcbPlcfHdd);
		}
		ReadLists(fib, stream);
		ReadDocumentProperties(fib, stream);
		ReadComplexPart(stream);
		ParsePieceTableEncodings();
		ReadFootnotes(fib, stream);
		ReadAnnotations(fib, stream);
		ReadEndnotes(fib, stream);
		ReadArtObjects(fib, stream);
		ReadMacroCommands(fib, stream);
		ReadGrammarSpellingData(fib, stream);
		ReadVariables(fib, stream);
		ReadAssocStrings(fib, stream);
		ReadSttbfRMark(fib, stream);
	}

	internal void Write(Stream stream, bool hasSubDocument)
	{
		Fib fib = m_fib;
		GenerateTables(hasSubDocument);
		WriteStyleSheet(stream);
		if (hasSubDocument)
		{
			WriteFootnotes(fib, stream);
		}
		if (hasSubDocument)
		{
			WriteAnnotations(fib, stream);
		}
		uint fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		uint num = (uint)m_sectionTable.Save(stream);
		if (num != 0)
		{
			fib.FibRgFcLcb97FcPlcfSed = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfSed = num;
		}
		if (hasSubDocument && m_charPosTableHF != null && m_charPosTableHF.Positions != null)
		{
			fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
			num = (uint)m_charPosTableHF.Save(stream);
			if (num != 0)
			{
				fib.FibRgFcLcb97FcPlcfHdd = fibRgFcLcb97FcPlcfSed;
				fib.FibRgFcLcb97LcbPlcfHdd = num;
			}
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		num = (uint)m_binTableCHPX.Save(stream);
		if (num != 0)
		{
			fib.FibRgFcLcb97FcPlcfBteChpx = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfBteChpx = num;
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		num = (uint)m_binTablePAPX.Save(stream);
		if (num != 0)
		{
			fib.FibRgFcLcb97FcPlcfBtePapx = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfBtePapx = num;
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteFFNTable(stream);
		uint num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb97FcSttbfFfn = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbSttbfFfn = num2 - fibRgFcLcb97FcPlcfSed;
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteFields(stream, WordSubdocument.Main);
		num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb97FcPlcfFldMom = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfFldMom = num2 - fibRgFcLcb97FcPlcfSed;
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteFields(stream, WordSubdocument.HeaderFooter);
		num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb97FcPlcfFldHdr = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfFldHdr = num2 - fibRgFcLcb97FcPlcfSed;
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteFields(stream, WordSubdocument.Footnote);
		num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb97FcPlcfFldFtn = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfFldFtn = num2 - fibRgFcLcb97FcPlcfSed;
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteFields(stream, WordSubdocument.Annotation);
		num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb97FcPlcfFldAtn = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfFldAtn = num2 - fibRgFcLcb97FcPlcfSed;
		}
		WriteBookmarks(stream, fib);
		WriteMacroCommands(fib, stream);
		WriteDocumentProperties(stream, fib);
		WriteAssocStrings(fib, stream);
		WriteSttbfRMark(fib, stream);
		fib.FibRgFcLcb97FcClx = (uint)stream.Position;
		m_pieceTable.Save(stream);
		fib.FibRgFcLcb97LcbClx = (uint)m_pieceTable.Length;
		if (hasSubDocument)
		{
			WriteEndnotes(fib, stream);
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteFields(stream, WordSubdocument.Endnote);
		num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb97FcPlcfFldEdn = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfFldEdn = num2 - fibRgFcLcb97FcPlcfSed;
		}
		WriteArtObjects(stream, fib);
		if (m_grammarSpellingTablesData != null && m_grammarSpellingTablesData.PlcfsplData != null)
		{
			fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
			num = (uint)m_grammarSpellingTablesData.PlcfsplData.Length;
			if (num != 0)
			{
				fib.FibRgFcLcb97FcPlcfSpl = fibRgFcLcb97FcPlcfSed;
				stream.Write(m_grammarSpellingTablesData.PlcfsplData, 0, (int)num);
				fib.FibRgFcLcb97LcbPlcfSpl = num;
			}
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteFields(stream, WordSubdocument.TextBox);
		num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb97FcPlcfFldTxbx = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcfFldTxbx = num2 - fibRgFcLcb97FcPlcfSed;
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteFields(stream, WordSubdocument.HeaderTextBox);
		num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb97FcPlcffldHdrTxbx = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb97LcbPlcffldHdrTxbx = num2 - fibRgFcLcb97FcPlcfSed;
		}
		WriteVariables(fib, stream);
		if (m_listInfo != null)
		{
			fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
			num = (uint)m_listInfo.WriteLst(stream);
			if (num != 0)
			{
				m_fib.FibRgFcLcb97FcPlfLst = fibRgFcLcb97FcPlcfSed;
				m_fib.FibRgFcLcb97LcbPlfLst = num;
			}
			fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
			num = (uint)m_listInfo.WriteLfo(stream);
			if (num != 0)
			{
				m_fib.FibRgFcLcb97FcPlfLfo = fibRgFcLcb97FcPlcfSed;
				m_fib.FibRgFcLcb97LcbPlfLfo = num;
			}
		}
		if (m_grammarSpellingTablesData != null && m_grammarSpellingTablesData.PlcfgramData != null)
		{
			fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
			num = (uint)m_grammarSpellingTablesData.PlcfgramData.Length;
			if (num != 0)
			{
				fib.FibRgFcLcb97FcPlcfGram = fibRgFcLcb97FcPlcfSed;
				stream.Write(m_grammarSpellingTablesData.PlcfgramData, 0, (int)num);
				fib.FibRgFcLcb97LcbPlcfGram = num;
			}
		}
		if (m_listInfo != null)
		{
			fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
			num = (uint)(2 + m_listInfo.WriteStringTable(stream));
			if (num != 0)
			{
				m_fib.FibRgFcLcb97FcSttbListNames = fibRgFcLcb97FcPlcfSed;
				m_fib.FibRgFcLcb97LcbSttbListNames = num;
			}
		}
		fibRgFcLcb97FcPlcfSed = (uint)stream.Position;
		WriteRmdThreading(stream);
		num2 = (uint)stream.Position;
		if (num2 > fibRgFcLcb97FcPlcfSed)
		{
			fib.FibRgFcLcb2000FcRmdThreading = fibRgFcLcb97FcPlcfSed;
			fib.FibRgFcLcb2000LcbRmdThreading = num2 - fibRgFcLcb97FcPlcfSed;
		}
	}

	internal uint ConvertCharPosToFileCharPos(uint charPos)
	{
		uint num = uint.MaxValue;
		PieceTable pieceTable = m_pieceTable;
		int i = 0;
		for (int entriesCount = pieceTable.EntriesCount; i < entriesCount; i++)
		{
			if (charPos >= pieceTable.FileCharacterPos[i] && charPos < pieceTable.FileCharacterPos[i + 1])
			{
				num = NormalizeFileCharPos(pieceTable.Entries[i].FileOffset, out var bIsUnicode);
				if (!bIsUnicode)
				{
					num += charPos - pieceTable.FileCharacterPos[i];
					m_fib.Encoding = Encoding.UTF8;
				}
				else
				{
					num += (charPos - pieceTable.FileCharacterPos[i]) * 2;
					m_fib.Encoding = Encoding.Unicode;
				}
				break;
			}
		}
		if (num == uint.MaxValue)
		{
			int num2 = pieceTable.EntriesCount - 1;
			num = NormalizeFileCharPos(pieceTable.Entries[num2].FileOffset, out var bIsUnicode2);
			if (!bIsUnicode2)
			{
				num += charPos - pieceTable.FileCharacterPos[num2];
				m_fib.Encoding = Encoding.UTF8;
			}
			else
			{
				num += (charPos - pieceTable.FileCharacterPos[num2]) * 2;
				m_fib.Encoding = Encoding.Unicode;
			}
		}
		return num;
	}

	internal BookmarkInfo[] GetBookmarks()
	{
		if (m_bkmkStringTable == null || m_bkmkStringTable.BookmarkCount == 0)
		{
			return null;
		}
		int bookmarkCount = m_bkmkStringTable.BookmarkCount;
		BookmarkInfo[] array = new BookmarkInfo[bookmarkCount];
		for (int i = 0; i < bookmarkCount; i++)
		{
			array[i] = new BookmarkInfo(m_bkmkStringTable[i], m_bkmkDescriptor.GetBeginPos(i), m_bkmkDescriptor.GetEndPos(i), m_bkmkDescriptor.IsCellGroup(i), m_bkmkDescriptor.GetStartCellIndex(i), m_bkmkDescriptor.GetEndCellIndex(i));
			array[i].Index = i;
		}
		return array;
	}

	internal void AddSectionRecord(int charPos, int sepxPos)
	{
		m_secPositions.Add(charPos);
		m_sepxPositions.Add(sepxPos);
	}

	internal void AddPapxRecord(uint charPos, int papxPos)
	{
		m_papPositions.Add(charPos);
		m_papxPositions.Add(papxPos);
	}

	internal void AddChpxRecord(uint pos, int chpxPos)
	{
		m_chpPositions.Add(pos);
		m_chpxPositions.Add(chpxPos);
	}

	internal void AddStyleSheetTable(WordStyleSheet styleSheet)
	{
		int stylesCount = styleSheet.StylesCount;
		StyleSheetInfo.StylesCount = (ushort)stylesCount;
		StyleDefinitionRecord[] array = new StyleDefinitionRecord[stylesCount];
		ushort num = 4094;
		for (int i = 0; i < stylesCount; i++)
		{
			WordStyle styleByIndex = styleSheet.GetStyleByIndex(i);
			StyleDefinitionRecord styleDefinitionRecord = null;
			if (styleByIndex != WordStyle.Empty)
			{
				ushort num2 = (ushort)((styleByIndex.ID > -1) ? styleByIndex.ID : num);
				styleDefinitionRecord = new StyleDefinitionRecord(styleByIndex.Name, num2, StyleSheetInfo);
				if (num2 > 0 && num2 < 10)
				{
					styleDefinitionRecord.IsQFormat = true;
					styleDefinitionRecord.UnhideWhenUsed = true;
				}
				else if (styleByIndex.IsPrimary)
				{
					styleDefinitionRecord.IsQFormat = true;
				}
				styleDefinitionRecord.BaseStyle = (ushort)styleByIndex.BaseStyleIndex;
				styleDefinitionRecord.NextStyleId = (ushort)styleByIndex.NextStyleIndex;
				styleDefinitionRecord.HasUpe = styleByIndex.HasUpe;
				styleDefinitionRecord.IsSemiHidden = styleByIndex.IsSemiHidden;
				styleDefinitionRecord.UnhideWhenUsed = styleByIndex.UnhideWhenUsed;
				styleDefinitionRecord.TypeCode = styleByIndex.TypeCode;
				if (styleByIndex.LinkStyleIndex >= 0 && styleByIndex.LinkStyleIndex < stylesCount)
				{
					styleDefinitionRecord.LinkStyleId = (ushort)styleByIndex.LinkStyleIndex;
				}
				if (styleByIndex.IsCharacterStyle)
				{
					styleDefinitionRecord.TypeCode = WordStyleType.CharacterStyle;
					styleDefinitionRecord.CharacterProperty = styleByIndex.CHPX;
					styleDefinitionRecord.ParagraphProperty = null;
				}
				else if (styleDefinitionRecord.TypeCode == WordStyleType.TableStyle && styleByIndex.TableStyleData != null)
				{
					styleDefinitionRecord.UpxNumber = 3;
					styleDefinitionRecord.Tapx = new byte[styleByIndex.TableStyleData.Length];
					Buffer.BlockCopy(styleByIndex.TableStyleData, 0, styleDefinitionRecord.Tapx, 0, styleByIndex.TableStyleData.Length);
				}
				else
				{
					styleDefinitionRecord.TypeCode = WordStyleType.ParagraphStyle;
					styleDefinitionRecord.ParagraphProperty = styleByIndex.PAPX;
					styleDefinitionRecord.CharacterProperty = styleByIndex.CHPX;
				}
			}
			array[i] = styleDefinitionRecord;
		}
		StyleDefinitions = array;
		ushort[] array2 = new ushort[3];
		if (m_standardAsciiFont != null)
		{
			array2[0] = (ushort)styleSheet.FontNameToIndex(m_standardAsciiFont);
		}
		if (m_standardFarEastFont != null)
		{
			array2[1] = (ushort)styleSheet.FontNameToIndex(m_standardFarEastFont);
		}
		if (m_standardNonFarEastFont != null)
		{
			array2[2] = (ushort)styleSheet.FontNameToIndex(m_standardNonFarEastFont);
		}
		if (m_standardBidiFont != null)
		{
			StyleSheetInfo.FtcBi = (ushort)styleSheet.FontNameToIndex(m_standardBidiFont);
		}
		StyleSheetInfo.StandardChpStsh = array2;
		AddFFNTable(styleSheet);
	}

	internal void Close()
	{
		if (m_escher != null)
		{
			m_escher = null;
		}
		if (m_fib != null)
		{
			m_fib.Encoding = null;
			m_fib = null;
		}
		if (m_sectionTable != null)
		{
			m_sectionTable = null;
		}
		if (m_pieceTable != null)
		{
			m_pieceTable = null;
		}
		if (m_ffnStringTable != null)
		{
			m_ffnStringTable = null;
		}
		if (m_bkmkStringTable != null)
		{
			m_bkmkStringTable = null;
		}
		if (m_bkmkDescriptor != null)
		{
			m_bkmkDescriptor = null;
		}
		if (m_binTableCHPX != null)
		{
			m_binTableCHPX = null;
		}
		if (m_binTablePAPX != null)
		{
			m_binTablePAPX = null;
		}
		if (m_listInfo != null)
		{
			m_listInfo = null;
		}
		if (m_charPosTableHF != null)
		{
			m_charPosTableHF = null;
		}
		if (m_styleSheetInfo != null)
		{
			m_styleSheetInfo.Close();
			m_styleSheetInfo = null;
		}
		if (m_arrStyleDefinitions != null)
		{
			StyleDefinitionRecord[] arrStyleDefinitions = m_arrStyleDefinitions;
			for (int i = 0; i < arrStyleDefinitions.Length; i++)
			{
				arrStyleDefinitions[i]?.Close();
			}
			m_arrStyleDefinitions = null;
		}
		if (m_pieceTablePositions != null)
		{
			m_pieceTablePositions.Clear();
			m_pieceTablePositions = null;
		}
		if (m_pieceTableEncodings != null)
		{
			m_pieceTableEncodings.Clear();
			m_pieceTableEncodings = null;
		}
		m_dopDescriptor = null;
		if (m_artObjects != null)
		{
			m_artObjects.Close();
			m_artObjects = null;
		}
		if (m_anotations != null)
		{
			m_anotations.Close();
			m_anotations = null;
		}
		if (m_footnotes != null)
		{
			m_footnotes.Close();
			m_footnotes = null;
		}
		if (m_endnotes != null)
		{
			m_endnotes.Close();
			m_endnotes = null;
		}
		if (m_fields != null)
		{
			m_fields.Close();
			m_fields = null;
		}
		m_macroCommands = null;
		m_variables = null;
		m_assocStrings = null;
		if (m_clxModifiers != null)
		{
			m_clxModifiers.Close();
			m_clxModifiers = null;
		}
		if (m_secPositions != null)
		{
			m_secPositions.Clear();
			m_secPositions = null;
		}
		if (m_sepxPositions != null)
		{
			m_sepxPositions.Clear();
			m_sepxPositions = null;
		}
		if (m_papPositions != null)
		{
			m_papPositions.Clear();
			m_papPositions = null;
		}
		if (m_papxPositions != null)
		{
			m_papxPositions.Clear();
			m_papxPositions = null;
		}
		if (m_chpPositions != null)
		{
			m_chpPositions.Clear();
			m_chpPositions = null;
		}
		if (m_chpxPositions != null)
		{
			m_chpxPositions.Clear();
			m_chpxPositions = null;
		}
		m_headerPositions = null;
		if (m_grammarSpellingTablesData != null)
		{
			m_grammarSpellingTablesData = null;
		}
	}

	private void ReadFFNTable(Stream stream, Fib fib)
	{
		m_ffnStringTable = new FontFamilyNameStringTable();
		stream.Position = fib.FibRgFcLcb97FcSttbfFfn;
		m_ffnStringTable.Parse(stream, (int)fib.FibRgFcLcb97LcbSttbfFfn);
	}

	internal Encoding GetEncodingByFC(long position)
	{
		for (int i = 0; i < m_pieceTable.EntriesCount; i++)
		{
			if (m_pieceTablePositions[i] <= position && position <= m_pieceTablePositions[i + 1])
			{
				return m_pieceTableEncodings[i];
			}
		}
		return m_fib.Encoding;
	}

	internal uint ConvertFCToCP(uint fc)
	{
		uint result = 0u;
		int i = 0;
		for (int num = m_pieceTablePositions.Count - 1; i < num; i++)
		{
			if (fc >= m_pieceTablePositions[i] && fc <= m_pieceTablePositions[i + 1])
			{
				uint num2 = ((m_pieceTableEncodings[i] != Encoding.Unicode) ? 1u : 2u);
				if (fc == m_pieceTablePositions[i + 1])
				{
					i++;
				}
				uint num3 = (fc - m_pieceTablePositions[i]) / num2;
				result = m_pieceTable.FileCharacterPos[i] + num3;
				break;
			}
		}
		return result;
	}

	internal bool HasSubdocument(WordSubdocument wsType)
	{
		switch (wsType)
		{
		case WordSubdocument.Annotation:
			if (m_anotations != null)
			{
				return m_anotations.Count > 0;
			}
			return false;
		case WordSubdocument.Endnote:
			if (m_endnotes != null)
			{
				return m_endnotes.Count > 0;
			}
			return false;
		case WordSubdocument.Footnote:
			if (m_footnotes != null)
			{
				return m_footnotes.Count > 0;
			}
			return false;
		case WordSubdocument.TextBox:
			if (m_artObjects != null && m_artObjects.MainDocTxBxs != null)
			{
				return m_artObjects.MainDocTxBxs.Count > 0;
			}
			return false;
		case WordSubdocument.HeaderTextBox:
			if (m_artObjects != null && m_artObjects.HfDocTxBxs != null)
			{
				return m_artObjects.HfDocTxBxs.Count > 0;
			}
			return false;
		default:
			return true;
		}
	}

	internal bool HasList()
	{
		return m_listInfo != null;
	}

	private void AddFFNTable(WordStyleSheet styleSheet)
	{
		FontFamilyNameStringTable fontFamilyNameStringTable = new FontFamilyNameStringTable();
		fontFamilyNameStringTable.RecordsCount = styleSheet.FontNamesList.Count;
		int i = 0;
		for (int num = fontFamilyNameStringTable.FontFamilyNameRecords.Length; i < num; i++)
		{
			bool flag = true;
			if (m_ffnStringTable != null)
			{
				for (int j = 0; j < FFNStringTable.RecordsCount; j++)
				{
					if (FFNStringTable.FontFamilyNameRecords[j].FontName == styleSheet.FontNamesList[i])
					{
						fontFamilyNameStringTable.FontFamilyNameRecords[i] = FFNStringTable.FontFamilyNameRecords[j];
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				FontFamilyNameRecord fontFamilyNameRecord = new FontFamilyNameRecord();
				if (styleSheet.FontSubstitutionTable.ContainsKey(styleSheet.FontNamesList[i]))
				{
					fontFamilyNameRecord.AlternativeFontName = styleSheet.FontSubstitutionTable[styleSheet.FontNamesList[i]];
				}
				fontFamilyNameRecord.FontName = styleSheet.FontNamesList[i];
				fontFamilyNameRecord.PitchRequest = 2;
				fontFamilyNameRecord.TrueType = true;
				fontFamilyNameRecord.FontFamilyID = 1;
				fontFamilyNameRecord.Weight = 400;
				fontFamilyNameStringTable.FontFamilyNameRecords[i] = fontFamilyNameRecord;
			}
		}
		FFNStringTable = fontFamilyNameStringTable;
	}

	private void WriteFFNTable(Stream stream)
	{
		m_ffnStringTable.Save(stream);
	}

	private void WriteStyleSheet(Stream stream)
	{
		uint num = (uint)stream.Position;
		ushort stylesCount = (ushort)m_arrStyleDefinitions.Length;
		ushort val = (ushort)m_styleSheetInfo.Length;
		WriteShort(stream, val);
		m_styleSheetInfo.StylesCount = stylesCount;
		m_styleSheetInfo.Save(stream);
		int i = 0;
		for (int num2 = m_arrStyleDefinitions.Length; i < num2; i++)
		{
			StyleDefinitionRecord styleDefinitionRecord = m_arrStyleDefinitions[i];
			if (styleDefinitionRecord == null || styleDefinitionRecord.StyleName == null)
			{
				stream.Write(new byte[2], 0, 2);
				continue;
			}
			if (styleDefinitionRecord.StyleId == 0 || styleDefinitionRecord.StyleId == 65)
			{
				styleDefinitionRecord.BaseStyle = 4095;
			}
			WriteShort(stream, (ushort)styleDefinitionRecord.Length);
			styleDefinitionRecord.Save(stream);
		}
		Fib fib = m_fib;
		uint fibRgFcLcb97FcStshfOrig = (m_fib.FibRgFcLcb97FcStshf = num);
		fib.FibRgFcLcb97FcStshfOrig = fibRgFcLcb97FcStshfOrig;
		Fib fib2 = m_fib;
		fibRgFcLcb97FcStshfOrig = (m_fib.FibRgFcLcb97LcbStshf = (uint)(stream.Position - num));
		fib2.FibRgFcLcb97LcbStshfOrig = fibRgFcLcb97FcStshfOrig;
	}

	private int WriteShort(Stream stream, ushort val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		stream.Write(bytes, 0, bytes.Length);
		return bytes.Length;
	}

	private void ReadStyleSheet(Stream stream)
	{
		long lNextBlockStart = ReadStyleSheetTable(stream);
		ReadStylesDefinitions(stream, lNextBlockStart);
		if (StyleSheetInfo.StandardChpStsh[0] < m_ffnStringTable.FontFamilyNameRecords.Length)
		{
			m_standardAsciiFont = m_ffnStringTable.FontFamilyNameRecords[StyleSheetInfo.StandardChpStsh[0]].FontName;
		}
		if (StyleSheetInfo.StandardChpStsh[1] < m_ffnStringTable.FontFamilyNameRecords.Length)
		{
			m_standardFarEastFont = m_ffnStringTable.FontFamilyNameRecords[StyleSheetInfo.StandardChpStsh[1]].FontName;
		}
		if (StyleSheetInfo.StandardChpStsh[2] < m_ffnStringTable.FontFamilyNameRecords.Length)
		{
			m_standardNonFarEastFont = m_ffnStringTable.FontFamilyNameRecords[StyleSheetInfo.StandardChpStsh[2]].FontName;
		}
		if (StyleSheetInfo.FtcBi < m_ffnStringTable.FontFamilyNameRecords.Length)
		{
			m_standardBidiFont = m_ffnStringTable.FontFamilyNameRecords[StyleSheetInfo.FtcBi].FontName;
		}
		if (string.IsNullOrEmpty(m_standardAsciiFont))
		{
			m_standardAsciiFont = "Times New Roman";
		}
		if (string.IsNullOrEmpty(m_standardFarEastFont))
		{
			m_standardFarEastFont = "Times New Roman";
		}
		if (string.IsNullOrEmpty(m_standardNonFarEastFont))
		{
			m_standardNonFarEastFont = "Times New Roman";
		}
		if (string.IsNullOrEmpty(m_standardBidiFont))
		{
			m_standardBidiFont = "Times New Roman";
		}
	}

	private long ReadStyleSheetTable(Stream stream)
	{
		uint fibRgFcLcb97FcStshf = m_fib.FibRgFcLcb97FcStshf;
		uint fibRgFcLcb97LcbStshf = m_fib.FibRgFcLcb97LcbStshf;
		if (fibRgFcLcb97LcbStshf == 0)
		{
			throw new Exception("Length of StyleSheetInfo record can not be less 0!");
		}
		stream.Position = fibRgFcLcb97FcStshf;
		ushort iCount = BaseWordRecord.ReadUInt16(stream);
		m_styleSheetInfo = new StyleSheetInfoRecord(stream, iCount);
		return fibRgFcLcb97FcStshf + fibRgFcLcb97LcbStshf;
	}

	private void ReadStylesDefinitions(Stream stream, long lNextBlockStart)
	{
		int stylesCount = m_styleSheetInfo.StylesCount;
		StyleDefinitionRecord[] array = new StyleDefinitionRecord[stylesCount];
		for (int i = 0; i < stylesCount; i++)
		{
			ushort iCount = BaseWordRecord.ReadUInt16(stream);
			StyleDefinitionRecord styleDefinitionRecord = new StyleDefinitionRecord(stream, iCount, m_styleSheetInfo);
			array[i] = styleDefinitionRecord;
		}
		m_arrStyleDefinitions = array;
	}

	private void GenerateTables(bool hasSubDocument)
	{
		m_sectionTable = new SectionExceptionsTable();
		m_sectionTable.EntriesCount = m_secPositions.Count;
		int i = 0;
		for (int count = m_secPositions.Count; i < count; i++)
		{
			m_sectionTable.Positions[i] = m_secPositions[i];
			m_sectionTable.Descriptors[i].MacPrintOffset = -1;
			m_sectionTable.Descriptors[i].SepxPosition = (uint)(512 * m_sepxPositions[i]);
		}
		int num = m_fib.CcpText + m_fib.CcpFtn + m_fib.CcpHdd + m_fib.CcpAtn + m_fib.CcpEdn + m_fib.CcpTxbx + m_fib.CcpHdrTxbx;
		m_sectionTable.Positions[m_secPositions.Count] = num;
		m_binTableCHPX = new BinaryTable();
		m_binTableCHPX.EntriesCount = m_chpPositions.Count;
		int j = 0;
		for (int count2 = m_chpPositions.Count; j < count2; j++)
		{
			m_binTableCHPX.FileCharacterPos[j] = m_chpPositions[j];
			BinTableEntry binTableEntry = new BinTableEntry();
			binTableEntry.Value = m_chpxPositions[j];
			m_binTableCHPX.Entries[j] = binTableEntry;
		}
		m_binTableCHPX.FileCharacterPos[m_binTableCHPX.EntriesCount] = m_fib.BaseReserved6;
		m_binTablePAPX = new BinaryTable();
		m_binTablePAPX.EntriesCount = m_papPositions.Count;
		int k = 0;
		for (int count3 = m_papPositions.Count; k < count3; k++)
		{
			m_binTablePAPX.FileCharacterPos[k] = m_papPositions[k];
			BinTableEntry binTableEntry2 = new BinTableEntry();
			binTableEntry2.Value = m_papxPositions[k];
			m_binTablePAPX.Entries[k] = binTableEntry2;
		}
		m_binTablePAPX.FileCharacterPos[m_binTablePAPX.EntriesCount] = m_fib.BaseReserved6;
		m_pieceTable = new PieceTable();
		m_pieceTable.EntriesCount = 1;
		m_pieceTable.FileCharacterPos[0] = 0u;
		m_pieceTable.FileCharacterPos[1] = (uint)num;
		m_pieceTable.Entries[0].FileOffset = ((m_fib.Encoding == Encoding.Unicode) ? 2048u : 1073745920u);
		m_pieceTable.Entries[0].fCopied = true;
		if (hasSubDocument)
		{
			m_charPosTableHF = new CharPosTableRecord();
			m_charPosTableHF.Positions = m_headerPositions;
		}
	}

	private uint NormalizeFileCharPos(uint fileCharPos, out bool bIsUnicode)
	{
		bIsUnicode = true;
		if ((fileCharPos & 0x40000000u) != 0)
		{
			fileCharPos &= 0xBFFFFFFFu;
			fileCharPos /= 2;
			bIsUnicode = false;
		}
		return fileCharPos;
	}

	private void ReadComplexPart(Stream stream)
	{
		uint fibRgFcLcb97FcClx = m_fib.FibRgFcLcb97FcClx;
		uint fibRgFcLcb97LcbClx = m_fib.FibRgFcLcb97LcbClx;
		long num = fibRgFcLcb97FcClx + fibRgFcLcb97LcbClx;
		stream.Position = fibRgFcLcb97FcClx;
		while (stream.Position < num)
		{
			switch ((WordComplexBlockType)stream.ReadByte())
			{
			case WordComplexBlockType.Sprms:
			{
				int num2 = ReadUInt16(stream);
				byte[] array = new byte[num2];
				if (stream.Read(array, 0, num2) != num2)
				{
					throw new Exception("Was unable to read specified number of bytes");
				}
				m_clxModifiers = new SinglePropertyModifierArray();
				m_clxModifiers.Parse(array, 0, num2);
				break;
			}
			case WordComplexBlockType.PieceTable:
			{
				byte[] array = new byte[4];
				int num2 = stream.Read(array, 0, 4);
				if (num2 != 4)
				{
					throw new Exception("Was unable to read bytes from the stream");
				}
				num2 = BitConverter.ToInt32(array, 0);
				array = new byte[num2];
				if (num2 != stream.Read(array, 0, num2))
				{
					throw new Exception("Was unable to read bytes from the stream");
				}
				m_pieceTable = new PieceTable(array);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException("Unknown block type.");
			}
		}
	}

	private ushort ReadUInt16(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] array = new byte[2];
		if (stream.Read(array, 0, 2) != 2)
		{
			throw new Exception("Unable to read enough data from the stream");
		}
		return BitConverter.ToUInt16(array, 0);
	}

	private uint ReadUInt32(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] array = new byte[4];
		if (stream.Read(array, 0, 4) != 4)
		{
			throw new Exception("Unable to read enough data from the stream");
		}
		return BitConverter.ToUInt32(array, 0);
	}

	private void ReadLists(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbPlfLst != 0 && fib.FibRgFcLcb97LcbPlfLfo != 0)
		{
			m_listInfo = new ListInfo(fib, stream);
		}
	}

	private void WriteDocumentProperties(Stream stream, Fib fib)
	{
		fib.FibRgFcLcb97FcDop = (uint)stream.Position;
		fib.FibRgFcLcb97LcbDop = m_dopDescriptor.Write(stream);
	}

	private void ReadDocumentProperties(Fib fib, Stream stream)
	{
		stream.Position = fib.FibRgFcLcb97FcDop;
		int fibRgFcLcb97LcbDop = (int)fib.FibRgFcLcb97LcbDop;
		m_dopDescriptor = new DOPDescriptor(stream, (int)fib.FibRgFcLcb97FcDop, fibRgFcLcb97LcbDop, fib.FDot);
	}

	private void WriteFields(Stream stream, WordSubdocument subDocument)
	{
		if (m_fields != null)
		{
			int entriesCount = m_pieceTable.EntriesCount;
			uint endPosition = m_pieceTable.FileCharacterPos[entriesCount] + 1;
			m_fields.Write(stream, endPosition, subDocument);
		}
	}

	private void ReadFields(Fib fib, Stream stream)
	{
		m_fields = new Fields(fib, new BinaryReader(stream));
	}

	private void WriteBookmarks(Stream stream, Fib fib)
	{
		if (m_bkmkStringTable != null && m_bkmkStringTable.BookmarkNamesLength != 0)
		{
			m_bkmkStringTable.Save(stream, fib);
			int entriesCount = m_pieceTable.EntriesCount;
			uint endChar = m_pieceTable.FileCharacterPos[entriesCount] + 2;
			m_bkmkDescriptor.Save(stream, fib, endChar);
		}
	}

	private void ReadBookmarks(Fib fib, Stream stream)
	{
		stream.Position = fib.FibRgFcLcb97FcSttbfBkmk;
		int fibRgFcLcb97LcbSttbfBkmk = (int)fib.FibRgFcLcb97LcbSttbfBkmk;
		int fibRgFcLcb97FcPlcfBkf = (int)fib.FibRgFcLcb97FcPlcfBkf;
		int fibRgFcLcb97LcbPlcfBkf = (int)fib.FibRgFcLcb97LcbPlcfBkf;
		int fibRgFcLcb97FcPlcfBkl = (int)fib.FibRgFcLcb97FcPlcfBkl;
		int fibRgFcLcb97LcbPlcfBkl = (int)fib.FibRgFcLcb97LcbPlcfBkl;
		if (fibRgFcLcb97LcbSttbfBkmk > 0 && fibRgFcLcb97LcbPlcfBkf > 0 && fibRgFcLcb97LcbPlcfBkl > 0)
		{
			m_bkmkStringTable = new BookmarkNameStringTable(stream, fibRgFcLcb97LcbSttbfBkmk);
			m_bkmkDescriptor = new BookmarkDescriptor(stream, m_bkmkStringTable.BookmarkCount, fibRgFcLcb97FcPlcfBkf, fibRgFcLcb97LcbPlcfBkf, fibRgFcLcb97FcPlcfBkl, fibRgFcLcb97LcbPlcfBkl);
		}
	}

	private void ParsePieceTableEncodings()
	{
		m_pieceTablePositions.Clear();
		m_pieceTableEncodings.Clear();
		int i = 0;
		for (int entriesCount = m_pieceTable.EntriesCount; i < entriesCount; i++)
		{
			bool bIsUnicode;
			uint item = NormalizeFileCharPos(m_pieceTable.Entries[i].FileOffset, out bIsUnicode);
			m_pieceTablePositions.Add(item);
			m_pieceTableEncodings.Add(bIsUnicode ? Encoding.Unicode : Encoding.UTF8);
		}
		m_pieceTablePositions.Add(ConvertCharPosToFileCharPos(m_pieceTable.FileCharacterPos[m_pieceTable.EntriesCount]));
	}

	private void ReadArtObjects(Fib fib, Stream stream)
	{
		if (ContainShapes(fib))
		{
			m_artObjects = new ArtObjectsRW(fib, stream);
		}
	}

	private void ReadAnnotations(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbPlcfandTxt != 0 || fib.FibRgFcLcb97lcbPlcfandRef != 0)
		{
			m_anotations = new AnnotationsRW(stream, fib);
		}
	}

	private void ReadFootnotes(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbPlcffndTxt != 0 || fib.FibRgFcLcb97LcbPlcffndRef != 0)
		{
			m_footnotes = new FootnotesRW(stream, fib);
		}
	}

	private void ReadEndnotes(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbPlcfendTxt != 0 || fib.FibRgFcLcb97LcbPlcfendRef != 0)
		{
			m_endnotes = new EndnotesRW(stream, fib);
		}
	}

	private void WriteArtObjects(Stream stream, Fib fib)
	{
		MsofbtDgContainer msofbtDgContainer = m_escher.FindDgContainerForSubDocType(ShapeDocType.Main);
		MsofbtDgContainer msofbtDgContainer2 = m_escher.FindDgContainerForSubDocType(ShapeDocType.HeaderFooter);
		if (msofbtDgContainer == null && msofbtDgContainer2 == null)
		{
			return;
		}
		if (m_artObjects != null)
		{
			if (m_artObjects.MainDocFSPAs != null)
			{
				_ = msofbtDgContainer.PatriarchGroupContainer.Children;
			}
			if (m_artObjects.HfDocFSPAs != null)
			{
				_ = msofbtDgContainer2.PatriarchGroupContainer.Children;
			}
			int entriesCount = m_pieceTable.EntriesCount;
			int endHeader = fib.CcpHdd + 2;
			int endMain = (int)m_pieceTable.FileCharacterPos[entriesCount];
			m_artObjects.Write(stream, fib, endMain, endHeader);
		}
		fib.FibRgFcLcb97FcDggInfo = (uint)stream.Position;
		fib.FibRgFcLcb97LcbDggInfo = m_escher.WriteContainers(stream);
	}

	private void WriteAnnotations(Fib fib, Stream stream)
	{
		if (m_anotations != null)
		{
			m_anotations.Write(stream, fib);
		}
	}

	private void WriteFootnotes(Fib fib, Stream stream)
	{
		if (m_footnotes != null)
		{
			m_footnotes.InitialDescriptorNumber = DOP.InitialFootnoteNumber;
			m_footnotes.Write(stream, fib);
		}
	}

	private void WriteEndnotes(Fib fib, Stream stream)
	{
		if (m_endnotes != null)
		{
			m_endnotes.InitialDescriptorNumber = DOP.InitialEndnoteNumber;
			m_endnotes.Write(stream, fib);
		}
	}

	private static void SynchronizeSpids(ContainerCollection collection, IDictionaryEnumerator mainEnumerator)
	{
		foreach (BaseEscherRecord item in collection)
		{
			if (item is MsofbtSpContainer msofbtSpContainer && msofbtSpContainer.Shape.ShapeType == EscherShapeType.msosptPictureFrame)
			{
				mainEnumerator.MoveNext();
				(mainEnumerator.Value as FileShapeAddress).Spid = msofbtSpContainer.Shape.ShapeId;
			}
		}
	}

	private void ReadMacroCommands(Fib fib, Stream stream)
	{
		stream.Position = fib.FibRgFcLcb97FcCmds;
		m_macroCommands = new byte[fib.FibRgFcLcb97LcbCmds];
		stream.Read(m_macroCommands, 0, m_macroCommands.Length);
	}

	private void WriteMacroCommands(Fib fib, Stream stream)
	{
		if (m_macroCommands != null && m_macroCommands.Length != 0)
		{
			fib.FibRgFcLcb97FcCmds = (uint)stream.Position;
			stream.Write(m_macroCommands, 0, m_macroCommands.Length);
			fib.FibRgFcLcb97LcbCmds = (uint)m_macroCommands.Length;
		}
	}

	private void ReadGrammarSpellingData(Fib fib, Stream stream)
	{
		m_grammarSpellingTablesData = new GrammarSpelling(fib, stream, m_charPosTableHF);
	}

	private void WriteGrammarSpellingData(Fib fib, Stream stream)
	{
		if (m_grammarSpellingTablesData != null)
		{
			m_grammarSpellingTablesData.Write(fib, stream);
		}
	}

	private bool ContainShapes(Fib fib)
	{
		bool result = false;
		if (fib.FibRgFcLcb97LcbPlcSpaMom != 0 || fib.FibRgFcLcb97LcbPlcSpaHdr != 0 || fib.FibRgFcLcb97LcbPlcftxbxTxt != 0 || fib.FibRgFcLcb97LcbPlcfHdrtxbxTxt != 0 || fib.FibRgFcLcb97LcbPlcfTxbxBkd != 0 || fib.FibRgFcLcb97LcbPlcfTxbxHdrBkd != 0)
		{
			result = true;
		}
		return result;
	}

	private void ReadVariables(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbStwUser != 0)
		{
			stream.Position = (int)fib.FibRgFcLcb97FcStwUser;
			m_variables = new byte[fib.FibRgFcLcb97LcbStwUser];
			stream.Read(m_variables, 0, m_variables.Length);
		}
	}

	private void WriteVariables(Fib fib, Stream stream)
	{
		if (m_variables != null)
		{
			fib.FibRgFcLcb97FcStwUser = (uint)stream.Position;
			stream.Write(m_variables, 0, m_variables.Length);
			fib.FibRgFcLcb97LcbStwUser = (uint)m_variables.Length;
		}
	}

	private void ReadAssocStrings(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbSttbfAssoc != 0)
		{
			stream.Position = fib.FibRgFcLcb97FcSttbfAssoc;
			m_assocStrings = new byte[fib.FibRgFcLcb97LcbSttbfAssoc];
			stream.Read(m_assocStrings, 0, m_assocStrings.Length);
		}
	}

	private void ReadSttbfRMark(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbSttbfRMark != 0)
		{
			stream.Position = fib.FibRgFcLcb97FcSttbfRMark;
			m_sttbfRMark = new byte[fib.FibRgFcLcb97LcbSttbfRMark];
			stream.Read(m_sttbfRMark, 0, m_sttbfRMark.Length);
		}
	}

	private void WriteAssocStrings(Fib fib, Stream stream)
	{
		if (m_assocStrings != null)
		{
			fib.FibRgFcLcb97FcSttbfAssoc = (uint)stream.Position;
			stream.Write(m_assocStrings, 0, m_assocStrings.Length);
			fib.FibRgFcLcb97LcbSttbfAssoc = (uint)m_assocStrings.Length;
		}
	}

	private void WriteSttbfRMark(Fib fib, Stream stream)
	{
		if (CharacterPropertiesConverter.AuthorNames.Count > 0)
		{
			fib.FibRgFcLcb97FcSttbfRMark = (uint)stream.Position;
			UpdateSTTBStructure();
			stream.Write(m_sttbfRMark, 0, m_sttbfRMark.Length);
			fib.FibRgFcLcb97LcbSttbfRMark = (uint)m_sttbfRMark.Length;
		}
	}

	private void UpdateSTTBStructure()
	{
		byte[] bytes = BitConverter.GetBytes(ushort.MaxValue);
		List<string> authorNames = CharacterPropertiesConverter.AuthorNames;
		byte[] array = ((authorNames.Count >= 65535) ? BitConverter.GetBytes(authorNames.Count) : BitConverter.GetBytes((ushort)authorNames.Count));
		byte[] bytes2 = BitConverter.GetBytes((ushort)0);
		int num = 0;
		List<byte[]> list = new List<byte[]>(authorNames.Count);
		for (int i = 0; i < authorNames.Count; i++)
		{
			string text = authorNames[i];
			byte[] bytes3 = BitConverter.GetBytes((ushort)text.Length);
			list.Add(bytes3);
			num += bytes3.Length;
			byte[] bytes4 = Encoding.Unicode.GetBytes(text);
			list.Add(bytes4);
			num += bytes4.Length;
		}
		m_sttbfRMark = new byte[bytes.Length + array.Length + bytes2.Length + num];
		Buffer.BlockCopy(bytes, 0, m_sttbfRMark, 0, bytes.Length);
		Buffer.BlockCopy(array, 0, m_sttbfRMark, bytes.Length, array.Length);
		int num2 = array.Length + bytes.Length;
		Buffer.BlockCopy(bytes2, 0, m_sttbfRMark, num2, bytes2.Length);
		num2 += bytes2.Length;
		foreach (byte[] item in list)
		{
			Buffer.BlockCopy(item, 0, m_sttbfRMark, num2, item.Length);
			num2 += item.Length;
		}
		list.Clear();
	}

	private void WriteRmdThreading(Stream stream)
	{
		byte[] array = new byte[48]
		{
			255, 255, 1, 0, 8, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 255, 255, 1, 0,
			0, 0, 0, 0, 255, 255, 0, 0, 2, 0,
			255, 255, 0, 0, 0, 0, 255, 255, 0, 0,
			2, 0, 255, 255, 0, 0, 0, 0
		};
		stream.Write(array, 0, array.Length);
	}
}
