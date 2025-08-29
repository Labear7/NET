using Sabio.Models.Domain.Comments;
using Sabio.Models.Requests.CommentsRequests;
using System.Collections.Generic;
using System.Data;

namespace Sabio.Services
{
    public interface ICommentsService
    {
        List<Comment> GetByEntityId(int EntityId, int EntityTypeId);

        int Add(CommentAddRequest request, int authId);

        void Update(CommentUpdateRequest request);

        void DeleteById(int id);
        
    }
}