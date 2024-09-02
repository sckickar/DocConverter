namespace DocGen.Office;

internal class GDEFTable
{
	private CDEFTable m_glyphCdefTable;

	private CDEFTable m_markAttachmentCdefTable;

	internal CDEFTable GlyphCdefTable
	{
		get
		{
			return m_glyphCdefTable;
		}
		set
		{
			m_glyphCdefTable = value;
		}
	}

	internal CDEFTable MarkAttachmentCdefTable
	{
		get
		{
			return m_markAttachmentCdefTable;
		}
		set
		{
			m_markAttachmentCdefTable = value;
		}
	}

	internal GDEFTable(BigEndianReader reader, TtfTableInfo tableInfo)
	{
		if (tableInfo.Offset > 0)
		{
			reader.Seek(tableInfo.Offset);
			reader.ReadUInt16();
			reader.ReadUInt16();
			int num = reader.ReadUInt16();
			reader.ReadUInt16();
			reader.ReadUInt16();
			int num2 = reader.ReadUInt16();
			if (num > 0)
			{
				m_glyphCdefTable = GetTable(reader, num + tableInfo.Offset);
			}
			if (num2 > 0)
			{
				m_markAttachmentCdefTable = GetTable(reader, num2 + tableInfo.Offset);
			}
		}
	}

	private CDEFTable GetTable(BigEndianReader reader, int offset)
	{
		CDEFTable cDEFTable = new CDEFTable();
		reader.Seek(offset);
		switch (reader.ReadUInt16())
		{
		case 1:
		{
			ushort num4 = reader.ReadUInt16();
			int num5 = reader.ReadInt16();
			int num6 = num4 + num5;
			for (int k = num4; k < num6; k++)
			{
				int value2 = reader.ReadInt16();
				cDEFTable.Records.Add(k, value2);
			}
			break;
		}
		case 2:
		{
			int num = reader.ReadUInt16();
			for (int i = 0; i < num; i++)
			{
				ushort num2 = reader.ReadUInt16();
				int num3 = reader.ReadUInt16();
				int value = reader.ReadUInt16();
				for (int j = num2; j <= num3; j++)
				{
					if (cDEFTable.Records.ContainsKey(j))
					{
						cDEFTable.Records[j] = value;
					}
					else
					{
						cDEFTable.Records.Add(j, value);
					}
				}
			}
			break;
		}
		default:
			cDEFTable = null;
			break;
		}
		return cDEFTable;
	}

	internal bool IsSkip(int glyph, int flag)
	{
		if (m_glyphCdefTable != null && ((uint)flag & 0xEu) != 0)
		{
			int value = m_glyphCdefTable.GetValue(glyph);
			if (value == 1 && ((uint)flag & 2u) != 0)
			{
				return true;
			}
			if (value == 3 && ((uint)flag & 8u) != 0)
			{
				return true;
			}
			if (value == 2 && ((uint)flag & 4u) != 0)
			{
				return true;
			}
		}
		if (m_markAttachmentCdefTable != null && m_markAttachmentCdefTable.GetValue(glyph) > 0 && flag >> 8 > 0)
		{
			return m_markAttachmentCdefTable.GetValue(glyph) != flag >> 8;
		}
		return false;
	}
}
