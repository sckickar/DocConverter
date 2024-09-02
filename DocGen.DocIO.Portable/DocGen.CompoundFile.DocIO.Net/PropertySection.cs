using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class PropertySection
{
	private class DictionaryInfo
	{
		public long StreamOffset;

		public int DataSize;
	}

	private const int PropertyNamesId = int.MinValue;

	private const short UnicodeCodePage = 1200;

	private int m_iOffset;

	private Guid m_id;

	private int m_iLength;

	private List<PropertyData> m_lstProperties = new List<PropertyData>();

	private int m_sCodePage = -1;

	private DictionaryInfo m_dictionaryInfo;

	public int Offset
	{
		get
		{
			return m_iOffset;
		}
		set
		{
			m_iOffset = value;
		}
	}

	public Guid Id
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public int Length
	{
		get
		{
			return m_iLength;
		}
		set
		{
			m_iLength = value;
		}
	}

	public int Count => m_lstProperties.Count;

	public List<PropertyData> Properties => m_lstProperties;

	public PropertySection(Guid guid, int sectionOffset)
	{
		m_id = guid;
		m_iOffset = sectionOffset;
	}

	public void Parse(Stream stream)
	{
		byte[] buffer = new byte[4];
		stream.Position = m_iOffset;
		m_iLength = StreamHelper.ReadInt32(stream, buffer);
		int num = StreamHelper.ReadInt32(stream, buffer);
		List<int> list = new List<int>();
		for (int i = 0; i < num; i++)
		{
			int id = StreamHelper.ReadInt32(stream, buffer);
			int item = StreamHelper.ReadInt32(stream, buffer);
			m_lstProperties.Add(new PropertyData(id));
			list.Add(item);
		}
		list.Add((int)stream.Length);
		Dictionary<int, string> dictNames = null;
		for (int j = 0; j < num; j++)
		{
			PropertyData propertyData = m_lstProperties[j];
			int num2 = list[j];
			int num3 = list[j + 1];
			stream.Position = m_iOffset + list[j];
			int num4 = num3 - num2;
			if (propertyData.Id < 2)
			{
				ParseSpecialProperties(propertyData, stream, num4, ref dictNames);
				continue;
			}
			ParseDictionary(stream, ref dictNames);
			propertyData.Parse(stream, num4, m_sCodePage);
			if (dictNames != null && dictNames.TryGetValue(propertyData.Id, out var value))
			{
				propertyData.Name = value;
			}
		}
	}

	private void ParseDictionary(Stream stream, ref Dictionary<int, string> dictNames)
	{
		if (m_dictionaryInfo != null)
		{
			dictNames = ParsePropertyNames(stream, m_dictionaryInfo);
			m_dictionaryInfo = null;
		}
	}

	private Dictionary<int, string> ParsePropertyNames(Stream stream, DictionaryInfo dictionaryInfo)
	{
		long position = stream.Position;
		stream.Position = dictionaryInfo.StreamOffset;
		Dictionary<int, string> result = ParsePropertyNames(stream);
		stream.Position = position;
		return result;
	}

	private void ParseSpecialProperties(PropertyData property, Stream stream, int reservedSize, ref Dictionary<int, string> dictNames)
	{
		if (property.Id == 0)
		{
			m_dictionaryInfo = new DictionaryInfo();
			m_dictionaryInfo.StreamOffset = stream.Position;
			m_dictionaryInfo.DataSize = reservedSize;
			stream.Position += reservedSize;
		}
		else if (property.Id == 1)
		{
			byte[] buffer = new byte[4];
			property.PropertyType = (PropertyType)StreamHelper.ReadInt32(stream, buffer);
			if ((property.PropertyType & PropertyType.Vector) != 0)
			{
				property.Value = property.ParseVector(stream, reservedSize, m_sCodePage);
			}
			else
			{
				PropertyType itemType = ((property.PropertyType == PropertyType.Int16) ? PropertyType.Int32 : property.PropertyType);
				property.Value = property.ParseSingleValue(itemType, stream, reservedSize, m_sCodePage);
			}
			m_sCodePage = (int)property.Value;
			ParseDictionary(stream, ref dictNames);
		}
		else
		{
			property.Parse(stream, reservedSize, m_sCodePage);
		}
	}

	private Dictionary<int, string> ParsePropertyNames(Stream stream)
	{
		byte[] buffer = new byte[4];
		int num = StreamHelper.ReadInt32(stream, buffer);
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		for (int i = 0; i < num; i++)
		{
			int key = StreamHelper.ReadInt32(stream, buffer);
			string value = (((short)m_sCodePage != 1200) ? StreamHelper.GetAsciiString(stream, -1, m_sCodePage) : StreamHelper.GetUnicodeString(stream, -1));
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	public void Serialize(Stream stream)
	{
		_ = new byte[4];
		m_iOffset = (int)stream.Position;
		StreamHelper.WriteInt32(stream, 0);
		PrepareNames();
		int count = m_lstProperties.Count;
		StreamHelper.WriteInt32(stream, count);
		stream.Position += count * 8;
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			PropertyData propertyData = m_lstProperties[i];
			list.Add((int)stream.Position);
			propertyData.Serialize(stream);
		}
		long position = stream.Position;
		stream.Position = m_iOffset + 8;
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			int value = list[j] - m_iOffset;
			StreamHelper.WriteInt32(stream, m_lstProperties[j].Id);
			StreamHelper.WriteInt32(stream, value);
		}
		m_iLength = (int)(position - m_iOffset);
		stream.Position = m_iOffset;
		StreamHelper.WriteInt32(stream, m_iLength);
		stream.Position = position;
	}

	private Dictionary<int, string> PrepareNames()
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		int i = 0;
		for (int count = m_lstProperties.Count; i < count; i++)
		{
			PropertyData propertyData = m_lstProperties[i];
			if (propertyData.Name != null)
			{
				dictionary.Add(propertyData.Id, propertyData.Name);
			}
		}
		if (dictionary.Count > 0)
		{
			PropertyData propertyData2 = new PropertyData(0);
			propertyData2.Value = dictionary;
			m_lstProperties.Insert(0, propertyData2);
		}
		return dictionary;
	}
}
