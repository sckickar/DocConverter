using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordFootnoteReader : WordSubdocumentReader
{
	protected int m_prevStreamPos = -1;

	public bool IsNextItem
	{
		get
		{
			bool result = false;
			bool flag = CheckPosition();
			if (flag)
			{
				result = m_streamsManager.MainStream.Position != m_prevStreamPos && flag;
				m_prevStreamPos = (int)m_streamsManager.MainStream.Position;
			}
			return result;
		}
	}

	public WordFootnoteReader(WordReader mainReader)
		: base(mainReader)
	{
		Init();
	}

	protected override void CreateStatePositions()
	{
		InitStatePositions();
		base.CreateStatePositions();
	}

	protected virtual bool CheckPosition()
	{
		int num = CalcCP(base.StatePositions.StartText, m_textChunk.Length);
		if (m_docInfo.FkpData.Tables.Footnotes.HasPosition(num))
		{
			return num != 0;
		}
		return false;
	}

	protected virtual void Init()
	{
		m_type = WordSubdocument.Footnote;
	}

	protected virtual void InitStatePositions()
	{
		m_statePositions = new FootnoteStatePositions(m_docInfo.FkpData);
	}

	protected virtual bool IsEndOfItems()
	{
		return m_docInfo.FkpData.Tables.Footnotes.Count == m_itemIndex + 1;
	}

	public override WordChunkType ReadChunk()
	{
		WordChunkType result = base.ReadChunk();
		if (IsEndOfItems())
		{
			result = WordChunkType.DocumentEnd;
		}
		return result;
	}
}
