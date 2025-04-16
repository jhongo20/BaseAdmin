using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModulesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ModulesController> _logger;

        public ModulesController(IUnitOfWork unitOfWork, ILogger<ModulesController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Module>>> GetAll()
        {
            try
            {
                var modules = await _unitOfWork.Modules.GetAllAsync();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los módulos");
                return StatusCode(500, "Error interno del servidor al obtener módulos");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Module>> GetById(Guid id)
        {
            try
            {
                var module = await _unitOfWork.Modules.GetByIdAsync(id);
                if (module == null)
                {
                    return NotFound($"Módulo con ID {id} no encontrado");
                }
                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulo con ID {ModuleId}", id);
                return StatusCode(500, "Error interno del servidor al obtener el módulo");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<Module>> GetByName(string name)
        {
            try
            {
                var module = await _unitOfWork.Modules.GetByNameAsync(name);
                if (module == null)
                {
                    return NotFound($"Módulo con nombre {name} no encontrado");
                }
                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulo con nombre {Name}", name);
                return StatusCode(500, "Error interno del servidor al obtener el módulo");
            }
        }

        [HttpGet("route/{route}")]
        public async Task<ActionResult<Module>> GetByRoute(string route)
        {
            try
            {
                var module = await _unitOfWork.Modules.GetByRouteAsync(route);
                if (module == null)
                {
                    return NotFound($"Módulo con ruta {route} no encontrado");
                }
                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulo con ruta {Route}", route);
                return StatusCode(500, "Error interno del servidor al obtener el módulo");
            }
        }

        [HttpGet("main")]
        public async Task<ActionResult<IEnumerable<Module>>> GetMainModules()
        {
            try
            {
                var modules = await _unitOfWork.Modules.GetMainModulesAsync();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos principales");
                return StatusCode(500, "Error interno del servidor al obtener módulos principales");
            }
        }

        [HttpGet("{parentId}/submodules")]
        public async Task<ActionResult<IEnumerable<Module>>> GetSubmodules(Guid parentId)
        {
            try
            {
                var modules = await _unitOfWork.Modules.GetSubmodulesAsync(parentId);
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener submódulos del módulo con ID {ParentId}", parentId);
                return StatusCode(500, "Error interno del servidor al obtener submódulos");
            }
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<Module>>> GetModuleTree()
        {
            try
            {
                // Como no existe GetModuleTreeAsync, construimos el árbol manualmente
                var mainModules = await _unitOfWork.Modules.GetMainModulesAsync();
                var result = new List<Module>();
                
                foreach (var module in mainModules)
                {
                    // Cargar los submódulos recursivamente
                    await LoadSubmodulesRecursively(module);
                    result.Add(module);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener árbol de módulos");
                return StatusCode(500, "Error interno del servidor al obtener árbol de módulos");
            }
        }

        // Método auxiliar para cargar submódulos recursivamente
        private async Task LoadSubmodulesRecursively(Module module)
        {
            if (module == null) return;
            
            var submodules = await _unitOfWork.Modules.GetSubmodulesAsync(module.Id);
            
            if (submodules != null && submodules.Count > 0)
            {
                module.Children = submodules.ToList();
                
                foreach (var submodule in module.Children)
                {
                    await LoadSubmodulesRecursively(submodule);
                }
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Module>>> GetAccessibleModulesForUser(Guid userId)
        {
            try
            {
                // Usar el método GetAccessibleByUserAsync en lugar de GetAccessibleModulesForUserAsync
                var modules = await _unitOfWork.Modules.GetAccessibleByUserAsync(userId);
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulos accesibles para el usuario con ID {UserId}", userId);
                return StatusCode(500, "Error interno del servidor al obtener módulos accesibles");
            }
        }

        // Métodos POST, PUT y DELETE se implementarán en la siguiente fase
    }
}
