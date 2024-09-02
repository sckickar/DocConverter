using System.Text;

namespace DocGen.DocIO.DLS;

internal class LinearGradient
{
	private short m_angle;

	private byte m_bFlags = 2;

	internal bool AnglePositive
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal short Angle
	{
		get
		{
			return m_angle;
		}
		set
		{
			m_angle = value;
		}
	}

	internal bool IsAngleDefined
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool Scaled
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal LinearGradient()
	{
	}

	internal LinearGradient Clone()
	{
		return (LinearGradient)MemberwiseClone();
	}

	internal bool Compare(LinearGradient linearGradient)
	{
		if (AnglePositive != linearGradient.AnglePositive || IsAngleDefined != linearGradient.IsAngleDefined || Scaled != linearGradient.Scaled || Angle != linearGradient.Angle)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (AnglePositive ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsAngleDefined ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Scaled ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(Angle + ";");
		return stringBuilder;
	}
}
