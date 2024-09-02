using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;

namespace DocGen.Pdf;

internal class FontFile2
{
	private const long serialVersionUID = -3097990864237320960L;

	private RectangularArrays m_rectangularArrays = new RectangularArrays();

	public const int HEAD = 0;

	public const int MAXP = 1;

	public const int CMAP = 2;

	public const int LOCA = 3;

	public const int GLYF = 4;

	public const int HHEA = 5;

	public const int HMTX = 6;

	public const int NAME = 7;

	public const int POST = 8;

	public const int CVT = 9;

	public const int FPGM = 10;

	public const int HDMX = 11;

	public const int KERN = 12;

	public const int OS2 = 13;

	public const int PREP = 14;

	public const int DSIG = 15;

	public const int CFF = 16;

	public const int GSUB = 17;

	public const int BASE = 18;

	public const int EBDT = 19;

	public const int EBLC = 20;

	public const int GASP = 21;

	public const int VHEA = 22;

	public const int VMTX = 23;

	public const int GDEF = 24;

	public const int JSTF = 25;

	public const int LTSH = 26;

	public const int PCLT = 27;

	public const int VDMX = 28;

	public const int BSLN = 29;

	public const int MORT = 30;

	public const int FDSC = 31;

	public const int FFTM = 32;

	public const int GPOS = 33;

	public const int FEAT = 34;

	public const int JUST = 35;

	public const int PROP = 36;

	protected internal int tableCount = 37;

	protected internal int[][] checksums;

	protected internal int[][] tables;

	protected internal int[][] tableLength;

	protected internal int[][] offsets;

	public byte[] fontDataAsArray;

	private int m_offset;

	private int id;

	private int m_firstCode;

	private ushort[] encodevalue;

	private ushort[] startcodevalue;

	private short[] idDeltavalue;

	private ushort[] array4;

	private List<ushort[]> notable = new List<ushort[]>();

	private ushort[] glyphsIdValue;

	private ushort numgl;

	private MemoryStream cmapStream = new MemoryStream();

	private int numglyphs;

	private uint[] locaoffset;

	private Maxp m_maxp;

	private Head m_head;

	private IndexLocation m_loca;

	private TrueTypeCmap m_cmap;

	private ReadFontArray m_reader;

	private TrueTypeGlyphs m_trueTypeGlypf;

	private Dictionary<ushort, TrueTypeGlyphs> pathtable = new Dictionary<ushort, TrueTypeGlyphs>();

	private bool m_isfontfile2;

	private bool useArray = true;

	protected internal List<string> tableList = new List<string>();

	internal List<TableEntry> tableEntries = new List<TableEntry>();

	internal Dictionary<int, TableEntry> table = new Dictionary<int, TableEntry>();

	internal int pointer;

	public const int OPENTYPE = 1;

	public const int TRUETYPE = 2;

	public const int TTC = 3;

	protected internal int type = 2;

	public int currentFontID;

	internal int fontCount = 1;

	public int segment;

	protected internal int numTables = 11;

	protected internal int searchRange = 128;

	protected internal int entrySelector = 3;

	protected internal int rangeShift = 48;

	public List<OutlinePoint[]> contours;

	private ushort noofSubtable;

	public bool IsFontFile2
	{
		get
		{
			return m_isfontfile2;
		}
		set
		{
			m_isfontfile2 = value;
		}
	}

	public ReadFontArray FontArrayReader
	{
		get
		{
			if (m_reader == null)
			{
				m_reader = new ReadFontArray(fontDataAsArray);
			}
			return m_reader;
		}
	}

	public byte[] FontFileArrayData => fontDataAsArray;

	public Maxp MaximumProfile
	{
		get
		{
			if (m_maxp == null)
			{
				m_maxp = new Maxp(this);
				ReadTable(m_maxp);
			}
			return m_maxp;
		}
	}

	public IndexLocation Loca
	{
		get
		{
			if (m_loca == null)
			{
				m_loca = new IndexLocation(this);
				ReadTable(m_loca);
			}
			return m_loca;
		}
	}

	public TrueTypeCmap Cmap
	{
		get
		{
			if (m_cmap == null)
			{
				m_cmap = new TrueTypeCmap(this);
				ReadTable(m_cmap);
			}
			return m_cmap;
		}
	}

	public Head Header
	{
		get
		{
			if (m_head == null)
			{
				m_head = new Head(this);
				ReadTable(m_head);
			}
			return m_head;
		}
	}

	public TrueTypeCmap CmapTable
	{
		get
		{
			_ = m_cmap;
			return m_cmap;
		}
	}

	internal List<ushort[]> Segments
	{
		get
		{
			return notable;
		}
		set
		{
			notable = value;
		}
	}

	public TrueTypeGlyphs TrueTypeFontGlyf
	{
		get
		{
			if (m_trueTypeGlypf == null)
			{
				m_trueTypeGlypf = new TrueTypeGlyphs(this);
				ReadTable(m_trueTypeGlypf);
			}
			return m_trueTypeGlypf;
		}
	}

	public int FirstCode
	{
		get
		{
			return m_firstCode;
		}
		set
		{
			m_firstCode = value;
		}
	}

	public int NumGlyphs => MaximumProfile.NumGlyphs;

	public int OffsetVal
	{
		get
		{
			m_offset = getOffsets(id);
			return m_offset;
		}
		set
		{
			m_offset = value;
		}
	}

	public FontFile2(byte[] data)
	{
		m_isfontfile2 = true;
		useArray = true;
		fontDataAsArray = data;
		readHeader();
	}

	public FontFile2()
	{
	}

	private void readHeader()
	{
		switch (FontArrayReader.getnextUint32())
		{
		case 1330926671:
			type = 1;
			break;
		case 1953784678:
			type = 3;
			break;
		}
		if (type == 3)
		{
			FontArrayReader.getnextUint32();
			fontCount = FontArrayReader.getnextUint32();
			checksums = m_rectangularArrays.ReturnRectangularIntArray(tableCount, fontCount);
			tables = m_rectangularArrays.ReturnRectangularIntArray(tableCount, fontCount);
			tableLength = m_rectangularArrays.ReturnRectangularIntArray(tableCount, fontCount);
			int[] array = new int[fontCount];
			for (int i = 0; i < fontCount; i++)
			{
				currentFontID = i;
				int num = FontArrayReader.getnextUint32();
				array[i] = num;
			}
			for (int j = 0; j < fontCount; j++)
			{
				currentFontID = j;
				pointer = array[j];
				int num2 = FontArrayReader.getnextUint32();
				readTablesForFont();
			}
			currentFontID = 0;
		}
		else
		{
			checksums = m_rectangularArrays.ReturnRectangularIntArray(tableCount, 1);
			tables = m_rectangularArrays.ReturnRectangularIntArray(tableCount, 1);
			tableLength = m_rectangularArrays.ReturnRectangularIntArray(tableCount, 1);
			readTablesForFont();
		}
	}

	private void readTablesForFont()
	{
		numTables = FontArrayReader.getnextUint16();
		searchRange = FontArrayReader.getnextUint16();
		entrySelector = FontArrayReader.getnextUint16();
		rangeShift = FontArrayReader.getnextUint16();
		for (int i = 0; i < numTables; i++)
		{
			TableEntry tableEntry = new TableEntry();
			tableEntry.id = FontArrayReader.getnextUint32AsTag();
			tableEntry.checkSum = FontArrayReader.getnextUint32();
			tableEntry.offset = FontArrayReader.getnextUint32();
			tableEntry.length = FontArrayReader.getnextUint32();
			tableList.Add(tableEntry.id);
			tableEntries.Add(tableEntry);
			int tableID = getTableID(tableEntry.id);
			if (tableID != -1)
			{
				if (table.ContainsKey(tableID))
				{
					table.Remove(tableID);
				}
				table.Add(tableID, tableEntry);
				checksums[tableID][currentFontID] = tableEntry.checkSum;
				tables[tableID][currentFontID] = tableEntry.offset;
				tableLength[tableID][currentFontID] = tableEntry.length;
			}
		}
	}

	public void ReadTable(TableBase tabb)
	{
		if (table.TryGetValue(tabb.Id, out TableEntry value))
		{
			int num = FontArrayReader.Pointer;
			FontArrayReader.Pointer = value.offset;
			tabb.Read(FontArrayReader);
			tabb.Offset = value.offset;
			FontArrayReader.Pointer = num;
		}
	}

	public string GetWinencodeCharactername(byte val, string Winansi)
	{
		return PredefinedEncoding.GetPredefinedEncoding(Winansi).GetNames()[val];
	}

	public ushort GetChatid(byte b)
	{
		(new byte[1])[0] = b;
		return b;
	}

	private bool TryAppendByte(byte b, out ushort res)
	{
		res = 0;
		try
		{
			byte[] @byte = GetByte(FirstCode);
			byte[] val = new byte[2]
			{
				@byte[0],
				b
			};
			int @int = GetInt(val);
			res = (ushort)@int;
			return true;
		}
		catch
		{
			return false;
		}
	}

	public int GetInt(byte[] val)
	{
		int num = 0;
		int num2 = val.Length;
		for (int i = 0; i < num2; i++)
		{
			num |= ((num2 > i) ? (val[i] & 0xFF) : 0);
			if (i < num2 - 1)
			{
				num <<= 8;
			}
		}
		return num;
	}

	public byte[] GetByte(int val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		byte[] obj = new byte[2]
		{
			bytes[0],
			bytes[1]
		};
		Array.Reverse(obj);
		return obj;
	}

	public void GetFirstCode(CmapTables unicode)
	{
		if (m_firstCode == 0)
		{
			m_firstCode = unicode.FirstCode;
		}
	}

	protected internal int getTableID(string tag)
	{
		id = -1;
		if (tag.Equals("maxp"))
		{
			id = 1;
		}
		else if (tag.Equals("head"))
		{
			id = 0;
		}
		else if (tag.Equals("cmap"))
		{
			id = 2;
		}
		else if (tag.Equals("loca"))
		{
			id = 3;
		}
		else if (tag.Equals("glyf"))
		{
			id = 4;
		}
		else if (tag.Equals("hhea"))
		{
			id = 5;
		}
		else if (tag.Equals("hmtx"))
		{
			id = 6;
		}
		else if (tag.Equals("name"))
		{
			id = 7;
		}
		else if (tag.Equals("post"))
		{
			id = 8;
		}
		else if (tag.Equals("cvt "))
		{
			id = 9;
		}
		else if (tag.Equals("fpgm"))
		{
			id = 10;
		}
		else if (tag.Equals("hdmx"))
		{
			id = 11;
		}
		else if (tag.Equals("kern"))
		{
			id = 12;
		}
		else if (tag.Equals("OS/2"))
		{
			id = 13;
		}
		else if (tag.Equals("prep"))
		{
			id = 14;
		}
		else if (tag.Equals("DSIG"))
		{
			id = 15;
		}
		else if (tag.Equals("BASE"))
		{
			id = 18;
		}
		else if (tag.Equals("CFF "))
		{
			id = 16;
		}
		else if (tag.Equals("GSUB"))
		{
			id = 17;
		}
		else if (tag.Equals("EBDT"))
		{
			id = 19;
		}
		else if (tag.Equals("EBLC"))
		{
			id = 20;
		}
		else if (tag.Equals("gasp"))
		{
			id = 21;
		}
		else if (tag.Equals("vhea"))
		{
			id = 22;
		}
		else if (tag.Equals("vmtx"))
		{
			id = 23;
		}
		else if (tag.Equals("GDEF"))
		{
			id = 24;
		}
		else if (tag.Equals("JSTF"))
		{
			id = 25;
		}
		else if (tag.Equals("LTSH"))
		{
			id = 26;
		}
		else if (tag.Equals("PCLT"))
		{
			id = 27;
		}
		else if (tag.Equals("VDMX"))
		{
			id = 28;
		}
		else if (tag.Equals("mort"))
		{
			id = 30;
		}
		else if (tag.Equals("bsln"))
		{
			id = 29;
		}
		else if (tag.Equals("fdsc"))
		{
			id = 31;
		}
		else if (tag.Equals("FFTM"))
		{
			id = 32;
		}
		else if (tag.Equals("GPOS"))
		{
			id = 33;
		}
		else if (tag.Equals("feat"))
		{
			id = 34;
		}
		else if (tag.Equals("just"))
		{
			id = 35;
		}
		else if (tag.Equals("prop"))
		{
			id = 36;
		}
		return id;
	}

	public TrueTypeGlyphs readGlyphdata(ushort value)
	{
		TrueTypeGlyphs trueTypeGlyphs;
		if (!pathtable.ContainsKey(value))
		{
			long offset = Loca.GetOffset(value);
			table.TryGetValue(TrueTypeFontGlyf.Id, out TableEntry value2);
			if (offset == -1 || value2 == null || offset >= value2.offset + value2.length)
			{
				return new TrueTypeGlyphs(this, value);
			}
			int num = FontArrayReader.Pointer;
			FontArrayReader.Pointer = (int)offset + value2.offset;
			trueTypeGlyphs = TrueTypeGlyphs.ReadGlyf(this, value);
			FontArrayReader.Pointer = num;
			pathtable[value] = trueTypeGlyphs;
		}
		else
		{
			trueTypeGlyphs = pathtable[value];
		}
		return trueTypeGlyphs;
	}

	private PointF GetMidPoint(PointF a, PointF b)
	{
		return new PointF((float)((double)(a.X + b.X) / 2.0), (float)((double)(a.Y + b.Y) / 2.0));
	}

	public PointF ConvertFunittoPoint(PointF units, float fontSize)
	{
		double num = (double)units.X * 72.0 * (double)fontSize / (72.0 * (double)(int)Header.UnitsPerEm);
		double num2 = (double)units.Y * 72.0 * (double)fontSize / (72.0 * (double)(int)Header.UnitsPerEm);
		return new PointF((float)num, (float)(0.0 - num2));
	}

	private static bool XIsByte(byte[] flags, int index)
	{
		return GetBit(flags[index], 1);
	}

	private static bool YIsByte(byte[] flags, int index)
	{
		return GetBit(flags[index], 2);
	}

	private static bool XIsSame(byte[] flags, int index)
	{
		return GetBit(flags[index], 4);
	}

	private static bool YIsSame(byte[] flags, int index)
	{
		return GetBit(flags[index], 5);
	}

	private static bool Repeat(byte[] flags, int index)
	{
		return GetBit(flags[index], 3);
	}

	internal static bool GetBit(int n, byte bit)
	{
		return (n & (1 << (int)bit)) != 0;
	}

	public int getOffsets(int tableID)
	{
		m_offset = tables[tableID][currentFontID];
		return m_offset;
	}

	public MemoryStream Getcmapstream()
	{
		return cmapStream;
	}

	private void WriteByte(byte value)
	{
		byte[] buffer = new byte[1] { value };
		cmapStream.Write(buffer, 0, 1);
	}

	private void WriteuShort(short value)
	{
		byte[] array = new byte[2];
		array = BitConverter.GetBytes(value);
		for (int num = 1; num >= 0; num--)
		{
			WriteByte(array[num]);
		}
	}

	private void WriteShort(ushort value)
	{
		byte[] array = new byte[2];
		array = BitConverter.GetBytes(value);
		for (int num = 1; num >= 0; num--)
		{
			WriteByte(array[num]);
		}
	}

	private void WriteLong(ulong value)
	{
		byte[] array = new byte[4];
		array = BitConverter.GetBytes(value);
		for (int num = 3; num >= 0; num--)
		{
			WriteByte(array[num]);
		}
	}

	private void Write4()
	{
		WriteShort(4);
		WriteShort((ushort)m_firstCode);
		WriteShort(numgl);
		for (int i = 0; i < numgl; i++)
		{
			WriteShort(startcodevalue[i]);
			WriteShort(encodevalue[i]);
			WriteuShort(idDeltavalue[i]);
			WriteShort((ushort)notable[i].Length);
			for (int j = 0; j < notable[i].Length; j++)
			{
				WriteShort(notable[i][j]);
			}
		}
	}

	public void Write6()
	{
		WriteShort(6);
		WriteShort((ushort)m_firstCode);
		WriteShort((ushort)glyphsIdValue.Length);
		for (int i = 0; i < glyphsIdValue.Length; i++)
		{
			WriteShort(glyphsIdValue[i]);
		}
	}

	public byte getnextbyte()
	{
		byte result = fontDataAsArray[pointer];
		pointer++;
		return result;
	}

	public int getnextUint32()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 4; i++)
		{
			num2 = ((pointer < fontDataAsArray.Length) ? (fontDataAsArray[pointer] & 0xFF) : 0);
			num += num2 << 8 * (3 - i);
			pointer++;
		}
		return num;
	}

	public int getnextUint64()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 8; i++)
		{
			num2 = fontDataAsArray[pointer];
			if (num2 < 0)
			{
				num2 = 256 + num2;
			}
			num += num2 << 8 * (7 - i);
			pointer++;
		}
		return num;
	}

	public string getnextUint32AsTag()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 4; i++)
		{
			char value = (char)fontDataAsArray[pointer];
			stringBuilder.Append(value);
			pointer++;
		}
		return stringBuilder.ToString();
	}

	public int getnextUint16()
	{
		int num = 0;
		for (int i = 0; i < 2; i++)
		{
			if (fontDataAsArray.Length != 0)
			{
				int num2 = fontDataAsArray[pointer] & 0xFF;
				num += num2 << 8 * (1 - i);
			}
			pointer++;
		}
		return num;
	}

	public ushort getnextUshort()
	{
		byte[] array = new byte[2];
		for (int num = 1; num >= 0; num--)
		{
			if (fontDataAsArray.Length != 0)
			{
				array[num] = fontDataAsArray[pointer];
			}
			pointer++;
		}
		return BitConverter.ToUInt16(array, 0);
	}

	public ulong getnextULong()
	{
		byte[] array = new byte[4];
		for (int num = 3; num >= 0; num--)
		{
			if (fontDataAsArray.Length != 0)
			{
				array[num] = fontDataAsArray[pointer];
			}
			pointer++;
		}
		return BitConverter.ToUInt32(array, 0);
	}

	public uint getULong()
	{
		byte[] array = new byte[4];
		for (int num = 3; num >= 0; num--)
		{
			if (fontDataAsArray.Length != 0)
			{
				array[num] = fontDataAsArray[pointer];
			}
			pointer++;
		}
		return BitConverter.ToUInt32(array, 0);
	}

	public short getnextshort()
	{
		byte[] array = new byte[2] { 0, 0 };
		for (int num = 1; num >= 0; num--)
		{
			if (fontDataAsArray.Length != 0)
			{
				array[num] = fontDataAsArray[pointer];
			}
			pointer++;
		}
		return BitConverter.ToInt16(array, 0);
	}

	public byte[] getTableBytes(int tableID)
	{
		if (id != 2)
		{
			int sourceIndex = tables[tableID][currentFontID];
			int num = tableLength[tableID][currentFontID];
			byte[] array = new byte[num];
			Array.Copy(fontDataAsArray, sourceIndex, array, 0, num);
			return array;
		}
		return cmapStream.GetBuffer();
	}

	internal byte[] getTableBytes(int tableID, bool isTrueType)
	{
		if (id == -1)
		{
			return null;
		}
		if (id != 2 || isTrueType)
		{
			int sourceIndex = tables[tableID][currentFontID];
			int num = tableLength[tableID][currentFontID];
			byte[] array = new byte[num];
			Array.Copy(fontDataAsArray, sourceIndex, array, 0, num);
			return array;
		}
		return cmapStream.GetBuffer();
	}

	internal void Dispose()
	{
		fontDataAsArray = null;
		if (pathtable != null && pathtable.Count > 0)
		{
			pathtable.Clear();
			pathtable = null;
		}
		if (cmapStream != null)
		{
			cmapStream.Dispose();
			cmapStream = null;
		}
		if (notable != null && notable.Count > 0)
		{
			notable.Clear();
			notable = null;
		}
		if (tableList != null && tableList.Count > 0)
		{
			tableList.Clear();
			tableList = null;
		}
		if (tableEntries != null && tableEntries.Count > 0)
		{
			tableEntries.Clear();
			tableEntries = null;
		}
		if (table != null && table.Count > 0)
		{
			table.Clear();
			table = null;
		}
	}
}
