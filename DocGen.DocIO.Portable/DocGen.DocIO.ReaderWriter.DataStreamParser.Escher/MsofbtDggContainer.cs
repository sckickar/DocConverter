using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtDggContainer : BaseContainer
{
	internal MsofbtBstoreContainer BstoreContainer => FindContainerByType(typeof(MsofbtBstoreContainer)) as MsofbtBstoreContainer;

	internal MsofbtDgg Dgg => FindContainerByType(typeof(MsofbtDgg)) as MsofbtDgg;

	internal MsofbtDggContainer(WordDocument doc)
		: base(MSOFBT.msofbtDggContainer, doc)
	{
	}
}
