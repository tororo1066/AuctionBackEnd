using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace AuctionBackEnd.Database
{
    [Table("normal_auction_data")]
    public class AuctionData
    {
        [Key]
        public int Id { get; set; }
        [Column("auc_uuid")]
        public string AuctionUuid { get; set; }
        [Column("seller_uuid")]
        public string SellerUuid { get; set; }
        [Column("seller_name")]
        public string SellerName { get; set; }
        [Column("item")]
        public string Item { get; set; }
        [Column("start_date")]
        public DateTime StartDate { get; set; }
        [Column("activate_day")]
        public int ActivateDay { get; set; }
        [Column("end_date")]
        public DateTime? EndDate { get; set; }
        [Column("now_price")]
        public double NowPrice { get; set; }
        [Column("default_price")]
        public double DefaultPrice { get; set; }
        [Column("isEnd")]
        public bool IsEnd { get; set; }
        [Column("isReceived")]
        public bool IsReceived { get; set; }
        [Column("last_bid_uuid")]
        public string LastBidUuid { get; set; }
        [Column("last_bid_name")]
        public string LastBidName { get; set; }
        [Column("split_money")]
        public double SplitMoney { get; set; }
        [Column("delay_minute")]
        public int DelayMinute { get; set; }
        [Column("item_info")]
        public string ItemInfo { get; set; }
        
        [NotMapped]
        public string MaterialName { get; set; }
    }
}