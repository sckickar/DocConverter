using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal abstract class WordWriterBase : IWordWriterBase
{
	private const int DEF_FIELDSHAPETYPE_VAL = 2;

	public StreamsManager m_streamsManager;

	public DocInfo m_docInfo;

	protected WordStyleSheet m_styleSheet;

	protected int m_nextPicLocation;

	protected CharacterPropertyException m_chpx;

	protected CharacterPropertyException m_breakChpx;

	protected ParagraphPropertyException m_papx;

	protected ListProperties m_listProperties;

	private Stack<FieldDescriptor> m_endStack = new Stack<FieldDescriptor>();

	private int m_iCountCell;

	private int m_currStyleIndex;

	protected int m_curTxbxId;

	protected int m_curPicId;

	private int m_curTxid;

	protected int m_textColIndex;

	protected int m_iStartText;

	protected BinaryWriter m_textWriter;

	private byte m_bFlag = 7;

	protected WordSubdocument m_type;

	public WordStyleSheet StyleSheet => m_styleSheet;

	public int CurrentStyleIndex
	{
		get
		{
			return m_currStyleIndex;
		}
		set
		{
			if (m_currStyleIndex != value)
			{
				if (value < 0 || value > m_styleSheet.StylesCount - 1)
				{
					throw new ArgumentOutOfRangeException("CurrentStyleIndex", $"value must be between 0 and {m_styleSheet.StylesCount - 1}");
				}
				m_currStyleIndex = value;
			}
		}
	}

	public CharacterPropertyException CHPX
	{
		get
		{
			return m_chpx;
		}
		set
		{
			m_chpx = value;
		}
	}

	public CharacterPropertyException BreakCHPX
	{
		get
		{
			return m_breakChpx;
		}
		set
		{
			m_breakChpx = value;
		}
	}

	public ParagraphPropertyException PAPX
	{
		get
		{
			return m_papx;
		}
		set
		{
			m_papx = value;
		}
	}

	public bool BreakCHPXStickProperties
	{
		get
		{
			return (m_bFlag & 1) != 0;
		}
		set
		{
			m_bFlag = (byte)((m_bFlag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public bool CHPXStickProperties
	{
		get
		{
			return (m_bFlag & 2) != 0;
		}
		set
		{
			m_bFlag = (byte)((m_bFlag & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public bool PAPXStickProperties
	{
		get
		{
			return (m_bFlag & 4) != 0;
		}
		set
		{
			m_bFlag = (byte)((m_bFlag & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	public ListProperties ListProperties
	{
		get
		{
			if (m_listProperties == null)
			{
				m_listProperties = new ListProperties(m_docInfo.TablesData.ListInfo, m_papx);
			}
			return m_listProperties;
		}
	}

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

	public StreamsManager StreamsManager => m_streamsManager;

	public BinaryWriter MainWriter
	{
		get
		{
			if (m_textWriter == null)
			{
				m_textWriter = new BinaryWriter(m_streamsManager.MainStream, m_docInfo.Fib.Encoding);
			}
			return m_textWriter;
		}
	}

	internal int NextTextId
	{
		get
		{
			m_curTxid += 65536;
			return m_curTxid;
		}
	}

	public virtual void WriteChunk(string textChunk)
	{
		bool cHPXStickProperties = CHPXStickProperties;
		bool pAPXStickProperties = PAPXStickProperties;
		bool breakCHPXStickProperties = BreakCHPXStickProperties;
		bool boolean = PAPX.PropertyModifiers.GetBoolean(9238, defValue: false);
		bool flag = PAPX.PropertyModifiers.GetByte(9291, 0) == 1;
		bool boolean2 = PAPX.PropertyModifiers.GetBoolean(9292, defValue: false);
		textChunk = textChunk.Replace(ControlChar.CrLf, ControlChar.CarriegeReturn);
		textChunk = textChunk.Replace(ControlChar.LineFeedChar, '\r');
		string[] array = textChunk.Split(ControlChar.CarriegeReturn.ToCharArray());
		int num = array.Length;
		if (num > 1)
		{
			CHPXStickProperties = true;
			PAPXStickProperties = true;
			BreakCHPXStickProperties = true;
			if (flag)
			{
				PAPX.PropertyModifiers.RemoveValue(9291);
			}
			if (boolean2)
			{
				PAPX.PropertyModifiers.RemoveValue(9292);
			}
		}
		for (int i = 0; i < num; i++)
		{
			WriteString(array[i]);
			if (i < num - 1)
			{
				WriteChar('\r');
			}
		}
		if (num > 1)
		{
			if (!cHPXStickProperties)
			{
				CHPX.PropertyModifiers.Clear();
			}
			if (flag)
			{
				PAPX.PropertyModifiers.SetBoolValue(9291, flag: true);
			}
			if (boolean2)
			{
				PAPX.PropertyModifiers.SetBoolValue(9292, flag: true);
			}
		}
		CHPXStickProperties = cHPXStickProperties;
		PAPXStickProperties = pAPXStickProperties;
		BreakCHPXStickProperties = breakCHPXStickProperties;
		if (boolean)
		{
			SetCellMark(PAPX.PropertyModifiers, value: true);
		}
	}

	public virtual void WriteSafeChunk(string textChunk)
	{
		bool boolean = PAPX.PropertyModifiers.GetBoolean(9238, defValue: false);
		WriteString(textChunk);
		if (!CHPXStickProperties)
		{
			CHPX.PropertyModifiers.Clear();
		}
		if (boolean)
		{
			SetCellMark(PAPX.PropertyModifiers, value: true);
		}
	}

	public void WriteCellMark(int nestingLevel)
	{
		if (nestingLevel == 1)
		{
			WriteMarker(WordChunkType.TableCell);
			return;
		}
		SetCellMark(PAPX.PropertyModifiers, value: true);
		SetTableNestingLevel(PAPX.PropertyModifiers, nestingLevel);
		if (PAPX.PropertyModifiers.HasSprm(9291))
		{
			PAPX.PropertyModifiers.RemoveValue(9291);
		}
		SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(9291);
		singlePropertyModifierRecord.BoolValue = true;
		PAPX.PropertyModifiers.InsertAt(singlePropertyModifierRecord, 0);
		WriteNestedMark();
	}

	public void WriteRowMark(int nestingLevel, int cellCount)
	{
		if (nestingLevel == 1)
		{
			m_iCountCell = cellCount;
			WriteMarker(WordChunkType.TableRow);
			return;
		}
		if (PAPX.PropertyModifiers[54789] == null)
		{
			byte[] array = new byte[24];
			TableBorders tableBorders = CreateTableBorders();
			for (int i = 0; i < 6; i++)
			{
				tableBorders[i].SaveBytes(array, i * 4);
			}
			PAPX.PropertyModifiers.SetByteArrayValue(54789, array);
		}
		SetCellMark(PAPX.PropertyModifiers, value: true);
		SetTableNestingLevel(PAPX.PropertyModifiers, nestingLevel);
		PAPX.PropertyModifiers.SetBoolValue(9291, flag: true);
		PAPX.PropertyModifiers.SetBoolValue(9292, flag: true);
		WriteNestedMark();
		SetCellMark(PAPX.PropertyModifiers, value: false);
	}

	public virtual void WriteMarker(WordChunkType chunkType)
	{
		switch (chunkType)
		{
		case WordChunkType.ParagraphEnd:
			WriteChar('\r');
			break;
		case WordChunkType.PageBreak:
			WriteChar('\f');
			break;
		case WordChunkType.Table:
			WriteChar('\a');
			break;
		case WordChunkType.TableCell:
			SetCellMark(PAPX.PropertyModifiers, value: true);
			WriteChar('\a');
			m_iCountCell++;
			break;
		case WordChunkType.TableRow:
			if (PAPX.PropertyModifiers[54789] == null)
			{
				byte[] array = new byte[24];
				TableBorders tableBorders = CreateTableBorders();
				for (int i = 0; i < 6; i++)
				{
					tableBorders[i].SaveBytes(array, i * 4);
				}
				PAPX.PropertyModifiers.SetByteArrayValue(54789, array);
			}
			SetCellMark(PAPX.PropertyModifiers, value: true);
			PAPX.PropertyModifiers.SetBoolValue(9239, flag: true);
			WriteChar('\a');
			SetCellMark(PAPX.PropertyModifiers, value: false);
			m_iCountCell = 0;
			break;
		case WordChunkType.FieldBeginMark:
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			WriteChar('\u0013');
			CHPX.PropertyModifiers.RemoveValue(2133);
			break;
		case WordChunkType.FieldSeparator:
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			WriteChar('\u0014');
			CHPX.PropertyModifiers.RemoveValue(2133);
			break;
		case WordChunkType.FieldEndMark:
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			WriteChar('\u0015');
			CHPX.PropertyModifiers.RemoveValue(2133);
			break;
		case WordChunkType.Tab:
			WriteChar('\t');
			break;
		case WordChunkType.Annotation:
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			WriteChar('\u0005');
			CHPX.PropertyModifiers.RemoveValue(2133);
			break;
		case WordChunkType.LineBreak:
			WriteChar('\v');
			break;
		case WordChunkType.Image:
			WriteChar('\u0001');
			break;
		case WordChunkType.Shape:
			CHPX.PropertyModifiers.SetIntValue(27139, 0);
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			WriteChar('\b');
			CHPX.PropertyModifiers.RemoveValue(2133);
			break;
		case WordChunkType.Symbol:
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			WriteChar('(');
			CHPX.PropertyModifiers.RemoveValue(2133);
			break;
		case WordChunkType.Footnote:
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			WriteChar('\u0002');
			CHPX.PropertyModifiers.RemoveValue(2133);
			break;
		case WordChunkType.Text:
		case WordChunkType.SectionEnd:
		case WordChunkType.ColumnBreak:
		case WordChunkType.DocumentEnd:
			break;
		}
	}

	private void SetCellMark(SinglePropertyModifierArray sprms, bool value)
	{
		if (value)
		{
			sprms.SetBoolValue(9238, flag: true);
		}
		else
		{
			sprms.RemoveValue(9238);
		}
	}

	private void SetTableNestingLevel(SinglePropertyModifierArray sprms, int value)
	{
		if (value > 0)
		{
			sprms.SetIntValue(26185, value);
		}
		else
		{
			sprms.RemoveValue(26185);
		}
	}

	public TableBorders CreateTableBorders()
	{
		return new TableBorders();
	}

	public void InsertStartField(string fieldcode, bool hasSeparator)
	{
		if (fieldcode == null || fieldcode.Length == 0)
		{
			throw new ArgumentException("fieldcode must be present.");
		}
		bool cHPXStickProperties = CHPXStickProperties;
		CHPXStickProperties = true;
		int textPos = GetTextPos();
		FieldDescriptor fieldDescriptor = WriteFieldStart(FieldTypeDefiner.GetFieldType(fieldcode));
		AddFieldDescriptor(fieldDescriptor, textPos);
		WriteSafeChunk(fieldcode);
		CHPXStickProperties = cHPXStickProperties;
		if (hasSeparator)
		{
			textPos = GetTextPos();
			FieldDescriptor fld = WriteFieldSeparator();
			AddFieldDescriptor(fld, textPos);
		}
		FieldDescriptor fieldDescriptor2 = new FieldDescriptor();
		fieldDescriptor2.HasSeparator = hasSeparator;
		if (WordDocument.DisableDateTimeUpdating && (fieldDescriptor.Type == FieldType.FieldDate || fieldDescriptor.Type == FieldType.FieldTime))
		{
			fieldDescriptor2.IsLocked = true;
		}
		m_endStack.Push(fieldDescriptor2);
	}

	public void InsertStartField(string fieldcode, WField field, bool hasSeparator)
	{
		bool cHPXStickProperties = CHPXStickProperties;
		CHPXStickProperties = true;
		int textPos = GetTextPos();
		FieldDescriptor fieldDescriptor = WriteFieldStart(field.FieldType);
		if (field.FieldType == FieldType.FieldUnknown)
		{
			fieldDescriptor.Type = (FieldType)field.SourceFieldType;
		}
		AddFieldDescriptor(fieldDescriptor, textPos);
		if (!string.IsNullOrEmpty(fieldcode))
		{
			WriteSafeChunk(fieldcode);
		}
		if (field.FieldType == FieldType.FieldRef || field.FieldType == FieldType.FieldPageRef || field.FieldType == FieldType.FieldNoteRef)
		{
			WriteNilPICFAndBinData(field);
		}
		CHPXStickProperties = cHPXStickProperties;
		if (hasSeparator)
		{
			textPos = GetTextPos();
			FieldDescriptor fld = WriteFieldSeparator();
			AddFieldDescriptor(fld, textPos);
		}
		FieldDescriptor fieldDescriptor2 = new FieldDescriptor();
		fieldDescriptor2.HasSeparator = true;
		if (WordDocument.DisableDateTimeUpdating && (field.FieldType == FieldType.FieldDate || field.FieldType == FieldType.FieldTime))
		{
			fieldDescriptor2.IsLocked = true;
		}
		m_endStack.Push(fieldDescriptor2);
	}

	public void InsertFieldSeparator()
	{
		int textPos = GetTextPos();
		FieldDescriptor fld = WriteFieldSeparator();
		AddFieldDescriptor(fld, textPos);
	}

	public void InsertEndField()
	{
		if (m_endStack.Count > 0)
		{
			FieldDescriptor fieldDescriptor = m_endStack.Pop();
			fieldDescriptor.FieldBoundary = 21;
			fieldDescriptor.IsNested = m_endStack.Count != 0;
			int textPos = GetTextPos();
			AddFieldDescriptor(fieldDescriptor, textPos);
			WriteFieldEnd();
		}
	}

	public void InsertFieldIndexEntry(string fieldCode)
	{
		bool cHPXStickProperties = CHPXStickProperties;
		CHPXStickProperties = true;
		WriteMarker(WordChunkType.FieldBeginMark);
		WriteSafeChunk(fieldCode);
		WriteMarker(WordChunkType.FieldEndMark);
		CHPX.PropertyModifiers.Clear();
		CHPXStickProperties = cHPXStickProperties;
	}

	public void InsertFormField(string fieldcode, FormField formField, WFormField wFormField)
	{
		bool cHPXStickProperties = CHPXStickProperties;
		CHPXStickProperties = true;
		bool flag = false;
		if (formField != null)
		{
			flag = formField.FormFieldType == FormFieldType.TextInput;
			if (formField.Params == 1)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		int textPos = GetTextPos();
		FieldDescriptor fld = WriteFieldStart(wFormField.FieldType);
		AddFieldDescriptor(fld, textPos);
		if (!string.IsNullOrEmpty(fieldcode))
		{
			WriteSafeChunk(fieldcode);
		}
		CHPXStickProperties = false;
		int value = (int)m_streamsManager.DataStream.Position;
		if (formField != null)
		{
			formField.Write(m_streamsManager.DataStream);
			CHPX.PropertyModifiers.SetBoolValue(2050, flag: true);
			CHPX.PropertyModifiers.SetIntValue(27139, value);
			CHPX.PropertyModifiers.SetBoolValue(2054, flag: true);
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			WriteMarker(WordChunkType.Image);
			CHPXStickProperties = true;
		}
		CHPXStickProperties = cHPXStickProperties;
		FieldDescriptor fieldDescriptor = new FieldDescriptor();
		fieldDescriptor.HasSeparator = flag;
		m_endStack.Push(fieldDescriptor);
	}

	public void InsertHyperlink(string displayText, string url, bool isLocalUrl)
	{
		InsertStartField(string.Format("HYPERLINK {0}\"{1}\"", (!isLocalUrl) ? "\\l " : "", url), hasSeparator: true);
		WriteChunk(displayText);
		InsertEndField();
	}

	public void InsertImage(WPicture picture)
	{
		if (picture.ImageRecord != null)
		{
			Size size = picture.ImageRecord.Size;
			InsertImage(picture, size.Height, size.Width);
		}
	}

	public void InsertImage(WPicture picture, int height, int width)
	{
		if (picture.ImageRecord != null)
		{
			m_nextPicLocation = (int)m_streamsManager.DataStream.Position;
			CHPX.PropertyModifiers.SetIntValue(27139, m_nextPicLocation);
			CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
			m_docInfo.ImageWriter.WriteImage(picture, height, width);
			WriteMarker(WordChunkType.Image);
			CHPX.PropertyModifiers.RemoveValue(2133);
		}
	}

	public void InsertShape(WPicture pict, PictureShapeProps pictProps)
	{
		bool flag = false;
		int textPos = GetTextPos();
		WriteMarker(WordChunkType.Shape);
		MsofbtSpContainer spContainer = null;
		if (Escher.Containers.ContainsKey(pictProps.Spid))
		{
			spContainer = Escher.Containers[pictProps.Spid] as MsofbtSpContainer;
		}
		int[] array = new int[Escher.Containers.Count];
		Escher.Containers.Keys.CopyTo(array, 0);
		if (pict.IsCloned)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (Escher.Containers[array[i]] is MsofbtSpContainer)
				{
					MsofbtSpContainer msofbtSpContainer = Escher.Containers[array[i]] as MsofbtSpContainer;
					if (msofbtSpContainer.Bse != null && msofbtSpContainer.Bse.Blip.ImageRecord == pict.ImageRecord)
					{
						pictProps.Spid = array[i];
						m_docInfo.TablesData.ArtObj.AddFSPA(pictProps, m_type, textPos);
						flag = true;
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
			SetFSPASpid(pictProps);
			AddPictContainer(pict, spContainer, pictProps);
			m_docInfo.TablesData.ArtObj.AddFSPA(pictProps, m_type, textPos);
		}
	}

	public int InsertTextBox(bool visible, WTextBoxFormat txbxFormat)
	{
		MsofbtSpContainer msofbtSpContainer = null;
		if (Escher.Containers.ContainsKey(txbxFormat.TextBoxShapeID))
		{
			msofbtSpContainer = Escher.Containers[txbxFormat.TextBoxShapeID] as MsofbtSpContainer;
		}
		if (msofbtSpContainer == null && txbxFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			return InsertInlineTextBox(visible, txbxFormat);
		}
		if (msofbtSpContainer == null || msofbtSpContainer.ShapeOptions.Txid == null)
		{
			AddTxBxContainer(visible, txbxFormat);
		}
		else
		{
			SyncTxBxContainer(msofbtSpContainer, visible, txbxFormat);
		}
		int textPos = GetTextPos();
		FileShapeAddress fspa = new FileShapeAddress();
		TextBoxPropertiesConverter.Import(fspa, txbxFormat);
		m_docInfo.TablesData.ArtObj.AddFSPA(fspa, m_type, textPos);
		WriteMarker(WordChunkType.Shape);
		return txbxFormat.TextBoxShapeID;
	}

	public int InsertInlineTextBox(bool visible, WTextBoxFormat txbxFormat)
	{
		txbxFormat.TextBoxShapeID = m_curTxbxId;
		FileShapeAddress fileShapeAddress = new FileShapeAddress();
		TextBoxPropertiesConverter.Import(fileShapeAddress, txbxFormat);
		AddTxBxContainer(visible, txbxFormat);
		InsertStartField(" SHAPE \\*MERGEFORMAT ", hasSeparator: true);
		int textPos = GetTextPos();
		fileShapeAddress.TxbxCount = 0;
		fileShapeAddress.IsAnchorLock = true;
		fileShapeAddress.TextWrappingStyle = TextWrappingStyle.InFrontOfText;
		m_docInfo.TablesData.ArtObj.AddFSPA(fileShapeAddress, m_type, textPos);
		WriteMarker(WordChunkType.Shape);
		m_nextPicLocation = (int)m_streamsManager.DataStream.Position;
		CHPX.PropertyModifiers.SetIntValue(27139, m_nextPicLocation);
		CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
		m_nextPicLocation = m_docInfo.ImageWriter.WriteInlineTxBxPicture(txbxFormat);
		WriteMarker(WordChunkType.Image);
		CHPX.PropertyModifiers.RemoveValue(2133);
		InsertEndField();
		return txbxFormat.TextBoxShapeID;
	}

	public void InsertShapeObject(ShapeObject shapeObj)
	{
		int num = -1;
		if (Escher.Containers.ContainsKey(shapeObj.FSPA.Spid))
		{
			BaseContainer baseContainer = Escher.Containers[shapeObj.FSPA.Spid];
			WTextBoxCollection autoShapeTextCollection = shapeObj.AutoShapeTextCollection;
			m_textColIndex = 0;
			baseContainer.SynchronizeIdent(autoShapeTextCollection, ref m_curTxbxId, ref m_curPicId, ref m_curTxid, ref m_textColIndex);
			num = baseContainer.GetSpid();
			shapeObj.FSPA.Spid = num;
			Escher.FillCollectionForSearch(baseContainer);
		}
		int textPos = GetTextPos();
		m_docInfo.TablesData.ArtObj.AddFSPA(shapeObj.FSPA, m_type, textPos);
		WriteMarker(WordChunkType.Shape);
	}

	public void InsertInlineShapeObject(InlineShapeObject shapeObj)
	{
		if (shapeObj.IsOLE && shapeObj.OLEContainerId != -1)
		{
			CHPX.PropertyModifiers.SetIntValue(27139, shapeObj.OLEContainerId);
			CHPX.PropertyModifiers.SetBoolValue(2058, flag: true);
		}
		else
		{
			int num = (m_nextPicLocation = (int)m_streamsManager.DataStream.Position);
			CHPX.PropertyModifiers.SetIntValue(27139, m_nextPicLocation);
			m_nextPicLocation = m_docInfo.ImageWriter.WriteInlineShapeObject(shapeObj);
			if (num == m_nextPicLocation)
			{
				m_streamsManager.DataStream.Position++;
			}
		}
		CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
		WriteMarker(WordChunkType.Image);
	}

	public void InsertBookmarkStart(string name, BookmarkStart start)
	{
		m_docInfo.TablesData.BookmarkStrings.Add(name);
		int textPos = GetTextPos();
		m_docInfo.TablesData.BookmarkDescriptor.Add(textPos);
		int num = m_docInfo.TablesData.BookmarkStrings.Find(name);
		if (start.ColumnLast > -1 && num != -1)
		{
			BookmarkDescriptor bookmarkDescriptor = m_docInfo.TablesData.BookmarkDescriptor;
			bookmarkDescriptor.SetCellGroup(num, isCellGroup: true);
			bookmarkDescriptor.SetStartCellIndex(num, start.ColumnFirst);
			bookmarkDescriptor.SetEndCellIndex(num, start.ColumnLast + 1);
		}
	}

	public void InsertBookmarkEnd(string name)
	{
		int num = m_docInfo.TablesData.BookmarkStrings.Find(name);
		if (num != -1)
		{
			int textPos = GetTextPos();
			m_docInfo.TablesData.BookmarkDescriptor.SetEndPos(num, textPos);
		}
	}

	public void InsertWatermark(Watermark watermark, UnitsConvertor unitsConvertor, float maxWidth)
	{
		FileShapeAddress fspa = CreateWatermarkFSPA();
		MsofbtSpContainer msofbtSpContainer = new MsofbtSpContainer(watermark.Document);
		msofbtSpContainer = ((watermark.Type != WatermarkType.TextWatermark) ? InsertPictureWatermark(watermark, fspa, unitsConvertor, maxWidth) : InsertTextWatermark(watermark, fspa, unitsConvertor));
		Escher.AddContainerForSubDocument(WordSubdocument.HeaderFooter, msofbtSpContainer);
		int textPos = GetTextPos();
		m_docInfo.TablesData.ArtObj.AddFSPA(fspa, m_type, textPos);
		WriteMarker(WordChunkType.Shape);
	}

	public WordWriterBase(StreamsManager streamsManager)
	{
		m_streamsManager = streamsManager;
	}

	protected WordWriterBase()
	{
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
		m_chpx = null;
		m_papx = null;
		m_breakChpx = null;
		if (m_listProperties != null)
		{
			m_listProperties.Close();
			m_listProperties = null;
		}
		if (m_endStack != null)
		{
			m_endStack.Clear();
			m_endStack = null;
		}
		if (m_textWriter != null)
		{
			m_textWriter.Dispose();
			m_textWriter = null;
		}
	}

	protected abstract void IncreaseCcp(int dataLength);

	protected virtual void InitClass()
	{
		m_curTxbxId = 3026;
		m_curPicId = 17000;
		if (m_styleSheet == null)
		{
			m_styleSheet = new WordStyleSheet(createDefCharStyle: true);
		}
		m_chpx = new CharacterPropertyException();
		m_breakChpx = new CharacterPropertyException();
		BreakCHPXStickProperties = true;
		CHPXStickProperties = true;
		m_papx = new ParagraphPropertyException();
		m_currStyleIndex = m_styleSheet.DefaultStyleIndex;
	}

	protected void WriteString(string text)
	{
		if (text != null && text != string.Empty)
		{
			byte[] bytes = m_docInfo.Fib.Encoding.GetBytes(text);
			m_streamsManager.MainStream.Write(bytes, 0, bytes.Length);
			AddChpxProperties(isParaBreak: false);
			IncreaseCcp(text.Length);
		}
	}

	protected void AddChpxProperties(bool isParaBreak)
	{
		CharacterPropertyException ex = new CharacterPropertyException();
		if (isParaBreak)
		{
			ex.PropertyModifiers = BreakCHPX.PropertyModifiers.Clone();
		}
		else
		{
			ex.PropertyModifiers = CHPX.PropertyModifiers.Clone();
		}
		m_docInfo.FkpData.AddChpxProperties((uint)m_streamsManager.MainStream.Position, ex);
	}

	protected void AddPapxProperties()
	{
		MemoryStream mainStream = m_streamsManager.MainStream;
		ParagraphPropertyException pAPX = PAPX;
		ParagraphExceptionInDiskPage paragraphExceptionInDiskPage = new ParagraphExceptionInDiskPage(PAPX.ClonePapx(PAPXStickProperties, PAPX));
		if (pAPX.PropertyModifiers[17920] != null && (StyleSheet.GetStyleByIndex(pAPX.PropertyModifiers[17920].ShortValue).ID > 0 || CurrentStyleIndex != 0))
		{
			paragraphExceptionInDiskPage.ParagraphStyleId = (ushort)CurrentStyleIndex;
		}
		UpdateShadingSprms(pAPX);
		paragraphExceptionInDiskPage.PropertyModifiers.RemoveValue(25707);
		paragraphExceptionInDiskPage.PropertyModifiers.RemoveValue(26182);
		paragraphExceptionInDiskPage.StyleIndex = (ushort)CurrentStyleIndex;
		m_docInfo.FkpData.AddPapxProperties((uint)mainStream.Position, paragraphExceptionInDiskPage, m_streamsManager.DataStream);
		PAPX.PropertyModifiers.Clear();
	}

	private void UpdateShadingSprms(ParagraphPropertyException paraPropertyException)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = null;
		if (paraPropertyException.PropertyModifiers[54802] != null)
		{
			singlePropertyModifierRecord = paraPropertyException.PropertyModifiers[54802];
			paraPropertyException.PropertyModifiers.SetByteArrayValue(54896, singlePropertyModifierRecord.Operand);
		}
		if (paraPropertyException.PropertyModifiers[54806] != null)
		{
			singlePropertyModifierRecord = paraPropertyException.PropertyModifiers[54806];
			paraPropertyException.PropertyModifiers.SetByteArrayValue(54897, singlePropertyModifierRecord.Operand);
		}
		if (paraPropertyException.PropertyModifiers[54796] != null)
		{
			singlePropertyModifierRecord = paraPropertyException.PropertyModifiers[54796];
			paraPropertyException.PropertyModifiers.SetByteArrayValue(54898, singlePropertyModifierRecord.Operand);
		}
	}

	protected void WriteSymbol(char symbol)
	{
		MemoryStream mainStream = m_streamsManager.MainStream;
		byte[] bytes = m_docInfo.Fib.Encoding.GetBytes(symbol.ToString());
		mainStream.Write(bytes, 0, bytes.Length);
	}

	protected void WriteChar(char symbol)
	{
		WriteSymbol(symbol);
		AddChpxProperties(symbol == '\r' || symbol == '\f');
		if (symbol == '\r' || symbol == '\f' || symbol == '\a')
		{
			AddPapxProperties();
		}
		IncreaseCcp(1);
	}

	protected void WriteNestedMark()
	{
		WriteSymbol('\r');
		AddChpxProperties(isParaBreak: false);
		AddPapxProperties();
		IncreaseCcp(1);
	}

	internal virtual int GetTextPos()
	{
		return (int)(m_streamsManager.MainStream.Position - m_iStartText) / m_docInfo.Fib.EncodingCharSize;
	}

	private void SetFSPASpid(BaseProps props)
	{
		if (props is PictureShapeProps)
		{
			props.Spid = m_curPicId;
		}
		else
		{
			props.Spid = m_curTxbxId;
		}
	}

	protected FieldDescriptor WriteFieldStart(FieldType fieldType)
	{
		FieldDescriptor fieldDescriptor = new FieldDescriptor();
		fieldDescriptor.FieldBoundary = 19;
		if (fieldType == FieldType.FieldShape)
		{
			fieldDescriptor.Type = (FieldType)2;
		}
		else
		{
			fieldDescriptor.Type = fieldType;
		}
		WriteMarker(WordChunkType.FieldBeginMark);
		return fieldDescriptor;
	}

	protected FieldDescriptor WriteFieldSeparator()
	{
		FieldDescriptor result = new FieldDescriptor
		{
			FieldBoundary = 20,
			IsNested = (m_endStack.Count > 1)
		};
		WriteMarker(WordChunkType.FieldSeparator);
		return result;
	}

	protected void WriteFieldEnd()
	{
		WriteMarker(WordChunkType.FieldEndMark);
	}

	protected void WriteNilPICFAndBinData(WField field)
	{
		CHPXStickProperties = false;
		int value = (int)m_streamsManager.DataStream.Position;
		BinaryWriter binaryWriter = new BinaryWriter(m_streamsManager.DataStream);
		PICF pICF = new PICF();
		pICF.lcb = 29 + (field.FieldValue.Length + 1) * 2 + pICF.cbHeader;
		pICF.Write(m_streamsManager.DataStream);
		byte[] array = new byte[pICF.lcb - pICF.cbHeader];
		array[0] = Convert.ToByte(8);
		string[] array2 = "D0 C9 EA 79 F9 BA CE 11 8C 82 00 AA 00 4B A9 0B".Split(' ');
		byte[] array3 = new byte[array2.Length];
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i] = Convert.ToByte(int.Parse(array2[i], NumberStyles.HexNumber).ToString());
		}
		array3.CopyTo(array, 1);
		array[17] = Convert.ToByte(2u);
		array[21] = Convert.ToByte(8);
		string text = RemoveFormattingString(field.FieldValue);
		array[25] = Convert.ToByte(text.Length + 1);
		Encoding.Unicode.GetBytes((text + "\0").ToCharArray()).CopyTo(array, 29);
		binaryWriter.Write(array);
		CHPX.PropertyModifiers.SetBoolValue(2050, flag: true);
		CHPX.PropertyModifiers.SetIntValue(27139, value);
		CHPX.PropertyModifiers.SetBoolValue(2054, flag: true);
		CHPX.PropertyModifiers.SetBoolValue(2133, flag: true);
		WriteMarker(WordChunkType.Image);
		CHPXStickProperties = true;
	}

	private string RemoveFormattingString(string value)
	{
		foreach (Group group in new Regex("([\\\\+].)+").Match(value).Groups)
		{
			if (group.Value != string.Empty)
			{
				value = value.Replace(group.Value, string.Empty);
			}
		}
		return value.Trim();
	}

	protected void AddFieldDescriptor(FieldDescriptor fld, int pos)
	{
		m_docInfo.TablesData.Fields.AddField(m_type, fld, pos);
	}

	public void AddTxBxContainer(bool visible, WTextBoxFormat txbxFormat)
	{
		txbxFormat.TextBoxShapeID = m_curTxbxId;
		txbxFormat.TextBoxIdentificator = NextTextId;
		MsofbtSpContainer msofbtSpContainer = new MsofbtSpContainer(txbxFormat.Document);
		msofbtSpContainer.CreateTextBoxContainer(visible, txbxFormat);
		Escher.AddContainerForSubDocument(m_type, msofbtSpContainer);
		m_curTxbxId++;
	}

	internal void SyncTxBxContainer(MsofbtSpContainer spContainer, bool visible, WTextBoxFormat txbxFormat)
	{
		m_textColIndex = 0;
		if (spContainer.ShapeOptions != null)
		{
			UpdateContainers(spContainer.Shape.ShapeId, m_curTxbxId, spContainer);
			_ = spContainer.ShapeOptions.Txid.Value;
			txbxFormat.TextBoxShapeID = m_curTxbxId;
			spContainer.SynchronizeIdent(null, ref m_curTxbxId, ref m_curPicId, ref m_curTxid, ref m_textColIndex);
			txbxFormat.TextBoxIdentificator = m_curTxid;
			spContainer.WriteTextBoxOptions(visible, txbxFormat);
		}
	}

	internal void AddPictContainer(WPicture pict, MsofbtSpContainer spContainer, PictureShapeProps pictProps)
	{
		if (spContainer == null || spContainer.Bse == null || (pict.IsMetaFile && spContainer.Bse.Blip is MsofbtImage) || spContainer.Bse.Blip.IsDib || pict.Document.IsReadOnly)
		{
			spContainer = new MsofbtSpContainer(pict.Document);
			spContainer.CreateImageContainer(pict, pictProps);
			m_curPicId++;
			Escher.AddContainerForSubDocument(m_type, spContainer);
		}
		else
		{
			Escher.ModifyBStoreByPid((int)spContainer.ShapeOptions.Pib.Value, spContainer.Bse);
			SyncPictContainer(spContainer, pictProps, pict);
		}
	}

	internal void SyncPictContainer(MsofbtSpContainer spContainer, PictureShapeProps pictProps, WPicture pic)
	{
		UpdateContainers(spContainer.Shape.ShapeId, m_curPicId, spContainer);
		spContainer.Shape.HasAnchor = true;
		spContainer.Shape.ShapeId = m_curPicId;
		m_curPicId++;
		spContainer.WritePictureOptions(pictProps, pic);
	}

	private void UpdateContainers(int oldId, int newId, MsofbtSpContainer spContainer)
	{
		try
		{
			Escher.Containers.Add(newId, spContainer);
		}
		catch (ArgumentNullException)
		{
		}
		catch (ArgumentException)
		{
		}
	}

	private FileShapeAddress CreateWatermarkFSPA()
	{
		return new FileShapeAddress
		{
			Height = 2000,
			Width = 2000,
			RelHrzPos = HorizontalOrigin.Column,
			RelVrtPos = VerticalOrigin.Paragraph,
			TextWrappingStyle = TextWrappingStyle.InFrontOfText,
			TextWrappingType = TextWrappingType.Both
		};
	}

	private MsofbtSpContainer InsertTextWatermark(Watermark watermark, FileShapeAddress fspa, UnitsConvertor unitsConvertor)
	{
		TextWatermark textWatermark = watermark as TextWatermark;
		MsofbtSpContainer msofbtSpContainer = new MsofbtSpContainer(watermark.Document);
		msofbtSpContainer.CreateTextWatermarkContainer(GetWatermarkNumber(), textWatermark);
		if (textWatermark.Height != -1f)
		{
			fspa.Height = (int)textWatermark.Height * 20;
			fspa.Width = (int)textWatermark.Width * 20;
		}
		else
		{
			fspa.Height = (int)(textWatermark.ShapeSize.Height * 13.43f);
			fspa.Width = (int)(textWatermark.ShapeSize.Width * 13.88f);
		}
		textWatermark.VerticalPosition = 0f;
		textWatermark.HorizontalPosition = 0f;
		fspa.YaTop = (int)textWatermark.VerticalPosition * 20;
		fspa.XaLeft = (int)textWatermark.HorizontalPosition * 20;
		fspa.Spid = m_curTxbxId;
		msofbtSpContainer.Shape.ShapeId = m_curTxbxId;
		m_curTxbxId++;
		msofbtSpContainer.IsWatermark = true;
		return msofbtSpContainer;
	}

	private MsofbtSpContainer InsertPictureWatermark(Watermark watermark, FileShapeAddress fspa, UnitsConvertor unitsConvertor, float maxWidth)
	{
		MsofbtSpContainer msofbtSpContainer = new MsofbtSpContainer(watermark.Document);
		PictureWatermark pictureWatermark = watermark as PictureWatermark;
		SizeF sizeF = FitPictureToPage(pictureWatermark, maxWidth, unitsConvertor);
		CreatePictureWatermarkCont(pictureWatermark, msofbtSpContainer);
		float num = sizeF.Height * 20f;
		float num2 = sizeF.Width * 20f;
		fspa.Width = (int)(num2 / 100f * pictureWatermark.Scaling);
		fspa.Height = (int)(num / 100f * pictureWatermark.Scaling);
		fspa.YaTop = (int)Math.Round(pictureWatermark.WordPicture.VerticalPosition * 20f);
		fspa.XaLeft = (int)Math.Round(pictureWatermark.WordPicture.HorizontalPosition * 20f);
		fspa.RelVrtPos = pictureWatermark.WordPicture.VerticalOrigin;
		if (pictureWatermark.WordPicture.HorizontalOrigin == HorizontalOrigin.LeftMargin || pictureWatermark.WordPicture.HorizontalOrigin == HorizontalOrigin.RightMargin || pictureWatermark.WordPicture.HorizontalOrigin == HorizontalOrigin.InsideMargin || pictureWatermark.WordPicture.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
		{
			fspa.RelHrzPos = HorizontalOrigin.Margin;
		}
		else
		{
			fspa.RelHrzPos = pictureWatermark.WordPicture.HorizontalOrigin;
		}
		fspa.TextWrappingStyle = pictureWatermark.WordPicture.TextWrappingStyle;
		fspa.TextWrappingType = pictureWatermark.WordPicture.TextWrappingType;
		fspa.IsBelowText = pictureWatermark.WordPicture.IsBelowText;
		fspa.Spid = m_curPicId;
		msofbtSpContainer.Shape.ShapeId = m_curPicId;
		m_curPicId++;
		msofbtSpContainer.IsWatermark = true;
		return msofbtSpContainer;
	}

	private SizeF FitPictureToPage(PictureWatermark picWatermark, float maxWidth, UnitsConvertor unitsConvertor)
	{
		float height = picWatermark.WordPicture.Size.Height;
		float width = picWatermark.WordPicture.Size.Width;
		return new SizeF(width, height);
	}

	private void CreatePictureWatermarkCont(PictureWatermark pictWatermark, MsofbtSpContainer pictContainer)
	{
		if (pictWatermark.OriginalPib != -1)
		{
			bool flag = Escher.CheckBStoreContByPid(pictWatermark.OriginalPib);
			pictWatermark.OriginalPib = (flag ? pictWatermark.OriginalPib : (-1));
		}
		pictContainer.CreatePictWatermarkContainer(GetWatermarkNumber(), pictWatermark);
		pictContainer.Pib = pictWatermark.OriginalPib;
	}

	private int GetWatermarkNumber()
	{
		int num = 0;
		return (this as WordHeaderFooterWriter).HeaderType switch
		{
			HeaderType.FirstPageHeader => 1, 
			HeaderType.OddHeader => 2, 
			_ => 3, 
		};
	}

	private bool IsImageEqual(byte[] imageHash1, byte[] imageHash2)
	{
		bool result = true;
		for (int i = 0; i < imageHash1.Length && i < imageHash2.Length; i++)
		{
			if (imageHash1[i] != imageHash2[i])
			{
				result = false;
				break;
			}
		}
		return result;
	}
}
