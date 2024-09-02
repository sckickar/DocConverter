using System.IO;
using System.Text;

namespace DocGen.Pdf.Graphics;

internal class GifMetadataParser : IImageMetadataParser
{
	private MemoryStream m_stream;

	private BinaryReader m_reader;

	internal MemoryStream Stream
	{
		get
		{
			if (m_stream == null)
			{
				m_stream = new MemoryStream();
			}
			return m_stream;
		}
	}

	internal GifMetadataParser(Stream stream)
	{
		m_reader = new BinaryReader(stream);
	}

	public MemoryStream GetMetadata()
	{
		if (Encoding.UTF8.GetString(m_reader.ReadBytes(3), 0, 3) == "GIF")
		{
			string @string = Encoding.UTF8.GetString(m_reader.ReadBytes(3), 0, 3);
			if (@string == "87a" || @string == "89a")
			{
				m_reader.ReadBytes(4);
				byte b = m_reader.ReadByte();
				int num = 1 << (b & 7) + 1;
				bool num2 = b >> 7 != 0;
				m_reader.ReadBytes(2);
				if (num2)
				{
					m_reader.ReadBytes(3 * num);
				}
				bool flag = true;
				while (flag)
				{
					byte b2;
					try
					{
						b2 = m_reader.ReadByte();
					}
					catch
					{
						break;
					}
					switch (b2)
					{
					case 33:
					{
						byte b4 = m_reader.ReadByte();
						byte b5 = m_reader.ReadByte();
						long position = m_reader.BaseStream.Position;
						switch (b4)
						{
						case 1:
							if (b5 == 12)
							{
								m_reader.ReadBytes(12);
								SkipBlocks();
							}
							break;
						case 249:
							if (b5 < 4)
							{
								b5 = 4;
							}
							m_reader.ReadBytes(4);
							break;
						case 254:
						{
							for (byte b6 = b5; b6 > 0; b6 = m_reader.ReadByte())
							{
								m_reader.ReadBytes(b6);
							}
							break;
						}
						case byte.MaxValue:
							ReadApplicationExtensionBlock(b5);
							break;
						}
						long num3 = position + b5 - m_reader.BaseStream.Position;
						if (num3 > 0)
						{
							m_reader.ReadBytes((int)num3);
						}
						break;
					}
					case 44:
					{
						m_reader.ReadBytes(8);
						byte b3 = m_reader.ReadByte();
						if (b3 >> 7 != 0)
						{
							m_reader.ReadBytes(3 * (2 << (b3 & 7)));
						}
						m_reader.ReadByte();
						SkipBlocks();
						break;
					}
					default:
						flag = false;
						break;
					}
				}
			}
		}
		return m_stream;
	}

	private void SkipBlocks()
	{
		while (true)
		{
			byte b = m_reader.ReadByte();
			if (b == 0)
			{
				break;
			}
			m_reader.ReadBytes(b);
		}
	}

	private void ReadApplicationExtensionBlock(byte length)
	{
		if (length != 11)
		{
			return;
		}
		switch (Encoding.UTF8.GetString(m_reader.ReadBytes(length), 0, length))
		{
		case "XMP DataXMP":
		{
			MemoryStream memoryStream = new MemoryStream();
			byte[] array = new byte[257];
			while (true)
			{
				byte b2 = m_reader.ReadByte();
				if (b2 == 0)
				{
					break;
				}
				array[0] = b2;
				m_reader.BaseStream.Read(array, 1, b2);
				memoryStream.Write(array, 0, b2 + 1);
			}
			byte[] array2 = memoryStream.ToArray();
			if (array2 != null)
			{
				Stream.Write(array2, 0, array2.Length - 257);
			}
			break;
		}
		case "ICCRGBG1012":
		{
			for (byte b = m_reader.ReadByte(); b > 0; b = m_reader.ReadByte())
			{
				m_reader.ReadBytes(b);
			}
			break;
		}
		case "NETSCAPE2.0":
			m_reader.ReadBytes(5);
			break;
		default:
			SkipBlocks();
			break;
		}
	}
}
