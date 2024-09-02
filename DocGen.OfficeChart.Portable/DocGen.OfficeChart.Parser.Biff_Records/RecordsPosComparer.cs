using System.Collections;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class RecordsPosComparer : IComparer, IComparer<BiffRecordPosAttribute>
{
	public int Compare(object x, object y)
	{
		BiffRecordPosAttribute biffRecordPosAttribute = (BiffRecordPosAttribute)x;
		BiffRecordPosAttribute biffRecordPosAttribute2 = (BiffRecordPosAttribute)y;
		int num = biffRecordPosAttribute.Position.CompareTo(biffRecordPosAttribute2.Position);
		if (num == 0 && (biffRecordPosAttribute.IsBit || biffRecordPosAttribute2.IsBit))
		{
			if (biffRecordPosAttribute.IsBit && !biffRecordPosAttribute2.IsBit)
			{
				return 1;
			}
			if (!biffRecordPosAttribute.IsBit && biffRecordPosAttribute2.IsBit)
			{
				return -1;
			}
			if (biffRecordPosAttribute.IsBit && biffRecordPosAttribute2.IsBit)
			{
				return biffRecordPosAttribute.SizeOrBitPosition.CompareTo(biffRecordPosAttribute2.SizeOrBitPosition);
			}
		}
		return num;
	}

	public int Compare(BiffRecordPosAttribute x, BiffRecordPosAttribute y)
	{
		int num = x.Position.CompareTo(y.Position);
		if (num == 0 && (x.IsBit || y.IsBit))
		{
			if (x.IsBit && !y.IsBit)
			{
				return 1;
			}
			if (!x.IsBit && y.IsBit)
			{
				return -1;
			}
			if (x.IsBit && y.IsBit)
			{
				return x.SizeOrBitPosition.CompareTo(y.SizeOrBitPosition);
			}
		}
		return num;
	}
}
