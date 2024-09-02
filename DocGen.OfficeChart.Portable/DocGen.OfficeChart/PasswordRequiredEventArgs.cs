using System;

namespace DocGen.OfficeChart;

internal class PasswordRequiredEventArgs : EventArgs
{
	private bool m_bStopParsing;

	private string m_strNewPassword;

	public bool StopParsing
	{
		get
		{
			return m_bStopParsing;
		}
		set
		{
			m_bStopParsing = value;
		}
	}

	public string NewPassword
	{
		get
		{
			return m_strNewPassword;
		}
		set
		{
			m_strNewPassword = value;
		}
	}
}
