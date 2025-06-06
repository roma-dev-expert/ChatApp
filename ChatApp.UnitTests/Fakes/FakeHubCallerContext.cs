using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.UnitTests.Fakes
{
    public class FakeHubCallerContext : HubCallerContext
    {
        private readonly string _connectionId;

        public FakeHubCallerContext(string connectionId)
        {
            _connectionId = connectionId;
        }

        public override string ConnectionId => _connectionId;

        public override string UserIdentifier => "TestUser";

        public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

        public override IFeatureCollection Features { get; } = new FeatureCollection();

        public override ClaimsPrincipal User { get; } = new ClaimsPrincipal();

        public override CancellationToken ConnectionAborted => CancellationToken.None;

        public override void Abort() { }
    }
}
