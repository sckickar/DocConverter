namespace BitMiracle.LibTiff.Classic;

public class TiffCodec
{
	protected Tiff m_tif;

	protected internal Compression m_scheme;

	protected internal string m_name;

	public virtual bool CanEncode => false;

	public virtual bool CanDecode => false;

	public TiffCodec(Tiff tif, Compression scheme, string name)
	{
		m_scheme = scheme;
		m_tif = tif;
		m_name = name;
	}

	public virtual bool Init()
	{
		return true;
	}

	public virtual bool SetupDecode()
	{
		return true;
	}

	public virtual bool PreDecode(short plane)
	{
		return true;
	}

	public virtual bool DecodeRow(byte[] buffer, int offset, int count, short plane)
	{
		return noDecode("scanline");
	}

	public virtual bool DecodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		return noDecode("strip");
	}

	public virtual bool DecodeTile(byte[] buffer, int offset, int count, short plane)
	{
		return noDecode("tile");
	}

	public virtual bool SetupEncode()
	{
		return true;
	}

	public virtual bool PreEncode(short plane)
	{
		return true;
	}

	public virtual bool PostEncode()
	{
		return true;
	}

	public virtual bool EncodeRow(byte[] buffer, int offset, int count, short plane)
	{
		return noEncode("scanline");
	}

	public virtual bool EncodeStrip(byte[] buffer, int offset, int count, short plane)
	{
		return noEncode("strip");
	}

	public virtual bool EncodeTile(byte[] buffer, int offset, int count, short plane)
	{
		return noEncode("tile");
	}

	public virtual void Close()
	{
	}

	public virtual bool Seek(int row)
	{
		Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "Compression algorithm does not support random access");
		return false;
	}

	public virtual void Cleanup()
	{
	}

	public virtual int DefStripSize(int size)
	{
		if (size < 1)
		{
			int num = m_tif.ScanlineSize();
			size = 8192 / ((num == 0) ? 1 : num);
			if (size == 0)
			{
				size = 1;
			}
		}
		return size;
	}

	public virtual void DefTileSize(ref int width, ref int height)
	{
		if (width < 1)
		{
			width = 256;
		}
		if (height < 1)
		{
			height = 256;
		}
		if (((uint)width & 0xFu) != 0)
		{
			width = Tiff.roundUp(width, 16);
		}
		if (((uint)height & 0xFu) != 0)
		{
			height = Tiff.roundUp(height, 16);
		}
	}

	private bool noEncode(string method)
	{
		TiffCodec tiffCodec = m_tif.FindCodec(m_tif.m_dir.td_compression);
		if (tiffCodec != null)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "{0} {1} encoding is not implemented", tiffCodec.m_name, method);
		}
		else
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "Compression scheme {0} {1} encoding is not implemented", m_tif.m_dir.td_compression, method);
		}
		return false;
	}

	private bool noDecode(string method)
	{
		TiffCodec tiffCodec = m_tif.FindCodec(m_tif.m_dir.td_compression);
		if (tiffCodec != null)
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "{0} {1} decoding is not implemented", tiffCodec.m_name, method);
		}
		else
		{
			Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "Compression scheme {0} {1} decoding is not implemented", m_tif.m_dir.td_compression, method);
		}
		return false;
	}
}
