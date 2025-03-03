using System.Text;
using AutoFixture;
using LearningRabbitMQ.Tests.Fixtures;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;

namespace LearningRabbitMQ.Tests;

public class RabbitMqTests : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public RabbitMqTests(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Hello World Test
    /// </summary>
    [Fact]
    public async Task ShouldPublishAndConsumeMessage()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var queueName = "SomeQueue";
        var message = new Fixture().Create<string>();
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        var body = Encoding.UTF8.GetBytes(message);
        
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_fixture.RabbitMqContainer.GetConnectionString())
        };
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        
        // Publish message
        await using var publishChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await publishChannel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false,
            arguments: null, cancellationToken: cancellationToken);
        await publishChannel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body, cancellationToken: cancellationToken);
        
        // Act
        var actual = "";

        // Consume message
        await using var consumeChannel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await consumeChannel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false,
            arguments: null, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(consumeChannel);
        consumer.ReceivedAsync += EventHandler;
        await consumeChannel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer, cancellationToken: cancellationToken);

        // Delay to wait for message queue to process
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    
        // Assert
        var expected = message;
        actual.ShouldBe(expected);
        return;

        Task EventHandler(object sender, BasicDeliverEventArgs @event)
        {
            actual = Encoding.UTF8.GetString(@event.Body.ToArray());
            return Task.CompletedTask;
            
            // Note: thought of using cancel channel right away, and no change in time usage.
            // https://github.com/rabbitmq/rabbitmq-dotnet-client/issues/1567
        }
    }
}