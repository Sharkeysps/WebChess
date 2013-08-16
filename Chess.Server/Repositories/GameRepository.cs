namespace Chess.Server.Repositories
{
    using System.Data.Entity;
    using Chess.Model;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Chess.Server.Models;

    public class GameRepository : EfRepository<Game>
    {
        protected const string GameStatusOpen = "Pending";
        protected const string GameStatusFull = "Active";
        protected const string GameStatusFinished = "Finished";

        protected const string UserMessageTypeGameStarted = "game-started";
        protected const string UserMessageTypeGameJoined = "game-joined";
        protected const string UserMessageTypeGameFinished = "game-finished";
        protected const string UserMessageTypeGameMove = "game-move";

        public GameRepository(DbContext context)
            : base(context)
        {
        }

        public Game CreateJoinGame(int userId, string name)
        {
            Game game = this.DbSet.FirstOrDefault(x => x.GameStatus.StatusName == "Pending");
            if (game != null && game.WhitePlayerId != userId)
            {
                game.StatusId = this.Context.Set<GameStatus>().First(x => x.StatusName == "Active").Id;
                game.BlackPlayerId = userId;
                this.Update(game.Id, game);
            }
            else
            {
                game = new Game()
                {
                    StatusId = this.Context.Set<GameStatus>().First(x => x.StatusName == "Pending").Id,
                    WhitePlayerId = userId,
                    Name = name
                };
                this.Add(game);
            }

            return game;
        }

        public IEnumerable<OpenGameModel> GetOpenGames(int userId)
        {
            IEnumerable<OpenGameModel> openGames = this.Context.Set<Game>().Include("GameStatus").
               Where(x => x.WhitePlayerId == userId && x.GameStatus.StatusName == "Pending").
               Select(OpenGameModel.FromGame);
            if (openGames == null)
            {
                openGames = new List<OpenGameModel>();
            }
            return openGames;

            //User user = this.Context.Set<User>().FirstOrDefault(x => x.Id == userId);
            //GameStatus openStatus = this.Context.Set<GameStatus>().FirstOrDefault(st => st.StatusName == "Pending");
            //IEnumerable<OpenGameModel> openGames;

            //if (openStatus.Games.Any())
            //{
            //    openGames =
            //               from game in openStatus.Games
            //               where game.WhitePlayerId == userId
            //               select new OpenGameModel()
            //               {
            //                   Id = game.Id,
            //                   Name = game.Name,
            //                   Creator = this.Context.Set<User>().FirstOrDefault(x => x.Id == game.WhitePlayerId).Nickname
            //               };
            //}
            //else
            //{
            //    openGames = new List<OpenGameModel>();
            //}
            //return openGames.ToList();
        }

        public IEnumerable<ActiveGameModel> GetActiveGames(int userId)
        {
            //var user = this.Context.Set<User>().FirstOrDefault(x => x.Id == userId);
            var games = this.Context.Set<Game>();
            var openStatus = this.Context.Set<GameStatus>().FirstOrDefault(st => st.StatusName == "Active");
            var inProgressStatus = this.Context.Set<GameStatus>().FirstOrDefault(st => st.StatusName == "InProgress");
            IEnumerable<ActiveGameModel> activeGames = new List<ActiveGameModel>();

            activeGames =
                       from game in games
                       where (game.WhitePlayerId == userId || game.BlackPlayerId == userId) && (game.StatusId == openStatus.Id || game.StatusId == inProgressStatus.Id)
                       select new ActiveGameModel()
                       {
                           Id = game.Id,
                           Name = game.Name.Trim(),
                           Creator = this.Context.Set<User>().FirstOrDefault(x => x.Id == game.WhitePlayerId).Nickname.Trim(),
                           Status = game.GameStatus.StatusName.Trim()
                       };


            return activeGames.ToList();
        }

        public Game StartGame(int gameId)
        {
            DbContext context = this.Context;

            Game game = GetGame(gameId);
            if (game.GameStatus.StatusName == "Active")
            {
                throw new InvalidOperationException("Nobody has joined this game");
            }

            var whiteUnits = GenerateUnits(gameId, true);
            var blackUnits = GenerateUnits(gameId, false);

            game.StatusId = this.Context.Set<GameStatus>().First(s => s.StatusName == "InProgress").Id;
            game.UserIdInTurn = game.WhitePlayerId;

            User whiteUser = this.Context.Set<User>().FirstOrDefault(u => u.Id == game.WhitePlayerId);
            User blackUser = this.Context.Set<User>().FirstOrDefault(u => u.Id == game.BlackPlayerId);

            var gameStartedMessageText = string.Format("{0} just started game {1}", whiteUser.Nickname, game.Name);
            var gameStartedMessageType = context.Set<MessagesType>().First(mt => mt.TypeName == UserMessageTypeGameStarted);
            SendMessage(gameStartedMessageText, blackUser, game, gameStartedMessageType);

            var gameMoveMessageText = string.Format("It is your turn in game {0}", game.Name);
            var gameMoveMessageType = context.Set<MessagesType>().First(mt => mt.TypeName == UserMessageTypeGameMove);
            SendMessage(gameMoveMessageText, whiteUser, game, gameMoveMessageType);

            context.SaveChanges();

            return game;
        }

        public FieldModel GetBattleField(int gameId)
        {
            var context = this.Context;
            Game game = GetGame(gameId);
            ValidateGameInProgressStatus(game);
            User whitePlayer = context.Set<User>().First(u => u.Id == game.WhitePlayerId);
            User blackPlayer = context.Set<User>().First(u => u.Id == game.BlackPlayerId);

            var whiteUnitModels = GetUserUnits(whitePlayer, game);
            var blackUnitModels = GetUserUnits(blackPlayer, game);

            var gameField = new FieldModel()
            {
                GameId = game.Id,
                Title = game.Name.Trim(),
                White = new UserInGameModel()
                {
                    Nickname = whitePlayer.Nickname.Trim(),
                    Units = whiteUnitModels
                },
                Black = new UserInGameModel()
                {
                    Nickname = blackPlayer.Nickname.Trim(),
                    Units = blackUnitModels
                },
                Turn = game.UserIdInTurn.GetValueOrDefault(0),
                PlayerInTurn = (game.UserIdInTurn == game.WhitePlayerId) ? "white" : "black"
            };
            return gameField;
        }

        protected static IEnumerable<UnitModel> GetUserUnits(User owner, Game game)
        {
            bool isOwnerWhite = game.WhitePlayerId == owner.Id;
            var unitModels =
                            from figure in game.Figures
                            where figure.IsWhite == isOwnerWhite
                            select new UnitModel()
                            {
                                Id = figure.Id,
                                Type = figure.FigureType.TypeName.Trim(),
                                IsWhite = isOwnerWhite,
                                Position = new PositionModel()
                                {
                                    Col = figure.PositionCol,
                                    Row = figure.PositionRow
                                }
                            };
            return unitModels.ToList();
        }

        protected static void ValidateGameInProgressStatus(Game game)
        {
            if (game.GameStatus.StatusName == GameStatusOpen)
            {
                throw new InvalidOperationException("Game not yet full");
            }
            else if (game.GameStatus.StatusName == GameStatusFinished)
            {
                throw new InvalidOperationException("Game is already finished");
            }
            else if (game.GameStatus.StatusName == GameStatusFull)
            {
                throw new InvalidOperationException("Game is not started yet");
            }
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

        private IEnumerable<Figure> GenerateUnits(int gameId, bool isWhite)
        {
            var figures = this.Context.Set<Figure>();
            var figureTypes = this.Context.Set<FigureType>();
            int RookId = figureTypes.First(f => f.TypeName == "Rook").Id;
            int KnightId = figureTypes.First(f => f.TypeName == "Knight").Id;
            int BishopId = figureTypes.First(f => f.TypeName == "Bishop").Id;
            int KingId = figureTypes.First(f => f.TypeName == "King").Id;
            int QueenId = figureTypes.First(f => f.TypeName == "Queen").Id;
            int PawnId = figureTypes.First(f => f.TypeName == "Pawn").Id;

            List<Figure> generatedUnits = new List<Figure>();

            int pawnRow = 7;
            int mainForceRow = 8;
            if (isWhite)
            {
                pawnRow = 2;
                mainForceRow = 1;
            }

            for (int i = 1; i < 9; i++)
            {
                // Pawn
                Figure pawn = new Figure()
                {
                    PositionRow = pawnRow,
                    PositionCol = i,
                    TypeId = PawnId,
                    IsWhite = isWhite,
                    GameId = gameId,
                    IsMoved = false
                };
                figures.Add(pawn);
                generatedUnits.Add(pawn);
            }

            // Left Rook
            Figure leftRook = new Figure()
            {
                PositionRow = mainForceRow,
                PositionCol = 1,
                TypeId = RookId,
                IsWhite = isWhite,
                GameId = gameId,
                IsMoved = false
            };
            figures.Add(leftRook);
            generatedUnits.Add(leftRook);

            // Left Knight
            Figure leftKnight = new Figure()
            {
                PositionRow = mainForceRow,
                PositionCol = 2,
                TypeId = KnightId,
                IsWhite = isWhite,
                GameId = gameId,
                IsMoved = false
            };
            figures.Add(leftKnight);
            generatedUnits.Add(leftKnight);

            // Left Bishop
            Figure leftBishop = new Figure()
            {
                PositionRow = mainForceRow,
                PositionCol = 3,
                TypeId = BishopId,
                IsWhite = isWhite,
                GameId = gameId,
                IsMoved = false
            };
            figures.Add(leftBishop);
            generatedUnits.Add(leftBishop);

            // King
            Figure king = new Figure()
            {
                PositionRow = mainForceRow,
                PositionCol = 4,
                TypeId = KingId,
                IsWhite = isWhite,
                GameId = gameId,
                IsMoved = false
            };
            figures.Add(king);
            generatedUnits.Add(king);

            // Queen
            Figure queen = new Figure()
            {
                PositionRow = mainForceRow,
                PositionCol = 5,
                TypeId = QueenId,
                IsWhite = isWhite,
                GameId = gameId,
                IsMoved = false
            };
            figures.Add(queen);
            generatedUnits.Add(queen);

            // Right Bishop
            Figure rightBishop = new Figure()
            {
                PositionRow = mainForceRow,
                PositionCol = 6,
                TypeId = BishopId,
                IsWhite = isWhite,
                GameId = gameId,
                IsMoved = false
            };
            figures.Add(rightBishop);
            generatedUnits.Add(rightBishop);

            // Right Knight
            Figure rightKnight = new Figure()
            {
                PositionRow = mainForceRow,
                PositionCol = 7,
                TypeId = KnightId,
                IsWhite = isWhite,
                GameId = gameId,
                IsMoved = false
            };
            figures.Add(rightKnight);
            generatedUnits.Add(rightKnight);

            // Right Rook
            Figure rightRook = new Figure()
            {
                PositionRow = mainForceRow,
                PositionCol = 8,
                TypeId = RookId,
                IsWhite = isWhite,
                GameId = gameId,
                IsMoved = false
            };
            figures.Add(rightRook);
            generatedUnits.Add(rightRook);

            this.Context.SaveChanges();

            return generatedUnits;
        }

        protected Game GetGame(int gameId)
        {
            var game = this.Context.Set<Game>().FirstOrDefault(g => g.Id == gameId);
            if (game == null)
            {
                throw new InvalidOperationException("No such game");
            }
            return game;
        }
    }
}