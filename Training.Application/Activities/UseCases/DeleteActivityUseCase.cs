using Training.Application.Activities.Interfaces;

namespace Training.Application.Activities.UseCases
{
    public class DeleteActivityUseCase
    {
        private readonly IActivityRepository _activityRepository;

        public DeleteActivityUseCase(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
        }

        public async Task<bool> ExecuteAsync(Guid id)
        {
            var activity = await _activityRepository.GetByIdAsync(id);

            if (activity == null)
                return false;

            await _activityRepository.DeleteAsync(id);
            return true;
        }
    }
}