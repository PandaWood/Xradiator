using System.Linq;
using Xradiator.Config;
using Xradiator.Extensions;
using Xradiator.Model;
using NUnit.Framework;

namespace Xradiator.Tests
{
	[TestFixture]
	public class BuildDataTransformer_Tests
	{
		ViewUrl _viewUrl;
		IConfigSettings _configSettings;
		BuildDataTransformer _transformer;

		[SetUp]
		public void SetUp()
		{
			_viewUrl = new ViewUrl("http://valid/XmlStatusReport.aspx");
			_configSettings = new ConfigSettings { URL = _viewUrl.Url };
			_transformer = new BuildDataTransformer(_configSettings);
		}

		[Test]
		public void status_is_building_if_building_and_last_status_was_failure()
		{
			const string xml =
				@"<Projects><Project name='FooProject' activity='Building' lastBuildStatus='Failure' /></Projects>";

			var projectStatuses = _transformer.Transform(xml);
			Assert.That(projectStatuses.First().CurrentState, Is.EqualTo(ProjectStatus.BUILDING));
		}

		[Test]
		public void status_is_failure_if_sleeping_and_last_status_was_failure()
		{
			const string xml =
				@"<Projects><Project name='FooProject' activity='Sleeping' lastBuildStatus='Failure' /></Projects>";

			var projectStatuses = _transformer.Transform(xml);
			Assert.That(projectStatuses.First().CurrentState.EqualsIgnoreCase(ProjectStatus.FAILURE));
		}

		[Test]
		public void status_is_success_if_sleeping_and_last_status_was_success()
		{
			const string xml =
				@"<Projects><Project name='FooProject' activity='Sleeping' lastBuildStatus='Success' /></Projects>";

			var projectStatuses = _transformer.Transform(xml);
			Assert.That(projectStatuses.First().CurrentState.EqualsIgnoreCase(ProjectStatus.SUCCESS));
		}

		const string SimilarProjectXml =
			@"<Projects>
					<Project name='FooProject' activity='Sleeping' lastBuildStatus='Success' webUrl='http://foo/ccnet'/>
					<Project name='BarProject' activity='Sleeping' lastBuildStatus='Failure' webUrl='http://foo/ccnet'/>
					<Project name='FunProject' activity='Sleeping' lastBuildStatus='Failure' webUrl='http://foo/ccnet'/>
				</Projects>";

		[Test]
		public void CanFilter_ProjectName_With_BeginsWith_RegEx()
		{
			_configSettings.ProjectNameRegEx = "^F.*";
			_transformer = new BuildDataTransformer(_configSettings);

			var projectStatuses = _transformer.Transform(SimilarProjectXml).ToList();

			Assert.That(projectStatuses.Count, Is.EqualTo(2));
			Assert.That(projectStatuses.First().Name, Is.EqualTo("FooProject"));
			Assert.That(projectStatuses.Second().Name, Is.EqualTo("FunProject"));
		}

		[Test]
		public void CanFilter_ProjectName_With_OR_RegEx()
		{
			_configSettings.ProjectNameRegEx = "FooProject|BarProject";
			_transformer = new BuildDataTransformer(_configSettings);

			var projectStatuses = _transformer.Transform(SimilarProjectXml).ToList();

			Assert.That(projectStatuses.Count, Is.EqualTo(2));
			Assert.That(projectStatuses.First().Name, Is.EqualTo("FooProject"));
			Assert.That(projectStatuses.Second().Name, Is.EqualTo("BarProject"));
		}

		[Test]
		public void CanTransform_MultipleProjects_WithNoFiltering()
		{
			const string xml =
				@"<Projects>
					<Project name='FooProject' category='' activity='Sleeping' lastBuildStatus='Success' lastBuildTime='2007-11-16T15:03:46.358374-05:00' webUrl='http://foo/ccnet'/>
					<Project name='BarProject' category='' activity='Sleeping' lastBuildStatus='Failure' lastBuildTime='2007-11-16T05:00:00.2127436-05:00' webUrl='http://foo/ccnet'/>
					<Project name='One_More_Project' category='' activity='Sleeping' lastBuildStatus='Failure' lastBuildTime='2007-11-16T05:50:00.1105168-05:00' webUrl='http://foo/ccnet'/>
				</Projects>";

			var projectStatuses = _transformer.Transform(xml).ToList();
			Assert.That(projectStatuses.Count, Is.EqualTo(3));
			Assert.That(projectStatuses.First().Name, Is.EqualTo("FooProject"));
			Assert.That(projectStatuses.Second().Name, Is.EqualTo("BarProject"));
			Assert.That(projectStatuses.Third().Name, Is.EqualTo("One_More_Project"));
		}

		[Test]
		public void CurrentMessage_IsSet_IfPresent_InXml()
		{
			const string xml =
				@"<Projects><Project name='FooProject' CurrentMessage='A message' category='' /></Projects>";

			var projectStatuses = _transformer.Transform(xml);
			Assert.That(projectStatuses.First().CurrentMessage, Is.EqualTo("A message"));
		}

		[Test]
		public void CanHandleNull()
		{
			var projectStatuses = _transformer.Transform(null);
			Assert.That(projectStatuses.Count(), Is.EqualTo(0));
		}

		[Test]
		public void CanFilter_Category()
		{
			const string xml =
				@"<Projects>
					<Project name='ImportantProject' category='Important' />
					<Project name='LowPriorityProject' category='LowPriority'/>
				</Projects>";

			_configSettings.CategoryRegEx = "Important";
			_transformer = new BuildDataTransformer(_configSettings);
			var projectStatuses = _transformer.Transform(xml).ToList();

			Assert.That(projectStatuses.Count, Is.EqualTo(1));
			Assert.That(projectStatuses.First().Name, Is.EqualTo("ImportantProject"));
		}

		[Test]
		public void CanRead_Breakers_FromMessages_In_CCnet15()
		{
			const string xml =
				@"<Projects>
					<Project name='ccnet1.5_project' activity='Sleeping' lastBuildStatus='Failure'>
						<messages>
							<message text='Breakers : a, b' kind='Breakers'/>
							<message text='FailingTasks : Step1, Step2' kind='FailingTasks'/>
						</messages>
					</Project>
				</Projects>";

			var projectStatuses = _transformer.Transform(xml).ToList();

			Assert.That(projectStatuses.Count, Is.EqualTo(1));
			Assert.That(projectStatuses.First().CurrentMessage, Is.EqualTo("Breakers : a, b"));
		}
	}
}
