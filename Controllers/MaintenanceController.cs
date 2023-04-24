using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private static readonly List<Maintenance> Maintenances = new List<Maintenance>();

        // POST: api/Maintenance
        [HttpPost]
        public IActionResult ScheduleMaintenance([FromBody] Maintenance maintenance)
        {
            if (maintenance == null)
            {
                return BadRequest("Maintenance object is null.");
            }

            maintenance.Id = Maintenances.Count + 1;
            Maintenances.Add(maintenance);
            return CreatedAtRoute("GetMaintenanceById", new { id = maintenance.Id }, maintenance);
        }

        // GET: api/Maintenance
        [HttpGet]
        public IActionResult GetAllMaintenances()
        {
            return Ok(Maintenances);
        }

        // GET: api/Maintenance/{id}
        [HttpGet("{id}", Name = "GetMaintenanceById")]
        public IActionResult GetMaintenanceById(int id)
        {
            var maintenance = Maintenances.FirstOrDefault(m => m.Id == id);

            if (maintenance == null)
            {
                return NotFound($"Maintenance with ID {id} was not found.");
            }

            return Ok(maintenance);
        }

        // PUT: api/Maintenance/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateMaintenance(int id, [FromBody] Maintenance maintenance)
        {
            if (maintenance == null)
            {
                return BadRequest("Maintenance object is null.");
            }

            var existingMaintenance = Maintenances.FirstOrDefault(m => m.Id == id);

            if (existingMaintenance == null)
            {
                return NotFound($"Maintenance with ID {id} was not found.");
            }

            existingMaintenance.VehicleId = maintenance.VehicleId;
            existingMaintenance.Description = maintenance.Description;
            existingMaintenance.ScheduledDate = maintenance.ScheduledDate;

            return NoContent();
        }

        // DELETE: api/Maintenance/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteMaintenance(int id)
        {
            var maintenance = Maintenances.FirstOrDefault(m => m.Id == id);

            if (maintenance == null)
            {
                return NotFound($"Maintenance with ID {id} was not found.");
            }

            Maintenances.Remove(maintenance);
            return NoContent();
        }
    }
}
