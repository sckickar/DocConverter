using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class DocumentPropertyCollection
{
	private const int ByteOrder = 65534;

	private static readonly Guid FirstSectionGuid = new Guid("f29f85e0-4ff9-1068-ab91-08002b27b3d9");

	private int m_iFirstSectionOffset = -1;

	private List<PropertySection> m_lstSections = new List<PropertySection>();

	public List<PropertySection> Sections => m_lstSections;

	public DocumentPropertyCollection()
	{
	}

	public DocumentPropertyCollection(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		stream.Position = 0L;
		ReadHeader(stream);
		ParseSections(stream);
	}

	private void ParseSections(Stream stream)
	{
		int i = 0;
		for (int count = m_lstSections.Count; i < count; i++)
		{
			m_lstSections[i].Parse(stream);
		}
	}

	private void ReadHeader(Stream stream)
	{
		byte[] array = new byte[16];
		stream.Read(array, 0, 4);
		int num = BitConverter.ToInt32(array, 0);
		if (num != 65534)
		{
			throw new IOException($"iValue = {num} instead of {65534}");
		}
		stream.Read(array, 0, 2);
		stream.Read(array, 0, 2);
		stream.Read(array, 0, 16);
		stream.Read(array, 0, 4);
		int num2 = BitConverter.ToInt32(array, 0);
		for (int i = 0; i < num2; i++)
		{
			stream.Read(array, 0, 16);
			Guid guid = new Guid(array);
			int sectionOffset = StreamHelper.ReadInt32(stream, array);
			m_lstSections.Add(new PropertySection(guid, sectionOffset));
		}
	}

	private void WriteSections(Stream stream)
	{
		int i = 0;
		for (int count = m_lstSections.Count; i < count; i++)
		{
			m_lstSections[i].Serialize(stream);
		}
	}

	private void WriteHeader(Stream stream)
	{
		_ = new byte[16];
		StreamHelper.WriteInt32(stream, 65534);
		StreamHelper.WriteInt16(stream, 261);
		StreamHelper.WriteInt16(stream, 2);
		for (int i = 0; i < 16; i++)
		{
			stream.WriteByte(0);
		}
		int count = m_lstSections.Count;
		StreamHelper.WriteInt32(stream, count);
		List<long> list = new List<long>();
		for (int j = 0; j < count; j++)
		{
			byte[] array = m_lstSections[j].Id.ToByteArray();
			stream.Write(array, 0, array.Length);
			list.Add(stream.Position);
			StreamHelper.WriteInt32(stream, 0);
		}
		for (int k = 0; k < count; k++)
		{
			PropertySection propertySection = m_lstSections[k];
			long position = stream.Position;
			stream.Position = list[k];
			StreamHelper.WriteInt32(stream, (int)position);
			stream.Position = position;
			propertySection.Serialize(stream);
		}
	}

	public void Serialize(Stream stream)
	{
		WriteHeader(stream);
	}
}
