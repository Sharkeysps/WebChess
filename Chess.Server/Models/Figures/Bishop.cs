using Chess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chess.Server.Models.Figures
{
    public class Bishop: IFigure
    {
        public PositionModel pos { get; set; }

        public bool isWhite { get; set; }

        public bool IsMoved { get; set; }

        private static readonly List<Tuple<int, int>> Directions =
        new List<Tuple<int, int>>(){ 
                    new Tuple<int,int>(-1, 1),
                    new Tuple<int,int>(1, 1),
                    new Tuple<int,int>(1, -1),
                    new Tuple<int,int>(-1, -1)
                };
        
        public Bishop(Figure fig)
        {
            this.pos = new PositionModel()
            {
                Row = fig.PositionRow,
                Col = fig.PositionCol
            };
            isWhite = fig.IsWhite;
            IsMoved = fig.IsMoved;
        }

        public List<PositionModel> GetPossibleMoves()
        {
            return GetPossibleHits();
        }

        public List<PositionModel> GetPossibleHits()
        {
            List<PositionModel> possibleHits = new List<PositionModel>();

            for (int i = 0; i < Directions.Count; i++)
            {
                bool isPossible = true;
                int newCol = this.pos.Col;
                int newRow = this.pos.Row;
                do
                {
                    Tuple<int, int> direction = Directions[i];
                    newCol += direction.Item1;
                    newRow += direction.Item2;

                    if ((newCol <= 8 && newCol >= 1) &&
                        (newRow <= 8 && newRow >= 1))
                    {
                        possibleHits.Add(new PositionModel()
                        {
                            Row = newRow,
                            Col = newCol
                        });
                    }
                    else
                    {
                        isPossible = false;
                    }
                }
                while (isPossible);
            }

            return possibleHits;
        }

        public bool CanJump()
        {
            return false;
        }
    }
}