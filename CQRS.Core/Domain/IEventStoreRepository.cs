using CQRS.Core.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQRS.Core.Domain
{
    public interface IEventStoreRepository
    {
        Task SaveAsync(EventModel @event);
        Task<List<EventModel>> FindByAggregateId(Guid aggregateId);
    }
}