using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordAnnotationWriter : WordSubdocumentWriter
{
	internal WordAnnotationWriter(WordWriter mainWriter)
		: base(mainWriter)
	{
		m_type = WordSubdocument.Annotation;
	}

	public override void WriteDocumentEnd()
	{
		m_docInfo.TablesData.Annotations.AddTxtPosition(m_docInfo.Fib.CcpAtn);
		m_docInfo.TablesData.Annotations.AddTxtPosition(m_docInfo.Fib.CcpAtn + 3);
		WriteChar('\r');
	}

	public override void WriteItemStart()
	{
		m_docInfo.TablesData.Annotations.AddTxtPosition(m_docInfo.Fib.CcpAtn);
		WriteMarker(WordChunkType.Annotation);
	}

	protected override void IncreaseCcp(int dataLength)
	{
		m_docInfo.Fib.CcpAtn += dataLength;
	}
}
