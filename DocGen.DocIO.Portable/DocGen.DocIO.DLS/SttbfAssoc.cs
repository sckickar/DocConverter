using System;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.DLS;

internal class SttbfAssoc
{
	private ushort m_fExtend = ushort.MaxValue;

	private ushort m_cData = 18;

	private ushort m_cbExtra;

	private string m_template;

	private string m_title;

	private string m_subject;

	private string m_keyWords;

	private string m_author;

	private string m_lastModifiedBy;

	private string m_dataSource;

	private string m_headerDocument;

	private string m_writePassword;

	internal string AttachedTemplate
	{
		get
		{
			return m_template;
		}
		set
		{
			m_template = value;
		}
	}

	internal string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	internal string Subject
	{
		get
		{
			return m_subject;
		}
		set
		{
			m_subject = value;
		}
	}

	internal string KeyWords
	{
		get
		{
			return m_keyWords;
		}
		set
		{
			m_keyWords = value;
		}
	}

	internal string Author
	{
		get
		{
			return m_author;
		}
		set
		{
			m_author = value;
		}
	}

	internal string LastModifiedBy
	{
		get
		{
			return m_lastModifiedBy;
		}
		set
		{
			m_lastModifiedBy = value;
		}
	}

	internal string MailMergeDataSource
	{
		get
		{
			return m_dataSource;
		}
		set
		{
			m_dataSource = value;
		}
	}

	internal string MailMergeHeaderDocument
	{
		get
		{
			return m_headerDocument;
		}
		set
		{
			m_headerDocument = value;
		}
	}

	internal string WritePassword
	{
		get
		{
			return m_writePassword;
		}
		set
		{
			m_writePassword = value;
		}
	}

	internal SttbfAssoc()
	{
	}

	internal void Parse(byte[] associatedStrings)
	{
		if (associatedStrings.Length >= 42)
		{
			int num = 0;
			m_fExtend = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_cData = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_cbExtra = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			ushort num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_template = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_title = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_subject = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_keyWords = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_author = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_lastModifiedBy = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_dataSource = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			m_headerDocument = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			num += num2 * 2;
			num2 = BaseWordRecord.ReadUInt16(associatedStrings, num);
			num += 2;
			if (num2 > 0)
			{
				m_writePassword = BaseWordRecord.ReadString(associatedStrings, num, (ushort)(num2 * 2));
			}
			num += num2 * 2;
		}
	}

	internal byte[] GetAssociatedStrings()
	{
		MemoryStream memoryStream = new MemoryStream();
		BaseWordRecord.WriteUInt16(memoryStream, m_fExtend);
		BaseWordRecord.WriteUInt16(memoryStream, m_cData);
		BaseWordRecord.WriteUInt16(memoryStream, m_cbExtra);
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		if (string.IsNullOrEmpty(m_template))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			BaseWordRecord.WriteString(memoryStream, m_template);
		}
		if (string.IsNullOrEmpty(m_title))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			BaseWordRecord.WriteString(memoryStream, m_title);
		}
		if (string.IsNullOrEmpty(m_subject))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			BaseWordRecord.WriteString(memoryStream, m_subject);
		}
		if (string.IsNullOrEmpty(m_keyWords))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			BaseWordRecord.WriteString(memoryStream, m_keyWords);
		}
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		if (string.IsNullOrEmpty(m_author))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			BaseWordRecord.WriteString(memoryStream, m_author);
		}
		if (string.IsNullOrEmpty(m_lastModifiedBy))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			BaseWordRecord.WriteString(memoryStream, m_lastModifiedBy);
		}
		if (string.IsNullOrEmpty(m_dataSource))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			BaseWordRecord.WriteString(memoryStream, m_dataSource);
		}
		if (string.IsNullOrEmpty(m_headerDocument))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			BaseWordRecord.WriteString(memoryStream, m_headerDocument);
		}
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		BaseWordRecord.WriteUInt16(memoryStream, 0);
		if (string.IsNullOrEmpty(m_writePassword))
		{
			BaseWordRecord.WriteUInt16(memoryStream, 0);
		}
		else
		{
			if (m_writePassword.Length > 15)
			{
				m_writePassword = m_writePassword.Substring(0, 15);
			}
			BaseWordRecord.WriteString(memoryStream, m_writePassword);
		}
		byte[] array = new byte[memoryStream.Length];
		Buffer.BlockCopy(memoryStream.ToArray(), 0, array, 0, (int)memoryStream.Length);
		return array;
	}
}
