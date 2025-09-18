using System;

namespace Axis2.WPF.Models
{
    public static class SObjectTypeHelper
    {
        public static Array Values
        {
            get { return Enum.GetValues(typeof(SObjectType)); }
        }
    }
}