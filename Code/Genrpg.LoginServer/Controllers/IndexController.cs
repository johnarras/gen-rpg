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
        [Route("/login")]
        public async Task<string> PostLogin(Core.LoginServer webService, [FromForm] string Data)
        {
            return await webService.HandleLogin(Data);
        }

        [HttpPost]
        [Route("/client")]
        public async Task<string> PostClient(Core.LoginServer webService, [FromForm] string Data)
        {
            return await webService.HandleClient(Data);
        }
    }
}
