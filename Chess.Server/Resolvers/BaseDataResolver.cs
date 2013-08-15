using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Chess.Model;
using Chess.Server.Models;

namespace Chess.Server.Resolvers
{
    public class BaseDataResolver
    {
        protected const int Sha1CodeLength = 40;

        protected const string GameStatusOpen = "open";
        protected const string GameStatusFull = "full";
        protected const string GameStatusInProgress = "in-progress";
        protected const string GameStatusFinished = "finished";

        protected static Random rand = new Random();
        protected const string MessageStateUnread = "unread";

        protected const string UserMessageTypeGameStarted = "game-started";
        protected const string UserMessageTypeGameJoined = "game-joined";
        protected const string UserMessageTypeGameFinished = "game-finished";
        protected const string UserMessageTypeGameMove = "game-move";

        protected static User GetUser(int userId, ChessEntities context)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new ServerErrorException("Invalid user", "ERR_INV_USR");
            }
            return user;
        }

        protected static Game GetGame(int gameId, ChessEntities context)
        {
            var game = context.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null)
            {
                throw new ServerErrorException("No such game", "ERR_INV_GAME");
            }
            return game;
        }

        protected static void ValidateOpenGameStatus(Game game, ChessEntities context)
        {
            if (game.GameStatus.StatusName == GameStatusInProgress)
            {
                throw new ServerErrorException("Game has already been started", "INV_GAME_STATE");
            }
            else if (game.GameStatus.StatusName == GameStatusFinished)
            {
                throw new ServerErrorException("Game is already finished", "INV_GAME_STATE");
            }
            else if (game.GameStatus.StatusName == GameStatusFull)
            {
                throw new ServerErrorException("Game is not started yet", "INV_GAME_STATE");
            }
        }

        protected static void ValidateGameInProgressStatus(Game game, ChessEntities context)
        {
            if (game.GameStatus.StatusName == GameStatusOpen)
            {
                throw new ServerErrorException("Game not yet full", "INV_GAME_STATE");
            }
            else if (game.GameStatus.StatusName == GameStatusFinished)
            {
                throw new ServerErrorException("Game is already finished", "INV_GAME_STATE");
            }
            else if (game.GameStatus.StatusName == GameStatusFull)
            {
                throw new ServerErrorException("Game is not started yet", "INV_GAME_STATE");
            }
        }
    }
}