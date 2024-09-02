using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class ThreeDFormat
{
	private byte m_flagsA;

	private byte m_flagsB;

	private ShapeBase m_shape;

	private Dictionary<int, object> m_propertiesHash;

	internal Dictionary<string, Stream> m_docxProps = new Dictionary<string, Stream>();

	internal const byte ContourWidthKey = 0;

	internal const byte ExtrusionHeightKey = 1;

	internal const byte PresetMaterialTypeKey = 2;

	internal const byte DistanceFromGroundKey = 3;

	internal const byte BevelBottomHeightKey = 4;

	internal const byte BevelBottomWidthKey = 5;

	internal const byte BevelBPresetTypeKey = 6;

	internal const byte BevelTopHeightKey = 7;

	internal const byte BevelTopWidthKey = 8;

	internal const byte BevelTPresetTypeKey = 9;

	internal const byte ContourColorKey = 10;

	internal const byte ContourOpacityKey = 11;

	internal const byte ExtrusionColorKey = 12;

	internal const byte ExtrusionOpacityKey = 13;

	internal const byte BackdropAnchorXKey = 14;

	internal const byte BackdropAnchorYKey = 15;

	internal const byte BackdropAnchorZKey = 16;

	internal const byte BackdropNormalXKey = 17;

	internal const byte BackdropNormalYKey = 18;

	internal const byte BackdropNormalZKey = 19;

	internal const byte BackdropUpXKey = 20;

	internal const byte BackdropUpYKey = 21;

	internal const byte BackdropUpZKey = 22;

	internal const byte FieldOfViewKey = 23;

	internal const byte CameraPresetTypeKey = 24;

	internal const byte ZoomKey = 25;

	internal const byte CameraRotationXKey = 26;

	internal const byte CameraRotationYKey = 27;

	internal const byte CameraRotationZKey = 28;

	internal const byte LightRigTypeKey = 29;

	internal const byte LightRigDirectionKey = 30;

	internal const byte LightRigRotationXKey = 31;

	internal const byte LightRigRotationYKey = 32;

	internal const byte LightRigRotationZKey = 33;

	internal const byte BrightnessKey = 34;

	internal const byte ColorModeKey = 36;

	internal const byte DiffusityKey = 37;

	internal const byte EdgeKey = 38;

	internal const byte FacetKey = 39;

	internal const byte ForeDepthKey = 40;

	internal const byte LightLevelKey = 41;

	internal const byte LightLevel2Key = 42;

	internal const byte LightRigRotation2XKey = 43;

	internal const byte LightRigRotation2YKey = 44;

	internal const byte LightRigRotation2ZKey = 45;

	internal const byte RotationXKey = 46;

	internal const byte RotationYKey = 47;

	internal const byte RotationZKey = 48;

	internal const byte OrientationAngleKey = 49;

	internal const byte ExtrusionPlaneKey = 50;

	internal const byte ExtrusionTypeKey = 51;

	internal const byte ExtrusionRenderModeKey = 52;

	internal const byte RotationAngleXKey = 53;

	internal const byte RotationAngleYKey = 54;

	internal const byte RotationCenterXKey = 55;

	internal const byte RotationCenterYKey = 56;

	internal const byte RotationCenterZKey = 57;

	internal const byte ShininessKey = 58;

	internal const byte SkewAmountKey = 59;

	internal const byte SkewAngleKey = 60;

	internal const byte SpecularityKey = 61;

	internal const byte ViewPointXKey = 62;

	internal const byte ViewPointYKey = 63;

	internal const byte ViewPointZKey = 64;

	internal const byte ViewPointOriginXKey = 65;

	internal const byte ViewPointOriginYKey = 66;

	internal const byte VisibleKey = 67;

	internal const byte LightFaceKey = 68;

	internal const byte LightHarshKey = 69;

	internal const byte LightHarsh2Key = 70;

	internal const byte LockRotationCenterKey = 71;

	internal const byte MetalKey = 72;

	internal const byte ExtensionKey = 73;

	internal const byte BackDepthKey = 74;

	internal const byte AutoRotationCenterKey = 75;

	internal float ContourWidth
	{
		get
		{
			return (float)PropertiesHash[0];
		}
		set
		{
			if (value < 0f && value > 20116800f)
			{
				throw new ArgumentOutOfRangeException("Contour Width should be between 0 and 1584");
			}
			SetKeyValue(0, value);
		}
	}

	internal float ExtrusionHeight
	{
		get
		{
			return (float)PropertiesHash[1];
		}
		set
		{
			if (value < 0f && value > 20116800f)
			{
				throw new ArgumentOutOfRangeException("Extrusion Height should be between 0 and 1584");
			}
			SetKeyValue(1, value);
		}
	}

	internal PresetMaterialType PresetMaterialType
	{
		get
		{
			string value = PropertiesHash[2].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(2) && PropertiesHash[2] != null && !string.IsNullOrEmpty(value))
			{
				return (PresetMaterialType)Enum.Parse(typeof(PresetMaterialType), value, ignoreCase: true);
			}
			return PresetMaterialType.None;
		}
		set
		{
			SetKeyValue(2, value);
		}
	}

	internal float DistanceFromGround
	{
		get
		{
			return (float)PropertiesHash[3];
		}
		set
		{
			if (value < -50800000f && value > 50800000f)
			{
				throw new ArgumentOutOfRangeException("Contour Width should be between -4000 and 4000");
			}
			SetKeyValue(3, value);
		}
	}

	internal float BevelBottomHeight
	{
		get
		{
			return (float)PropertiesHash[4];
		}
		set
		{
			if (value < 0f && value > 20116800f)
			{
				throw new ArgumentOutOfRangeException("Bevel Bottom Height should be between 0 and 1584");
			}
			SetKeyValue(4, value);
		}
	}

	internal float BevelBottomWidth
	{
		get
		{
			return (float)PropertiesHash[5];
		}
		set
		{
			if (value < 0f && value > 20116800f)
			{
				throw new ArgumentOutOfRangeException("Bevel Bottom Width should be between 0 and 1584");
			}
			SetKeyValue(5, value);
		}
	}

	internal BevelPresetType BevelBPresetType
	{
		get
		{
			string value = PropertiesHash[6].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(6) && PropertiesHash[6] != null && !string.IsNullOrEmpty(value))
			{
				return (BevelPresetType)Enum.Parse(typeof(BevelPresetType), value, ignoreCase: true);
			}
			return BevelPresetType.None;
		}
		set
		{
			SetKeyValue(6, value);
		}
	}

	internal float BevelTopHeight
	{
		get
		{
			return (float)PropertiesHash[7];
		}
		set
		{
			if (value < 0f && value > 20116800f)
			{
				throw new ArgumentOutOfRangeException("Bevel Top Height should be between 0 and 1584");
			}
			SetKeyValue(7, value);
		}
	}

	internal float BevelTopWidth
	{
		get
		{
			return (float)PropertiesHash[8];
		}
		set
		{
			if (value < 0f && value > 20116800f)
			{
				throw new ArgumentOutOfRangeException("Bevel Top Width should be between 0 and 1584");
			}
			SetKeyValue(8, value);
		}
	}

	internal BevelPresetType BevelTPresetType
	{
		get
		{
			string value = PropertiesHash[9].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(9) && PropertiesHash[9] != null && !string.IsNullOrEmpty(value))
			{
				return (BevelPresetType)Enum.Parse(typeof(BevelPresetType), value, ignoreCase: true);
			}
			return BevelPresetType.None;
		}
		set
		{
			SetKeyValue(9, value);
		}
	}

	internal Color ContourColor
	{
		get
		{
			return (Color)PropertiesHash[10];
		}
		set
		{
			SetKeyValue(10, value);
		}
	}

	internal float ContourOpacity
	{
		get
		{
			return (float)PropertiesHash[11];
		}
		set
		{
			SetKeyValue(11, value);
		}
	}

	internal Color ExtrusionColor
	{
		get
		{
			return (Color)PropertiesHash[12];
		}
		set
		{
			SetKeyValue(12, value);
		}
	}

	internal float ExtrusionOpacity
	{
		get
		{
			return (float)PropertiesHash[13];
		}
		set
		{
			SetKeyValue(13, value);
		}
	}

	internal float BackdropAnchorX
	{
		get
		{
			return (float)PropertiesHash[14];
		}
		set
		{
			SetKeyValue(14, value);
		}
	}

	internal float BackdropAnchorY
	{
		get
		{
			return (float)PropertiesHash[15];
		}
		set
		{
			SetKeyValue(15, value);
		}
	}

	internal float BackdropAnchorZ
	{
		get
		{
			return (float)PropertiesHash[16];
		}
		set
		{
			SetKeyValue(16, value);
		}
	}

	internal float BackdropNormalX
	{
		get
		{
			return (float)PropertiesHash[17];
		}
		set
		{
			SetKeyValue(17, value);
		}
	}

	internal float BackdropNormalY
	{
		get
		{
			return (float)PropertiesHash[18];
		}
		set
		{
			SetKeyValue(18, value);
		}
	}

	internal float BackdropNormalZ
	{
		get
		{
			return (float)PropertiesHash[19];
		}
		set
		{
			SetKeyValue(19, value);
		}
	}

	internal float BackdropUpX
	{
		get
		{
			return (float)PropertiesHash[20];
		}
		set
		{
			SetKeyValue(20, value);
		}
	}

	internal float BackdropUpY
	{
		get
		{
			return (float)PropertiesHash[21];
		}
		set
		{
			SetKeyValue(21, value);
		}
	}

	internal float BackdropUpZ
	{
		get
		{
			return (float)PropertiesHash[22];
		}
		set
		{
			SetKeyValue(22, value);
		}
	}

	internal float FieldOfView
	{
		get
		{
			return (float)PropertiesHash[23];
		}
		set
		{
			if (value < 0f && value > 10800000f)
			{
				throw new ArgumentOutOfRangeException("Field of View Angle should be between 0 and 180");
			}
			SetKeyValue(23, value);
		}
	}

	internal CameraPresetType CameraPresetType
	{
		get
		{
			string value = PropertiesHash[24].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(24) && PropertiesHash[24] != null && !string.IsNullOrEmpty(value))
			{
				return (CameraPresetType)Enum.Parse(typeof(CameraPresetType), value, ignoreCase: true);
			}
			return CameraPresetType.None;
		}
		set
		{
			SetKeyValue(24, value);
		}
	}

	internal float Zoom
	{
		get
		{
			return (float)PropertiesHash[25];
		}
		set
		{
			SetKeyValue(25, value);
		}
	}

	internal float CameraRotationX
	{
		get
		{
			return (float)PropertiesHash[26];
		}
		set
		{
			if (value < 0f && value > 21600000f)
			{
				throw new ArgumentOutOfRangeException("Camera Rotation X-axis should be between 0 and 360");
			}
			SetKeyValue(26, value);
		}
	}

	internal float CameraRotationY
	{
		get
		{
			return (float)PropertiesHash[27];
		}
		set
		{
			if (value < 0f && value > 21600000f)
			{
				throw new ArgumentOutOfRangeException("Camera Rotation Y-axis should be between 0 and 360");
			}
			SetKeyValue(27, value);
		}
	}

	internal float CameraRotationZ
	{
		get
		{
			return (float)PropertiesHash[28];
		}
		set
		{
			if (value < 0f && value > 21600000f)
			{
				throw new ArgumentOutOfRangeException("Camera Rotation Z-axis should be between 0 and 360");
			}
			SetKeyValue(28, value);
		}
	}

	internal LightRigType LightRigType
	{
		get
		{
			string value = PropertiesHash[29].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(29) && PropertiesHash[29] != null && !string.IsNullOrEmpty(value))
			{
				return (LightRigType)Enum.Parse(typeof(LightRigType), value, ignoreCase: true);
			}
			return LightRigType.None;
		}
		set
		{
			SetKeyValue(29, value);
		}
	}

	internal LightRigDirection LightRigDirection
	{
		get
		{
			string value = PropertiesHash[30].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(30) && PropertiesHash[30] != null && !string.IsNullOrEmpty(value))
			{
				return (LightRigDirection)Enum.Parse(typeof(LightRigDirection), value, ignoreCase: true);
			}
			return LightRigDirection.T;
		}
		set
		{
			SetKeyValue(30, value);
		}
	}

	internal float LightRigRotationX
	{
		get
		{
			return (float)PropertiesHash[31];
		}
		set
		{
			SetKeyValue(31, value);
		}
	}

	internal float LightRigRotationY
	{
		get
		{
			return (float)PropertiesHash[32];
		}
		set
		{
			SetKeyValue(32, value);
		}
	}

	internal float LightRigRotationZ
	{
		get
		{
			return (float)PropertiesHash[33];
		}
		set
		{
			SetKeyValue(33, value);
		}
	}

	internal float Brightness
	{
		get
		{
			return (float)PropertiesHash[34];
		}
		set
		{
			SetKeyValue(34, value);
		}
	}

	internal string ColorMode
	{
		get
		{
			return PropertiesHash[11].ToString();
		}
		set
		{
			SetKeyValue(36, value);
		}
	}

	internal float Diffusity
	{
		get
		{
			return (float)PropertiesHash[37];
		}
		set
		{
			SetKeyValue(37, value);
		}
	}

	internal float Edge
	{
		get
		{
			return (float)PropertiesHash[38];
		}
		set
		{
			SetKeyValue(38, value);
		}
	}

	internal float Facet
	{
		get
		{
			return (float)PropertiesHash[39];
		}
		set
		{
			SetKeyValue(39, value);
		}
	}

	internal float ForeDepth
	{
		get
		{
			return (float)PropertiesHash[40];
		}
		set
		{
			SetKeyValue(40, value);
		}
	}

	internal float BackDepth
	{
		get
		{
			return (float)PropertiesHash[74];
		}
		set
		{
			SetKeyValue(74, value);
		}
	}

	internal float LightLevel
	{
		get
		{
			return (float)PropertiesHash[41];
		}
		set
		{
			SetKeyValue(41, value);
		}
	}

	internal float LightLevel2
	{
		get
		{
			return (float)PropertiesHash[42];
		}
		set
		{
			SetKeyValue(42, value);
		}
	}

	internal float LightRigRotation2X
	{
		get
		{
			return (float)PropertiesHash[43];
		}
		set
		{
			if (value < 0f && value > 21600000f)
			{
				throw new ArgumentOutOfRangeException("LightRig Rotation X-axis should be between 0 and 360");
			}
			SetKeyValue(43, value);
		}
	}

	internal float LightRigRotation2Y
	{
		get
		{
			return (float)PropertiesHash[44];
		}
		set
		{
			if (value < 0f && value > 21600000f)
			{
				throw new ArgumentOutOfRangeException("LightRig Rotation Y-axis should be between 0 and 360");
			}
			SetKeyValue(44, value);
		}
	}

	internal float LightRigRotation2Z
	{
		get
		{
			return (float)PropertiesHash[45];
		}
		set
		{
			if (value < 0f && value > 21600000f)
			{
				throw new ArgumentOutOfRangeException("LightRig Rotation Z-axis should be between 0 and 360");
			}
			SetKeyValue(45, value);
		}
	}

	internal float RotationX
	{
		get
		{
			return (float)PropertiesHash[46];
		}
		set
		{
			SetKeyValue(46, value);
		}
	}

	internal float RotationY
	{
		get
		{
			return (float)PropertiesHash[47];
		}
		set
		{
			SetKeyValue(47, value);
		}
	}

	internal float RotationZ
	{
		get
		{
			return (float)PropertiesHash[48];
		}
		set
		{
			SetKeyValue(48, value);
		}
	}

	internal float OrientationAngle
	{
		get
		{
			return (float)PropertiesHash[49];
		}
		set
		{
			SetKeyValue(49, value);
		}
	}

	internal ExtrusionPlane ExtrusionPlane
	{
		get
		{
			string value = PropertiesHash[50].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(50) && PropertiesHash[50] != null && !string.IsNullOrEmpty(value))
			{
				return (ExtrusionPlane)Enum.Parse(typeof(ExtrusionPlane), value, ignoreCase: true);
			}
			return ExtrusionPlane.XY;
		}
		set
		{
			SetKeyValue(50, value);
		}
	}

	internal ExtrusionType ExtrusionType
	{
		get
		{
			string value = PropertiesHash[51].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(51) && PropertiesHash[51] != null && !string.IsNullOrEmpty(value))
			{
				return (ExtrusionType)Enum.Parse(typeof(ExtrusionType), value, ignoreCase: true);
			}
			return ExtrusionType.Parallel;
		}
		set
		{
			SetKeyValue(51, value);
		}
	}

	internal ExtrusionRenderMode ExtrusionRenderMode
	{
		get
		{
			string value = PropertiesHash[52].ToString();
			if (PropertiesHash != null && PropertiesHash.ContainsKey(52) && PropertiesHash[52] != null && !string.IsNullOrEmpty(value))
			{
				return (ExtrusionRenderMode)Enum.Parse(typeof(ExtrusionRenderMode), value, ignoreCase: true);
			}
			return ExtrusionRenderMode.Solid;
		}
		set
		{
			SetKeyValue(52, value);
		}
	}

	internal float RotationAngleX
	{
		get
		{
			return (float)PropertiesHash[53];
		}
		set
		{
			SetKeyValue(53, value);
		}
	}

	internal float RotationAngleY
	{
		get
		{
			return (float)PropertiesHash[54];
		}
		set
		{
			SetKeyValue(54, value);
		}
	}

	internal float RotationCenterX
	{
		get
		{
			return (float)PropertiesHash[55];
		}
		set
		{
			SetKeyValue(55, value);
		}
	}

	internal float RotationCenterY
	{
		get
		{
			return (float)PropertiesHash[56];
		}
		set
		{
			SetKeyValue(56, value);
		}
	}

	internal float RotationCenterZ
	{
		get
		{
			return (float)PropertiesHash[57];
		}
		set
		{
			SetKeyValue(57, value);
		}
	}

	internal float Shininess
	{
		get
		{
			return (float)PropertiesHash[58];
		}
		set
		{
			SetKeyValue(58, value);
		}
	}

	internal float SkewAmount
	{
		get
		{
			return (float)PropertiesHash[59];
		}
		set
		{
			SetKeyValue(59, value);
		}
	}

	internal float SkewAngle
	{
		get
		{
			return (float)PropertiesHash[60];
		}
		set
		{
			SetKeyValue(60, value);
		}
	}

	internal float Specularity
	{
		get
		{
			return (float)PropertiesHash[61];
		}
		set
		{
			SetKeyValue(61, value);
		}
	}

	internal float ViewPointX
	{
		get
		{
			return (float)PropertiesHash[62];
		}
		set
		{
			SetKeyValue(62, value);
		}
	}

	internal float ViewPointY
	{
		get
		{
			return (float)PropertiesHash[63];
		}
		set
		{
			SetKeyValue(63, value);
		}
	}

	internal float ViewPointZ
	{
		get
		{
			return (float)PropertiesHash[64];
		}
		set
		{
			SetKeyValue(64, value);
		}
	}

	internal float ViewPointOriginX
	{
		get
		{
			return (float)PropertiesHash[65];
		}
		set
		{
			SetKeyValue(65, value);
		}
	}

	internal float ViewPointOriginY
	{
		get
		{
			return (float)PropertiesHash[66];
		}
		set
		{
			SetKeyValue(66, value);
		}
	}

	internal bool Visible
	{
		get
		{
			return (m_flagsA & 1) != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xFEu) | (value ? 1u : 0u));
			SetKeyValue(67, value);
		}
	}

	internal bool LightFace
	{
		get
		{
			return (m_flagsA & 2) >> 1 != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xFDu) | ((value ? 1u : 0u) << 1));
			SetKeyValue(68, value);
		}
	}

	internal bool LightHarsh
	{
		get
		{
			return (m_flagsA & 4) >> 2 != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xFBu) | ((value ? 1u : 0u) << 2));
			SetKeyValue(69, value);
		}
	}

	internal bool LightHarsh2
	{
		get
		{
			return (m_flagsA & 8) >> 3 != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xF7u) | ((value ? 1u : 0u) << 3));
			SetKeyValue(70, value);
		}
	}

	internal bool LockRotationCenter
	{
		get
		{
			return (m_flagsA & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xEFu) | ((value ? 1u : 0u) << 4));
			SetKeyValue(71, value);
		}
	}

	internal bool Metal
	{
		get
		{
			return (m_flagsA & 0x20) >> 5 != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xDFu) | ((value ? 1u : 0u) << 5));
			SetKeyValue(72, value);
		}
	}

	internal bool AutoRotationCenter
	{
		get
		{
			return (m_flagsA & 0x40) >> 6 != 0;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xBFu) | ((value ? 1u : 0u) << 6));
			SetKeyValue(75, value);
		}
	}

	internal string Extension
	{
		get
		{
			return PropertiesHash[73].ToString();
		}
		set
		{
			SetKeyValue(73, value);
		}
	}

	internal bool HasBackdropEffect
	{
		get
		{
			return (m_flagsB & 1) != 0;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool HasCameraEffect
	{
		get
		{
			return (m_flagsB & 2) >> 1 != 0;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool HasLightRigEffect
	{
		get
		{
			return (m_flagsB & 4) >> 2 != 0;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool HasBevelBottom
	{
		get
		{
			return (m_flagsB & 8) >> 3 != 0;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool HasBevelTop
	{
		get
		{
			return (m_flagsB & 0x10) >> 4 != 0;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool HasContourColor
	{
		get
		{
			return (m_flagsB & 0x20) >> 5 != 0;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool HasExtrusionColor
	{
		get
		{
			return (m_flagsB & 0x40) >> 6 != 0;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	internal Dictionary<string, Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new Dictionary<string, Stream>();
			}
			return m_docxProps;
		}
	}

	protected object this[int key]
	{
		get
		{
			return key;
		}
		set
		{
			PropertiesHash[key] = value;
		}
	}

	internal ThreeDFormat(ShapeBase shape)
	{
		m_shape = shape;
	}

	internal void Close()
	{
		if (m_shape != null)
		{
			m_shape = null;
		}
		if (m_docxProps != null && m_docxProps.Count > 0)
		{
			m_docxProps.Clear();
			m_docxProps = null;
		}
		if (m_propertiesHash != null && m_propertiesHash.Count > 0)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
	}

	internal ThreeDFormat Clone()
	{
		ThreeDFormat threeDFormat = (ThreeDFormat)MemberwiseClone();
		if (PropertiesHash != null && PropertiesHash.Count > 0)
		{
			threeDFormat.m_propertiesHash = new Dictionary<int, object>();
			foreach (KeyValuePair<int, object> item in PropertiesHash)
			{
				threeDFormat.PropertiesHash.Add(item.Key, item.Value);
			}
		}
		if (m_docxProps != null && m_docxProps.Count > 0)
		{
			m_shape.Document.CloneProperties(m_docxProps, ref threeDFormat.m_docxProps);
		}
		return threeDFormat;
	}

	private void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	internal bool Compare(ThreeDFormat threeDFormat)
	{
		if (Visible != threeDFormat.Visible || LightFace != threeDFormat.LightFace || LightHarsh != threeDFormat.LightHarsh || LightHarsh2 != threeDFormat.LightHarsh2 || LockRotationCenter != threeDFormat.LockRotationCenter || Metal != threeDFormat.Metal || AutoRotationCenter != threeDFormat.AutoRotationCenter || HasBackdropEffect != threeDFormat.HasBackdropEffect || HasCameraEffect != threeDFormat.HasCameraEffect || HasLightRigEffect != threeDFormat.HasLightRigEffect || HasBevelBottom != threeDFormat.HasBevelBottom || HasBevelTop != threeDFormat.HasBevelTop || HasContourColor != threeDFormat.HasContourColor || HasExtrusionColor != threeDFormat.HasExtrusionColor || PresetMaterialType != threeDFormat.PresetMaterialType || BevelBPresetType != threeDFormat.BevelBPresetType || BevelTPresetType != threeDFormat.BevelTPresetType || CameraPresetType != threeDFormat.CameraPresetType || LightRigType != threeDFormat.LightRigType || LightRigDirection != threeDFormat.LightRigDirection || ExtrusionPlane != threeDFormat.ExtrusionPlane || ExtrusionType != threeDFormat.ExtrusionType || ExtrusionRenderMode != threeDFormat.ExtrusionRenderMode || ContourWidth != threeDFormat.ContourWidth || ExtrusionHeight != threeDFormat.ExtrusionHeight || DistanceFromGround != threeDFormat.DistanceFromGround || BevelBottomHeight != threeDFormat.BevelBottomHeight || BevelBottomWidth != threeDFormat.BevelBottomWidth || BevelTopHeight != threeDFormat.BevelTopHeight || BevelTopWidth != threeDFormat.BevelTopWidth || ContourOpacity != threeDFormat.ContourOpacity || ExtrusionOpacity != threeDFormat.ExtrusionOpacity || BackdropAnchorX != threeDFormat.BackdropAnchorX || BackdropAnchorY != threeDFormat.BackdropAnchorY || BackdropAnchorZ != threeDFormat.BackdropAnchorZ || BackdropNormalX != threeDFormat.BackdropNormalX || BackdropNormalY != threeDFormat.BackdropNormalY || BackdropNormalZ != threeDFormat.BackdropNormalZ || BackdropUpX != threeDFormat.BackdropUpX || BackdropUpY != threeDFormat.BackdropUpY || BackdropUpZ != threeDFormat.BackdropUpZ || FieldOfView != threeDFormat.FieldOfView || Zoom != threeDFormat.Zoom || CameraRotationX != threeDFormat.CameraRotationX || CameraRotationY != threeDFormat.CameraRotationY || CameraRotationZ != threeDFormat.CameraRotationZ || LightRigRotationX != threeDFormat.LightRigRotationX || LightRigRotationY != threeDFormat.LightRigRotationY || LightRigRotationZ != threeDFormat.LightRigRotationZ || Brightness != threeDFormat.Brightness || ColorMode != threeDFormat.ColorMode || Diffusity != threeDFormat.Diffusity || Edge != threeDFormat.Edge || Facet != threeDFormat.Facet || ForeDepth != threeDFormat.ForeDepth || BackDepth != threeDFormat.BackDepth || LightLevel != threeDFormat.LightLevel || LightLevel2 != threeDFormat.LightLevel2 || LightRigRotation2X != threeDFormat.LightRigRotation2X || LightRigRotation2Y != threeDFormat.LightRigRotation2Y || LightRigRotation2Z != threeDFormat.LightRigRotation2Z || RotationX != threeDFormat.RotationX || RotationY != threeDFormat.RotationY || RotationZ != threeDFormat.RotationZ || OrientationAngle != threeDFormat.OrientationAngle || RotationAngleX != threeDFormat.RotationAngleX || RotationAngleY != threeDFormat.RotationAngleY || RotationCenterX != threeDFormat.RotationCenterX || RotationCenterY != threeDFormat.RotationCenterY || RotationCenterZ != threeDFormat.RotationCenterZ || Shininess != threeDFormat.Shininess || SkewAmount != threeDFormat.SkewAmount || SkewAngle != threeDFormat.SkewAngle || Specularity != threeDFormat.Specularity || ViewPointX != threeDFormat.ViewPointX || ViewPointY != threeDFormat.ViewPointY || ViewPointZ != threeDFormat.ViewPointZ || ViewPointOriginX != threeDFormat.ViewPointOriginX || ViewPointOriginY != threeDFormat.ViewPointOriginY || Extension != threeDFormat.Extension || ContourColor.ToArgb() != threeDFormat.ContourColor.ToArgb() || ExtrusionColor.ToArgb() != threeDFormat.ExtrusionColor.ToArgb())
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Visible ? "1" : "0;");
		stringBuilder.Append(LightFace ? "1" : "0;");
		stringBuilder.Append(LightHarsh ? "1" : "0;");
		stringBuilder.Append(LightHarsh2 ? "1" : "0;");
		stringBuilder.Append(LockRotationCenter ? "1" : "0;");
		stringBuilder.Append(Metal ? "1" : "0;");
		stringBuilder.Append(AutoRotationCenter ? "1" : "0;");
		stringBuilder.Append(HasBackdropEffect ? "1" : "0;");
		stringBuilder.Append(HasCameraEffect ? "1" : "0;");
		stringBuilder.Append(HasLightRigEffect ? "1" : "0;");
		stringBuilder.Append(HasBevelBottom ? "1" : "0;");
		stringBuilder.Append(HasBevelTop ? "1" : "0;");
		stringBuilder.Append(HasContourColor ? "1" : "0;");
		stringBuilder.Append(HasExtrusionColor ? "1" : "0;");
		stringBuilder.Append((int)PresetMaterialType + ";");
		stringBuilder.Append((int)BevelBPresetType + ";");
		stringBuilder.Append((int)BevelTPresetType + ";");
		stringBuilder.Append((int)CameraPresetType + ";");
		stringBuilder.Append((int)LightRigType + ";");
		stringBuilder.Append((int)LightRigDirection + ";");
		stringBuilder.Append((int)ExtrusionPlane + ";");
		stringBuilder.Append((int)ExtrusionType + ";");
		stringBuilder.Append((int)ExtrusionRenderMode + ";");
		stringBuilder.Append(ContourWidth + ";");
		stringBuilder.Append(ExtrusionHeight + ";");
		stringBuilder.Append(DistanceFromGround + ";");
		stringBuilder.Append(BevelBottomHeight + ";");
		stringBuilder.Append(BevelBottomWidth + ";");
		stringBuilder.Append(BevelTopHeight + ";");
		stringBuilder.Append(BevelTopWidth + ";");
		stringBuilder.Append(ContourOpacity + ";");
		stringBuilder.Append(ExtrusionOpacity + ";");
		stringBuilder.Append(BackdropAnchorX + ";");
		stringBuilder.Append(BackdropAnchorY + ";");
		stringBuilder.Append(BackdropAnchorZ + ";");
		stringBuilder.Append(BackdropNormalX + ";");
		stringBuilder.Append(BackdropNormalY + ";");
		stringBuilder.Append(BackdropNormalZ + ";");
		stringBuilder.Append(BackdropUpX + ";");
		stringBuilder.Append(BackdropUpY + ";");
		stringBuilder.Append(BackdropUpZ + ";");
		stringBuilder.Append(FieldOfView + ";");
		stringBuilder.Append(Zoom + ";");
		stringBuilder.Append(CameraRotationX + ";");
		stringBuilder.Append(CameraRotationY + ";");
		stringBuilder.Append(CameraRotationZ + ";");
		stringBuilder.Append(LightRigRotationX + ";");
		stringBuilder.Append(LightRigRotationY + ";");
		stringBuilder.Append(LightRigRotationZ + ";");
		stringBuilder.Append(Brightness + ";");
		stringBuilder.Append(ColorMode + ";");
		stringBuilder.Append(Diffusity + ";");
		stringBuilder.Append(Edge + ";");
		stringBuilder.Append(Facet + ";");
		stringBuilder.Append(ForeDepth + ";");
		stringBuilder.Append(BackDepth + ";");
		stringBuilder.Append(LightLevel + ";");
		stringBuilder.Append(LightLevel2 + ";");
		stringBuilder.Append(LightRigRotation2X + ";");
		stringBuilder.Append(LightRigRotation2Y + ";");
		stringBuilder.Append(LightRigRotation2Z + ";");
		stringBuilder.Append(RotationX + ";");
		stringBuilder.Append(RotationY + ";");
		stringBuilder.Append(RotationZ + ";");
		stringBuilder.Append(OrientationAngle + ";");
		stringBuilder.Append(RotationAngleX + ";");
		stringBuilder.Append(RotationAngleY + ";");
		stringBuilder.Append(RotationCenterX + ";");
		stringBuilder.Append(RotationCenterY + ";");
		stringBuilder.Append(RotationCenterZ + ";");
		stringBuilder.Append(Shininess + ";");
		stringBuilder.Append(SkewAmount + ";");
		stringBuilder.Append(SkewAngle + ";");
		stringBuilder.Append(Specularity + ";");
		stringBuilder.Append(ViewPointX + ";");
		stringBuilder.Append(ViewPointY + ";");
		stringBuilder.Append(ViewPointZ + ";");
		stringBuilder.Append(ViewPointOriginX + ";");
		stringBuilder.Append(ViewPointOriginY + ";");
		stringBuilder.Append(Extension + ";");
		stringBuilder.Append(ContourColor.ToArgb() + ";");
		stringBuilder.Append(ExtrusionColor.ToArgb() + ";");
		return stringBuilder;
	}
}
