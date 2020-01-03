using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.PubSub.V1;
using ApiEstudo.ApiModel;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Api.Gax.ResourceNames;
using Google.Protobuf;
using Grpc.Core;
using System.Text;

namespace ApiEstudo.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	[Route("[controller]")]
	[ApiController]
	public class EstudoPubSubController : ControllerBase
	{
		private const string ProjectId = "";
		private const string TopicId = "";
		private const string SubscriptionId = "";

		private readonly ILogger<EstudoPubSubController> _logger;
		public EstudoPubSubController(ILogger<EstudoPubSubController> logger)
		{
			_logger = logger;
		}
		/// <summary>
		///  Passe o valor do projectId = estudogcp-261420
		///  Passe o id do topico desejado.
		/// </summary>
		/// <param name="attr"></param>
		[HttpPost]
		[Route("CriarTopico")]
		public void CreateTopic([FromBody]CreatTopicModel attr)
		{
			var credential = GoogleCredential.FromFile(@"C:\gCredentials\estudogcp-261420-2a979ebda616.json");
			// Creates the new topic

			PublisherServiceApiClient publisherService = PublisherServiceApiClient.Create();
			TopicName topicName = new TopicName(attr.projectId, attr.topicId);
			publisherService.CreateTopic(topicName);
		}

		/// <summary>
		/// Passe apenas o valor do projectId = estudogcp-261420
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("ListarTopicos")]
		public IEnumerable<string> ListTopics([FromBody]CreatTopicModel attr)
		{
			try
			{
				// List all topics for the project
				PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();
				var topics = publisher.ListTopics(new ProjectName(attr.projectId));
				List<string> retorno = new List<string>();
				foreach (Topic topic1 in topics)
				{
					retorno.Add(topic1.Name);
				}
				return retorno;
			}
			catch (Exception ex)
			{

				throw ex;
			}
			
		}
		/// <summary>
		/// Passe o valor do projectId = estudogcp-261420
		/// Passe o id do topico desejado
		/// Passe a mensagem desejada
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("PublicarMensagemNoTopico")]
		public IEnumerable<string> PublishToTopic([FromBody]CreatTopicModel attr)
		{
			List<string> retorno = new List<string>();
			var topicName = new TopicName(attr.projectId, attr.topicId);

			PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();

			// Create a message
			var message = new PubsubMessage()
			{
				Data = ByteString.CopyFromUtf8(attr.message)
			};
			//message.Attributes.Add("myattrib", "its value");
			var messageList = new List<PubsubMessage>() { message };

			var response = publisher.Publish(topicName, messageList);
			foreach (string messageId in response.MessageIds)
			{
				retorno.Add(messageId);
			}
			return retorno;
		}
		/// <summary>
		/// Passe o valor do projectId = estudogcp-261420
		/// passe o valor do subscriptionID
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("RecuperarMensagemNoTopico")]
		public IEnumerable<ListaRetorno> ShowMessagesForSubscription([FromBody]CreatTopicModel attr)
		{
			List<ListaRetorno> retorno = new List<ListaRetorno>();

			var subscriptionName = new SubscriptionName(attr.projectId, attr.subscriptionID);

			var subscription = SubscriberServiceApiClient.Create();

			try
			{
				PullResponse response = subscription.Pull(subscriptionName, true, 10);

				var all = response.ReceivedMessages;

				foreach (ReceivedMessage message in all)
				{
					retorno.Add(new ListaRetorno
					{
						id = message.Message.MessageId,
						publishDate = message.Message.PublishTime.ToDateTime().ToString("dd-M-yyyy HH:MM:ss"),
						data = Encoding.UTF8.GetString(message.Message.Data.ToArray(), 0, message.Message.Data.Length),
					});

					subscription.Acknowledge(subscriptionName, new string[] { message.AckId });
				}

				return retorno;
			}
			catch (RpcException e)
			{
				Console.WriteLine("Erro: {0}", e.Message);
				return null;
			}
		}

	}
}