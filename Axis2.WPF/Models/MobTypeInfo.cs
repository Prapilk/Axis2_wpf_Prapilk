using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis2.WPF.Models
{
    public class MobTypeInfo
    {
        public int Id { get; set; }
        public string Category { get; private set; }
        public string SubId { get; private set; }
        public string Name { get; set; }
        public string Comment { get; set; }

        public bool IsUop
        {
            get
            {
                if (int.TryParse(SubId, out int subIdValue))
                {
                    return subIdValue >= 10000 && subIdValue != 20000;
                }
                return false;
            }
        }

        public MobTypeInfo(int id, string category, string subId, string name, string comment)
        {
            Id = id;
            Category = category;
            SubId = subId;
            Name = name;
            Comment = comment;
        }
    }
}
