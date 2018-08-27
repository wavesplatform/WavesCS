Get the last transactions of a certain address:
```C#
    var node = new Node(Node.MainNetHost);
    var limit = 100;
    var address = "3PBmsJXAcgnH9cu81oyW8abNh9jsaNzFQKJ";

    var transactions = node.GetTransactions(address, limit);
```

Get a transaction by its Id:
```C#
    var transaction = node.GetTransactionById("37nfgadHFw92hNqzyHFZXmGFo5Wmct6Eik1Y2AdYW1Aq");
```

Get all recent TransferTransactions:
```C#
    var transferTransactions = node.GetTransactions(address)
                                   .OfType<TransferTransaction>();
```

List all distinct recipients of MassTransferTransactions with BTC asset:
```C#
    var massTransferBTCRecipients = node.GetTransactions(address)
                                 .OfType<MassTransferTransaction>()
                                 .Where(tx => tx.Asset.Id == Assets.BTC.Id)
                                 .SelectMany(tx => tx.Transfers)
                                 .Select(t => t.Recipient)
                                 .Distinct();
```

List all recently issued custom assets' names:
```C#
    var customAssetsNames = node.GetTransactions(address)
                                .OfType<IssueTransaction>()
                                .Select(tx => tx.Name);
```

Calculate the total amount of recently leased assets that are still leased:
```C#
    var leasedAmount = node.GetTransactions(address)
                         .OfType<LeaseTransaction>()
                         .Where(tx => tx.IsActive)
                         .Sum(tx => tx.Amount);
```

Count transactions of each type in recent transactions:
```C#
    var transactionsByType = node.GetTransactions(address)
                                 .GroupBy(tx => tx.GetType())
                                 .Select(group => new { Type = group.Key.Name, Count = group.Count() })
                                 .OrderByDescending(x => x.Count);

    foreach (var tx in transactionsByType)
        Console.WriteLine($"{tx.Type}\t\t{tx.Count}");
```
