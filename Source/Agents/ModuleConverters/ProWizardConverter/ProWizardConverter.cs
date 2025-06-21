/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("9B24479C-4F90-4335-AC5B-15C238C741AD")]

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class ProWizardConverter : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("9442A794-5558-468F-B2E0-872F2F67A142");
		private static readonly Guid agent2Id = Guid.Parse("3F94A0CC-085C-4541-BFEB-5A83A7BDAD42");
		private static readonly Guid agent3Id = Guid.Parse("DC44F257-781D-4A86-9B4C-61CF56C7F611");
		private static readonly Guid agent4Id = Guid.Parse("FF93358F-66BE-4966-A7AC-5F40EADB7530");
		private static readonly Guid agent5Id = Guid.Parse("B07B9C27-2524-445F-A249-C0246A7DF6E8");
		private static readonly Guid agent6Id = Guid.Parse("F62253DA-4ACA-45A0-8ADE-6A16F4FD6365");
		private static readonly Guid agent7Id = Guid.Parse("F09A5E72-8F5E-446C-93D2-12808492D767");
		private static readonly Guid agent8Id = Guid.Parse("EF0BEEC4-95F2-424D-A6F2-B2571F5E16C6");
		private static readonly Guid agent9Id = Guid.Parse("51353BE5-370A-48E3-9144-08B8FD8C7FA7");
		private static readonly Guid agent10Id = Guid.Parse("C641EE45-4F41-445F-BD6D-9C3BDA594AB5");
		private static readonly Guid agent11Id = Guid.Parse("009D5A06-46C2-482C-88F0-4E273A7E840F");
		private static readonly Guid agent12Id = Guid.Parse("F79FE959-C72F-498B-930A-E27132F1AE34");
		private static readonly Guid agent13Id = Guid.Parse("8C1FB353-5C38-4548-B9B5-9D85F45262E7");
		private static readonly Guid agent14Id = Guid.Parse("C32BCF1C-E8F6-4BDE-BC02-F49E8E167023");
		private static readonly Guid agent15Id = Guid.Parse("D96BEDFB-8DD6-4A1B-9958-E9AB7F137197");
		private static readonly Guid agent16Id = Guid.Parse("C93BA9D7-E338-48F5-AB65-47227E5A6D20");
		private static readonly Guid agent17Id = Guid.Parse("579ED378-2649-4AC5-A944-2762F26724B3");
		private static readonly Guid agent18Id = Guid.Parse("B7D9410E-CD74-4C14-9C14-B42C02C7BE4B");
		private static readonly Guid agent19Id = Guid.Parse("69AC38A4-6E77-4CCD-BA30-9D5AB0B6AD37");
		private static readonly Guid agent20Id = Guid.Parse("3C54F0AB-0197-4ED2-8329-BB098091C390");
		private static readonly Guid agent21Id = Guid.Parse("71148366-6437-4BE2-B176-6770E81880FB");
		private static readonly Guid agent22Id = Guid.Parse("108B645B-3893-4043-B876-14171B93CEF2");
		private static readonly Guid agent23Id = Guid.Parse("AA89FE9F-838A-4BC1-9E1B-18943431B83C");
		private static readonly Guid agent24Id = Guid.Parse("FE3E9D35-9AAA-44C7-9FC0-B2C7610E85DB");
		private static readonly Guid agent25Id = Guid.Parse("F5EF1342-E4C7-44F9-BDD6-190B424E7890");
		private static readonly Guid agent26Id = Guid.Parse("F8A42F8E-44E6-4AB1-9E7A-BA2956988308");
		private static readonly Guid agent27Id = Guid.Parse("CB424ED2-90CC-44C0-A422-6B0A77AAAA33");
		private static readonly Guid agent28Id = Guid.Parse("72B09FB6-C386-4D73-922A-FECC7834A5CB");
		private static readonly Guid agent29Id = Guid.Parse("47323161-1105-4852-8C2D-E7B27997B9FE");
		private static readonly Guid agent30Id = Guid.Parse("17EB0373-51A0-4EFC-8633-BB6A5C4061E0");
		private static readonly Guid agent31Id = Guid.Parse("8B48D10A-872C-4BDC-8760-C5F6D4CA5F20");
		private static readonly Guid agent32Id = Guid.Parse("F7D7FC6B-3449-46EE-AED4-D702FD614896");
		private static readonly Guid agent33Id = Guid.Parse("BACB01D1-2E28-4B8E-B9EB-1069E356EA2D");
		private static readonly Guid agent34Id = Guid.Parse("310A6A6A-3A11-48B3-A1B7-8D9A014FDA7A");
		private static readonly Guid agent35Id = Guid.Parse("60C77B32-FB18-4D55-BBFE-667AFAE6235A");
		private static readonly Guid agent36Id = Guid.Parse("BBF6856D-9F40-4363-80E5-B4560B747ACC");
		private static readonly Guid agent37Id = Guid.Parse("50DDAD6E-29B8-401A-B38D-5B37DED19139");
		private static readonly Guid agent38Id = Guid.Parse("C00E827A-8B2A-4B77-B265-2CCBF4038C37");
		private static readonly Guid agent39Id = Guid.Parse("625AF115-422F-4C03-98DB-0E38EE184302");
		private static readonly Guid agent40Id = Guid.Parse("A7D31951-8C47-4A98-A1B1-12E75794EF34");
		private static readonly Guid agent41Id = Guid.Parse("3D367472-38CB-4EBC-AB62-EC8607BC8CC9");
		private static readonly Guid agent42Id = Guid.Parse("72419212-BCB4-427C-9A03-6FDFD654BD36");
		private static readonly Guid agent43Id = Guid.Parse("71BA287E-E84E-4BB1-8183-D9EE44DC1D6D");
		private static readonly Guid agent44Id = Guid.Parse("B5F9E8B4-DA52-4073-BE17-709653D6DB5E");
		private static readonly Guid agent45Id = Guid.Parse("B3B6F3CA-3837-42A9-9B39-5F5AD71A9AAF");
		private static readonly Guid agent46Id = Guid.Parse("4DC91C5C-166A-499D-81C3-0559382176F1");
		private static readonly Guid agent47Id = Guid.Parse("4E844C1F-A38F-4F2F-8A24-05D66DCDFD07");
		private static readonly Guid agent48Id = Guid.Parse("C02965F7-B17C-46DD-8E0A-2CDB4951961B");

		private static readonly Guid agent49Id = Guid.Parse("43221431-A820-4F43-A428-EAF117E2FA5C");
		private static readonly Guid agent50Id = Guid.Parse("2A2584C8-8566-49C7-BE8A-4920FCDD7D52");
		private static readonly Guid agent51Id = Guid.Parse("8870582C-6B1B-43F7-8946-BEFA09C98E0D");
		private static readonly Guid agent52Id = Guid.Parse("2A9E8B08-30ED-4FE9-A2D0-748F0873013B");
		private static readonly Guid agent53Id = Guid.Parse("596498CF-6B75-48CE-9C07-3E67A3947CF7");
		private static readonly Guid agent54Id = Guid.Parse("98F2B102-6592-47F2-BF9A-0D4826C0CD3C");
		private static readonly Guid agent55Id = Guid.Parse("BB892306-5E39-4048-9F07-63EAE5287706");
		private static readonly Guid agent56Id = Guid.Parse("2EF52E59-2B59-4688-A462-9DEA44B2545F");
		private static readonly Guid agent57Id = Guid.Parse("59BCC0C6-1D86-45A4-B3BD-DEE1DF299F09");
		private static readonly Guid agent58Id = Guid.Parse("D892C2B1-E171-46A8-8955-5FBD7349BA13");
		private static readonly Guid agent59Id = Guid.Parse("FED470B5-82BA-416D-8455-16EA8169C123");
		private static readonly Guid agent60Id = Guid.Parse("09676385-57D7-43AA-A33B-E47A947F6614");
		private static readonly Guid agent61Id = Guid.Parse("2AFDA9C3-D213-4036-90F2-6E3145AB6A4D");
		private static readonly Guid agent62Id = Guid.Parse("A4E24245-A678-4535-89D1-A645A6A677D1");
		private static readonly Guid agent63Id = Guid.Parse("2CC711C5-A812-4B1B-991F-772EAC5A6603");
		private static readonly Guid agent64Id = Guid.Parse("814D9C79-5E35-4778-9476-DD353D5C47DB");
		private static readonly Guid agent65Id = Guid.Parse("F86A3C6D-48B5-4D3A-A7C4-F9959FA15AC9");
		private static readonly Guid agent66Id = Guid.Parse("8CC30FBE-191C-466B-997E-F6139A9AAC5F");
		private static readonly Guid agent67Id = Guid.Parse("F591D8C0-F14D-49C5-AE95-45EA87067365");
		private static readonly Guid agent68Id = Guid.Parse("B0F55657-43CD-416E-9CD5-CB665082F065");
		private static readonly Guid agent69Id = Guid.Parse("615E17E3-55F7-4AEA-B7D7-2DE1BB7B6FB7");

		private static readonly Guid agent70Id = Guid.Parse("4C0049C7-9B3C-4D84-A47B-EC6A1B51A66E");
		private static readonly Guid agent71Id = Guid.Parse("750B008B-D8B0-4221-A9C9-9A096BFFEE19");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_PROWIZ_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description
		{
			get
			{
				return string.Format(Resources.IDS_PROWIZ_DESCRIPTION, string.Join("\r\n", AgentInformation.Select(ai => ai.Name).OrderBy(n => n)));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			// These formats are from ProWizard the Amiga version by Gryzor
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT1, Resources.IDS_PROWIZ_DESCRIPTION_AGENT1, agent1Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT2, Resources.IDS_PROWIZ_DESCRIPTION_AGENT2, agent2Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT3, Resources.IDS_PROWIZ_DESCRIPTION_AGENT3, agent3Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT4, Resources.IDS_PROWIZ_DESCRIPTION_AGENT4, agent4Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT5, Resources.IDS_PROWIZ_DESCRIPTION_AGENT5, agent5Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT6, Resources.IDS_PROWIZ_DESCRIPTION_AGENT6, agent6Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT7, Resources.IDS_PROWIZ_DESCRIPTION_AGENT7, agent7Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT8, Resources.IDS_PROWIZ_DESCRIPTION_AGENT8, agent8Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT9, Resources.IDS_PROWIZ_DESCRIPTION_AGENT9, agent9Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT10, Resources.IDS_PROWIZ_DESCRIPTION_AGENT10, agent10Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT11, Resources.IDS_PROWIZ_DESCRIPTION_AGENT11, agent11Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT12, Resources.IDS_PROWIZ_DESCRIPTION_AGENT12, agent12Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT13, Resources.IDS_PROWIZ_DESCRIPTION_AGENT13, agent13Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT14, Resources.IDS_PROWIZ_DESCRIPTION_AGENT14, agent14Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT15, Resources.IDS_PROWIZ_DESCRIPTION_AGENT15, agent15Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT16, Resources.IDS_PROWIZ_DESCRIPTION_AGENT16, agent16Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT17, Resources.IDS_PROWIZ_DESCRIPTION_AGENT17, agent17Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT18, Resources.IDS_PROWIZ_DESCRIPTION_AGENT18, agent18Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT19, Resources.IDS_PROWIZ_DESCRIPTION_AGENT19, agent19Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT20, Resources.IDS_PROWIZ_DESCRIPTION_AGENT20, agent20Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT21, Resources.IDS_PROWIZ_DESCRIPTION_AGENT21, agent21Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT22, Resources.IDS_PROWIZ_DESCRIPTION_AGENT22, agent22Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT23, Resources.IDS_PROWIZ_DESCRIPTION_AGENT23, agent23Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT24, Resources.IDS_PROWIZ_DESCRIPTION_AGENT24, agent24Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT25, Resources.IDS_PROWIZ_DESCRIPTION_AGENT25, agent25Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT26, Resources.IDS_PROWIZ_DESCRIPTION_AGENT26, agent26Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT27, Resources.IDS_PROWIZ_DESCRIPTION_AGENT27, agent27Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT28, Resources.IDS_PROWIZ_DESCRIPTION_AGENT28, agent28Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT29, Resources.IDS_PROWIZ_DESCRIPTION_AGENT29, agent29Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT30, Resources.IDS_PROWIZ_DESCRIPTION_AGENT30, agent30Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT31, Resources.IDS_PROWIZ_DESCRIPTION_AGENT31, agent31Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT32, Resources.IDS_PROWIZ_DESCRIPTION_AGENT32, agent32Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT33, Resources.IDS_PROWIZ_DESCRIPTION_AGENT33, agent33Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT34, Resources.IDS_PROWIZ_DESCRIPTION_AGENT34, agent34Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT35, Resources.IDS_PROWIZ_DESCRIPTION_AGENT35, agent35Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT36, Resources.IDS_PROWIZ_DESCRIPTION_AGENT36, agent36Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT37, Resources.IDS_PROWIZ_DESCRIPTION_AGENT37, agent37Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT38, Resources.IDS_PROWIZ_DESCRIPTION_AGENT38, agent38Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT39, Resources.IDS_PROWIZ_DESCRIPTION_AGENT39, agent39Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT40, Resources.IDS_PROWIZ_DESCRIPTION_AGENT40, agent40Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT41, Resources.IDS_PROWIZ_DESCRIPTION_AGENT41, agent41Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT42, Resources.IDS_PROWIZ_DESCRIPTION_AGENT42, agent42Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT43, Resources.IDS_PROWIZ_DESCRIPTION_AGENT43, agent43Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT44, Resources.IDS_PROWIZ_DESCRIPTION_AGENT44, agent44Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT45, Resources.IDS_PROWIZ_DESCRIPTION_AGENT45, agent45Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT46, Resources.IDS_PROWIZ_DESCRIPTION_AGENT46, agent46Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT47, Resources.IDS_PROWIZ_DESCRIPTION_AGENT47, agent47Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT48, Resources.IDS_PROWIZ_DESCRIPTION_AGENT48, agent48Id),

			// These formats are from ProWizard for PC by Sylvain "Asle" Chipaux
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT49, Resources.IDS_PROWIZ_DESCRIPTION_AGENT49, agent49Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT50, Resources.IDS_PROWIZ_DESCRIPTION_AGENT50, agent50Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT51, Resources.IDS_PROWIZ_DESCRIPTION_AGENT51, agent51Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT52, Resources.IDS_PROWIZ_DESCRIPTION_AGENT52, agent52Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT53, Resources.IDS_PROWIZ_DESCRIPTION_AGENT53, agent53Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT54, Resources.IDS_PROWIZ_DESCRIPTION_AGENT54, agent54Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT55, Resources.IDS_PROWIZ_DESCRIPTION_AGENT55, agent55Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT56, Resources.IDS_PROWIZ_DESCRIPTION_AGENT56, agent56Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT57, Resources.IDS_PROWIZ_DESCRIPTION_AGENT57, agent57Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT58, Resources.IDS_PROWIZ_DESCRIPTION_AGENT58, agent58Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT59, Resources.IDS_PROWIZ_DESCRIPTION_AGENT59, agent59Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT60, Resources.IDS_PROWIZ_DESCRIPTION_AGENT60, agent60Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT61, Resources.IDS_PROWIZ_DESCRIPTION_AGENT61, agent61Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT62, Resources.IDS_PROWIZ_DESCRIPTION_AGENT62, agent62Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT63, Resources.IDS_PROWIZ_DESCRIPTION_AGENT63, agent63Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT64, Resources.IDS_PROWIZ_DESCRIPTION_AGENT64, agent64Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT65, Resources.IDS_PROWIZ_DESCRIPTION_AGENT65, agent65Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT66, Resources.IDS_PROWIZ_DESCRIPTION_AGENT66, agent66Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT67, Resources.IDS_PROWIZ_DESCRIPTION_AGENT67, agent67Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT68, Resources.IDS_PROWIZ_DESCRIPTION_AGENT68, agent68Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT69, Resources.IDS_PROWIZ_DESCRIPTION_AGENT69, agent69Id),

			// These formats are from LibXmp
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT70, Resources.IDS_PROWIZ_DESCRIPTION_AGENT70, agent70Id),
			new AgentSupportInfo(Resources.IDS_PROWIZ_NAME_AGENT71, Resources.IDS_PROWIZ_DESCRIPTION_AGENT71, agent71Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			// These formats are from ProWizard the Amiga version by Gryzor
			if (typeId == agent1Id)
				return new AC1DPackerFormat();

			if (typeId == agent2Id)
				return new ChannelPlayer1Format();

			if (typeId == agent3Id)
				return new ChannelPlayer2Format();

			if (typeId == agent4Id)
				return new ChannelPlayer3Format();

			if (typeId == agent5Id)
				return new ChipTrackerFormat();

			if (typeId == agent6Id)
				return new DigitalIllusionsFormat();

			if (typeId == agent7Id)
				return new EurekaPackerFormat();

			if (typeId == agent8Id)
				return new FcmPackerFormat();

			if (typeId == agent9Id)
				return new FuzzacPackerFormat();

			if (typeId == agent10Id)
				return new HeatseekerFormat();

			if (typeId == agent11Id)
				return new HornetPackerFormat();

			if (typeId == agent12Id)
				return new KefrensSoundMachineFormat();

			if (typeId == agent13Id)
				return new LaxityTrackerFormat();

			if (typeId == agent14Id)
				return new ModuleProtectorFormat();

			if (typeId == agent15Id)
				return new NoisePacker1Format();

			if (typeId == agent16Id)
				return new NoisePacker2Format();

			if (typeId == agent17Id)
				return new NoisePacker3Format();

			if (typeId == agent18Id)
				return new NoiseRunnerFormat();

			if (typeId == agent19Id)
				return new NoiseTrackerCompressedFormat();

			if (typeId == agent20Id)
				return new PhaPackerFormat();

			if (typeId == agent21Id)
				return new PolkaPackerFormat();

			if (typeId == agent22Id)
				return new PowerMusicFormat();

			if (typeId == agent23Id)
				return new Promizer01Format();

			if (typeId == agent24Id)
				return new Promizer10Format();

			if (typeId == agent25Id)
				return new Promizer18Format();

			if (typeId == agent26Id)
				return new Promizer20Format();

			if (typeId == agent27Id)
				return new Promizer40Format();

			if (typeId == agent28Id)
				return new ProPacker10Format();

			if (typeId == agent29Id)
				return new ProPacker21Format();

			if (typeId == agent30Id)
				return new ProPacker30Format();

			if (typeId == agent31Id)
				return new ProRunner1Format();

			if (typeId == agent32Id)
				return new ProRunner2Format();

			if (typeId == agent33Id)
				return new PygmyPackerFormat();

			if (typeId == agent34Id)
				return new SkytPackerFormat();

			if (typeId == agent35Id)
				return new StarTrekkerPackerFormat();

			if (typeId == agent36Id)
				return new ThePlayer40AFormat();

			if (typeId == agent37Id)
				return new ThePlayer40BFormat();

			if (typeId == agent38Id)
				return new ThePlayer41AFormat();

			if (typeId == agent39Id)
				return new ThePlayer50AFormat();

			if (typeId == agent40Id)
				return new ThePlayer60AFormat();

			if (typeId == agent41Id)
				return new ThePlayer61AFormat();

			if (typeId == agent42Id)
				return new TrackerPacker1Format();

			if (typeId == agent43Id)
				return new TrackerPacker2Format();

			if (typeId == agent44Id)
				return new TrackerPacker3Format();

			if (typeId == agent45Id)
				return new UnicTrackerFormat();

			if (typeId == agent46Id)
				return new WantonPackerFormat();

			if (typeId == agent47Id)
				return new XannPackerFormat();

			if (typeId == agent48Id)
				return new ZenPackerFormat();

			// These formats are from ProWizard for PC by Sylvain "Asle" Chipaux
			if (typeId == agent49Id)
				return new BenReplayFormat();

			if (typeId == agent50Id)
				return new BinaryPackerFormat();

			if (typeId == agent51Id)
				return new DevilsReplayFormat();

			if (typeId == agent52Id)
				return new FuchsTrackerFormat();

			if (typeId == agent53Id)
				return new GnoiPackerFormat();

			if (typeId == agent54Id)
				return new GnuPlayerFormat();

			if (typeId == agent55Id)
				return new GnuProtectedModuleFormat();

			if (typeId == agent56Id)
				return new HcdProtectorFormat();

			if (typeId == agent57Id)
				return new ModulePatterncompressorFormat();

			if (typeId == agent58Id)
				return new MoshFormat();

			if (typeId == agent59Id)
				return new NewtronPacker10Format();

			if (typeId == agent60Id)
				return new NewtronPacker20Format();

			if (typeId == agent61Id)
				return new NovoTradePackerFormat();

			if (typeId == agent62Id)
				return new PerfectSong1Format();

			if (typeId == agent63Id)
				return new PerfectSong2Format();

			if (typeId == agent64Id)
				return new SlamTiltFormat();

			if (typeId == agent65Id)
				return new TheDarkDemonFormat();

			if (typeId == agent66Id)
				return new ThePlayer22AFormat();

			if (typeId == agent67Id)
				return new ThePlayer30AFormat();

			if (typeId == agent68Id)
				return new TitanicsPlayerFormat();

			if (typeId == agent69Id)
				return new TmkReplayFormat();

			// These formats are from LibXmp
			if (typeId == agent70Id)
				return new ImagesMusicSystemFormat();

			if (typeId == agent71Id)
				return new MagneticFieldsPackerFormat();

			return null;
		}
		#endregion
	}
}
