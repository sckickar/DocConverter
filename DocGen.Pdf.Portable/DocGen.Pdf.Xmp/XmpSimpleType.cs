using System;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class XmpSimpleType : XmpType
{
	public string Value
	{
		get
		{
			if (base.XmlData != null)
			{
				return base.XmlData.Value;
			}
			return string.Empty;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Value");
			}
			XmpUtils.SetTextValue(base.XmlData, value);
		}
	}

	internal XmpSimpleType(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI)
		: base(xmp, parent, prefix, localName, namespaceURI)
	{
	}

	protected internal void SetBool(bool value)
	{
		XmpUtils.SetBoolValue(base.XmlData, value);
	}

	protected internal bool GetBool()
	{
		return XmpUtils.GetBoolValue(Value);
	}

	protected internal void SetReal(float value)
	{
		XmpUtils.SetRealValue(base.XmlData, value);
	}

	protected internal float GetReal()
	{
		return XmpUtils.GetRealValue(Value);
	}

	protected internal void SetInt(int value)
	{
		XmpUtils.SetIntValue(base.XmlData, value);
	}

	protected internal int GetInt()
	{
		return XmpUtils.GetIntValue(Value);
	}

	protected internal void SetUri(Uri value)
	{
		XmpUtils.SetUriValue(base.XmlData, value);
	}

	protected internal Uri GetUri()
	{
		return XmpUtils.GetUriValue(Value);
	}

	protected internal void SetDateTime(DateTime value)
	{
		XmpUtils.SetDateTimeValue(base.XmlData, value);
	}

	protected internal DateTime GetDateTime()
	{
		return XmpUtils.GetDateTimeValue(Value);
	}

	protected override void CreateEntity()
	{
		XElement content = base.Xmp.CreateElement(base.EntityPrefix, base.EntityName, base.EntityNamespaceURI);
		base.EntityParent.Add(content);
	}
}
