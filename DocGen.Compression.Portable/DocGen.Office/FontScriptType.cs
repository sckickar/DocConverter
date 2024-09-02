using System;

namespace DocGen.Office;

[Flags]
internal enum FontScriptType
{
	English = 0,
	Hindi_Devanagari = 1,
	Hindi_Devanagari_Extended = 2,
	Hindi_Vedic_Extensions = 4,
	Hindi = 7,
	Korean_Hangul = 8,
	Korean_Hangul_Jamo = 0x10,
	Korean_Hangul_Compatibility_Jamo = 0x20,
	Korean_Hangul_Jamo_ExtendedA = 0x40,
	Korean_Hangul_Jamo_ExtendedB = 0x80,
	Korean_Hangul_Syllables = 0x100,
	Korean = 0x1F8,
	Chinese_Unified_Ideographs = 0x200,
	Chinese_Unified_Ideographs_ExtensionA = 0x400,
	Chinese_Unified_Ideographs_ExtensionB1 = 0x800,
	Chinese_Unified_Ideographs_ExtensionB2 = 0x1000,
	Chinese_Compatibility_Ideographs = 0x2000,
	Chinese_HalfAndFull_width_Forms = 0x4000,
	Chinese_Symbols_And_Punctuation = 0x8000,
	Chinese = 0xFE00,
	Arabic_Unicode = 0x10000,
	Arabic_Supplement = 0x20000,
	Arabic_ExtendedA = 0x40000,
	Arabic_Presentation_FormsA = 0x80000,
	Arabic_Presentation_FormsB = 0x100000,
	Arabic = 0x1F0000,
	Hebrew_Unicode = 0x200000,
	Hebrew_Alphabetic_Presentation_Forms = 0x400000,
	Hebrew = 0x600000,
	Japanese_Katakana = 0x800000,
	Japanese_Hiragana = 0x1000000,
	Japanese = 0x1800000,
	Thai = 0x2000000
}
