namespace Chess.Server.Controllers
{
    using System.Net.Http;
    using Chess.Model;
    using Chess.Server.Models;
    using Chess.Server.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using Chess.Server.Resolvers;
    using Chess.Server.Controllers;
    using System.Net;

    public class UserController : ApiController
    {
        private AllRepositories<ChessEntities> data;

        public UserController(AllRepositories<ChessEntities> data)
        {
            this.data = data;
        }

        [HttpPost]
        [ActionName("register")]
        public HttpResponseMessage RegisterUser(UserRegisterModel user)
        {
            try
            {
                UserRepository userRepository = this.data.GetUserRepository();
                userRepository.CreateUser(user.Username, user.Nickname, user.AuthCode);
                string nickname = string.Empty;
                var sessionKey = userRepository.LoginUser(user.Username, user.AuthCode, out nickname);
                UserLoggedModel result = new UserLoggedModel()
                {
                    Nickname = user.Nickname,
                    SessionKey = sessionKey
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (InvalidOperationException e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpPost]
        [ActionName("login")]
        public HttpResponseMessage LoginUser(UserLoginModel user)
        {
            try
            {
                UserRepository userRepository = this.data.GetUserRepository();
                string nickname = string.Empty;
                var sessionKey = userRepository.LoginUser(user.Username, user.AuthCode, out nickname);
                UserLoggedModel result = new UserLoggedModel()
                {
                    Nickname = nickname,
                    SessionKey = sessionKey
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (InvalidOperationException e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message);
            }
        }

        [HttpGet]
        [ActionName("logout")]
        public HttpResponseMessage LogoutUser(string sessionKey)
        {
            try
            {
                UserRepository userRepository = data.GetUserRepository();
                userRepository.LogoutUser(sessionKey);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (InvalidOperationException e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message);
            }
        }
    }    
}
