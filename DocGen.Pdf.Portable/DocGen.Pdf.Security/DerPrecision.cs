using System;

namespace DocGen.Pdf.Security;

internal class DerPrecision : Asn1Encode
{
	private readonly DerInteger m_seconds;

	private readonly DerInteger m_milliSeconds;

	private readonly DerInteger m_microSeconds;

	private readonly int m_minMilliSeconds = 1;

	private readonly int m_maxMilliSeconds = 999;

	private readonly int m_minMicroSeconds = 1;

	private readonly int m_maxMicroSeconds = 999;

	internal DerInteger Seconds => m_seconds;

	internal DerInteger MilliSeconds => m_milliSeconds;

	internal DerInteger MicroSeconds => m_microSeconds;

	internal DerPrecision(DerInteger seconds, DerInteger milliSeconds, DerInteger microSeconds)
	{
		if (milliSeconds != null && (milliSeconds.Value.IntValue < m_minMilliSeconds || milliSeconds.Value.IntValue > m_maxMilliSeconds))
		{
			throw new ArgumentException("Specified milli seconds value is not in range");
		}
		if (microSeconds != null && (microSeconds.Value.IntValue < m_minMicroSeconds || microSeconds.Value.IntValue > m_maxMicroSeconds))
		{
			throw new ArgumentException("Specified micro seconds value is not in range");
		}
		m_seconds = seconds;
		m_milliSeconds = milliSeconds;
		m_microSeconds = microSeconds;
	}

	private DerPrecision(Asn1Sequence sequence)
	{
		for (int i = 0; i < sequence.Count; i++)
		{
			if (sequence[i] is DerInteger)
			{
				m_seconds = (DerInteger)sequence[i];
			}
			else
			{
				if (!(sequence[i] is DerTag))
				{
					continue;
				}
				DerTag derTag = (DerTag)sequence[i];
				switch (derTag.TagNumber)
				{
				case 0:
					m_milliSeconds = DerInteger.GetNumber(derTag, isExplicit: false);
					if (m_milliSeconds.Value.IntValue < m_minMilliSeconds || m_milliSeconds.Value.IntValue > m_maxMilliSeconds)
					{
						throw new ArgumentException("Specified value is not in range");
					}
					break;
				case 1:
					m_microSeconds = DerInteger.GetNumber(derTag, isExplicit: false);
					if (m_microSeconds.Value.IntValue < m_minMicroSeconds || m_microSeconds.Value.IntValue > m_maxMicroSeconds)
					{
						throw new ArgumentException("Specified value is not in range");
					}
					break;
				default:
					throw new ArgumentException("Invalid entry in sequence");
				}
			}
		}
	}

	internal static DerPrecision GetDerPrecision(object obj)
	{
		if (obj == null || obj is DerPrecision)
		{
			return (DerPrecision)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new DerPrecision((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in precision check : " + obj.GetType().FullName);
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		if (m_seconds != null)
		{
			asn1EncodeCollection.Add(m_seconds);
		}
		if (m_milliSeconds != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 0, m_milliSeconds));
		}
		if (m_microSeconds != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 1, m_microSeconds));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
