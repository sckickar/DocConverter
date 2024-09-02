using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class Settings
{
	private CompatibilityOptions m_compatibilityOptions;

	private CompatibilityMode m_CompatibilityMode = CompatibilityMode.Word2013;

	private WordDocument m_document;

	private byte m_bFlags;

	private string m_hashValue;

	private string m_saltValue;

	private string m_cryptProviderTypeValue = "rsaAES";

	private string m_cryptAlgorithmClassValue = "hash";

	private string m_cryptAlgorithmTypeValue = "typeAny";

	private string m_cryptAlgorithmSidValue = 14.ToString();

	private string m_cryptSpinCountValue = 100000.ToString();

	private string m_duplicateListStyleNames = string.Empty;

	private WCharacterFormat m_themeFontLanguages;

	private OfficeMathProperties m_mathProperties;

	internal CompatibilityOptions CompatibilityOptions
	{
		get
		{
			if (m_compatibilityOptions == null)
			{
				m_compatibilityOptions = new CompatibilityOptions(m_document);
			}
			return m_compatibilityOptions;
		}
	}

	public CompatibilityMode CompatibilityMode
	{
		get
		{
			return m_CompatibilityMode;
		}
		set
		{
			CompatibilityModeEnabled = true;
			m_CompatibilityMode = value;
		}
	}

	internal bool CompatibilityModeEnabled
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

	public bool MaintainFormattingOnFieldUpdate
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

	public bool UpdateResultOnFieldCodeChange
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	public bool DisableMovingEntireField
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	public bool DisplayBackgrounds
	{
		get
		{
			if (m_document != null)
			{
				return m_document.DOP.Dop2003.DispBkSpSaved;
			}
			return false;
		}
		set
		{
			if (m_document != null && value != m_document.DOP.Dop2003.DispBkSpSaved)
			{
				m_document.DOP.Dop2003.DispBkSpSaved = value;
			}
		}
	}

	internal string HashValue
	{
		get
		{
			return m_hashValue;
		}
		set
		{
			m_hashValue = value;
		}
	}

	internal string SaltValue
	{
		get
		{
			return m_saltValue;
		}
		set
		{
			m_saltValue = value;
		}
	}

	internal string CryptProviderTypeValue
	{
		get
		{
			return m_cryptProviderTypeValue;
		}
		set
		{
			m_cryptProviderTypeValue = value;
		}
	}

	internal string CryptAlgorithmSidValue
	{
		get
		{
			return m_cryptAlgorithmSidValue;
		}
		set
		{
			m_cryptAlgorithmSidValue = value;
		}
	}

	internal string CryptAlgorithmClassValue
	{
		get
		{
			return m_cryptAlgorithmClassValue;
		}
		set
		{
			m_cryptAlgorithmClassValue = value;
		}
	}

	internal string CryptAlgorithmTypeValue
	{
		get
		{
			return m_cryptAlgorithmTypeValue;
		}
		set
		{
			m_cryptAlgorithmTypeValue = value;
		}
	}

	internal string CryptSpinCountValue
	{
		get
		{
			return m_cryptSpinCountValue;
		}
		set
		{
			m_cryptSpinCountValue = value;
		}
	}

	internal bool IsOptimizedForBrowser
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

	internal string DuplicateListStyleNames
	{
		get
		{
			return m_duplicateListStyleNames;
		}
		set
		{
			m_duplicateListStyleNames = value;
		}
	}

	internal WCharacterFormat ThemeFontLanguages
	{
		get
		{
			return m_themeFontLanguages;
		}
		set
		{
			m_themeFontLanguages = value;
		}
	}

	public bool MaintainImportedListCache
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			if (!value)
			{
				DuplicateListStyleNames = string.Empty;
			}
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	public bool SkipIncrementalSaveValidation
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal OfficeMathProperties MathProperties
	{
		get
		{
			return m_mathProperties;
		}
		set
		{
			m_mathProperties = value;
		}
	}

	public bool PreserveOleImageAsImage
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal Settings(WordDocument document)
	{
		m_document = document;
		UpdateResultOnFieldCodeChange = true;
		IsOptimizedForBrowser = true;
	}

	internal void Close()
	{
		m_document = null;
		if (m_compatibilityOptions != null)
		{
			m_compatibilityOptions.Close();
			m_compatibilityOptions = null;
		}
	}

	internal void SetCompatibilityModeValue(CompatibilityMode value)
	{
		m_CompatibilityMode = value;
	}
}
