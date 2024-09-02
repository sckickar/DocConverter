using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListData : BaseWordRecord
{
	private const int DEF_LEVELS_COUNT = 9;

	private const int DEF_RGISTD = 4095;

	private const int DEF_SIMPLE_BIT = 1;

	private const int DEF_HYBRID_BIT = 16;

	private int m_lsid;

	private int m_tplc;

	private int[] m_rgistd;

	private int m_Options;

	private ListLevels m_levels;

	private string m_name;

	internal ListLevels Levels => m_levels;

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal bool RestartHeading => (m_Options & 2) != 0;

	internal bool SimpleList
	{
		get
		{
			return (m_Options & 1) != 0;
		}
		set
		{
			m_Options &= 254;
			m_Options |= (value ? 1 : 0);
		}
	}

	internal bool IsHybridMultilevel => (m_Options & 0x10) != 0;

	internal int ListID
	{
		get
		{
			return m_lsid;
		}
		set
		{
			m_lsid = value;
		}
	}

	internal ListData(int lsid)
		: this(lsid, isHybrid: true, isSimpleList: false)
	{
	}

	internal ListData(int lsid, bool isHybrid, bool isSimpleList)
	{
		m_rgistd = new int[9];
		m_levels = new ListLevels();
		m_lsid = lsid;
		m_tplc = ~lsid;
		int num = (isSimpleList ? 1 : 9);
		for (int i = 0; i < num; i++)
		{
			m_rgistd[i] = 4095;
		}
		if (isSimpleList)
		{
			m_Options |= 1;
		}
		if (isHybrid)
		{
			m_Options |= 16;
		}
		m_name = "";
	}

	internal ListData(Stream reader)
	{
		m_rgistd = new int[9];
		m_levels = new ListLevels();
		ReadListData(reader);
	}

	internal override void Close()
	{
		base.Close();
		m_rgistd = null;
		if (m_levels != null)
		{
			m_levels.Clear();
			m_levels = null;
		}
	}

	internal void ReadLvl(Stream stream)
	{
		int num = (SimpleList ? 1 : 9);
		for (int i = 0; i < num; i++)
		{
			m_levels.Add(new ListLevel(stream));
		}
	}

	internal void WriteListData(Stream stream)
	{
		if (m_levels.Count == 1)
		{
			SimpleList = true;
		}
		BaseWordRecord.WriteInt32(stream, m_lsid);
		BaseWordRecord.WriteInt32(stream, m_tplc);
		for (int i = 0; i < m_rgistd.Length; i++)
		{
			BaseWordRecord.WriteInt16(stream, (short)m_rgistd[i]);
		}
		BaseWordRecord.WriteUInt16(stream, (ushort)m_Options);
	}

	internal void ReadListData(Stream stream)
	{
		m_lsid = BaseWordRecord.ReadInt32(stream);
		m_tplc = BaseWordRecord.ReadInt32(stream);
		for (int i = 0; i < m_rgistd.Length; i++)
		{
			m_rgistd[i] = BaseWordRecord.ReadInt16(stream);
		}
		m_Options = BaseWordRecord.ReadUInt16(stream);
	}

	internal void WriteLvl(Stream stream)
	{
		foreach (ListLevel level in m_levels)
		{
			level.Write(stream);
		}
	}
}
