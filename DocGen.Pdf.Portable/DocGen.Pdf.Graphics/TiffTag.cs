using System.Collections.Generic;
using System.IO;
using DocGen.Pdf.Graphics.Images.Decoder;

namespace DocGen.Pdf.Graphics;

internal class TiffTag
{
	private Stack<TagDirectory> m_dictionary = new Stack<TagDirectory>();

	private TagDirectory m_currentTag;

	private List<TagDirectory> m_directories = new List<TagDirectory>();

	private MemoryStream m_stream;

	internal MemoryStream Data => m_stream;

	internal void PushDirectory(TagDirectory tag)
	{
		if (m_currentTag != null)
		{
			m_dictionary.Push(m_currentTag);
			tag.Parent = m_currentTag;
		}
		m_currentTag = tag;
		m_directories.Add(m_currentTag);
	}

	internal void SetTiffMarker(int marker)
	{
		switch (marker)
		{
		case 42:
		case 20306:
		case 21330:
			PushDirectory(new TagDirectory("Exif IFD0"));
			break;
		case 85:
			PushDirectory(new TagDirectory("PanasonicRaw Exif IFD0"));
			break;
		}
	}

	internal bool TryCustomProcessFormat(int tagId, DataTypeID formatCode, uint componentCount, out long byteCount)
	{
		byteCount = 0L;
		switch (formatCode)
		{
		case (DataTypeID)13:
			byteCount = 4L * (long)componentCount;
			return true;
		case (DataTypeID)0:
			return true;
		default:
			return false;
		}
	}

	internal bool TryEnterSubDirectory(int tagId)
	{
		if (tagId == 330)
		{
			PushDirectory(new TagDirectory("Exif SubIFD"));
			return true;
		}
		if (m_currentTag.Name == "Exif IFD0" || m_currentTag.Name == "PanasonicRaw Exif IFD0")
		{
			switch (tagId)
			{
			case 34665:
				PushDirectory(new TagDirectory("Exif SubIFD"));
				return true;
			case 34853:
				PushDirectory(new TagDirectory("GPS"));
				return true;
			}
		}
		else if (m_currentTag.Name == "Exif SubIFD")
		{
			if (tagId == 40965)
			{
				PushDirectory(new TagDirectory("Interoperability"));
				return true;
			}
		}
		else if (m_currentTag.Name == "Olympus Makernote")
		{
			switch (tagId)
			{
			case 8208:
				PushDirectory(new TagDirectory("Olympus Equipment"));
				return true;
			case 8224:
				PushDirectory(new TagDirectory("Olympus Camera Settings"));
				return true;
			case 8240:
				PushDirectory(new TagDirectory("Olympus Raw Development"));
				return true;
			case 8241:
				PushDirectory(new TagDirectory("Olympus Raw Development 2"));
				return true;
			case 8256:
				PushDirectory(new TagDirectory("Olympus Image Processing"));
				return true;
			case 8272:
				PushDirectory(new TagDirectory("Olympus Focus Info"));
				return true;
			case 12288:
				PushDirectory(new TagDirectory("Olympus Raw Info"));
				return true;
			case 16384:
				PushDirectory(new TagDirectory("Olympus Makernote"));
				return true;
			}
		}
		return false;
	}

	private bool HandlePrintIM(TagDirectory directory, int tagId)
	{
		switch (tagId)
		{
		case 50341:
			return true;
		case 3584:
			if (directory.Name == "Casio Makernote" || directory.Name == "Kyocera/Contax Makernote" || directory.Name == "Nikon Makernote" || directory.Name == "Olympus Makernote" || directory.Name == "Panasonic Makernote" || directory.Name == "Pentax Makernote" || directory.Name == "Ricoh Makernote" || directory.Name == "Sanyo Makernote" || directory.Name == "Sony Makernote")
			{
				return true;
			}
			break;
		}
		return false;
	}

	internal void CustomProcessTag(int tagOffset, ICollection<int> processedIfdOffsets, CatalogedReaderBase reader, int tagId, int byteCount)
	{
		if (tagId == 0 || byteCount == 0 || (tagId == 37500 && m_currentTag.Name == "Exif SubIFD") || (tagId == 33723 && m_currentTag.Name == "Exif IFD0" && reader.ReadSignedByte(tagOffset) == 28) || (tagId == 34377 && m_currentTag.Name == "Exif IFD0"))
		{
			return;
		}
		if (tagId == 34675)
		{
			m_directories.Add(new TagDirectory("ICC Profile"));
		}
		else if (tagId == 700 && (m_currentTag.Name == "Exif IFD0" || m_currentTag.Name == "Exif SubIFD"))
		{
			byte[] array = reader.ReadNullTerminatedBytes(tagOffset, byteCount);
			if (m_stream == null)
			{
				m_stream = new MemoryStream();
			}
			m_stream.Write(array, 0, array.Length);
			m_directories.Add(new TagDirectory("XMP"));
		}
		else if (HandlePrintIM(m_currentTag, tagId))
		{
			m_directories.Add(new TagDirectory("PrintIM"));
		}
		else if (m_currentTag.Name == "Olympus Makernote")
		{
			switch (tagId)
			{
			case 8208:
				PushDirectory(new TagDirectory("Olympus Equipment"));
				TiffMetadataParser.ProcessImageFileDirectory(this, reader, processedIfdOffsets, tagOffset);
				break;
			case 8224:
				PushDirectory(new TagDirectory("Olympus Camera Settings"));
				TiffMetadataParser.ProcessImageFileDirectory(this, reader, processedIfdOffsets, tagOffset);
				break;
			case 8240:
				PushDirectory(new TagDirectory("Olympus Raw Development"));
				TiffMetadataParser.ProcessImageFileDirectory(this, reader, processedIfdOffsets, tagOffset);
				break;
			case 8241:
				PushDirectory(new TagDirectory("Olympus Raw Development 2"));
				TiffMetadataParser.ProcessImageFileDirectory(this, reader, processedIfdOffsets, tagOffset);
				break;
			case 8256:
				PushDirectory(new TagDirectory("Olympus Image Processing"));
				TiffMetadataParser.ProcessImageFileDirectory(this, reader, processedIfdOffsets, tagOffset);
				break;
			case 8272:
				PushDirectory(new TagDirectory("Olympus Focus Info"));
				TiffMetadataParser.ProcessImageFileDirectory(this, reader, processedIfdOffsets, tagOffset);
				break;
			case 12288:
				PushDirectory(new TagDirectory("Olympus Raw Info"));
				TiffMetadataParser.ProcessImageFileDirectory(this, reader, processedIfdOffsets, tagOffset);
				break;
			case 16384:
				PushDirectory(new TagDirectory("Olympus Makernote"));
				TiffMetadataParser.ProcessImageFileDirectory(this, reader, processedIfdOffsets, tagOffset);
				break;
			}
		}
		if (m_currentTag.Name == "PanasonicRaw Exif IFD0")
		{
			switch (tagId)
			{
			case 19:
				m_directories.Add(new TagDirectory("PanasonicRaw WbInfo"));
				break;
			case 39:
				m_directories.Add(new TagDirectory("PanasonicRaw WbInfo2"));
				break;
			case 281:
				m_directories.Add(new TagDirectory("PanasonicRaw DistortionInfo"));
				break;
			}
		}
		if (tagId == 46 && m_currentTag.Name == "PanasonicRaw Exif IFD0")
		{
			JpegDecoder jpegDecoder = new JpegDecoder(new MemoryStream(reader.GetBytes(tagOffset, byteCount)), enableMetadata: true);
			m_stream = jpegDecoder.MetadataStream;
		}
	}
}
