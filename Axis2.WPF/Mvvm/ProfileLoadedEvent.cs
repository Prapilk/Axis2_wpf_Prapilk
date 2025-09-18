using Axis2.WPF.Models;

namespace Axis2.WPF.Mvvm
{
    public class ProfileLoadedEvent
    {
        public Profile LoadedProfile { get; }

        public ProfileLoadedEvent(Profile loadedProfile)
        {
            LoadedProfile = loadedProfile;
        }
    }
}