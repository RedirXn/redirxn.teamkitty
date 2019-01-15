using TeamKitty.Forms.Domain;

namespace TeamKitty.Forms.Repository
{
    internal class InMemoryRepository : IRepository
    {
        private Team _team;

        public void Save(Team team)
        {
            _team = team;
        }
    }
}