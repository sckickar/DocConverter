using System;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordTextBoxWriter : WordSubdocumentWriter
{
	protected const uint DEF_TEXTBOX_RESERVED_DATA = uint.MaxValue;

	protected int m_lastTxbxPosition;

	protected int m_txbxBkDCnt;

	protected long m_dataPosition;

	internal WordTextBoxWriter(WordWriter mainWriter)
		: base(mainWriter)
	{
		m_type = WordSubdocument.TextBox;
	}

	public override void WriteDocumentEnd()
	{
		for (int i = 0; i < 2; i++)
		{
			WriteChar('\r');
		}
		AddNewTxbx(isLast: true, 0);
	}

	public override void WriteItemEnd()
	{
		base.WriteItemEnd();
		AddNewTxbx(isLast: false, 0);
	}

	internal void WriteTextBoxEnd(int spid)
	{
		base.WriteMarker(WordChunkType.ParagraphEnd);
		AddNewTxbx(isLast: false, spid);
	}

	protected override void IncreaseCcp(int dataLength)
	{
		m_docInfo.Fib.CcpTxbx += dataLength;
	}

	protected virtual void AddNewTxbx(bool isLast, int spid)
	{
		TextBoxStoryDescriptor textBoxStoryDescriptor = new TextBoxStoryDescriptor();
		BreakDescriptor breakDescriptor = new BreakDescriptor();
		textBoxStoryDescriptor.TextBoxCnt = 1;
		if (!isLast)
		{
			breakDescriptor.Ipgd = (short)m_txbxBkDCnt;
			breakDescriptor.Options = 16;
			textBoxStoryDescriptor.ShapeIdent = spid;
			textBoxStoryDescriptor.Reserved = uint.MaxValue;
		}
		else
		{
			breakDescriptor.Ipgd = -1;
			breakDescriptor.Options = 0;
			textBoxStoryDescriptor.ShapeIdent = 0;
		}
		m_docInfo.TablesData.ArtObj.AddTxbx(WordSubdocument.Main, textBoxStoryDescriptor, breakDescriptor, m_lastTxbxPosition);
		m_lastTxbxPosition = m_docInfo.Fib.CcpTxbx;
		m_txbxBkDCnt++;
	}
}
