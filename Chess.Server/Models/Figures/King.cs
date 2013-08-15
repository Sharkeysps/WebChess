using Chess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chess.Server.Models.Figures
{
    public class King : IFigure
    {
        public PositionModel pos { get; set; }
        public bool isWhite { get; set; }
        public bool IsMoved { get; set; }
        
        private static readonly List<Tuple<int, int>> Directions =
        new List<Tuple<int, int>>(){ 
                    new Tuple<int,int>(-1, 1),
                    new Tuple<int,int>(0, 1),
                    new Tuple<int,int>(1, 1),
                    new Tuple<int,int>(1, 0),
                    new Tuple<int,int>(1, -1),
                    new Tuple<int,int>(0, -1),
                    new Tuple<int,int>(-1, -1),
                    new Tuple<int,int>(-1, 0)
                };

        public King(Figure fig)
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
            return GetPossibleHits();
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
    }
}