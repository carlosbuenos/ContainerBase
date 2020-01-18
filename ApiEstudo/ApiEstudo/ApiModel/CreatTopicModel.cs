using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiEstudo.ApiModel
{
	/// <summary>
	/// 
	/// </summary>
	public class CreatTopicModel
	{
		/// <summary>
		/// 
		/// </summary>
		public string projectId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string topicId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string message { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string subscriptionID { get; set; }
	}
}
