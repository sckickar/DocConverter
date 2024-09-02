using System;
using System.IO;
using DocGen.CompoundFile.DocIO.Net;

namespace DocGen.CompoundFile.DocIO;

internal interface ICompoundFile : IDisposable
{
	ICompoundStorage RootStorage { get; }

	DocGen.CompoundFile.DocIO.Net.Directory Directory { get; }

	void Flush();

	void Save(Stream stream);
}
