using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Chess.Server.Models
{
    [DataContract(Name = "ChessUnit")]
    public class UnitModel
    {
        [DataMember(Name = "id")]
        public long Id { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "isWhite")]
        public bool IsWhite { get; set; }
        
        [DataMember(Name = "position")]
        public PositionModel Position { get; set; }
    }
}