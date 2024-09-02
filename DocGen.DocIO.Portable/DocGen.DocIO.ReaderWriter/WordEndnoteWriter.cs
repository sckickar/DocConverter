using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordEndnoteWriter : WordSubdocumentWriter
{
	internal WordEndnoteWriter(WordWriter mainWriter)
		: base(mainWriter)
	{
		m_type = WordSubdocument.Endnote;
	}

	public override void WriteItemStart()
	{
		m_docInfo.TablesData.Endnotes.AddTxtPosition(m_docInfo.Fib.CcpEdn);
	}

	public override void WriteDocumentEnd()
	{
		m_docInfo.TablesData.Endnotes.AddTxtPosition(m_docInfo.Fib.CcpEdn);
		m_docInfo.TablesData.Endnotes.AddTxtPosition(m_docInfo.Fib.CcpEdn + 3);
		WriteChar('\r');
	}

	protected override void IncreaseCcp(int dataLength)
	{
		m_docInfo.Fib.CcpEdn += dataLength;
	}
}
