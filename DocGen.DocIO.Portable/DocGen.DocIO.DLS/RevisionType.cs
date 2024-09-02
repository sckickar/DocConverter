using System;

namespace DocGen.DocIO.DLS;

[Flags]
public enum RevisionType
{
	None = 1,
	Insertions = 2,
	Deletions = 4,
	Formatting = 8,
	StyleDefinitionChange = 0x10,
	MoveFrom = 0x20,
	MoveTo = 0x40
}
