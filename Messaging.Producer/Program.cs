using Messaging;
using Messaging.Producer.Samples;

using var provider = NotifyProvider.Create(new MessagingConfiguration());

// may work only one

// new HelloWorld(provider).DoWork();

// new Worker(provider).DoWork();

new Producer(provider).DoWork();