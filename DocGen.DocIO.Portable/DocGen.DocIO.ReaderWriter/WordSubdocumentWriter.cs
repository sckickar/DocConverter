using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal abstract class WordSubdocumentWriter : WordWriterBase, IWordSubdocumentWriter, IWordWriterBase
{
	public WordSubdocument Type => m_type;

	internal WordSubdocumentWriter(WordWriter mainWriter)
		: base(mainWriter.m_streamsManager)
	{
		m_docInfo = mainWriter.m_docInfo;
		m_styleSheet = mainWriter.StyleSheet;
		InitClass();
		m_listProperties = mainWriter.ListProperties;
		m_listProperties.UpdatePAPX(m_papx);
	}

	public abstract void WriteDocumentEnd();

	public virtual void WriteItemStart()
	{
	}

	public virtual void WriteItemEnd()
	{
		WriteMarker(WordChunkType.ParagraphEnd);
	}

	protected override void InitClass()
	{
		base.InitClass();
		m_iStartText = (int)m_streamsManager.MainStream.Position;
	}
}
