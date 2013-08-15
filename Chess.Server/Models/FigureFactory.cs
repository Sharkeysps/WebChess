using Chess.Model;
using Chess.Server.Models.Figures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chess.Server.Models
{
    public static class FigureFactory
    {
        public static IFigure GetFigure(Figure figure)
        {
            var type=figure.FigureType.TypeName.ToLower().Trim();
            switch (type)
            {
                case "pawn":
                    return new Pawn(figure);
                case "king":
                    return new King(figure);
                case "queen":
                    return new Queen(figure);
                case "rook":
                    return new Rook(figure);
                case "bishop":
                    return new Bishop(figure);
                case "knight":
                    return new Knight(figure);
                default:
                    throw new InvalidOperationException("Unknown figure");
            }
        }
    }
}