using Messaging;
using RabbitMQ.Client;

using var provider = NotifyProvider.Create(new MessagingConfiguration());
provider.ConfigureChannel("hello").QueueDeclare(queue: "hello",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

var taskChannel = provider.ConfigureChannel("task_queue");
taskChannel.QueueDeclare(queue: "task_queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);
var taskProperties = taskChannel.CreateBasicProperties();
taskProperties.Persistent = true;

HelloWorld(provider);
TaskQueue(provider, taskProperties);

void HelloWorld(NotifyProvider provider)
{
    var message = "Hello World!";
    provider.Send("hello", message);
    Console.WriteLine(" [x] Sent {0}", message);
}

void TaskQueue(NotifyProvider provider, IBasicProperties properties)
{
    const string message = "Some text";
    while (!Console.KeyAvailable)
    {
        Thread.Sleep(1000);
        provider.Send("task_queue", message, properties: properties);
        Console.WriteLine(" [x] Sent {0}", message);
    }
}