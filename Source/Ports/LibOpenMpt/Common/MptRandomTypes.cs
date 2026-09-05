/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using Random_Device = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.Sane_Random_Device;
global using Fast_Prng = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.Lcg_Engine<uint, ushort, Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.Lcg_Msvc_Spec>;
global using Good_Prng = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.MptRanlux48;
global using Default_Prng = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.MptRanlux48;
