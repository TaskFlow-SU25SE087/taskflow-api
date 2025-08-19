# Effort Points Feature Documentation

## Overview

The Effort Points feature allows you to assign effort points to tasks and distribute them among multiple assignees. This helps in better project planning, workload distribution, and progress tracking.

## Features

### 1. Task Effort Points
- Assign total effort points to a task when creating or updating it
- Effort points represent the estimated complexity or time required for the task
- Optional field - tasks can exist without effort points

### 2. Assignee Effort Points Distribution
- Distribute effort points among multiple assignees
- Each assignee can be assigned a specific number of effort points
- Validation ensures total assigned points equals task effort points

### 3. Bulk Assignment
- Assign multiple users to a task with effort points distribution in a single request
- Automatic validation of effort points distribution
- Support for equal or custom distribution

## Database Schema Changes

### TaskProject Table
- Added `EffortPoints` (int, nullable) - Total effort points for the task

### TaskAssignee Table
- Added `AssignedEffortPoints` (int, nullable) - Effort points assigned to this specific assignee

## API Endpoints

### 1. Create Task with Effort Points
```http
POST /api/projects/{projectId}/tasks
Content-Type: multipart/form-data

{
  "title": "Task Title",
  "description": "Task Description",
  "priority": "High",
  "deadline": "2024-01-15T00:00:00Z",
  "effortPoints": 8,
  "file": [optional file]
}
```

### 2. Update Task with Effort Points
```http
PUT /api/projects/{projectId}/tasks/update/{taskId}
Content-Type: application/json

{
  "title": "Updated Task Title",
  "description": "Updated Description",
  "priority": "Medium",
  "deadline": "2024-01-20T00:00:00Z",
  "effortPoints": 12
}
```

### 3. Assign Task to User with Effort Points
```http
POST /api/projects/{projectId}/tasks/{taskId}/assign
Content-Type: application/json

{
  "implementerId": "user-guid",
  "assignedEffortPoints": 5
}
```

### 4. Bulk Assign Task to Multiple Users
```http
POST /api/projects/{projectId}/tasks/{taskId}/bulk-assign
Content-Type: application/json

{
  "assignees": [
    {
      "implementerId": "user1-guid",
      "assignedEffortPoints": 3
    },
    {
      "implementerId": "user2-guid",
      "assignedEffortPoints": 5
    }
  ]
}
```

## Validation Rules

### 1. Effort Points Validation
- Effort points must be positive integers (0 or greater)
- When assigning to a single user, assigned points cannot exceed task effort points
- When bulk assigning, total assigned points must equal task effort points

### 2. Assignment Validation
- Users must be project members
- Users cannot be assigned to the same task multiple times
- No duplicate assignees in bulk assignment

## Error Codes

- `8003` - Invalid effort points distribution
- `8004` - Duplicate assignee found in request
- `6002` - Task already assigned to user
- `3006` - User not in project

## Usage Examples

### Example 1: Create a task with 8 effort points
```json
{
  "title": "Implement User Authentication",
  "description": "Add JWT-based authentication to the API",
  "priority": "High",
  "effortPoints": 8
}
```

### Example 2: Assign task to one user with 5 points
```json
{
  "implementerId": "123e4567-e89b-12d3-a456-426614174000",
  "assignedEffortPoints": 5
}
```

### Example 3: Bulk assign to multiple users
```json
{
  "assignees": [
    {
      "implementerId": "123e4567-e89b-12d3-a456-426614174000",
      "assignedEffortPoints": 3
    },
    {
      "implementerId": "987fcdeb-51a2-43d1-b789-123456789abc",
      "assignedEffortPoints": 5
    }
  ]
}
```

## Response Format

Tasks now include effort points in the response:

```json
{
  "id": "task-guid",
  "title": "Task Title",
  "description": "Task Description",
  "priority": "High",
  "effortPoints": 8,
  "taskAssignees": [
    {
      "projectMemberId": "member-guid",
      "executor": "John Doe",
      "avatar": "avatar-url",
      "role": "Member",
      "assignedEffortPoints": 5
    }
  ]
}
```

## Best Practices

1. **Estimation**: Use effort points to estimate task complexity (e.g., 1-3 points for simple tasks, 5-8 for medium, 13+ for complex)

2. **Distribution**: When multiple assignees work on a task, distribute points based on:
   - Individual workload capacity
   - Expertise level
   - Time availability

3. **Validation**: Always validate that total assigned points equal task effort points

4. **Updates**: When updating task effort points, consider redistributing among existing assignees

## Migration

Run the following command to apply database changes:

```bash
dotnet ef database update
```

This will add the new columns to your existing database tables.

## Future Enhancements

1. **Automatic Distribution**: Add endpoints for automatic equal distribution
2. **Percentage-based Distribution**: Support for percentage-based effort point allocation
3. **Effort Points Templates**: Predefined effort point values for common task types
4. **Reporting**: Effort points analytics and reporting features
5. **Sprint Planning**: Integration with sprint capacity planning

