using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtClientAnchor : MsofbtGeneral
{
	internal MsofbtClientAnchor(WordDocument doc)
		: base(doc)
	{
		base.Header.Type = MSOFBT.msofbtClientAnchor;
	}
}
