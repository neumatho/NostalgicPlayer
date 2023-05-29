/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Synth_S
	{
		public delegate c_int Func_Synth(Memory<Real> bandPtr, c_int channel, Mpg123_Handle fr, bool final);
		public delegate c_int Func_Synth_Mono(Memory<Real> r, Mpg123_Handle fr);
		public delegate c_int Func_Synth_Stereo(Memory<Real> r1, Memory<Real> r2, Mpg123_Handle fr);

		public Func_Synth[,] Plain = new Func_Synth[(c_int)Synth_Resample.Limit, (c_int)Synth_Format.Limit];
		public Func_Synth_Stereo[,] Stereo = new Func_Synth_Stereo[(c_int)Synth_Resample.Limit, (c_int)Synth_Format.Limit];
		public Func_Synth_Mono[,] Mono2Stereo = new Func_Synth_Mono[(c_int)Synth_Resample.Limit, (c_int)Synth_Format.Limit];
		public Func_Synth_Mono[,] Mono = new Func_Synth_Mono[(c_int)Synth_Resample.Limit, (c_int)Synth_Format.Limit];

		/********************************************************************/
		/// <summary>
		/// Copy the current object into the given
		/// </summary>
		/********************************************************************/
		public void Copy(Synth_S destination)
		{
			Array.Copy(Plain, destination.Plain, Plain.Length);
			Array.Copy(Stereo, destination.Stereo, Stereo.Length);
			Array.Copy(Mono2Stereo, destination.Mono2Stereo, Mono2Stereo.Length);
			Array.Copy(Mono, destination.Mono, Mono.Length);
		}
	}
}
