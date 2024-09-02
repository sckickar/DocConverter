using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.CompoundFile.Net;

namespace DocGen.CompoundFile.DocIO.Net;

internal class PropertyData : IPropertyData, IComparable
{
	private const int LinkBit = 16777216;

	private const int NamesDictionaryId = 0;

	public const long DEF_FILETIME_TICKS_DIFFERENCE = 504911232000000000L;

	private int m_iId;

	private string m_strName;

	public PropertyType PropertyType;

	public object Data;

	public bool IsLinkToSource => (Id & 0x1000000) != 0;

	public int ParentId
	{
		get
		{
			if (!IsLinkToSource)
			{
				return Id;
			}
			return Id - 16777216;
		}
	}

	public int Id
	{
		get
		{
			return m_iId;
		}
		set
		{
			m_iId = value;
		}
	}

	public object Value
	{
		get
		{
			return Data;
		}
		set
		{
			Data = value;
		}
	}

	public VarEnum Type
	{
		get
		{
			return (VarEnum)PropertyType;
		}
		set
		{
			PropertyType = (PropertyType)value;
		}
	}

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			m_strName = value;
		}
	}

	internal PropertyData()
	{
	}

	public PropertyData(int id)
	{
		Id = id;
	}

	public void Parse(Stream stream, int roundedSize)
	{
		byte[] buffer = new byte[4];
		PropertyType = (PropertyType)StreamHelper.ReadInt32(stream, buffer);
		if ((PropertyType & PropertyType.Vector) != 0)
		{
			Data = ParseVector(stream, roundedSize);
		}
		else
		{
			Data = ParseSingleValue(PropertyType, stream, roundedSize);
		}
	}

	internal void Parse(Stream stream, int roundedSize, int codePage)
	{
		byte[] buffer = new byte[4];
		PropertyType = (PropertyType)StreamHelper.ReadInt32(stream, buffer);
		if ((PropertyType & PropertyType.Vector) != 0)
		{
			Data = ParseVector(stream, roundedSize, codePage);
		}
		else
		{
			Data = ParseSingleValue(PropertyType, stream, roundedSize, codePage);
		}
	}

	internal bool IsValidProperty()
	{
		switch (Id)
		{
		case 1:
			return PropertyType == PropertyType.Int16;
		case 2:
		case 3:
		case 14:
		case 15:
		case 26:
		case 27:
		case 28:
		case 29:
			return PropertyType == PropertyType.AsciiString;
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 17:
		case 23:
			return PropertyType == PropertyType.Int32;
		case 11:
		case 16:
		case 19:
		case 22:
			return PropertyType == PropertyType.Bool;
		case 12:
			if (PropertyType != PropertyType.Vector)
			{
				return PropertyType == PropertyType.Object;
			}
			return true;
		case 13:
			if (PropertyType != PropertyType.Vector)
			{
				return PropertyType == PropertyType.AsciiString;
			}
			return true;
		case 24:
			return PropertyType == PropertyType.Blob;
		default:
			return false;
		}
	}

	private IList ParseVector(Stream stream, int roundedSize)
	{
		byte[] buffer = new byte[4];
		int num = StreamHelper.ReadInt32(stream, buffer);
		PropertyType itemType = PropertyType & ~PropertyType.Vector;
		IList list = CreateArray(itemType, num);
		for (int i = 0; i < num; i++)
		{
			list[i] = ParseSingleValue(itemType, stream, roundedSize - 4);
		}
		return list;
	}

	internal IList ParseVector(Stream stream, int roundedSize, int codePage)
	{
		byte[] buffer = new byte[4];
		int num = StreamHelper.ReadInt32(stream, buffer);
		PropertyType itemType = PropertyType & ~PropertyType.Vector;
		IList list = CreateArray(itemType, num);
		for (int i = 0; i < num; i++)
		{
			list[i] = ParseSingleValue(itemType, stream, roundedSize - 4, codePage);
		}
		return list;
	}

	private IList CreateArray(PropertyType itemType, int count)
	{
		switch (itemType)
		{
		case PropertyType.AsciiString:
		case PropertyType.String:
			return new string[count];
		case PropertyType.Int32:
		case PropertyType.Int:
			return new int[count];
		default:
			return new object[count];
		}
	}

	private object ParseSingleValue(PropertyType itemType, Stream stream, int roundedSize)
	{
		byte[] buffer = new byte[8];
		object obj = null;
		switch (itemType)
		{
		case PropertyType.Bool:
			obj = StreamHelper.ReadInt32(stream, buffer) != 0;
			break;
		case PropertyType.Blob:
			obj = GetBlob(stream, buffer);
			break;
		case PropertyType.ClipboardData:
			obj = GetClipboardData(stream, buffer);
			break;
		case PropertyType.DateTime:
			obj = GetDateTime(stream, buffer);
			break;
		case PropertyType.Double:
			obj = StreamHelper.ReadDouble(stream, buffer);
			break;
		case PropertyType.Int32:
		case PropertyType.Int:
			obj = StreamHelper.ReadInt32(stream, buffer);
			break;
		case PropertyType.UInt32:
			obj = (uint)StreamHelper.ReadInt16(stream, buffer);
			break;
		case PropertyType.Int16:
			obj = StreamHelper.ReadInt16(stream, buffer);
			stream.Position += 2L;
			break;
		case PropertyType.AsciiString:
			obj = StreamHelper.GetAsciiString(stream, roundedSize - 4);
			break;
		case PropertyType.String:
			obj = StreamHelper.GetUnicodeString(stream, roundedSize - 4);
			break;
		case PropertyType.Empty:
		case PropertyType.Null:
			obj = null;
			break;
		case PropertyType.Object:
			obj = GetObject(stream, roundedSize - 4);
			break;
		default:
			throw new NotImplementedException();
		}
		return obj;
	}

	internal object ParseSingleValue(PropertyType itemType, Stream stream, int roundedSize, int codePage)
	{
		byte[] buffer = new byte[8];
		object obj = null;
		switch (itemType)
		{
		case PropertyType.Bool:
			obj = StreamHelper.ReadInt32(stream, buffer) != 0;
			break;
		case PropertyType.Blob:
			obj = GetBlob(stream, buffer);
			break;
		case PropertyType.ClipboardData:
			obj = GetClipboardData(stream, buffer);
			break;
		case PropertyType.DateTime:
			obj = GetDateTime(stream, buffer);
			break;
		case PropertyType.Double:
			obj = StreamHelper.ReadDouble(stream, buffer);
			break;
		case PropertyType.Int32:
		case PropertyType.Int:
			obj = StreamHelper.ReadInt32(stream, buffer);
			break;
		case PropertyType.UInt32:
			obj = (uint)StreamHelper.ReadInt16(stream, buffer);
			break;
		case PropertyType.Int16:
			obj = StreamHelper.ReadInt16(stream, buffer);
			stream.Position += 2L;
			break;
		case PropertyType.AsciiString:
			obj = StreamHelper.GetAsciiString(stream, roundedSize - 4, codePage);
			break;
		case PropertyType.String:
			obj = StreamHelper.GetUnicodeString(stream, roundedSize - 4);
			break;
		case PropertyType.Empty:
		case PropertyType.Null:
			obj = null;
			break;
		case PropertyType.Object:
			obj = GetObject(stream, roundedSize - 4, codePage);
			break;
		default:
			throw new NotImplementedException();
		}
		return obj;
	}

	private object GetDateTime(Stream stream, byte[] buffer)
	{
		stream.Read(buffer, 0, 8);
		long ticks = BitConverter.ToInt64(buffer, 0) + 504911232000000000L;
		DateTime dateTime = new DateTime(ticks);
		if (Id != 10)
		{
			dateTime = dateTime.ToLocalTime();
		}
		return dateTime;
	}

	private object GetBlob(Stream stream, byte[] buffer)
	{
		int num = StreamHelper.ReadInt32(stream, buffer);
		byte[] array = new byte[num];
		if (stream.Read(array, 0, num) != num)
		{
			throw new Exception();
		}
		return array;
	}

	private object GetClipboardData(Stream stream, byte[] buffer)
	{
		ClipboardData clipboardData = new ClipboardData();
		clipboardData.Parse(stream);
		return clipboardData;
	}

	private object GetObject(Stream stream, int roundedSize)
	{
		byte[] buffer = new byte[4];
		PropertyType itemType = (PropertyType)StreamHelper.ReadInt32(stream, buffer);
		return ParseSingleValue(itemType, stream, roundedSize - 4);
	}

	private object GetObject(Stream stream, int roundedSize, int codePage)
	{
		byte[] buffer = new byte[4];
		PropertyType itemType = (PropertyType)StreamHelper.ReadInt32(stream, buffer);
		return ParseSingleValue(itemType, stream, roundedSize - 4, codePage);
	}

	private int WriteObject(Stream stream, object value)
	{
		PropertyType propertyType;
		if (value is int)
		{
			propertyType = PropertyType.Int32;
		}
		else if (value is double)
		{
			propertyType = PropertyType.Double;
		}
		else if (value is bool)
		{
			propertyType = PropertyType.Bool;
		}
		else
		{
			if (!(value is string))
			{
				throw new NotImplementedException();
			}
			propertyType = PropertyType.String;
		}
		StreamHelper.WriteInt32(stream, (int)propertyType);
		return SerializeSingleValue(stream, value, propertyType) + 4;
	}

	public int Serialize(Stream stream)
	{
		_ = new byte[8];
		int num = StreamHelper.WriteInt32(stream, (int)PropertyType);
		if ((PropertyType & PropertyType.Vector) == PropertyType.Vector)
		{
			num += SerializeVector(stream, (IList)Data);
		}
		else if (Id == 0)
		{
			stream.Position -= 4L;
			num += SerializeDictionary(stream, (Dictionary<int, string>)Data);
		}
		else
		{
			num += SerializeSingleValue(stream, Data, PropertyType);
		}
		if (PropertyType != PropertyType.AsciiString)
		{
			StreamHelper.AddPadding(stream, ref num);
		}
		return num;
	}

	private int SerializeDictionary(Stream stream, Dictionary<int, string> dictionary)
	{
		int num = 0;
		int count = dictionary.Count;
		num += StreamHelper.WriteInt32(stream, count);
		foreach (KeyValuePair<int, string> item in dictionary)
		{
			num += StreamHelper.WriteInt32(stream, item.Key);
			num += StreamHelper.WriteAsciiString(stream, item.Value, align: false);
		}
		return num;
	}

	private int SerializeVector(Stream stream, IList data)
	{
		int count = data.Count;
		StreamHelper.WriteInt32(stream, count);
		int num = 4;
		PropertyType valueType = PropertyType & ~PropertyType.Vector;
		for (int i = 0; i < count; i++)
		{
			num += SerializeSingleValue(stream, data[i], valueType);
		}
		return num;
	}

	private int SerializeSingleValue(Stream stream, object value, PropertyType valueType)
	{
		int num = 0;
		switch (valueType)
		{
		case PropertyType.Bool:
		{
			bool flag = (bool)value;
			num += StreamHelper.WriteInt32(stream, flag ? 1 : 0);
			break;
		}
		case PropertyType.Blob:
		{
			byte[] value2 = (byte[])value;
			num += SerializeBlob(stream, value2);
			break;
		}
		case PropertyType.ClipboardData:
		{
			ClipboardData data = (ClipboardData)value;
			num += SerializeClipboardData(stream, data);
			break;
		}
		case PropertyType.DateTime:
		{
			DateTime dateTime = ((!(value is TimeSpan timeSpan)) ? ((DateTime)value) : DateTime.FromBinary(timeSpan.Ticks));
			if (Id != 10)
			{
				dateTime = dateTime.ToUniversalTime();
			}
			byte[] bytes = BitConverter.GetBytes((ulong)(dateTime.Ticks - 504911232000000000L));
			stream.Write(bytes, 0, bytes.Length);
			num += bytes.Length;
			break;
		}
		case PropertyType.Double:
			num += StreamHelper.WriteDouble(stream, (double)value);
			break;
		case PropertyType.Int32:
		case PropertyType.Int:
			num += StreamHelper.WriteInt32(stream, (int)value);
			break;
		case PropertyType.UInt32:
			num += StreamHelper.WriteInt32(stream, (int)(uint)value);
			break;
		case PropertyType.Int16:
			num = ((Id != 1) ? (num + StreamHelper.WriteInt16(stream, (short)value)) : (num + StreamHelper.WriteInt32(stream, Convert.ToInt32(value))));
			break;
		case PropertyType.AsciiString:
			num += StreamHelper.WriteAsciiString(stream, (string)value, align: false);
			break;
		case PropertyType.String:
			num += StreamHelper.WriteUnicodeString(stream, (string)value);
			break;
		case PropertyType.Object:
			num += WriteObject(stream, value);
			break;
		default:
			throw new NotImplementedException();
		case PropertyType.Empty:
		case PropertyType.Null:
			break;
		}
		return num;
	}

	private int SerializeClipboardData(Stream stream, ClipboardData data)
	{
		return data.Serialize(stream);
	}

	private int SerializeBlob(Stream stream, byte[] value)
	{
		int num = value.Length;
		int num2 = 0 + StreamHelper.WriteInt32(stream, num);
		stream.Write(value, 0, num);
		return num2 + num;
	}

	public bool SetValue(object value, PropertyType type)
	{
		bool result = false;
		switch (type)
		{
		case PropertyType.Empty:
		case PropertyType.Null:
		case PropertyType.Int16:
		case PropertyType.Int32:
		case PropertyType.Double:
		case PropertyType.Bool:
		case PropertyType.Object:
		case PropertyType.UInt32:
		case PropertyType.Int:
		case PropertyType.AsciiString:
		case PropertyType.String:
		case PropertyType.DateTime:
		case PropertyType.Blob:
		case PropertyType.ClipboardData:
		case PropertyType.Vector:
		case PropertyType.ObjectArray:
		case PropertyType.AsciiStringArray:
		case PropertyType.StringArray:
			Value = value;
			Type = (VarEnum)type;
			result = true;
			break;
		}
		return result;
	}

	public int CompareTo(object obj)
	{
		PropertyData propertyData = (PropertyData)obj;
		return Id - propertyData.Id;
	}
}
