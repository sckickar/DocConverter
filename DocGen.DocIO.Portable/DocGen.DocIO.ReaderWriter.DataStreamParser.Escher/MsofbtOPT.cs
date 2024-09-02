using System;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Escher;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class MsofbtOPT : BaseEscherRecord
{
	public const int DEF_PIB_ID = 260;

	public const int DEF_PIBFLAGS_ID = 262;

	public const int DEF_TXID = 128;

	public const int DEF_WRAP_REXT = 133;

	internal const uint DEF_WRAP_DIST = 114300u;

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

	internal FOPTEBid Pib
	{
		get
		{
			if (m_prop.ContainsKey(260))
			{
				return m_prop[260] as FOPTEBid;
			}
			return null;
		}
	}

	internal FOPTEBid Txid
	{
		get
		{
			FOPTEBid result = null;
			if (m_prop.ContainsKey(128))
			{
				result = m_prop[128] as FOPTEBid;
			}
			return result;
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

	internal uint LayoutInTableCell
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

	internal uint DistanceFromBottom
	{
		get
		{
			uint propertyValue = GetPropertyValue(903);
			if (propertyValue == uint.MaxValue)
			{
				return 0u;
			}
			return propertyValue;
		}
		set
		{
			SetPropertyValue(903, value);
		}
	}

	internal uint DistanceFromLeft
	{
		get
		{
			uint propertyValue = GetPropertyValue(900);
			if (propertyValue == uint.MaxValue)
			{
				return 114300u;
			}
			return propertyValue;
		}
		set
		{
			SetPropertyValue(900, value);
		}
	}

	internal uint DistanceFromRight
	{
		get
		{
			uint propertyValue = GetPropertyValue(902);
			if (propertyValue == uint.MaxValue)
			{
				return 114300u;
			}
			return propertyValue;
		}
		set
		{
			SetPropertyValue(902, value);
		}
	}

	internal uint DistanceFromTop
	{
		get
		{
			uint propertyValue = GetPropertyValue(901);
			if (propertyValue == uint.MaxValue)
			{
				return 0u;
			}
			return propertyValue;
		}
		set
		{
			SetPropertyValue(901, value);
		}
	}

	internal uint Roation
	{
		get
		{
			uint propertyValue = GetPropertyValue(4);
			if (propertyValue == uint.MaxValue)
			{
				return 0u;
			}
			return propertyValue;
		}
		set
		{
			SetPropertyValue(4, value);
		}
	}

	internal MsofbtOPT(WordDocument doc)
		: base(MSOFBT.msofbtOPT, 3, doc)
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
		MsofbtOPT msofbtOPT = new MsofbtOPT(m_doc);
		FOPTEBase fOPTEBase = null;
		foreach (FOPTEBase value in m_prop.Values)
		{
			msofbtOPT.m_prop.Add(value);
		}
		msofbtOPT.m_doc = m_doc;
		return msofbtOPT;
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

	internal bool Compare(MsofbtOPT shapeOptions)
	{
		if (LayoutInTableCell != shapeOptions.LayoutInTableCell || AllowOverlap != shapeOptions.AllowOverlap || AllowInTableCell != shapeOptions.AllowInTableCell || Visible != shapeOptions.Visible || DistanceFromBottom != shapeOptions.DistanceFromBottom || DistanceFromLeft != shapeOptions.DistanceFromLeft || DistanceFromRight != shapeOptions.DistanceFromRight || DistanceFromTop != shapeOptions.DistanceFromTop || Roation != shapeOptions.Roation)
		{
			return false;
		}
		if ((m_lineProps == null && shapeOptions.m_lineProps != null) || (m_lineProps != null && shapeOptions.m_lineProps == null) || (m_wrapPolygonVetrices == null && shapeOptions.m_wrapPolygonVetrices != null) || (m_wrapPolygonVetrices != null && shapeOptions.m_wrapPolygonVetrices == null) || (Properties == null && shapeOptions.Properties != null) || (Properties != null && shapeOptions.Properties == null))
		{
			return false;
		}
		if (m_lineProps != null && shapeOptions.m_lineProps != null && !LineProperties.Compare(shapeOptions.LineProperties))
		{
			return false;
		}
		if (m_wrapPolygonVetrices != null && shapeOptions.m_wrapPolygonVetrices != null && !WrapPolygonVertices.Compare(shapeOptions.WrapPolygonVertices))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(LayoutInTableCell);
		string text = (AllowOverlap ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (AllowInTableCell ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Visible ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(DistanceFromBottom + ";");
		stringBuilder.Append(DistanceFromLeft + ";");
		stringBuilder.Append(DistanceFromRight + ";");
		stringBuilder.Append(DistanceFromTop + ";");
		stringBuilder.Append(Roation + ";");
		if (m_lineProps != null)
		{
			stringBuilder.Append(LineProperties.GetAsString());
		}
		if (m_wrapPolygonVetrices != null)
		{
			stringBuilder.Append(WrapPolygonVertices.GetAsString());
		}
		if (Properties != null)
		{
			foreach (FOPTELineStyle value in Enum.GetValues(typeof(FOPTELineStyle)))
			{
				if (Properties.ContainsKey((int)value))
				{
					FOPTEBase fOPTEBase = Properties[(int)value];
					stringBuilder.Append(fOPTEBase.Id + ";");
				}
			}
		}
		return stringBuilder;
	}
}
