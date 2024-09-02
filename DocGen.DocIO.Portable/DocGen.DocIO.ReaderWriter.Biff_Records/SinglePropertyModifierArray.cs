using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class SinglePropertyModifierArray : BaseWordRecord, ICollection, IEnumerable
{
	private class SPRMEnumerator : IEnumerator
	{
		private int m_iIndex = -1;

		private SinglePropertyModifierArray m_parent;

		public object Current
		{
			get
			{
				if (m_iIndex < 0 || m_iIndex >= m_parent.Count)
				{
					return null;
				}
				return m_parent.GetSprmByIndex(m_iIndex);
			}
		}

		internal SPRMEnumerator(SinglePropertyModifierArray parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}
			m_parent = parent;
		}

		public void Reset()
		{
			m_iIndex = -1;
		}

		public bool MoveNext()
		{
			m_iIndex++;
			if (m_iIndex < 0 || m_iIndex >= m_parent.Count)
			{
				return false;
			}
			return true;
		}
	}

	private List<SinglePropertyModifierRecord> m_arrModifiers = new List<SinglePropertyModifierRecord>();

	internal List<SinglePropertyModifierRecord> Modifiers => m_arrModifiers;

	internal SinglePropertyModifierRecord this[int option]
	{
		get
		{
			int i = 0;
			for (int count = m_arrModifiers.Count; i < count; i++)
			{
				if (m_arrModifiers[i].TypedOptions == option)
				{
					return m_arrModifiers[i];
				}
			}
			return null;
		}
	}

	public int Count => m_arrModifiers.Count;

	internal override int Length
	{
		get
		{
			int num = 0;
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				num += GetSprmByIndex(i).Length;
			}
			return num;
		}
	}

	public bool IsSynchronized => false;

	public object SyncRoot => new object();

	internal SinglePropertyModifierArray()
	{
	}

	internal SinglePropertyModifierArray(byte[] data)
		: base(data)
	{
	}

	internal SinglePropertyModifierArray(byte[] arrData, int iOffset)
		: base(arrData, iOffset)
	{
	}

	internal SinglePropertyModifierArray(byte[] arrData, int iOffset, int iCount)
		: base(arrData, iOffset, iCount)
	{
	}

	internal SinglePropertyModifierArray(Stream stream, int iCount)
		: base(stream, iCount)
	{
	}

	internal void RemoveValue(int options)
	{
		for (SinglePropertyModifierRecord singlePropertyModifierRecord = this[options]; singlePropertyModifierRecord != null; singlePropertyModifierRecord = this[options])
		{
			m_arrModifiers.Remove(singlePropertyModifierRecord);
		}
	}

	internal override void Parse(byte[] arrData, int iOffset, int iCount)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0)
		{
			throw new ArgumentOutOfRangeException("iOffset < 0");
		}
		if (iCount < 0)
		{
			throw new ArgumentOutOfRangeException("iCount < 0 ");
		}
		if (iOffset + iCount > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset + iCount");
		}
		Clear();
		int num = iOffset;
		while (iCount - (iOffset - num) > 1)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord();
			iOffset = singlePropertyModifierRecord.Parse(arrData, iOffset);
			if (IsValidParagraphPropertySprm(singlePropertyModifierRecord))
			{
				CheckDuplicateSprms(singlePropertyModifierRecord);
				m_arrModifiers.Add(singlePropertyModifierRecord);
			}
			else if (IsCorrectSprm(singlePropertyModifierRecord))
			{
				m_arrModifiers.Add(singlePropertyModifierRecord);
			}
		}
	}

	internal void CheckDuplicateSprms(SinglePropertyModifierRecord sprm)
	{
		int num = m_arrModifiers.Count - 1;
		while (num >= 0)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = m_arrModifiers[num];
			if (singlePropertyModifierRecord.OptionType != WordSprmOptionType.sprmPWall && singlePropertyModifierRecord.OptionType != WordSprmOptionType.sprmCWall)
			{
				if (sprm.OptionType == singlePropertyModifierRecord.OptionType)
				{
					m_arrModifiers.Remove(singlePropertyModifierRecord);
				}
				num--;
				continue;
			}
			break;
		}
	}

	private bool IsCorrectSprm(SinglePropertyModifierRecord sprm)
	{
		if (IsValidCharacterPropertySprm(sprm) || IsValidTablePropertySprm(sprm) || IsValidSectionPropertySprm(sprm) || IsValidPicturePropertySprm(sprm))
		{
			return true;
		}
		return false;
	}

	internal bool IsValidCharacterPropertySprm(SinglePropertyModifierRecord sprm)
	{
		switch (sprm.Options)
		{
		case 2048:
			if (sprm.ByteValue != 0 && sprm.ByteValue != 1 && sprm.ByteValue != 128 && sprm.ByteValue != 129)
			{
				sprm.ByteValue = 0;
			}
			return true;
		case 2049:
		case 2050:
		case 2054:
		case 2058:
		case 2065:
		case 2072:
		case 2101:
		case 2102:
		case 2103:
		case 2104:
		case 2105:
		case 2106:
		case 2107:
		case 2108:
		case 2132:
		case 2133:
		case 2134:
		case 2136:
		case 2138:
		case 2140:
		case 2141:
		case 2152:
		case 2165:
		case 2178:
		case 10329:
		case 10351:
		case 10361:
		case 10764:
		case 10803:
		case 10804:
		case 10814:
		case 10818:
		case 10824:
		case 10835:
		case 10883:
		case 10886:
		case 10896:
		case 18436:
		case 18439:
		case 18501:
		case 18507:
		case 18510:
		case 18514:
		case 18527:
		case 18531:
		case 18534:
		case 18535:
		case 18541:
		case 18542:
		case 18547:
		case 18548:
		case 18568:
		case 18992:
		case 19011:
		case 19023:
		case 19024:
		case 19025:
		case 19038:
		case 19040:
		case 19041:
		case 26629:
		case 26645:
		case 26646:
		case 26647:
		case 26724:
		case 26725:
		case 26736:
		case 26743:
		case 26759:
		case 27139:
		case 27145:
		case 34880:
		case 51226:
		case 51761:
		case 51783:
		case 51799:
		case 51810:
		case 51825:
		case 51826:
		case 51830:
		case 51832:
		case 51845:
		case 51849:
			return true;
		default:
			return false;
		}
	}

	private bool IsValidParagraphPropertySprm(SinglePropertyModifierRecord sprm)
	{
		switch (sprm.Options)
		{
		case 9219:
		case 9221:
		case 9222:
		case 9223:
		case 9228:
		case 9238:
		case 9239:
		case 9251:
		case 9258:
		case 9264:
		case 9265:
		case 9267:
		case 9268:
		case 9269:
		case 9270:
		case 9271:
		case 9272:
		case 9281:
		case 9283:
		case 9287:
		case 9288:
		case 9291:
		case 9292:
		case 9306:
		case 9307:
		case 9308:
		case 9313:
		case 9314:
		case 9325:
		case 9328:
		case 9329:
		case 9730:
		case 9738:
		case 9755:
		case 9792:
		case 9828:
		case 17451:
		case 17452:
		case 17453:
		case 17465:
		case 17466:
		case 17493:
		case 17494:
		case 17495:
		case 17496:
		case 17497:
		case 17920:
		case 17931:
		case 17936:
		case 18015:
		case 25618:
		case 25636:
		case 25637:
		case 25638:
		case 25639:
		case 25640:
		case 25701:
		case 25703:
		case 25707:
		case 26153:
		case 26182:
		case 26185:
		case 26186:
		case 33806:
		case 33807:
		case 33809:
		case 33816:
		case 33817:
		case 33818:
		case 33838:
		case 33839:
		case 33885:
		case 33886:
		case 33888:
		case 42003:
		case 42004:
		case 50689:
		case 50701:
		case 50709:
		case 50757:
		case 50765:
		case 50766:
		case 50767:
		case 50768:
		case 50769:
		case 50770:
		case 50771:
		case 50790:
		case 50793:
		case 50796:
		case 50799:
			return true;
		default:
			return false;
		}
	}

	private bool IsValidTablePropertySprm(SinglePropertyModifierRecord sprm)
	{
		switch (sprm.Options)
		{
		case 13315:
		case 13316:
		case 13413:
		case 13414:
		case 13436:
		case 13437:
		case 13448:
		case 13449:
		case 13837:
		case 13845:
		case 13849:
		case 13928:
		case 21504:
		case 21642:
		case 22027:
		case 22050:
		case 22052:
		case 22053:
		case 22074:
		case 22116:
		case 29706:
		case 29801:
		case 29817:
		case 30241:
		case 30243:
		case 30249:
		case 37895:
		case 37902:
		case 37903:
		case 37904:
		case 37905:
		case 37918:
		case 37919:
		case 38401:
		case 38402:
		case 54399:
		case 54789:
		case 54792:
		case 54793:
		case 54796:
		case 54802:
		case 54803:
		case 54806:
		case 54810:
		case 54811:
		case 54812:
		case 54813:
		case 54816:
		case 54827:
		case 54828:
		case 54829:
		case 54830:
		case 54831:
		case 54834:
		case 54835:
		case 54836:
		case 54837:
		case 54841:
		case 54846:
		case 54850:
		case 54880:
		case 54882:
		case 54887:
		case 54890:
		case 54896:
		case 54897:
		case 54898:
		case 54912:
		case 54913:
		case 54914:
		case 54915:
		case 54916:
		case 54917:
		case 54918:
		case 54919:
		case 62996:
		case 62999:
		case 63000:
		case 63030:
		case 63073:
			return true;
		default:
			return false;
		}
	}

	private bool IsValidSectionPropertySprm(SinglePropertyModifierRecord sprm)
	{
		switch (sprm.Options)
		{
		case 12288:
		case 12289:
		case 12293:
		case 12294:
		case 12297:
		case 12298:
		case 12302:
		case 12305:
		case 12306:
		case 12307:
		case 12313:
		case 12314:
		case 12317:
		case 12347:
		case 12348:
		case 12350:
		case 12840:
		case 12842:
		case 12857:
		case 20487:
		case 20488:
		case 20491:
		case 20501:
		case 20507:
		case 20508:
		case 20518:
		case 20530:
		case 20531:
		case 20543:
		case 20544:
		case 20545:
		case 20546:
		case 21039:
		case 28715:
		case 28716:
		case 28717:
		case 28718:
		case 28720:
		case 28730:
		case 28740:
		case 36876:
		case 36886:
		case 36899:
		case 36900:
		case 36913:
		case 45079:
		case 45080:
		case 45087:
		case 45088:
		case 45089:
		case 45090:
		case 45093:
		case 53812:
		case 53813:
		case 53814:
		case 53815:
		case 53827:
		case 61955:
		case 61956:
			return true;
		default:
			return false;
		}
	}

	private bool IsValidPicturePropertySprm(SinglePropertyModifierRecord sprm)
	{
		ushort options = sprm.Options;
		if ((uint)(options - 27650) <= 3u || (uint)(options - 52744) <= 3u)
		{
			return true;
		}
		return false;
	}

	internal override int Save(byte[] arrData, int iOffset)
	{
		int length = Length;
		if (length == 0)
		{
			return 0;
		}
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iOffset < 0 || iOffset + length > arrData.Length)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		int num = 0;
		int num2 = 0;
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			num2 = m_arrModifiers[i].Save(arrData, iOffset);
			num += num2;
			iOffset += num2;
		}
		return num;
	}

	internal int Save(BinaryWriter writer, Stream stream, int length)
	{
		if (length == 0)
		{
			return 0;
		}
		if (writer == null)
		{
			throw new ArgumentNullException("stream");
		}
		int num = 0;
		int num2 = 0;
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = m_arrModifiers[i];
			if (singlePropertyModifierRecord.Operand != null)
			{
				num2 = singlePropertyModifierRecord.Save(writer, stream);
				num += num2;
			}
		}
		return num;
	}

	internal int Save(BinaryWriter writer, Stream stream)
	{
		long position = stream.Position;
		if (Modifiers.Count == 0)
		{
			return 0;
		}
		if (writer == null)
		{
			throw new ArgumentNullException("stream");
		}
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			m_arrModifiers[i].Save(writer, stream);
		}
		return (int)(stream.Position - position);
	}

	internal void Clear()
	{
		m_arrModifiers.Clear();
	}

	internal void Add(SinglePropertyModifierRecord modifier)
	{
		m_arrModifiers.Add(modifier);
	}

	internal void SortSprms()
	{
		int trackChangeSprmIndex = 0;
		if (IsContainTrackChangesSprm(out trackChangeSprmIndex))
		{
			for (int i = 0; i < trackChangeSprmIndex; i++)
			{
				for (int j = i + 1; j < trackChangeSprmIndex; j++)
				{
					if (m_arrModifiers[i].UniqueID > m_arrModifiers[j].UniqueID)
					{
						SinglePropertyModifierRecord value = m_arrModifiers[i];
						m_arrModifiers[i] = m_arrModifiers[j];
						m_arrModifiers[j] = value;
					}
				}
			}
			for (int k = trackChangeSprmIndex + 1; k < m_arrModifiers.Count; k++)
			{
				for (int l = k + 1; l < m_arrModifiers.Count; l++)
				{
					if (m_arrModifiers[k].UniqueID > m_arrModifiers[l].UniqueID)
					{
						SinglePropertyModifierRecord value = m_arrModifiers[k];
						m_arrModifiers[k] = m_arrModifiers[l];
						m_arrModifiers[l] = value;
					}
				}
			}
			return;
		}
		for (int m = 0; m < m_arrModifiers.Count; m++)
		{
			for (int n = m + 1; n < m_arrModifiers.Count; n++)
			{
				if (m_arrModifiers[m].UniqueID > m_arrModifiers[n].UniqueID)
				{
					SinglePropertyModifierRecord value = m_arrModifiers[m];
					m_arrModifiers[m] = m_arrModifiers[n];
					m_arrModifiers[n] = value;
				}
			}
		}
	}

	private bool IsContainTrackChangesSprm(out int trackChangeSprmIndex)
	{
		trackChangeSprmIndex = 0;
		for (int i = 0; i < m_arrModifiers.Count; i++)
		{
			if (m_arrModifiers[i].Options == 10883 || m_arrModifiers[i].Options == 9828 || m_arrModifiers[i].Options == 13928 || m_arrModifiers[i].Options == 12857)
			{
				trackChangeSprmIndex = i;
				return true;
			}
		}
		return false;
	}

	internal void InsertAt(SinglePropertyModifierRecord modifier, int index)
	{
		m_arrModifiers.Insert(index, modifier);
	}

	internal void InsertRangeAt(SinglePropertyModifierArray modifiers, int index)
	{
		m_arrModifiers.InsertRange(index, modifiers.Modifiers);
	}

	internal bool GetBoolean(int options, bool defValue)
	{
		return TryGetSprm(options)?.BoolValue ?? defValue;
	}

	internal byte GetByte(int options, byte defValue)
	{
		return TryGetSprm(options)?.ByteValue ?? defValue;
	}

	internal bool HasSprm(int options)
	{
		if (this[options] != null)
		{
			return true;
		}
		return false;
	}

	internal ushort GetUShort(int options, ushort defValue)
	{
		return TryGetSprm(options)?.UshortValue ?? defValue;
	}

	internal short GetShort(int options, short defValue)
	{
		return TryGetSprm(options)?.ShortValue ?? defValue;
	}

	internal int GetInt(int icoe, int defVal)
	{
		return TryGetSprm(icoe)?.IntValue ?? defVal;
	}

	internal uint GetUInt(int icoe, uint defVal)
	{
		return TryGetSprm(icoe)?.UIntValue ?? defVal;
	}

	internal byte[] GetByteArray(int options)
	{
		return TryGetSprm(options)?.ByteArray;
	}

	internal void SetBoolValue(int options, bool flag)
	{
		GetSPRM(options).BoolValue = flag;
	}

	internal void SetByteValue(int options, byte value)
	{
		GetSPRM(options).ByteValue = value;
	}

	internal void SetUShortValue(int options, ushort value)
	{
		GetSPRM(options).UshortValue = value;
	}

	internal void SetShortValue(int options, short value)
	{
		GetSPRM(options).ShortValue = value;
	}

	internal void SetIntValue(int options, int value)
	{
		GetSPRM(options).IntValue = value;
	}

	internal void SetUIntValue(int options, uint value)
	{
		GetSPRM(options).UIntValue = value;
	}

	internal void SetByteArrayValue(int options, byte[] value)
	{
		GetSPRM(options).ByteArray = value;
	}

	internal SinglePropertyModifierArray Clone()
	{
		SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray();
		int i = 0;
		for (int count = m_arrModifiers.Count; i < count; i++)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = GetSprmByIndex(i).Clone();
			if (singlePropertyModifierRecord != null)
			{
				singlePropertyModifierArray.Add(singlePropertyModifierRecord);
			}
		}
		return singlePropertyModifierArray;
	}

	internal new void Close()
	{
		if (m_arrModifiers != null)
		{
			m_arrModifiers.Clear();
			m_arrModifiers = null;
		}
	}

	public void CopyTo(Array array, int index)
	{
	}

	public IEnumerator GetEnumerator()
	{
		return new SPRMEnumerator(this);
	}

	private SinglePropertyModifierRecord GetSPRM(int options)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = this[options];
		if (singlePropertyModifierRecord == null)
		{
			singlePropertyModifierRecord = new SinglePropertyModifierRecord(options);
			Add(singlePropertyModifierRecord);
		}
		return singlePropertyModifierRecord;
	}

	internal SinglePropertyModifierRecord GetSprmByIndex(int sprmIndex)
	{
		if (sprmIndex < 0 || sprmIndex >= m_arrModifiers.Count)
		{
			throw new ArgumentOutOfRangeException("index", "Value can not be less than 0 and greater than Length");
		}
		return m_arrModifiers[sprmIndex];
	}

	internal bool Contain(int option)
	{
		foreach (SinglePropertyModifierRecord arrModifier in m_arrModifiers)
		{
			if (arrModifier.Options == option)
			{
				return true;
			}
		}
		return false;
	}

	internal SinglePropertyModifierRecord TryGetSprm(int options)
	{
		SinglePropertyModifierRecord result = this[options];
		if (HasSprm(21039) || HasSprm(53827))
		{
			for (int num = m_arrModifiers.Count - 1; num >= 0; num--)
			{
				if (m_arrModifiers[num].Options == options)
				{
					return m_arrModifiers[num];
				}
			}
		}
		return result;
	}

	internal SinglePropertyModifierRecord GetNewSprm(int option, int wallSprmOption)
	{
		SinglePropertyModifierRecord result = this[option];
		int newPropsStartIndex = GetNewPropsStartIndex(wallSprmOption);
		if (newPropsStartIndex == -1)
		{
			return result;
		}
		int i = newPropsStartIndex;
		for (int count = m_arrModifiers.Count; i < count; i++)
		{
			result = GetSprmByIndex(i);
			if (result.OptionType == (WordSprmOptionType)option)
			{
				return result;
			}
		}
		return null;
	}

	private int GetNewPropsStartIndex(int wallSprmOption)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = this[wallSprmOption];
		if (singlePropertyModifierRecord != null)
		{
			return m_arrModifiers.IndexOf(singlePropertyModifierRecord) + 1;
		}
		return -1;
	}

	internal SinglePropertyModifierRecord GetOldSprm(int option, int wallSprmOption)
	{
		SinglePropertyModifierRecord result = this[option];
		int newPropsStartIndex = GetNewPropsStartIndex(wallSprmOption);
		if (newPropsStartIndex == -1)
		{
			return result;
		}
		for (int i = 0; i < newPropsStartIndex; i++)
		{
			result = GetSprmByIndex(i);
			if (result.OptionType == (WordSprmOptionType)option)
			{
				return result;
			}
		}
		return null;
	}
}
