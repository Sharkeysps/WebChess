namespace Chess.Server.Controllers
{
    using Chess.Model;
    using Chess.Server.Models;
    using Chess.Server.Repositories;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    public class GameController : ApiController
    {
        private AllRepositories<ChessEntities> data;

        public GameController(AllRepositories<ChessEntities> data)
        {
            this.data = data;
        }

        [HttpPost]
        [ActionName("create")]
        public HttpResponseMessage CreateGame(string sessionKey, [FromBody]
                                              GameModel gameModel)
        {
            UserRepository userRepository = data.GetUserRepository();
            var userId = userRepository.LoginUser(sessionKey);

            GameRepository gameRepository = data.GetGameRepository();
            var game = gameRepository.CreateJoinGame(userId, gameModel.Name);

            if (game == null)
            {
                //FAIL
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "No such game.");
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, "Game success");
        }

        

        [HttpGet]
        [ActionName("start")]
        public HttpResponseMessage StartGame(string sessionKey, int gameId)
        {
            UserRepository userRepository = data.GetUserRepository();
            var userId = userRepository.LoginUser(sessionKey); // Check if the user is logged

            GameRepository gameRepository = data.GetGameRepository();
            var game = gameRepository.StartGame(gameId);

            if (game == null)
            {
                //FAIL
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "No such game.");
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, "Game started successfully");
        }

        [HttpGet]
        [ActionName("open")]
        public HttpResponseMessage GetOpenGames(string sessionKey)
        {
            UserRepository userRepository = data.GetUserRepository();
            var userId = userRepository.LoginUser(sessionKey);

            GameRepository gameRepository = data.GetGameRepository();
            var games = gameRepository.GetOpenGames(userId);

            if (games == null)
            {
                //FAIL
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "No such games.");
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, games);
        }

        [HttpGet]
        [ActionName("my-active")]
        public HttpResponseMessage GetActiveGames(string sessionKey)
        {
            UserRepository userRepository = data.GetUserRepository();
            var userId = userRepository.LoginUser(sessionKey);

            GameRepository gameRepository = data.GetGameRepository();
            var games = gameRepository.GetActiveGames(userId);

            if (games == null)
            {
                //FAIL
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "No such games.");
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, games);
        }

        [HttpGet]
        [ActionName("field")]
        public HttpResponseMessage GetBattleField(string sessionKey, int gameId)
        {
            try
            {
                UserRepository userRepository = data.GetUserRepository();
                var userId = userRepository.LoginUser(sessionKey);

                GameRepository gameRepository = data.GetGameRepository();
                FieldModel gameField = gameRepository.GetBattleField(gameId);

                return Request.CreateResponse(HttpStatusCode.OK, gameField);
            }
            catch (InvalidOperationException e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message);
            }
        }



        /*
        // GET api/game/
        public IQueryable<GameModel> Get()
        {
            return data.gameRepo.All().Select(GameModel.FromGame);
        }

        // GET api/game/5
        public GameModel Get(int id)
        {
            return GameModel.ConvertToModel(data.gameRepo.Get(id));
        }

        
        // GET api/game/create
        public GameModel GetCreate()
        {
            return GameModel.ConvertToModel(this.data.gameRepo.FindNewGame());
        }

        // POST api/game
        public void Post([FromBody]GameModel value)
        {
            throw new NotImplementedException();
        }

        // PUT api/game/5
        public void Put(int id, [FromBody]GameModel value)
        {
            var game = this.data.gameRepo.Get(id);
            value.UpdateGame(game);
        }

        // DELETE api/game/5
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }*/
    }
}
