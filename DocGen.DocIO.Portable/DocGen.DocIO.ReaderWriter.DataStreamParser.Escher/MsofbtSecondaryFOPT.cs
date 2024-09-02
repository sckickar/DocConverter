using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtSecondaryFOPT : BaseEscherRecord
{
	private msofbtRGFOPTE m_prop;

	internal msofbtRGFOPTE Properties
	{
		get
		{
			return m_prop;
		}
		set
		{
			m_prop = value;
		}
	}

	internal MsofbtSecondaryFOPT(WordDocument doc)
		: base(MSOFBT.msofbtSecondaryFOPT, 3, doc)
	{
		m_prop = new msofbtRGFOPTE();
	}

	protected override void ReadRecordData(Stream stream)
	{
		m_prop.Clear();
		m_prop.Read(stream, base.Header.Length);
	}

	protected override void WriteRecordData(Stream stream)
	{
		base.Header.Instance = CountInstanceValue();
		m_prop.Write(stream);
	}

	internal override BaseEscherRecord Clone()
	{
		MsofbtSecondaryFOPT msofbtSecondaryFOPT = new MsofbtSecondaryFOPT(m_doc);
		FOPTEBase fOPTEBase = null;
		foreach (FOPTEBase value in m_prop.Values)
		{
			msofbtSecondaryFOPT.m_prop.Add(value);
		}
		msofbtSecondaryFOPT.m_doc = m_doc;
		return msofbtSecondaryFOPT;
	}

	internal override void Close()
	{
		base.Close();
		if (m_prop != null)
		{
			m_prop.Clear();
			m_prop = null;
		}
	}

	public uint GetPropertyValue(int key)
	{
		if (Properties.ContainsKey(key) && Properties[key] is FOPTEBid fOPTEBid)
		{
			return fOPTEBid.Value;
		}
		return uint.MaxValue;
	}

	public byte[] GetComplexPropValue(int key)
	{
		if (Properties.ContainsKey(key))
		{
			return ((FOPTEComplex)Properties[key]).Value;
		}
		return null;
	}

	private int CountInstanceValue()
	{
		int num = 0;
		foreach (FOPTEBase value in m_prop.Values)
		{
			if ((value as FOPTEBase).Id < 10000)
			{
				num++;
			}
		}
		return num;
	}
}
