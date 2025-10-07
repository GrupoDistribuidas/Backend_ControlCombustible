using Microsoft.AspNetCore.Mvc;
using MS.Autenticacion.Application.Services;
using MS.Autenticacion.Domain.Interfaces;
using System.Data;

namespace MS.Autenticacion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly IDatabaseConnection _databaseService;
        private readonly ILogger<DatabaseTestController> _logger;

        public DatabaseTestController(IDatabaseConnection databaseService, ILogger<DatabaseTestController> logger)
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
                        message = "Conexión a la base de datos exitosa",
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return StatusCode(500, new { 
                        success = false, 
                        message = "No se pudo conectar a la base de datos",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test de conexión");
                return StatusCode(500, new { 
                    success = false, 
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet("select-test")]
        public async Task<IActionResult> SelectTest()
        {
            try
            {
                var result = await _databaseService.ExecuteQueryAsync("SELECT 1 as test_value, NOW() as current_datetime");
                
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
                    message = "SELECT ejecutado exitosamente",
                    data = rows,
                    rowCount = result.Rows.Count,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SELECT test");
                return StatusCode(500, new { 
                    success = false, 
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet("database-info")]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var result = await _databaseService.ExecuteQueryAsync(@"
                    SELECT 
                        DATABASE() as current_database,
                        VERSION() as mysql_version,
                        USER() as current_user
                ");
                
                var info = new Dictionary<string, object>();
                if (result.Rows.Count > 0)
                {
                    var row = result.Rows[0];
                    foreach (DataColumn column in result.Columns)
                    {
                        info[column.ColumnName] = row[column];
                    }
                }

                return Ok(new { 
                    success = true, 
                    message = "Información de base de datos obtenida",
                    data = info,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo información de BD");
                return StatusCode(500, new { 
                    success = false, 
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }
    }
}