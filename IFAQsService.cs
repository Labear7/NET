using Sabio.Models.Domain;
using Sabio.Models.Requests;
using System.Collections.Generic;

namespace Sabio.Services
{
    public interface IFAQsService
    {
        int Add(FAQsAddRequest request, int userId);
        void Delete(int id);
        List<FAQs> Get(int id);
        List<FAQs> GetAll();
        void Update(FAQsUpdateRequest request, int userId);
    }
}