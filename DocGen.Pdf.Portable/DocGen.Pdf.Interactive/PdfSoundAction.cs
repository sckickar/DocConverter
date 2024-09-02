using System;
using System.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfSoundAction : PdfAction
{
	private float m_volume = 1f;

	private PdfSound m_sound;

	private bool m_synchronous;

	private bool m_repeat;

	private bool m_mix;

	public float Volume
	{
		get
		{
			return m_volume;
		}
		set
		{
			if (value > 1f || value < -1f)
			{
				throw new ArgumentOutOfRangeException("Volume");
			}
			if (m_volume != value)
			{
				m_volume = value;
				base.Dictionary.SetNumber("Volume", m_volume);
			}
		}
	}

	public string FileName
	{
		get
		{
			return m_sound.FileName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("FileName");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("FileName can't be an empty string.");
			}
			if (value != m_sound.FileName)
			{
				m_sound.FileName = value;
			}
		}
	}

	public PdfSound Sound
	{
		get
		{
			return m_sound;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Sound");
			}
			if (value != m_sound)
			{
				m_sound = value;
			}
		}
	}

	public bool Synchronous
	{
		get
		{
			return m_synchronous;
		}
		set
		{
			if (m_synchronous != value)
			{
				m_synchronous = value;
				base.Dictionary.SetBoolean("Synchronous", m_synchronous);
			}
		}
	}

	public bool Repeat
	{
		get
		{
			return m_repeat;
		}
		set
		{
			if (m_repeat != value)
			{
				m_repeat = value;
				base.Dictionary.SetBoolean("Repeat", m_repeat);
			}
		}
	}

	public bool Mix
	{
		get
		{
			return m_mix;
		}
		set
		{
			if (value != m_mix)
			{
				m_mix = value;
				base.Dictionary.SetBoolean("Mix", m_mix);
			}
		}
	}

	public PdfSoundAction(Stream data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_sound = new PdfSound(data);
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.BeginSave += Dictionary_BeginSave;
		base.Dictionary.SetProperty("S", new PdfName("Sound"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		base.Dictionary.SetProperty("Sound", new PdfReferenceHolder(m_sound));
	}
}
