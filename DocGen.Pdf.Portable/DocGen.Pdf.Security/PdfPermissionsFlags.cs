using System;

namespace DocGen.Pdf.Security;

[Flags]
public enum PdfPermissionsFlags
{
	Default = 0,
	Print = 4,
	EditContent = 8,
	CopyContent = 0x10,
	EditAnnotations = 0x20,
	FillFields = 0x100,
	AccessibilityCopyContent = 0x200,
	AssembleDocument = 0x400,
	FullQualityPrint = 0x800
}
