using System;
using System.IO;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListInfo : BaseWordRecord
{
	private const int DEF_LIST_ID = 1720085641;

	private const string DEF_BULLLET_FIRST = "\uf0b7";

	private const string DEF_BULLLET_SECOND = "o";

	private const string DEF_BULLLET_THIRD = "\uf0a7";

	private const int DEF_MULTIPLIER = 1440;

	private int m_listid = 1720085641;

	private ListFormats m_listFormats;

	private ListFormatOverrides m_listFormatOverrides;

	internal ListFormatOverrides ListFormatOverrides => m_listFormatOverrides;

	internal ListFormats ListFormats => m_listFormats;

	internal ListInfo()
	{
		m_listFormats = new ListFormats();
		m_listFormatOverrides = new ListFormatOverrides();
	}

	internal ListInfo(Fib fib, Stream stream)
	{
		m_listFormats = new ListFormats();
		m_listFormatOverrides = new ListFormatOverrides();
		ReadLst(fib, stream);
		ReadLfo(fib, stream);
		ReadStringTable(fib, stream);
	}

	internal void ReadLst(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbPlfLst == 0)
		{
			return;
		}
		stream.Position = fib.FibRgFcLcb97FcPlfLst;
		int num = BaseWordRecord.ReadInt16(stream);
		for (int i = 0; i < num; i++)
		{
			m_listFormats.Add(new ListData(stream));
		}
		foreach (ListData listFormat in m_listFormats)
		{
			listFormat.ReadLvl(stream);
		}
	}

	internal void ReadLfo(Fib fib, Stream stream)
	{
		if (fib.FibRgFcLcb97LcbPlfLfo == 0)
		{
			return;
		}
		stream.Position = fib.FibRgFcLcb97FcPlfLfo;
		int num = BaseWordRecord.ReadInt32(stream);
		for (int i = 0; i < num; i++)
		{
			m_listFormatOverrides.Add(new ListFormatOverride(stream));
		}
		foreach (ListFormatOverride listFormatOverride in m_listFormatOverrides)
		{
			listFormatOverride.ReadLfoLvls(stream);
		}
	}

	internal void ReadStringTable(Fib fib, Stream stream)
	{
		stream.Position = fib.FibRgFcLcb97FcSttbListNames;
	}

	internal int WriteLfo(Stream stream)
	{
		if (m_listFormatOverrides.Count == 0)
		{
			return 0;
		}
		int num = (int)stream.Position;
		BaseWordRecord.WriteInt32(stream, m_listFormatOverrides.Count);
		foreach (ListFormatOverride listFormatOverride in m_listFormatOverrides)
		{
			listFormatOverride.WriteLfo(stream);
		}
		foreach (ListFormatOverride listFormatOverride2 in m_listFormatOverrides)
		{
			listFormatOverride2.WriteLfoLvls(stream);
		}
		return (int)stream.Position - num;
	}

	internal int WriteLst(Stream stream)
	{
		if (m_listFormats.Count == 0)
		{
			return 0;
		}
		int num = (int)stream.Position;
		BaseWordRecord.WriteInt16(stream, (short)m_listFormats.Count);
		foreach (ListData listFormat in m_listFormats)
		{
			listFormat.WriteListData(stream);
		}
		int result = (int)stream.Position - num;
		foreach (ListData listFormat2 in m_listFormats)
		{
			listFormat2.WriteLvl(stream);
		}
		return result;
	}

	internal int WriteStringTable(Stream stream)
	{
		byte[] array = new byte[8];
		byte b;
		array[1] = (b = byte.MaxValue);
		array[0] = b;
		array[2] = 1;
		stream.Write(array, 0, array.Length);
		return array.Length;
	}

	internal short ApplyNumberList()
	{
		ListData listData = new ListData(m_listid);
		m_listid++;
		m_listFormats.Add(listData);
		int num = 0;
		for (float num2 = 0.5f; num2 < 4.5f; num2 += 1.5f)
		{
			listData.Levels.Add(CreateNumberLvl((int)(1440f * num2), num++, ListPatternType.Arabic, ListNumberAlignment.Left));
			listData.Levels.Add(CreateNumberLvl((int)(1440.0 * ((double)num2 + 0.5)), num++, ListPatternType.LowLetter, ListNumberAlignment.Right));
			listData.Levels.Add(CreateNumberLvl((int)(1440f * (num2 + 1f)), num++, ListPatternType.LowRoman, ListNumberAlignment.Left));
		}
		ListFormatOverride listFormatOverride = new ListFormatOverride();
		listFormatOverride.ListID = listData.ListID;
		m_listFormatOverrides.Add(listFormatOverride);
		return Convert.ToInt16(m_listFormatOverrides.Count);
	}

	internal ListData GetLevelFormat(int levelNumber)
	{
		ListFormatOverride listFormatOverride = m_listFormatOverrides[levelNumber - 1];
		return m_listFormats.FindListData(listFormatOverride.ListID);
	}

	internal ListInfo Clone()
	{
		return MemberwiseClone() as ListInfo;
	}

	internal short ApplyBulletList()
	{
		ListData listData = new ListData(m_listid);
		m_listid++;
		m_listFormats.Add(listData);
		for (float num = 0.5f; num < 4.5f; num += 1.5f)
		{
			listData.Levels.Add(CreateBuletLvl((int)(1440f * num), "\uf0b7"));
			listData.Levels.Add(CreateBuletLvl((int)(1440.0 * ((double)num + 0.5)), "o"));
			listData.Levels.Add(CreateBuletLvl((int)(1440f * (num + 1f)), "\uf0a7"));
		}
		ListFormatOverride listFormatOverride = new ListFormatOverride();
		listFormatOverride.ListID = listData.ListID;
		m_listFormatOverrides.Add(listFormatOverride);
		return Convert.ToInt16(m_listFormatOverrides.Count);
	}

	internal short ApplyList(ListData listData, WListFormat listFormat, WordStyleSheet styleSheet)
	{
		m_listFormats.Add(listData);
		return ApplyLFO(listData, listFormat, styleSheet);
	}

	internal short ApplyLFO(ListData listData, WListFormat listFormat, WordStyleSheet styleSheet)
	{
		ListFormatOverride listFormatOverride = new ListFormatOverride();
		if (listFormat.LFOStyleName != null)
		{
			string lFOStyleName = listFormat.LFOStyleName;
			ListOverrideStyle listOverrideStyle = listFormat.Document.ListOverrides.FindByName(lFOStyleName);
			if (listOverrideStyle != null)
			{
				ListPropertiesConverter.ImportListOverride(listOverrideStyle, listFormatOverride, styleSheet);
			}
		}
		listFormatOverride.ListID = listData.ListID;
		m_listFormatOverrides.Add(listFormatOverride);
		return Convert.ToInt16(m_listFormatOverrides.Count);
	}

	internal override void Close()
	{
		base.Close();
		if (m_listFormats != null)
		{
			foreach (ListData listFormat in m_listFormats)
			{
				listFormat?.Close();
			}
			m_listFormats.Clear();
			m_listFormats = null;
		}
		if (m_listFormatOverrides == null)
		{
			return;
		}
		foreach (ListFormatOverride listFormatOverride in m_listFormatOverrides)
		{
			listFormatOverride?.Close();
		}
		m_listFormatOverrides.Clear();
		m_listFormatOverrides = null;
	}

	private ListLevel CreateBuletLvl(int dxLeft, string str)
	{
		return new ListLevel();
	}

	private ListLevel CreateNumberLvl(int dxLeft, int levelNumber, ListPatternType nfc, ListNumberAlignment align)
	{
		return new ListLevel();
	}
}
