using System;
using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Xunmei.IdentityServer
{
    public class ClientController : Controller
    {
        ConfigurationDbContext configDb;

        public ClientController(ConfigurationDbContext configDb)
        {
            this.configDb = configDb;
        }
        public IActionResult Index(int page = 1)
        {
            var pageSize = 10;
            var skip = (page - 1) * pageSize;
            ViewData["Clients"] = configDb.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.RedirectUris)
                .Include(c => c.AllowedScopes)
                .OrderBy(c=>c.Id)
                .Take(pageSize)
                .Skip(skip)
                .ToList();

            return View();
        }
    }
}