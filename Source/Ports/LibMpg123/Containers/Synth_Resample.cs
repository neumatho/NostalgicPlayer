/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum Synth_Resample
	{
		None = -1,
		OneToOne = 0,
		TwoToOne,
		FourToOne,
		NToM,
		Limit
	}
}
