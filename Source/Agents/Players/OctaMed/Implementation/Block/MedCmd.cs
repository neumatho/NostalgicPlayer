/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block
{
	/// <summary>
	/// 
	/// </summary>
	internal class MedCmd
	{
		private byte cmdNum;
		private byte data0;
		private byte data1;

		/********************************************************************/
		/// <summary>
		/// Changes the command and arguments
		/// </summary>
		/********************************************************************/
		public void SetCmdData(byte cmd, byte data, byte data2)
		{
			cmdNum = cmd;
			data0 = data;
			data1 = data2;
		}



		/********************************************************************/
		/// <summary>
		/// Changes both arguments
		/// </summary>
		/********************************************************************/
		public void SetData(byte data, byte data2)
		{
			data0 = data;
			data1 = data2;
		}



		/********************************************************************/
		/// <summary>
		/// Changes the first argument
		/// </summary>
		/********************************************************************/
		public void SetData(byte data)
		{
			data0 = data;
		}



		/********************************************************************/
		/// <summary>
		/// Changes the second argument
		/// </summary>
		/********************************************************************/
		public void SetData2(byte data)
		{
			data1 = data;
		}



		/********************************************************************/
		/// <summary>
		/// Return the command
		/// </summary>
		/********************************************************************/
		public byte GetCmd()
		{
			return cmdNum;
		}



		/********************************************************************/
		/// <summary>
		/// Return all the arguments
		/// </summary>
		/********************************************************************/
		public ushort GetData()
		{
			return (ushort)(data0 * 256 + data1);
		}



		/********************************************************************/
		/// <summary>
		/// Return the command argument 1
		/// </summary>
		/********************************************************************/
		public byte GetDataB()
		{
			return data0;
		}



		/********************************************************************/
		/// <summary>
		/// Return the command argument 2
		/// </summary>
		/********************************************************************/
		public byte GetData2()
		{
			return data1;
		}
	}
}
