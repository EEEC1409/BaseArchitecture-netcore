using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.Common
{
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; }

        public override bool Equals(object obj)
        {
            if (obj is not Entity<TId> other) return false;
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
