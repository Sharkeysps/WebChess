using Chess.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Chess.Server.Repositories
{
    public class FigureRepository : EfRepository<Figure>
    {
        public FigureRepository(DbContext context)
            : base(context)
        {
        }

        public IQueryable<Figure> GetGameFigures(int gameId)
        {
            IQueryable<Figure> result = this.Context.Set<Figure>().Where(x => x.GameId == gameId);
            return result;
        }

        public Figure GetFigureByGameAndPosition(int gameId, int posRow, int posCol)
        {
            return this.Context.Set<Figure>().FirstOrDefault(x => x.GameId == gameId 
                && x.PositionCol == posCol && x.PositionRow == posRow);
        }
    }
}