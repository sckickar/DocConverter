using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class RichTextString : CommonWrapper, IRichTextString, IParentApplication, IOptimizedUpdate
{
	protected TextWithFormat m_text;

	private string m_rtfText;

	protected WorkbookImpl m_book;

	private bool m_bIsReadOnly;

	private object m_rtfParent;

	private static readonly char[] DEF_DIGITS = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

	private const char DEF_ZERO = 'X';

	private object m_parent;

	private int m_iFontIndex;

	private string m_imageRTF;

	private PreservationLogger m_logger;

	public string Text
	{
		get
		{
			if (m_text == null)
			{
				m_text = new TextWithFormat();
			}
			return m_text.Text;
		}
		set
		{
			BeginUpdate();
			string text = m_text.Text;
			m_text.Text = value;
			if (text.Length < m_text.Text.Length && IsFormatted)
			{
				m_text.SetTextFontIndex(text.Length, m_text.Text.Length - 1, m_text.FormattingRuns.Values[m_text.FormattingRuns.Values.Count - 1]);
			}
			else if (IsFormatted)
			{
				int iFontIndex = m_text.FormattingRuns.Values[0];
				m_text.ClearFormatting();
				m_text.SetTextFontIndex(0, m_text.Text.Length - 1, iFontIndex);
			}
			EndUpdate();
		}
	}

	public string RtfText
	{
		get
		{
			if (m_rtfParent != null)
			{
				return m_rtfText;
			}
			return GenerateRtfText();
		}
		set
		{
			m_rtfText = value;
			if (m_rtfParent != null && m_rtfParent is TextBoxShapeBase)
			{
				(m_rtfParent as TextBoxShapeImpl).RichTextReader.SetRTF(m_rtfParent, m_rtfText);
			}
		}
	}

	public bool IsFormatted => m_text.FormattingRunsCount > 0;

	public object Parent => m_parent;

	public IApplication Application => m_book.Application;

	public SizeF StringSize
	{
		get
		{
			SizeF result = new SizeF(0f, 0f);
			int iStartPos = 0;
			int i = 0;
			SizeF sizePart;
			for (int formattingRunsCount = m_text.FormattingRunsCount; i < formattingRunsCount; i++)
			{
				int positionByIndex = m_text.GetPositionByIndex(i);
				sizePart = GetSizePart(iStartPos, positionByIndex);
				result.Width += sizePart.Width;
				result.Height = Math.Max(sizePart.Height, result.Height);
				iStartPos = positionByIndex;
			}
			sizePart = GetSizePart(iStartPos, Text.Length);
			result.Width += sizePart.Width;
			result.Height = Math.Max(sizePart.Height, result.Height);
			return result;
		}
	}

	public virtual FontImpl DefaultFont
	{
		get
		{
			return (FontImpl)m_book.InnerFonts[m_iFontIndex];
		}
		internal set
		{
			m_iFontIndex = value.Index;
		}
	}

	public TextWithFormat TextObject => m_text;

	public WorkbookImpl Workbook => m_book;

	public int DefaultFontIndex
	{
		get
		{
			return m_iFontIndex;
		}
		set
		{
			m_iFontIndex = value;
		}
	}

	internal string ImageRTF
	{
		get
		{
			return m_imageRTF;
		}
		set
		{
			m_imageRTF = value;
		}
	}

	public RichTextString(IApplication application, object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_parent = parent;
		SetParents();
		m_logger = new PreservationLogger();
	}

	public RichTextString(IApplication application, object parent, bool isReadOnly)
		: this(application, parent, isReadOnly, bCreateText: false)
	{
	}

	public RichTextString(IApplication application, object parent, object rtfParent, bool isReadOnly, bool bCreateText)
		: this(application, parent, isReadOnly, bCreateText)
	{
		m_rtfParent = rtfParent;
	}

	public RichTextString(IApplication application, object parent, bool isReadOnly, bool bCreateText)
		: this(application, parent)
	{
		m_bIsReadOnly = isReadOnly;
		if (bCreateText)
		{
			m_text = new TextWithFormat();
		}
	}

	internal RichTextString(IApplication application, object parent, bool isReadOnly, bool bCreateText, PreservationLogger logger)
		: this(application, parent)
	{
		m_bIsReadOnly = isReadOnly;
		if (bCreateText)
		{
			m_text = new TextWithFormat();
		}
		m_logger = logger;
	}

	public RichTextString(IApplication application, object parent, TextWithFormat text)
		: this(application, parent)
	{
		m_text = text;
	}

	protected virtual void SetParents()
	{
		m_book = CommonObject.FindParent(m_parent, typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	public IOfficeFont GetFont(int iPosition)
	{
		if (iPosition < 0 || iPosition >= m_text.Text.Length)
		{
			throw new ArgumentOutOfRangeException("iPosition");
		}
		int textFontIndex = m_text.GetTextFontIndex(iPosition);
		return new FontWrapper(GetFontByIndex(textFontIndex), bReadOnly: true, bRaiseEvents: false);
	}

	public IOfficeFont GetFont(int iPosition, bool isCopy)
	{
		if (iPosition < 0 || iPosition >= m_text.Text.Length)
		{
			throw new ArgumentOutOfRangeException("iPosition");
		}
		int textFontIndex = m_text.GetTextFontIndex(iPosition, isCopy);
		return new FontWrapper(GetFontByIndex(textFontIndex), bReadOnly: true, bRaiseEvents: false);
	}

	public void SetFont(int iStartPos, int iEndPos, IOfficeFont font)
	{
		BeginUpdate();
		int num = AddFont(font);
		if (iStartPos == 0)
		{
			if (m_text.FormattingRunsCount > 0)
			{
				int length = m_text.Text.Length;
				int defaultFontIndex = DefaultFontIndex;
				int num2 = ((length < iEndPos + 1) ? m_text.GetTextFontIndex(iEndPos + 1) : (-1));
				m_text.ReplaceFont(0, defaultFontIndex);
				if (num2 >= 0)
				{
					m_text.FormattingRuns[iEndPos + 1] = num2;
				}
				m_text.SetTextFontIndex(iStartPos, iEndPos, num);
			}
			else if (iEndPos < m_text.Text.Length - 1)
			{
				SetFont(iEndPos + 1, m_text.Text.Length - 1, DefaultFont);
			}
			FontImpl defaultFont = ((font is FontWrapper) ? (font as FontWrapper).Wrapped : (font as FontImpl));
			DefaultFont = defaultFont;
			if (num < 0)
			{
				num = 0;
			}
			else
			{
				DefaultFontIndex = num;
			}
			if (m_text.Text.Length > 0)
			{
				m_text.SetTextFontIndex(iStartPos, iEndPos, num);
			}
		}
		else
		{
			m_text.SetTextFontIndex(iStartPos, iEndPos, num);
		}
		EndUpdate();
	}

	public void ClearFormatting()
	{
		if (m_text != null && IsFormatted)
		{
			BeginUpdate();
			m_text.ClearFormatting();
			EndUpdate();
		}
	}

	public void Append(string text, IOfficeFont font)
	{
		BeginUpdate();
		int length = m_text.Text.Length;
		m_text.Text += text;
		SetFont(length, length + text.Length - 1, font);
		EndUpdate();
	}

	public void Substring(int startIndex, int length)
	{
		string text = m_text.Text;
		if (startIndex > 0)
		{
			if (startIndex >= text.Length)
			{
				m_text.Text = string.Empty;
				ClearFormatting();
			}
			else
			{
				m_text.RemoveAtStart(startIndex);
			}
		}
		m_text.RemoveAtEnd(m_text.Text.Length - length);
	}

	internal void SetText(string text)
	{
		BeginUpdate();
		_ = m_text.Text;
		m_text.Text = text;
		EndUpdate();
	}

	internal void UpdateRTF(IRange range, FontImpl font)
	{
		IWorkbook workbook = range.Worksheet.Workbook;
		IRichTextString richText = range.RichText;
		string text = range.RichText.Text;
		IOfficeFont officeFont = workbook.CreateFont();
		officeFont = font;
		richText.SetFont(0, text.Length - 1, officeFont);
	}

	protected internal virtual int GetFontIndex(int iPosition)
	{
		return m_text.GetFontByIndex(iPosition);
	}

	protected internal virtual FontImpl GetFontByIndex(int iFontIndex)
	{
		FontImpl fontImpl = null;
		if (iFontIndex == 0 && DefaultFontIndex >= 0)
		{
			return DefaultFont;
		}
		if (iFontIndex >= m_book.InnerFonts.Count)
		{
			return DefaultFont;
		}
		return (FontImpl)m_book.InnerFonts[iFontIndex];
	}

	public override void BeginUpdate()
	{
		if (m_bIsReadOnly)
		{
			throw new ReadOnlyException();
		}
		base.BeginUpdate();
	}

	public override void EndUpdate()
	{
		base.EndUpdate();
		m_logger.SetFlag(PreservedFlag.RichText);
	}

	public virtual void CopyFrom(RichTextString source, Dictionary<int, int> dicFontIndexes)
	{
		BeginUpdate();
		m_text = source.m_text.Clone(dicFontIndexes);
		EndUpdate();
	}

	public virtual void Parse(TextWithFormat text, Dictionary<int, int> dicFontIndexes, OfficeParseOptions options)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		m_text = text.TypedClone();
		int num = 0;
		FontsCollection innerFonts = m_book.InnerFonts;
		int formattingRunsCount = text.FormattingRunsCount;
		if (formattingRunsCount <= 0)
		{
			return;
		}
		for (int i = 0; i < formattingRunsCount; i++)
		{
			num = text.GetFontByIndex(i);
			num = FontImpl.UpdateFontIndexes(num, dicFontIndexes, options);
			if (num > innerFonts.Count)
			{
				num = 0;
			}
			m_text.SetFontByIndex(i, num);
		}
	}

	public override object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		RichTextString obj = (RichTextString)base.Clone(parent);
		obj.m_parent = parent;
		obj.m_text = m_text.TypedClone();
		SetParents();
		return obj;
	}

	public virtual void Clear()
	{
		m_text = m_text.TypedClone();
		m_text.ClearFormatting();
		m_text.Text = string.Empty;
	}

	protected virtual int AddFont(IOfficeFont font)
	{
		FontImpl font2 = ((IInternalFont)font).Font;
		font2 = m_book.InnerFonts.Add(font2) as FontImpl;
		return font2.Index;
	}

	internal void SetTextObject(TextWithFormat commentText)
	{
		if (commentText == null)
		{
			throw new ArgumentNullException("commentText");
		}
		m_text = commentText;
	}

	internal FontImpl GetFontObject(int iPosition)
	{
		if (iPosition < 0 || iPosition >= m_text.Text.Length)
		{
			throw new ArgumentOutOfRangeException("iPosition");
		}
		int textFontIndex = m_text.GetTextFontIndex(iPosition);
		return GetFontByIndex(textFontIndex);
	}

	private SizeF GetSizePart(int iStartPos, int iEndPos)
	{
		if (iStartPos < iEndPos)
		{
			FontImpl fontObject = GetFontObject(iStartPos);
			int length = iEndPos - iStartPos;
			string text = Text.Substring(iStartPos, length);
			text.IndexOfAny(DEF_DIGITS);
			return fontObject.MeasureStringSpecial(text);
		}
		return new SizeF(0f, 0f);
	}

	internal string GenerateRtfText()
	{
		m_text.Defragment();
		RtfTextWriter rtfTextWriter = new RtfTextWriter();
		AddFonts(rtfTextWriter);
		string text = m_text.Text;
		int formattingRunsCount = m_text.FormattingRunsCount;
		int iStartPos = 0;
		if (text.Length > 0)
		{
			if (formattingRunsCount > 0)
			{
				for (int i = 0; i <= formattingRunsCount; i++)
				{
					iStartPos = WriteFormattingRun(rtfTextWriter, i, iStartPos);
				}
			}
			else
			{
				WriteText(rtfTextWriter, DefaultFont.Index, text);
			}
		}
		rtfTextWriter.WriteTag(RtfTags.RtfEnd);
		return rtfTextWriter.ToString();
	}

	internal string GenerateRtfText(string alignment)
	{
		m_text.Defragment();
		RtfTextWriter rtfTextWriter = new RtfTextWriter();
		AddFonts(rtfTextWriter, alignment);
		string text = m_text.Text;
		int formattingRunsCount = m_text.FormattingRunsCount;
		int iStartPos = 0;
		if (text.Length > 0)
		{
			if (formattingRunsCount > 0)
			{
				for (int i = 0; i <= formattingRunsCount; i++)
				{
					iStartPos = WriteFormattingRun(rtfTextWriter, i, iStartPos, alignment);
				}
			}
			else
			{
				WriteText(rtfTextWriter, DefaultFont.Index, text, alignment);
			}
		}
		rtfTextWriter.WriteTag(RtfTags.RtfEnd);
		return rtfTextWriter.ToString();
	}

	private int WriteFormattingRun(RtfTextWriter writer, int iRunIndex, int iStartPos)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int formattingRunsCount = m_text.FormattingRunsCount;
		if (iRunIndex < 0 || iRunIndex > formattingRunsCount)
		{
			throw new ArgumentOutOfRangeException("iRunIndex", "Value cannot be less than 0 and greater than iCount - 1");
		}
		string text = m_text.Text;
		int num = ((iRunIndex == formattingRunsCount) ? text.Length : m_text.GetPositionByIndex(iRunIndex));
		if (num == iStartPos)
		{
			return iStartPos;
		}
		string strText = text.Substring(iStartPos, num - iStartPos);
		int num2 = ((iRunIndex != 0) ? m_text.GetFontByIndex(iRunIndex - 1) : 0);
		if (num2 == 0)
		{
			num2 = DefaultFont.Index;
		}
		WriteText(writer, num2, strText);
		return num;
	}

	private int WriteFormattingRun(RtfTextWriter writer, int iRunIndex, int iStartPos, string alignment)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int formattingRunsCount = m_text.FormattingRunsCount;
		if (iRunIndex < 0 || iRunIndex > formattingRunsCount)
		{
			throw new ArgumentOutOfRangeException("iRunIndex", "Value cannot be less than 0 and greater than iCount - 1");
		}
		string text = m_text.Text;
		int num = ((iRunIndex == formattingRunsCount) ? text.Length : m_text.GetPositionByIndex(iRunIndex));
		if (num == iStartPos)
		{
			return iStartPos;
		}
		string strText = text.Substring(iStartPos, num - iStartPos);
		int num2 = ((iRunIndex != 0) ? m_text.GetFontByIndex(iRunIndex - 1) : 0);
		if (num2 == 0)
		{
			num2 = DefaultFont.Index;
		}
		WriteText(writer, num2, strText, alignment);
		return num;
	}

	private void AddFonts(RtfTextWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int i = 0;
		for (int formattingRunsCount = m_text.FormattingRunsCount; i < formattingRunsCount; i++)
		{
			int fontByIndex = m_text.GetFontByIndex(i);
			if (fontByIndex != 0)
			{
				FontImpl fontByIndex2 = GetFontByIndex(fontByIndex);
				AddFont(fontByIndex2, writer);
			}
		}
		AddFont(DefaultFont, writer);
		writer.WriteTag(RtfTags.RtfBegin);
		writer.WriteFontTable();
		writer.WriteColorTable();
	}

	private void AddFonts(RtfTextWriter writer, string alignment)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		int i = 0;
		for (int formattingRunsCount = m_text.FormattingRunsCount; i < formattingRunsCount; i++)
		{
			int fontByIndex = m_text.GetFontByIndex(i);
			if (fontByIndex != 0)
			{
				FontImpl fontByIndex2 = GetFontByIndex(fontByIndex);
				AddFont(fontByIndex2, writer);
			}
		}
		AddFont(DefaultFont, writer);
		writer.WriteTag(RtfTags.RtfBegin);
		writer.WriteFontTable();
		writer.WriteColorTable();
		writer.WriteAlignment(alignment);
	}

	private void AddFont(FontImpl fontToAdd, RtfTextWriter writer)
	{
		if (fontToAdd == null)
		{
			throw new ArgumentNullException("fontToAdd");
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		Font font = fontToAdd.GenerateNativeFont();
		writer.AddFont(font);
		writer.AddColor(fontToAdd.RGBColor);
	}

	private void WriteText(RtfTextWriter writer, int iFontIndex, string strText)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strText == null)
		{
			throw new ArgumentNullException("strText");
		}
		if (strText.Length == 0)
		{
			throw new ArgumentException("strText - string cannot be empty");
		}
		IOfficeFont fontByIndex = GetFontByIndex(iFontIndex);
		writer.WriteText(fontByIndex, strText);
	}

	private void WriteText(RtfTextWriter writer, int iFontIndex, string strText, string alignment)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (strText == null)
		{
			throw new ArgumentNullException("strText");
		}
		if (strText.Length == 0)
		{
			throw new ArgumentException("strText - string cannot be empty");
		}
		IOfficeFont fontByIndex = GetFontByIndex(iFontIndex);
		writer.WriteImageText(fontByIndex, strText, ImageRTF, alignment);
	}

	internal void AddText(string text, IOfficeFont font)
	{
		int length = m_text.Text.Replace("\r\n", string.Empty).Length;
		m_text.Text += text;
		SetFont(length, m_text.Text.Length - 1, font);
	}
}
