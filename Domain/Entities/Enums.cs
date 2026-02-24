using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities;

public enum TaskStatus { Todo, InProgress, Done, Archived }
public enum TaskPriority { Low, Medium, High, Urgent }
public enum RecurrenceType { Daily, Weekly, Monthly, Yearly }
public enum ConversationType { Direct, Group, TaskThread }
public enum MessageType { Text, File, System }
public enum NotificationType
{
    TaskAssigned,
    TaskUpdated,
    TaskCompleted,
    MessageReceived,
    OccurrenceDue
}