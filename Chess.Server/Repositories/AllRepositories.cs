namespace Chess.Server.Repositories
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class AllRepositories<T> where T : DbContext
    {
        public GameRepository gameRepository;
        public FigureRepository figureRepository;
        public UserRepository userRepository;
        public MessagesRepository messageRepository;

        public GameRepository GetGameRepository() 
        {
            DbContext newContext = (T)Activator.CreateInstance(typeof(T), new object[] {});
            return new GameRepository(newContext);
        }

        public FigureRepository GetFigureRepository()
        {
            DbContext newContext = (T)Activator.CreateInstance(typeof(T), new object[] { });
            return new FigureRepository(newContext);
        }

        public UserRepository GetUserRepository()
        {
            DbContext newContext = (T)Activator.CreateInstance(typeof(T), new object[] { });
            return new UserRepository(newContext);
        }

        public MessagesRepository GetMessagesRepository()
        {
            DbContext newContext = (T)Activator.CreateInstance(typeof(T), new object[] { });
            return new MessagesRepository(newContext);
        }
    }
}