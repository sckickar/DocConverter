using DocGen.Pdf.IO;

namespace DocGen.Pdf.Graphics.Fonts;

internal class GPOSTable : OtfTable
{
	internal GPOSTable(BigEndianReader reader, int offset, GDEFTable gdef, TtfReader ttfReader)
		: base(reader, offset, gdef, ttfReader)
	{
		Initialize();
	}

	internal override LookupTable ReadLookupTable(int type, int flag, int[] offsets)
	{
		if (type == 9)
		{
			for (int i = 0; i < offsets.Length; i++)
			{
				int num = offsets[i];
				base.Reader.Seek(num);
				base.Reader.ReadUInt16();
				type = base.Reader.ReadInt16();
				num += base.Reader.ReadInt32();
				offsets[i] = num;
			}
		}
		LookupTable result = null;
		switch (type)
		{
		case 1:
			result = new LookupTable1(this, flag, offsets);
			break;
		case 2:
			result = new LookupTable2(this, flag, offsets);
			break;
		case 4:
			result = new LookupTable4(this, flag, offsets);
			break;
		case 5:
			result = new LookupTable5(this, flag, offsets);
			break;
		case 6:
			result = new LookupTable6(this, flag, offsets);
			break;
		}
		return result;
	}
}
