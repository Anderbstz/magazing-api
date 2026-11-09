using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class EmailOutbox
{
    public Guid EmailOutboxId { get; set; }

    public string ToEmail { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string HtmlBody { get; set; } = null!;

    public string? Template { get; set; }

    public string Status { get; set; } = null!;

    public int Attempts { get; set; }

    public string? LastError { get; set; }

    public string? ProviderMessageId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
