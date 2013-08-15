using Chess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Chess.Server.Models
{
    public class FigureModel
    {
        public int Id { get; set; }
        public int PosRow { get; set; }
        public int PosCol { get; set; }
        public string Type { get; set; }
        public bool IsWhite { get; set; }
        public bool IsMoved { get; set; }

        public static Expression<Func<Figure, FigureModel>> FromFigure
        {
            get
            {
                return x => new FigureModel()
                {
                    Id = x.Id,
                    PosRow=x.PositionRow,
                    PosCol=x.PositionCol,
                    Type=x.FigureType.TypeName
                };
            }
        }
        public static FigureModel ConvertToModel(Figure figure)
        {
            return new FigureModel()
            {
                Id = figure.Id,
                PosRow = figure.PositionRow,
                PosCol = figure.PositionCol,
                Type = figure.FigureType.TypeName,
                IsWhite=figure.IsWhite,
                IsMoved=figure.IsMoved

            };
        }
    }
}