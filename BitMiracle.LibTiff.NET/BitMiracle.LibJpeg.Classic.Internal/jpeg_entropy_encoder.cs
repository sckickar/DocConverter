namespace BitMiracle.LibJpeg.Classic.Internal;

internal abstract class jpeg_entropy_encoder
{
	public delegate void finish_pass_delegate();

	public delegate bool encode_mcu_delegate(JBLOCK[][] MCU_data);

	public encode_mcu_delegate encode_mcu;

	public finish_pass_delegate finish_pass;

	public abstract void start_pass(bool gather_statistics);
}
