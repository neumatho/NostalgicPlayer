/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Containers;

namespace NostalgicPlayer.Kit.C.Test.Time
{
	/// <summary>
	///
	/// </summary>
	[TestClass]
	public class Time : TestTimeBase
	{
		/********************************************************************/
		/// <summary>
		/// time() with a null pointer should just return the current time
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Null_Argument()
		{
			time_t before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			time_t result = CTime.time(out _);
			time_t after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			Assert.IsTrue((result >= before) && (result <= after), "Returned time is not within the expected range");
		}



		/********************************************************************/
		/// <summary>
		/// time() with a non-null pointer should store the result and
		/// return the same value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stores_Into_Argument()
		{
			time_t before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			time_t result = CTime.time(out time_t buf);
			time_t after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			Assert.AreEqual(result, buf, "Stored value differs from the returned value");
			Assert.IsTrue((result >= before) && (result <= after), "Returned time is not within the expected range");
		}



		/********************************************************************/
		/// <summary>
		/// The value returned by time() should round-trip through gmtime()
		/// and mktime() back to (approximately) the same value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_RoundTrip_With_GmTime()
		{
			time_t now = CTime.time(out _);

			tm broken = CTime.gmtime(now);

			// gmtime gives UTC, so build the same broken-down time back as UTC
			DateTime utc = new DateTime(broken.tm_Year + 1900, broken.tm_Mon + 1, broken.tm_MDay, broken.tm_Hour, broken.tm_Min, broken.tm_Sec, DateTimeKind.Utc);
			time_t rebuilt = new DateTimeOffset(utc).ToUnixTimeSeconds();

			Assert.AreEqual(now, rebuilt, "Round-trip through gmtime did not preserve the time");
		}
	}
}
