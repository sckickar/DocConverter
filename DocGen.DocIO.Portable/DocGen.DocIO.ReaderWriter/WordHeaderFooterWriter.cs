using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordHeaderFooterWriter : WordSubdocumentWriter
{
	private const int DEF_HEADER_INDEX = 7;

	protected HeaderType m_headerType;

	private int m_iItemIndex;

	private int m_iSectionIndex;

	internal HeaderType HeaderType
	{
		get
		{
			return m_headerType;
		}
		set
		{
			if (value < m_headerType)
			{
				throw new ArgumentOutOfRangeException($"HeaderType must be greater from {m_headerType}");
			}
			ClosePrevHeaderTypes(value);
		}
	}

	internal WordHeaderFooterWriter(WordWriter mainWriter)
		: base(mainWriter)
	{
		m_type = WordSubdocument.HeaderFooter;
	}

	public override void WriteDocumentEnd()
	{
		ClosePrevHeaderTypes((HeaderType)6);
		WriteChar('\r');
		int textPos = GetTextPos();
		m_docInfo.TablesData.HeaderPositions[m_iItemIndex] = textPos + 3;
	}

	internal void WriteSectionEnd()
	{
		HeaderType = (HeaderType)6;
		m_iSectionIndex++;
		m_iItemIndex = (m_iSectionIndex + 1) * 6 + 1;
		m_headerType = HeaderType.EvenHeader;
	}

	internal void ClosePrevSeparator()
	{
		int textPos = GetTextPos();
		if (textPos != m_docInfo.TablesData.HeaderPositions[m_iItemIndex - 1])
		{
			WriteChar('\r');
			int textPos2 = GetTextPos();
			m_docInfo.TablesData.HeaderPositions[m_iItemIndex] = textPos2;
		}
		else
		{
			m_docInfo.TablesData.HeaderPositions[m_iItemIndex] = textPos;
		}
		m_iItemIndex++;
	}

	protected void ClosePrevHeaderTypes(HeaderType headerType)
	{
		while (m_headerType != headerType)
		{
			if (GetTextPos() != m_docInfo.TablesData.HeaderPositions[m_iItemIndex - 1])
			{
				WriteChar('\r');
				m_headerType++;
				int textPos = GetTextPos();
				m_docInfo.TablesData.HeaderPositions[m_iItemIndex] = textPos;
			}
			else
			{
				m_headerType++;
				int textPos2 = GetTextPos();
				m_docInfo.TablesData.HeaderPositions[m_iItemIndex] = textPos2;
			}
			m_iItemIndex++;
		}
	}

	protected override void IncreaseCcp(int dataLength)
	{
		m_docInfo.Fib.CcpHdd += dataLength;
	}

	protected override void InitClass()
	{
		base.InitClass();
		int sepxAddedCount = m_docInfo.FkpData.SepxAddedCount;
		m_docInfo.TablesData.HeaderPositions = new int[7 + sepxAddedCount * 6 + 1];
		WriteHeaderFooterHead();
		m_headerType = HeaderType.EvenHeader;
		m_curTxbxId = 4050;
		m_curPicId = 4500;
	}

	private void WriteHeaderFooterHead()
	{
		m_iItemIndex = 1;
	}
}
