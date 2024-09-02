using System;

namespace DocGen.DocIO.DLS;

[Flags]
internal enum RevisionBalloonsOptions
{
	Inline = 1,
	Deletions = 2,
	Formatting = 4
}
