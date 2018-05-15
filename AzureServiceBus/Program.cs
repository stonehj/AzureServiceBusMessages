namespace AzureServiceBus
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Azure.ServiceBus;

    public class Program
    {
        internal const string ServiceBusConnectionString = "Endpoint=sb://helensbnstest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=+MbiecUkdjHlsZQz7DY3kytpr9aUb1hvfwvAcm5W9WY=";
        internal const string QueueName = "queue1";
        private static IQueueClient queueClient;

        public static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        internal static async Task MainAsync()
        {
            // const int NumberOfMessages = 10;
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            /*// Send Messages
            await SendMessagesAsync(NumberOfMessages);*/

            // Register the queue message handler and receive messages in a loop
            RegisterOnMessageHandlerAndReceivedMessages();

            Console.ReadKey();

            await queueClient.CloseAsync();
        }

        /*internal static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console
                    Console.WriteLine($"Sending message : {messageBody}");

                    // Send the message to the queue
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }*/

        private static void RegisterOnMessageHandlerAndReceivedMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                                            {
                                                // Maximum number of concurrent calls to the callback ProcessMessagesAsync (), set to 1 for simplicity.
                                                // Set it according to how many messages the application wants to process in parallel
                                                MaxConcurrentCalls = 1,

                                                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                                                // False below indicated the complete operation is andled the the user callback as in ProcessMessagesAsync().
                                                AutoComplete = false
                                            };
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default)
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($" - Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: { context.Action}");
            return Task.CompletedTask;
        }
    }
}
