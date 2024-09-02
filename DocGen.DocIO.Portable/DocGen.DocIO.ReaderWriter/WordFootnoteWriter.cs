using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordFootnoteWriter : WordSubdocumentWriter
{
	internal WordFootnoteWriter(WordWriter mainWriter)
		: base(mainWriter)
	{
		m_type = WordSubdocument.Footnote;
	}

	public override void WriteDocumentEnd()
	{
		m_docInfo.TablesData.Footnotes.AddTxtPosition(m_docInfo.Fib.CcpFtn);
		m_docInfo.TablesData.Footnotes.AddTxtPosition(m_docInfo.Fib.CcpFtn + 3);
		WriteChar('\r');
	}

	public override void WriteItemStart()
	{
		m_docInfo.TablesData.Footnotes.AddTxtPosition(m_docInfo.Fib.CcpFtn);
	}

	protected override void IncreaseCcp(int dataLength)
	{
		m_docInfo.Fib.CcpFtn += dataLength;
	}
}
