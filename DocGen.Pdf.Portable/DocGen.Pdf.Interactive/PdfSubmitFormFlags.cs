using System;

namespace DocGen.Pdf.Interactive;

[Flags]
public enum PdfSubmitFormFlags
{
	IncludeExclude = 1,
	IncludeNoValueFields = 2,
	ExportFormat = 4,
	GetMethod = 8,
	SubmitCoordinates = 0x10,
	Xfdf = 0x20,
	IncludeAppendSaves = 0x40,
	IncludeAnnotations = 0x80,
	SubmitPdf = 0x100,
	CanonicalFormat = 0x200,
	ExclNonUserAnnots = 0x400,
	ExclFKey = 0x800,
	EmbedForm = 0x1000
}
