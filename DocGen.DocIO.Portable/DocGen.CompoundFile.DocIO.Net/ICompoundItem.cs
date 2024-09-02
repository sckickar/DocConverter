namespace DocGen.CompoundFile.DocIO.Net;

internal interface ICompoundItem
{
	DirectoryEntry Entry { get; }

	void Flush();
}
