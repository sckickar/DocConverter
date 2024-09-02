namespace DocGen.Pdf.Security;

internal class OneTimeResponse : X509ExtensionBase
{
	private OneTimeResponseHelper m_helper;

	internal GeneralizedTime NextUpdate => m_helper.NextUpdate;

	internal GeneralizedTime CurrentUpdate => m_helper.CurrentUpdate;

	internal CertificateIdentityHelper CertificateID => m_helper.CertificateIdentification;

	internal object CertificateStatus
	{
		get
		{
			OcspStatus status = m_helper.Status;
			if (status.TagNumber == 0)
			{
				return null;
			}
			return status;
		}
	}

	internal OneTimeResponse(OneTimeResponseHelper helper)
	{
		m_helper = helper;
	}

	protected override X509Extensions GetX509Extensions()
	{
		return null;
	}
}
