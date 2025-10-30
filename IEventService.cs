using Sabio.Models;
using Sabio.Models.Domain.Events;
using Sabio.Models.Requests.Events;

namespace Sabio.Services.Events
{
    public interface IEventService
    {
        void Delete(int id);
        int EventsInsert(EventsAddRequest request, int authId);
        Paged<Event> EventsSelectAll(int pageIndex, int pageSize);
        Event EventsSelectById(int id);
        void EventsUpdate(EventsUpdateRequest updateRequest, int authId);
        Paged<Event> SelectByCreatedBy(int pageIndex, int pageSize, int CreatedBy);
    }
}