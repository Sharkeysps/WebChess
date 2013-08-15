namespace Chess.Server.Models
{
    using Chess.Model;
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class GameModel
    {
        public static Expression<Func<Game, GameModel>> FromGame
        {
            get
            {
                return x => new GameModel() 
                {
                    Id=x.Id,
                    WhitePlayerId=x.WhitePlayerId,
                    BlackPlayerId=x.BlackPlayerId,
                    UserIdInTurn=x.UserIdInTurn,
                    Status=x.GameStatus.StatusName,
                    Name = x.Name
                };
            }
        }
        public static GameModel ConvertToModel(Game game)
        {
            return new GameModel()
            {
                Id = game.Id,
                WhitePlayerId = game.WhitePlayerId,
                BlackPlayerId = game.BlackPlayerId,
                UserIdInTurn = game.UserIdInTurn,
                Status = game.GameStatus.StatusName,
                Name = game.Name
            };
        }

        public int Id { get; set; }
        public int WhitePlayerId { get; set; }
        public int? BlackPlayerId { get; set; }
        public int? UserIdInTurn { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }

        internal void UpdateGame(Game game)
        {
            game.BlackPlayerId = this.BlackPlayerId;
            game.WhitePlayerId = this.WhitePlayerId;
            game.UserIdInTurn = this.UserIdInTurn;
            game.GameStatus.StatusName = this.Status;
        }
    }
}