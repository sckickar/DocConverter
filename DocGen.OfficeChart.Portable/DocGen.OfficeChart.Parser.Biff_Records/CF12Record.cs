using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CF12)]
[CLSCompliant(false)]
internal class CF12Record : BiffRecordRaw
{
	private const int DEF_MINIMUM_RECORD_SIZE = 46;

	private FutureHeader m_header;

	private bool m_isRange;

	private bool m_isFutureAlert;

	private TAddr m_addrEncloseRange;

	private byte m_typeOfCondition = 1;

	private byte m_compareOperator;

	private ushort m_usFirstFormulaSize;

	private ushort m_usSecondFormulaSize;

	private byte[] m_arrFirstFormula = new byte[0];

	private byte[] m_arrSecondFormula = new byte[0];

	private Ptg[] m_arrFirstFormulaParsed;

	private Ptg[] m_arrSecondFormulaParsed;

	private ushort m_formulaLength;

	private byte[] m_arrFormula = new byte[0];

	private Ptg[] m_arrFormulaParsed;

	private ushort m_sizeOfDXF;

	private ushort m_propertyCount;

	private List<ExtendedProperty> m_properties;

	private byte m_undefined;

	private ushort m_priority;

	private ushort m_template;

	private ushort m_templateParamCount = 16;

	private long m_defaultParameter;

	private ushort m_reserved = ushort.MaxValue;

	private DXFN m_dxfn;

	private CFExFilterParameter m_cfExFilterParam;

	private CFExTextTemplateParameter m_cfExTextParam;

	private CFExDateTemplateParameter m_cfExDateParam;

	private CFExAverageTemplateParameter m_cfExAverageParam;

	private DataBar m_dataBar;

	private CFIconSet m_iconSet;

	private ColorScale m_colorScale;

	private bool m_isParsed;

	public ExcelCFType FormatType
	{
		get
		{
			return (ExcelCFType)m_typeOfCondition;
		}
		set
		{
			m_typeOfCondition = (byte)value;
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

	public ushort FirstFormulaSize => m_usFirstFormulaSize;

	public ushort SecondFormulaSize => m_usSecondFormulaSize;

	public Ptg[] FirstFormulaPtgs
	{
		get
		{
			return m_arrFirstFormulaParsed;
		}
		set
		{
			m_arrFirstFormula = FormulaUtil.PtgArrayToByteArray(value, OfficeVersion.Excel2007);
			m_arrFirstFormulaParsed = value;
			m_usFirstFormulaSize = (ushort)m_arrFirstFormula.Length;
		}
	}

	public Ptg[] SecondFormulaPtgs
	{
		get
		{
			return m_arrSecondFormulaParsed;
		}
		set
		{
			m_arrSecondFormula = FormulaUtil.PtgArrayToByteArray(value, OfficeVersion.Excel2007);
			m_arrSecondFormulaParsed = value;
			m_usSecondFormulaSize = (ushort)m_arrSecondFormula.Length;
		}
	}

	public byte[] FirstFormulaBytes => m_arrFirstFormula;

	public byte[] SecondFormulaBytes => m_arrSecondFormula;

	public Ptg[] FormulaPtgs
	{
		get
		{
			return m_arrFormulaParsed;
		}
		set
		{
			m_arrFormula = FormulaUtil.PtgArrayToByteArray(value, OfficeVersion.Excel2007);
			m_arrFormulaParsed = value;
			m_formulaLength = (ushort)m_arrFormula.Length;
		}
	}

	public byte[] FormulaBytes => m_arrFormula;

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

	public bool IsParsed
	{
		get
		{
			return m_isParsed;
		}
		set
		{
			m_isParsed = value;
		}
	}

	public ColorScale ColorScaleCF12
	{
		get
		{
			return m_colorScale;
		}
		set
		{
			m_colorScale = value;
		}
	}

	public DataBar DataBarCF12
	{
		get
		{
			return m_dataBar;
		}
		set
		{
			m_dataBar = value;
		}
	}

	public CFIconSet IconSetCF12
	{
		get
		{
			return m_iconSet;
		}
		set
		{
			m_iconSet = value;
		}
	}

	public CF12Record()
	{
		m_header = new FutureHeader();
		m_header.Type = 2170;
		m_dxfn = new DXFN();
		m_colorScale = new ColorScale();
		m_dataBar = new DataBar();
		m_iconSet = new CFIconSet();
		m_properties = new List<ExtendedProperty>();
		m_cfExFilterParam = new CFExFilterParameter();
		m_cfExTextParam = new CFExTextTemplateParameter();
		m_cfExDateParam = new CFExDateTemplateParameter();
		m_cfExAverageParam = new CFExAverageTemplateParameter();
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		IsParsed = true;
		m_header.Type = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_isRange = provider.ReadBit(iOffset, 0);
		m_isFutureAlert = provider.ReadBit(iOffset, 1);
		iOffset += 2;
		m_addrEncloseRange = provider.ReadAddr(iOffset);
		iOffset += 8;
		m_typeOfCondition = provider.ReadByte(iOffset);
		iOffset++;
		m_compareOperator = provider.ReadByte(iOffset);
		iOffset++;
		m_usFirstFormulaSize = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usSecondFormulaSize = provider.ReadUInt16(iOffset);
		iOffset += 2;
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
		m_arrFirstFormula = new byte[m_usFirstFormulaSize];
		provider.ReadArray(iOffset, m_arrFirstFormula);
		iOffset += m_usFirstFormulaSize;
		m_arrSecondFormula = new byte[m_usSecondFormulaSize];
		provider.ReadArray(iOffset, m_arrSecondFormula);
		iOffset += m_usSecondFormulaSize;
		m_arrFirstFormulaParsed = FormulaUtil.ParseExpression(new ByteArrayDataProvider(m_arrFirstFormula), m_usFirstFormulaSize, version);
		m_arrSecondFormulaParsed = FormulaUtil.ParseExpression(new ByteArrayDataProvider(m_arrSecondFormula), m_usSecondFormulaSize, version);
		if (version != OfficeVersion.Excel2007)
		{
			if (m_usFirstFormulaSize > 0)
			{
				m_arrFirstFormula = FormulaUtil.PtgArrayToByteArray(m_arrFirstFormulaParsed, OfficeVersion.Excel2007);
				m_usFirstFormulaSize = (ushort)m_arrFirstFormula.Length;
			}
			if (m_usSecondFormulaSize > 0)
			{
				m_arrSecondFormula = FormulaUtil.PtgArrayToByteArray(m_arrSecondFormulaParsed, OfficeVersion.Excel2007);
				m_usSecondFormulaSize = (ushort)m_arrSecondFormula.Length;
			}
		}
		m_formulaLength = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_arrFormula = new byte[m_formulaLength];
		provider.ReadArray(iOffset, m_arrFormula);
		iOffset += m_formulaLength;
		m_arrFormulaParsed = FormulaUtil.ParseExpression(new ByteArrayDataProvider(m_arrFormula), m_formulaLength, version);
		if (version != OfficeVersion.Excel2007 && m_formulaLength > 0)
		{
			m_arrFormula = FormulaUtil.PtgArrayToByteArray(m_arrFormulaParsed, OfficeVersion.Excel2007);
			m_formulaLength = (ushort)m_arrFormula.Length;
		}
		m_undefined = provider.ReadByte(iOffset);
		iOffset++;
		m_priority = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_template = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_templateParamCount = provider.ReadByte(iOffset);
		iOffset++;
		iOffset = ParseCFExTemplateParameter(provider, iOffset, version);
		switch (FormatType)
		{
		case ExcelCFType.ColorScale:
			m_colorScale = new ColorScale();
			iOffset = m_colorScale.ParseColorScale(provider, iOffset, version);
			break;
		case ExcelCFType.DataBar:
			m_dataBar = new DataBar();
			iOffset = m_dataBar.ParseDataBar(provider, iOffset, version);
			break;
		case ExcelCFType.IconSet:
			m_iconSet = new CFIconSet();
			iOffset = m_iconSet.ParseIconSet(provider, iOffset, version);
			break;
		case (ExcelCFType)5:
			break;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		if (m_arrFirstFormulaParsed != null && m_arrFirstFormulaParsed.Length != 0)
		{
			m_arrFirstFormula = FormulaUtil.PtgArrayToByteArray(m_arrFirstFormulaParsed, version);
			m_usFirstFormulaSize = (ushort)m_arrFirstFormula.Length;
		}
		else
		{
			m_arrFirstFormula = null;
			m_usFirstFormulaSize = 0;
		}
		if (m_arrSecondFormulaParsed != null && m_arrSecondFormulaParsed.Length != 0)
		{
			m_arrSecondFormula = FormulaUtil.PtgArrayToByteArray(m_arrSecondFormulaParsed, version);
			m_usSecondFormulaSize = (ushort)m_arrSecondFormula.Length;
		}
		else
		{
			m_arrSecondFormula = null;
			m_usSecondFormulaSize = 0;
		}
		if (m_arrFormulaParsed != null && m_arrFormulaParsed.Length != 0)
		{
			m_arrFormula = FormulaUtil.PtgArrayToByteArray(m_arrFormulaParsed, version);
			m_formulaLength = (ushort)m_arrFormula.Length;
		}
		else
		{
			m_arrFormula = null;
			m_formulaLength = 0;
		}
		provider.WriteUInt16(iOffset, m_header.Type);
		iOffset += 2;
		provider.WriteBit(iOffset, m_isRange, 0);
		provider.WriteBit(iOffset, m_isFutureAlert, 1);
		iOffset += 2;
		provider.WriteAddr(iOffset, m_addrEncloseRange);
		iOffset += 8;
		provider.WriteByte(iOffset, m_typeOfCondition);
		iOffset++;
		provider.WriteByte(iOffset, m_compareOperator);
		iOffset++;
		provider.WriteUInt16(iOffset, m_usFirstFormulaSize);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usSecondFormulaSize);
		iOffset += 2;
		if (FormatType == ExcelCFType.ColorScale || FormatType == ExcelCFType.DataBar || FormatType == ExcelCFType.IconSet)
		{
			m_sizeOfDXF = 0;
		}
		provider.WriteUInt32(iOffset, m_sizeOfDXF);
		iOffset += 4;
		if (m_sizeOfDXF == 0)
		{
			provider.WriteUInt16(iOffset, 0);
			iOffset += 2;
		}
		int num = iOffset;
		if (m_sizeOfDXF != 0)
		{
			iOffset = m_dxfn.SerializeDXFN(provider, iOffset, version);
		}
		num = iOffset - num;
		if (m_sizeOfDXF != num)
		{
			provider.WriteUInt16(iOffset, 0);
			iOffset += 2;
			provider.WriteUInt16(iOffset, m_reserved);
			iOffset += 2;
			provider.WriteUInt16(iOffset, 0);
			iOffset += 2;
			provider.WriteUInt16(iOffset, m_propertyCount);
			iOffset += 2;
			foreach (ExtendedProperty property in m_properties)
			{
				iOffset = property.InfillInternalData(provider, iOffset, version);
			}
		}
		provider.WriteBytes(iOffset, m_arrFirstFormula, 0, m_usFirstFormulaSize);
		iOffset += m_usFirstFormulaSize;
		provider.WriteBytes(iOffset, m_arrSecondFormula, 0, m_usSecondFormulaSize);
		iOffset += m_usSecondFormulaSize;
		provider.WriteUInt16(iOffset, m_formulaLength);
		iOffset += 2;
		provider.WriteBytes(iOffset, m_arrFormula, 0, m_formulaLength);
		iOffset += m_formulaLength;
		provider.WriteByte(iOffset, m_undefined);
		iOffset++;
		provider.WriteUInt16(iOffset, m_priority);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_template);
		iOffset += 2;
		provider.WriteByte(iOffset, (byte)m_templateParamCount);
		iOffset++;
		iOffset = SerializeCFExTemplateParameter(provider, iOffset, version);
		switch (FormatType)
		{
		}
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
			m_defaultParameter = provider.ReadInt64(iOffset);
			iOffset += 8;
			m_defaultParameter = provider.ReadInt64(iOffset);
			iOffset += 8;
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
		int num = 46;
		if (m_sizeOfDXF == 0)
		{
			num += 2;
		}
		if (FormatType == ExcelCFType.ColorScale)
		{
			num = ((m_colorScale.ListCFInterpolationCurve.Count != 0) ? (num + m_colorScale.GetStoreSize(version)) : (num + m_colorScale.DefaultRecordSize));
			num += DVRecord.GetFormulaSize(m_arrFormulaParsed, version, addAdditionalDataSize: true);
		}
		if (FormatType == ExcelCFType.DataBar)
		{
			num += m_dataBar.GetStoreSize(version);
			num += DVRecord.GetFormulaSize(m_arrFormulaParsed, version, addAdditionalDataSize: true);
		}
		if (FormatType == ExcelCFType.IconSet)
		{
			num = ((m_iconSet.ListCFIconSet.Count != 0) ? (num + m_iconSet.GetStoreSize(version)) : (num + m_iconSet.DefaultRecordSize));
			num += DVRecord.GetFormulaSize(m_arrFormulaParsed, version, addAdditionalDataSize: true);
		}
		if (FormatType == ExcelCFType.CellValue)
		{
			num += m_sizeOfDXF;
			num += DVRecord.GetFormulaSize(m_arrFirstFormulaParsed, version, addAdditionalDataSize: true);
			num += DVRecord.GetFormulaSize(m_arrSecondFormulaParsed, version, addAdditionalDataSize: true);
		}
		return num;
	}

	public override int GetHashCode()
	{
		return m_header.Type.GetHashCode() ^ m_isRange.GetHashCode() ^ m_isFutureAlert.GetHashCode() ^ m_addrEncloseRange.GetHashCode() ^ m_typeOfCondition.GetHashCode() ^ m_compareOperator.GetHashCode() ^ m_usFirstFormulaSize.GetHashCode() ^ m_usSecondFormulaSize.GetHashCode() ^ m_sizeOfDXF.GetHashCode() ^ m_dxfn.GetHashCode() ^ m_propertyCount.GetHashCode() ^ m_properties.GetHashCode() ^ m_arrFirstFormula.GetHashCode() ^ m_arrSecondFormula.GetHashCode() ^ m_formulaLength.GetHashCode() ^ m_arrFormula.GetHashCode() ^ m_undefined.GetHashCode() ^ m_priority.GetHashCode() ^ m_template.GetHashCode() ^ m_templateParamCount.GetHashCode() ^ (m_cfExTextParam.GetHashCode() ^ m_cfExDateParam.GetHashCode() ^ m_cfExFilterParam.GetHashCode() ^ m_cfExAverageParam.GetHashCode()) ^ (m_colorScale.GetHashCode() ^ m_dataBar.GetHashCode() ^ m_iconSet.GetHashCode());
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CF12Record cF12Record))
		{
			return false;
		}
		if (m_typeOfCondition == cF12Record.m_typeOfCondition && m_compareOperator == cF12Record.m_compareOperator && m_usFirstFormulaSize == cF12Record.m_usFirstFormulaSize && m_usSecondFormulaSize == cF12Record.m_usSecondFormulaSize && m_sizeOfDXF == cF12Record.m_sizeOfDXF && m_dxfn == cF12Record.m_dxfn && m_propertyCount == cF12Record.m_propertyCount && m_properties == cF12Record.m_properties && m_arrFirstFormula == cF12Record.m_arrFirstFormula && m_arrSecondFormula == cF12Record.m_arrSecondFormula && m_formulaLength == cF12Record.m_formulaLength && m_arrFormula == cF12Record.m_arrFormula && m_undefined == cF12Record.m_undefined && m_priority == cF12Record.m_priority && m_template == cF12Record.m_template && m_templateParamCount == cF12Record.m_templateParamCount && m_cfExAverageParam == cF12Record.m_cfExAverageParam && m_cfExDateParam == cF12Record.m_cfExDateParam && m_cfExFilterParam == cF12Record.m_cfExFilterParam && m_cfExTextParam == cF12Record.m_cfExTextParam)
		{
			if (m_colorScale == cF12Record.m_colorScale && m_dataBar == cF12Record.m_dataBar)
			{
				return m_iconSet == cF12Record.m_iconSet;
			}
			return false;
		}
		return false;
	}

	internal void ClearAll()
	{
		m_iconSet.ClearAll();
		m_iconSet = null;
		m_header = null;
		m_dxfn = null;
		m_arrFirstFormulaParsed = null;
		m_arrFirstFormula = null;
		m_arrFormula = null;
		m_arrSecondFormula = null;
		m_arrSecondFormulaParsed = null;
	}
}
