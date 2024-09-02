using System;

namespace DocGen.Pdf.Security;

[Flags]
internal enum Asn1UniversalTags
{
	ReservedBER = 0,
	Boolean = 1,
	Integer = 2,
	BitString = 3,
	OctetString = 4,
	Null = 5,
	ObjectIdentifier = 6,
	ObjectDescriptor = 7,
	External = 8,
	Real = 9,
	Enumerated = 0xA,
	EmbeddedPDV = 0xB,
	UTF8String = 0xC,
	RelativeOid = 0xD,
	Sequence = 0x10,
	Set = 0x11,
	NumericString = 0x12,
	PrintableString = 0x13,
	TeletexString = 0x14,
	VideotexString = 0x15,
	IA5String = 0x16,
	UTFTime = 0x17,
	GeneralizedTime = 0x18,
	GraphicsString = 0x19,
	VisibleString = 0x1A,
	GeneralString = 0x1B,
	UniversalString = 0x1C,
	CharacterString = 0x1D,
	BMPString = 0x1E,
	Constructed = 0x20,
	Application = 0x40,
	Tagged = 0x80
}
