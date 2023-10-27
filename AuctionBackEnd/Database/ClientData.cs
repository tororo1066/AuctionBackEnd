using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionBackEnd.Database
{
    public class ClientData
    {
        [Key]
        public int Id { get; set; }
        public string Uuid { get; set; }
        public int LinkId { get; set; }
        public string Pass { get; set; }
        public string Salt { get; set; }
    }
}