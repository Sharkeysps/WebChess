using Chess.Model;
using Chess.Server.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

namespace Chess.Server.Repositories
{
    public class UserRepository : EfRepository<User>
    {
        private const string SessionKeyChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const int SessionKeyLen = 50;

        private const int Sha1CodeLength = 40;
        private const string ValidUsernameChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM_1234567890";
        private const string ValidNicknameChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM_1234567890 -";
        private const int MinUsernameNicknameChars = 6;
        private const int MaxUsernameNicknameChars = 30;
        protected static Random rand = new Random();

        public UserRepository(DbContext context)
            : base(context)
        {
        }

        public User GetUserBySessionKey(string sessionKey)
        {
            var result = this.Context.Set<User>().Where(x => x.SessionKey == sessionKey).FirstOrDefault();
            return result;
        }

        public void CreateUser(string username, string nickname, string authCode)
        {
            ValidateUsername(username);
            ValidateNickname(nickname);
            ValidateAuthCode(authCode);
            using (ChessEntities context = new ChessEntities())
            {
                var usernameToLower = username.ToLower();
                var nicknameToLower = nickname.ToLower();

                var dbUser = context.Users.FirstOrDefault(u => u.Name == usernameToLower || u.Nickname.ToLower() == nicknameToLower);

                if (dbUser != null)
                {
                    if (dbUser.Name.ToLower() == usernameToLower)
                    {
                        throw new InvalidOperationException("Username already exists");
                    }
                    else
                    {
                        throw new InvalidOperationException("Nickname already exists");
                    }
                }

                dbUser = new User()
                {
                    Name = usernameToLower,
                    Nickname = nickname,
                    AuthCode = authCode
                };
                context.Users.Add(dbUser);
                context.SaveChanges();
            }
        }
        public string LoginUser(string username, string authCode, out string nickname)
        {
            ValidateUsername(username);
            ValidateAuthCode(authCode);
            var context = new ChessEntities();
            using (context)
            {
                var usernameToLower = username.ToLower();
                var user = context.Users.FirstOrDefault(u => u.Name == usernameToLower && u.AuthCode == authCode);
                if (user == null)
                {
                    throw new InvalidOperationException("Invalid username or password");
                }

                var sessionKey = GenerateSessionKey((int)user.Id);
                user.SessionKey = sessionKey;
                nickname = user.Nickname;
                context.SaveChanges();
                return sessionKey;
            }
        }

        public int LoginUser(string sessionKey)
        {
            ValidateSessionKey(sessionKey);
            var context = new ChessEntities();
            using (context)
            {
                var user = context.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                if (user == null)
                {
                    throw new InvalidOperationException("Invalid user authentication");
                }
                return (int)user.Id;
            }
        }

        public void LogoutUser(string sessionKey)
        {
            ValidateSessionKey(sessionKey);
            var context = new ChessEntities();
            using (context)
            {
                var user = context.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                if (user == null)
                {
                    throw new InvalidOperationException("Invalid user authentication");
                }
                user.SessionKey = null;
                context.SaveChanges();
            }
        }

        public static IEnumerable<UserModel> GetAllUsers()
        {
            var context = new ChessEntities();
            using (context)
            {
                var users =
                    (from user in context.Users
                     select new UserModel()
                     {
                         Id = (int)user.Id,
                         Nickname = user.Nickname
                     });
                return users.ToList();
            }
        }

        private void ValidateSessionKey(string sessionKey)
        {
            if (sessionKey.Length != SessionKeyLen || sessionKey.Any(ch => !SessionKeyChars.Contains(ch)))
            {
                throw new InvalidOperationException("Invalid Password");
            }
        }

        private static string GenerateSessionKey(int userId)
        {
            StringBuilder keyChars = new StringBuilder(50);
            keyChars.Append(userId.ToString());
            while (keyChars.Length < SessionKeyLen)
            {
                int randomCharNum;
                lock (rand)
                {
                    randomCharNum = rand.Next(SessionKeyChars.Length);
                }
                char randomKeyChar = SessionKeyChars[randomCharNum];
                keyChars.Append(randomKeyChar);
            }
            string sessionKey = keyChars.ToString();
            return sessionKey;
        }

        private static void ValidateUsername(string username)
        {
            if (username == null || username.Length < MinUsernameNicknameChars || username.Length > MaxUsernameNicknameChars)
            {
                throw new InvalidOperationException(string.Format("Username should be between {0} and {1} symbols long", MinUsernameNicknameChars, MaxUsernameNicknameChars));
            }
            else if (username.Any(ch => !ValidUsernameChars.Contains(ch)))
            {
                throw new InvalidOperationException("Username contains invalid characters");
            }
        }

        private static void ValidateNickname(string nickname)
        {
            if (nickname == null || nickname.Length < MinUsernameNicknameChars || nickname.Length > MaxUsernameNicknameChars)
            {
                throw new ServerErrorException(string.Format("Nickname should be between {0} and {1} symbols long", MinUsernameNicknameChars, MaxUsernameNicknameChars), "INV_NICK_LEN");
            }
            else if (nickname.Any(ch => !ValidNicknameChars.Contains(ch)))
            {
                throw new ServerErrorException("Nickname contains invalid characters", "INV_NICK_CHARS");
            }
        }

        private static void ValidateAuthCode(string authCode)
        {
            if (authCode.Length != Sha1CodeLength)
            {
                throw new InvalidOperationException("Invalid user authentication");
            }
        }
    }
}