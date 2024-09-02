using System.IO;
using System.Xml;
using System.Xml.Linq;
using DocGen.DocIO.DLS.Convertors;

namespace DocGen.DocIO.DLS;

internal class Part
{
	protected Stream m_dataStream;

	protected string m_name;

	internal Stream DataStream => m_dataStream;

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

	public Part(Stream dataStream)
	{
		if (dataStream == null)
		{
			m_dataStream = new MemoryStream();
			return;
		}
		byte[] buffer = new byte[dataStream.Length];
		dataStream.Position = 0L;
		dataStream.Read(buffer, 0, (int)dataStream.Length);
		m_dataStream = new MemoryStream(buffer);
	}

	internal Part Clone()
	{
		return new Part(m_dataStream)
		{
			Name = m_name
		};
	}

	internal void SetDataStream(Stream stream)
	{
		m_dataStream.Dispose();
		m_dataStream = stream;
	}

	internal void Close()
	{
		if (m_dataStream != null)
		{
			m_dataStream.Close();
			m_dataStream = null;
		}
	}

	internal System.Xml.Linq.XAttribute GetXMLAttribute(string attributeName)
	{
		System.Xml.Linq.XAttribute result = null;
		Stream stream = UtilityMethods.CloneStream(DataStream);
		stream.Position = 0L;
		System.Xml.Linq.XDocument xDocument = new System.Xml.Linq.XDocument();
		using (XmlReader reader = XmlReader.Create(stream))
		{
			xDocument = System.Xml.Linq.XDocument.Load(reader, LoadOptions.None);
		}
		XElement root = xDocument.Root;
		if (root != null)
		{
			foreach (System.Xml.Linq.XAttribute item in root.Attributes())
			{
				if (item.Name.ToString().Contains(attributeName))
				{
					result = item;
				}
			}
		}
		return result;
	}
}
