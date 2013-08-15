namespace Chess.Server.Repositories
{
    using Chess.Model;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    public class MessagesRepository : EfRepository<Message>
    {
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