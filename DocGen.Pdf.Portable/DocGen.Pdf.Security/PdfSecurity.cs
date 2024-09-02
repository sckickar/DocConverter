using System;

namespace DocGen.Pdf.Security;

public class PdfSecurity
{
	private string m_ownerPassword;

	private string m_userPassword;

	private PdfEncryptor m_encryptor;

	internal bool m_modifiedSecurity;

	internal bool m_encryptOnlyAttachment;

	internal PdfEncryptionOptions m_encryptionOption;

	public string OwnerPassword
	{
		get
		{
			if (m_encryptOnlyAttachment)
			{
				return string.Empty;
			}
			return m_encryptor.OwnerPassword;
		}
		set
		{
			if (PdfDocument.ConformanceLevel != 0 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001)
			{
				throw new Exception("Document encryption is not allowed with" + PdfDocument.ConformanceLevel.ToString() + " Conformance documents.");
			}
			m_encryptor.OwnerPassword = value;
			m_encryptor.Encrypt = true;
			m_modifiedSecurity = true;
		}
	}

	public string UserPassword
	{
		get
		{
			return m_encryptor.UserPassword;
		}
		set
		{
			if (PdfDocument.ConformanceLevel != 0 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001)
			{
				throw new Exception("Document encryption is not allowed with" + PdfDocument.ConformanceLevel.ToString() + " Conformance documents.");
			}
			m_encryptor.UserPassword = value;
			m_encryptor.Encrypt = true;
			m_modifiedSecurity = true;
		}
	}

	public PdfPermissionsFlags Permissions
	{
		get
		{
			return m_encryptor.Permissions;
		}
		set
		{
			if (m_encryptor.Permissions != value)
			{
				m_encryptor.Permissions = value;
			}
			m_encryptor.Encrypt = true;
			m_modifiedSecurity = true;
		}
	}

	internal PdfEncryptor Encryptor
	{
		get
		{
			return m_encryptor;
		}
		set
		{
			m_encryptor = value;
		}
	}

	public PdfEncryptionKeySize KeySize
	{
		get
		{
			return m_encryptor.CryptographicAlgorithm;
		}
		set
		{
			m_encryptor.CryptographicAlgorithm = value;
			m_encryptor.Encrypt = true;
			m_modifiedSecurity = true;
		}
	}

	public PdfEncryptionAlgorithm Algorithm
	{
		get
		{
			return m_encryptor.EncryptionAlgorithm;
		}
		set
		{
			m_encryptor.EncryptionAlgorithm = value;
			m_encryptor.Encrypt = true;
			m_modifiedSecurity = true;
		}
	}

	internal bool Enabled
	{
		get
		{
			return m_encryptor.Encrypt;
		}
		set
		{
			m_encryptor.Encrypt = value;
		}
	}

	internal bool EncryptOnlyAttachment
	{
		get
		{
			return m_encryptOnlyAttachment;
		}
		set
		{
			m_encryptOnlyAttachment = value;
			m_encryptor.EncryptOnlyAttachment = value;
			m_modifiedSecurity = true;
		}
	}

	public PdfEncryptionOptions EncryptionOptions
	{
		get
		{
			return m_encryptionOption;
		}
		set
		{
			m_encryptionOption = value;
			m_encryptor.Encrypt = true;
			m_modifiedSecurity = true;
			if (PdfEncryptionOptions.EncryptOnlyAttachments == value)
			{
				EncryptOnlyAttachment = true;
				m_encryptor.EncryptMetaData = false;
			}
			else if (PdfEncryptionOptions.EncryptAllContentsExceptMetadata == value)
			{
				m_encryptor.EncryptMetaData = false;
				EncryptOnlyAttachment = false;
			}
			else
			{
				m_encryptor.EncryptMetaData = true;
				EncryptOnlyAttachment = false;
			}
		}
	}

	public PdfSecurity()
	{
		m_ownerPassword = string.Empty;
		m_userPassword = string.Empty;
		m_encryptor = new PdfEncryptor();
	}

	public PdfPermissionsFlags SetPermissions(PdfPermissionsFlags flags)
	{
		Permissions |= flags;
		return Permissions;
	}

	public PdfPermissionsFlags ResetPermissions(PdfPermissionsFlags flags)
	{
		Permissions &= ~flags;
		return Permissions;
	}
}
