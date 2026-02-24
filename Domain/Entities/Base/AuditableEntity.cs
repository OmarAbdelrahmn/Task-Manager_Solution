using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Base;

public abstract class AuditableEntity : BaseEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedById { get; set; }
}