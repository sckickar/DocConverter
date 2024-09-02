using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class HeaderFooterContent
{
	private bool m_display;

	private HeaderSection m_regionCenter;

	private HeaderSection m_regionLeft;

	private HeaderSection m_regionRight;

	private OTable m_table;

	private string m_alphabeticalIndex;

	private string m_alphabeticalIndexAutoMarkFile;

	private string m_bibiliography;

	private string m_change;

	private string m_changeEnd;

	private string m_changeStart;

	private string m_DDEConnectionDecls;

	private string m_heading;

	private string m_illustrationIndex;

	private string m_list;

	private string m_objectIndex;

	private string m_Para;

	private string m_section;

	private string m_sequenceDecls;

	private int m_tableindex;

	private string m_TableOfContent;

	private string m_trackedChanges;

	private string m_userFileds;

	private string m_userIndex;

	private string m_variableDecls;

	private List<OTextBodyItem> m_bodyItems;

	internal List<OTextBodyItem> ChildItems
	{
		get
		{
			if (m_bodyItems == null)
			{
				m_bodyItems = new List<OTextBodyItem>();
			}
			return m_bodyItems;
		}
	}

	internal bool Display
	{
		get
		{
			return m_display;
		}
		set
		{
			m_display = value;
		}
	}

	internal HeaderSection RegionCenter
	{
		get
		{
			if (m_regionCenter == null)
			{
				m_regionCenter = new HeaderSection();
			}
			return m_regionCenter;
		}
		set
		{
			m_regionCenter = value;
		}
	}

	internal HeaderSection RegionLeft
	{
		get
		{
			if (m_regionLeft == null)
			{
				m_regionLeft = new HeaderSection();
			}
			return m_regionLeft;
		}
		set
		{
			m_regionLeft = value;
		}
	}

	internal HeaderSection RegionRight
	{
		get
		{
			if (m_regionRight == null)
			{
				m_regionRight = new HeaderSection();
			}
			return m_regionRight;
		}
		set
		{
			m_regionRight = value;
		}
	}

	internal OTable Table
	{
		get
		{
			return m_table;
		}
		set
		{
			m_table = value;
		}
	}

	internal string AlphabeticalIndex
	{
		get
		{
			return m_alphabeticalIndex;
		}
		set
		{
			m_alphabeticalIndex = value;
		}
	}

	internal string AlphabeticalIndexAutoMarkFile
	{
		get
		{
			return m_alphabeticalIndexAutoMarkFile;
		}
		set
		{
			m_alphabeticalIndexAutoMarkFile = value;
		}
	}

	internal string Bibiliography
	{
		get
		{
			return m_bibiliography;
		}
		set
		{
			m_bibiliography = value;
		}
	}

	internal string Change
	{
		get
		{
			return m_change;
		}
		set
		{
			m_change = value;
		}
	}

	internal string ChangeEnd
	{
		get
		{
			return m_changeEnd;
		}
		set
		{
			m_changeEnd = value;
		}
	}

	internal string ChangeStart
	{
		get
		{
			return m_changeStart;
		}
		set
		{
			m_changeStart = value;
		}
	}

	internal string DDEConnectionDecls
	{
		get
		{
			return m_DDEConnectionDecls;
		}
		set
		{
			m_DDEConnectionDecls = value;
		}
	}

	internal string Heading
	{
		get
		{
			return m_heading;
		}
		set
		{
			m_heading = value;
		}
	}

	internal string IllustrationIndex
	{
		get
		{
			return m_illustrationIndex;
		}
		set
		{
			m_illustrationIndex = value;
		}
	}

	internal string List
	{
		get
		{
			return m_list;
		}
		set
		{
			m_list = value;
		}
	}

	internal string ObjectIndex
	{
		get
		{
			return m_objectIndex;
		}
		set
		{
			m_objectIndex = value;
		}
	}

	internal string Para
	{
		get
		{
			return m_Para;
		}
		set
		{
			m_Para = value;
		}
	}

	internal string Section
	{
		get
		{
			return m_section;
		}
		set
		{
			m_section = value;
		}
	}

	internal string SequenceDecls
	{
		get
		{
			return m_sequenceDecls;
		}
		set
		{
			m_sequenceDecls = value;
		}
	}

	internal int Tableindex
	{
		get
		{
			return m_tableindex;
		}
		set
		{
			m_tableindex = value;
		}
	}

	internal string TableOfContent
	{
		get
		{
			return m_TableOfContent;
		}
		set
		{
			m_TableOfContent = value;
		}
	}

	internal string TrackedChanges
	{
		get
		{
			return m_trackedChanges;
		}
		set
		{
			m_trackedChanges = value;
		}
	}

	internal string UserFileds
	{
		get
		{
			return m_userFileds;
		}
		set
		{
			m_userFileds = value;
		}
	}

	internal string UserIndex
	{
		get
		{
			return m_userIndex;
		}
		set
		{
			m_userIndex = value;
		}
	}

	internal string VariableDecls
	{
		get
		{
			return m_variableDecls;
		}
		set
		{
			m_variableDecls = value;
		}
	}

	internal void Dispose()
	{
		if (m_regionCenter != null)
		{
			m_regionCenter.Dispose();
			m_regionCenter = null;
		}
		if (m_regionLeft != null)
		{
			m_regionLeft.Dispose();
			m_regionLeft = null;
		}
		if (m_regionRight != null)
		{
			m_regionRight.Dispose();
			m_regionRight = null;
		}
		if (m_table != null)
		{
			m_table.Dispose();
			m_table = null;
		}
	}
}
