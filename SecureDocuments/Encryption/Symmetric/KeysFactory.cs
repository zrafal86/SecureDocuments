using SecureDocuments.Models;

namespace SecureDocuments.Encryption.Symmetric
{
    public class KeysFactory : IKeysFactory
    {
        public IEnumerable<IKey> CreateKeys()
        {
            var allKeys = new List<IKey>();
            var readersKey = new RoleKey(
                "ScNEbDWZmXhdkf+ABzhFGI462aVHAffw6iWc74Zrnxw=",
                "eF4jkcMQwuHgUNxwJNY4pw==",
                Role.Reader);
            var technologistsKey = new RoleKey(
                "MTydALzoveL/mTkjfIMzW4UMPCkEJIDDVOkNxYNQyMA=",
                "A90PiQ7KNOtzagim6yZJ0w==",
                Role.Technologist);
            var buildersKey = new RoleKey(
                "+ZrtjiD2ouQTq1o6+wXQE3nHzLqdoPSmSmmUG95i2kM=",
                "CglXZsXIxkkPccbYLwDZAQ==",
                Role.Builder);
            var managerKey = new RoleKey(
                "64HgUQPT/PB/YJyyFEFSpi1k2hvmXufFEm7xweZHxlg=",
                "Xpeh+23BoHUsS9HLr/PEhw==",
                Role.Manager);
            var ownerKey = new RoleKey(
                "T0baRFPZefxV1D4sHYs2O3OnUHTEJgGjlhf1QoUdny0=",
                "cMpJrK6is96yt/QhVZIpNw==",
                Role.Admin);
            var adminKey = new RoleKey(
                "Ca11x4B+vo9xR8/ilFXKQpxS9rwESC28vvM9vuVLgog=",
                "h8d7kQDvD6y1Garm5ppAVg==",
                Role.Creator);
            allKeys.Add(readersKey);
            allKeys.Add(technologistsKey);
            allKeys.Add(buildersKey);
            allKeys.Add(managerKey);
            allKeys.Add(ownerKey);
            allKeys.Add(adminKey);
            return allKeys.ToImmutableArray();
        }
    }
}