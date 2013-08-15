namespace Chess.Server.Controllers
{
    using Chess.Model;
    using Chess.Server.Models;
    using Chess.Server.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    public class FigureController : ApiController
    {
        private AllRepositories<ChessEntities> data;
        protected const string UserMessageTypeGameMove = "game-move";

        public FigureController(AllRepositories<ChessEntities> data)
        {
            this.data = data;
        }

        // GET api/figure/5

        [HttpGet]
        [ActionName("Figure")]
        public List<FigureModel> GetFigure(string sessionKey, int gameId)
        {
            //hard code
          //  return this.data.figureRepository.GetGameFigures(gameId);
            FigureRepository figureRepository = data.GetFigureRepository();
            var figures = figureRepository.GetGameFigures(gameId);
            List<FigureModel> returnList = new List<FigureModel>();
            foreach (var figure in figures)
            {
                var checkListForDuplicate = returnList.Find(x => x.PosCol == figure.PositionCol && x.PosRow == figure.PositionRow);

                if (figure.GameId == gameId && checkListForDuplicate == null)
                {
                    returnList.Add(FigureModel.ConvertToModel(figure));
                }
            }
            return returnList;
        }



        //Sql has these parameters that will change the position of one of the pawns
        //In the database the figure should be with coordinates row:2 col:3 if you want this to work
        //   {
        // "figureId": 4,
        //"position": { "row": 3, "col": 3 }
        //}


        [HttpPost]
        [ActionName("Move")]
        public HttpResponseMessage PostMove(string sessionKey, int gameId, [FromBody]
                                               MoveModel move)
        {
            UserRepository userRepository = data.GetUserRepository();
            Chess.Model.User user = userRepository.GetUserBySessionKey(sessionKey);
            if (user == null)
            {
                var httpError = new HttpError("No such user found");
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, httpError);
            }
            var allowedMove = PerformMove(user.Id, gameId, move.FigureId,
                 move.Position.Row, move.Position.Col);

            if (allowedMove)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, "Move post success");
            }
            else
            {
                var httpError = new HttpError("BAD MOVE");
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, httpError);
            }


        }

        // PUT api/figure/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/figure/5
        public void Delete(int id)
        {
        }

        private bool PerformMove(int userId, int gameId, int figureId, int toRow, int toCol)
        {
            GameRepository gameRepository = data.GetGameRepository();
            Game game = gameRepository.Get(gameId);

            if (game.UserIdInTurn != userId)
            {
                return false;
            }

            FigureRepository figureRepository = data.GetFigureRepository();
            Figure figure = figureRepository.Get(figureId);

            if (!ValidateMove(figure, toRow, toCol))
            {
                return false;
            }

            Figure hitFigure = figureRepository.GetFigureByGameAndPosition(gameId, toRow, toCol);
            if (hitFigure != null && hitFigure.IsWhite != figure.IsWhite)
            {
                figureRepository.Delete(hitFigure);
            }
            else if (hitFigure != null && hitFigure.IsWhite == figure.IsWhite)
            {
                return false;
            }

            figure.PositionRow = toRow;
            figure.PositionCol = toCol;
            figureRepository.Update(figureId, figure);
            
            if (game.WhitePlayerId == game.UserIdInTurn)
            {
                game.UserIdInTurn = game.BlackPlayerId;
            }
            else
            {
                game.UserIdInTurn = game.WhitePlayerId;
            }

            gameRepository.Update(game.Id, game);

            UserRepository userRepository = data.GetUserRepository();
            User userInTurn = userRepository.Get(game.UserIdInTurn.GetValueOrDefault(0));

            var gameStartedMessageText = string.Format("{0} it is your move {1}", userInTurn.Nickname, game.Name);

            MessagesRepository messageRepository = data.GetMessagesRepository();
            messageRepository.CreateGameMessage(game.Id, game.UserIdInTurn.GetValueOrDefault(0), gameStartedMessageText, UserMessageTypeGameMove);
            
            return true;
        }

        protected void SendMessage(string text, User toUser, Game game, MessagesType msgType)
        {
            game.Messages.Add(new Message()
            {
                UserId = toUser.Id,
                MessagesType = msgType,
                IsMsgRead = false,
                Text = text
            });
        }

        private bool ValidateMove(Figure figure, int toRow, int toCol)
        {
            PositionModel toPosition = new PositionModel() { Row = toRow, Col = toCol };
            PositionModel fromPosition = new PositionModel() { Row = figure.PositionRow, Col = figure.PositionCol };

            if (toRow > 8 || toCol > 8)
            {
                return false;
            }

            if (toRow < 0 || toCol < 0)
            {
                return false;
            }

            var currentFigure = FigureFactory.GetFigure(figure);

            List<PositionModel> possibleMoves = currentFigure.GetPossibleMoves();
            List<PositionModel> possibleHits = currentFigure.GetPossibleHits();

            var checkedPosMoves = possibleMoves.Find(x => x.Col == toPosition.Col && x.Row == toPosition.Row);
            var checkedPosHits = possibleHits.Find(x => x.Col == toPosition.Col && x.Row == toPosition.Row);

            if (checkedPosMoves == null && checkedPosHits == null)
            {
                return false;
            }


            if (!currentFigure.CanJump())
            {
                FigureRepository figureRepository = data.GetFigureRepository();

                HashSet<PositionModel> movePath = GetMovePath(fromPosition, toPosition);
                IQueryable<Figure> allGameFigures = figureRepository.GetGameFigures(figure.GameId);

                HashSet<PositionModel> gameFiguresPositions = new HashSet<PositionModel>();
                foreach (Figure gameFig in allGameFigures)
                {
                    gameFiguresPositions.Add(new PositionModel()
                    {
                        Row = gameFig.PositionRow,
                        Col = gameFig.PositionCol
                    });
                }

                var positionIntersection = movePath.Intersect<PositionModel>(gameFiguresPositions);

                if (positionIntersection.Count() > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private HashSet<PositionModel> GetMovePath(PositionModel fromPosition, PositionModel toPosition)
        {
            HashSet<PositionModel> movePath = new HashSet<PositionModel>();

            if ((fromPosition.Col > toPosition.Col && fromPosition.Row < toPosition.Row) ||
                (fromPosition.Col < toPosition.Col && fromPosition.Row > toPosition.Row))
            {
                int startCol = Math.Min(fromPosition.Col, toPosition.Col);
                int endCol = Math.Max(fromPosition.Col, toPosition.Col);
                int startRow = Math.Max(fromPosition.Row, toPosition.Row);
                int endRow = Math.Min(fromPosition.Row, toPosition.Row);

                int colDirection = 1;
                int rowDirection = -1;

                int numberOfSteps = Math.Abs(endCol - startCol);

                for (int i = 1; i < numberOfSteps; i++)
                {
                    startCol += colDirection;
                    startRow += rowDirection;
                    PositionModel newPosition = new PositionModel()
                    {
                        Col = startCol,
                        Row = startRow
                    };

                    movePath.Add(newPosition);
                }
            }
            else
            {
                int startCol = Math.Min(fromPosition.Col, toPosition.Col);
                int endCol = Math.Max(fromPosition.Col, toPosition.Col);
                int startRow = Math.Min(fromPosition.Row, toPosition.Row);
                int endRow = Math.Max(fromPosition.Row, toPosition.Row);

                int colDirection = 0;
                if (startCol != endCol)
                {
                    colDirection = 1;
                }

                int rowDirection = 0;
                if (startRow != endRow)
                {
                    rowDirection = 1;
                }

                int numberOfSteps = Math.Max(endCol - startCol, endRow - startRow);

                for (int i = 1; i < numberOfSteps; i++)
                {
                    startCol += colDirection;
                    startRow += rowDirection;
                    PositionModel newPosition = new PositionModel()
                    {
                        Col = startCol,
                        Row = startRow
                    };

                    movePath.Add(newPosition);
                }
            }

            return movePath;
        }

    }
}
