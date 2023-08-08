using ApplicationPoolTask.Models;
using ApplicationPoolTask.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using System.Diagnostics;

namespace ApplicationPoolTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            using (ServerManager iisManager = new ServerManager())
            {
                List<string> appPoolNames = iisManager.ApplicationPools
                    .Select(appPool => appPool.Name)
                    .ToList();

                var viewModel = new AppPoolViewModel
                {
                    AppPoolNames = appPoolNames
                };

                return View(viewModel);
            }
        }

        public ActionResult Start(string appPoolName)
        {
            try
            {
                if (string.IsNullOrEmpty(appPoolName) || appPoolName == "أختر أسم")
                {
                    return Json("InvalidAppPool");

                }
                using (ServerManager iisManager = new ServerManager())
                {
                    var appPool = iisManager.ApplicationPools[appPoolName];

                    if (appPool != null)
                    {
                        if (appPool.State == ObjectState.Stopped || appPool.State == ObjectState.Stopping)
                        {
                            appPool.Start();
                        }
                        else
                        {
                            appPool.Recycle();
                        }
                     
                    }
                    
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Console.WriteLine("Application Pool Name Invalid",ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return RedirectToAction("Index");
        }
        
        public ActionResult Stop (string appPoolName)
        {
            try
            {
                if (string.IsNullOrEmpty(appPoolName) || appPoolName == "أختر أسم")
                {
                    return Json("InvalidAppPool");

                }
                using (ServerManager iisManager = new ServerManager())
                {
                    var appPool = iisManager.ApplicationPools[appPoolName];
                    if (appPool != null)
                    {
                        if (appPool.State == ObjectState.Started || appPool.State == ObjectState.Starting)
                        {
                            appPool.Stop();
                        }
                        else
                        {
                            appPool.Recycle();
                        }
                    }

                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Console.WriteLine("Application Pool Name Invalid", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            return RedirectToAction("Index");
        }

        public ActionResult Recycle(string appPoolName)
        {
            try
            {
                if (string.IsNullOrEmpty(appPoolName) || appPoolName == "أختر أسم")
                {
                    return Json("InvalidAppPool");

                }
                using (ServerManager serverManager = new ServerManager())
                {
                    ApplicationPool appPool = serverManager.ApplicationPools[appPoolName];
                    if (appPool != null)
                    {
                        if (appPool.State == ObjectState.Started || appPool.State == ObjectState.Starting)
                        {
                            // Wait for the app pool to finish starting or stopping
                            while (appPool.State == ObjectState.Starting || appPool.State == ObjectState.Stopping)
                            {
                                //System.Threading.Thread.Sleep(1000);
                                appPool = serverManager.ApplicationPools[appPoolName];
                            }

                            if (appPool.State != ObjectState.Stopped)
                            {
                                // Stop the app pool if it isn't already stopped
                                appPool.Stop();

                                // Wait for the app pool to finish stopping
                                while (appPool.State == ObjectState.Stopping)
                                {
                                    //System.Threading.Thread.Sleep(1000);
                                    appPool = serverManager.ApplicationPools[appPoolName];
                                }
                            }

                        }
                        // Start the app pool
                        appPool.Start();
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Console.WriteLine("Application Pool Name Invalid", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return RedirectToAction("Index");
        }


        public ActionResult GetAppPoolStatus(string appPoolName)
        {
            try
            {
                using (ServerManager iisManager = new ServerManager())
                {
                    ApplicationPool appPool = iisManager.ApplicationPools[appPoolName];
                    if (appPool != null)
                    {
                        if (appPool.State == ObjectState.Started || appPool.State == ObjectState.Starting)
                        {
                            return Content("Started");
                        }
                        else if (appPool.State == ObjectState.Stopped || appPool.State == ObjectState.Stopping)
                        {
                            return Content("Stopped");
                        }
                    }

                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Console.WriteLine("Application Pool Name Invalid", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return Content("Not Found");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
