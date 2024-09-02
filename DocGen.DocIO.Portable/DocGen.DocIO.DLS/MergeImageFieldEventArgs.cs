using System.IO;
using DocGen.DocIO.DLS.Entities;

namespace DocGen.DocIO.DLS;

public class MergeImageFieldEventArgs : MergeFieldEventArgs
{
	private bool m_useText;

	private Image m_image;

	private Stream m_imageStream;

	private bool m_bSkip;

	private WPicture m_picture;

	public bool UseText => m_useText;

	public Stream ImageStream
	{
		get
		{
			return m_imageStream;
		}
		set
		{
			m_imageStream = value;
			LoadImage(m_imageStream);
		}
	}

	internal Image Image
	{
		get
		{
			return m_image;
		}
		set
		{
			m_image = value;
		}
	}

	public bool Skip
	{
		get
		{
			return m_bSkip;
		}
		set
		{
			m_bSkip = value;
		}
	}

	public WPicture Picture
	{
		get
		{
			if (ImageStream != null)
			{
				m_picture.LoadImage(ImageStream);
			}
			return m_picture;
		}
	}

	public MergeImageFieldEventArgs(IWordDocument doc, string tableName, int rowIndex, IWMergeField field, object obj)
		: base(doc, tableName, rowIndex, field, obj)
	{
		m_image = obj as Image;
		m_picture = new WPicture(doc);
	}

	private void LoadImage(Stream stream)
	{
		m_image = new Image(stream);
	}
}
