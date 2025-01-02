/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Shell_Coder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Decode_Split(CPointer<opus_int16> p_child1, CPointer<opus_int16> p_child2, Ec_Dec psRangeDec, opus_int p, CPointer<opus_uint8> shell_table)
		{
			if (p > 0)
			{
				p_child1[0] = (opus_int16)EntDec.Ec_Dec_Icdf(psRangeDec, shell_table + Tables_Pulses_Per_Block.Silk_Shell_Code_Table_Offsets[p], 8);
				p_child2[0] = (opus_int16)(p - p_child1[0]);
			}
			else
			{
				p_child1[0] = 0;
				p_child2[0] = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Shell decoder, operates on one shell code frame of 16 pulses
		/// </summary>
		/********************************************************************/
		public static void Silk_Shell_Decoder(CPointer<opus_int16> pulses0, Ec_Dec psRangeDec, opus_int pulses4)
		{
			CPointer<opus_int16> pulses3 = new CPointer<opus_int16>(2);
			CPointer<opus_int16> pulses2 = new CPointer<opus_int16>(4);
			CPointer<opus_int16> pulses1 = new CPointer<opus_int16>(8);

			Decode_Split(pulses3     , pulses3 + 1 , psRangeDec, pulses4   , Tables_Pulses_Per_Block.Silk_Shell_Code_Table3);

			Decode_Split(pulses2     , pulses2 + 1 , psRangeDec, pulses3[0], Tables_Pulses_Per_Block.Silk_Shell_Code_Table2);

			Decode_Split(pulses1     , pulses1 + 1 , psRangeDec, pulses2[0], Tables_Pulses_Per_Block.Silk_Shell_Code_Table1);
			Decode_Split(pulses0     , pulses0 + 1 , psRangeDec, pulses1[0], Tables_Pulses_Per_Block.Silk_Shell_Code_Table0);
			Decode_Split(pulses0 + 2 , pulses0 + 3 , psRangeDec, pulses1[1], Tables_Pulses_Per_Block.Silk_Shell_Code_Table0);

			Decode_Split(pulses1 + 2 , pulses1 + 3 , psRangeDec, pulses2[1], Tables_Pulses_Per_Block.Silk_Shell_Code_Table1);
			Decode_Split(pulses0 + 4 , pulses0 + 5 , psRangeDec, pulses1[2], Tables_Pulses_Per_Block.Silk_Shell_Code_Table0);
			Decode_Split(pulses0 + 6 , pulses0 + 7 , psRangeDec, pulses1[3], Tables_Pulses_Per_Block.Silk_Shell_Code_Table0);

			Decode_Split(pulses2 + 2 , pulses2 + 3 , psRangeDec, pulses3[1], Tables_Pulses_Per_Block.Silk_Shell_Code_Table2);

			Decode_Split(pulses1 + 4 , pulses1 + 5 , psRangeDec, pulses2[2], Tables_Pulses_Per_Block.Silk_Shell_Code_Table1);
			Decode_Split(pulses0 + 8 , pulses0 + 9 , psRangeDec, pulses1[4], Tables_Pulses_Per_Block.Silk_Shell_Code_Table0);
			Decode_Split(pulses0 + 10, pulses0 + 11, psRangeDec, pulses1[5], Tables_Pulses_Per_Block.Silk_Shell_Code_Table0);

			Decode_Split(pulses1 + 6 , pulses1 + 7 , psRangeDec, pulses2[3], Tables_Pulses_Per_Block.Silk_Shell_Code_Table1);
			Decode_Split(pulses0 + 12, pulses0 + 13, psRangeDec, pulses1[6], Tables_Pulses_Per_Block.Silk_Shell_Code_Table0);
			Decode_Split(pulses0 + 14, pulses0 + 15, psRangeDec, pulses1[7], Tables_Pulses_Per_Block.Silk_Shell_Code_Table0);
		}
	}
}
