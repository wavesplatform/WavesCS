using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{
    public class MassTransferItem
    {
        public string Recipient { get; }
        public decimal Amount { get;  }
        
        public MassTransferItem(string recipient, decimal amount)
        {
            Recipient = recipient;
            Amount = amount;
        }        
    }
    
    public class MassTransferTransaction : Transaction
    {
        public Asset Asset { get; }
        public byte[] Attachment { get; }
        public MassTransferItem[] Transfers { get; }

        public override byte Version { get; set; } = 1;

        public MassTransferTransaction(byte[] senderPublicKey, Asset asset, IEnumerable<MassTransferItem> transfers,
            string attachment, decimal? fee = null) : 
            this(senderPublicKey, asset, transfers, Encoding.UTF8.GetBytes(attachment), fee) 
        {
        }
        
        public MassTransferTransaction(byte[] senderPublicKey, Asset asset, IEnumerable<MassTransferItem> transfers,
            byte[] attachment = null, decimal? fee = null) : base(senderPublicKey)
        {
            Asset = asset;
            Attachment = attachment ?? new byte[0];
            Transfers = transfers.ToArray();
            Fee = fee ?? Math.Round(0.001m + Transfers.Length * 0.0005m, 3, MidpointRounding.AwayFromZero);
        }

        public MassTransferTransaction(DictionaryObject tx) : base(tx)
        {
            Asset = Assets.GetById(tx.GetString("assetId") ?? Assets.WAVES.Id);
            Attachment = tx.GetString("attachment").FromBase58();

            Transfers = tx.GetObjects("transfers")
                          .Select(transfer => new MassTransferItem(transfer.GetString("recipient"),
                                                                   Asset.LongToAmount(transfer.GetLong("amount"))))
                          .ToArray();
            Fee = Assets.WAVES.LongToAmount(tx.GetLong("fee"));
        }

        public override byte[] GetBody()
        {
            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream))
            {
                writer.Write(TransactionType.MassTransfer);
                writer.Write(Version);
                writer.Write(SenderPublicKey);
                writer.WriteAsset(Asset.Id);
                writer.WriteShort(Transfers.Length);
                foreach (var transfer in Transfers)
                {
                    if (transfer.Recipient.StartsWith("alias", StringComparison.Ordinal))
                    {
                        var chainId = transfer.Recipient[6];
                        var name = transfer.Recipient.Substring(8);

                        writer.Write((byte)2);
                        writer.Write(chainId);
                        writer.WriteShort((short)name.Length);
                        writer.Write(Encoding.UTF8.GetBytes(name));
                    }
                    else
                        writer.Write(transfer.Recipient.FromBase58());

                    writer.WriteLong(Asset.AmountToLong(transfer.Amount));
                }
                writer.WriteLong(Timestamp.ToLong());
                writer.WriteLong(Assets.WAVES.AmountToLong(Fee));
                writer.WriteShort((short) Attachment.Length);
                writer.Write(Attachment);
                return stream.ToArray();
            }
        }

        public override byte[] GetIdBytes()
        {
            return GetBody();
        }

        public override DictionaryObject GetJson()
        {
            return new DictionaryObject
            {
                { "type", TransactionType.MassTransfer},
                { "version", Version},
                { "senderPublicKey", Base58.Encode(SenderPublicKey)},                
                { "transfers", Transfers.Select(t => new DictionaryObject()
                {
                    {"recipient", t.Recipient },
                    {"amount", Asset.AmountToLong(t.Amount)}
                }).ToArray() },
                { "assetId", Asset.IdOrNull},
                { "fee", Assets.WAVES.AmountToLong(Fee)},
                { "timestamp", Timestamp.ToLong() },
                { "attachment", Attachment.ToBase58() }
            };
        }

        protected override bool SupportsProofs()
        {
            return true;
        }  
    }
}