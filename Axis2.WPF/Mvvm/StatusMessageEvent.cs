namespace Axis2.WPF.Mvvm
{
    public class StatusMessageEvent
    {
        public string Message { get; }
        public StatusMessageEvent(string message)
        {
            Message = message;
        }
    }
}