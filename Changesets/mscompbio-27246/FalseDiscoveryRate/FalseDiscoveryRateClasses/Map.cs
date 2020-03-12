using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FalseDiscoveryRateClasses
{
    public class Map<K,V> : Dictionary<K, V>
    {
        private List<K> m_lKeys;
        public List<K> KeyList
        {
            get
            {
                if ((m_lKeys == null) || (m_lKeys.Count != Keys.Count))
                    m_lKeys = new List<K>(Keys);
                return m_lKeys;
            }
        }


        public Map(IEqualityComparer<K> comp) : base( comp )
        {
            m_lKeys = null;
        }
        public Map() : base()
        {
            m_lKeys = null;
        }
    }
}
