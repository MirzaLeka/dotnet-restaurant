using Apache.NMS.ActiveMQ;
using Apache.NMS;
using DotNet8Starter.BL.ServiceInterfaces;

namespace DotNet8Starter.BL.Services
{
	public class AMQPublisherService : IAMQPublisherService
	{
		private readonly string _brokerUri = "broker_uri";
		private readonly string _username = "username";
		private readonly string _password = "password";
		private readonly string _queueName = "queue";

		public void SendOrder(string order)
		{
			try
			{
				// Create a Connection Factory
				IConnectionFactory factory = new ConnectionFactory(_brokerUri);

				// Connect to the Broker
				using var connection = factory.CreateConnection(_username, _password);
				connection.Start();

				// Start a session
				using var session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

				// Connect to the existing queue (e.g. my.queue)
				IDestination destination = session.GetQueue(_queueName);

				// Set delivery mode
				using var producer = session.CreateProducer(destination);
				producer.DeliveryMode = MsgDeliveryMode.Persistent;

				// Convert string message to ITextMessage and send it
				ITextMessage message = session.CreateTextMessage(order);
				message.NMSCorrelationID = Guid.NewGuid().ToString();

				// nista bez configa
				//int randomIndex = new Random().Next(0, 10);

				//message.NMSPriority = (MsgPriority)randomIndex;

				producer.Send(message);
			}
			catch (NMSException ex)
			{
				var a = ex;
			}
			catch (Exception ex)
			{
				var b = ex;
			}
		}

	}
}
