/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using uint8_t = System.Byte;
global using uint16_t = System.UInt16;
global using uint32_t = System.UInt32;

global using uint_least8_t = System.Byte;
global using uint_least16_t = System.UInt16;
global using uint_least32_t = System.UInt32;

global using int_least32_t = System.Int32;

global using event_clock_t = System.Int64;

global using sidBankMap_t = System.Collections.Generic.Dictionary<int, Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks.ExtraSidBank>;
global using sidBankMap_value_type = System.Collections.Generic.KeyValuePair<int, Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Banks.ExtraSidBank>;
