using System;
using System.Collections.Generic;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class BulkMoveToSprintRequest
    {
        public List<Guid> TaskIds { get; set; } = new List<Guid>();
        public Guid? SprintId { get; set; }
    }
}
