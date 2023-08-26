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
    }
}
