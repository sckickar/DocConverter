using DocGen.DocIO;
using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal class FootnoteLayoutInfo : LayoutInfo
{
	private string m_footnoteID;

	private WTextRange m_textRange;

	private float m_height;

	private float m_endnoteheight;

	internal float FootnoteHeight
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	internal float Endnoteheight
	{
		get
		{
			return m_endnoteheight;
		}
		set
		{
			m_endnoteheight = value;
		}
	}

	internal string FootnoteID
	{
		get
		{
			return m_footnoteID;
		}
		set
		{
			m_footnoteID = value;
		}
	}

	internal WTextRange TextRange
	{
		get
		{
			return m_textRange;
		}
		set
		{
			m_textRange = value;
		}
	}

	internal FootnoteLayoutInfo(ChildrenLayoutDirection childLayoutDirection)
		: base(childLayoutDirection)
	{
	}

	internal string GetFootnoteID(WFootnote footnote, int id)
	{
		WSection wSection = GetBaseEntity(footnote) as WSection;
		string result = id.ToString();
		if (wSection != null)
		{
			FootEndNoteNumberFormat footEndNoteNumberFormat = wSection.PageSetup.FootnoteNumberFormat;
			if (footnote.FootnoteType == FootnoteType.Endnote)
			{
				footEndNoteNumberFormat = wSection.PageSetup.EndnoteNumberFormat;
			}
			result = wSection.PageSetup.GetNumberFormatValue((byte)footEndNoteNumberFormat, id);
			if (footnote.CustomMarkerIsSymbol)
			{
				result = char.ConvertFromUtf32(footnote.SymbolCode);
			}
			else if (footnote.m_strCustomMarker != string.Empty)
			{
				result = footnote.m_strCustomMarker;
			}
		}
		return result;
	}

	internal Entity GetBaseEntity(Entity entity)
	{
		Entity entity2 = entity;
		while (!(entity2 is WSection) && entity2.Owner != null)
		{
			entity2 = entity2.Owner;
		}
		return entity2;
	}
}
