using Microsoft.AspNetCore.Mvc;
using MS.Vehiculos.Services;
using System.Data;

namespace MS.Vehiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<DatabaseTestController> _logger;

        public DatabaseTestController(IDatabaseService databaseService, ILogger<DatabaseTestController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var isConnected = await _databaseService.TestConnectionAsync();

                if (isConnected)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Conexión a VehiclesDB exitosa",
                        database = "VehiclesDB",
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "No se pudo conectar a VehiclesDB",
                        database = "VehiclesDB",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test de conexión VehiclesDB");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    database = "VehiclesDB",
                    timestamp = DateTime.Now
                });
            }
        }
        [HttpGet("select-vehiculos")]
        public async Task<IActionResult> SelectVehiculos()
        {
            try
            {
                var result = await _databaseService.ExecuteQueryAsync("SELECT * FROM vehiculos LIMIT 10");

                var rows = new List<object>();
                foreach (DataRow row in result.Rows)
                {
                    var rowData = new Dictionary<string, object>();
                    foreach (DataColumn column in result.Columns)
                    {
                        rowData[column.ColumnName] = row[column];
                    }
                    rows.Add(rowData);
                }

                return Ok(new
                {
                    success = true,
                    message = "SELECT de vehiculos ejecutado exitosamente",
                    database = "VehiclesDB",
                    data = rows,
                    rowCount = result.Rows.Count,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SELECT de vehiculos");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    database = "VehiclesDB",
                    timestamp = DateTime.Now
                });
            }
        }
        [HttpGet("select-test")]
        public async Task<IActionResult> SelectTest()
        {
            try
            {
                var result = await _databaseService.ExecuteQueryAsync("SELECT 'VehiclesDB' AS database_name, 1 AS test_value, NOW() AS current_datetime");

                var rows = new List<object>();
                foreach (DataRow row in result.Rows)
                {
                    var rowData = new Dictionary<string, object>();
                    foreach (DataColumn column in result.Columns)
                    {
                        rowData[column.ColumnName] = row[column];
                    }
                    rows.Add(rowData);
                }

                return Ok(new
                {
                    success = true,
                    message = "SELECT ejecutado exitosamente en VehiclesDB",
                    database = "VehiclesDB",
                    data = rows,
                    rowCount = result.Rows.Count,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SELECT test VehiclesDB");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    database = "VehiclesDB",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}