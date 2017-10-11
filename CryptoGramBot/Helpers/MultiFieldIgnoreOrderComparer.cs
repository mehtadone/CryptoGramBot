using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoGramBot.Helpers
{
    public class MultiFieldIgnoreOrderComparer : IEquatable<IEnumerable<object>>, IEqualityComparer<IEnumerable<object>>
    {
        private IEnumerable<object> objects;

        public MultiFieldIgnoreOrderComparer(IEnumerable<object> objects)
        {
            this.objects = objects;
        }

        public bool Equals(IEnumerable<object> x, IEnumerable<object> y)
        {
            return x.All(y.Contains);
        }

        public override bool Equals(object obj)
        {
            MultiFieldIgnoreOrderComparer other = obj as MultiFieldIgnoreOrderComparer;
            if (other == null) return false;
            return this.Equals(this.objects, other.objects);
        }

        public bool Equals(IEnumerable<object> other)
        {
            return this.Equals(this.objects, other);
        }

        public int GetHashCode(IEnumerable<object> objects)
        {
            unchecked
            {
                int detailHash = 0;
                unchecked
                {
                    // order doesn't matter, so we need to order:
                    foreach (object obj in objects.OrderBy(x => x))
                        detailHash = 17 * detailHash + (obj == null ? 0 : obj.GetHashCode());
                }
                return detailHash;
            }
        }

        public override int GetHashCode()
        {
            return GetHashCode(this.objects);
        }
    }
}