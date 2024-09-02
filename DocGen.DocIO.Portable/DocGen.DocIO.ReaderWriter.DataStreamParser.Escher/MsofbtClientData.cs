using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtClientData : MsofbtGeneral
{
	internal MsofbtClientData(WordDocument doc)
		: base(doc)
	{
		base.Header.Type = MSOFBT.msofbtClientData;
	}
}
