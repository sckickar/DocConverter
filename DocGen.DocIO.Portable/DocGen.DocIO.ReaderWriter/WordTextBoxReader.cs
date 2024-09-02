using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordTextBoxReader : WordSubdocumentReader
{
	public WordTextBoxReader(WordReader mainReader)
		: base(mainReader)
	{
		m_type = WordSubdocument.TextBox;
	}

	protected override void CreateStatePositions()
	{
		m_statePositions = new TextBoxStatePositions(m_docInfo.FkpData);
		base.CreateStatePositions();
	}
}
