using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordEndnoteReader : WordFootnoteReader
{
	public WordEndnoteReader(WordReader mainReader)
		: base(mainReader)
	{
	}

	protected override bool CheckPosition()
	{
		int num = CalcCP(base.StatePositions.StartText, m_textChunk.Length);
		if (m_docInfo.FkpData.Tables.Endnotes.HasPosition(num))
		{
			return num != 0;
		}
		return false;
	}

	protected override void Init()
	{
		m_type = WordSubdocument.Endnote;
	}

	protected override void InitStatePositions()
	{
		m_statePositions = new EndnoteStatePositions(m_docInfo.FkpData);
	}

	protected override bool IsEndOfItems()
	{
		return m_docInfo.FkpData.Tables.Endnotes.Count == m_itemIndex + 1;
	}
}
