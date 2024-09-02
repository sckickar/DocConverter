using System.Text;

namespace DocGen.DocIO.DLS;

public class CheckBoxState
{
	private string m_Font;

	private string m_Value;

	private ContentControlProperties m_contentControlProperties;

	internal ContentControlProperties ContentControlProperties
	{
		get
		{
			return m_contentControlProperties;
		}
		set
		{
			m_contentControlProperties = value;
		}
	}

	public string Font
	{
		get
		{
			return m_Font;
		}
		set
		{
			m_Font = value;
			if (ContentControlProperties != null && !ContentControlProperties.Document.IsOpening && !ContentControlProperties.Document.IsCloning)
			{
				ContentControlProperties.ChangeCheckboxState(ContentControlProperties.IsChecked);
			}
		}
	}

	public string Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
			if (ContentControlProperties != null && !ContentControlProperties.Document.IsOpening && !ContentControlProperties.Document.IsCloning)
			{
				ContentControlProperties.ChangeCheckboxState(ContentControlProperties.IsChecked);
			}
		}
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Font + ";");
		stringBuilder.Append(Value + ";");
		return stringBuilder;
	}

	internal bool Compare(CheckBoxState checkBoxState)
	{
		if (Font != checkBoxState.Font || Value != checkBoxState.Value)
		{
			return false;
		}
		return true;
	}
}
