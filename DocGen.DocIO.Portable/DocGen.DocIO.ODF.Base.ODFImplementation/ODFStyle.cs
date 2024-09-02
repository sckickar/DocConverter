using System;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ODFStyle : DefaultStyle, INamedObject
{
	private string m_name;

	private string m_dataStyleName;

	private uint m_defaultOutlineLevel;

	private string m_displayName;

	private uint m_listLevel;

	private string m_listStyleName;

	private string m_masterPageName;

	private string m_nextStyleName;

	private string m_parentStyleName;

	private string m_percentageDataStyleName;

	private bool m_isInlineStyle;

	private bool m_hasParent;

	internal byte styleFlags;

	internal bool isDefault;

	public new string Name
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

	internal string DataStyleName
	{
		get
		{
			return m_dataStyleName;
		}
		set
		{
			m_dataStyleName = value;
		}
	}

	internal uint DefaultOutlineLevel
	{
		get
		{
			return m_defaultOutlineLevel;
		}
		set
		{
			m_defaultOutlineLevel = value;
		}
	}

	internal string DisplayName
	{
		get
		{
			return m_displayName;
		}
		set
		{
			m_displayName = value;
		}
	}

	internal string ListStyleName
	{
		get
		{
			return m_listStyleName;
		}
		set
		{
			m_listStyleName = value;
		}
	}

	internal string MasterPageName
	{
		get
		{
			return m_masterPageName;
		}
		set
		{
			m_masterPageName = value;
		}
	}

	internal string NextStyleName
	{
		get
		{
			return m_nextStyleName;
		}
		set
		{
			m_nextStyleName = value;
		}
	}

	internal string ParentStyleName
	{
		get
		{
			return m_parentStyleName;
		}
		set
		{
			m_parentStyleName = value;
		}
	}

	internal string PercentageDataStyleName
	{
		get
		{
			return m_percentageDataStyleName;
		}
		set
		{
			m_percentageDataStyleName = value;
		}
	}

	internal uint ListLevel
	{
		get
		{
			return m_listLevel;
		}
		set
		{
			m_listLevel = value;
		}
	}

	internal bool IsInlineSTyle
	{
		get
		{
			return m_isInlineStyle;
		}
		set
		{
			m_isInlineStyle = value;
		}
	}

	internal bool HasParent
	{
		get
		{
			return m_hasParent;
		}
		set
		{
			m_hasParent = value;
		}
	}

	internal bool HasKey(int propertyKey, int flagname)
	{
		return (flagname & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	public override bool Equals(object obj)
	{
		ODFStyle oDFStyle = obj as ODFStyle;
		bool flag = false;
		if (oDFStyle == null)
		{
			return false;
		}
		if (base.ParagraphProperties != null && oDFStyle.ParagraphProperties != null && HasKey(1, StylePropFlag) && oDFStyle.HasKey(1, oDFStyle.StylePropFlag))
		{
			flag = base.ParagraphProperties.Equals(oDFStyle.ParagraphProperties);
			if (!flag)
			{
				return flag;
			}
		}
		if (base.TableCellProperties != null && oDFStyle.TableCellProperties != null && HasKey(2, StylePropFlag) && oDFStyle.HasKey(2, oDFStyle.StylePropFlag))
		{
			flag = base.TableCellProperties.Equals(oDFStyle.TableCellProperties);
			if (!flag)
			{
				return flag;
			}
		}
		flag = base.Textproperties != null && oDFStyle.Textproperties != null && HasKey(6, StylePropFlag) && oDFStyle.HasKey(6, oDFStyle.StylePropFlag);
		if (flag)
		{
			base.Textproperties.Equals(oDFStyle.Textproperties);
			if (!flag)
			{
				return flag;
			}
		}
		flag = base.TableColumnProperties != null && oDFStyle.TableColumnProperties != null && HasKey(3, StylePropFlag) && oDFStyle.HasKey(3, oDFStyle.StylePropFlag);
		if (flag)
		{
			base.TableColumnProperties.Equals(oDFStyle.TableColumnProperties);
			if (!flag)
			{
				return flag;
			}
		}
		flag = base.TableRowProperties != null && oDFStyle.TableRowProperties != null && HasKey(5, StylePropFlag) && oDFStyle.HasKey(5, oDFStyle.StylePropFlag);
		if (flag)
		{
			return base.TableRowProperties.Equals(oDFStyle.TableRowProperties);
		}
		return flag;
	}

	internal void Close()
	{
		Dispose();
	}
}
