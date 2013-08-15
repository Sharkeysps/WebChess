using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Chess.Server.Models
{
    [DataContract(Name = "Game")]
    public class ActiveGameModel : OpenGameModel
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}