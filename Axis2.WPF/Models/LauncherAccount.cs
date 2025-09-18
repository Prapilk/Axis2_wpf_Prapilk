namespace Axis2.WPF.Models
{
    public class LauncherAccount
    {
        public string Name { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public string AlternateClientPath { get; set; } = string.Empty;
    }
}