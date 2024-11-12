using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;
using WebApplication1.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;

namespace WebApplication1.Controllers
{
    public class DynamicFormController : Controller
    {
        private readonly IDynamicFormService _formService;
        private readonly ILogger<DynamicFormController> _logger;

        public DynamicFormController(IDynamicFormService formService, ILogger<DynamicFormController> logger)
        {
            _formService = formService ?? throw new ArgumentNullException(nameof(formService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Index()
        {
            try
            {
                var properties = _formService.GetPropertyListing();
                return View(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property listing");
                ViewBag.ErrorMessage = "An error occurred while retrieving the property listing.";
                return View(new List<PropertyListingViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Details(string id)
        {
            try
            {
                var properties = _formService.GetPropertyListing();
                var property = properties.FirstOrDefault(p => p.PropertyID == id);

                if (property == null)
                {
                    return NotFound();
                }

                return View(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property details for ID: {PropertyId}", id);
                ViewBag.ErrorMessage = "An error occurred while retrieving the property details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Print()
        {
            try
            {
                var properties = _formService.GetPropertyListing();
                return View("Print", properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating print view");
                ViewBag.ErrorMessage = "An error occurred while preparing the print view.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}