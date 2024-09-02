using System;

namespace DocGen.Pdf.Security;

[Flags]
internal enum SignatureFlags
{
	None = 0,
	SignaturesExists = 1,
	AppendOnly = 2
}
