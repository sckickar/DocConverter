namespace DocGen.Office;

internal class GSUBTable : OtfTable
{
	internal GSUBTable(BigEndianReader reader, int gsubTableLocation, GDEFTable gdef, TtfReader ttfReader)
		: base(reader, gsubTableLocation, gdef, ttfReader)
	{
		Initialize();
	}

	internal override LookupTable ReadLookupTable(int type, int flag, int[] offset)
	{
		if (type == 7)
		{
			for (int i = 0; i < offset.Length; i++)
			{
				int num = offset[i];
				base.Reader.Seek(num);
				base.Reader.ReadUInt16();
				type = base.Reader.ReadUInt16();
				num += base.Reader.ReadInt32();
				offset[i] = num;
			}
		}
		LookupTable result = null;
		switch (type)
		{
		case 1:
			result = new LookupSubTable1(this, flag, offset);
			break;
		case 2:
			result = new LookupSubTable2(this, flag, offset);
			break;
		case 3:
			result = new LookupSubTable3(this, flag, offset);
			break;
		case 4:
			result = new LookupSubTable4(this, flag, offset);
			break;
		case 5:
			result = new LookupSubTable5(this, flag, offset);
			break;
		case 6:
			result = new LookupSubTable6(this, flag, offset);
			break;
		}
		return result;
	}
}
