using System;
using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class WordImageWriter
{
	private MemoryStream m_dataStream;

	private PICF m_picData = new PICF();

	private Metafile m_srcMetafile;

	internal MemoryStream DataStream => m_dataStream;

	internal PICF PictureData => m_picData;

	internal WordImageWriter(MemoryStream dataStream)
	{
		m_dataStream = dataStream;
	}

	internal int WriteImage(WPicture pict, int height, int width)
	{
		if (!pict.PictureShape.PictureDescriptor.BorderLeft.IsDefault)
		{
			m_picData = pict.PictureShape.PictureDescriptor.Clone();
		}
		SetPictureSize(height, width, pict);
		m_picData.cProps = 0;
		m_picData.mm = 100;
		m_picData.bm_rcWinMF = 8;
		MsofbtSpContainer msofbtSpContainer = new MsofbtSpContainer(pict.Document);
		msofbtSpContainer.CreateInlineImageContainer(pict);
		long position = m_dataStream.Position;
		m_dataStream.Position += 68L;
		msofbtSpContainer.WriteContainer(m_dataStream);
		int num = (int)m_dataStream.Position;
		m_picData.lcb = (int)(num - position);
		m_picData.cbHeader = 68;
		m_dataStream.Position = position;
		m_picData.Write(m_dataStream);
		m_dataStream.Position = num;
		return num;
	}

	private void SetPictureSize(float height, float width, WPicture pict)
	{
		int num = (int)Math.Round(pict.Height * 20f);
		int num2 = (int)Math.Round(pict.Width * 20f);
		if (width < 32767f)
		{
			m_picData.dxaGoal = (short)width;
			m_picData.mx = (ushort)Math.Round(pict.WidthScale * 10f);
		}
		else if (num2 < 32767)
		{
			m_picData.dxaGoal = (short)num2;
			m_picData.mx = 1000;
		}
		else
		{
			m_picData.dxaGoal = short.MaxValue;
			m_picData.mx = (ushort)Math.Round(pict.WidthScale * 10f);
		}
		if (height < 32767f)
		{
			m_picData.dyaGoal = (short)height;
			m_picData.my = (ushort)Math.Round(pict.HeightScale * 10f);
		}
		else if (num < 32767)
		{
			m_picData.dyaGoal = (short)num;
			m_picData.my = 1000;
		}
		else
		{
			m_picData.dyaGoal = short.MaxValue;
			m_picData.my = (ushort)Math.Round(pict.HeightScale * 10f);
		}
	}

	internal int WriteInlineShapeObject(InlineShapeObject shapeObj)
	{
		MsofbtSpContainer shapeContainer = shapeObj.ShapeContainer;
		if (shapeContainer != null)
		{
			if (shapeObj.PictureDescriptor.cbHeader == 68)
			{
				long position = m_dataStream.Position;
				m_dataStream.Position += 68L;
				shapeContainer.WriteContainer(m_dataStream);
				int num = (int)m_dataStream.Position;
				shapeObj.PictureDescriptor.lcb = (int)(num - position);
				shapeObj.PictureDescriptor.cbHeader = 68;
				m_dataStream.Position = position;
				shapeObj.PictureDescriptor.Write(m_dataStream);
				m_dataStream.Position = num;
			}
			else
			{
				shapeObj.PictureDescriptor.Write(m_dataStream);
				shapeContainer.WriteContainer(m_dataStream);
			}
		}
		else if (shapeObj.PictureDescriptor != null && shapeObj.UnparsedData != null)
		{
			long position2 = m_dataStream.Position;
			m_dataStream.Position += 68L;
			new BinaryWriter(m_dataStream).Write(shapeObj.UnparsedData);
			int num2 = (int)m_dataStream.Position;
			shapeObj.PictureDescriptor.lcb = (int)(num2 - position2);
			shapeObj.PictureDescriptor.cbHeader = 68;
			m_dataStream.Position = position2;
			shapeObj.PictureDescriptor.Write(m_dataStream);
			m_dataStream.Position = num2;
		}
		return (int)m_dataStream.Position;
	}

	internal int WriteInlineTxBxPicture(WTextBoxFormat txbxFormat)
	{
		MsofbtSpContainer msofbtSpContainer = new MsofbtSpContainer(txbxFormat.Document);
		msofbtSpContainer.CreateInlineTxbxImageCont();
		new PICF();
		int height = (int)Math.Round(txbxFormat.Height * 20f);
		int width = (int)Math.Round(txbxFormat.Width * 20f);
		m_picData.SetBasePictureOptions(height, width, 100f, 100f);
		long num = (int)m_dataStream.Position;
		m_dataStream.Position += 68L;
		msofbtSpContainer.WriteContainer(m_dataStream);
		long num2 = (int)m_dataStream.Position;
		m_picData.lcb = (int)(num2 - num);
		m_picData.cbHeader = 68;
		m_picData.mm = 100;
		m_picData.bm_rcWinMF = 2;
		m_dataStream.Position = num;
		m_picData.Write(m_dataStream);
		m_dataStream.Position = num2;
		return (int)m_dataStream.Position;
	}

	internal void Close()
	{
		if (m_dataStream != null)
		{
			m_dataStream = null;
		}
		m_picData = null;
		if (m_srcMetafile != null)
		{
			m_srcMetafile.Dispose();
			m_srcMetafile = null;
		}
	}

	private void SavePicf()
	{
		m_picData.lcb += 205;
		m_picData.cbHeader = 68;
		m_picData.Write(m_dataStream);
	}
}
