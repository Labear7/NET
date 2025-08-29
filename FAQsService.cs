using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models.Domain;
using Sabio.Models.Requests;
using Sabio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabio.Services
{
    public class FAQsService : IFAQsService
    {
        IDataProvider _data = null;
        ILookUpService _lookUp = null;

        public FAQsService(IDataProvider data, ILookUpService lookUp)
        {
            _lookUp = lookUp;
            _data = data;
        }

        public int Add(FAQsAddRequest request, int userId)
        {
            int id = 0;
            string procName = "[dbo].[FAQs_Insert]";

            _data.ExecuteNonQuery(procName, inputParamMapper:
                delegate (SqlParameterCollection collection)
                {
                    AddCommonParams(request, collection);
                    collection.AddWithValue("@CreatedBy", userId);
                    collection.AddWithValue("@ModifiedBy", userId);

                    SqlParameter idOut = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                    idOut.Direction = System.Data.ParameterDirection.Output;
                    collection.Add(idOut);
                },
                returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object originalId = returnCollection["@Id"].Value;
                    Int32.TryParse(originalId.ToString(), out id);
                });
            return id;
        }
        public List<FAQs> GetAll()
        {
            string procName = "[dbo].[FAQs_SelectAll]";

            List<FAQs> faqsList = null;

            _data.ExecuteCmd(procName, inputParamMapper: null,
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    FAQs faqs = FAQsMapper(reader);

                    if (faqsList == null)
                    {
                        faqsList = new List<FAQs>();
                    }
                    faqsList.Add(faqs);
                });
            return faqsList;
        }
        public List<FAQs> Get(int id)
        {
            List<FAQs> faqsList = null;

            string procName = "[dbo].[FAQs_Select_ByCategoryId]";
            _data.ExecuteCmd(procName, inputParamMapper:
                delegate (SqlParameterCollection collection)
                {
                    collection.AddWithValue("@CategoryId", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    FAQs faqs = FAQsMapper(reader);

                    if (faqsList == null)
                    {
                        faqsList = new List<FAQs>();
                    }
                    faqsList.Add(faqs);
                });
            return faqsList;
        }

        public void Update(FAQsUpdateRequest request, int userId)
        {
            string procName = "[dbo].[FAQs_Update]";
            _data.ExecuteNonQuery(procName, inputParamMapper:
                delegate (SqlParameterCollection collection)
                {
                    AddCommonParams(request, collection);
                    collection.AddWithValue("@ModifiedBy", userId);
                    collection.AddWithValue("@Id", request.Id);
                },
                returnParameters: null);
        }

        public void Delete(int id)
        {
            string procName = "[dbo].[FAQs_Delete_ById]";
            _data.ExecuteNonQuery(procName, inputParamMapper:
                delegate (SqlParameterCollection collection)
                {
                    collection.AddWithValue("@Id", id);
                },
                returnParameters: null);
        }

        private FAQs FAQsMapper(IDataReader reader)
        {
            FAQs faqs = new FAQs();
            int index = 0;

            faqs.Id = reader.GetSafeInt32(index++);
            faqs.Question = reader.GetSafeString(index++);
            faqs.Answer = reader.GetSafeString(index++);
            faqs.CategoryId = _lookUp.MapSingleLookUp(reader, ref index);
            faqs.SortOrder = reader.GetSafeInt32(index++);
            faqs.DateCreated = reader.GetSafeDateTime(index++);
            faqs.DateModified = reader.GetSafeDateTime(index++);
            faqs.CreatedBy = reader.DeserializeObject<BaseUser>(index++);
            faqs.ModifiedBy = reader.DeserializeObject<BaseUser>(index++);
            return faqs;
        }

        private static void AddCommonParams(FAQsAddRequest request, SqlParameterCollection collection)
        {
            collection.AddWithValue("@Question", request.Question);
            collection.AddWithValue("@Answer", request.Answer);
            collection.AddWithValue("@CategoryId", request.CategoryId);
            collection.AddWithValue("@SortOrder", request.SortOrder);
        }


    }
}
