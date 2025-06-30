namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class Term
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public string Season { get; set; } = null!;
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = false;

        public List<Project> Projects { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }
}
