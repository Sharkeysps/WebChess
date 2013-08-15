namespace Chess.Server.Controllers
{
    using DropNet;
    using System;
    using System.IO;
    using System.Linq;
    using System.Web.Http;

    public class DropBoxController : ApiController
    {
        static string appKey = "1vbpi114qvtkczd";
        static string appSecret = "8iicdrpbw3wxthl";
        static DropNetClient client = new DropNetClient(appKey, appSecret, "d8lef4k66bur01ja", "5q9kronxh4jjbtd");        
        
        public DropBoxController()
        {
            client.UseSandbox = true;
        }

        [HttpPost]
        public void SaveFile([FromBody]Stream stream, [FromBody]string fileName, [FromBody]int fileSize)
        {
            byte[] file = new byte[fileSize];
            stream.Read(file, 0, fileSize);
            client.UploadFile("/", fileName, file);
        }
        [HttpGet]
        public string GetShareLink(string fileName)
        {
            return client.GetMedia(fileName).Url;
        }
    }
}