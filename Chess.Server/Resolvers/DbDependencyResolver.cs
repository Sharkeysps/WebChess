using Chess.Model;
using Chess.Server.Controllers;
using Chess.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;

namespace Chess.Server.Resolvers
{
    public class DbDependencyResolver : IDependencyResolver
    {
        private static AllRepositories<ChessEntities> allRepositories = new AllRepositories<ChessEntities>();

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(GameController))
            {
                return new GameController(allRepositories);
            }
            else if (serviceType == typeof(FigureController))
            {
                return new FigureController(allRepositories);
            }
            else if (serviceType == typeof(UserController))
            {
                return new UserController(allRepositories);
            }
            else if (serviceType == typeof(MessagesController))
            {
                return new MessagesController(allRepositories);
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }

        public void Dispose()
        {
        }
    }
}