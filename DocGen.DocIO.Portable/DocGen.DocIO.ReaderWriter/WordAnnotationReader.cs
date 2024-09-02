using System;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordAnnotationReader : WordSubdocumentReader
{
	public bool IsStartAnnotation
	{
		get
		{
			int position = CalcCP(base.StatePositions.StartText, m_textChunk.Length);
			return m_docInfo.FkpData.Tables.Annotations.HasPosition(position);
		}
	}

	public AnnotationDescriptor Descriptor => m_docInfo.TablesData.Annotations.GetDescriptor(m_itemIndex);

	public string User => m_docInfo.TablesData.Annotations.GetUser(m_itemIndex);

	public int BookmarkStartOffset => m_docInfo.TablesData.Annotations.GetBookmarkStartOffset(m_itemIndex);

	public int BookmarkEndOffset => m_docInfo.TablesData.Annotations.GetBookmarkEndOffset(m_itemIndex);

	public int Position => m_docInfo.TablesData.Annotations.GetPosition(m_itemIndex);

	public WordAnnotationReader(WordReader mainReader)
		: base(mainReader)
	{
		m_type = WordSubdocument.Annotation;
	}

	public override WordChunkType ReadChunk()
	{
		base.ReadChunk();
		if (m_docInfo.FkpData.Tables.Annotations.Count == m_itemIndex + 1)
		{
			m_chunkType = WordChunkType.DocumentEnd;
		}
		return m_chunkType;
	}

	protected override void CreateStatePositions()
	{
		m_statePositions = new AtnStatePositions(m_docInfo.FkpData);
		base.CreateStatePositions();
	}
}
