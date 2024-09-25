using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Services; // Assuming you'll create a Services folder for NexusPayablesService

namespace WebApplication1.Controllers
{
    public class NexusIBSController : Controller
    {
        private readonly ILogger<NexusIBSController> _logger;
        private readonly NexusPayablesService _nexusPayablesService;

        public NexusIBSController(ILogger<NexusIBSController> logger, NexusPayablesService nexusPayablesService)
        {
            _logger = logger;
            _nexusPayablesService = nexusPayablesService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UploadNexusPayables()
        {
            return View();
        }
        public IActionResult ProcessNexusPayables()
        {
            return View();
        }
        public IActionResult PrintYellowSheets()
        {
            // Implement printing logic here
            return View();
        }

        public IActionResult RecordApprovals()
        {
            // Implement approval recording logic here
            return View();
        }

        public IActionResult MoveApprovedForms()
        {
            // Implement form moving logic here
            return View();
        }
        public IActionResult ProcessConnectOnePayables()
        {
            return View();
        }

        public IActionResult DatabaseMaintenance()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ImportNexusIBSCSV(DateTime systemDate, IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                _logger.LogWarning("File not selected for import");
                return BadRequest("File not selected");
            }

            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await csvFile.CopyToAsync(stream);
            }

            try
            {
                _nexusPayablesService.ImportNexusCSV(filePath, systemDate);
                _logger.LogInformation($"CSV imported successfully at {DateTime.Now}");
                return Ok("CSV imported successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing CSV");
                return BadRequest($"Error importing CSV: {ex.Message}");
            }
            finally
            {
                System.IO.File.Delete(filePath);
            }
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

        // TODO: Implement other actions (ProcessNexusPayables, ProcessConnectOnePayables, DatabaseMaintenance)
    }
}