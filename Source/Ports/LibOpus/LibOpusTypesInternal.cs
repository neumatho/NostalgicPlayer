/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using celt_sig = System.Single;
global using celt_norm = System.Single;
global using celt_ener = System.Single;
global using celt_glog = System.Single;
global using celt_coef = System.Single;

global using kiss_fft_scalar = System.Single;
global using kiss_twiddle_scalar = System.Single;

global using CeltDecoder = Polycode.NostalgicPlayer.Ports.LibOpus.Containers.OpusCustomDecoder;
global using CeltMode = Polycode.NostalgicPlayer.Ports.LibOpus.Containers.OpusCustomMode;

global using ec_window = System.UInt32;
global using Ec_Enc = Polycode.NostalgicPlayer.Ports.LibOpus.Containers.Ec_Ctx;
global using Ec_Dec = Polycode.NostalgicPlayer.Ports.LibOpus.Containers.Ec_Ctx;
