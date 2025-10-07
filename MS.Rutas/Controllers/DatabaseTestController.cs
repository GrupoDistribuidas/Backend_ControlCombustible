using Microsoft.AspNetCore.Mvc;
using MS.Rutas.Services;
using System.Data;

namespace MS.Rutas.Controllers
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
                    return Ok(new { 
                        success = true, 
                        message = "Conexión a RoutesDB exitosa",
                        database = "RoutesDB",
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return StatusCode(500, new { 
                        success = false, 
                        message = "No se pudo conectar a RoutesDB",
                        database = "RoutesDB",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test de conexión RoutesDB");
                return StatusCode(500, new { 
                    success = false, 
                    message = ex.Message,
                    database = "RoutesDB",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet("select-test")]
        public async Task<IActionResult> SelectTest()
        {
            try
            {
                var result = await _databaseService.ExecuteQueryAsync("SELECT 'RoutesDB' as database_name, 1 as test_value, NOW() as current_datetime");
                
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

                return Ok(new { 
                    success = true, 
                    message = "SELECT ejecutado exitosamente en RoutesDB",
                    database = "RoutesDB",
                    data = rows,
                    rowCount = result.Rows.Count,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SELECT test RoutesDB");
                return StatusCode(500, new { 
                    success = false, 
                    message = ex.Message,
                    database = "RoutesDB",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}