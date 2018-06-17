﻿using System;
using System.Collections.Generic;
using System.IO;

namespace WavesCS
{
    public class ReissueTransaction : Transaction
    {
        public Asset Asset { get; }
        public decimal Quantity { get; }
        public bool Reissuable { get; }
        public decimal Fee { get; }

        public ReissueTransaction(byte[] senderPublicKey, DateTime timestamp, Asset asset, decimal quantity, bool reissuable, decimal fee = 1m) : 
            base(senderPublicKey, timestamp)
        {
            Asset = asset;
            Quantity = quantity;
            Reissuable = reissuable;
            Fee = fee;            
        }

        public override byte[] GetBody()
        {            
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Reissue);
                writer.Write(SenderPublicKey);
                writer.Write(Asset.Id.FromBase58());
                writer.WriteLong(Asset.AmountToLong(Quantity));
                writer.Write((byte) (Reissuable ? 1 : 0));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }            
        }

        public override Dictionary<string, object> GetJson()
        {
            return new Dictionary<string, object>
            {
                {"type", TransactionType.Reissue},
                {"senderPublicKey", SenderPublicKey.ToBase58()},
                {"assetId", Asset.Id},
                {"quantity", Asset.AmountToLong(Quantity)},
                {"reissuable", Reissuable},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()}
            };
        }

        protected override bool SupportsProofs()
        {
            return false;
        }
    }
}