using System;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal abstract class WordSubdocumentReader : WordReaderBase, IWordSubdocumentReader, IWordReaderBase
{
	protected int DEF_SECTION_NUMBER = 1;

	protected HeaderType m_headerType = HeaderType.InvalidValue;

	protected int m_itemIndex = -1;

	private bool m_bIsNextItemPos;

	private WordReader m_mainReader;

	public WordSubdocument Type => m_type;

	public HeaderType HeaderType
	{
		get
		{
			return m_headerType;
		}
		set
		{
			m_headerType = value;
		}
	}

	public int ItemNumber => m_itemIndex;

	public StatePositionsBase StatePositions => m_statePositions;

	internal bool IsNextItemPos => m_bIsNextItemPos;

	public WordSubdocumentReader(WordReader mainReader)
		: base(mainReader.m_streamsManager)
	{
		m_mainReader = mainReader;
		m_styleSheet = mainReader.StyleSheet;
		m_currStyleIndex = mainReader.CurrentStyleIndex;
		m_mainReader.FreezeStreamPos();
		InitClass();
	}

	public override FieldDescriptor GetFld()
	{
		int pos = CalcCP(StatePositions.StartItemPos, 1);
		return m_docInfo.TablesData.Fields.FindFld(m_type, pos);
	}

	public virtual void Reset()
	{
		m_docInfo = m_mainReader.m_docInfo;
		CreateStatePositions();
		UpdateCharacterProperties();
		UpdateParagraphProperties();
	}

	public virtual void MoveToItem(int itemIndex)
	{
		m_itemIndex = itemIndex;
		UpdateStreamPosition();
	}

	protected virtual void CreateStatePositions()
	{
		MoveToItem(0);
	}

	protected override long GetChunkEndPosition(long iCurrentPos)
	{
		return Math.Min(base.GetChunkEndPosition(iCurrentPos), StatePositions.EndItemPos);
	}

	protected override void InitClass()
	{
		Reset();
	}

	protected virtual void UpdateStreamPosition()
	{
		m_streamsManager.MainStream.Position = StatePositions.MoveToItem(m_itemIndex);
	}

	protected override void UpdateEndPositions(long iEndPos)
	{
		m_bIsNextItemPos = false;
		base.UpdateEndPositions(iEndPos);
		if (m_type == WordSubdocument.HeaderFooter)
		{
			if ((StatePositions as HFStatePositions).UpdateHeaderEndPos(iEndPos, m_headerType))
			{
				m_headerType++;
			}
		}
		else if (StatePositions.UpdateItemEndPos(iEndPos))
		{
			m_itemIndex++;
			m_bIsNextItemPos = true;
		}
		UpdateCharacterProperties();
		UpdateParagraphProperties();
	}

	public override void UnfreezeStreamPos()
	{
		m_mainReader.FreezeStreamPos();
		base.UnfreezeStreamPos();
	}
}
