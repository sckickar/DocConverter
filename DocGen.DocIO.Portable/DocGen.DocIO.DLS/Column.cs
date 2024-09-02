using System;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class Column : FormatBase
{
	internal const int WidthKey = 1;

	internal const int SpaceKey = 2;

	public float Width
	{
		get
		{
			return (float)GetPropertyValue(1);
		}
		set
		{
			SetPropertyValue(1, value);
		}
	}

	public float Space
	{
		get
		{
			return (float)GetPropertyValue(2);
		}
		set
		{
			SetPropertyValue(2, value);
		}
	}

	public Column(IWordDocument doc)
		: base((WordDocument)doc, null)
	{
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("Width", Width);
		writer.WriteValue("Spacing", Space);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		Width = reader.ReadFloat("Width");
		Space = reader.ReadFloat("Spacing");
	}

	internal Column Clone()
	{
		return (Column)base.CloneImpl();
	}

	protected override object GetDefValue(int key)
	{
		if ((uint)(key - 1) <= 1u)
		{
			return 0f;
		}
		throw new ArgumentException("key has invalid value");
	}

	internal bool Compare(Column column)
	{
		if (!Compare(1, column))
		{
			return false;
		}
		if (!Compare(2, column))
		{
			return false;
		}
		return true;
	}

	internal object GetPropertyValue(int propKey)
	{
		return base[propKey];
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		base[propKey] = value;
		OnStateChange(this);
	}
}
