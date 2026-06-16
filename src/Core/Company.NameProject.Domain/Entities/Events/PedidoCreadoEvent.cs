using Company.NameProject.Domain.ValueObjects;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.NameProject.Domain.Entities.Events
{
    public record PedidoCreadoEvent(Guid PedidoId, decimal Total) : INotification;
}
