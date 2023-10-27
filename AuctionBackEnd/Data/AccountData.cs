namespace AuctionBackEnd.Data
{
    public class SignUpUser
    {
        public string Name { get; set; }
        public int LinkId { get; set; }
        public string Pass { get; set; }
    }
    
    public class LoginUser
    {
        public string Name { get; set; }
        public string Pass { get; set; }
    }
    
    public class Token
    {
        public string TokenString { get; set; }

        public Token(string tokenString)
        {
            TokenString = tokenString;
        }
    }

    public class Bid
    {
        public string Token { get; set; }
        public string AuctionUuid { get; set; }
        public double Price { get; set; }
    }
}