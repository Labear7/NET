using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models;
using Sabio.Models.Domain;
using Sabio.Models.Domain.Events;
using Sabio.Models.Requests.Events;
using Sabio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Sabio.Services.Events
{
    public class EventService : IEventService
    {
        #region DataProvider & LookUp Service
        IDataProvider _data = null;
        ILookUpService _lookup = null;

        public EventService(IDataProvider data, ILookUpService lookup)
        {
            _data = data;
            _lookup = lookup;
        }
        #endregion

        #region Insert
        public int EventsInsert(EventsAddRequest request, int authId)
        {
            int id = 0;
            string procName = "[dbo].[Events_Insert]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection collection)
                {
                    AddCommonParams(request, collection);
                    collection.AddWithValue("CreatedBy", authId);
                    collection.AddWithValue("@ModifiedBy", authId);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;

                    collection.Add(idOut);
                }, returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;
                    int.TryParse(oId.ToString(), out id);
                });
            return id;
        }
        #endregion

        #region EventsSelectAll
        public Paged<Event> EventsSelectAll(int pageIndex, int pageSize)
        {
            Paged<Event> pagedList = null;
            List<Event> list = null;
            int totalCount = 0;
            string procName = "[dbo].[Events_SelectAll_Paginated]";

            _data.ExecuteCmd(procName,
                delegate (SqlParameterCollection collection)
                {
                    collection.AddWithValue("@PageIndex", pageIndex);
                    collection.AddWithValue("@PageSize", pageSize);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    Event aEvent = MapEvent(reader, ref startingIndex);

                    totalCount = reader.GetSafeInt32(startingIndex++);

                    if (list == null)
                    {
                        list = new List<Event>();
                    }
                    list.Add(aEvent);
                });
            if (list != null)
            {
                pagedList = new Paged<Event>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }
        #endregion

        #region EventsSelectByCreatedBy
        public Paged<Event> SelectByCreatedBy(int pageIndex, int pageSize, int CreatedBy)
        {
            Paged<Event> pagedList = null;
            List<Event> list = null;
            int totalCount = 0;
            string procName = "[dbo].[Events_SelectByCreatedBy_Paginated]";

            _data.ExecuteCmd(procName,
                delegate (SqlParameterCollection collection)
                {
                    collection.AddWithValue("@PageIndex", pageIndex);
                    collection.AddWithValue("@PageSize", pageSize);
                    collection.AddWithValue("@CreatedBy", CreatedBy);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    Event aEvent = MapEvent(reader, ref startingIndex);

                    totalCount = reader.GetSafeInt32(startingIndex);

                    if (list == null)
                    {
                        list = new List<Event>();
                    }
                    list.Add(aEvent);
                });
            if (list != null)
            {
                pagedList = new Paged<Event>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        #endregion 

        #region SelectById
        public Event EventsSelectById(int id)
        {
            string procName = "[dbo].[Events_SelectById]";
            Event aEvent = null;
            _data.ExecuteCmd(procName, delegate (SqlParameterCollection collection)
            {
                collection.AddWithValue("@Id", id);
            }, delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                aEvent = MapEvent(reader, ref startingIndex);
            });
            return aEvent;
        }
        #endregion

        #region Update
        public void EventsUpdate(EventsUpdateRequest updateRequest, int authId)
        {
            string procName = "[dbo].[Events_Update]";
            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection collection)
                {
                    AddCommonParams(updateRequest, collection);
                    collection.AddWithValue("@ModifiedBy", authId);
                    collection.AddWithValue("@Id", updateRequest.Id);
                }, returnParameters: null);
        }
        #endregion

        #region Delete
        public void Delete(int id)
        {
            string procName = "[dbo].[Events_DeleteById]";
            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@Id", id);
                },
                returnParameters: null);
        }
        #endregion

        #region Common Params
        private static void AddCommonParams(EventsAddRequest request, SqlParameterCollection collection)
        {
            collection.AddWithValue("@EventTypeId", request.EventTypeId);
            collection.AddWithValue("@Name", request.Name);
            collection.AddWithValue("@Summary", request.Summary);
            collection.AddWithValue("@ShortDescription", request.ShortDescription);
            collection.AddWithValue("@VenueId", request.VenueId);
            collection.AddWithValue("@EventStatusId", request.EventStatusId);
            collection.AddWithValue("@ImageId", request.ImageId);
            collection.AddWithValue("@ExternalSiteUrl", request.ExternalSiteUrl);
            collection.AddWithValue("@isFree", request.isFree);
            collection.AddWithValue("@DateStart", request.DateStart);
            collection.AddWithValue("@DateEnd", request.DateEnd);
        }

        #endregion

        #region Mapper
        private Event MapEvent(IDataReader reader, ref int startingIndex)
        {
            Event aEvent = new Event();

            aEvent.Id = reader.GetSafeInt32(startingIndex++);
            aEvent.Name = reader.GetSafeString(startingIndex++);
            aEvent.Summary = reader.GetSafeString(startingIndex++);
            aEvent.ShortDescription = reader.GetSafeString(startingIndex++);
            aEvent.ImageUrl = reader.GetSafeString(startingIndex++);
            aEvent.ExternalSiteUrl = reader.GetString(startingIndex++);
            aEvent.isFree = reader.GetBoolean(startingIndex++);

            aEvent.EventType = _lookup.MapSingleLookUp(reader, ref startingIndex);
            aEvent.EventStatus = _lookup.MapSingleLookUp(reader, ref startingIndex);
            aEvent.Venue = _lookup.MapSingleLookUp(reader, ref startingIndex);

            aEvent.DateCreated = reader.GetDateTime(startingIndex++);
            aEvent.DateModified = reader.GetDateTime(startingIndex++);
            aEvent.DateStart = reader.GetDateTime(startingIndex++);
            aEvent.DateEnd = reader.GetDateTime(startingIndex++);
            aEvent.CreatedBy = reader.DeserializeObject<BaseUser>(startingIndex++);
            aEvent.ModifiedBy = reader.DeserializeObject<BaseUser>(startingIndex++);

            return aEvent;
        }
        #endregion

    }
}