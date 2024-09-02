using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[CLSCompliant(false)]
internal class MsoFactory
{
	private static Dictionary<int, MsoBase> m_hashCodeToMSORecord;

	private MsoFactory()
	{
	}

	static MsoFactory()
	{
		m_hashCodeToMSORecord = new Dictionary<int, MsoBase>();
		RegisterAllTypes();
	}

	public static MsoBase CreateMsoRecord(MsoBase parent, MsoRecords recordType, byte[] data, ref int iOffset)
	{
		MsoBase msoBase = (m_hashCodeToMSORecord.ContainsKey((int)recordType) ? m_hashCodeToMSORecord[(int)recordType] : m_hashCodeToMSORecord[65535]);
		msoBase = (MsoBase)msoBase.Clone();
		msoBase.FillRecord(data, iOffset);
		iOffset += msoBase.Length + 8;
		return msoBase;
	}

	public static MsoBase CreateMsoRecord(MsoBase parent, MsoRecords recordType, byte[] data, ref int iOffset, GetNextMsoDrawingData dataGetter)
	{
		MsoBase msoBase = (m_hashCodeToMSORecord.ContainsKey((int)recordType) ? m_hashCodeToMSORecord[(int)recordType] : m_hashCodeToMSORecord[65535]);
		msoBase = (MsoBase)msoBase.Clone();
		msoBase.DataGetter = dataGetter;
		msoBase.FillRecord(data, iOffset);
		msoBase.UpdateNextMsoDrawingData();
		iOffset += msoBase.Length + 8;
		return msoBase;
	}

	public static MsoBase CreateMsoRecord(MsoBase parent, byte[] data, ref int iOffset)
	{
		MsoRecords recordType = (MsoRecords)BitConverter.ToUInt16(data, iOffset + 2);
		return CreateMsoRecord(parent, recordType, data, ref iOffset);
	}

	public static MsoBase CreateMsoRecord(MsoBase parent, byte[] data, ref int iOffset, GetNextMsoDrawingData dataGetter)
	{
		MsoRecords recordType = (MsoRecords)BitConverter.ToUInt16(data, iOffset + 2);
		return CreateMsoRecord(parent, recordType, data, ref iOffset, dataGetter);
	}

	public static MsoBase CreateMsoRecord(MsoBase parent, Stream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		stream.Position -= 4L;
		MsoRecords recordType = (MsoRecords)BitConverter.ToUInt16(array, 2);
		return CreateMsoRecord(parent, recordType, stream);
	}

	public static MsoBase CreateMsoRecord(MsoBase parent, MsoRecords recordType, Stream stream)
	{
		MsoBase msoBase = (m_hashCodeToMSORecord.ContainsKey((int)recordType) ? m_hashCodeToMSORecord[(int)recordType] : m_hashCodeToMSORecord[65535]);
		msoBase = (MsoBase)msoBase.Clone();
		msoBase.FillRecord(stream);
		return msoBase;
	}

	public static MsoBase CreateMsoRecord(MsoBase parent, Stream stream, GetNextMsoDrawingData dataGetter)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		stream.Position -= 4L;
		MsoRecords recordType = (MsoRecords)BitConverter.ToUInt16(array, 2);
		return CreateMsoRecord(parent, recordType, stream, dataGetter);
	}

	public static MsoBase CreateMsoRecord(MsoBase parent, MsoRecords recordType, Stream stream, GetNextMsoDrawingData dataGetter)
	{
		MsoBase msoBase = (m_hashCodeToMSORecord.ContainsKey((int)recordType) ? m_hashCodeToMSORecord[(int)recordType] : m_hashCodeToMSORecord[65535]);
		msoBase = (MsoBase)msoBase.Clone();
		msoBase.DataGetter = dataGetter;
		msoBase.FillRecord(stream);
		msoBase.UpdateNextMsoDrawingData();
		return msoBase;
	}

	public static MsoBase GetRecord(MsoRecords type)
	{
		MsoBase msoBase = m_hashCodeToMSORecord[(int)type];
		if (msoBase != null)
		{
			msoBase = (MsoBase)msoBase.Clone();
		}
		return msoBase;
	}

	private static void RegisterType(Type type, MsoDrawingAttribute[] attributes)
	{
		ConstructorInfo? constructor = type.GetConstructor(new Type[1] { typeof(MsoBase) });
		if (constructor == null)
		{
			throw new ApplicationException("Cannot find constructor");
		}
		object obj = constructor.Invoke(new object[1]);
		m_hashCodeToMSORecord.Add((int)attributes[0].RecordType, (MsoBase)obj);
	}

	private static void RegisterAllTypes()
	{
		MsoBase value = new MsofbtClientTextBox(null);
		m_hashCodeToMSORecord.Add(61453, value);
		value = new MsofbtSp(null);
		m_hashCodeToMSORecord.Add(61450, value);
		value = new MsofbtSpgrContainer(null);
		m_hashCodeToMSORecord.Add(61443, value);
		value = new MsofbtAnchor(null);
		m_hashCodeToMSORecord.Add(61454, value);
		value = new MsofbtClientAnchor(null);
		m_hashCodeToMSORecord.Add(61456, value);
		value = new MsofbtDgContainer(null);
		m_hashCodeToMSORecord.Add(61442, value);
		value = new MsofbtRegroupItems(null);
		m_hashCodeToMSORecord.Add(61720, value);
		value = new MsofbtDg(null);
		m_hashCodeToMSORecord.Add(61448, value);
		value = new MsofbtDggContainer(null);
		m_hashCodeToMSORecord.Add(61440, value);
		value = new MsofbtOPT(null);
		m_hashCodeToMSORecord.Add(61451, value);
		value = new MsofbtSpContainer(null);
		m_hashCodeToMSORecord.Add(61444, value);
		value = new MsofbtSplitMenuColors(null);
		m_hashCodeToMSORecord.Add(61726, value);
		value = new MsofbtDgg(null);
		m_hashCodeToMSORecord.Add(61446, value);
		value = new MsofbtBSE(null);
		m_hashCodeToMSORecord.Add(61447, value);
		value = new MsofbtSpgr(null);
		m_hashCodeToMSORecord.Add(61449, value);
		value = new MsofbtBstoreContainer(null);
		m_hashCodeToMSORecord.Add(61441, value);
		value = new MsofbtClientData(null);
		m_hashCodeToMSORecord.Add(61457, value);
		value = new MsoUnknown(null);
		m_hashCodeToMSORecord.Add(65535, value);
		value = new MsofbtChildAnchor(null);
		m_hashCodeToMSORecord.Add(61455, value);
	}
}
