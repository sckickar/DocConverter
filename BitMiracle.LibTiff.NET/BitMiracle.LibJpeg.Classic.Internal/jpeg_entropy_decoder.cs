namespace BitMiracle.LibJpeg.Classic.Internal;

internal abstract class jpeg_entropy_decoder
{
	public delegate void finish_pass_delegate();

	public delegate bool decode_mcu_delegate(JBLOCK[] MCU_data);

	public decode_mcu_delegate decode_mcu;

	public finish_pass_delegate finish_pass;

	public abstract void start_pass();
}
