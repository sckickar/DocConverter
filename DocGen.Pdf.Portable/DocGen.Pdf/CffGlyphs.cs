using System.Collections.Generic;

namespace DocGen.Pdf;

internal class CffGlyphs
{
	private Dictionary<string, byte[]> m_glyphs = new Dictionary<string, byte[]>();

	private double[] m_fontMatrix;

	private Dictionary<int, string> m_differenceEncoding = new Dictionary<int, string>();

	private Dictionary<string, object> m_renderedPath = new Dictionary<string, object>();

	private int m_globalBias;

	private string[] m_diffTable;

	internal Dictionary<string, byte[]> Glyphs
	{
		get
		{
			return m_glyphs;
		}
		set
		{
			m_glyphs = value;
		}
	}

	public int GlobalBias
	{
		get
		{
			return m_globalBias;
		}
		set
		{
			m_globalBias = value;
		}
	}

	internal double[] FontMatrix
	{
		get
		{
			return m_fontMatrix;
		}
		set
		{
			m_fontMatrix = value;
		}
	}

	internal Dictionary<int, string> DifferenceEncoding
	{
		get
		{
			return m_differenceEncoding;
		}
		set
		{
			m_differenceEncoding = value;
		}
	}

	internal Dictionary<string, object> RenderedPath
	{
		get
		{
			return m_renderedPath;
		}
		set
		{
			m_renderedPath = value;
		}
	}

	internal string[] DiffTable
	{
		get
		{
			return m_diffTable;
		}
		set
		{
			m_diffTable = value;
		}
	}
}
