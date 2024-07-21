using Genrpg.LoginServer.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Controllers
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
        public async Task<string> PostAuth(Core.LoginServer loginServer, [FromForm] string Data)
        {
            return await loginServer.HandleAuth(Data);
        }

        [HttpPost]
        [Route("/client")]
        public async Task<string> PostClient(Core.LoginServer loginServer, [FromForm] string Data)
        {
            return await loginServer.HandleClient(Data);
        }

        [HttpPost]
        [Route("/nouser")]
        public async Task<string> PostNoUser(Core.LoginServer loginServer, [FromForm] string Data)
        {
            return await loginServer.HandleNoUser(Data);
        }

        [HttpGet]
        [Route("/txlist")]
        public async Task<string> PostTxList(Core.LoginServer loginServer, string address)
        {
            return await loginServer.HandleTxList(address);
        }
    }
}
