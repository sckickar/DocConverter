using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtBstoreContainer : BaseContainer
{
	internal MsofbtBstoreContainer(WordDocument doc)
		: base(MSOFBT.msofbtBstoreContainer, doc)
	{
	}

	protected override void WriteRecordData(Stream stream)
	{
		base.Header.Instance = base.Children.Count;
		base.WriteRecordData(stream);
	}
}
