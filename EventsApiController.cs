using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sabio.Models;
using Sabio.Models.Domain.Events;
using Sabio.Models.Requests.Events;
using Sabio.Services;
using Sabio.Services.Events;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using System;

namespace Sabio.Web.Api.Controllers.Events
{
    [Route("api/events")]
    [ApiController]
    public class EventsApiController : BaseApiController
    {
        #region Services Declarations
        private IEventService _service = null;
        private IAuthenticationService<int> _authService = null;

        public EventsApiController(IEventService eventsService, ILogger<EventsApiController> logger, IAuthenticationService<int> authService) : base(logger)
        {
            _service = eventsService;
            _authService = authService;
        }
        #endregion

        #region EventsSelectAll
        [HttpGet("paginate")]   
        public ActionResult<ItemResponse<Paged<Event>>> EventsSelectAll(int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try      
            {
                Paged<Event> paged = _service.EventsSelectAll(pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("No records found"));
                }
                else
                {
                    ItemResponse<Paged<Event>> response = new ItemResponse<Paged<Event>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }
        #endregion      

        #region EventsSelectById
        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<Event>> GetById(int id)
        {

            int code = 200;
            BaseResponse response = null;

            try
            {
                Event aEvent = _service.EventsSelectById(id);

                if (aEvent == null)
                {
                    code = 404;
                    response = new ErrorResponse("Event not found");
                }
                else
                {
                    response = new ItemResponse<Event> { Item = aEvent };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }
        #endregion

        #region EventsDelete
        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.Delete(id);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }
        #endregion

        #region EventsInsert
        [HttpPost]
        public ActionResult<ItemResponse<int>> EventsInsert(EventsAddRequest request)
        {
            ObjectResult result = null;

            try
            {
                int authId = _authService.GetCurrentUserId();

                int id = _service.EventsInsert(request, authId);

                ItemResponse<int> response = new ItemResponse<int>() { Item = id };

                result = Created201(response);
            }
            catch (Exception ex)
            {
                base.Logger.LogError(ex.ToString());
                ErrorResponse response = new ErrorResponse(ex.Message);

                result = StatusCode(500, response);
            }
            return result;
        }

        #endregion
                 
        #region EventsUpdate 
        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> EventsUpdate(EventsUpdateRequest model)
        {

            int code = 200;
            BaseResponse response = null;
            try
            {
                int authId = _authService.GetCurrentUserId();
                _service.EventsUpdate(model, authId);
                response = new SuccessResponse();

            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }
        #endregion

        #region Events_Select_ByCreatedBy
        [HttpGet("paginate/createdby")]
        public ActionResult<ItemResponse<Paged<Event>>> EventsSelectByCreatedBy(int pageIndex, int pageSize, int createdby)
        {
            ActionResult result = null;
            try
            {
                Paged<Event> paged = _service.SelectByCreatedBy(pageIndex, pageSize, createdby);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("No events found"));
                }
                else
                {
                    ItemResponse<Paged<Event>> response = new ItemResponse<Paged<Event>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }
        #endregion
    }
}                 
