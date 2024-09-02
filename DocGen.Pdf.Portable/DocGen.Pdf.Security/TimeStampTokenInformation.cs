using System;

namespace DocGen.Pdf.Security;

internal class TimeStampTokenInformation
{
	private TimeStampData m_timeStampData;

	private DateTime m_generalizedTime;

	internal DateTime GeneralizedTime => m_generalizedTime;

	internal string Policy => m_timeStampData.Policy.ID;

	internal string MessageImprintAlgOid => m_timeStampData.MessageImprint.HashAlgorithm;

	internal TimeStampData TimeStampData => m_timeStampData;

	internal TimeStampTokenInformation(TimeStampData timeStampData)
	{
		m_timeStampData = timeStampData;
		try
		{
			m_generalizedTime = timeStampData.GeneralizedTime.ToDateTime();
		}
		catch (Exception)
		{
			throw new Exception("Invalid entry");
		}
	}
}
