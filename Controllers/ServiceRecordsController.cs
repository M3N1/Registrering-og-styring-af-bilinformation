using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers
{
    [Route("api/vehicles/{vehicleId}/[controller]")]
    [ApiController]
    public class ServiceRecordsController : ControllerBase
    {
        // Placeholder for the service record data storage
        private static readonly List<ServiceRecord> ServiceRecords = new List<ServiceRecord>();

        // Get all service records for a specific vehicle
        [HttpGet]
        public ActionResult<IEnumerable<ServiceRecord>> GetAll(int vehicleId)
        {
            var records = ServiceRecords.Where(sr => sr.VehicleId == vehicleId);
            return Ok(records);
        }

        // Get a single service record by ID for a specific vehicle
        [HttpGet("{id}")]
        public ActionResult<ServiceRecord> Get(int vehicleId, int id)
        {
            var record = ServiceRecords.FirstOrDefault(sr => sr.VehicleId == vehicleId && sr.Id == id);
            if (record == null)
            {
                return NotFound();
            }

            return Ok(record);
        }

        // Create a new service record for a specific vehicle
        [HttpPost]
        public IActionResult Create(int vehicleId, [FromBody] ServiceRecord serviceRecord)
        {
            serviceRecord.VehicleId = vehicleId;
            serviceRecord.Id = ServiceRecords.Count + 1;
            ServiceRecords.Add(serviceRecord);

            return CreatedAtAction(nameof(Get), new { vehicleId = serviceRecord.VehicleId, id = serviceRecord.Id }, serviceRecord);
        }
    }
}
