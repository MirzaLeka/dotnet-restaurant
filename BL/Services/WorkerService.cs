using Apache.NMS.ActiveMQ;
using Apache.NMS;
using DotNet8Starter.BL.ServiceInterfaces;

namespace DotNet8Starter.BL.Services
{
	public class WorkerService(IServiceProvider serviceProvider, ILogger<WorkerService> logger) : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider = serviceProvider;
		private readonly ILogger<WorkerService> _logger = logger;

		private readonly string _brokerUri = "broker_uri";
		private readonly string _username = "username";
		private readonly string _password = "password";
		private readonly string _queueName = "queue";

		private IConnection _connection;
		private Apache.NMS.ISession _session;
		private IMessageConsumer _consumer;

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var factory = new ConnectionFactory(_brokerUri);
			_connection = factory.CreateConnection(_username, _password);
			_connection.Start();

			_session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
			var destination = _session.GetQueue(_queueName);
			_consumer = _session.CreateConsumer(destination);


			// (3) 
            // Wait X minutes.
		    // Drain everything currently in the queue.
			// Process those messages(all of them).
			// Repeat.
			while (!stoppingToken.IsCancellationRequested)
			{
				// wait before starting next batch
				await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

				IMessage message;
				var batch = new List<IMessage>();

				// drain queue into a batch
				while ((message = _consumer.ReceiveNoWait()) != null)
				{
					batch.Add(message);
				}

				if (batch.Count > 0)
				{
					_logger.LogInformation("Processing {Count} messages", batch.Count);

					foreach (var msg in batch)
					{
						await HandleMessageAsync(msg);
					}
				}
				else
				{
					_logger.LogInformation("No messages in queue at this interval");
				}
			}

		}

		private async Task HandleMessageAsync(IMessage message)
		{
			if (message is ITextMessage textMessage)
			{
				// read message
				var text = textMessage.Text;

				var correlationId = message.NMSCorrelationID;

				using var scope = _serviceProvider.CreateScope();
				var service = scope.ServiceProvider.GetRequiredService<IOrcherstrationService>();

				await service.OrcherstrateEvents(text, message.NMSCorrelationID);

			}

		}
	}
}
