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
using System.Threading;
using System.Net;

namespace ApiEstudo.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	[Route("[controller]")]
	[ApiController]
	public class EstudoPubSubController : ControllerBase
	{
		/// <summary>
		/// 
		/// </summary>
		private const string ProjectId = "";
		/// <summary>
		/// 
		/// </summary>
		private const string TopicId = "";
		/// <summary>
		/// 
		/// </summary>
		private const string SubscriptionId = "";
		/// <summary>
		/// 
		/// </summary>
		private readonly ILogger<EstudoPubSubController> _logger;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="logger"></param>
		public EstudoPubSubController(ILogger<EstudoPubSubController> logger)
		{
			_logger = logger;
		}
		/// <summary>
		///  Passe o valor do projectId = estudo-ci-cd
		///  Passe o id do topico desejado = tp-negocio-ex.
		/// </summary>
		/// <param name="attr"></param>
		[HttpPost]
		[Route("CriarTopico")]
		public string CreateTopic([FromBody]CreatTopicModel attr)
		{
			try
			{
				PublisherServiceApiClient publisherService = PublisherServiceApiClient.Create();
				TopicName topicName = new TopicName(attr.projectId, attr.topicId);
				publisherService.CreateTopic(topicName);
				return topicName.TopicId;
			}
			catch (Exception ex)
			{

				return ex.Message;
			}

		}

		/// <summary>
		/// Passe apenas o valor do projectId = estudo-ci-cd
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("ListarTopicos")]
		public IEnumerable<string> ListTopics([FromBody]CreatTopicModel attr)
		{
			List<string> retorno = new List<string>();
			try
			{
				// List all topics for the project
				PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();
				var topics = publisher.ListTopics(new ProjectName(attr.projectId));

				foreach (Topic topic1 in topics)
				{
					retorno.Add(topic1.Name);
				}

			}
			catch (Exception ex)
			{

				retorno.Add(ex.Message + "-------" + ex.StackTrace);
			}
			return retorno;
		}
		/// <summary>
		/// Passe o valor do projectId  = estudo-ci-cd
		/// Passe o id do topico desejado = tp-negocio-ex
		/// Passe a mensagem desejada
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("PublicarMensagemNoTopico")]
		public IEnumerable<string> PublishToTopic([FromBody]CreatTopicModel attr)
		{
			List<string> retorno = new List<string>();
			try
			{
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
			}
			catch (Exception ex)
			{

				retorno.Add(ex.Message);
				return retorno;
			}


			return retorno;
		}

		

		/// <summary>
		/// Passe o valor do projectId = estudo-ci-cd
		/// passe o valor do subscriptionID = sb-negocio-ex
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
			catch (RpcException ex)
			{
				Console.WriteLine("Erro: {0}", ex.Message);
				retorno.Add(new ListaRetorno { data = ex.Message });
				return retorno;
			}
		}

		/// <summary>
		/// Teste para criar uma assinatura em um tópico específico topicId = estudo-ci-cd
		/// </summary>
		/// <param name="projectId"></param>
		/// <param name="topicId"></param>
		/// <param name="subscriptionId"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("CriarAssinaturaEmUmTopico")]
		public  object CreateSubscription([FromBody] CreatTopicModel attr)
		{
			// [START pubsub_create_pull_subscription]
			SubscriberServiceApiClient subscriber = SubscriberServiceApiClient.Create();
			TopicName topicName = new TopicName(attr.projectId, attr.topicId);
			SubscriptionName subscriptionName = new SubscriptionName(attr.projectId,attr.subscriptionID);
			try
			{
				Subscription subscription = subscriber.CreateSubscription(
					subscriptionName, topicName, pushConfig: null,
					ackDeadlineSeconds: 60);
			}
			catch (RpcException e)
			when (e.Status.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
			{
				// Already exists.  That's fine.
			}
			// [END pubsub_create_pull_subscription]
			return 0;
		}


		/// <summary>
		/// Teste para buscar mensagens de uma assinatura de forma asyncrona 
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
		[HttpGet,HttpPost]
		[Route("RecuperarMensagemAsyncronamente")]
		public async Task<ICollection<ListaRetorno>> ShowMessagesForSubscriptionWithListen([FromBody]CreatTopicModel attr)
		{

			List<ListaRetorno> retorno = new List<ListaRetorno>();
			try
			{
				SubscriptionName subscriptionName = new SubscriptionName(attr.projectId, attr.subscriptionID);
				SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);
				// SubscriberClient runs your message handle function on multiple
				// threads to maximize throughput.
				await subscriber.StartAsync(async (PubsubMessage message, CancellationToken cancel)  => 
					{
						string text = Encoding.UTF8.GetString(message.Data.ToArray());
						retorno.Add(new ListaRetorno
						{
							id = message.MessageId,
							publishDate = message.PublishTime.ToDateTime().ToString("dd-M-yyyy HH:MM:ss"),
							data = text,
						});
						return attr.acknowledge ? SubscriberClient.Reply.Ack: SubscriberClient.Reply.Nack;
					});
				// Run for 3 seconds.
				await Task.Delay(3000);
				await subscriber.StopAsync(CancellationToken.None);

				return retorno;
			}
			catch (RpcException ex)
			{
				Console.WriteLine("Erro: {0}", ex.Message);
				retorno.Add(new ListaRetorno { data = ex.Message });
				return retorno;
			}
		}

	}
}