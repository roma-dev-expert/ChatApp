using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.UnitTests.Fakes;

namespace ChatApp.UnitTests.Helpers
{
    public static class HubTestFactory
    {
        public static HubInvocationContext CreateHubInvocationContextMock()
        {
            var fakeHub = new FakeHub();

            var hubCallerContext = new FakeHubCallerContext("conn1");

            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            var methodInfo = typeof(FakeHub).GetMethod("TestHubMethod")
                ?? throw new Exception("Method TestHubMethod not found in FakeHub.");

            var args = new List<object?>();

            var candidateType = typeof(HubInvocationContext).Assembly
                .GetTypes()
                .FirstOrDefault(t => typeof(HubInvocationContext).IsAssignableFrom(t) && !t.IsAbstract);

            if (candidateType == null)
            {
                throw new Exception("Could not find a suitable type for HubInvocationContext. Please check your SignalR version.");
            }

            var instance = Activator.CreateInstance(
                    candidateType,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    binder: null,
                    args: new object[] { hubCallerContext, serviceProvider, fakeHub, methodInfo, args },
                    culture: null) as HubInvocationContext;

            if (instance == null)
            {
                throw new Exception("Failed to create an instance of HubInvocationContext.");
            }

            return instance;
        }
    }
}
