using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiEstudo.ApiModel
{
	public class CreatTopicModel
	{
		public string projectId { get; set; }
		public string topicId { get; set; }
		public string message { get; set; }
		public string subscriptionID { get; set; }
	}
}
