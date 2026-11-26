using _202Oppgave.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CrisisSystem.Tests;

public class ConsoleEmailSenderTests
{
    [Fact]
    public async Task SendEmail_LogsRecipientAndDecodedMessage()
    {
        var logger = new RecordingLogger();
        await new ConsoleEmailSender(logger).SendEmailAsync("user@example.test", "Reset", "A &amp; B");
        var message = Assert.Single(logger.Messages);
        Assert.Contains("user@example.test", message); Assert.Contains("Reset", message);
        Assert.Contains("A & B", message); Assert.DoesNotContain("&amp;", message);
    }
    private sealed class RecordingLogger : ILogger<ConsoleEmailSender>
    {
        public List<string> Messages { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, error));
    }
}
