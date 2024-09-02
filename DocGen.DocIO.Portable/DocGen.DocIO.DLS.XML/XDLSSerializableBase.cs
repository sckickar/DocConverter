using System;

namespace DocGen.DocIO.DLS.XML;

public abstract class XDLSSerializableBase : OwnerHolder, IXDLSSerializable
{
	private XDLSHolder m_XDLSHolder;

	XDLSHolder IXDLSSerializable.XDLSHolder
	{
		get
		{
			if (m_XDLSHolder == null)
			{
				m_XDLSHolder = new XDLSHolder();
			}
			if (m_XDLSHolder.Cleared)
			{
				m_XDLSHolder.Cleared = false;
				InitXDLSHolder();
			}
			return m_XDLSHolder;
		}
	}

	protected XDLSHolder XDLSHolder => ((IXDLSSerializable)this).XDLSHolder;

	protected XDLSSerializableBase(WordDocument doc, Entity entity)
		: base(doc, entity)
	{
	}

	void IXDLSSerializable.WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		WriteXmlAttributes(writer);
	}

	void IXDLSSerializable.WriteXmlContent(IXDLSContentWriter writer)
	{
		XDLSHolder.WriteHolder(writer);
		WriteXmlContent(writer);
	}

	void IXDLSSerializable.ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		ReadXmlAttributes(reader);
	}

	bool IXDLSSerializable.ReadXmlContent(IXDLSContentReader reader)
	{
		if (!XDLSHolder.ReadHolder(reader))
		{
			return ReadXmlContent(reader);
		}
		return true;
	}

	void IXDLSSerializable.RestoreReference(string name, int value)
	{
		RestoreReference(name, value);
	}

	internal object CloneInt()
	{
		return CloneImpl();
	}

	internal virtual void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
	}

	protected virtual object CloneImpl()
	{
		XDLSSerializableBase obj = (XDLSSerializableBase)MemberwiseClone();
		obj.m_XDLSHolder = null;
		obj.SetOwner(obj.Document, null);
		return obj;
	}

	protected virtual void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
	}

	protected virtual void WriteXmlContent(IXDLSContentWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
	}

	protected virtual void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
	}

	protected virtual bool ReadXmlContent(IXDLSContentReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return false;
	}

	protected virtual void InitXDLSHolder()
	{
	}

	protected virtual void RestoreReference(string name, int index)
	{
	}

	internal override void Close()
	{
		base.Close();
		if (m_XDLSHolder != null)
		{
			m_XDLSHolder.Close();
			m_XDLSHolder = null;
		}
	}
}
