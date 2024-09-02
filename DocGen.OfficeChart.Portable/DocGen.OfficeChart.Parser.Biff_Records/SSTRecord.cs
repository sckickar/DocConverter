using System;
using System.Text;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.SST)]
[CLSCompliant(false)]
internal class SSTRecord : BiffRecordWithContinue
{
	private const int DEF_OPTIONS_OFFET = 2;

	[BiffRecordPos(0, 4)]
	private uint m_uiNumberOfStrings;

	[BiffRecordPos(4, 4)]
	private uint m_uiNumberOfUniqueStrings;

	private object[] m_arrStrings;

	private int[] m_arrStringsPos;

	private int[] m_arrStringOffset;

	private bool m_bAutoAttach = true;

	public uint NumberOfStrings
	{
		get
		{
			return m_uiNumberOfStrings;
		}
		set
		{
			m_uiNumberOfStrings = value;
		}
	}

	public uint NumberOfUniqueStrings => m_uiNumberOfUniqueStrings;

	public object[] Strings
	{
		get
		{
			return m_arrStrings;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_arrStrings = value;
			m_uiNumberOfUniqueStrings = (uint)m_arrStrings.Length;
		}
	}

	public int[] StringsStreamPos => m_arrStringsPos;

	public int[] StringsOffsets => m_arrStringOffset;

	public bool AutoAttachContinue
	{
		get
		{
			return m_bAutoAttach;
		}
		set
		{
			m_bAutoAttach = value;
		}
	}

	public override bool NeedDataArray => true;

	public override void ParseStructure()
	{
		m_uiNumberOfStrings = m_provider.ReadUInt32(0);
		m_uiNumberOfUniqueStrings = m_provider.ReadUInt32(4);
		m_arrStrings = new object[m_uiNumberOfUniqueStrings];
		int num = 8;
		int count = m_arrContinuePos.Count;
		int iBreakIndex = 0;
		TextWithFormat textWithFormat = new TextWithFormat(0);
		for (int i = 0; i < m_uiNumberOfUniqueStrings; i++)
		{
			if (3 + num > m_iLength)
			{
				throw new WrongBiffRecordDataException("SSTRecord");
			}
			int length;
			byte[] rich;
			byte[] extended;
			string unkTypeString = GetUnkTypeString(num, m_arrContinuePos, count, ref iBreakIndex, out length, out rich, out extended);
			object obj = null;
			if (rich != null && rich.Length != 0)
			{
				TextWithFormat textWithFormat2 = textWithFormat.TypedClone();
				textWithFormat2.Text = unkTypeString;
				textWithFormat2.ParseFormattingRuns(rich);
				obj = textWithFormat2;
			}
			else
			{
				obj = unkTypeString;
			}
			m_arrStrings[i] = obj;
			num += length;
		}
		InternalDataIntegrityCheck(num);
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_arrContinuePos.Clear();
		PrognoseRecordSize();
		m_uiNumberOfStrings = m_uiNumberOfUniqueStrings;
		m_provider.WriteUInt32(0, m_uiNumberOfStrings);
		m_provider.WriteUInt32(4, m_uiNumberOfUniqueStrings);
		m_iLength = 8;
		m_arrStringsPos = new int[m_uiNumberOfUniqueStrings];
		m_arrStringOffset = new int[m_uiNumberOfUniqueStrings];
		byte[] arrBuffer = null;
		byte[] arrBuffer2 = null;
		new TextWithFormat(0);
		for (int i = 0; i < m_uiNumberOfUniqueStrings; i++)
		{
			object obj = m_arrStrings[i];
			TextWithFormat textWithFormat = obj as TextWithFormat;
			int num = 0;
			TextWithFormat.StringType stringType = TextWithFormat.StringType.Unicode;
			Encoding encoding = Encoding.Unicode;
			string text;
			if (textWithFormat == null)
			{
				text = (string)obj;
			}
			else
			{
				text = textWithFormat.Text;
				num = textWithFormat.GetFormattingSize();
				stringType = textWithFormat.GetOptions();
			}
			if (BiffRecordRawWithArray.IsAsciiString(text))
			{
				stringType &= ~TextWithFormat.StringType.Unicode;
				stringType |= TextWithFormat.StringType.NonUnicode;
				encoding = Encoding.UTF8;
			}
			ushort charCount = (ushort)text.Length;
			int iSize = encoding.GetByteCount(text) + 3;
			EnsureSize(ref arrBuffer, iSize);
			EnsureSize(ref arrBuffer2, num);
			encoding.GetBytes(text, 0, charCount, arrBuffer, 0);
			if (num > 0)
			{
				textWithFormat.SerializeFormatting(arrBuffer2, 0, bDefragment: false);
			}
		}
	}

	private void PrognoseRecordSize()
	{
		int num = 0;
		for (int i = 0; i < m_uiNumberOfUniqueStrings; i++)
		{
			object obj = m_arrStrings[i];
			string text;
			if (!(obj is TextWithFormat textWithFormat))
			{
				text = (string)obj;
			}
			else
			{
				text = textWithFormat.Text;
				if (textWithFormat.FormattingRunsCount > 0)
				{
					num += textWithFormat.FormattingRunsCount * 4 + 2;
				}
			}
			num += text.Length * 2 + 3;
		}
		num += num / 1000;
		m_provider.EnsureCapacity(8 + num);
	}

	public static void EnsureSize(ref byte[] arrBuffer, int iSize)
	{
		if (((arrBuffer != null) ? arrBuffer.Length : 0) < iSize)
		{
			arrBuffer = new byte[iSize];
		}
	}

	private void InternalDataIntegrityCheck(int iCurPos)
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (base.NeedInfill)
		{
			InfillInternalData(version);
			base.NeedInfill = false;
		}
		return m_iLength;
	}
}
