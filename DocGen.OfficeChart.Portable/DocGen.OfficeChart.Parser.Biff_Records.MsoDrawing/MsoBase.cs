using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[CLSCompliant(false)]
internal abstract class MsoBase : BiffRecordRawWithArray
{
	private const ushort DEF_VERSION_MASK = 15;

	private const ushort DEF_INST_MASK = 65520;

	private const ushort DEF_INST_START_BIT = 4;

	private const int DEF_MAXIMUM_RECORD_SIZE = int.MaxValue;

	protected ushort m_usVersionAndInst;

	private ushort m_usRecordType;

	private GetNextMsoDrawingData m_dataGetter;

	private MsoBase m_parent;

	private static Dictionary<Type, int> s_dicTypeToCode;

	public int Version
	{
		get
		{
			return BiffRecordRaw.GetUInt16BitsByMask(m_usVersionAndInst, 15);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usVersionAndInst, 15, (ushort)value);
		}
	}

	public int Instance
	{
		get
		{
			return BiffRecordRaw.GetUInt16BitsByMask(m_usVersionAndInst, 65520) >> 4;
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usVersionAndInst, 65520, (ushort)(value << 4));
		}
	}

	public MsoRecords MsoRecordType
	{
		get
		{
			return (MsoRecords)m_usRecordType;
		}
		set
		{
			m_usRecordType = (ushort)value;
		}
	}

	public GetNextMsoDrawingData DataGetter
	{
		get
		{
			return m_dataGetter;
		}
		set
		{
			m_dataGetter = value;
		}
	}

	public MsoBase Parent => m_parent;

	public override int MaximumRecordSize => int.MaxValue;

	static MsoBase()
	{
		s_dicTypeToCode = new Dictionary<Type, int>();
		s_dicTypeToCode.Add(typeof(MsofbtClientTextBox), 61453);
		s_dicTypeToCode.Add(typeof(MsofbtSp), 61450);
		s_dicTypeToCode.Add(typeof(MsofbtSpgrContainer), 61443);
		s_dicTypeToCode.Add(typeof(MsofbtAnchor), 61454);
		s_dicTypeToCode.Add(typeof(MsofbtClientAnchor), 61456);
		s_dicTypeToCode.Add(typeof(MsofbtDgContainer), 61442);
		s_dicTypeToCode.Add(typeof(MsofbtRegroupItems), 61720);
		s_dicTypeToCode.Add(typeof(MsofbtDg), 61448);
		s_dicTypeToCode.Add(typeof(MsofbtDggContainer), 61440);
		s_dicTypeToCode.Add(typeof(MsofbtOPT), 61451);
		s_dicTypeToCode.Add(typeof(MsofbtSpContainer), 61444);
		s_dicTypeToCode.Add(typeof(MsofbtSplitMenuColors), 61726);
		s_dicTypeToCode.Add(typeof(MsofbtDgg), 61446);
		s_dicTypeToCode.Add(typeof(MsofbtBSE), 61447);
		s_dicTypeToCode.Add(typeof(MsofbtSpgr), 61449);
		s_dicTypeToCode.Add(typeof(MsofbtBstoreContainer), 61441);
		s_dicTypeToCode.Add(typeof(MsofbtClientData), 61457);
		s_dicTypeToCode.Add(typeof(MsoUnknown), 65535);
		s_dicTypeToCode.Add(typeof(MsofbtChildAnchor), 61455);
		s_dicTypeToCode.Add(typeof(MsoMetafilePicture), 0);
		s_dicTypeToCode.Add(typeof(MsoBitmapPicture), 0);
	}

	public MsoBase()
	{
		Type type = GetType();
		s_dicTypeToCode.TryGetValue(type, out m_iCode);
		m_usRecordType = (ushort)m_iCode;
	}

	public MsoBase(MsoBase parent)
		: this()
	{
		m_parent = parent;
	}

	public MsoBase(MsoBase parent, byte[] data, int offset)
		: this(parent, data, offset, null)
	{
	}

	public MsoBase(MsoBase parent, byte[] data, int offset, GetNextMsoDrawingData dataGetter)
		: this(parent)
	{
		m_dataGetter = dataGetter;
		FillRecord(data, offset);
	}

	public MsoBase(MsoBase parent, Stream stream, GetNextMsoDrawingData dataGetter)
		: this(parent)
	{
		m_dataGetter = dataGetter;
		FillRecord(stream);
	}

	public virtual int FillRecord(byte[] data, int iOffset)
	{
		if (data == null)
		{
			throw new ArgumentNullException("reader");
		}
		int num = iOffset;
		try
		{
			if (data.Length - iOffset - 8 < 0)
			{
				throw new ApplicationException("Unexpected end of record - reached end of the array.");
			}
			m_usVersionAndInst = BitConverter.ToUInt16(data, iOffset);
			iOffset += 2;
			m_usRecordType = BitConverter.ToUInt16(data, iOffset);
			iOffset += 2;
			if (m_usRecordType == 0)
			{
				throw new ApplicationException("Mso Record identification code is wrong (zero).");
			}
			m_iLength = BitConverter.ToInt32(data, iOffset);
			iOffset += 4;
			if (m_iLength < MinimumRecordSize)
			{
				throw new SmallBiffRecordDataException("Code :" + m_iCode + "\n Real size: " + m_iLength + ". Expected size: " + MaximumRecordSize);
			}
			if (m_iLength > MaximumRecordSize)
			{
				string[] obj = new string[7] { "Code :", null, null, null, null, null, null };
				MsoRecords iCode = (MsoRecords)m_iCode;
				obj[1] = iCode.ToString();
				obj[2] = m_iCode.ToString();
				obj[3] = "\n Real size: ";
				obj[4] = m_iLength.ToString();
				obj[5] = ". Expected size: ";
				obj[6] = MaximumRecordSize.ToString();
				throw new LargeBiffRecordDataException(string.Concat(obj));
			}
			if (data.Length - iOffset - m_iLength < 0)
			{
				throw new ApplicationException("Unexpected end of records stream. Record data cannot be read - reached end of stream.");
			}
			m_data = new byte[m_iLength];
			Array.Copy(data, iOffset, m_data, 0, m_iLength);
			ParseStructure();
			return m_iLength + 8;
		}
		catch (ApplicationException ex)
		{
			_ = ex.InnerException;
			iOffset = num;
			throw;
		}
	}

	public virtual void FillArray(Stream stream)
	{
		FillArray(stream, 0, null, null);
	}

	public virtual void FillArray(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		byte[] array = new byte[8];
		long position = stream.Position;
		stream.Position += 8L;
		InfillInternalData(stream, iOffset, arrBreaks, arrRecords);
		long position2 = stream.Position;
		stream.Position = position;
		int num = 0;
		BitConverter.GetBytes(m_usVersionAndInst).CopyTo(array, num);
		num += 2;
		BitConverter.GetBytes(m_usRecordType).CopyTo(array, num);
		num += 2;
		BitConverter.GetBytes(m_iLength).CopyTo(array, num);
		num += 4;
		stream.Write(array, 0, num);
		stream.Position = position2;
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		MemoryStream stream = new MemoryStream();
		InfillInternalData(stream, 0, null, null);
	}

	public abstract void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords);

	public MsoBase Clone(MsoBase parent)
	{
		MsoBase obj = (MsoBase)InternalClone();
		obj.m_parent = parent;
		return obj;
	}

	protected virtual object InternalClone()
	{
		MsoBase msoBase = (MsoBase)base.Clone();
		if (m_data != null)
		{
			msoBase.m_data = CloneUtils.CloneByteArray(m_data);
		}
		return msoBase;
	}

	public override object Clone()
	{
		return InternalClone();
	}

	public virtual void UpdateNextMsoDrawingData()
	{
	}

	public abstract void ParseStructure(Stream stream);

	public override void ParseStructure()
	{
		throw new NotSupportedException("The method or operation is not supported for MsoRecords.");
	}

	public static double ConvertFromInt32(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		double num = BitConverter.ToInt16(bytes, 0);
		ushort num2 = BitConverter.ToUInt16(bytes, 2);
		double num3 = 0.5;
		ushort num4 = 32768;
		for (int i = 0; i < 16; i++)
		{
			if ((num4 & num2) != 0)
			{
				num += num3;
			}
			num3 /= 2.0;
			num4 >>= 1;
		}
		return num;
	}

	public static void WriteInt32(Stream stream, int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static void WriteUInt32(Stream stream, uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static void WriteInt16(Stream stream, short value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static void WriteUInt16(Stream stream, ushort value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static int ReadInt32(Stream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return BitConverter.ToInt32(array, 0);
	}

	public static uint ReadUInt32(Stream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return BitConverter.ToUInt32(array, 0);
	}

	public static short ReadInt16(Stream stream)
	{
		byte[] array = new byte[2];
		stream.Read(array, 0, 2);
		return BitConverter.ToInt16(array, 0);
	}

	public static ushort ReadUInt16(Stream stream)
	{
		byte[] array = new byte[2];
		stream.Read(array, 0, 2);
		return BitConverter.ToUInt16(array, 0);
	}

	internal void FillRecord(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		try
		{
			if (stream.Length - stream.Position - 8 < 0)
			{
				throw new ApplicationException("Unexpected end of record - reached end of the array.");
			}
			m_usVersionAndInst = ReadUInt16(stream);
			m_usRecordType = ReadUInt16(stream);
			if (m_usRecordType == 0)
			{
				throw new ApplicationException("Mso Record identification code is wrong (zero).");
			}
			m_iLength = ReadInt32(stream);
			if (m_iLength < MinimumRecordSize)
			{
				throw new SmallBiffRecordDataException("Code :" + m_iCode + "\n Real size: " + m_iLength + ". Expected size: " + MaximumRecordSize);
			}
			if (m_iLength > MaximumRecordSize)
			{
				string[] obj = new string[7] { "Code :", null, null, null, null, null, null };
				MsoRecords iCode = (MsoRecords)m_iCode;
				obj[1] = iCode.ToString();
				obj[2] = m_iCode.ToString();
				obj[3] = "\n Real size: ";
				obj[4] = m_iLength.ToString();
				obj[5] = ". Expected size: ";
				obj[6] = MaximumRecordSize.ToString();
				throw new LargeBiffRecordDataException(string.Concat(obj));
			}
			if (stream.Length - stream.Position - m_iLength < 0)
			{
				throw new ApplicationException("Unexpected end of records stream. Record data cannot be read - reached end of stream.");
			}
			ParseStructure(stream);
		}
		catch (ApplicationException ex)
		{
			_ = ex.InnerException;
			throw;
		}
	}
}
