using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtTertiaryFOPT : BaseEscherRecord
{
	private const int DEF_UNKNOWN2_PID = 1343;

	private const uint DEF_NOTALLOWINCELL = 2147483648u;

	private msofbtRGFOPTE m_prop;

	private LineStyleBooleanProperties m_lineProps;

	private WrapPolygonVertices m_wrapPolygonVetrices;

	internal LineStyleBooleanProperties LineProperties
	{
		get
		{
			if (m_lineProps == null)
			{
				m_lineProps = new LineStyleBooleanProperties(m_prop, 511);
			}
			return m_lineProps;
		}
	}

	internal WrapPolygonVertices WrapPolygonVertices
	{
		get
		{
			if (m_wrapPolygonVetrices == null)
			{
				m_wrapPolygonVetrices = new WrapPolygonVertices(m_prop, 899);
			}
			return m_wrapPolygonVetrices;
		}
	}

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

	public uint XAlign
	{
		get
		{
			return GetPropertyValue(911);
		}
		set
		{
			SetPropertyValue(911, value);
		}
	}

	public uint XRelTo
	{
		get
		{
			return GetPropertyValue(912);
		}
		set
		{
			SetPropertyValue(912, value);
		}
	}

	public uint YAlign
	{
		get
		{
			return GetPropertyValue(913);
		}
		set
		{
			SetPropertyValue(913, value);
		}
	}

	public uint YRelTo
	{
		get
		{
			return GetPropertyValue(914);
		}
		set
		{
			SetPropertyValue(914, value);
		}
	}

	public uint LayoutInTableCell
	{
		get
		{
			return GetPropertyValue(959);
		}
		set
		{
			SetPropertyValue(959, value);
		}
	}

	public uint Unknown1
	{
		get
		{
			return GetPropertyValue(447);
		}
		set
		{
			SetPropertyValue(447, value);
		}
	}

	public uint Unknown2
	{
		get
		{
			return GetPropertyValue(1343);
		}
		set
		{
			SetPropertyValue(1343, value);
		}
	}

	internal bool AllowInTableCell
	{
		get
		{
			if (LayoutInTableCell != uint.MaxValue && (LayoutInTableCell & 0x80000000u) >> 31 != 0)
			{
				return (LayoutInTableCell & 0x8000) >> 15 != 0;
			}
			return true;
		}
		set
		{
			if (LayoutInTableCell == uint.MaxValue)
			{
				LayoutInTableCell = 2147483648u;
			}
			LayoutInTableCell = (uint)((LayoutInTableCell & 0x7FFFFFFFu) | int.MinValue);
			LayoutInTableCell = (uint)((LayoutInTableCell & 0xFFFF7FFFu) | (int)((value ? 1u : 0u) << 15));
		}
	}

	internal bool AllowOverlap
	{
		get
		{
			if (LayoutInTableCell != uint.MaxValue && (LayoutInTableCell & 0x2000000) >> 25 != 0)
			{
				return (LayoutInTableCell & 0x200) >> 9 != 0;
			}
			return true;
		}
		set
		{
			if (LayoutInTableCell == uint.MaxValue)
			{
				LayoutInTableCell = 33554432u;
			}
			LayoutInTableCell = (LayoutInTableCell & 0xFDFFFFFFu) | 0x2000000u;
			LayoutInTableCell = (uint)((LayoutInTableCell & 0xFFFFFDFFu) | (int)((value ? 1u : 0u) << 9));
		}
	}

	internal bool Visible
	{
		get
		{
			if (LayoutInTableCell != uint.MaxValue && (LayoutInTableCell & 0x20000) >> 17 != 0)
			{
				return (LayoutInTableCell & 2) >> 1 == 0;
			}
			return true;
		}
		set
		{
			if (LayoutInTableCell == uint.MaxValue)
			{
				LayoutInTableCell = 131072u;
			}
			LayoutInTableCell = (uint)((LayoutInTableCell & 0xFFFDFFFFu) | (int)((!value) ? 131072u : 0u));
			LayoutInTableCell = (uint)((LayoutInTableCell & 0xFFFFFFFDu) | (int)((!value) ? 2u : 0u));
		}
	}

	internal MsofbtTertiaryFOPT(WordDocument doc)
		: base(MSOFBT.msofbtTertiaryFOPT, 3, doc)
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
		MsofbtTertiaryFOPT msofbtTertiaryFOPT = new MsofbtTertiaryFOPT(m_doc);
		FOPTEBase fOPTEBase = null;
		foreach (FOPTEBase value in m_prop.Values)
		{
			msofbtTertiaryFOPT.m_prop.Add(value);
		}
		msofbtTertiaryFOPT.m_doc = m_doc;
		return msofbtTertiaryFOPT;
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

	internal uint GetPropertyValue(int key)
	{
		if (m_prop.ContainsKey(key) && m_prop[key] is FOPTEBid fOPTEBid)
		{
			return fOPTEBid.Value;
		}
		return uint.MaxValue;
	}

	internal void SetPropertyValue(int key, uint value)
	{
		if (m_prop.ContainsKey(key))
		{
			(m_prop[key] as FOPTEBid).Value = value;
		}
		else
		{
			m_prop.Add(key, new FOPTEBid(key, isBid: false, value));
		}
	}

	internal byte[] GetComplexPropValue(int key)
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

	internal bool Compare(MsofbtTertiaryFOPT shapePosition)
	{
		if (XAlign != shapePosition.XAlign || XRelTo != shapePosition.XRelTo || YAlign != shapePosition.YAlign || YRelTo != shapePosition.YRelTo || LayoutInTableCell != shapePosition.LayoutInTableCell || Unknown1 != shapePosition.Unknown1 || Unknown2 != shapePosition.Unknown2 || AllowInTableCell != shapePosition.AllowInTableCell || AllowOverlap != shapePosition.AllowOverlap || Visible != shapePosition.Visible)
		{
			return false;
		}
		if ((m_lineProps != null && shapePosition.m_lineProps == null) || (m_lineProps == null && shapePosition.m_lineProps != null) || (m_wrapPolygonVetrices != null && shapePosition.m_wrapPolygonVetrices == null) || (m_wrapPolygonVetrices == null && shapePosition.m_wrapPolygonVetrices != null) || (Properties != null && shapePosition.Properties == null) || (Properties == null && shapePosition.Properties != null))
		{
			return false;
		}
		if (m_lineProps != null && shapePosition.m_lineProps != null && !LineProperties.Compare(shapePosition.LineProperties))
		{
			return false;
		}
		if (m_wrapPolygonVetrices != null && shapePosition.m_wrapPolygonVetrices != null && !WrapPolygonVertices.Compare(shapePosition.WrapPolygonVertices))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(XAlign + ";");
		stringBuilder.Append(XRelTo + ";");
		stringBuilder.Append(YAlign + ";");
		stringBuilder.Append(YRelTo + ";");
		stringBuilder.Append(LayoutInTableCell + ";");
		stringBuilder.Append(Unknown1 + ";");
		stringBuilder.Append(Unknown2 + ";");
		string text = (AllowInTableCell ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (AllowOverlap ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Visible ? "1" : "0");
		stringBuilder.Append(text + ";");
		if (m_lineProps != null)
		{
			stringBuilder.Append(LineProperties.GetAsString());
		}
		if (m_wrapPolygonVetrices != null)
		{
			stringBuilder.Append(WrapPolygonVertices.GetAsString());
		}
		return stringBuilder;
	}
}
