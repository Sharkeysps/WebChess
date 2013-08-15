using Chess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Chess.Server.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public static Expression<Func<Message, MessageModel>> FromMessage
        {
            get
            {
                return x => new MessageModel()
                {
                    Id = x.Id,
                    Text=x.Text
                };
            }
        }
    }
}