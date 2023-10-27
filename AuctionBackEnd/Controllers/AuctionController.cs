using System.Collections.Generic;
using System.Linq;
using AuctionBackEnd.Common;
using AuctionBackEnd.Data;
using AuctionBackEnd.Database;
using Microsoft.AspNetCore.Mvc;

namespace AuctionBackEnd.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuctionController : ControllerBase
    {
        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] SignUpUser signUpUser)
        {
            return Account.SignUp(signUpUser).Result;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUser loginUser)
        {
            return Account.Login(loginUser).Result;
        }

        [HttpPost("login-with-token")]
        public IActionResult LoginWithToken([FromBody] Token token)
        {
            return Account.LoginWithToken(token).Result;
        }
        
        [HttpGet("active-auction-list")]
        public IActionResult ActiveAuctionList()
        {
            return Auction.ActiveAuctionList().Result;
        }
        
        [HttpGet("get-auction")]
        public IActionResult GetAuction(string auctionUuid)
        {
            return Auction.GetAuction(auctionUuid).Result;
        }

        [HttpPost("bid")]
        public IActionResult Bid([FromBody] Bid bid)
        {
            return Auction.Bid(bid).Result;
        }
        
    }
}