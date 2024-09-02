namespace DocGen.DocIO.DLS.Convertors;

internal class FormFieldData
{
	private string m_name;

	private string m_helpText;

	private string m_statusHelpText;

	private FormFieldType m_formFieldType;

	private bool m_bCalculateOnExit;

	private string m_marcoOnStart;

	private string m_marcoOnEnd;

	private bool m_enabled = true;

	private int m_checkboxSize;

	private CheckBoxSizeType m_checkboxSizeType;

	private string m_defaultText;

	private string m_stringFormat;

	private int m_maxLength;

	internal WDropDownCollection DropDownItems;

	private bool m_bIsListBox;

	internal int Ffres;

	internal int Ffdefres;

	internal bool m_bIsChecked;

	internal bool IsChecked
	{
		get
		{
			return m_bIsChecked;
		}
		set
		{
			m_bIsChecked = value;
		}
	}

	internal bool IsListBox
	{
		get
		{
			return m_bIsListBox;
		}
		set
		{
			m_bIsListBox = value;
		}
	}

	internal int MaxLength
	{
		get
		{
			return m_maxLength;
		}
		set
		{
			m_maxLength = value;
		}
	}

	internal string StringFormat
	{
		get
		{
			return m_stringFormat;
		}
		set
		{
			m_stringFormat = value;
		}
	}

	internal string DefaultText
	{
		get
		{
			return m_defaultText;
		}
		set
		{
			m_defaultText = value;
		}
	}

	internal CheckBoxSizeType CheckboxSizeType
	{
		get
		{
			return m_checkboxSizeType;
		}
		set
		{
			m_checkboxSizeType = value;
		}
	}

	internal int CheckboxSize
	{
		get
		{
			return m_checkboxSize;
		}
		set
		{
			m_checkboxSize = value;
		}
	}

	internal bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
		}
	}

	internal string MacroOnExit
	{
		get
		{
			return m_marcoOnEnd;
		}
		set
		{
			m_marcoOnEnd = value;
		}
	}

	internal string MarcoOnStart
	{
		get
		{
			return m_marcoOnStart;
		}
		set
		{
			m_marcoOnStart = value;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal string HelpText
	{
		get
		{
			return m_helpText;
		}
		set
		{
			m_helpText = value;
		}
	}

	internal bool CalculateOnExit
	{
		get
		{
			return m_bCalculateOnExit;
		}
		set
		{
			m_bCalculateOnExit = value;
		}
	}

	internal string StatusHelpText
	{
		get
		{
			return m_statusHelpText;
		}
		set
		{
			m_statusHelpText = value;
		}
	}

	internal FormFieldType FormFieldType
	{
		get
		{
			return m_formFieldType;
		}
		set
		{
			m_formFieldType = value;
		}
	}
}
