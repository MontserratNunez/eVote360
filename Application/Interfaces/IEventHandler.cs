using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Core.Application.Interfaces
{
    public interface IEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent domainEvent);
    }
}
