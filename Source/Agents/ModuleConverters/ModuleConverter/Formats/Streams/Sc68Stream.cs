/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Utility;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats.Streams
{
	/// <summary>
	/// Handle of a single entry
	/// </summary>
	internal class Sc68Stream : ArchiveStream
	{
		private readonly Sc68Entry entry;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sc68Stream(Sc68Entry entry, Stream archiveStream) : base(new MemoryStream(), false)
		{
			this.entry = entry;

			BuildModuleData(archiveStream);
			wrapperStream.Seek(0, SeekOrigin.Begin);
		}

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		public override int GetDecrunchedLength()
		{
			return (int)wrapperStream.Length;
		}
		#endregion

		#region ArchiveStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the crunched data
		/// </summary>
		/********************************************************************/
		public override int GetCrunchedLength()
		{
			return (int)wrapperStream.Length;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create a real SC68 module out of a single module
		/// </summary>
		/********************************************************************/
		private void BuildModuleData(Stream archiveStream)
		{
			using (WriterStream writerStream = new WriterStream(wrapperStream, true))
			{
				writerStream.WriteMark(Sc68Helper.IdString);
				writerStream.WriteByte(0);

				writerStream.WriteMark("SC68");
				writerStream.Write_L_INT32(8 + entry.DataBlockInfo.ModuleLength + 8);

				// Copy the module data
				archiveStream.Seek(entry.DataBlockInfo.ModuleStartPosition, SeekOrigin.Begin);
				StreamHelper.CopyData(archiveStream, writerStream, entry.DataBlockInfo.ModuleLength);

				// End mark
				writerStream.WriteMark("SCEF");
				writerStream.Write_L_UINT32(0);
			}
		}
		#endregion
	}
}
