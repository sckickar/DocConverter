using System;

namespace DocGen.DocIO.DLS;

[Flags]
public enum ImportOptions
{
	KeepSourceFormatting = 1,
	MergeFormatting = 2,
	KeepTextOnly = 4,
	UseDestinationStyles = 8,
	ListContinueNumbering = 0x10,
	ListRestartNumbering = 0x20
}
