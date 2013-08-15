using Chess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chess.Server.Models.Figures
{
    public class Knight: IFigure
    {
        public PositionModel pos { get; set; }

        public bool isWhite { get; set; }

        public bool IsMoved { get; set; }

        private static readonly List<Tuple<int, int>> Directions =
        new List<Tuple<int, int>>(){ 
                    new Tuple<int,int>(-1, -2),
                    new Tuple<int,int>(1, -2),
                    new Tuple<int,int>(-1, 2),
                    new Tuple<int,int>(1, 2),
                    new Tuple<int,int>(-2, -1),
                    new Tuple<int,int>(-2, 1),
                    new Tuple<int,int>(2, -1),
                    new Tuple<int,int>(2, 1)
                };


        public Knight(Figure fig)
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
            return this.GetPossibleHits();
        }

        public List<PositionModel> GetPossibleHits()
        {
            List<PositionModel> possibleHits = new List<PositionModel>();

            for (int i = 0; i < Directions.Count; i++)
            {
                Tuple<int, int> direction = Directions[i];
                int newCol = this.pos.Col + direction.Item1;
                int newRow = this.pos.Row + direction.Item2;

                if ((newCol <= 8 && newCol >= 1) &&
                    (newRow <= 8 && newRow >= 1))
                {
                    possibleHits.Add(new PositionModel()
                    {
                        Row = newRow,
                        Col = newCol
                    });
                }
            }

            return possibleHits;
        }

        public bool CanJump()
        {
            return true;
        }
    }
}