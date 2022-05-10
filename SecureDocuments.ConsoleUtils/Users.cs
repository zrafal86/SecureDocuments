using SecureDocuments.Data;
using SecureDocuments.Encryption.Hash;
using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SecureDocuments.ConsoleUtils
{
    public sealed class Users : ConsoleAppBase
    {
        private readonly IOptions<AppConfig> _config;

        public Users(IOptions<AppConfig> config)
        {
            _config = config;
        }

        public async Task Run(
            [Option("method", "method name to invoke.")]
            string method,
            [Option("e", "email")] string email,
            [Option("path", "path to file")] string path,
            [Option("pass", "password")] string password = "",
            [Option("f", "firstname")] string firstName = "",
            [Option("l", "lastname")] string lastName = "",
            [Option("role", "role - 0,1,2,3,4,5 - where 0 is admin and 5 reader")]
            string role = "5")
        {
            Context.Logger.LogDebug($"Hello My ConsoleApp invoke method '{method}' with param '{path}'");
            switch (method)
            {
                case "add":
                    await AddUser(firstName, lastName, email, password, role, path);
                    break;

                case "changePass":
                    await ChangePassword(email, password, path);
                    break;

                case "del":
                    await Remove(email, path);
                    break;

                default:
                    throw new ArgumentException(
                        "method name is not a recognized",
                        nameof(method));
            }

            Context.Logger.LogDebug($"GlobalValue: {_config.Value.GlobalValue}, EnvValue: {_config.Value.EnvValue}");
        }

        private static async Task Remove(string email, string path)
        {
            var source = new AppConfigSource(new AesEncryption { KeyProvider = new KeyProvider(new KeysFactory()) });
            var config = await source.Get(path);
            var users = config?.Users;
            var userList = users?.ToList();
            var user = userList?.Where(u => u.Email!.Equals(email)).FirstOrDefault();
            if (user is not null)
            {
                userList?.Remove(user);
            }
            await source.Save(config!, path);
        }

        private async Task AddUser(string firstName, string lastName, string email, string password, string role,
            string path)
        {
            string logMessage = $"firstName: {firstName},\nlastName: {lastName},\nemail: {email},\npassword: {password},\nrole: {role},\npath: {path}";
            Context.Logger.LogDebug(logMessage);

            var source = new AppConfigSource(new AesEncryption { KeyProvider = new KeyProvider(new KeysFactory()) });
            var config = await source.Get(path);
            var users = config?.Users;

            var user = users?.Where(u => u.Email!.Equals(email));
            if (user?.Count() == 0 && password != null)
            {
                var passwordHashed = Hash.DoHash(password);
                var usersList = users?.ToList();
                usersList?.Add(
                    new User
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        PasswordHash = passwordHashed,
                        Role = (Role)int.Parse(role)
                    }
                );
                var newConfig = new Models.AppConfig
                {
                    Users = usersList,
                    EmailNotification = config?.EmailNotification
                };
                await source.Save(newConfig, path);
            }
        }

        //dotnet run -p .\SecureDocuments.ConsoleUtils\ users run -method changePass
        //.\SecureDocuments.ConsoleUtils.exe users run -method changePass -email "admin@admin.pl" -path 'C:\Users\zrafa\Desktop\DMFS\app_config.dtms' -password 'password'
        private async Task ChangePassword(string email, string password, string path)
        {
            Context.Logger.LogDebug($"email: {email},\npassword: {password},\npath: {path}");
            var source = new AppConfigSource(new AesEncryption { KeyProvider = new KeyProvider(new KeysFactory()) });
            var config = await source.Get(path);
            var users = config?.Users;
            var userList = users?.ToList();
            var user = userList?.Where(u => u.Email!.Equals(email)).FirstOrDefault();
            if (user is not null)
            {
                var passwordHashed = Hash.DoHash(password);
                var newEmployee = new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PasswordHash = passwordHashed,
                    Role = user.Role
                };

                userList?.Remove(user);
                userList?.Add(newEmployee);
                var newConfig = new Models.AppConfig
                {
                    Users = userList,
                    EmailNotification = config?.EmailNotification
                };
                await source.Save(newConfig, path);
            }
        }
    }
}