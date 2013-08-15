namespace Chess.Server.Repositories
{
    using Chess.Model;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    public class MessagesRepository : EfRepository<Message>
    {
        protected const string UserMessageTypeGameStarted = "game-started";
        protected const string UserMessageTypeGameJoined = "game-joined";
        protected const string UserMessageTypeGameFinished = "game-finished";
        protected const string UserMessageTypeGameMove = "game-move";

        public MessagesRepository(DbContext context) : base(context)
        {
        }

        public IQueryable<Message> GetMessagesByUserId(int id)
        {
            List<Message> result = this.Context.Set<Message>().Where(x => x.UserId == id).ToList<Message>();

            for (int i = 0; i < result.Count; i++)
            {
                result[i].IsMsgRead = true;
            }
            /*foreach (var msg in result)
            {
                msg.IsMsgRead = true;
            }*/
            this.Context.SaveChanges();
            return result.AsQueryable();
        }

        public void CreateGameMessage(int gameId, int userId, string messageText, string messageType)
        {
            User user = this.Context.Set<User>().First(u => u.Id == userId);
            Game game = this.Context.Set<Game>().First(g => g.Id == gameId);

            var gameMoveMessageType = this.Context.Set<MessagesType>().First(mt => mt.TypeName == messageType);
            SendMessage(messageText, user, game, gameMoveMessageType);
            this.Context.SaveChanges();
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

        internal IQueryable<Message> GetUreadMessagesByUserId(int userId)
        {
            List<Message> result = this.Context.Set<Message>().Where(x => x.UserId == userId && x.IsMsgRead == false).ToList<Message>();

            for (int i = 0; i < result.Count; i++)
            {
                result[i].IsMsgRead = true;
            }
            /*foreach (var msg in result)
            {
                msg.IsMsgRead = true;                
            }*/
            this.Context.SaveChanges();

            return result.AsQueryable();
        }
    }
}