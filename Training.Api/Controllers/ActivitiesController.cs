using Microsoft.AspNetCore.Mvc;
using Training.Application.Activities.DTOs;
using Training.Application.Activities.UseCases;
using Training.Application.Activities.UseCases.Training.Application.Activities.UseCases;

namespace Training.Api.Controllers
{
    [ApiController]
    [Route("api/activities")]
    public class ActivitiesController : ControllerBase
    {
        // UseCase injecté (Dependency Injection)
        private readonly CreateActivityUseCase _createActivityUseCase;

        // Constructeur
        public ActivitiesController(CreateActivityUseCase createActivityUseCase)
        {
            _createActivityUseCase = createActivityUseCase;
        }

        // POST api/activities
        // Créer une nouvelle activité
        [HttpPost]
        public async Task<ActionResult<ActivityResponse>> CreateActivity(
            [FromBody] CreateActivityRequest request)
        {
            // Appeler le UseCase
            var result = await _createActivityUseCase.ExecuteAsync(request);

            // Retourner 200 OK avec l'activité créée
            return Ok(result);
        }
    }
}