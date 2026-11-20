namespace Magazing.Infrastructure.Adapters.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public bool DispatcherEnabled { get; set; } = true;

    public string From { get; set; } = "Magazing <onboarding@resend.dev>";

    public string? ResendApiKey { get; set; }

    public int DispatchIntervalSeconds { get; set; } = 30;

    public int BatchSize { get; set; } = 10;

    public int MaxAttempts { get; set; } = 5;
}
