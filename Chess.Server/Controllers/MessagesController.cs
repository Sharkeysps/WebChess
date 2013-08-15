namespace Chess.Server.Controllers
{
    using Chess.Model;
    using Chess.Server.Models;
    using Chess.Server.Repositories;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    public class MessagesController : ApiController
    {
        private AllRepositories<ChessEntities> data;

        public MessagesController(AllRepositories<ChessEntities> data)
        {
            this.data = data;
        }

        [HttpGet]
        [ActionName("all")]
        public IQueryable<MessageModel> GetAllMessages(string sessionKey)
        {
            //Find the user
            UserRepository userRepository = data.GetUserRepository();
            int userId = userRepository.LoginUser(sessionKey);
            if (userId == 0)
            {
                HttpResponseMessage response = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User not found");
                throw new HttpResponseException(response);
            }

            //Retrive his messages
            MessagesRepository messagesRepository = data.GetMessagesRepository();
            IQueryable<MessageModel> messages = messagesRepository.GetMessagesByUserId(userId).Select(MessageModel.FromMessage);
            if (messages == null)
            {
                HttpResponseMessage response = this.Request.CreateResponse(HttpStatusCode.BadRequest, "Bad request");
                throw new HttpResponseException(response);
            }

            return messages;
        }

        [HttpGet]
        [ActionName("unread")]
        public IQueryable<MessageModel> GetUnreadMessages(string sessionKey)
        {
            HttpResponseMessage response;
            UserRepository userRepository = data.GetUserRepository();
            int userId = userRepository.LoginUser(sessionKey);
            if (userId == 0)
            {
                response = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User not found");
                throw new HttpResponseException(response);
            }

            MessagesRepository messagesRepository = data.GetMessagesRepository();
            IQueryable<MessageModel> messages = messagesRepository.GetUreadMessagesByUserId(userId).Select(MessageModel.FromMessage);
            if (messages == null)
            {
                response = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Empty colection");

                throw new HttpResponseException(response);
            }

            return messages;
        }
    }
}
