using System;
using System.Collections.Generic;
using System.Text;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
internal abstract class BiffRecordRawWithDataProvider : BiffRecordRaw, IDisposable
{
	protected DataProvider m_provider;

	public BiffRecordRawWithDataProvider()
	{
	}

	public abstract void ParseStructure();

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_provider = ApplicationImpl.CreateDataProvider();
		m_provider.EnsureCapacity(iLength);
		m_iLength = iLength;
		provider.CopyTo(iOffset, m_provider, 0, iLength);
		ParseStructure();
		if (!NeedDataArray)
		{
			m_provider.Clear();
			AutoGrowData = true;
		}
	}

	public abstract void InfillInternalData(OfficeVersion version);

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		InfillInternalData(version);
		if (m_iLength > 0)
		{
			m_provider.CopyTo(0, provider, iOffset, m_iLength);
		}
	}

	public override object Clone()
	{
		BiffRecordRawWithDataProvider obj = (BiffRecordRawWithDataProvider)base.Clone();
		nint zero = IntPtr.Zero;
		obj.m_provider = ((zero != IntPtr.Zero) ? ApplicationImpl.CreateDataProvider(zero) : ApplicationImpl.CreateDataProvider());
		return obj;
	}

	protected internal string GetString(int offset, int iStrLen)
	{
		int iBytesInString;
		return GetString(offset, iStrLen, out iBytesInString);
	}

	protected internal string GetString(int offset, int iStrLen, out int iBytesInString)
	{
		return GetString(offset, iStrLen, out iBytesInString, isByteCounted: false);
	}

	protected internal string GetString(int offset, int iStrLen, out int iBytesInString, bool isByteCounted)
	{
		return m_provider.ReadString(offset, iStrLen, out iBytesInString, isByteCounted);
	}

	protected string GetUnkTypeString(int offset, IList<int> continuePos, int continueCount, ref int iBreakIndex, out int length, out byte[] rich, out byte[] extended)
	{
		string text = null;
		int num = 3;
		rich = null;
		extended = null;
		ushort num2 = m_provider.ReadUInt16(offset);
		byte num3 = m_provider.ReadByte(offset + 2);
		bool flag = (num3 & 1) == 1;
		bool flag2 = (num3 & 4) != 0;
		bool flag3 = (num3 & 8) != 0;
		int num4 = 3;
		short num5 = 0;
		int num6 = 0;
		if (flag3)
		{
			num5 = m_provider.ReadInt16(offset + num4);
			num4 += 2;
			num += 2;
		}
		if (flag2)
		{
			num6 = m_provider.ReadInt32(offset + num4);
			num4 += 4;
			num += 4;
		}
		int num7 = offset + num4;
		int num8 = 0;
		Encoding encoding = (flag ? Encoding.Unicode : BiffRecordRaw.LatinEncoding);
		while (num8 < num2)
		{
			int num9 = (flag ? ((num2 - num8) * 2) : (num2 - num8));
			int num10 = BiffRecordRaw.FindNextBreak(continuePos, continueCount, num7, ref iBreakIndex) - num7;
			if (num9 <= num10)
			{
				string text2 = m_provider.ReadString(num7, num9, encoding, flag);
				text = ((text == null) ? text2 : (text + text2));
				num += num9;
				break;
			}
			string text3 = m_provider.ReadString(num7, num10, encoding, flag);
			text = ((text == null) ? text3 : (text + text3));
			num8 += (flag ? (num10 / 2) : num10);
			byte b = m_provider.ReadByte(num7 + num10);
			if (b == 0 || b == 1)
			{
				flag = b == 1;
				encoding = (flag ? Encoding.Unicode : BiffRecordRaw.LatinEncoding);
				num7++;
				num++;
			}
			num7 += num10;
			num += num10;
		}
		if (flag3)
		{
			int num11 = num5 * 4;
			rich = new byte[num11];
			m_provider.ReadArray(offset + num, rich, num11);
			num += num11;
		}
		if (flag2)
		{
			extended = new byte[num6];
			m_provider.ReadArray(offset + num, extended, num6);
			num += num6;
		}
		length = num;
		if (text == null)
		{
			return string.Empty;
		}
		return text;
	}

	protected internal void SetByte(int offset, byte value)
	{
		m_provider.WriteByte(offset, value);
	}

	protected internal void SetUInt16(int offset, ushort value)
	{
		m_provider.WriteUInt16(offset, value);
	}

	protected internal void SetBytes(int offset, byte[] value, int pos, int length)
	{
		m_provider.WriteBytes(offset, value, pos, length);
	}

	protected internal void SetBytes(int offset, byte[] value)
	{
		SetBytes(offset, value, 0, value.Length);
	}

	protected internal int SetStringNoLen(int offset, string value)
	{
		return SetStringNoLen(offset, value, bEmptyCompressed: false);
	}

	protected internal int SetStringNoLen(int offset, string value, bool bEmptyCompressed)
	{
		if (value == null || value.Length == 0)
		{
			if (bEmptyCompressed)
			{
				if (AutoGrowData)
				{
					m_provider.EnsureCapacity(offset);
				}
				m_provider.WriteByte(offset, 0);
				return 1;
			}
			return 0;
		}
		byte[] bytes = Encoding.Unicode.GetBytes(value);
		if (AutoGrowData)
		{
			m_provider.EnsureCapacity(offset + bytes.Length);
		}
		m_provider.WriteByte(offset, 1);
		m_provider.WriteBytes(offset + 1, bytes, 0, bytes.Length);
		return bytes.Length + 1;
	}

	public void Dispose()
	{
		if (m_provider != null)
		{
			m_provider.Dispose();
			m_provider = null;
		}
		GC.SuppressFinalize(this);
	}
}
