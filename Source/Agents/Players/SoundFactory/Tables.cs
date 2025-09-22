/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SoundFactory.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// Default instrument
		/// </summary>
		/********************************************************************/
		public static readonly Instrument DefaultInstrument = new Instrument
		{
			SampleLength = 1,
			SamplingPeriod = 0,

			EffectByte = InstrumentFlag.None,

			TremoloSpeed = 0,
			TremoloStep = 0,
			TremoloRange = 0,

			PortamentoStep = 0,
			PortamentoSpeed = 0,

			ArpeggioSpeed = 0,

			VibratoDelay = 0,
			VibratoSpeed = 0,
			VibratoStep = 0,
			VibratoAmount = 0,

			AttackTime = 1,
			DecayTime = 0,
			SustainLevel = 64,
			ReleaseTime = 30,

			PhasingStart = 0,
			PhasingEnd = 0,
			PhasingSpeed = 0,
			PhasingStep = 0,

			WaveCount = 1,
			Octave = 0,

			FilterFrequency = 1,
			FilterEnd = 50,
			FilterSpeed = 2,

			DASR_SustainOffset = 0,
			DASR_ReleaseOffset = 0,

			SampleData = [ 100, -100 ]
		};



		/********************************************************************/
		/// <summary>
		/// Period multiply table
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] MultiplyTable =
		[
			32768, 30929, 29193, 27555, 26008, 24549,
			23171, 21870, 20643, 19484, 18391, 17359
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] SampleTable =
		[
			54728, 51656, 48757, 46020, 43437, 40999,
			38698, 36526, 34476, 32541, 30715, 28964
		];
	}
}
