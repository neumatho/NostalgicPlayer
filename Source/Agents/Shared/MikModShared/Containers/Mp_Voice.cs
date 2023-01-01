/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
#pragma warning disable 1591
	/// <summary>
	/// Mp_Voice structure
	///
	/// Used by NNA only player (audio control. AUDTMP is used for full effects
	/// control).
	/// </summary>
	public class Mp_Voice
	{
		public Mp_Channel Main;

		public EnvPr VEnv;
		public EnvPr PEnv;
		public EnvPr CEnv;

		public short EnvStartPos;				// Start position for envelopes set by XM effect L

		public ushort AVibPos;					// Auto vibrato pos
		public ushort ASwpPos;					// Auto vibrato sweep pos

		public uint TotalVol;					// Total volume of channel (before global mixings)

		public bool MFlag;
		public short MasterChn;
		public ushort MasterPeriod;
		public Mp_Control Master;				// Index of "master" effects channel
	}
#pragma warning restore 1591
}
