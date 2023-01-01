/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Synth
{
	/// <summary>
	/// 
	/// </summary>
	internal class SynthWf
	{
		public uint SyWfLength;						// Length in WORDS, not bytes/samples
		public sbyte[] SyWfData = new sbyte[128];
	}
}
