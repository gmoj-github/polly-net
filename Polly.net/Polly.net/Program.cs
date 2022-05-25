using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Polly.net
{
	internal class Program
	{
		static List<HttpStatusCode> httpStatusCodeList = new List<HttpStatusCode>()
		{
			HttpStatusCode.BadRequest,
			HttpStatusCode.Forbidden,
			HttpStatusCode.NotFound,
			HttpStatusCode.Accepted,
			HttpStatusCode.InternalServerError,
			HttpStatusCode.NotImplemented
		};

		public static void Main()
		{
			Console.WriteLine("### Starting program! ###");
			
			MainAsync().GetAwaiter().GetResult();

			Console.WriteLine("### Finishing program! ###");

			Console.ReadLine();
		}

		private static async Task MainAsync()
		{
			//await CallEndpointOneRetryAsync();

			await CallEndpointOneWaitAndRetryAsync();
		}

		private static async Task CallEndpointOneWaitAndRetryAsync()
		{
			Console.WriteLine("###### Start CallEndpointOneWaitAndRetryRAsync ######");

			try
			{
				await Policy.Handle<HttpResponseException>().WaitAndRetryAsync(httpStatusCodeList.Count,
					retryCounter =>
					{
						//var retryTimeSpan = TimeSpan.FromSeconds(Math.Pow(2, retryCounter)); // exponential
						var retryTimeSpan = TimeSpan.FromSeconds(retryCounter); // linear
						Console.WriteLine($"### Retry attempt: {retryTimeSpan.ToString()} ###");
						return retryTimeSpan;
					},
					(exception, timeSpan, retryCounter, context) =>
					{
						Console.WriteLine($"### Retry attempt: {retryCounter} ###");
						Console.WriteLine($"### Delay : {timeSpan} ###");
						Console.WriteLine($"### Exception status Code: {((HttpResponseException)exception).Response.StatusCode} ###");
					}).
					ExecuteAsync(async () =>
					{
						var result = await EndpointOneAsync();
						Console.WriteLine($"### Success: {result.ToString()} ###");
					});
			}
			catch (Exception ex)
			{
				Console.WriteLine("### It was not possible to reach the server! ###");
			}

			Console.WriteLine("###### End CallEndpointOneWaitAndRetryRAsync ######\n");
		}

		private static async Task CallEndpointOneRetryAsync()
		{
			Console.WriteLine("###### Start CallEndpointOneRetryAsync ######");

			try
			{
				await Policy.Handle<HttpResponseException>()
					.RetryAsync(httpStatusCodeList.Count, (exception, retryCount) =>
					{
						Console.WriteLine($"### Retry attempt: {retryCount} ###");
						Console.WriteLine($"### Exception status Code: {((HttpResponseException)exception).Response.StatusCode} ###");
					})
					.ExecuteAsync(async () =>
					{
						var result = await EndpointOneAsync();
						Console.WriteLine($"### Success: {result.ToString()} ###");
					});
			}
			catch (Exception ex)
			{
				Console.WriteLine("### It was not possible to reach the server! ###");
			}

			Console.WriteLine("###### End CallEndpointOneRetryAsync ######\n");
		}

		private static async Task<HttpStatusCode> EndpointOneAsync()
		{
			await Task.Delay(1000);

			var randon = new Random();
			int index = randon.Next(httpStatusCodeList.Count);
			if (httpStatusCodeList[index] != HttpStatusCode.Accepted)
				throw new HttpResponseException(httpStatusCodeList[index]);

			return HttpStatusCode.Accepted;
		}
	}
}
