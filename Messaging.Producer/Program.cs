using Messaging;
using Messaging.Producer.Samples;

using var provider = NotifyProvider.Create(new MessagingConfiguration());

new HelloWorld(provider).DoWork();

new Worker(provider).DoWork();