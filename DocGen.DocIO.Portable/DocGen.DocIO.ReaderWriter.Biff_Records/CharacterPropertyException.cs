using System;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class CharacterPropertyException : BaseWordRecord
{
	private byte m_btLength;

	protected SinglePropertyModifierArray m_arrSprms;

	internal SinglePropertyModifierArray PropertyModifiers
	{
		get
		{
			if (m_arrSprms == null)
			{
				m_arrSprms = new SinglePropertyModifierArray();
			}
			return m_arrSprms;
		}
		set
		{
			m_arrSprms = value;
		}
	}

	internal int ModifiersCount => PropertyModifiers.Count;

	internal override int Length => 1 + PropertyModifiers.Length;

	internal ushort FontAscii
	{
		get
		{
			return PropertyModifiers.GetUShort(19023, ushort.MaxValue);
		}
		set
		{
			PropertyModifiers.SetUShortValue(19023, value);
		}
	}

	internal ushort FontFarEast
	{
		get
		{
			return PropertyModifiers.GetUShort(19024, 0);
		}
		set
		{
			PropertyModifiers.SetUShortValue(19024, value);
		}
	}

	internal ushort FontNonFarEast
	{
		get
		{
			return PropertyModifiers.GetUShort(19025, 0);
		}
		set
		{
			PropertyModifiers.SetUShortValue(19025, value);
		}
	}

	internal CharacterPropertyException()
	{
	}

	internal CharacterPropertyException(byte[] arrData, int iOffset)
	{
		Parse(arrData, iOffset, arrData.Length);
	}

	internal CharacterPropertyException(UniversalPropertyException property)
	{
		PropertyModifiers.Parse(property.Data, 0, property.Data.Length);
	}

	internal CharacterPropertyException(byte[] arrData)
	{
		PropertyModifiers.Parse(arrData, 0, arrData.Length);
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		m_btLength = arrData[iOffset];
		iOffset++;
		PropertyModifiers.Parse(arrData, iOffset, m_btLength);
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		byte b = (byte)m_arrSprms.Length;
		if (iOffset < 0 || iOffset + b + 1 > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		arrData[iOffset++] = b;
		if (m_arrSprms != null)
		{
			m_arrSprms.Save(arrData, iOffset);
		}
		return b + 1;
	}

	internal int Save(BinaryWriter writer, Stream stream, int length)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte b = (byte)(length - 1);
		writer.Write(b);
		if (m_arrSprms != null)
		{
			m_arrSprms.Save(writer, stream, b);
		}
		return b + 1;
	}

	internal bool HasSprms()
	{
		if (m_arrSprms == null || m_arrSprms.Count <= 0)
		{
			return false;
		}
		return true;
	}

	internal override void Close()
	{
		base.Close();
		if (m_arrSprms != null)
		{
			m_arrSprms = null;
		}
	}

	public bool Equals(CharacterPropertyException chpx)
	{
		bool flag = false;
		foreach (SinglePropertyModifierRecord propertyModifier in PropertyModifiers)
		{
			switch (propertyModifier.Options)
			{
			case 18514:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord7 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord7 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord7, SprmCompareType.UShortValue);
				break;
			}
			case 34880:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord39 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord39 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord39, SprmCompareType.ShortValue);
				break;
			}
			case 2138:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord13 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord13 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord13, SprmCompareType.ByteValue);
				break;
			}
			case 2101:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord27 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord27 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord27, SprmCompareType.ByteValue);
				break;
			}
			case 2140:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord45 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord45 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord45, SprmCompareType.ByteValue);
				break;
			}
			case 2107:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord25 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord25 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord25, SprmCompareType.ByteValue);
				break;
			}
			case 2178:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord48 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord48 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord48, SprmCompareType.ByteValue);
				break;
			}
			case 2054:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord34 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord34 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord34, SprmCompareType.ByteValue);
				break;
			}
			case 10835:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord18 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord18 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord18, SprmCompareType.ByteValue);
				break;
			}
			case 2136:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord4 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord4 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord4, SprmCompareType.ByteValue);
				break;
			}
			case 2050:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord40 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord40 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord40, SprmCompareType.ByteValue);
				break;
			}
			case 2132:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord31 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord31 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord31, SprmCompareType.ByteValue);
				break;
			}
			case 2102:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord21 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord21 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord21, SprmCompareType.ByteValue);
				break;
			}
			case 2141:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord12 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord12 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord12, SprmCompareType.ByteValue);
				break;
			}
			case 2165:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord49 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord49 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord49, SprmCompareType.ByteValue);
				break;
			}
			case 2134:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord43 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord43 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord43, SprmCompareType.ByteValue);
				break;
			}
			case 2058:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord36 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord36 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord36, SprmCompareType.ByteValue);
				break;
			}
			case 2104:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord30 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord30 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord30, SprmCompareType.ByteValue);
				break;
			}
			case 2049:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord22 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord22 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord22, SprmCompareType.ByteValue);
				break;
			}
			case 2048:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord16 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord16 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord16, SprmCompareType.ByteValue);
				break;
			}
			case 2105:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord9 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord9 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord9, SprmCompareType.ByteValue);
				break;
			}
			case 2106:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord3 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord3 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord3, SprmCompareType.ByteValue);
				break;
			}
			case 2133:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord46 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord46 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord46, SprmCompareType.ByteValue);
				break;
			}
			case 2103:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord42 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord42 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord42, SprmCompareType.ByteValue);
				break;
			}
			case 19038:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord37 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord37 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord37, SprmCompareType.UShortValue);
				break;
			}
			case 2108:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord33 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord33 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord33, SprmCompareType.ByteValue);
				break;
			}
			case 10764:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord28 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord28 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord28, SprmCompareType.ByteValue);
				break;
			}
			case 19011:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord24 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord24 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord24, SprmCompareType.UShortValue);
				break;
			}
			case 19041:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord19 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord19 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord19, SprmCompareType.UShortValue);
				break;
			}
			case 10820:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord15 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord15 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord15, SprmCompareType.ByteArray);
				break;
			}
			case 18501:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord10 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord10 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord10, SprmCompareType.UShortValue);
				break;
			}
			case 10818:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord6 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord6 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord6, SprmCompareType.ByteValue);
				break;
			}
			case 26736:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord50 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord50 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord50, SprmCompareType.UIntValue);
				break;
			}
			case 10351:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord47 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord47 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord47, SprmCompareType.ByteValue);
				break;
			}
			case 10824:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord44 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord44 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord44, SprmCompareType.ByteValue);
				break;
			}
			case 18992:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord41 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord41 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord41, SprmCompareType.UShortValue);
				break;
			}
			case 10814:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord38 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord38 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord38, SprmCompareType.ByteValue);
				break;
			}
			case 18527:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord35 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord35 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord35, SprmCompareType.ShortValue);
				break;
			}
			case 26759:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord32 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord32 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord32, SprmCompareType.UIntValue);
				break;
			}
			case 27139:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord29 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord29 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord29, SprmCompareType.IntValue);
				break;
			}
			case 19023:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord26 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord26 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord26, SprmCompareType.UShortValue);
				break;
			}
			case 19024:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord23 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord23 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord23, SprmCompareType.UShortValue);
				break;
			}
			case 19025:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord20 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord20 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord20, SprmCompareType.UShortValue);
				break;
			}
			case 18541:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord17 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord17 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord17, SprmCompareType.ShortValue);
				break;
			}
			case 18542:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord14 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord14 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord14, SprmCompareType.ShortValue);
				break;
			}
			case 18547:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord11 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord11 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord11, SprmCompareType.ShortValue);
				break;
			}
			case 18548:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord8 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord8 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord8, SprmCompareType.ShortValue);
				break;
			}
			case 18534:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord5 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord5 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord5, SprmCompareType.ShortValue);
				break;
			}
			default:
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord2 = chpx.PropertyModifiers[propertyModifier.Options];
				flag = singlePropertyModifierRecord2 != null && IsSprmEqual(propertyModifier, singlePropertyModifierRecord2, SprmCompareType.ByteArray);
				break;
			}
			}
			if (!flag)
			{
				return false;
			}
		}
		return flag;
	}

	private bool IsSprmEqual(SinglePropertyModifierRecord sprm, SinglePropertyModifierRecord prevSprm, SprmCompareType sprmCompareType)
	{
		switch (sprmCompareType)
		{
		case SprmCompareType.Boolean:
			if (sprm.BoolValue == prevSprm.BoolValue)
			{
				return true;
			}
			return false;
		case SprmCompareType.ByteValue:
			if (sprm.ByteValue == prevSprm.ByteValue)
			{
				return true;
			}
			return false;
		case SprmCompareType.IntValue:
			if (sprm.IntValue == prevSprm.IntValue)
			{
				return true;
			}
			return false;
		case SprmCompareType.ShortValue:
			if (sprm.ShortValue == prevSprm.ShortValue)
			{
				return true;
			}
			return false;
		case SprmCompareType.UIntValue:
			if (sprm.UIntValue == prevSprm.UIntValue)
			{
				return true;
			}
			return false;
		case SprmCompareType.UShortValue:
			if (sprm.UshortValue == prevSprm.UshortValue)
			{
				return true;
			}
			return false;
		case SprmCompareType.ByteArray:
		{
			for (int i = 0; i < sprm.ByteArray.Length; i++)
			{
				if (sprm.ByteArray[i] != prevSprm.ByteArray[i])
				{
					return false;
				}
			}
			return true;
		}
		default:
			return false;
		}
	}
}
