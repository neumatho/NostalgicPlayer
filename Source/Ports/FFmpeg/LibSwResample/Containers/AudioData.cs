/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class AudioData : IClearable, IDeepCloneable<AudioData>, ICopyTo<AudioData>
	{
		/// <summary>
		/// Samples buffer per channel
		/// </summary>
		public CPointer<uint8_t>[] Ch = new CPointer<uint8_t>[SwrConstants.Swr_Ch_Max];

		/// <summary>
		/// Samples buffer
		/// </summary>
		public CPointer<uint8_t> Data;

		/// <summary>
		/// Number of channels
		/// </summary>
		public c_int Ch_Count;

		/// <summary>
		/// Bytes per sample
		/// </summary>
		public c_int Bps;

		/// <summary>
		/// Number of samples
		/// </summary>
		public c_int Count;

		/// <summary>
		/// 1 if planar audio, 0 otherwise
		/// </summary>
		public c_int Planar;

		/// <summary>
		/// Sample format
		/// </summary>
		public AvSampleFormat Fmt;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Array.Clear(Ch);

			Data.SetToNull();
			Ch_Count = 0;
			Bps = 0;
			Count = 0;
			Planar = 0;
			Fmt = AvSampleFormat.U8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public AudioData MakeDeepClone()
		{
			AudioData clone = (AudioData)MemberwiseClone();

			clone.Ch = ArrayHelper.CloneArray(Ch);

			return clone;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(AudioData destination)
		{
			destination.Data = Data;
			destination.Ch_Count = Ch_Count;
			destination.Bps = Bps;
			destination.Count = Count;
			destination.Planar = Planar;
			destination.Fmt = Fmt;

			Array.Copy(Ch, destination.Ch, Ch.Length);
		}
	}
}
