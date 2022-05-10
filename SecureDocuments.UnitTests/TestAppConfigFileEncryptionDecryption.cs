using SecureDocuments.Data;
using SecureDocuments.Encryption.Hash;
using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Models;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SecureDocuments.UnitTests
{
    public class TestAppConfigFileEncryptionDecryption
    {
        [Fact]
        public async Task TestEncryptAppConfig()
        {
            var password = "pa$$w0rd";
            var email = "admin@admin.pl";
            var path = @"<path to config>\app_config.dtms";
            var source = new AppConfigSource(new AesEncryption { KeyProvider = new KeyProvider(new KeysFactory()) });
            var config = await source.Get(path);
            var users = config?.Users;
            var userList = users?.ToList();
            var user = userList?.Where(u => u.Email!.Equals(email)).FirstOrDefault();
            var passwordHashed = "";
            if (user is not null)
            {
                passwordHashed = Hash.DoHash(password);
                var newEmployee = new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PasswordHash = passwordHashed,
                    Role = user.Role,
                    Phone = user.Phone,
                };

                userList?.Remove(user);
                userList?.Add(newEmployee);
                var newConfig = new AppConfig
                {
                    Users = userList,
                    EmailNotification = config?.EmailNotification,
                    CompanyNames = config?.CompanyNames,
                    Subjects = config?.Subjects,
                    CustomerCountries = config?.CustomerCountries,
                    ListOfPeopleToBeInformedWhenOfferAccepted = config?.ListOfPeopleToBeInformedWhenOfferAccepted,
                };
                await source.Save(newConfig, path);
            }
            var config2 = await source.Get(path);
            var usersList2 = config?.Users?.ToList();
            var user2 = userList?.Where(u => u.Email!.Equals(email)).FirstOrDefault();
            Assert.Equal(passwordHashed, user2.PasswordHash);
        }
    }
}
