using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

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
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return BadRequest("Vehicle object is null");
            }

            vehicle.Id = Guid.NewGuid();
            vehicle.ImageHistory = new List<ImageRecord>();

            MongoClient dbClient = new MongoClient("mongodb://admin:1234@localhost:27018/?authSource=admin");
            var collection = dbClient.GetDatabase("vehicle").GetCollection<Vehicle>("vehicles");
            await collection.InsertOneAsync(vehicle);
            return Ok(vehicle);
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListVehicles()
        {
            MongoClient dbClient = new MongoClient("mongodb://admin:1234@localhost:27018/?authSource=admin");
            var collection = dbClient.GetDatabase("vehicle").GetCollection<Vehicle>("vehicles");
            var vehicles = await collection.Find(_ => true).ToListAsync();
            return Ok(vehicles);
        }

        [HttpGet("{reg}")]
        public async Task<IActionResult> GetVehicle(string reg)
        {
            MongoClient dbClient = new MongoClient("mongodb://admin:1234@localhost:27018/?authSource=admin");
            var collection = dbClient.GetDatabase("vehicle").GetCollection<Vehicle>("vehicles");
            Vehicle vehicle = await collection.Find(v => v.RegistrationNumber == reg).FirstOrDefaultAsync();

            if (vehicle == null)
            {
                return NotFound($"Vehicle with RegistrationNumber {reg} not found.");
            }
            return Ok(vehicle);
        }


        [HttpGet("{reg}/listImages")]
        public IActionResult ListImages(string reg)
        {
            Vehicle vehicle = Vehicles.FirstOrDefault(v => v.RegistrationNumber == reg);
            if (vehicle == null)
            {
                return NotFound($"Vehicle with RegistrationNumber {reg} not found.");
            }
            return Ok(vehicle.ImageHistory);
        }

        [HttpPost("uploadImage/{reg}"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadImage(string reg)
        {
            if (!Directory.Exists(_imagePath))
            {
                Directory.CreateDirectory(_imagePath);
            }

            MongoClient dbClient = new MongoClient("mongodb://admin:1234@localhost:27018/?authSource=admin");
            var collection = dbClient.GetDatabase("vehicle").GetCollection<Vehicle>("vehicles");
            var filter = Builders<Vehicle>.Filter.Eq(v => v.RegistrationNumber, reg);
            Vehicle vehicle = await collection.Find(filter).FirstOrDefaultAsync();

            if (vehicle == null)
            {
                return NotFound($"Vehicle with RegistrationNumber {reg} not found.");
            }

            if (vehicle.ImageHistory == null)
            {
                vehicle.ImageHistory = new List<ImageRecord>();
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
                        var update = Builders<Vehicle>.Update.Push(v => v.ImageHistory, imageRecord);
                        await collection.UpdateOneAsync(filter, update);
                    }
                    else
                    {
                        return BadRequest("Empty file submitted.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }

            return Ok("Image(s) uploaded successfully.");
        }


        [HttpPost("{reg}/servicerecords")]
        public async Task<IActionResult> AddServiceRecord(string reg, [FromBody] ServiceRecord serviceRecord)
        {
            MongoClient dbClient = new MongoClient("mongodb://admin:1234@localhost:27018/?authSource=admin");
            var collection = dbClient.GetDatabase("vehicle").GetCollection<Vehicle>("vehicles");
            Vehicle vehicle = await collection.Find(v => v.RegistrationNumber == reg).FirstOrDefaultAsync();

            if (vehicle == null)
            {
                return NotFound($"Vehicle with RegistrationNumber {reg} not found.");
            }

            if (vehicle.ServiceHistory == null)
            {
                vehicle.ServiceHistory = new List<ServiceRecord>();
            }

            serviceRecord.Id = vehicle.ServiceHistory.Count + 1;
            vehicle.ServiceHistory.Add(serviceRecord);

            var update = Builders<Vehicle>.Update.Push(v => v.ServiceHistory, serviceRecord);
            await collection.UpdateOneAsync(v => v.RegistrationNumber == reg, update);

            return CreatedAtAction(nameof(GetVehicle), new { reg = reg }, vehicle);
        }

    }
}

