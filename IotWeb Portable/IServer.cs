using System.Threading.Tasks;

namespace IotWeb.Common
{
    public delegate void ServerStoppedHandler(IServer server);

    public interface IServer
    {
        event ServerStoppedHandler ServerStopped;

        bool Running { get; }

        Task<bool> StartAsync();

        void Stop();
    }
}
