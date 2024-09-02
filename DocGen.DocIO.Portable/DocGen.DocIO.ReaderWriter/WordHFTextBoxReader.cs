using System;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class WordHFTextBoxReader : WordSubdocumentReader
{
	public WordHFTextBoxReader(WordReader mainReader)
		: base(mainReader)
	{
		m_type = WordSubdocument.HeaderTextBox;
	}

	protected override void CreateStatePositions()
	{
		m_statePositions = new HFTextBoxStatePositions(m_docInfo.FkpData);
		base.CreateStatePositions();
	}
}
