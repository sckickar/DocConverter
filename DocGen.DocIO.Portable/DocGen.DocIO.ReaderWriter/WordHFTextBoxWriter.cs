using System;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordHFTextBoxWriter : WordTextBoxWriter
{
	internal WordHFTextBoxWriter(WordWriter mainWriter)
		: base(mainWriter)
	{
		m_type = WordSubdocument.HeaderTextBox;
	}

	internal void WriteHFTextBoxEnd(int spid)
	{
		base.WriteMarker(WordChunkType.ParagraphEnd);
		AddNewTxbx(isLast: false, spid);
	}

	protected override void IncreaseCcp(int dataLength)
	{
		m_docInfo.Fib.CcpHdrTxbx += dataLength;
	}

	protected override void AddNewTxbx(bool isLast, int spid)
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
		}
		m_docInfo.TablesData.ArtObj.AddTxbx(WordSubdocument.HeaderFooter, textBoxStoryDescriptor, breakDescriptor, m_lastTxbxPosition);
		m_lastTxbxPosition = m_docInfo.Fib.CcpHdrTxbx;
		m_txbxBkDCnt++;
	}
}
