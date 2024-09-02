using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Parsing;

internal class SystemFontNameFormat0 : SystemFontSystemFontName
{
	private ushort stringOffset;

	private SystemFontNameRecord[] nameRecords;

	private Dictionary<SystemFontNameRecord, string> strings;

	private string fontFamily;

	public override string FontFamily
	{
		get
		{
			if (fontFamily == null)
			{
				fontFamily = ReadString(base.Reader, 1033, 1);
			}
			return fontFamily;
		}
	}

	public SystemFontNameFormat0(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}

	private string ReadString(SystemFontOpenTypeFontReader reader, ushort languageId, ushort nameId)
	{
		foreach (SystemFontNameRecord item in FindNameRecords(3, languageId, nameId))
		{
			Encoding encodingFromEncodingID = SystemFontIDs.GetEncodingFromEncodingID(item.EncodingID);
			if (encodingFromEncodingID != null)
			{
				return ReadString(reader, item, encodingFromEncodingID);
			}
		}
		return null;
	}

	private string ReadString(SystemFontOpenTypeFontReader reader, SystemFontNameRecord record, Encoding encoding)
	{
		if (!strings.TryGetValue(record, out string value))
		{
			reader.BeginReadingBlock();
			long offset = base.Offset + stringOffset + record.Offset;
			reader.Seek(offset, SeekOrigin.Begin);
			byte[] array = new byte[record.Length];
			reader.Read(array, record.Length);
			reader.EndReadingBlock();
			value = encoding.GetString(array, 0, array.Length);
			strings[record] = value;
		}
		return value;
	}

	private IEnumerable<SystemFontNameRecord> FindNameRecords(int platformId, ushort languageId, ushort nameId)
	{
		return SystemFontEnumerable.Where(nameRecords, (SystemFontNameRecord r) => r.PlatformID == platformId && r.LanguageID == languageId && r.NameID == nameId);
	}

	internal override string ReadName(ushort languageID, ushort nameID)
	{
		return ReadString(base.Reader, languageID, nameID);
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		stringOffset = reader.ReadUShort();
		nameRecords = new SystemFontNameRecord[num];
		strings = new Dictionary<SystemFontNameRecord, string>();
		for (int i = 0; i < num; i++)
		{
			nameRecords[i] = new SystemFontNameRecord();
			nameRecords[i].Read(reader);
		}
	}
}
