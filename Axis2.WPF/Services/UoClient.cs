using System.Threading.Tasks;

namespace Axis2.WPF.Services
{
    public class UoClient : IUoClient
    {
        private readonly UoClientCommunicator _communicator;

        public UoClient(UoClientCommunicator communicator)
        {
            _communicator = communicator;
        }

        public void SendToClient(string command)
        {
            Task.Run(async () => await _communicator.SendToUOAsync(command));
        }
    }
}