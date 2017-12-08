using System;
using System.Collections.Generic;
using System.Text;

namespace WavesCS.Main
{
    public class OrderBook
    {
        public List<Order> bids, asks;

        public List<Order> Bids
        {
            get { return bids; }
        }

        public List<Order> Asks
        {
            get { return asks; }
        }

        public OrderBook(List<Order> bids, List<Order> asks)
        {
            this.bids = bids;
            this.asks = asks;
        }
    }
}
