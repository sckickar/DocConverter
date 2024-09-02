using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtSpgrContainer : BaseContainer
{
	internal MsofbtSp Shape
	{
		get
		{
			if (base.Children[0] is MsofbtSpContainer msofbtSpContainer)
			{
				return msofbtSpContainer.Shape;
			}
			return null;
		}
	}

	internal MsofbtSpgrContainer(WordDocument doc)
		: base(MSOFBT.msofbtSpgrContainer, doc)
	{
	}
}
