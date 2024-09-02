using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtSolverContainer : BaseContainer
{
	internal MsofbtSolverContainer(WordDocument doc)
		: base(MSOFBT.msofbtSolverContainer, doc)
	{
	}
}
