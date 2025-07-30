/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.InStereo20.Containers
{
	/// <summary>
	/// Sample information
	/// </summary>
	internal class Sample
	{
		public string Name { get; set; }
		public ushort OneShotLength { get; set; }
		public ushort RepeatLength { get; set; }
		public sbyte SampleNumber { get; set; }
		public byte Volume { get; set; }
		public byte VibratoDelay { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoLevel { get; set; }
		public byte PortamentoSpeed { get; set; }
	}
}
