using System;

namespace HarfBuzzSharp;

public struct Script : IEquatable<Script>
{
	private readonly Tag tag;

	public static readonly Script Invalid = new Script(Tag.None);

	public static readonly Script MaxValue = new Script(Tag.Max);

	public static readonly Script MaxValueSigned = new Script(Tag.MaxSigned);

	public static readonly Script Common = new Script(new Tag('Z', 'y', 'y', 'y'));

	public static readonly Script Inherited = new Script(new Tag('Z', 'i', 'n', 'h'));

	public static readonly Script Unknown = new Script(new Tag('Z', 'z', 'z', 'z'));

	public static readonly Script Arabic = new Script(new Tag('A', 'r', 'a', 'b'));

	public static readonly Script Armenian = new Script(new Tag('A', 'r', 'm', 'n'));

	public static readonly Script Bengali = new Script(new Tag('B', 'e', 'n', 'g'));

	public static readonly Script Cyrillic = new Script(new Tag('C', 'y', 'r', 'l'));

	public static readonly Script Devanagari = new Script(new Tag('D', 'e', 'v', 'a'));

	public static readonly Script Georgian = new Script(new Tag('G', 'e', 'o', 'r'));

	public static readonly Script Greek = new Script(new Tag('G', 'r', 'e', 'k'));

	public static readonly Script Gujarati = new Script(new Tag('G', 'u', 'j', 'r'));

	public static readonly Script Gurmukhi = new Script(new Tag('G', 'u', 'r', 'u'));

	public static readonly Script Hangul = new Script(new Tag('H', 'a', 'n', 'g'));

	public static readonly Script Han = new Script(new Tag('H', 'a', 'n', 'i'));

	public static readonly Script Hebrew = new Script(new Tag('H', 'e', 'b', 'r'));

	public static readonly Script Hiragana = new Script(new Tag('H', 'i', 'r', 'a'));

	public static readonly Script Kannada = new Script(new Tag('K', 'n', 'd', 'a'));

	public static readonly Script Katakana = new Script(new Tag('K', 'a', 'n', 'a'));

	public static readonly Script Lao = new Script(new Tag('L', 'a', 'o', 'o'));

	public static readonly Script Latin = new Script(new Tag('L', 'a', 't', 'n'));

	public static readonly Script Malayalam = new Script(new Tag('M', 'l', 'y', 'm'));

	public static readonly Script Oriya = new Script(new Tag('O', 'r', 'y', 'a'));

	public static readonly Script Tamil = new Script(new Tag('T', 'a', 'm', 'l'));

	public static readonly Script Telugu = new Script(new Tag('T', 'e', 'l', 'u'));

	public static readonly Script Thai = new Script(new Tag('T', 'h', 'a', 'i'));

	public static readonly Script Tibetan = new Script(new Tag('T', 'i', 'b', 't'));

	public static readonly Script Bopomofo = new Script(new Tag('B', 'o', 'p', 'o'));

	public static readonly Script Braille = new Script(new Tag('B', 'r', 'a', 'i'));

	public static readonly Script CanadianSyllabics = new Script(new Tag('C', 'a', 'n', 's'));

	public static readonly Script Cherokee = new Script(new Tag('C', 'h', 'e', 'r'));

	public static readonly Script Ethiopic = new Script(new Tag('E', 't', 'h', 'i'));

	public static readonly Script Khmer = new Script(new Tag('K', 'h', 'm', 'r'));

	public static readonly Script Mongolian = new Script(new Tag('M', 'o', 'n', 'g'));

	public static readonly Script Myanmar = new Script(new Tag('M', 'y', 'm', 'r'));

	public static readonly Script Ogham = new Script(new Tag('O', 'g', 'a', 'm'));

	public static readonly Script Runic = new Script(new Tag('R', 'u', 'n', 'r'));

	public static readonly Script Sinhala = new Script(new Tag('S', 'i', 'n', 'h'));

	public static readonly Script Syriac = new Script(new Tag('S', 'y', 'r', 'c'));

	public static readonly Script Thaana = new Script(new Tag('T', 'h', 'a', 'a'));

	public static readonly Script Yi = new Script(new Tag('Y', 'i', 'i', 'i'));

	public static readonly Script Deseret = new Script(new Tag('D', 's', 'r', 't'));

	public static readonly Script Gothic = new Script(new Tag('G', 'o', 't', 'h'));

	public static readonly Script OldItalic = new Script(new Tag('I', 't', 'a', 'l'));

	public static readonly Script Buhid = new Script(new Tag('B', 'u', 'h', 'd'));

	public static readonly Script Hanunoo = new Script(new Tag('H', 'a', 'n', 'o'));

	public static readonly Script Tagalog = new Script(new Tag('T', 'g', 'l', 'g'));

	public static readonly Script Tagbanwa = new Script(new Tag('T', 'a', 'g', 'b'));

	public static readonly Script Cypriot = new Script(new Tag('C', 'p', 'r', 't'));

	public static readonly Script Limbu = new Script(new Tag('L', 'i', 'm', 'b'));

	public static readonly Script LinearB = new Script(new Tag('L', 'i', 'n', 'b'));

	public static readonly Script Osmanya = new Script(new Tag('O', 's', 'm', 'a'));

	public static readonly Script Shavian = new Script(new Tag('S', 'h', 'a', 'w'));

	public static readonly Script TaiLe = new Script(new Tag('T', 'a', 'l', 'e'));

	public static readonly Script Ugaritic = new Script(new Tag('U', 'g', 'a', 'r'));

	public static readonly Script Buginese = new Script(new Tag('B', 'u', 'g', 'i'));

	public static readonly Script Coptic = new Script(new Tag('C', 'o', 'p', 't'));

	public static readonly Script Glagolitic = new Script(new Tag('G', 'l', 'a', 'g'));

	public static readonly Script Kharoshthi = new Script(new Tag('K', 'h', 'a', 'r'));

	public static readonly Script NewTaiLue = new Script(new Tag('T', 'a', 'l', 'u'));

	public static readonly Script OldPersian = new Script(new Tag('X', 'p', 'e', 'o'));

	public static readonly Script SylotiNagri = new Script(new Tag('S', 'y', 'l', 'o'));

	public static readonly Script Tifinagh = new Script(new Tag('T', 'f', 'n', 'g'));

	public static readonly Script Balinese = new Script(new Tag('B', 'a', 'l', 'i'));

	public static readonly Script Cuneiform = new Script(new Tag('X', 's', 'u', 'x'));

	public static readonly Script Nko = new Script(new Tag('N', 'k', 'o', 'o'));

	public static readonly Script PhagsPa = new Script(new Tag('P', 'h', 'a', 'g'));

	public static readonly Script Phoenician = new Script(new Tag('P', 'h', 'n', 'x'));

	public static readonly Script Carian = new Script(new Tag('C', 'a', 'r', 'i'));

	public static readonly Script Cham = new Script(new Tag('C', 'h', 'a', 'm'));

	public static readonly Script KayahLi = new Script(new Tag('K', 'a', 'l', 'i'));

	public static readonly Script Lepcha = new Script(new Tag('L', 'e', 'p', 'c'));

	public static readonly Script Lycian = new Script(new Tag('L', 'y', 'c', 'i'));

	public static readonly Script Lydian = new Script(new Tag('L', 'y', 'd', 'i'));

	public static readonly Script OlChiki = new Script(new Tag('O', 'l', 'c', 'k'));

	public static readonly Script Rejang = new Script(new Tag('R', 'j', 'n', 'g'));

	public static readonly Script Saurashtra = new Script(new Tag('S', 'a', 'u', 'r'));

	public static readonly Script Sundanese = new Script(new Tag('S', 'u', 'n', 'd'));

	public static readonly Script Vai = new Script(new Tag('V', 'a', 'i', 'i'));

	public static readonly Script Avestan = new Script(new Tag('A', 'v', 's', 't'));

	public static readonly Script Bamum = new Script(new Tag('B', 'a', 'm', 'u'));

	public static readonly Script EgyptianHieroglyphs = new Script(new Tag('E', 'g', 'y', 'p'));

	public static readonly Script ImperialAramaic = new Script(new Tag('A', 'r', 'm', 'i'));

	public static readonly Script InscriptionalPahlavi = new Script(new Tag('P', 'h', 'l', 'i'));

	public static readonly Script InscriptionalParthian = new Script(new Tag('P', 'r', 't', 'i'));

	public static readonly Script Javanese = new Script(new Tag('J', 'a', 'v', 'a'));

	public static readonly Script Kaithi = new Script(new Tag('K', 't', 'h', 'i'));

	public static readonly Script Lisu = new Script(new Tag('L', 'i', 's', 'u'));

	public static readonly Script MeeteiMayek = new Script(new Tag('M', 't', 'e', 'i'));

	public static readonly Script OldSouthArabian = new Script(new Tag('S', 'a', 'r', 'b'));

	public static readonly Script OldTurkic = new Script(new Tag('O', 'r', 'k', 'h'));

	public static readonly Script Samaritan = new Script(new Tag('S', 'a', 'm', 'r'));

	public static readonly Script TaiTham = new Script(new Tag('L', 'a', 'n', 'a'));

	public static readonly Script TaiViet = new Script(new Tag('T', 'a', 'v', 't'));

	public static readonly Script Batak = new Script(new Tag('B', 'a', 't', 'k'));

	public static readonly Script Brahmi = new Script(new Tag('B', 'r', 'a', 'h'));

	public static readonly Script Mandaic = new Script(new Tag('M', 'a', 'n', 'd'));

	public static readonly Script Chakma = new Script(new Tag('C', 'a', 'k', 'm'));

	public static readonly Script MeroiticCursive = new Script(new Tag('M', 'e', 'r', 'c'));

	public static readonly Script MeroiticHieroglyphs = new Script(new Tag('M', 'e', 'r', 'o'));

	public static readonly Script Miao = new Script(new Tag('P', 'l', 'r', 'd'));

	public static readonly Script Sharada = new Script(new Tag('S', 'h', 'r', 'd'));

	public static readonly Script SoraSompeng = new Script(new Tag('S', 'o', 'r', 'a'));

	public static readonly Script Takri = new Script(new Tag('T', 'a', 'k', 'r'));

	public static readonly Script BassaVah = new Script(new Tag('B', 'a', 's', 's'));

	public static readonly Script CaucasianAlbanian = new Script(new Tag('A', 'g', 'h', 'b'));

	public static readonly Script Duployan = new Script(new Tag('D', 'u', 'p', 'l'));

	public static readonly Script Elbasan = new Script(new Tag('E', 'l', 'b', 'a'));

	public static readonly Script Grantha = new Script(new Tag('G', 'r', 'a', 'n'));

	public static readonly Script Khojki = new Script(new Tag('K', 'h', 'o', 'j'));

	public static readonly Script Khudawadi = new Script(new Tag('S', 'i', 'n', 'd'));

	public static readonly Script LinearA = new Script(new Tag('L', 'i', 'n', 'a'));

	public static readonly Script Mahajani = new Script(new Tag('M', 'a', 'h', 'j'));

	public static readonly Script Manichaean = new Script(new Tag('M', 'a', 'n', 'i'));

	public static readonly Script MendeKikakui = new Script(new Tag('M', 'e', 'n', 'd'));

	public static readonly Script Modi = new Script(new Tag('M', 'o', 'd', 'i'));

	public static readonly Script Mro = new Script(new Tag('M', 'r', 'o', 'o'));

	public static readonly Script Nabataean = new Script(new Tag('N', 'b', 'a', 't'));

	public static readonly Script OldNorthArabian = new Script(new Tag('N', 'a', 'r', 'b'));

	public static readonly Script OldPermic = new Script(new Tag('P', 'e', 'r', 'm'));

	public static readonly Script PahawhHmong = new Script(new Tag('H', 'm', 'n', 'g'));

	public static readonly Script Palmyrene = new Script(new Tag('P', 'a', 'l', 'm'));

	public static readonly Script PauCinHau = new Script(new Tag('P', 'a', 'u', 'c'));

	public static readonly Script PsalterPahlavi = new Script(new Tag('P', 'h', 'l', 'p'));

	public static readonly Script Siddham = new Script(new Tag('S', 'i', 'd', 'd'));

	public static readonly Script Tirhuta = new Script(new Tag('T', 'i', 'r', 'h'));

	public static readonly Script WarangCiti = new Script(new Tag('W', 'a', 'r', 'a'));

	public static readonly Script Ahom = new Script(new Tag('A', 'h', 'o', 'm'));

	public static readonly Script AnatolianHieroglyphs = new Script(new Tag('H', 'l', 'u', 'w'));

	public static readonly Script Hatran = new Script(new Tag('H', 'a', 't', 'r'));

	public static readonly Script Multani = new Script(new Tag('M', 'u', 'l', 't'));

	public static readonly Script OldHungarian = new Script(new Tag('H', 'u', 'n', 'g'));

	public static readonly Script Signwriting = new Script(new Tag('S', 'g', 'n', 'w'));

	public static readonly Script Adlam = new Script(new Tag('A', 'd', 'l', 'm'));

	public static readonly Script Bhaiksuki = new Script(new Tag('B', 'h', 'k', 's'));

	public static readonly Script Marchen = new Script(new Tag('M', 'a', 'r', 'c'));

	public static readonly Script Osage = new Script(new Tag('O', 's', 'g', 'e'));

	public static readonly Script Tangut = new Script(new Tag('T', 'a', 'n', 'g'));

	public static readonly Script Newa = new Script(new Tag('N', 'e', 'w', 'a'));

	public static readonly Script MasaramGondi = new Script(new Tag('G', 'o', 'n', 'm'));

	public static readonly Script Nushu = new Script(new Tag('N', 's', 'h', 'u'));

	public static readonly Script Soyombo = new Script(new Tag('S', 'o', 'y', 'o'));

	public static readonly Script ZanabazarSquare = new Script(new Tag('Z', 'a', 'n', 'b'));

	public static readonly Script Dogra = new Script(new Tag('D', 'o', 'g', 'r'));

	public static readonly Script GunjalaGondi = new Script(new Tag('G', 'o', 'n', 'g'));

	public static readonly Script HanifiRohingya = new Script(new Tag('R', 'o', 'h', 'g'));

	public static readonly Script Makasar = new Script(new Tag('M', 'a', 'k', 'a'));

	public static readonly Script Medefaidrin = new Script(new Tag('M', 'e', 'd', 'f'));

	public static readonly Script OldSogdian = new Script(new Tag('S', 'o', 'g', 'o'));

	public static readonly Script Sogdian = new Script(new Tag('S', 'o', 'g', 'd'));

	public Direction HorizontalDirection => HarfBuzzApi.hb_script_get_horizontal_direction(tag);

	private Script(Tag tag)
	{
		this.tag = tag;
	}

	public static Script Parse(string str)
	{
		return HarfBuzzApi.hb_script_from_string(str, -1);
	}

	public static bool TryParse(string str, out Script script)
	{
		script = Parse(str);
		return (uint)script != (uint)Unknown;
	}

	public override string ToString()
	{
		return tag.ToString();
	}

	public static implicit operator uint(Script script)
	{
		return script.tag;
	}

	public static implicit operator Script(uint tag)
	{
		return new Script(tag);
	}

	public override bool Equals(object obj)
	{
		if (obj is Script script)
		{
			return tag.Equals(script.tag);
		}
		return false;
	}

	public bool Equals(Script other)
	{
		return tag.Equals(other.tag);
	}

	public override int GetHashCode()
	{
		return tag.GetHashCode();
	}
}
