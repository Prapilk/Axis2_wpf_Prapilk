using System.Collections.Generic;

namespace Axis2.WPF
{
    public class IndexDataAnimation
    {
        private List<IndexDataAnimationGroupUOP?> _uopGroups;
        public byte[] m_UopReplaceGroupIndex { get; private set; }

        public IndexDataAnimation()
        {
            _uopGroups = new List<IndexDataAnimationGroupUOP?>();
            m_UopReplaceGroupIndex = new byte[Constants.ANIMATION_GROUPS_COUNT];
        }

        public IndexDataAnimationGroupUOP AddUopGroup(int groupIndex, IndexDataAnimationGroupUOP group)
        {
            // Ensure the list is large enough to contain the group at groupIndex
            while (_uopGroups.Count <= groupIndex)
            {
                _uopGroups.Add(null); // Add placeholders if needed
            }
            _uopGroups[groupIndex] = group;
            return group;
        }

        public IndexDataAnimationGroupUOP? GetUopGroup(int groupIndex, bool createIfNull)
        {
            if (groupIndex < _uopGroups.Count && _uopGroups[groupIndex] != null)
            {
                return _uopGroups[groupIndex];
            }
            else if (createIfNull)
            {
                IndexDataAnimationGroupUOP newGroup = new IndexDataAnimationGroupUOP();
                AddUopGroup(groupIndex, newGroup);
                return newGroup;
            }
            return null;
        }
    }
}