namespace Axis2.WPF.Mvvm
{
    public class LogMessageEvent
    {
        public string Message { get; }
        public LogMessageEvent(string message)
        {
            Message = message;
        }
    }
}