using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Chess.Server.Models
{
    [DataContract(Name = "Move")]
    public class MoveModel
    {
        [DataMember(Name = "figureId")]
        public int FigureId { get; set; }

        [DataMember(Name = "position")]
        public PositionModel Position { get; set; }
    }
}