using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models.Domain;
using Sabio.Models.Domain.Comments;
using Sabio.Models.Requests.CommentsRequests;
using Sabio.Services.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sabio.Services
{
    
    public class CommentsService : ICommentsService
    {
        IDataProvider _data = null;
        ILookUpService _lookUp = null;
        
        
        public CommentsService(IDataProvider data, ILookUpService lookUp)
        {
            _data = data;
            _lookUp = lookUp;
        }    

        public List<Comment> GetByEntityId(int EntityId, int EntityTypeId)
        {
            List<Comment> comments = null;

            string procName = "[dbo].[CommentsSelectByEntityId]";

            _data.ExecuteCmd(procName, inputParamMapper: delegate (SqlParameterCollection paramCol)
            {
                paramCol.AddWithValue("@EntityId", EntityId);
                paramCol.AddWithValue("@EntityTypeId", EntityTypeId);
            },
            singleRecordMapper: delegate (IDataReader reader, short set)
            {
                Comment comment = SingleCommentMapper(reader);

                if (comments == null)
                {
                    comments = new List<Comment>();
                }

                comments.Add(comment);
            });
            return comments;
        }

        public int Add(CommentAddRequest request, int authId)
        {
            int id = 0;


            string storedProc = "[dbo].[Comments_Insert]";

            _data.ExecuteNonQuery(storedProc, inputParamMapper: delegate
            (SqlParameterCollection collection)
            {
                AddCommonParams(request, collection);
                collection.AddWithValue("@CreatedBy", authId);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;
                collection.Add(idOut);
            },
            returnParameters: delegate (SqlParameterCollection returnedCollection)
            {
                object originalId = returnedCollection["@Id"].Value;
                int.TryParse(originalId.ToString(), out id);
            });
            return id;           
        }
        
        public void Update(CommentUpdateRequest request)
        {
            string procName = "[dbo].[CommentsUpdate]";
            _data.ExecuteNonQuery(procName,
            inputParamMapper: delegate (SqlParameterCollection collection)
            {
                collection.AddWithValue("Id", request.Id);
                AddCommonParams(request, collection);
            },
            returnParameters: null);
        }

        public void DeleteById(int id)
        {
            string procName = "[dbo].[CommentsDeleteById]";
            _data.ExecuteNonQuery(procName, inputParamMapper:
                delegate (SqlParameterCollection collection)
                {
                    collection.AddWithValue("@Id", id);                   
                },
                    returnParameters: null);           
        }
        
        private static void AddCommonParams(CommentAddRequest request, SqlParameterCollection collection)
        {
            collection.AddWithValue("@Subject", request.Subject);
            collection.AddWithValue("@Text", request.Text);
            collection.AddWithValue("@ParentId", request.ParentId);
            collection.AddWithValue("@EntityTypeId", request.EntityType);
            collection.AddWithValue("@EntityId", request.EntityId);
            collection.AddWithValue("@IsDeleted", request.IsDeleted);
        }

   private Comment SingleCommentMapper(IDataReader reader)
        {
            Comment aComment = new Comment();
            int index = 0;

            aComment.Id = reader.GetInt32(index++);
            aComment.Subject = reader.GetString(index++);
            aComment.Text = reader.GetString(index++);
            aComment.ParentId = reader.GetInt32(index++);
            aComment.EntityType = _lookUp.MapSingleLookUp(reader, ref index);
            aComment.EntityId = reader.GetInt32(index++);
            aComment.DateCreated = reader.GetDateTime(index++);
            aComment.DateModified = reader.GetDateTime(index++);
            aComment.CreatedBy = reader.DeserializeObject<BaseUser>(index++);
            aComment.IsDeleted = reader.GetBoolean(index++);
            return aComment;
        }


    }
}
