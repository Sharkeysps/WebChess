using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Chess.Server.Models
{
    [DataContract(Name = "Position")]
    public class PositionModel : IComparable<PositionModel>
    {
        [DataMember(Name = "row")]
        public int Row { get; set; }

        [DataMember(Name = "col")]
        public int Col { get; set; }

        public int CompareTo(PositionModel other)
        {
            if (this.Row == other.Row && this.Col == other.Col)
            {
                return 0;
            }
            else
            {
                if (this.Row == other.Row)
                {
                    return this.Col - this.Col;
                }
                else
                {
                    return this.Row - other.Row;
                }
            }
        }
    }
}