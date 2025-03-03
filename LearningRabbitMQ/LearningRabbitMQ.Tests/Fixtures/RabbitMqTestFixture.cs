using Testcontainers.RabbitMq;

namespace LearningRabbitMQ.Tests.Fixtures;

public class RabbitMqTestFixture : IAsyncLifetime
{
    public RabbitMqTestFixture()
    {
        RabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:latest")
            // .WithExposedPort(5672) // RabbitMQ default port
            .Build();
    }

    public RabbitMqContainer RabbitMqContainer { get; }

    public async Task InitializeAsync()
    {
        await RabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await RabbitMqContainer.StopAsync();
    }
}