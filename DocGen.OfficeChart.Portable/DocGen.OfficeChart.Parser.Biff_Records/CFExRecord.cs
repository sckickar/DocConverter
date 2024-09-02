using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CFEx)]
[CLSCompliant(false)]
internal class CFExRecord : CondFMTRecord
{
	private const ushort DEF_MINIMUM_RECORD_SIZE = 18;

	private const ushort DEF_ISCF12_RECORD_SIZE = 25;

	private CF12Record m_cf12Record;

	private FutureHeader m_header;

	private bool m_isRefRange = true;

	private bool m_isFutureAlert;

	private byte m_headerAttribute = 1;

	private TAddr m_addrEncloseRange;

	private byte m_isCF12;

	private ushort m_CondFMTIndex;

	private ushort m_CFIndex;

	private byte m_compareOperator;

	private ushort m_template;

	private ushort m_priority;

	private byte m_undefined = 1;

	private bool m_cfExIsparsed;

	private byte m_hasDXF;

	private ushort m_sizeOfDXF;

	private ushort m_propertyCount;

	private List<ExtendedProperty> m_properties;

	private ushort m_templateParamCount = 16;

	private ushort m_reserved = ushort.MaxValue;

	private ushort m_defaultParameter;

	private DXFN m_dxfn;

	private CFExFilterParameter m_cfExFilterParam;

	internal CFExTextTemplateParameter m_cfExTextParam;

	internal CFExDateTemplateParameter m_cfExDateParam;

	private CFExAverageTemplateParameter m_cfExAverageParam;

	public new TAddr EncloseRange
	{
		get
		{
			return m_addrEncloseRange;
		}
		set
		{
			m_addrEncloseRange = value;
		}
	}

	public byte IsCF12Extends
	{
		get
		{
			return m_isCF12;
		}
		set
		{
			m_isCF12 = value;
		}
	}

	public ushort CondFmtIndex
	{
		get
		{
			return m_CondFMTIndex;
		}
		set
		{
			m_CondFMTIndex = value;
		}
	}

	public ushort CFIndex
	{
		get
		{
			return m_CFIndex;
		}
		set
		{
			m_CFIndex = value;
		}
	}

	public ExcelComparisonOperator ComparisonOperator
	{
		get
		{
			return (ExcelComparisonOperator)m_compareOperator;
		}
		set
		{
			m_compareOperator = (byte)value;
		}
	}

	public ConditionalFormatTemplate Template
	{
		get
		{
			return (ConditionalFormatTemplate)m_template;
		}
		set
		{
			m_template = (byte)value;
		}
	}

	public ushort Priority
	{
		get
		{
			return m_priority;
		}
		set
		{
			m_priority = value;
		}
	}

	public bool StopIfTrue
	{
		get
		{
			return (m_undefined & 2) == 2;
		}
		set
		{
			m_undefined = (byte)(value ? (m_undefined | 2u) : (m_undefined | 0u));
		}
	}

	public byte HasDXF
	{
		get
		{
			return m_hasDXF;
		}
		set
		{
			m_hasDXF = value;
		}
	}

	public ushort SizeOfDXF
	{
		get
		{
			return m_sizeOfDXF;
		}
		set
		{
			m_sizeOfDXF = value;
		}
	}

	public ushort PropertyCount
	{
		get
		{
			return m_propertyCount;
		}
		set
		{
			m_propertyCount = value;
		}
	}

	public List<ExtendedProperty> Properties
	{
		get
		{
			return m_properties;
		}
		set
		{
			m_properties = value;
		}
	}

	public override int MinimumRecordSize => 18;

	public bool IsCFExParsed
	{
		get
		{
			return m_cfExIsparsed;
		}
		set
		{
			m_cfExIsparsed = value;
		}
	}

	public CF12Record CF12RecordIfExtends
	{
		get
		{
			return m_cf12Record;
		}
		set
		{
			m_cf12Record = value;
		}
	}

	public CFExRecord()
	{
		m_header = new FutureHeader();
		m_header.Type = 2171;
		m_dxfn = new DXFN();
		m_properties = new List<ExtendedProperty>();
		m_cfExFilterParam = new CFExFilterParameter();
		m_cfExTextParam = new CFExTextTemplateParameter();
		m_cfExDateParam = new CFExDateTemplateParameter();
		m_cfExAverageParam = new CFExAverageTemplateParameter();
	}

	public CFExRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CFExRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_cfExIsparsed = true;
		m_header.Type = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_headerAttribute = provider.ReadByte(iOffset);
		iOffset += 2;
		m_addrEncloseRange = provider.ReadAddr(iOffset);
		iOffset += 8;
		m_isCF12 = (byte)provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_CondFMTIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		if (m_isCF12 != 0)
		{
			return;
		}
		m_CFIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_compareOperator = provider.ReadByte(iOffset);
		iOffset++;
		m_template = provider.ReadByte(iOffset);
		iOffset++;
		m_priority = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_undefined = provider.ReadByte(iOffset);
		iOffset++;
		m_hasDXF = provider.ReadByte(iOffset);
		iOffset++;
		if (m_hasDXF != 0)
		{
			m_sizeOfDXF = (ushort)provider.ReadUInt32(iOffset);
			iOffset += 4;
			if (m_sizeOfDXF == 0)
			{
				provider.ReadUInt16(iOffset);
				iOffset += 2;
			}
			int num = iOffset;
			if (m_sizeOfDXF > 0)
			{
				m_dxfn = new DXFN();
				iOffset = m_dxfn.ParseDXFN(provider, iOffset, version);
			}
			num = iOffset - num;
			if (m_sizeOfDXF != num)
			{
				provider.ReadUInt16(iOffset);
				iOffset += 2;
				m_reserved = provider.ReadUInt16(iOffset);
				iOffset += 2;
				provider.ReadUInt16(iOffset);
				iOffset += 2;
				m_propertyCount = provider.ReadUInt16(iOffset);
				iOffset += 2;
				m_properties = new List<ExtendedProperty>();
				for (int i = 0; i < m_propertyCount; i++)
				{
					ExtendedProperty extendedProperty = new ExtendedProperty();
					iOffset = extendedProperty.ParseExtendedProperty(provider, iOffset, version);
					m_properties.Add(extendedProperty);
				}
			}
		}
		m_templateParamCount = provider.ReadByte(iOffset);
		iOffset++;
		iOffset = ParseCFExTemplateParameter(provider, iOffset, version);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		ushort value = ushort.MaxValue;
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_header.Type);
		iOffset += 2;
		provider.WriteByte(iOffset, m_headerAttribute);
		iOffset += 2;
		provider.WriteAddr(iOffset, m_addrEncloseRange);
		iOffset += 8;
		provider.WriteUInt32(iOffset, m_isCF12);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_CondFMTIndex);
		iOffset += 2;
		if (m_isCF12 != 0)
		{
			return;
		}
		provider.WriteUInt16(iOffset, m_CFIndex);
		iOffset += 2;
		provider.WriteByte(iOffset, m_compareOperator);
		iOffset++;
		provider.WriteByte(iOffset, (byte)m_template);
		iOffset++;
		provider.WriteUInt16(iOffset, m_priority);
		iOffset += 2;
		provider.WriteByte(iOffset, m_undefined);
		iOffset++;
		provider.WriteByte(iOffset, m_hasDXF);
		iOffset++;
		if (m_hasDXF != 0)
		{
			provider.WriteUInt32(iOffset, m_sizeOfDXF);
			iOffset += 4;
			if (m_sizeOfDXF == 0)
			{
				provider.WriteUInt16(iOffset, 0);
				iOffset += 2;
			}
			int num = iOffset;
			if (m_sizeOfDXF > 0)
			{
				iOffset = m_dxfn.SerializeDXFN(provider, iOffset, version);
			}
			num = iOffset - num;
			if (m_sizeOfDXF != num)
			{
				provider.WriteUInt16(iOffset, 0);
				iOffset += 2;
				provider.WriteUInt16(iOffset, value);
				iOffset += 2;
				provider.WriteUInt16(iOffset, 0);
				iOffset += 2;
				provider.WriteUInt16(iOffset, (ushort)m_properties.Count);
				iOffset += 2;
				foreach (ExtendedProperty property in m_properties)
				{
					iOffset = property.InfillInternalData(provider, iOffset, version);
				}
			}
		}
		provider.WriteByte(iOffset, (byte)m_templateParamCount);
		iOffset++;
		iOffset = SerializeCFExTemplateParameter(provider, iOffset, version);
	}

	public int ParseCFExTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (Template == ConditionalFormatTemplate.Filter)
		{
			m_cfExFilterParam.ParseFilterTemplateParameter(provider, iOffset, version);
		}
		else if (Template == ConditionalFormatTemplate.ContainsText)
		{
			m_cfExTextParam.ParseTextTemplateParameter(provider, iOffset, version);
		}
		else if (Template == ConditionalFormatTemplate.Today || Template == ConditionalFormatTemplate.Tomorrow || Template == ConditionalFormatTemplate.Yesterday || Template == ConditionalFormatTemplate.Last7Days || Template == ConditionalFormatTemplate.LastMonth || Template == ConditionalFormatTemplate.NextMonth || Template == ConditionalFormatTemplate.ThisWeek || Template == ConditionalFormatTemplate.NextWeek || Template == ConditionalFormatTemplate.LastWeek || Template == ConditionalFormatTemplate.ThisMonth)
		{
			m_cfExDateParam.ParseDateTemplateParameter(provider, iOffset, version);
		}
		else if (Template == ConditionalFormatTemplate.AboveAverage || Template == ConditionalFormatTemplate.BelowAverage || Template == ConditionalFormatTemplate.AboveOrEqualToAverage || Template == ConditionalFormatTemplate.BelowOrEqualToAverage)
		{
			m_cfExAverageParam.ParseAverageTemplateParameter(provider, iOffset, version);
		}
		else
		{
			m_defaultParameter = provider.ReadUInt16(iOffset);
			iOffset += 16;
		}
		return iOffset;
	}

	public int SerializeCFExTemplateParameter(DataProvider provider, int iOffset, OfficeVersion version)
	{
		if (Template == ConditionalFormatTemplate.Filter)
		{
			m_cfExFilterParam.SerializeFilterParameter(provider, iOffset, version);
		}
		else if (Template == ConditionalFormatTemplate.ContainsText)
		{
			m_cfExTextParam.SerializeTextTemplateParameter(provider, iOffset, version);
		}
		else if (Template == ConditionalFormatTemplate.Today || Template == ConditionalFormatTemplate.Tomorrow || Template == ConditionalFormatTemplate.Yesterday || Template == ConditionalFormatTemplate.Last7Days || Template == ConditionalFormatTemplate.LastMonth || Template == ConditionalFormatTemplate.NextMonth || Template == ConditionalFormatTemplate.ThisWeek || Template == ConditionalFormatTemplate.NextWeek || Template == ConditionalFormatTemplate.LastWeek || Template == ConditionalFormatTemplate.ThisMonth)
		{
			m_cfExDateParam.SerializeDateTemplateParameter(provider, iOffset, version);
		}
		else if (Template == ConditionalFormatTemplate.AboveAverage || Template == ConditionalFormatTemplate.BelowAverage || Template == ConditionalFormatTemplate.AboveOrEqualToAverage || Template == ConditionalFormatTemplate.BelowOrEqualToAverage)
		{
			m_cfExAverageParam.SerializeAverageTemplateParameter(provider, iOffset, version);
		}
		else
		{
			provider.WriteInt64(iOffset, 0L);
			iOffset += 8;
			provider.WriteInt64(iOffset, 0L);
			iOffset += 8;
		}
		return iOffset;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 18;
		int num2 = 0;
		if (m_isCF12 == 0)
		{
			num += 25;
			if (m_hasDXF != 0)
			{
				num += 4;
				if (m_sizeOfDXF == 0)
				{
					num += 2;
				}
				if (m_sizeOfDXF != 0)
				{
					num2 = m_dxfn.GetStoreSize(version);
					num += num2;
				}
				if (m_sizeOfDXF != num2)
				{
					num += 8;
					if (m_propertyCount > 0)
					{
						foreach (ExtendedProperty property in m_properties)
						{
							num += property.Size;
						}
					}
				}
			}
		}
		return num;
	}

	public override int GetHashCode()
	{
		return m_header.Type.GetHashCode() ^ m_isRefRange.GetHashCode() ^ m_isFutureAlert.GetHashCode() ^ base.CellList.GetHashCode() ^ m_isCF12.GetHashCode() ^ m_CondFMTIndex.GetHashCode() ^ m_CFIndex.GetHashCode() ^ ComparisonOperator.GetHashCode() ^ m_template.GetHashCode() ^ m_priority.GetHashCode() ^ m_undefined.GetHashCode() ^ m_hasDXF.GetHashCode() ^ m_sizeOfDXF.GetHashCode() ^ m_dxfn.GetHashCode() ^ m_propertyCount.GetHashCode() ^ m_properties.GetHashCode() ^ m_templateParamCount.GetHashCode() ^ m_cfExFilterParam.GetHashCode() ^ m_cfExTextParam.GetHashCode() ^ m_cfExDateParam.GetHashCode() ^ m_cfExAverageParam.GetHashCode() ^ m_defaultParameter.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CFExRecord cFExRecord))
		{
			return false;
		}
		if (m_header.Type == cFExRecord.m_header.Type && m_isRefRange == cFExRecord.m_isRefRange && m_isFutureAlert == cFExRecord.m_isFutureAlert && base.CellList == cFExRecord.CellList && m_isCF12 == cFExRecord.m_isCF12 && m_CondFMTIndex == cFExRecord.m_CondFMTIndex && m_CFIndex == cFExRecord.m_CFIndex && ComparisonOperator == cFExRecord.ComparisonOperator && m_template == cFExRecord.m_template && m_priority == cFExRecord.m_priority && m_undefined == cFExRecord.m_undefined && m_hasDXF == cFExRecord.m_hasDXF && m_sizeOfDXF == cFExRecord.m_sizeOfDXF && m_dxfn == cFExRecord.m_dxfn && m_propertyCount == cFExRecord.m_propertyCount && m_properties == cFExRecord.m_properties && m_templateParamCount == cFExRecord.m_templateParamCount && m_cfExFilterParam == cFExRecord.m_cfExFilterParam && m_cfExTextParam == cFExRecord.m_cfExTextParam && m_cfExDateParam == cFExRecord.m_cfExDateParam && m_cfExAverageParam == cFExRecord.m_cfExAverageParam)
		{
			return m_defaultParameter == cFExRecord.m_defaultParameter;
		}
		return false;
	}

	public override object Clone()
	{
		return (CFExRecord)MemberwiseClone();
	}

	internal void ClearAll()
	{
		m_cf12Record.ClearAll();
	}
}
