using Apache.NMS.ActiveMQ;
using Apache.NMS;
using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.DL.Models;

namespace DotNet8Starter.BL.Services
{
	public class AMQPublisherService : IAMQPublisherService, IDisposable
	{
		private readonly IConnectionFactory _factory;
		private readonly IConnection _connection;
		private readonly Apache.NMS.ISession _session;

		private readonly string _brokerUri = "broker_uri"; 
		private readonly string _username = "username";
		private readonly string _password = "password";
		private readonly string _queueName = "queue";

		public AMQPublisherService()
		{
			_factory = new ConnectionFactory(_brokerUri);
			_connection = _factory.CreateConnection(_username, _password);
			_connection.Start();

			// You can reuse the session if you guard against threading issues,
			// but safest is to create new session per message
			_session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
		}

		public bool SendMessage<T>(string eventName, T payload, int occurrences = 1)
		{
			var order = OrderEvent.ToOrder(eventName, payload, occurrences);

			try
			{
				// Create session per call if concurrency is high
				using var session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
				IDestination destination = session.GetQueue(_queueName);

				using var producer = session.CreateProducer(destination);
				producer.DeliveryMode = MsgDeliveryMode.Persistent;

				ITextMessage message = session.CreateTextMessage(order);
				message.NMSCorrelationID = Guid.NewGuid().ToString();

				producer.Send(message);
				return true;
			}
			catch (Exception ex)
			{
				// log
			}
			return false;
		}

		public void Dispose()
		{
			_session?.Dispose();
			_connection?.Dispose();
		}
	}

}
