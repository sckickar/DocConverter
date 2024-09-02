using System;

namespace DocGen.CompoundFile.DocIO;

internal interface ICompoundStorage : IDisposable
{
	string[] Streams { get; }

	string[] Storages { get; }

	string Name { get; }

	CompoundStream CreateStream(string streamName);

	CompoundStream OpenStream(string streamName);

	void DeleteStream(string streamName);

	bool ContainsStream(string streamName);

	ICompoundStorage CreateStorage(string storageName);

	ICompoundStorage OpenStorage(string storageName);

	void DeleteStorage(string storageName);

	bool ContainsStorage(string storageName);

	void Flush();

	void InsertCopy(ICompoundStorage storageToCopy);

	void InsertCopy(CompoundStream streamToCopy);
}
