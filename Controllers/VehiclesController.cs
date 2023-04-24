using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        // Placeholder for the vehicle data storage
        private static readonly List<Vehicle> Vehicles = new List<Vehicle>();

        // Image storage path
        private readonly string _imagePath = "Images";

        [HttpPost("create")]
        public IActionResult CreateVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return BadRequest("Vehicle object is null");
            }

            vehicle.Id = Guid.NewGuid();
            vehicle.ImageHistory = new List<ImageRecord>();
            Vehicles.Add(vehicle);
            return Ok(vehicle);
        }

        [HttpGet("list")]
        public IActionResult ListVehicles()
        {
            return Ok(Vehicles);
        }

        [HttpGet("{vehicleId}")]
        public IActionResult GetVehicle(Guid vehicleId)
        {
            Vehicle vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {vehicleId} not found.");
            }
            return Ok(vehicle);
        }

        [HttpGet("{vehicleId}/listImages")]
        public IActionResult ListImages(Guid vehicleId)
        {
            Vehicle vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {vehicleId} not found.");
            }
            return Ok(vehicle.ImageHistory);
        }

        [HttpPost("{vehicleId}/uploadImage"), DisableRequestSizeLimit]
        public IActionResult UploadImage(Guid vehicleId)
        {
            Vehicle vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {vehicleId} not found.");
            }

            try
            {
                foreach (var formFile in Request.Form.Files)
                {
                    // Validate file type and size
                    if (formFile.ContentType != "image/jpeg" && formFile.ContentType != "image/png")
                    {
                        return BadRequest($"Invalid file type for file {formFile.FileName}. Only JPEG and PNG files are allowed.");
                    }
                    if (formFile.Length > 1048576) // 1MB
                    {
                        return BadRequest($"File {formFile.FileName} is too large. Maximum file size is 1MB.");
                    }
                    if (formFile.Length > 0)
                    {
                        var fileName = "image-" + Guid.NewGuid().ToString() + ".jpg";
                        var fullPath = _imagePath + Path.DirectorySeparatorChar + fileName;

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            formFile.CopyTo(stream);
                        }

                        var imageURI = new Uri(fileName, UriKind.RelativeOrAbsolute);
                        var imageRecord = new ImageRecord
                        {
                            Id = Guid.NewGuid(),
                            Location = imageURI,
                            Date = DateTime.UtcNow,
                            // Add other properties like Description and AddedBy as needed
                        };

                        vehicle.ImageHistory.Add(imageRecord);
                    }
                    else
                    {
                        return BadRequest("Empty file submitted.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error.");
            }

            return Ok(vehicle.ImageHistory);
        }
    }
}