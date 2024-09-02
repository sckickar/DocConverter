using System;

namespace DocGen.Drawing;

internal class StringFormat : IDisposable, IClone
{
	private StringFormat stringFormt;

	private static StringFormat m_genericDefaultStringFormt;

	internal static StringFormat GenericDefault
	{
		get
		{
			if (m_genericDefaultStringFormt == null)
			{
				m_genericDefaultStringFormt = new StringFormat();
				m_genericDefaultStringFormt.Alignment = StringAlignment.Near;
				m_genericDefaultStringFormt.LineAlignment = StringAlignment.Near;
				m_genericDefaultStringFormt.FormatFlags = (StringFormatFlags)0;
				m_genericDefaultStringFormt.Trimming = StringTrimming.Character;
			}
			return m_genericDefaultStringFormt;
		}
	}

	public StringAlignment Alignment { get; internal set; }

	public StringFormatFlags FormatFlags { get; internal set; }

	public StringAlignment LineAlignment { get; internal set; }

	public StringTrimming Trimming { get; internal set; }

	public StringFormat()
	{
	}

	internal StringFormat(StringFormatFlags flags)
	{
		FormatFlags = flags;
	}

	public StringFormat(StringFormat stringFormt)
	{
		this.stringFormt = stringFormt;
	}

	public void Dispose()
	{
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
