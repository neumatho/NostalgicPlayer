/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Structure for controlling decoder operation and reading decoder status
	/// </summary>
	internal class Silk_DecControlStruct : IDeepCloneable<Silk_DecControlStruct>
	{
		/// <summary>
		/// I: Number of channels; 1/2
		/// </summary>
		public opus_int32 nChannelsAPI;

		/// <summary>
		/// I: Number of channels; 1/2
		/// </summary>
		public opus_int32 nChannelsInternal;

		/// <summary>
		/// I: Output signal sampling rate in Hertz; 8000/12000/16000/24000/32000/44100/48000
		/// </summary>
		public opus_int32 API_sampleRate;

		/// <summary>
		/// I: Internal sampling rate used, in Hertz; 8000/12000/16000
		/// </summary>
		public opus_int32 internalSampleRate;

		/// <summary>
		/// I: Number of samples per packet in milliseconds; 10/20/40/60
		/// </summary>
		public opus_int payloadSize_ms;

		/// <summary>
		/// O: Pitch lag of previous frame (0 if unvoiced), measured in samples at 48 kHz
		/// </summary>
		public opus_int prevPitchLag;

		/// <summary>
		/// I: Enable Deep PLC
		/// </summary>
		public bool enable_deep_plc;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			nChannelsAPI = 0;
			nChannelsInternal = 0;
			API_sampleRate = 0;
			internalSampleRate = 0;
			payloadSize_ms = 0;
			prevPitchLag = 0;
			enable_deep_plc = false;
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Silk_DecControlStruct MakeDeepClone()
		{
			return new Silk_DecControlStruct
			{
				nChannelsAPI = nChannelsAPI,
				nChannelsInternal = nChannelsInternal,
				API_sampleRate = API_sampleRate,
				internalSampleRate = internalSampleRate,
				payloadSize_ms = payloadSize_ms,
				prevPitchLag = prevPitchLag,
				enable_deep_plc = enable_deep_plc
			};
		}
	}
}
