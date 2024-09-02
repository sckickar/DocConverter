using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class TabsInfo
{
	private short[] m_tabPositions = new short[0];

	private short[] m_tabDeletePositions = new short[0];

	private TabDescriptor[] m_tabDescriptors = new TabDescriptor[0];

	private byte m_tabsCount;

	private byte[] m_data;

	private byte m_opcode;

	private int m_deleteOffset;

	internal byte TabCount => m_tabsCount;

	internal short[] TabPositions => m_tabPositions;

	internal short[] TabDeletePositions
	{
		get
		{
			return m_tabDeletePositions;
		}
		set
		{
			m_tabDeletePositions = value;
		}
	}

	internal TabDescriptor[] Descriptors => m_tabDescriptors;

	internal TabsInfo(byte length)
	{
		m_tabsCount = length;
		m_tabPositions = new short[length];
		m_tabDescriptors = new TabDescriptor[length];
	}

	internal TabsInfo(SinglePropertyModifierArray sprms, int sprm)
	{
		Parse(sprms[sprm]);
	}

	internal TabsInfo(SinglePropertyModifierRecord record)
	{
		Parse(record);
	}

	internal void Save(SinglePropertyModifierArray sprms, int sprmOption)
	{
		bool flag = SaveInit();
		if (m_data != null)
		{
			SaveDeletePositions();
			if (flag)
			{
				SavePositions();
				SaveDescriptors();
			}
			sprms.SetByteArrayValue(sprmOption, m_data);
		}
	}

	private void Parse(SinglePropertyModifierRecord record)
	{
		m_data = record.ByteArray;
		if (m_data != null)
		{
			ParseInit();
			ParseDeletePositions();
			ParsePositions();
			ParseDescriptors();
		}
	}

	private void ParseInit()
	{
		m_opcode = m_data[0];
		m_deleteOffset = 0;
	}

	private void ParseDeletePositions()
	{
		if (m_opcode > 0)
		{
			m_deleteOffset = m_opcode * 2;
			m_tabDeletePositions = new short[m_opcode];
			if (m_data.Length > m_deleteOffset)
			{
				Buffer.BlockCopy(m_data, 1, m_tabDeletePositions, 0, m_opcode * 2);
			}
		}
	}

	private void ParsePositions()
	{
		if (m_data.Length > m_deleteOffset + 1)
		{
			m_tabsCount = m_data[m_deleteOffset + 1];
			m_tabPositions = new short[m_tabsCount];
			m_tabDescriptors = new TabDescriptor[m_tabsCount];
		}
		if (m_data.Length > m_deleteOffset + m_tabsCount * 2 + 1)
		{
			Buffer.BlockCopy(m_data, m_deleteOffset + 2, m_tabPositions, 0, m_tabsCount * 2);
		}
	}

	private void ParseDescriptors()
	{
		if (m_data.Length > m_deleteOffset + (m_tabsCount + 1) * 2 + (m_tabsCount - 1))
		{
			int num = (m_tabsCount + 1) * 2;
			for (int i = 0; i < m_tabsCount; i++)
			{
				m_tabDescriptors[i] = new TabDescriptor(m_data[num + m_deleteOffset]);
				num++;
			}
		}
	}

	private bool SaveInit()
	{
		m_opcode = (byte)((m_tabDeletePositions != null) ? ((uint)m_tabDeletePositions.Length) : 0u);
		m_deleteOffset = m_opcode * 2;
		int num = (m_opcode + 1) * 2;
		num += m_tabsCount * 3;
		if (num == 0)
		{
			return false;
		}
		m_data = new byte[num];
		return true;
	}

	private void SaveDeletePositions()
	{
		if (m_data.Length > m_deleteOffset && m_opcode > 0)
		{
			m_data[0] = m_opcode;
			Buffer.BlockCopy(m_tabDeletePositions, 0, m_data, 1, m_deleteOffset);
		}
	}

	private void SavePositions()
	{
		if (m_data.Length > m_deleteOffset + m_tabsCount * 2 + 1)
		{
			m_data[m_deleteOffset + 1] = m_tabsCount;
			Buffer.BlockCopy(m_tabPositions, 0, m_data, m_deleteOffset + 2, m_tabsCount * 2);
		}
	}

	private void SaveDescriptors()
	{
		if (m_data.Length > m_deleteOffset + m_tabsCount * 3 + 1)
		{
			int num = (m_tabsCount + 1) * 2;
			for (int i = 0; i < m_tabsCount; i++)
			{
				m_data[m_deleteOffset + num] = m_tabDescriptors[i].Save();
				num++;
			}
		}
	}
}
