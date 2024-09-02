using System;

namespace DocGen.Pdf.Security;

internal interface IRandom : IDisposable
{
	long Length { get; }

	int Get(long position);

	int Get(long position, byte[] bytes, int offset, int length);

	void Close();
}
