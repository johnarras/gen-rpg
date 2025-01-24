using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Website.Interfaces
{
    public interface IAuthRequest : IWebRequest
    {

    }

    public interface IAuthLoginRequest : IAuthRequest
    {
        long AccountProductId { get; set; }
        string ReferrerId { get; set; }
        string ClientVersion { get; set; }
        string DeviceId { get; set; }
        string Password { get; set; }
    }
}
