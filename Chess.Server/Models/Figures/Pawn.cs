using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Chess.Model;

namespace Chess.Server.Models.Figures
{
    public class Pawn:IFigure
    {
        public PositionModel pos { get; set; }
        public bool isWhite { get; set; }
        public bool IsMoved { get; set; }

        public Pawn(Figure fig)
        {
            this.pos = new PositionModel()
            {
                Row = fig.PositionRow,
                Col = fig.PositionCol
            };
            isWhite = fig.IsWhite;
            IsMoved = fig.IsMoved;
        }

        public bool CanJump()
        {
            return false;
        }

        public List<PositionModel> GetPossibleMoves()
        {
            List<PositionModel> possibleMoves = new List<PositionModel>();
            
            int direction = 1;
            
            if (!this.isWhite)
            {
             direction = -1;
            }

            if ((this.pos.Row + direction) > 8 || (this.pos.Row + direction) < 1)
            {
                return possibleMoves;
            }

            possibleMoves.Add(new PositionModel()
            {
                Row = this.pos.Row + direction,
                Col = this.pos.Col
            });


            if (!IsMoved && (this.pos.Row + direction*2) <= 8 && (this.pos.Row + direction*2) >= 1)
            {
                possibleMoves.Add(new PositionModel()
                {
                    Row = this.pos.Row + direction*2,
                    Col = this.pos.Col
                });
            }

            return possibleMoves;
        }

        public List<PositionModel> GetPossibleHits()
        {
            List<PositionModel> possibleHits = new List<PositionModel>();

            int direction = 1;

            if (!this.isWhite)
            {
                direction = -1;
            }

            if ((this.pos.Row + direction) > 8 || (this.pos.Row + direction) < 1)
            {
                return possibleHits;
            }

            if (this.pos.Col + 1 <= 8)
            {
                possibleHits.Add(new PositionModel()
                {
                    Row = this.pos.Row + direction,
                    Col = this.pos.Col + 1
                });
            }

            if (this.pos.Col - 1 >= 1)
            {
                possibleHits.Add(new PositionModel()
                {
                    Row = this.pos.Row + direction,
                    Col = this.pos.Col - 1
                });
            }

            return possibleHits;
        }
    }
}