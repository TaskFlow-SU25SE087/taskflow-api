namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class ImportStudentRequest
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ImportStudentRequest other && StudentId == other.StudentId;
        }

        public override int GetHashCode()
        {
            return StudentId?.GetHashCode() ?? 0;
        }
    }
}

