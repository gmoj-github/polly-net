using Polly;
using System.Net;
using System.Web.Http;

namespace PollyNetTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void HandleSingleExceptionTest()
        {
            // Arrange
            int maxRetries = 1;
            int executionCounter = 0;
            var policy = Policy.Handle<HttpResponseException>().
                RetryAsync();

            // Act

            // Assert
            Assert.ThrowsAsync<HttpResponseException>(async () => await policy.ExecuteAsync(() =>
            {
                executionCounter++;
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }));
            Assert.That(executionCounter, Is.EqualTo(maxRetries + 1));
        }

        [Test]
        public void HandleTwoExceptionsTest()
        {
            // Arrange
            int retryCounter = 2;
            int executionCounter = 0;
            var policy = Policy.Handle<HttpResponseException>().
                Or<Exception>().
                RetryAsync(retryCounter);

            // Act

            // Assert
            Assert.ThrowsAsync<Exception>(async () => await policy.ExecuteAsync(() =>
            {
                executionCounter++;
                if (executionCounter == 1)
                    throw new HttpResponseException(HttpStatusCode.Forbidden);
                else
                    throw new Exception();
            }));
            Assert.That(executionCounter, Is.EqualTo(retryCounter + 1));
        }

        [Test]
        public void FallbackTest()
        {
            // Arrange
            bool fallbackReached = false;
            var policy = Policy.Handle<HttpResponseException>()
                .Fallback(() =>
                {
                    fallbackReached = true;
                });

            // Act
            policy.Execute(() => throw new HttpResponseException(HttpStatusCode.BadRequest));

            // Assert        
            Assert.True(fallbackReached);
        }
    }
}