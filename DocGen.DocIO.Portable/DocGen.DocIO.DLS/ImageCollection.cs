using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Compression;
using DocGen.DocIO.DLS.Entities;

namespace DocGen.DocIO.DLS;

internal class ImageCollection
{
	internal Dictionary<int, ImageRecord> m_collection = new Dictionary<int, ImageRecord>();

	private List<int> m_removedImageIds = new List<int>();

	private int m_maxId;

	private WordDocument m_doc;

	internal ImageRecord this[int imageId]
	{
		get
		{
			if (m_collection.ContainsKey(imageId))
			{
				return m_collection[imageId];
			}
			return null;
		}
	}

	internal WordDocument Document => m_doc;

	internal ImageCollection(WordDocument doc)
	{
		m_doc = doc;
	}

	internal void Add(ImageRecord image)
	{
		if (!image.IsAdded)
		{
			int num = 1;
			if (m_removedImageIds.Count > 0)
			{
				num = m_removedImageIds[0];
				m_removedImageIds.RemoveAt(0);
			}
			else if (m_collection.Count > 0)
			{
				num = ++m_maxId;
			}
			else
			{
				m_maxId++;
			}
			image.ImageId = num;
			image.IsAdded = true;
			m_collection.Add(num, image);
		}
	}

	internal bool Remove(int imageId)
	{
		if (m_collection.ContainsKey(imageId))
		{
			m_collection[imageId].IsAdded = false;
			m_collection.Remove(imageId);
			m_removedImageIds.Add(imageId);
			return true;
		}
		return false;
	}

	internal void Clear()
	{
		if (m_collection != null)
		{
			foreach (ImageRecord value in m_collection.Values)
			{
				value.Close();
			}
			m_collection.Clear();
			m_collection = null;
		}
		if (m_removedImageIds != null)
		{
			m_removedImageIds.Clear();
			m_removedImageIds = null;
		}
		m_maxId = 0;
		m_doc = null;
	}

	internal ImageRecord LoadImage(byte[] imageBytes)
	{
		ImageRecord imageRecord = null;
		foreach (ImageRecord value in m_collection.Values)
		{
			if (!value.IsMetafile && value.m_imageBytes.Length == imageBytes.Length && WordDocument.CompareArray(value.m_imageBytes, imageBytes))
			{
				imageRecord = value;
				break;
			}
		}
		if (imageRecord == null)
		{
			imageRecord = new ImageRecord(m_doc, imageBytes);
			Add(imageRecord);
		}
		imageBytes = null;
		imageRecord.OccurenceCount++;
		return imageRecord;
	}

	internal ImageRecord LoadMetaFileImage(byte[] imageBytes, bool isCompressed)
	{
		int length = imageBytes.Length;
		if (!isCompressed)
		{
			imageBytes = CompressImageBytes(imageBytes);
		}
		ImageRecord imageRecord = null;
		foreach (ImageRecord value in m_collection.Values)
		{
			if (value.IsMetafile && value.m_imageBytes.Length == imageBytes.Length && WordDocument.CompareArray(value.m_imageBytes, imageBytes))
			{
				imageRecord = value;
				break;
			}
		}
		if (imageRecord == null)
		{
			imageRecord = new ImageRecord(m_doc, imageBytes);
			Add(imageRecord);
			if (!isCompressed)
			{
				imageRecord.Length = length;
			}
		}
		imageBytes = null;
		imageRecord.OccurenceCount++;
		imageRecord.IsMetafile = true;
		return imageRecord;
	}

	internal ImageRecord LoadXmlItemImage(byte[] imageBytes)
	{
		Image image = ImageRecord.GetImage(imageBytes);
		ImageRecord imageRecord = null;
		if (image.IsMetafile)
		{
			return LoadMetaFileImage(imageBytes, isCompressed: false);
		}
		return LoadImage(imageBytes);
	}

	internal byte[] CompressImageBytes(byte[] imageBytes)
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			new CompressedStreamWriter(memoryStream, bCloseStream: true).Write(imageBytes, 0, imageBytes.Length, bCloseAfterWrite: true);
			memoryStream.Close();
			return memoryStream.ToArray();
		}
		catch
		{
			return imageBytes;
		}
	}

	internal byte[] DecompressImageBytes(byte[] compressedImage)
	{
		byte[] result;
		try
		{
			MemoryStream memoryStream = new MemoryStream(compressedImage);
			CompressedStreamReader compressedStreamReader = new CompressedStreamReader(memoryStream);
			MemoryStream memoryStream2 = new MemoryStream();
			byte[] array = new byte[4096];
			while (true)
			{
				int num = compressedStreamReader.Read(array, 0, array.Length);
				if (num <= 0)
				{
					break;
				}
				memoryStream2.Write(array, 0, num);
			}
			memoryStream.Close();
			memoryStream = null;
			result = memoryStream2.ToArray();
			memoryStream2.Close();
			memoryStream2 = null;
		}
		catch (Exception)
		{
			try
			{
				result = compressedImage;
			}
			catch
			{
				compressedImage = CompressImageBytes(compressedImage);
				result = DecompressImageBytes(compressedImage);
			}
		}
		return result;
	}
}
