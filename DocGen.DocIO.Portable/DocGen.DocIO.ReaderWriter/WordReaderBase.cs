using System;
using System.Collections.Generic;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIO.Utilities;
using DocGen.Drawing;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal abstract class WordReaderBase : IWordReaderBase
{
	private const int INVALID_CHUNK_LENGTH = -1;

	private const int DEF_WORD9_DOP_LEN = 544;

	private const int DEF_WORD10_DOP_LEN = 594;

	private const int DEF_WORD11_DOP_LEN = 616;

	public StreamsManager m_streamsManager;

	public DocInfo m_docInfo;

	protected WordStyleSheet m_styleSheet;

	protected string m_textChunk = string.Empty;

	protected WordChunkType m_chunkType;

	protected int m_currStyleIndex;

	protected StatePositionsBase m_statePositions;

	protected WordSubdocument m_type;

	protected int m_startTextPos;

	protected int m_endTextPos;

	private ParagraphPropertyException m_papx;

	private CharacterPropertyException m_chpx;

	private BookmarkInfo[] m_bookmarks;

	private long m_iSavedStreamPosition = -1L;

	private bool m_bStreamPosSaved;

	private BookmarkInfo m_currentBookmark;

	private BookmarkInfo m_bookmarkAfterParaEnd;

	private bool m_isBookmarkStart;

	private bool m_isBKMKStartAfterParaEnd;

	private int m_cellCounter;

	private bool m_isCellMark;

	private Dictionary<int, string> m_sttbFRAuthorNames;

	private Stack<Dictionary<WTableRow, short>> m_tableRowWidthStack;

	private List<short> m_tableMaxRowWidth;

	public int CurrentStyleIndex => m_currStyleIndex;

	public WordStyleSheet StyleSheet => m_styleSheet;

	public WordChunkType ChunkType => m_chunkType;

	public string TextChunk
	{
		get
		{
			return m_textChunk;
		}
		set
		{
			m_textChunk = value;
		}
	}

	public CharacterPropertyException CHPX => m_chpx;

	internal SinglePropertyModifierArray CHPXSprms
	{
		get
		{
			if (m_chpx != null)
			{
				return m_chpx.PropertyModifiers;
			}
			return null;
		}
	}

	public ParagraphPropertyException PAPX => m_papx;

	internal SinglePropertyModifierArray PAPXSprms
	{
		get
		{
			if (m_papx != null)
			{
				return m_papx.PropertyModifiers;
			}
			return null;
		}
	}

	public ListInfo ListInfo => m_docInfo.TablesData.ListInfo;

	public bool HasTableBody => PAPXSprms.GetBoolean(9238, defValue: false);

	public EscherClass Escher
	{
		get
		{
			return m_docInfo.TablesData.Escher;
		}
		set
		{
			m_docInfo.TablesData.Escher = value;
		}
	}

	public Fields Fields => m_docInfo.TablesData.Fields;

	public WPTablesData TablesData => m_docInfo.TablesData;

	public int CurrentTextPosition
	{
		get
		{
			return (int)m_docInfo.TablesData.ConvertFCToCP((uint)m_streamsManager.MainStream.Position);
		}
		set
		{
			uint num = m_docInfo.TablesData.ConvertCharPosToFileCharPos((uint)value);
			m_streamsManager.MainStream.Position = num;
		}
	}

	public BookmarkInfo[] Bookmarks
	{
		get
		{
			if (m_bookmarks == null)
			{
				m_bookmarks = m_docInfo.TablesData.GetBookmarks();
			}
			return m_bookmarks;
		}
		set
		{
			m_bookmarks = value;
		}
	}

	public BookmarkInfo CurrentBookmark
	{
		get
		{
			return m_currentBookmark;
		}
		set
		{
			m_currentBookmark = value;
		}
	}

	public BookmarkInfo BookmarkAfterParaEnd
	{
		get
		{
			return m_bookmarkAfterParaEnd;
		}
		set
		{
			m_bookmarkAfterParaEnd = value;
		}
	}

	public bool IsBKMKStartAfterParaEnd
	{
		get
		{
			return m_isBKMKStartAfterParaEnd;
		}
		set
		{
			m_isBKMKStartAfterParaEnd = value;
		}
	}

	public bool IsBookmarkStart => m_isBookmarkStart;

	public DocumentVersion Version => GetDocVersion();

	protected Encoding Encoding => m_docInfo.TablesData.GetEncodingByFC(m_streamsManager.MainStream.Position);

	protected internal int EncodingCharSize
	{
		get
		{
			int result = 1;
			if (Encoding == Encoding.UTF8)
			{
				result = 1;
			}
			else if (Encoding == Encoding.Unicode)
			{
				result = 2;
			}
			return result;
		}
	}

	protected WordStyle CurrentStyle => StyleSheet.GetStyleByIndex(CurrentStyleIndex);

	internal int StartTextPos => m_startTextPos;

	internal int EndTextPos => m_endTextPos;

	public Dictionary<int, string> SttbfRMarkAuthorNames
	{
		get
		{
			if (m_docInfo.TablesData.SttbfRMark != null && m_sttbFRAuthorNames == null)
			{
				m_sttbFRAuthorNames = GetSTTBFRNames(m_docInfo.TablesData.SttbfRMark);
			}
			return m_sttbFRAuthorNames;
		}
	}

	public Stack<Dictionary<WTableRow, short>> TableRowWidthStack
	{
		get
		{
			if (m_tableRowWidthStack == null)
			{
				m_tableRowWidthStack = new Stack<Dictionary<WTableRow, short>>();
			}
			return m_tableRowWidthStack;
		}
	}

	public List<short> MaximumTableRowWidth
	{
		get
		{
			if (m_tableMaxRowWidth == null)
			{
				m_tableMaxRowWidth = new List<short>();
			}
			return m_tableMaxRowWidth;
		}
	}

	public WordReaderBase(StreamsManager streamsManager)
	{
		m_streamsManager = streamsManager;
	}

	protected WordReaderBase()
	{
	}

	public virtual WordChunkType ReadChunk()
	{
		UnfreezeStreamPos();
		m_textChunk = string.Empty;
		m_chunkType = WordChunkType.Text;
		int num = CalculateChunkLength();
		if (m_chunkType != WordChunkType.DocumentEnd && m_chunkType != WordChunkType.EndOfSubdocText && num > 0)
		{
			m_startTextPos = m_endTextPos;
			m_statePositions.CurrentTextPosition += ReadAndParseTextChunk(num);
			m_endTextPos = m_startTextPos + m_textChunk.Length;
			UpdateChunkType();
		}
		bool flag = false;
		string textChunk = m_textChunk;
		for (int i = 0; i < textChunk.Length; i++)
		{
			if (textChunk[i] == '(')
			{
				flag = true;
				continue;
			}
			flag = false;
			break;
		}
		if (flag)
		{
			if (CHPXSprms[27145] != null)
			{
				m_chunkType = WordChunkType.Symbol;
			}
			else
			{
				m_chunkType = WordChunkType.Text;
			}
		}
		return m_chunkType;
	}

	public virtual IWordImageReader GetImageReader(WordDocument doc)
	{
		return m_docInfo.GetImageReader(m_streamsManager, GetPicLocation(), doc);
	}

	public virtual FileShapeAddress GetFSPA()
	{
		return null;
	}

	public DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase GetDrawingObject()
	{
		UnfreezeStreamPos();
		FileShapeAddress fSPA = GetFSPA();
		if (fSPA == null)
		{
			return null;
		}
		MsofbtSpContainer msofbtSpContainer = null;
		if (Escher.Containers.ContainsKey(fSPA.Spid))
		{
			msofbtSpContainer = Escher.Containers[fSPA.Spid] as MsofbtSpContainer;
		}
		DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase result = null;
		if (msofbtSpContainer != null)
		{
			switch (msofbtSpContainer.Shape.ShapeType)
			{
			case EscherShapeType.msosptTextBox:
				result = ReadTextBoxProps(msofbtSpContainer, fSPA);
				break;
			case EscherShapeType.msosptPictureFrame:
				result = ReadPictureProps(msofbtSpContainer, fSPA);
				break;
			}
		}
		return result;
	}

	public FormField GetFormField(FieldType fieldType)
	{
		m_streamsManager.DataStream.Position = GetPicLocation();
		return new FormField(fieldType, m_streamsManager.DataReader);
	}

	public virtual FieldDescriptor GetFld()
	{
		throw new NotImplementedException();
	}

	public bool ReadWatermark(WordDocument doc, WTextBody m_textBody)
	{
		bool result = false;
		HeaderFooter headerFooter = null;
		if (m_textBody != null && m_textBody is HeaderFooter)
		{
			headerFooter = m_textBody as HeaderFooter;
		}
		if (this is WordHeaderFooterReader)
		{
			UnfreezeStreamPos();
			FileShapeAddress fSPA = GetFSPA();
			if (fSPA == null)
			{
				return false;
			}
			MsofbtSpContainer msofbtSpContainer = null;
			if (Escher.Containers.ContainsKey(fSPA.Spid))
			{
				msofbtSpContainer = Escher.Containers[fSPA.Spid] as MsofbtSpContainer;
			}
			if (msofbtSpContainer != null && IsWatermark(msofbtSpContainer))
			{
				result = true;
				if (headerFooter != null && headerFooter.Watermark != null && headerFooter.Watermark.Type == WatermarkType.NoWatermark)
				{
					if (msofbtSpContainer.Shape.ShapeType == EscherShapeType.msosptTextPlainText)
					{
						ReadTextWatermark(msofbtSpContainer, doc, fSPA, headerFooter);
						(headerFooter.Watermark as TextWatermark).Height = fSPA.Height / 20;
						(headerFooter.Watermark as TextWatermark).Width = fSPA.Width / 20;
						doc.SetTriggerElement(ref doc.m_supportedElementFlag_2, 17);
					}
					else if (msofbtSpContainer.Shape.ShapeType == EscherShapeType.msosptPictureFrame || msofbtSpContainer.Shape.ShapeType == EscherShapeType.msosptCustomShape)
					{
						ReadPictureWatermark(msofbtSpContainer, doc, fSPA, headerFooter);
						doc.SetTriggerElement(ref doc.m_supportedElementFlag_2, 0);
					}
					headerFooter.Watermark.ShapeId = fSPA.Spid;
				}
				else
				{
					Escher.RemoveContainerBySpid(msofbtSpContainer.Shape.ShapeId, isHeaderContainer: true);
				}
			}
		}
		return result;
	}

	public BookmarkInfo[] GetBookmarks()
	{
		m_bookmarks = m_docInfo.TablesData.GetBookmarks();
		return m_bookmarks;
	}

	public bool SubdocumentExist()
	{
		bool result = true;
		long position = m_streamsManager.MainStream.Position;
		if (m_statePositions.IsEndOfText(position) || GetChunkEndPosition(position) < 0)
		{
			result = false;
		}
		return result;
	}

	internal virtual void Close()
	{
		if (m_streamsManager != null)
		{
			m_streamsManager.CloseStg();
			m_streamsManager = null;
		}
		if (m_docInfo != null)
		{
			m_docInfo.Close();
			m_docInfo = null;
		}
		m_styleSheet = null;
		m_statePositions = null;
		m_chpx = null;
		m_papx = null;
		m_bookmarks = null;
		m_currentBookmark = null;
		m_bookmarkAfterParaEnd = null;
		if (m_tableRowWidthStack != null)
		{
			m_tableRowWidthStack.Clear();
			m_tableRowWidthStack = null;
		}
		if (m_tableMaxRowWidth != null)
		{
			m_tableMaxRowWidth.Clear();
			m_tableMaxRowWidth = null;
		}
	}

	public virtual void FreezeStreamPos()
	{
		if (!m_bStreamPosSaved)
		{
			m_iSavedStreamPosition = m_streamsManager.MainStream.Position;
			m_bStreamPosSaved = true;
		}
	}

	public virtual void UnfreezeStreamPos()
	{
		if (m_bStreamPosSaved)
		{
			m_streamsManager.MainStream.Position = m_iSavedStreamPosition;
			m_bStreamPosSaved = false;
		}
	}

	public bool HasList()
	{
		return m_docInfo.TablesData.HasList();
	}

	internal void RestoreBookmark()
	{
		if (m_currentBookmark != null)
		{
			int index = m_currentBookmark.Index;
			if (m_isBookmarkStart)
			{
				m_bookmarks[index].StartPos = m_currentBookmark.StartPos;
				m_bookmarks[index].StartCellIndex = m_currentBookmark.StartCellIndex;
			}
			else
			{
				m_bookmarks[index].EndPos = m_currentBookmark.EndPos;
				m_bookmarks[index].EndCellIndex = m_currentBookmark.EndCellIndex;
			}
			m_currentBookmark = null;
		}
	}

	protected void UpdateBookmarks()
	{
		if (m_bookmarks == null)
		{
			m_bookmarks = m_docInfo.TablesData.GetBookmarks();
		}
	}

	protected virtual void InitClass()
	{
		if (m_styleSheet == null)
		{
			m_styleSheet = new WordStyleSheet();
		}
		m_currStyleIndex = m_styleSheet.DefaultStyleIndex;
	}

	protected virtual long GetChunkEndPosition(long iCurrentPos)
	{
		if (!m_statePositions.IsFirstPass(iCurrentPos))
		{
			UpdateEndPositions(iCurrentPos);
		}
		return m_statePositions.GetMinEndPos(iCurrentPos);
	}

	protected virtual void UpdateEndPositions(long iEndPos)
	{
		if (m_statePositions.UpdateCHPxEndPos(iEndPos))
		{
			UpdateCharacterProperties();
		}
		if (m_statePositions.UpdatePAPxEndPos(iEndPos))
		{
			UpdateParagraphProperties();
		}
	}

	protected virtual void UpdateChunkType()
	{
		if (m_textChunk.Length > 1)
		{
			m_chunkType = WordChunkType.Text;
		}
		else if (m_textChunk.Length == 1)
		{
			switch (m_textChunk[0])
			{
			case '\u0005':
				m_chunkType = WordChunkType.Annotation;
				break;
			case '\r':
				if (PAPXSprms.GetInt(26185, 1) > 1)
				{
					if (PAPXSprms.GetBoolean(9292, defValue: false))
					{
						m_chunkType = WordChunkType.TableRow;
					}
					else if (PAPXSprms.GetByte(9291, 0) == 1)
					{
						if (m_streamsManager.MainStream.Position >= m_statePositions.m_iEndPAPxPos)
						{
							m_chunkType = WordChunkType.TableCell;
						}
						else
						{
							m_chunkType = WordChunkType.Text;
						}
					}
					else
					{
						m_chunkType = WordChunkType.ParagraphEnd;
					}
				}
				else if (Array.BinarySearch(m_docInfo.TablesData.SectionsTable.Positions, CurrentTextPosition) > 0 && CurrentTextPosition != m_docInfo.Fib.CcpText)
				{
					m_chunkType = WordChunkType.SectionEnd;
				}
				else
				{
					m_chunkType = WordChunkType.ParagraphEnd;
				}
				break;
			case '\u0001':
				m_chunkType = WordChunkType.Image;
				break;
			case '\b':
				m_chunkType = WordChunkType.Shape;
				break;
			case '\f':
			{
				int[] positions = m_docInfo.FkpData.Tables.SectionsTable.Positions;
				int charPos = 0;
				if (m_statePositions is MainStatePositions)
				{
					charPos = positions[(m_statePositions as MainStatePositions).SectionIndex + 1];
				}
				else if (m_statePositions is HFStatePositions)
				{
					charPos = positions[(m_statePositions as HFStatePositions).SectionIndex + 1];
				}
				if (m_docInfo.FkpData.Tables.ConvertCharPosToFileCharPos((uint)charPos) == m_streamsManager.MainStream.Position)
				{
					m_chunkType = WordChunkType.SectionEnd;
				}
				else
				{
					m_chunkType = WordChunkType.PageBreak;
				}
				break;
			}
			case '\u000e':
				m_chunkType = WordChunkType.ColumnBreak;
				break;
			case '\a':
				if ((PAPXSprms.GetBoolean(9239, defValue: false) && PAPXSprms.GetBoolean(9238, defValue: false)) || PAPXSprms.GetByteArray(29706) != null)
				{
					m_chunkType = WordChunkType.TableRow;
				}
				else if (PAPXSprms.GetBoolean(9238, defValue: false))
				{
					m_chunkType = WordChunkType.TableCell;
				}
				else if (m_streamsManager.MainStream.Position == m_statePositions.m_iEndPAPxPos)
				{
					m_chunkType = WordChunkType.ParagraphEnd;
				}
				else
				{
					m_chunkType = WordChunkType.Table;
				}
				break;
			case '\u0002':
				m_chunkType = WordChunkType.Footnote;
				break;
			case '\u0013':
				m_chunkType = WordChunkType.FieldBeginMark;
				break;
			case '\u0014':
				m_chunkType = WordChunkType.FieldSeparator;
				break;
			case '\u0015':
				m_chunkType = WordChunkType.FieldEndMark;
				break;
			case '\v':
				m_chunkType = WordChunkType.LineBreak;
				break;
			case '(':
				if (CHPXSprms[27145] != null)
				{
					m_chunkType = WordChunkType.Symbol;
				}
				else
				{
					m_chunkType = WordChunkType.Text;
				}
				break;
			case '\0':
				m_chunkType = WordChunkType.CurrentPageNumber;
				break;
			default:
				m_chunkType = WordChunkType.Text;
				break;
			}
		}
		else if (m_textChunk.Length == 0)
		{
			m_chunkType = WordChunkType.Text;
		}
	}

	protected void UpdateCharacterProperties()
	{
		m_chpx = m_statePositions.CurrentChpx;
	}

	protected void UpdateParagraphProperties()
	{
		ParagraphPropertyException currentPapx = m_statePositions.CurrentPapx;
		ParagraphPropertyException ex = null;
		if (currentPapx.PropertyModifiers[26181] != null || currentPapx.PropertyModifiers[26182] != null)
		{
			byte[] byteArray = currentPapx.PropertyModifiers.GetByteArray(26182);
			if (byteArray == null)
			{
				byteArray = currentPapx.PropertyModifiers.GetByteArray(26181);
			}
			int num = BitConverter.ToInt32(byteArray, 0);
			m_streamsManager.DataStream.Position = num;
			byteArray = new byte[2];
			m_streamsManager.DataStream.Read(byteArray, 0, byteArray.Length);
			short iCount = BitConverter.ToInt16(byteArray, 0);
			ex = new ParagraphPropertyException(m_streamsManager.DataStream, iCount, isHugePapx: true);
		}
		if (ex == null)
		{
			ex = currentPapx;
		}
		m_currStyleIndex = currentPapx.StyleIndex;
		_ = CurrentStyle;
		m_papx = ex;
	}

	protected void ReadChunkString(int length)
	{
		if (length < 1)
		{
			throw new ArgumentOutOfRangeException("length must be larger than 0");
		}
		byte[] array = new byte[length];
		m_streamsManager.MainStream.Read(array, 0, length);
		if (Encoding == Encoding.UTF8)
		{
			m_textChunk = DocIOEncoding.GetString(array);
		}
		else
		{
			m_textChunk = Encoding.GetString(array, 0, array.Length);
		}
	}

	protected int CalcCP(int startPos, int length)
	{
		uint num = m_docInfo.TablesData.ConvertFCToCP((uint)m_streamsManager.MainStream.Position);
		uint num2 = m_docInfo.TablesData.ConvertFCToCP((uint)startPos);
		return (int)(num - num2 - length);
	}

	private bool IsWatermark(MsofbtSpContainer spContainer)
	{
		bool result = false;
		byte[] complexPropValue = spContainer.GetComplexPropValue(896);
		if (complexPropValue != null)
		{
			string @string = Encoding.Unicode.GetString(complexPropValue, 0, complexPropValue.Length);
			if (@string.StartsWith("WordPictureWatermark") || @string.StartsWith("PowerPlusWaterMarkObject"))
			{
				result = true;
			}
		}
		return result;
	}

	private TextBoxShape ReadTextBoxProps(MsofbtSpContainer spContainer, FileShapeAddress fspa)
	{
		TextBoxShape textBoxShape = new TextBoxShape();
		InitBaseShapeProps(fspa, textBoxShape, spContainer);
		InitTextBoxProps(textBoxShape, spContainer);
		return textBoxShape;
	}

	private PictureShape ReadPictureProps(MsofbtSpContainer spContainer, FileShapeAddress fspa)
	{
		_Blip blipFromShapeContainer = MsofbtSpContainer.GetBlipFromShapeContainer(spContainer);
		PictureShape pictureShape = null;
		if (blipFromShapeContainer != null)
		{
			try
			{
				pictureShape = new PictureShape(blipFromShapeContainer.ImageRecord);
			}
			catch (ArgumentException)
			{
				throw new ArgumentException("Document image format is incorrect.");
			}
			InitBaseShapeProps(fspa, pictureShape, spContainer);
			InitPictureProps(pictureShape, spContainer);
		}
		return pictureShape;
	}

	private void ReadTextWatermark(MsofbtSpContainer spContainer, WordDocument doc, FileShapeAddress fspa, HeaderFooter header)
	{
		TextWatermark textWatermark = header.InsertWatermark(WatermarkType.TextWatermark) as TextWatermark;
		textWatermark.SetOwner(header);
		byte[] complexPropValue = spContainer.GetComplexPropValue(192);
		if (complexPropValue != null)
		{
			textWatermark.Text = Encoding.Unicode.GetString(complexPropValue, 0, complexPropValue.Length);
		}
		textWatermark.Text = ((textWatermark.Text == null) ? string.Empty : textWatermark.Text.Replace("\0", string.Empty));
		uint propertyValue = spContainer.GetPropertyValue(195);
		if (propertyValue != uint.MaxValue)
		{
			textWatermark.Size = propertyValue >> 16;
		}
		complexPropValue = spContainer.GetComplexPropValue(197);
		if (complexPropValue != null)
		{
			textWatermark.FontName = Encoding.Unicode.GetString(complexPropValue, 0, complexPropValue.Length);
		}
		textWatermark.FontName = ((textWatermark.FontName == null) ? string.Empty : textWatermark.FontName.Replace("\0", string.Empty));
		propertyValue = spContainer.GetPropertyValue(385);
		if (propertyValue != uint.MaxValue)
		{
			textWatermark.Color = WordColor.ConvertRGBToColor(propertyValue);
		}
		propertyValue = spContainer.GetPropertyValue(386);
		if (propertyValue == uint.MaxValue)
		{
			textWatermark.Semitransparent = false;
		}
		propertyValue = spContainer.GetPropertyValue(4);
		if (propertyValue == uint.MaxValue)
		{
			textWatermark.Layout = WatermarkLayout.Horizontal;
		}
		else
		{
			textWatermark.Rotation = (int)propertyValue / 65536;
		}
		UpdateTextWatermarkPositions(textWatermark, spContainer, fspa);
		if (doc.Watermark != null && doc.Watermark.Type == WatermarkType.NoWatermark)
		{
			doc.Watermark = textWatermark;
		}
	}

	private void UpdateTextWatermarkPositions(TextWatermark textWatermark, MsofbtSpContainer spContainer, FileShapeAddress fspa)
	{
		textWatermark.VerticalPosition = (float)fspa.YaTop / 20f;
		textWatermark.HorizontalPosition = (float)fspa.XaLeft / 20f;
		if (spContainer.ShapePosition.YRelTo != uint.MaxValue)
		{
			textWatermark.VerticalOrigin = (VerticalOrigin)spContainer.ShapePosition.YRelTo;
		}
		if (spContainer.ShapePosition.XRelTo != uint.MaxValue)
		{
			textWatermark.HorizontalOrigin = (HorizontalOrigin)spContainer.ShapePosition.XRelTo;
		}
		if (spContainer.ShapePosition.XAlign != uint.MaxValue)
		{
			textWatermark.HorizontalAlignment = (ShapeHorizontalAlignment)spContainer.ShapePosition.XAlign;
		}
		if (spContainer.ShapePosition.YAlign != uint.MaxValue)
		{
			textWatermark.VerticalAlignment = (ShapeVerticalAlignment)spContainer.ShapePosition.YAlign;
		}
		textWatermark.TextWrappingStyle = fspa.TextWrappingStyle;
	}

	private void ReadPictureWatermark(MsofbtSpContainer spContainer, WordDocument doc, FileShapeAddress fspa, HeaderFooter header)
	{
		PictureWatermark pictureWatermark = header.InsertWatermark(WatermarkType.PictureWatermark) as PictureWatermark;
		pictureWatermark.SetOwner(header);
		if (spContainer.Pib > 0)
		{
			int index = spContainer.Pib - 1;
			if (Escher.m_msofbtDggContainer.BstoreContainer.Children[index] is MsofbtBSE { Blip: not null } msofbtBSE)
			{
				pictureWatermark.WordPicture.LoadImage(msofbtBSE.Blip.ImageRecord);
				pictureWatermark.OriginalPib = spContainer.Pib;
				ApplyShapeProperties(pictureWatermark.WordPicture, fspa, spContainer.ShapePosition);
			}
		}
		uint propertyValue = spContainer.GetPropertyValue(265);
		uint propertyValue2 = spContainer.GetPropertyValue(264);
		pictureWatermark.Washout = ((propertyValue != uint.MaxValue || propertyValue2 != uint.MaxValue) ? true : false);
		if (doc.Watermark != null && doc.Watermark.Type == WatermarkType.NoWatermark)
		{
			doc.Watermark = pictureWatermark;
		}
	}

	private void ApplyShapeProperties(WPicture picture, FileShapeAddress fspa, MsofbtTertiaryFOPT shapePosition)
	{
		picture.Height = (float)fspa.Height / 20f;
		picture.Width = (float)fspa.Width / 20f;
		picture.VerticalPosition = (float)fspa.YaTop / 20f;
		picture.HorizontalPosition = (float)fspa.XaLeft / 20f;
		if (shapePosition.YRelTo != uint.MaxValue)
		{
			picture.VerticalOrigin = (VerticalOrigin)shapePosition.YRelTo;
		}
		if (shapePosition.XRelTo != uint.MaxValue)
		{
			picture.HorizontalOrigin = (HorizontalOrigin)shapePosition.XRelTo;
		}
		if (shapePosition.XAlign != uint.MaxValue)
		{
			picture.HorizontalAlignment = (ShapeHorizontalAlignment)shapePosition.XAlign;
		}
		if (shapePosition.YAlign != uint.MaxValue)
		{
			picture.VerticalAlignment = (ShapeVerticalAlignment)shapePosition.YAlign;
		}
		picture.SetTextWrappingStyleValue(fspa.TextWrappingStyle);
		picture.TextWrappingType = fspa.TextWrappingType;
		picture.IsBelowText = fspa.IsBelowText;
		picture.ShapeId = fspa.Spid;
	}

	private void InitBaseShapeProps(FileShapeAddress fspa, DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase shape, MsofbtSpContainer container)
	{
		shape.ShapeProps.Height = fspa.Height;
		shape.ShapeProps.RelHrzPos = fspa.RelHrzPos;
		shape.ShapeProps.RelVrtPos = fspa.RelVrtPos;
		shape.ShapeProps.Spid = fspa.Spid;
		shape.ShapeProps.TextWrappingStyle = fspa.TextWrappingStyle;
		shape.ShapeProps.TextWrappingType = fspa.TextWrappingType;
		shape.ShapeProps.Width = fspa.Width;
		shape.ShapeProps.XaLeft = fspa.XaLeft;
		shape.ShapeProps.XaRight = fspa.XaRight;
		shape.ShapeProps.YaBottom = fspa.YaBottom;
		shape.ShapeProps.YaTop = fspa.YaTop;
		shape.ShapeProps.Spid = fspa.Spid;
		shape.ShapeProps.IsHeaderShape = this is WordHeaderFooterReader;
		if (container.ShapePosition != null)
		{
			switch (container.ShapePosition.XAlign)
			{
			case 1u:
				shape.ShapeProps.HorizontalAlignment = ShapeHorizontalAlignment.Left;
				break;
			case 2u:
				shape.ShapeProps.HorizontalAlignment = ShapeHorizontalAlignment.Center;
				break;
			case 3u:
				shape.ShapeProps.HorizontalAlignment = ShapeHorizontalAlignment.Right;
				break;
			}
			switch (container.ShapePosition.YAlign)
			{
			case 1u:
				shape.ShapeProps.VerticalAlignment = ShapeVerticalAlignment.Top;
				break;
			case 2u:
				shape.ShapeProps.VerticalAlignment = ShapeVerticalAlignment.Center;
				break;
			case 3u:
				shape.ShapeProps.VerticalAlignment = ShapeVerticalAlignment.Bottom;
				break;
			case 4u:
				shape.ShapeProps.VerticalAlignment = ShapeVerticalAlignment.Inside;
				break;
			case 5u:
				shape.ShapeProps.VerticalAlignment = ShapeVerticalAlignment.Outside;
				break;
			}
			if (container.ShapePosition.XRelTo != uint.MaxValue)
			{
				shape.ShapeProps.RelHrzPos = (HorizontalOrigin)container.ShapePosition.XRelTo;
			}
			if (container.ShapePosition.YRelTo != uint.MaxValue)
			{
				shape.ShapeProps.RelVrtPos = (VerticalOrigin)container.ShapePosition.YRelTo;
			}
		}
		uint propertyValue = container.GetPropertyValue(959);
		if (propertyValue != uint.MaxValue)
		{
			shape.ShapeProps.IsBelowText = (propertyValue & 0x20) == 32;
		}
		else
		{
			shape.ShapeProps.IsBelowText = false;
		}
	}

	private void InitTextBoxProps(TextBoxShape shape, MsofbtSpContainer container)
	{
		uint propertyValue = container.GetPropertyValue(459);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.TxbxLineWidth = (float)propertyValue / 12700f;
		}
		propertyValue = container.GetPropertyValue(461);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.LineStyle = (TextBoxLineStyle)propertyValue;
		}
		propertyValue = container.GetPropertyValue(462);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.LineDashing = (LineDashing)propertyValue;
		}
		propertyValue = container.GetPropertyValue(133);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.WrapText = (WrapMode)propertyValue;
		}
		propertyValue = container.GetPropertyValue(385);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.FillColor = WordColor.ConvertRGBToColor(propertyValue);
		}
		propertyValue = container.GetPropertyValue(448);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.LineColor = WordColor.ConvertRGBToColor(propertyValue);
		}
		propertyValue = container.GetPropertyValue(447);
		if ((propertyValue & 0x10) != 16)
		{
			shape.TextBoxProps.FillColor = Color.Empty;
		}
		propertyValue = container.GetPropertyValue(511);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.NoLine = (propertyValue & 8) == 0;
		}
		propertyValue = container.GetPropertyValue(191);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.FitShapeToText = (propertyValue & 2) != 0;
		}
		shape.TextBoxProps.TXID = container.GetPropertyValue(128);
		if (container.ShapePosition != null)
		{
			if (container.ShapePosition.XAlign != uint.MaxValue)
			{
				shape.TextBoxProps.HorizontalAlignment = (ShapeHorizontalAlignment)container.ShapePosition.XAlign;
			}
			if (container.ShapePosition.YAlign != uint.MaxValue)
			{
				shape.TextBoxProps.VerticalAlignment = (ShapeVerticalAlignment)container.ShapePosition.YAlign;
			}
			if (container.ShapePosition.XRelTo != uint.MaxValue)
			{
				shape.TextBoxProps.RelHrzPos = (HorizontalOrigin)container.ShapePosition.XRelTo;
			}
			if (container.ShapePosition.YRelTo != uint.MaxValue)
			{
				shape.TextBoxProps.RelVrtPos = (VerticalOrigin)container.ShapePosition.YRelTo;
			}
		}
		propertyValue = container.GetPropertyValue(129);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.LeftMargin = propertyValue;
		}
		propertyValue = container.GetPropertyValue(131);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.RightMargin = propertyValue;
		}
		propertyValue = container.GetPropertyValue(130);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.TopMargin = propertyValue;
		}
		propertyValue = container.GetPropertyValue(132);
		if (propertyValue != uint.MaxValue)
		{
			shape.TextBoxProps.BottomMargin = propertyValue;
		}
	}

	private void InitPictureProps(PictureShape pictShape, MsofbtSpContainer spContainer)
	{
		byte[] complexPropValue = spContainer.GetComplexPropValue(897);
		if (complexPropValue != null)
		{
			pictShape.PictureProps.AlternativeText = Encoding.Unicode.GetString(complexPropValue, 0, complexPropValue.Length).Replace("\0", string.Empty);
		}
		complexPropValue = spContainer.GetComplexPropValue(896);
		if (complexPropValue != null)
		{
			pictShape.PictureProps.AlternativeText = Encoding.Unicode.GetString(complexPropValue, 0, complexPropValue.Length).Replace("\0", string.Empty);
		}
	}

	private int ReadAndParseTextChunk(int iLength)
	{
		ReadChunkString(iLength);
		if (m_textChunk.Length > 1)
		{
			int num = -1;
			if (m_textChunk.Length >= 10 && CHPXSprms.GetBoolean(2133, defValue: false) && CHPXSprms[27139] == null)
			{
				m_textChunk = string.Empty;
				return iLength;
			}
			if (m_textChunk.Length > 10 && IsZeroChunk())
			{
				m_textChunk = string.Empty;
				return iLength;
			}
			m_textChunk = m_textChunk.Trim(new char[1]);
			if ((num = m_textChunk.IndexOfAny(SpecialCharacters.SpecialSymbolArr)) > -1)
			{
				if (num == 0)
				{
					num = 1;
				}
				m_textChunk = m_textChunk.Substring(0, num);
				m_streamsManager.MainStream.Position -= iLength - num * EncodingCharSize;
				return num;
			}
		}
		return iLength;
	}

	private int CheckSpecCharacters(int iLength)
	{
		int num = -1;
		if ((num = m_textChunk.IndexOfAny(SpecialCharacters.SpecialSymbolArr)) > -1)
		{
			if (num == 0)
			{
				num = 1;
			}
			m_textChunk = m_textChunk.Substring(0, num);
			m_streamsManager.MainStream.Position -= iLength - num * EncodingCharSize;
			iLength = num;
		}
		return iLength;
	}

	private bool IsZeroChunk()
	{
		bool result = false;
		if (m_textChunk[0] == '\0')
		{
			result = true;
			for (int i = 1; i < m_textChunk.Length - 1; i++)
			{
				if (m_textChunk[i] != m_textChunk[i + 1])
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private int CalculateChunkLength()
	{
		long num = m_streamsManager.MainStream.Position;
		if (m_statePositions.IsEndOfText(num))
		{
			m_chunkType = WordChunkType.DocumentEnd;
			return 0;
		}
		long chunkEndPosition = GetChunkEndPosition(num);
		if (num < m_statePositions.m_iStartPieceTablePos)
		{
			long num2 = (m_streamsManager.MainStream.Position = m_statePositions.m_iStartPieceTablePos);
			num = num2;
		}
		if (m_statePositions.IsEndOfSubdocItemText(num))
		{
			m_chunkType = WordChunkType.EndOfSubdocText;
			return 0;
		}
		if (chunkEndPosition < 0 || m_statePositions.IsEndOfText(num))
		{
			m_chunkType = WordChunkType.DocumentEnd;
			return 0;
		}
		if (this is WordSubdocumentReader && (this as WordSubdocumentReader).IsNextItemPos)
		{
			m_chunkType = WordChunkType.Text;
			return 0;
		}
		if (m_bookmarks != null && m_bookmarks.Length != 0)
		{
			return GetBookmarkChunkLen((int)num, (int)chunkEndPosition);
		}
		return (int)(chunkEndPosition - num);
	}

	private bool IsNextItem()
	{
		if (this is WordEndnoteReader)
		{
			return (this as WordEndnoteReader).IsNextItem;
		}
		if (this is WordFootnoteReader)
		{
			return (this as WordFootnoteReader).IsNextItem;
		}
		return false;
	}

	private int GetFldChunkLen(int curPos, int endPos)
	{
		int result = endPos - curPos;
		DocIOSortedList<int, FieldDescriptor> fieldsForSubDoc = m_docInfo.TablesData.Fields.GetFieldsForSubDoc(m_type);
		if (fieldsForSubDoc != null)
		{
			int currentTextPosition = CurrentTextPosition;
			List<int> list = new List<int>();
			foreach (int key in fieldsForSubDoc.Keys)
			{
				if (key >= currentTextPosition || key <= endPos)
				{
					list.Add(key);
				}
			}
			if (list.Count > 0)
			{
				list.Sort();
				result = list[0] - currentTextPosition;
				result += ((result == 0) ? 1 : 0);
			}
		}
		return result;
	}

	private bool CheckCurTextChunk(int curChunkPosLen)
	{
		m_isCellMark = false;
		if (curChunkPosLen < 0)
		{
			return false;
		}
		if (m_chunkType == WordChunkType.TableCell)
		{
			m_cellCounter++;
			m_isCellMark = true;
		}
		else if (m_chunkType == WordChunkType.TableRow)
		{
			m_cellCounter = 0;
		}
		if (m_textChunk == string.Empty || IsZeroChunk() || (m_textChunk.Length > 10 && CHPXSprms.GetBoolean(2133, defValue: false) && CHPXSprms[27139] == null))
		{
			m_textChunk = string.Empty;
			return false;
		}
		return true;
	}

	private int CalculateBkmkChunkLen(int startStreamPos, int endStreamPos)
	{
		int charPos = (m_isBookmarkStart ? m_currentBookmark.StartPos : m_currentBookmark.EndPos);
		int num = (int)m_docInfo.TablesData.ConvertCharPosToFileCharPos((uint)charPos);
		if (num > endStreamPos)
		{
			num = endStreamPos;
		}
		return num - startStreamPos;
	}

	private int GetBookmarkChunkLen(long curDocStreamPos, long endDocStreamPos)
	{
		m_currentBookmark = null;
		int num = (int)(endDocStreamPos - curDocStreamPos);
		int num2 = CheckAndGetCurChunkLen(num);
		if (num2 == -1)
		{
			return num;
		}
		int currentTextPosition = CurrentTextPosition;
		int num3 = currentTextPosition + num2;
		if (HasTableBody)
		{
			CheckTableBookmark(currentTextPosition, num3);
			if (m_currentBookmark != null)
			{
				if (!m_isCellMark)
				{
					return 0;
				}
				return num;
			}
		}
		SetCurrentBookmark(currentTextPosition, num3);
		if (m_currentBookmark != null)
		{
			int charPos = (m_isBookmarkStart ? m_currentBookmark.StartPos : m_currentBookmark.EndPos);
			int num4 = (int)m_docInfo.TablesData.ConvertCharPosToFileCharPos((uint)charPos);
			if (num4 > endDocStreamPos)
			{
				num4 = (int)endDocStreamPos;
			}
			num = num4 - (int)curDocStreamPos;
		}
		return num;
	}

	private void CheckTableBookmark(int startTextPos, int endTextPos)
	{
		int bookmarkIndex = -1;
		BookmarkInfo bookmarkInfo = null;
		int i = 0;
		for (int num = m_bookmarks.Length; i < num; i++)
		{
			bookmarkInfo = m_bookmarks[i];
			if (bookmarkInfo.StartCellIndex == m_cellCounter && m_isCellMark)
			{
				if (bookmarkInfo.StartPos <= startTextPos && bookmarkInfo.EndPos > startTextPos)
				{
					m_isBookmarkStart = true;
					bookmarkIndex = i;
					break;
				}
			}
			else if (bookmarkInfo.EndCellIndex == m_cellCounter - 1 && m_isCellMark && bookmarkInfo.EndPos <= endTextPos && bookmarkInfo.StartCellIndex == -1)
			{
				m_isBookmarkStart = false;
				bookmarkIndex = i;
				break;
			}
		}
		DisableBookmark(bookmarkIndex);
	}

	private void SetCurrentBookmark(int startPos, int endPos)
	{
		int bookmarkIndex = -1;
		int num = int.MaxValue;
		for (int i = 0; i < m_bookmarks.Length; i++)
		{
			BookmarkInfo bookmarkInfo = m_bookmarks[i];
			bool flag = true;
			while (true)
			{
				int num2 = (flag ? bookmarkInfo.StartPos : bookmarkInfo.EndPos);
				if (num2 != int.MaxValue && num2 >= startPos && num2 < num && num2 <= endPos)
				{
					num = num2;
					bookmarkIndex = i;
					m_isBookmarkStart = flag;
				}
				if (!flag)
				{
					break;
				}
				flag = false;
			}
		}
		DisableBookmark(bookmarkIndex);
	}

	private void DisableBookmark(int bookmarkIndex)
	{
		if (bookmarkIndex != -1)
		{
			BookmarkInfo bookmarkInfo = m_bookmarks[bookmarkIndex];
			m_currentBookmark = bookmarkInfo.Clone();
			if (m_isBookmarkStart)
			{
				bookmarkInfo.StartPos = int.MaxValue;
				bookmarkInfo.StartCellIndex = -1;
			}
			else
			{
				bookmarkInfo.EndPos = int.MaxValue;
				bookmarkInfo.EndCellIndex = -1;
			}
		}
		else
		{
			m_currentBookmark = null;
		}
	}

	private int CheckAndGetCurChunkLen(int curChunkPosLen)
	{
		m_isCellMark = false;
		int result = -1;
		if (curChunkPosLen < 1)
		{
			return result;
		}
		long position = m_streamsManager.MainStream.Position;
		ReadChunkString(curChunkPosLen);
		UpdateChunkType();
		if (m_chunkType == WordChunkType.TableCell)
		{
			m_cellCounter++;
			m_isCellMark = true;
		}
		else if (m_chunkType == WordChunkType.TableRow)
		{
			m_cellCounter = 0;
		}
		if (m_textChunk == string.Empty || IsZeroChunk() || (m_textChunk.Length > 10 && CHPXSprms.GetBoolean(2133, defValue: false) && CHPXSprms[27139] == null))
		{
			result = -1;
		}
		else
		{
			CheckSpecCharacters(curChunkPosLen);
			result = m_textChunk.Length;
		}
		m_textChunk = string.Empty;
		m_chunkType = WordChunkType.Text;
		m_streamsManager.MainStream.Position = position;
		return result;
	}

	private int CheckForSymbols(int chunkLen)
	{
		int result = chunkLen;
		if (CHPXSprms[27145] != null && chunkLen > 1)
		{
			long position = m_streamsManager.MainStream.Position;
			ReadChunkString(chunkLen);
			if (m_textChunk[0] == '(')
			{
				result = 1;
			}
			m_streamsManager.MainStream.Position = position;
			m_textChunk = string.Empty;
		}
		return result;
	}

	private DocumentVersion GetDocVersion()
	{
		uint fibRgFcLcb97LcbDop = m_docInfo.Fib.FibRgFcLcb97LcbDop;
		if (fibRgFcLcb97LcbDop < 544)
		{
			return DocumentVersion.Word97;
		}
		if (fibRgFcLcb97LcbDop == 544)
		{
			return DocumentVersion.Word2000;
		}
		if (fibRgFcLcb97LcbDop <= 594)
		{
			return DocumentVersion.Word2002;
		}
		if (fibRgFcLcb97LcbDop <= 616)
		{
			return DocumentVersion.Word2003;
		}
		return DocumentVersion.Word2007;
	}

	internal SymbolDescriptor GetSymbolDescriptor()
	{
		byte[] byteArray = CHPXSprms.GetByteArray(27145);
		SymbolDescriptor symbolDescriptor = new SymbolDescriptor();
		if (byteArray != null)
		{
			symbolDescriptor.Parse(byteArray);
		}
		return symbolDescriptor;
	}

	internal string GetFontName(int wordSprmOption)
	{
		if (m_styleSheet.FontNamesList.Count == 0)
		{
			return string.Empty;
		}
		return m_styleSheet.FontNamesList[CHPXSprms.GetUShort(wordSprmOption, 0)];
	}

	internal int GetPicLocation()
	{
		int result = 0;
		if (CHPXSprms[27139] != null)
		{
			result = CHPXSprms.GetInt(27139, 0);
		}
		return result;
	}

	internal Dictionary<int, string> GetSTTBFRNames(byte[] sttb)
	{
		if (sttb == null)
		{
			return null;
		}
		int num = 0;
		bool flag = false;
		if (sttb.Length < 2)
		{
			return null;
		}
		ushort num2 = BitConverter.ToUInt16(sttb, num);
		num += 2;
		if (num2 == ushort.MaxValue)
		{
			flag = true;
		}
		if (sttb.Length < num + 2)
		{
			return null;
		}
		int num3 = BitConverter.ToUInt16(sttb, num);
		if (num3 >= 65535)
		{
			if (sttb.Length < num + 4)
			{
				return null;
			}
			num3 = BitConverter.ToInt32(sttb, num);
			if (num3 <= 0)
			{
				return null;
			}
			num += 4;
		}
		else
		{
			num += 2;
		}
		if (sttb.Length < num + 2)
		{
			return null;
		}
		ushort num4 = BitConverter.ToUInt16(sttb, num);
		num += 2;
		Dictionary<int, string> dictionary = new Dictionary<int, string>(num3);
		for (int i = 0; i < num3; i++)
		{
			int num5 = 0;
			string empty = string.Empty;
			_ = string.Empty;
			if (flag)
			{
				if (sttb.Length < num + 2)
				{
					return dictionary;
				}
				num5 = BitConverter.ToUInt16(sttb, num);
				num += 2;
				if (sttb.Length < num + 2 * num5)
				{
					return dictionary;
				}
				empty = Encoding.Unicode.GetString(sttb, num, 2 * num5);
				num += 2 * num5;
			}
			else
			{
				num5 = sttb[num];
				num++;
				if (sttb.Length < num + num5)
				{
					return dictionary;
				}
				if (Encoding == Encoding.UTF8)
				{
					byte[] array = new byte[num5];
					Buffer.BlockCopy(sttb, num, array, 0, num5);
					empty = DocIOEncoding.GetString(array);
				}
				else
				{
					empty = Encoding.GetString(sttb, num, num5);
				}
				num += num5;
			}
			if (sttb.Length < num + num4)
			{
				return dictionary;
			}
			Encoding.UTF8.GetString(sttb, num, num4);
			num += num4;
			dictionary[i] = empty;
		}
		return dictionary;
	}
}
