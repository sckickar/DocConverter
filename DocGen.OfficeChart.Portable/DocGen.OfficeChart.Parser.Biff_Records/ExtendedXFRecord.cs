using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ExtendedXFRecord)]
[CLSCompliant(false)]
internal class ExtendedXFRecord : BiffRecordRaw
{
	public const int StartLength = 20;

	private FutureHeader m_header;

	private ushort m_usXFIndex;

	private ushort m_propertyCount;

	private List<ExtendedProperty> m_properties;

	public ushort XFIndex
	{
		get
		{
			return m_usXFIndex;
		}
		set
		{
			m_usXFIndex = value;
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

	public ExtendedXFRecord()
	{
		InitializeObjects();
	}

	public ExtendedXFRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ExtendedXFRecord(int iReserve)
		: base(iReserve)
	{
		m_iCode = 2173;
	}

	private void InitializeObjects()
	{
		m_header = new FutureHeader();
		m_header.Type = 2173;
		m_properties = new List<ExtendedProperty>();
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_header.Type = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_header.Attributes = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadUInt16(iOffset);
		iOffset += 8;
		provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usXFIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_propertyCount = provider.ReadUInt16(iOffset);
		iOffset += 2;
		for (int i = 0; i < m_propertyCount; i++)
		{
			ExtendedProperty extendedProperty = new ExtendedProperty();
			iOffset = extendedProperty.ParseExtendedProperty(provider, iOffset, version);
			m_properties.Add(extendedProperty);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_header.Type);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_header.Attributes);
		iOffset += 2;
		provider.WriteUInt16(iOffset, 0);
		iOffset += 8;
		provider.WriteUInt16(iOffset, 0);
		iOffset += 2;
		provider.WriteUInt16(iOffset, XFIndex);
		iOffset += 2;
		provider.WriteUInt16(iOffset, 0);
		iOffset += 2;
		provider.WriteUInt16(iOffset, (ushort)Properties.Count);
		iOffset += 2;
		foreach (ExtendedProperty property in Properties)
		{
			iOffset = property.InfillInternalData(provider, iOffset, version);
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 0;
		foreach (ExtendedProperty property in m_properties)
		{
			num += property.Size;
		}
		return 20 + num;
	}

	public override int GetHashCode()
	{
		return m_header.Type.GetHashCode() ^ m_header.Attributes.GetHashCode() ^ m_usXFIndex.GetHashCode() ^ m_propertyCount.GetHashCode() ^ Properties.GetHashCode();
	}

	public int CompareTo(ExtendedXFRecord twin)
	{
		if (twin == null)
		{
			throw new ArgumentNullException("twin");
		}
		int num = m_header.Type - twin.m_header.Type;
		if (num != 0)
		{
			return num;
		}
		num = m_header.Attributes - twin.m_header.Attributes;
		if (num != 0)
		{
			return num;
		}
		num = m_usXFIndex - twin.m_usXFIndex;
		if (num != 0)
		{
			return num;
		}
		num = m_propertyCount - twin.m_propertyCount;
		if (num != 0)
		{
			return num;
		}
		if (m_properties != twin.m_properties)
		{
			return 1;
		}
		return num;
	}

	public override void CopyTo(BiffRecordRaw raw)
	{
		if (raw == null)
		{
			throw new ArgumentNullException("raw");
		}
		if (raw is ExtendedXFRecord twin)
		{
			CopyTo(twin);
			return;
		}
		throw new ArgumentException("raw");
	}

	public void CopyTo(ExtendedXFRecord twin)
	{
		twin.m_header.Type = m_header.Type;
		twin.m_header.Attributes = m_header.Attributes;
		twin.m_usXFIndex = m_usXFIndex;
		twin.m_propertyCount = m_propertyCount;
		twin.m_properties = m_properties;
	}

	public override object Clone()
	{
		ExtendedXFRecord obj = (ExtendedXFRecord)base.Clone();
		obj.Properties = new List<ExtendedProperty>();
		return obj;
	}

	public ExtendedXFRecord CloneObject()
	{
		return (ExtendedXFRecord)MemberwiseClone();
	}
}
