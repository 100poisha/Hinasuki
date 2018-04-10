using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Hinasuki.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                using (var connection = new SqlConnection(Settings.ConnectionString))
                {
                    var count = await connection.QuerySingleAsync<long>("SELECT MAX(Count) FROM Hinasuki");

                    ViewBag.Count = count;
                }
            }
            catch (Exception)
            {
                ViewBag.Count = 0;
            }
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}