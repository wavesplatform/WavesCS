﻿using System;
using System.IO;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class LeaseTransaction : Transaction
    {
        public string Recipient { get; }
        public decimal Amount { get; }
        public bool IsActive { get; }
        public override byte Version { get; set; } = 2;

        public LeaseTransaction(char chainId, byte[] senderPublicKey, string recipient, decimal amount, decimal fee = 0.001m) : 
            base(chainId, senderPublicKey)
        {
            Recipient = recipient;
            Amount = amount;
            Fee = fee;
        }

        public LeaseTransaction(DictionaryObject tx) : base(tx)
        {
            Recipient = tx.GetString("recipient");
            Amount = Assets.WAVES.LongToAmount(tx.GetLong("amount"));
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
            Version = tx.GetByte("version");
            IsActive = tx.ContainsKey("status") ? tx.GetString("status") == "active" : true;
        }

        public override byte[] GetBody()
        {
            
            using(var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.Lease);

                if (Version > 1)
                {
                    writer.WriteByte(Version);
                    writer.WriteByte((byte)0);
                }

                writer.Write(SenderPublicKey);
                writer.Write(Recipient.FromBase58());
                writer.WriteLong(Assets.WAVES.AmountToLong(Amount));
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteLong(Timestamp.ToLong());
                return stream.ToArray();
            }
        }

        public override byte[] GetBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            if (Version == 1)
            {
                writer.Write(GetBody());
                writer.Write(Proofs[0]);
            }
            else
            {
                writer.WriteByte(0);
                writer.Write(GetBody());
                writer.Write(GetProofsBytes());
            }

            return stream.ToArray();
        }

        public override DictionaryObject GetJson()
        {
            var result = new DictionaryObject {
                {"version", (byte) Version },
                {"type", (byte) TransactionType.Lease},
                {"senderPublicKey", SenderPublicKey.ToBase58()},                
                {"recipient", Recipient},
                {"amount", Assets.WAVES.AmountToLong(Amount)},
                {"fee", Assets.WAVES.AmountToLong(Fee)},
                {"timestamp", Timestamp.ToLong()}
            };

            if (Sender != null)
                result.Add("sender", Sender);

            return result;
        }

        protected override bool SupportsProofs()
        {
            return Version > 1;
        }
    }
}