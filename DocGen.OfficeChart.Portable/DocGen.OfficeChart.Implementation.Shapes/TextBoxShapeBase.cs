using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class TextBoxShapeBase : ShapeImpl
{
	private const int DEF_CONTINUE_FR_SIZE = 8;

	private const uint DEF_TEXTDIRECTION = 2u;

	private OfficeCommentHAlign m_hAlign = OfficeCommentHAlign.Left;

	private OfficeCommentVAlign m_vAlign = OfficeCommentVAlign.Top;

	private OfficeTextRotation m_textRotation;

	private bool m_bTextLocked = true;

	private RichTextString m_strText;

	private int m_iTextLen;

	private int m_iFormattingLen;

	private Color m_fillColor = ColorExtension.Empty;

	private Dictionary<string, string> m_unknownBodyProperties;

	private RichTextReader m_richTextReader;

	protected WorksheetImpl m_sheet;

	private ChartColor m_colorObject;

	public OfficeCommentHAlign HAlignment
	{
		get
		{
			return m_hAlign;
		}
		set
		{
			m_hAlign = value;
		}
	}

	public OfficeCommentVAlign VAlignment
	{
		get
		{
			return m_vAlign;
		}
		set
		{
			m_vAlign = value;
		}
	}

	public OfficeTextRotation TextRotation
	{
		get
		{
			return m_textRotation;
		}
		set
		{
			m_textRotation = value;
		}
	}

	public bool IsTextLocked
	{
		get
		{
			return m_bTextLocked;
		}
		set
		{
			m_bTextLocked = value;
		}
	}

	public IRichTextString RichText
	{
		get
		{
			if (m_strText == null)
			{
				InitializeVariables();
			}
			return m_strText;
		}
		set
		{
			m_strText = value as RichTextString;
		}
	}

	internal RichTextReader RichTextReader
	{
		get
		{
			if (m_richTextReader == null)
			{
				m_richTextReader = new RichTextReader(m_sheet);
			}
			return m_richTextReader;
		}
	}

	public string Text
	{
		get
		{
			return RichText.Text;
		}
		set
		{
			RichText.Text = value;
		}
	}

	internal RichTextString InnerRichText => m_strText;

	public Color FillColor
	{
		get
		{
			return m_fillColor;
		}
		set
		{
			m_fillColor = value;
		}
	}

	public Dictionary<string, string> UnknownBodyProperties
	{
		get
		{
			return m_unknownBodyProperties;
		}
		set
		{
			m_unknownBodyProperties = value;
		}
	}

	public ChartColor ColorObject
	{
		get
		{
			return m_colorObject;
		}
		set
		{
			m_colorObject = value;
		}
	}

	public TextBoxShapeBase(IApplication application, object parent)
		: base(application, parent)
	{
		InitializeVariables();
	}

	[CLSCompliant(false)]
	public TextBoxShapeBase(IApplication application, object parent, MsofbtSpContainer container, OfficeParseOptions options)
		: base(application, parent, container, options)
	{
	}

	public override IShape Clone(object parent, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes, bool addToCollections)
	{
		TextBoxShapeBase textBoxShapeBase = (TextBoxShapeBase)base.Clone(parent, hashNewNames, dicFontIndexes, addToCollections);
		int num = 0;
		if (textBoxShapeBase.m_strText != null)
		{
			textBoxShapeBase.m_strText = (RichTextString)m_strText.Clone(textBoxShapeBase);
			num = textBoxShapeBase.m_strText.TextObject.FormattingRunsCount;
		}
		WorkbookImpl workbookImpl = textBoxShapeBase.Workbook as WorkbookImpl;
		FontWrapper fontWrapper = null;
		for (int i = 0; i < num; i++)
		{
			if (i < m_strText.Text.Length && m_strText.GetFont(i, isCopy: true) is FontWrapper fontWrapper2)
			{
				FontImpl fontToAdd = fontWrapper2.Wrapped.Clone(workbookImpl.InnerFonts);
				FontImpl fontImpl = workbookImpl.AddFont(fontToAdd) as FontImpl;
				textBoxShapeBase.m_strText.TextObject.SetFontByIndex(i, fontImpl.Index);
			}
		}
		if (m_unknownBodyProperties != null)
		{
			textBoxShapeBase.m_unknownBodyProperties = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> unknownBodyProperty in m_unknownBodyProperties)
			{
				textBoxShapeBase.m_unknownBodyProperties.Add(unknownBodyProperty.Key, unknownBodyProperty.Value);
			}
		}
		return textBoxShapeBase;
	}

	internal void SetText(TextWithFormat text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("commentText");
		}
		((RichTextString)RichText).SetTextObject(text);
	}

	[CLSCompliant(false)]
	protected MsofbtClientTextBox GetClientTextBoxRecord(MsoBase parent)
	{
		MsofbtClientTextBox msofbtClientTextBox = (MsofbtClientTextBox)MsoFactory.GetRecord(MsoRecords.msofbtClientTextbox);
		TextObjectRecord textObjectRecord = (TextObjectRecord)BiffRecordFactory.GetRecord(TBIFFRecord.TextObject);
		int num = ((m_strText != null) ? m_strText.Text.Length : 0);
		textObjectRecord.HAlignment = HAlignment;
		textObjectRecord.VAlignment = VAlignment;
		textObjectRecord.TextLen = (ushort)num;
		textObjectRecord.FormattingRunsLen = 0;
		textObjectRecord.IsLockText = IsTextLocked;
		textObjectRecord.Rotation = TextRotation;
		msofbtClientTextBox.TextObject = textObjectRecord;
		if (num > 0)
		{
			AddTextContinueRecords(msofbtClientTextBox);
			AddFormattingContinueRecords(msofbtClientTextBox, textObjectRecord);
		}
		return msofbtClientTextBox;
	}

	private void AddFormattingContinueRecords(MsofbtClientTextBox result, TextObjectRecord textObject)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		byte[] array = ConvertFromShortToLongFR(SerializeFormattingRuns());
		int num = ((array != null) ? array.Length : 0);
		if (array != null)
		{
			int num2;
			for (int i = 0; i < num; i += num2)
			{
				num2 = Math.Min(num - i, 8224);
				ContinueRecord continueRecord = (ContinueRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Continue);
				byte[] array2 = new byte[num2];
				Buffer.BlockCopy(array, i, array2, 0, num2);
				continueRecord.SetData(array2);
				continueRecord.SetLength(num2);
				result.AddRecord(continueRecord);
			}
		}
		else
		{
			ContinueRecord continueRecord = (ContinueRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Continue);
			continueRecord.SetLength(0);
			result.AddRecord(continueRecord);
		}
		textObject.FormattingRunsLen = (ushort)num;
	}

	private void AddTextContinueRecords(MsofbtClientTextBox result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		string text = m_strText.Text;
		int length = text.Length;
		int num = 0;
		while (num < length)
		{
			int num2 = Math.Min(length - num, 4111);
			ContinueRecord continueRecord = (ContinueRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Continue);
			continueRecord.AutoGrowData = true;
			string value = text.Substring(num, num2);
			int length2 = continueRecord.SetStringNoLenDetectEncoding(0, value);
			continueRecord.SetLength(length2);
			num += num2;
			result.AddRecord(continueRecord);
		}
	}

	private void ParseTextObject(TextObjectRecord textObject)
	{
		if (textObject == null)
		{
			throw new ArgumentNullException("textObject");
		}
		m_hAlign = textObject.HAlignment;
		m_vAlign = textObject.VAlignment;
		m_textRotation = textObject.Rotation;
		m_bTextLocked = textObject.IsLockText;
		m_iTextLen = textObject.TextLen;
		m_iFormattingLen = textObject.FormattingRunsLen;
	}

	private void ParseContinueRecords(string strText, byte[] formattingRuns, OfficeParseOptions options)
	{
		TextWithFormat textWithFormat = new TextWithFormat();
		textWithFormat.Text = strText;
		if (formattingRuns != null)
		{
			int num = formattingRuns.Length / 8;
			byte[] array = new byte[num * 4];
			for (int i = 0; i < num; i++)
			{
				Buffer.BlockCopy(formattingRuns, i * 8, array, i * 4, 4);
			}
			textWithFormat.ParseFormattingRuns(array);
		}
		m_strText.Parse(textWithFormat, null, options);
	}

	private byte[] SerializeFormattingRuns()
	{
		byte[] array = m_strText.TextObject.SerializeFormatting();
		if (array == null || array.Length == 0)
		{
			byte[] array2 = new byte[8];
			byte[] bytes = BitConverter.GetBytes((ushort)m_strText.Text.Length);
			array2[4] = bytes[0];
			array2[5] = bytes[1];
			return array2;
		}
		if (array[0] != 0 || array[1] != 0)
		{
			byte[] array3 = array;
			array = new byte[array.Length + 4];
			array3.CopyTo(array, 4);
			BitConverter.GetBytes((ushort)0).CopyTo(array, 0);
			BitConverter.GetBytes((ushort)0).CopyTo(array, 1);
		}
		int num = array.Length;
		if (BitConverter.ToUInt16(array, num - 4) != m_strText.Text.Length)
		{
			byte[] array4 = array;
			array = new byte[array.Length + 4];
			array4.CopyTo(array, 0);
			BitConverter.GetBytes((ushort)m_strText.Text.Length).CopyTo(array, array4.Length);
			BitConverter.GetBytes((ushort)0).CopyTo(array, array4.Length + 2);
		}
		return array;
	}

	private byte[] ConvertFromShortToLongFR(byte[] arrShortFR)
	{
		if (arrShortFR == null)
		{
			return null;
		}
		int num = arrShortFR.Length / 4;
		byte[] array = new byte[num * 8];
		for (int i = 0; i < num; i++)
		{
			Buffer.BlockCopy(arrShortFR, i * 4, array, i * 8, 4);
		}
		return array;
	}

	protected virtual void InitializeVariables()
	{
		m_strText = new RichTextString(base.Application, base.ParentWorkbook, this, isReadOnly: false, bCreateText: true);
		m_bSupportOptions = true;
	}

	[CLSCompliant(false)]
	protected virtual void ParseClientTextBoxRecord(MsofbtClientTextBox textBox, OfficeParseOptions options)
	{
		if (textBox == null)
		{
			throw new ArgumentNullException("textBox");
		}
		RichText.Text = string.Empty;
		ParseTextObject(textBox.TextObject);
		string text = textBox.Text;
		byte[] formattingRuns = textBox.FormattingRuns;
		if (text != null && formattingRuns != null)
		{
			ParseContinueRecords(text, formattingRuns, options);
		}
	}

	public void CopyFrom(TextBoxShapeBase source, Dictionary<int, int> dicFontIndexes)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		m_strText.CopyFrom(source.m_strText, dicFontIndexes);
	}

	[CLSCompliant(false)]
	protected override MsofbtOPT CreateDefaultOptions()
	{
		MsofbtOPT msofbtOPT = base.CreateDefaultOptions();
		msofbtOPT.Version = 3;
		msofbtOPT.Instance = 2;
		if (Text.Length != 0)
		{
			SerializeOption(msofbtOPT, MsoOptions.TextId, 19990000);
		}
		SerializeTextDirection(msofbtOPT);
		SerializeSizeTextToFit(msofbtOPT);
		return msofbtOPT;
	}

	[CLSCompliant(false)]
	protected void SerializeTextDirection(MsofbtOPT options)
	{
		SerializeOption(options, MsoOptions.TextDirection, 2u);
	}

	[CLSCompliant(false)]
	protected override MsofbtOPT SerializeOptions(MsoBase parent)
	{
		if (m_options != null && m_options.Properties.Length != 0)
		{
			return m_options;
		}
		MsofbtOPT options = m_options;
		if (m_bUpdateLineFill || m_options == null)
		{
			options = (m_options = CreateDefaultOptions());
			options = SerializeMsoOptions(m_options);
		}
		SerializeShapeName(options);
		SerializeName(options, MsoOptions.AlternativeText, AlternativeText);
		return m_options;
	}

	[CLSCompliant(false)]
	protected override void ParseOtherRecords(MsoBase subRecord, OfficeParseOptions options)
	{
		if (subRecord == null)
		{
			throw new ArgumentNullException("subRecord");
		}
		if (subRecord.MsoRecordType == MsoRecords.msofbtClientTextbox)
		{
			ParseClientTextBoxRecord(subRecord as MsofbtClientTextBox, options);
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		if (m_unknownBodyProperties != null)
		{
			m_unknownBodyProperties.Clear();
		}
		if (m_strText != null)
		{
			m_strText = null;
		}
	}
}
