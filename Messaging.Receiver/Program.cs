using Messaging;using Messaging.Receiver.Samples;

using var provider = NotifyProvider.Create(new MessagingConfiguration());

// new HelloWorld(provider).Subscribe();

// new Worker(provider).Subscribe();

new Receiver(provider).Subscribe();

Console.WriteLine(" [*] Waiting for messages.");
while (!Console.KeyAvailable) { }