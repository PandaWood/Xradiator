namespace Xradiator.Model.TeamCity
{
	/// <summary> Flattened view of a TeamCity buildType + its latest build (REST DTO). </summary>
	public class TeamCityBuildType
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string ProjectName { get; set; }
		public string WebUrl { get; set; }

		public string Status { get; set; }      // SUCCESS / FAILURE / ERROR
		public string State { get; set; }        // finished / running
		public string Number { get; set; }
		public string FinishDate { get; set; }
	}
}
