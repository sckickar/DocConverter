using System;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordHeaderFooterReader : WordSubdocumentReader
{
	public new HFStatePositions StatePositions => (HFStatePositions)m_statePositions;

	public WordHeaderFooterReader(WordReader mainReader)
		: base(mainReader)
	{
		m_type = WordSubdocument.HeaderFooter;
	}

	public override void Reset()
	{
		base.Reset();
		MoveToSection(DEF_SECTION_NUMBER);
	}

	public void MoveToSection(int iSectionNumber)
	{
		StatePositions.SectionIndex = iSectionNumber - 1;
		MoveToHeader(HeaderType.EvenHeader);
		UpdateCharacterProperties();
		UpdateParagraphProperties();
	}

	public void MoveToHeader(HeaderType hType)
	{
		m_headerType = hType;
		UpdateStreamPosition();
	}

	public override void MoveToItem(int itemIndex)
	{
		MoveToHeader((HeaderType)itemIndex);
	}

	public override FileShapeAddress GetFSPA()
	{
		int cP = CalcCP(StatePositions.StartItemPos, 1);
		if (m_docInfo.TablesData.FileArtObjects == null)
		{
			return null;
		}
		return m_docInfo.TablesData.FileArtObjects.FindFileShape(m_type, cP);
	}

	protected override void UpdateStreamPosition()
	{
		if (m_docInfo.TablesData.HeaderFooterCharPosTable == null)
		{
			m_chunkType = WordChunkType.DocumentEnd;
			m_headerType = HeaderType.InvalidValue;
		}
		else
		{
			UnfreezeStreamPos();
			m_chunkType = WordChunkType.Text;
			m_streamsManager.MainStream.Position = StatePositions.MoveToItem((int)m_headerType);
		}
	}

	protected override void CreateStatePositions()
	{
		if (m_statePositions == null)
		{
			m_statePositions = new HFStatePositions(m_docInfo.FkpData);
		}
	}
}
