using NUnit.Framework;
using TeamKitty.Forms.Domain;

namespace Tests
{
    public class TeamTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void EnsureDefaultRowRreturnedWhereNoneExist()
        {
            var teamService = new TeamService(new TestRepository());

            var _team = teamService.AddTeam("TeamName");

            teamService.AddSku(_team, "Beer", 3.5M);
            teamService.AddSku(_team, "Soft Drink", 2.5M);

            teamService.AddTeamMember(_team, "yeah me");

            var result = _team.MemberRows;

            Assert.AreEqual(result.Count, 1);
        }
    }
}