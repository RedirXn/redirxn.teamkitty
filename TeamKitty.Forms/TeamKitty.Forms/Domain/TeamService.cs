using System.Collections.Generic;
using System.Linq;
using TeamKitty.Forms.Repository;

namespace TeamKitty.Forms.Domain
{
    public class TeamService
    {
        private readonly IRepository repository;
        private List<Team> _teams = new List<Team>();

        public TeamService(IRepository repository)
        {
            this.repository = repository;
            // build from repo :: replay?
        }

        public IEnumerable<Team> GetTeams()
        {
            return _teams;
        }

        public Team AddTeam(string teamName)
        {
            var addedTeam = new Team()
            {
                Name = teamName,
            };
            _teams.Add(addedTeam);
            // repo?
            return addedTeam;
        }
        
        public void AddSku(Team team, string productName, decimal price)
        {
            team.AddSku(productName, price);
            // repo?
        }

        public void AddTeamMember(Team team, string member)
        {
            team.AddMember(member);
            //repo?
        }

        public void PurchaseProduct(Team team, string loggedInUser, string product)
        {           
            team.PurchaseProduct(loggedInUser, product);
            //repo?
        }
    }
}