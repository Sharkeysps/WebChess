namespace Chess.Server.Models
{
    using Chess.Model;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;

    [DataContract(Name = "Game")]
    public class OpenGameModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "creator")]
        public string Creator { get; set; }

        public static Expression<Func<Game, OpenGameModel>> FromGame
        {
            get
            {
                return x => new OpenGameModel()
                {
                    Id=x.Id,
                    Name=x.Name.Trim(),
                    Creator=x.User.Nickname.Trim()
                };
            }
        }
    }

}