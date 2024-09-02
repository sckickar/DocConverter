using System;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO.ReaderWriter.Escher;

[CLSCompliant(false)]
internal class WordImageReader : IWordImageReader
{
	private readonly MemoryStream m_dataStream;

	private string m_strImageName = string.Empty;

	private int m_iStartImage;

	private PICF m_picData = new PICF();

	private Image m_bitmap;

	private MsofbtSpContainer m_spContainer;

	private string m_altText;

	private string m_name;

	private byte[] m_unparsedData;

	private int m_dataStreamPosiotion;

	private ImageRecord m_imageRecord;

	internal string ImageName => m_strImageName;

	public short Width
	{
		get
		{
			return m_picData.dxaGoal;
		}
		set
		{
			m_picData.dxaGoal = value;
		}
	}

	public short Height
	{
		get
		{
			return m_picData.dyaGoal;
		}
		set
		{
			m_picData.dyaGoal = value;
		}
	}

	public Image Image => m_bitmap;

	public int WidthScale => m_picData.mx;

	public int HeightScale => m_picData.my;

	internal MsofbtSpContainer InlineShapeContainer => m_spContainer;

	internal PICF PictureDescriptor => m_picData;

	internal ImageRecord ImageRecord => m_imageRecord;

	internal string AlternativeText
	{
		get
		{
			return m_altText;
		}
		set
		{
			m_altText = value;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal byte[] UnparsedData => m_unparsedData;

	internal WordImageReader(MemoryStream dataStream, int offset, WordDocument doc)
	{
		try
		{
			if (offset > dataStream.Length)
			{
				return;
			}
			m_dataStream = dataStream;
			m_iStartImage = offset;
			m_dataStream.Position = offset;
			BinaryReader reader = new BinaryReader(dataStream);
			m_picData.Read(reader);
			m_dataStreamPosiotion = (int)m_dataStream.Position;
			m_spContainer = MsofbtSpContainer.ReadInlineImageContainers(m_picData.lcb - m_picData.cbHeader, m_dataStream, doc);
			UpdateProps();
			if (m_spContainer == null)
			{
				UpdateUnParsedData();
				return;
			}
			_Blip blipFromShapeContainer = MsofbtSpContainer.GetBlipFromShapeContainer(m_spContainer);
			if (blipFromShapeContainer != null)
			{
				m_imageRecord = blipFromShapeContainer.ImageRecord;
			}
		}
		catch
		{
			if (m_spContainer == null)
			{
				UpdateUnParsedData();
			}
		}
	}

	private void UpdateUnParsedData()
	{
		int num = m_picData.lcb - m_picData.cbHeader;
		if (num > 0 && num < m_dataStream.Length - m_dataStreamPosiotion)
		{
			m_dataStream.Position = 0L;
			byte[] array = new byte[m_dataStream.Length];
			m_dataStream.Read(array, 0, array.Length);
			m_unparsedData = new byte[num];
			for (int i = 0; i < num; i++)
			{
				m_unparsedData[i] = array[m_dataStreamPosiotion + i];
			}
		}
	}

	private void UpdateProps()
	{
		byte[] complexPropValue = m_spContainer.GetComplexPropValue(897);
		if (complexPropValue != null)
		{
			string @string = Encoding.Unicode.GetString(complexPropValue, 0, complexPropValue.Length);
			m_altText = @string.Replace("\0", string.Empty);
		}
		complexPropValue = m_spContainer.GetComplexPropValue(896);
		if (complexPropValue != null)
		{
			string string2 = Encoding.Unicode.GetString(complexPropValue, 0, complexPropValue.Length);
			m_name = string2.Replace("\0", string.Empty);
		}
	}
}
