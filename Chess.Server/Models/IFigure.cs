using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Server.Models
{
   public interface IFigure
    {
        List<PositionModel> GetPossibleMoves();
        List<PositionModel> GetPossibleHits();
        bool CanJump();
    }
}
