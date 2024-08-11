using Genrpg.RequestServer.Core;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.WebServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Index" };
        }

        [HttpPost]
        [Route("/auth")]
        public async Task<string> PostAuth(WebRequestServer webServer, [FromForm] string Data)
        {
            return await webServer.HandleAuth(Data);
        }

        [HttpPost]
        [Route("/client")]
        public async Task<string> PostClient(WebRequestServer webServer, [FromForm] string Data)
        {
            return await webServer.HandleClient(Data);
        }

        [HttpPost]
        [Route("/nouser")]
        public async Task<string> PostNoUser(WebRequestServer webServer, [FromForm] string Data)
        {
            return await webServer.HandleNoUser(Data);
        }

        [HttpGet]
        [Route("/txlist")]
        public async Task<string> PostTxList(WebRequestServer webServer, string address)
        {
            return await webServer.HandleTxList(address);
        }
    }
}
