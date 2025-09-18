using System.Diagnostics;

namespace Axis2.WPF.Services
{
    public class DebugGameCommandService : IGameCommandService
    {
        public void SendCommand(string command)
        {
            // For now, just write the command to the debug output.
            // Later, this will be replaced with the actual C++/CLI implementation.
            Debug.WriteLine($"Sending command to game: {command}");
        }
    }
}
