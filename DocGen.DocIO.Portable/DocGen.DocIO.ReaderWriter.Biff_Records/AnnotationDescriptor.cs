using System.IO;
using System.Text;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class AnnotationDescriptor
{
	internal const int DEF_LENGTH = 30;

	private string m_xstUsrInitl;

	private short m_ibst;

	private short m_ak;

	private short m_grfbmc;

	private int m_lTagBkmk;

	internal short IndexToGrpOwner
	{
		get
		{
			return m_ibst;
		}
		set
		{
			m_ibst = value;
		}
	}

	internal string UserInitials
	{
		get
		{
			return m_xstUsrInitl;
		}
		set
		{
			m_xstUsrInitl = value;
		}
	}

	internal short Ak
	{
		get
		{
			return m_ak;
		}
		set
		{
			m_ak = value;
		}
	}

	internal short Grfbmc
	{
		get
		{
			return m_grfbmc;
		}
		set
		{
			m_grfbmc = value;
		}
	}

	internal int TagBkmk
	{
		get
		{
			return m_lTagBkmk;
		}
		set
		{
			m_lTagBkmk = value;
		}
	}

	internal AnnotationDescriptor(BinaryReader reader)
	{
		Read(reader);
	}

	internal AnnotationDescriptor()
	{
		TagBkmk = -1;
	}

	internal void Read(BinaryReader reader)
	{
		byte[] array = reader.ReadBytes(20);
		m_xstUsrInitl = Encoding.Unicode.GetString(array, 0, array.Length).Substring(1, array[0]);
		m_ibst = reader.ReadInt16();
		m_ak = reader.ReadInt16();
		m_grfbmc = reader.ReadInt16();
		m_lTagBkmk = reader.ReadInt32();
	}

	internal void Write(BinaryWriter writer)
	{
		string empty = string.Empty;
		empty = ((m_xstUsrInitl.Length <= 9) ? ((char)m_xstUsrInitl.Length + m_xstUsrInitl) : ("9" + m_xstUsrInitl.Substring(0, 9)));
		if (empty.Length < 10)
		{
			int i = 0;
			for (int num = 10 - empty.Length; i < num; i++)
			{
				empty += "\0";
			}
		}
		byte[] bytes = Encoding.Unicode.GetBytes(empty);
		writer.Write(bytes, 0, bytes.Length);
		writer.Write(m_ibst);
		writer.Write(m_ak);
		writer.Write(m_grfbmc);
		writer.Write(m_lTagBkmk);
	}
}
