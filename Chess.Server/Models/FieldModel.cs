using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Chess.Server.Models
{
    [DataContract(Name = "BattleField")]
    public class FieldModel
    {

        [DataMember(Name = "gameId")]
        public int GameId { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "white")]
        public UserInGameModel White { get; set; }

        [DataMember(Name = "black")]
        public UserInGameModel Black { get; set; }

        [DataMember(Name = "turn")]
        public int Turn { get; set; }

        [DataMember(Name = "inTurn")]
        public string PlayerInTurn { get; set; }
    }
}