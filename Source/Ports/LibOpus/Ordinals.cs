/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using c_short = System.Int16;
global using c_int = System.Int32;
global using c_long = System.Int32;
global using c_float = System.Single;
global using c_double = System.Double;
global using c_uint = System.UInt32;
global using c_ulong = System.UInt32;

global using size_t = System.UInt64;

global using opus_int8 = System.SByte;
global using opus_int16 = System.Int16;
global using opus_int32 = System.Int32;
global using opus_int64 = System.Int64;
global using opus_uint8 = System.Byte;
global using opus_uint32 = System.UInt32;

global using opus_int = System.Int32;			// Used for counters etc; at least 16 bit

global using opus_val16 = System.Single;
global using opus_val32 = System.Single;

global using celt_sig = System.Single;
global using celt_norm = System.Single;
global using celt_ener = System.Single;

global using kiss_fft_scalar = System.Single;
global using kiss_twiddle_scalar = System.Single;

global using CeltDecoder = Polycode.NostalgicPlayer.Ports.LibOpus.Containers.OpusCustomDecoder;
global using CeltMode = Polycode.NostalgicPlayer.Ports.LibOpus.Containers.OpusCustomMode;

global using ec_window = System.UInt32;
global using Ec_Enc = Polycode.NostalgicPlayer.Ports.LibOpus.Containers.Ec_Ctx;
global using Ec_Dec = Polycode.NostalgicPlayer.Ports.LibOpus.Containers.Ec_Ctx;
