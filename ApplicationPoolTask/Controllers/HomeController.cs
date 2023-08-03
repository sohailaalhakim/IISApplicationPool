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

                // Create a view model and pass the list of appPoolNames to the view
                var viewModel = new AppPoolViewModel
                {
                    AppPoolNames = appPoolNames
                };

                return View(viewModel);
            }
        }

        public ActionResult Start()
        {
            try
            {
                using (ServerManager iisManager = new ServerManager())
                {

                    var appPool = iisManager.ApplicationPools["testTask"];
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
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult Stop()
        {
            try
            {
                using (ServerManager iisManager = new ServerManager())
                {
                    var appPool = iisManager.ApplicationPools["testTask"];
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
                    else
                    {
                        throw new Exception("The 'testTask' application pool does not exist.");
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            return RedirectToAction("Index");
        }
        public ActionResult Recycle()
        {

            using (ServerManager serverManager = new ServerManager())
            {
                ApplicationPool appPool = serverManager.ApplicationPools["testTask"];
                if (appPool != null)
                {
                    if (appPool.State == ObjectState.Started || appPool.State == ObjectState.Starting)
                    {
                        // Wait for the app pool to finish starting or stopping
                        while (appPool.State == ObjectState.Starting || appPool.State == ObjectState.Stopping)
                        {
                            System.Threading.Thread.Sleep(1000);
                            appPool = serverManager.ApplicationPools["testTask"];
                        }

                        if (appPool.State != ObjectState.Stopped)
                        {
                            // Stop the app pool if it isn't already stopped
                            appPool.Stop();

                            // Wait for the app pool to finish stopping
                            while (appPool.State == ObjectState.Stopping)
                            {
                                System.Threading.Thread.Sleep(1000);
                                appPool = serverManager.ApplicationPools["testTask"];
                            }
                        }
                    }
                    // Start the app pool
                    appPool.Start();
                }
            }

            return RedirectToAction("Index");
        }


        public ActionResult GetAppPoolStatus(string appPoolName)
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

//public ActionResult Start(string appPoolName)
//{
//    using (ServerManager iisManager = new ServerManager())
//    {
//        var appPool = iisManager.ApplicationPools[appPoolName];
//        if (appPool != null)
//        {
//            appPool.Start();
//        }
//    }
//    return RedirectToAction("Index");
//}


//public ActionResult Stop(string appPoolName)
//{
//    using (ServerManager iisManager = new ServerManager())
//    {
//        var appPool = iisManager.ApplicationPools[appPoolName];
//        if (appPool != null)
//        {
//            appPool.Stop();
//            ViewBag.Message = "Application Pool stopped successfully!";
//        }
//        else
//        {
//            ViewBag.Message = "Invalid application pool name: " + appPoolName;
//        }
//    }
//    return RedirectToAction("Index");
//}

////////////////////////////////////////////////////////////////////////
//using (ServerManager serverManager = new ServerManager())
//{
//    ApplicationPool appPool = serverManager.ApplicationPools["testTask"];
//    if (appPool != null)
//    {
//        //Get the current state of the app pool
//        bool appPoolRunning = appPool.State == ObjectState.Started || appPool.State == ObjectState.Starting;
//        bool appPoolStopped = appPool.State == ObjectState.Stopped || appPool.State == ObjectState.Stopping;

//        //The app pool is running, so stop it first.
//        if (appPoolRunning)
//        {
//            //Wait for the app to finish before trying to stop
//            while (appPool.State == ObjectState.Starting) { System.Threading.Thread.Sleep(1000); }

//            //Stop the app if it isn't already stopped
//            if (appPool.State != ObjectState.Stopped)
//            {
//                appPool.Stop();
//            }
//            appPoolStopped = true;
//        }

//        //Only try restart the app pool if it was running in the first place, because there may be a reason it was not started.
//        if (appPoolStopped && appPoolRunning)
//        {
//            //Wait for the app to finish before trying to start
//            while (appPool.State == ObjectState.Stopping) { System.Threading.Thread.Sleep(1000); }

//            //Start the app
//            appPool.Start();
//        }
//    }
//}\




// using (ServerManager serverManager = new ServerManager())
//{
//    ApplicationPool appPool = serverManager.ApplicationPools["testTask"];
//    if (appPool != null)
//    {
//        appPool.Recycle();
//        //if (appPool.State == ObjectState.Stopped)
//        //{
//        //    appPool.Recycle();
//        //}

//    }