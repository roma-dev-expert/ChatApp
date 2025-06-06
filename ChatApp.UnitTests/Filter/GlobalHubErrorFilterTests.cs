using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Api.Filters;
using Microsoft.Extensions.Logging;
using ChatApp.UnitTests.Fakes;
using ChatApp.UnitTests.Helpers;

namespace ChatApp.UnitTests.Filters
{
    public class GlobalHubErrorFilterTests
    {
        private readonly GlobalHubErrorFilter _filter;
        private readonly Mock<ILogger<GlobalHubErrorFilter>> _loggerMock;

        public GlobalHubErrorFilterTests()
        {
            _loggerMock = new Mock<ILogger<GlobalHubErrorFilter>>();
            _filter = new GlobalHubErrorFilter(_loggerMock.Object);
        }

        [Fact]
        public async Task InvokeMethodAsync_ReturnsResult_WhenNoException()
        {
            var invocationContext = HubTestFactory.CreateHubInvocationContextMock();

            object expectedResult = new object();
            ValueTask<object?> Next(HubInvocationContext ctx) => new ValueTask<object?>(expectedResult);

            var result = await _filter.InvokeMethodAsync(invocationContext, Next);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("UnauthorizedAccessException", "Access denied.")]
        [InlineData("ArgumentException", "Invalid argument provided.")]
        [InlineData("Exception", "An error occurred while processing your request.")]
        public async Task InvokeMethodAsync_LogsErrorAndSendsErrorMessage_WhenExceptionOccurs(string exceptionName, string expectedErrorMessage)
        {
            var invocationContext = HubTestFactory.CreateHubInvocationContextMock();

            var fakeHub = invocationContext.Hub as FakeHub;
            if (fakeHub == null)
            {
                throw new Exception("Hub is not FakeHub.");
            }

            var mockClients = new Mock<IHubCallerClients>();
            var mockClientProxy = new Mock<ISingleClientProxy>();

            mockClientProxy.Setup(x => x.SendCoreAsync("ReceiveError", It.IsAny<object[]>(), default))
                           .Returns(Task.CompletedTask)
                           .Verifiable();
            mockClients.Setup(x => x.Caller).Returns(mockClientProxy.Object);
            fakeHub.Clients = mockClients.Object;

            Type exceptionType = exceptionName switch
            {
                "UnauthorizedAccessException" => typeof(UnauthorizedAccessException),
                "ArgumentException" => typeof(ArgumentException),
                _ => typeof(Exception)
            };
            Exception exceptionInstance = (Exception)Activator.CreateInstance(exceptionType, "Test Exception")!;

            ValueTask<object?> Next(HubInvocationContext ctx) => throw exceptionInstance;

            await Assert.ThrowsAsync(exceptionType, async () => await _filter.InvokeMethodAsync(invocationContext, Next));

            mockClientProxy.Verify(x => x.SendCoreAsync(
                 "ReceiveError",
                 It.Is<object[]>(args => args != null
                     && args.Length > 0
                     && args[0].Equals(expectedErrorMessage)),
                 default), Times.Once);
        }
    }
}
