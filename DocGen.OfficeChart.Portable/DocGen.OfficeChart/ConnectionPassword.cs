using System;

namespace DocGen.OfficeChart;

internal class ConnectionPassword : EventArgs
{
	private string m_connectionPassword;

	public string PasswordToConnectDB
	{
		get
		{
			return m_connectionPassword;
		}
		set
		{
			m_connectionPassword = value;
		}
	}
}
