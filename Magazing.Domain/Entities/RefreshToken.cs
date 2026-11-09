using System;
using System.Collections.Generic;

namespace Magazing.Domain.Entities;

public partial class RefreshToken
{
    public Guid RefreshTokenId { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public virtual User User { get; set; } = null!;
}
