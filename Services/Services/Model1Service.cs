using Data.Interfaces;
using Data.Models;
using Data.Repositories;
using Services.Interfaces;


namespace Services.Services
{
    public class Model1Service : IModel1Service
    {
        private readonly IModel1Repository _repository;
        public Model1Service(IModel1Repository repository)
        {
            _repository = repository;
        }

        public async Task<Model1> GetByIdAsync(Guid id)
        {
            var model = await _repository.GetByIdAsync(id);
            if (model == null)
            {
                return new Model1()
                {
                    Id = Guid.Empty,
                    Name = "Not found",
                };
            }
            return model;
        }

        public async Task AddAsync(Model1 model)
        {
            await _repository.AddAsync(model);
        }
    }
}
