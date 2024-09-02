using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontCharset : SystemFontCFFTable
{
	internal const ushort NotDefIndex = 0;

	private readonly int count;

	private Dictionary<string, ushort> indices;

	private string[] names;

	public ushort this[string name]
	{
		get
		{
			if (indices.TryGetValue(name, out var value))
			{
				return value;
			}
			return 0;
		}
	}

	public string this[ushort index] => names[index];

	public SystemFontCharset(SystemFontCFFFontFile file, long offset, int count)
		: base(file, offset)
	{
		this.count = count;
	}

	public SystemFontCharset(SystemFontCFFFontFile file, ushort[] glyphs)
		: base(file, -1L)
	{
		Initialize(glyphs);
	}

	private List<ushort> ReadFormat0(SystemFontCFFFontReader reader)
	{
		int num = count - 1;
		List<ushort> list = new List<ushort>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(reader.ReadSID());
		}
		return list;
	}

	private List<ushort> ReadFormat1(SystemFontCFFFontReader reader)
	{
		int num = count - 1;
		List<ushort> list = new List<ushort>(num);
		while (list.Count < num)
		{
			ushort num2 = reader.ReadSID();
			byte b = reader.ReadCard8();
			list.Add(num2);
			for (int i = 0; i < b; i++)
			{
				list.Add((ushort)(num2 + i + 1));
			}
		}
		return list;
	}

	private List<ushort> ReadFormat2(SystemFontCFFFontReader reader)
	{
		int num = count - 1;
		List<ushort> list = new List<ushort>(num);
		while (list.Count < num)
		{
			ushort num2 = reader.ReadSID();
			ushort num3 = reader.ReadCard16();
			list.Add(num2);
			for (int i = 0; i < num3; i++)
			{
				list.Add((ushort)(num2 + i + 1));
			}
		}
		return list;
	}

	private void Initialize(ushort[] glyphs)
	{
		indices = new Dictionary<string, ushort>(glyphs.Length);
		names = new string[glyphs.Length];
		for (ushort num = 0; num < glyphs.Length; num++)
		{
			string text = base.File.ReadString(glyphs[num]);
			indices[text] = (ushort)(num + 1);
			names[num] = text;
		}
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		Initialize((reader.ReadCard8() switch
		{
			0 => ReadFormat0(reader), 
			1 => ReadFormat1(reader), 
			2 => ReadFormat2(reader), 
			_ => throw new NotSupportedException("Charset format is not supported."), 
		}).ToArray());
	}
}
