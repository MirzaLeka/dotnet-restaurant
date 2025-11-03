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

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					// Create new scope for DI services
					using var scope = _serviceProvider.CreateScope();
					var externalService = scope.ServiceProvider.GetRequiredService<IReplicationService>();

					bool isDataReplicated = await externalService.GetReplicationStatus();

					if (isDataReplicated)
					{
						_logger.LogInformation("Data is already replicated. Skipping subscription for this interval.");
						await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
						continue; // skip this iteration — try again next minute
					}

					// only connect if replication not done
					var factory = new ConnectionFactory(_brokerUri);
					using var connection = factory.CreateConnection(_username, _password);
					connection.Start();

					using var session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
					var destination = session.GetQueue(_queueName);
					using var consumer = session.CreateConsumer(destination);

					while (!stoppingToken.IsCancellationRequested)
					{
						// wait before starting next batch
						await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

						var batch = new List<IMessage>();
						IMessage message;

						while ((message = consumer.ReceiveNoWait()) != null)
						{
							batch.Add(message);
						}

						if (batch.Count > 0)
						{
							_logger.LogInformation("Processing {Count} messages", batch.Count);

							foreach (var msg in batch)
							{
								try
								{
									await HandleMessageAsync(msg);
								}
								catch (Exception ex)
								{
									_logger.LogError(ex, "Failed to process message {CorrelationId}", msg.NMSCorrelationID);
								}
							}
						}
						else
						{
							_logger.LogInformation("No messages in queue at this interval");
						}
					}
				}
				catch (NMSException ex)
				{
					_logger.LogError(ex, "AMQ connection failed. Retrying in 10 seconds...");
					await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Unexpected error in subscriber. Retrying in 10 seconds...");
					await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
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
