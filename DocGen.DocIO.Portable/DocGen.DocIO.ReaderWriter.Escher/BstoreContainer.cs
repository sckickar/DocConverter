using System.IO;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class BstoreContainer : BaseWordRecord
{
	private FBSE m_fbse;

	private BitmapBLIP m_bitmapBlip;

	private Image m_bitmap;

	private Blip m_blip;

	internal FBSE Fbse
	{
		get
		{
			return m_fbse;
		}
		set
		{
			m_fbse = value;
		}
	}

	public Image Bitmap => m_bitmap;

	public BstoreContainer()
	{
		m_fbse = new FBSE();
		m_bitmapBlip = new BitmapBLIP();
	}

	public void Read(Stream stream)
	{
		m_fbse.Read(stream);
		MSOFBH mSOFBH = new MSOFBH();
		mSOFBH.Read(stream);
		bool chr = false;
		switch ((MSOBlipType)mSOFBH.Msofbt)
		{
		case (MSOBlipType)61470:
			if ((mSOFBH.Inst ^ 0x6E0) == 1)
			{
				chr = true;
			}
			m_blip = new BitmapBLIP();
			break;
		case (MSOBlipType)61469:
			if ((mSOFBH.Inst ^ 0x46A) == 1)
			{
				chr = true;
			}
			m_blip = new BitmapBLIP();
			break;
		case (MSOBlipType)61471:
			if ((mSOFBH.Inst ^ 0x7A8) == 1)
			{
				chr = true;
			}
			m_blip = new BitmapBLIP();
			break;
		case (MSOBlipType)61467:
			if ((mSOFBH.Inst ^ 0x216) == 1)
			{
				chr = true;
			}
			m_blip = new MetafileBlip();
			break;
		case (MSOBlipType)61466:
			if ((mSOFBH.Inst ^ 0x3D4) == 1)
			{
				chr = true;
			}
			m_blip = new MetafileBlip();
			break;
		case (MSOBlipType)61468:
			if ((mSOFBH.Inst ^ 0x542) == 1)
			{
				chr = true;
			}
			m_blip = new MetafileBlip();
			break;
		}
		m_bitmap = m_blip.Read(stream, (int)mSOFBH.Length, chr);
	}

	internal void Write(Stream stream, MemoryStream imageStream, byte[] id, Image image)
	{
		if (image.IsMetafile)
		{
			m_blip = new MetafileBlip();
			(m_blip as MetafileBlip).Metafile = image as Metafile;
			m_blip.Write(stream, imageStream, MSOBlipType.msoblipEMF, id);
		}
		else
		{
			m_blip = new BitmapBLIP();
			m_blip.Write(stream, imageStream, MSOBlipType.msoblipPNG, id);
		}
	}

	internal override void Close()
	{
		if (m_bitmap != null)
		{
			m_bitmap.Dispose();
			m_bitmap = null;
		}
		if (m_blip != null)
		{
			m_blip.Close();
		}
	}
}
