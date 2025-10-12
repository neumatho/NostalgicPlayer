/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Decoder super struct
	/// </summary>
	internal class Silk_Decoder : IClearable, IDeepCloneable<Silk_Decoder>
	{
		public readonly Silk_Decoder_State[] channel_state = ArrayHelper.InitializeArray<Silk_Decoder_State>(Constants.Decoder_Num_Channels);
		public Stereo_Dec_State sStereo = new Stereo_Dec_State();
		public opus_int nChannelsAPI;
		public opus_int nChannelsInternal;
		public bool prev_decode_only_middle;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			foreach (Silk_Decoder_State decoderState in channel_state)
				decoderState.Clear();

			sStereo.Clear();
			nChannelsAPI = 0;
			nChannelsInternal = 0;
			prev_decode_only_middle = false;
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Silk_Decoder MakeDeepClone()
		{
			Silk_Decoder clone = new Silk_Decoder
			{
				sStereo = sStereo.MakeDeepClone(),
				nChannelsAPI = nChannelsAPI,
				nChannelsInternal = nChannelsInternal,
				prev_decode_only_middle = prev_decode_only_middle
			};

			for (int i = channel_state.Length - 1; i >= 0; i--)
				clone.channel_state[i] = channel_state[i].MakeDeepClone();

			return clone;
		}
	}
}
