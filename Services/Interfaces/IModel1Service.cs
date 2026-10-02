using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IModel1Service
    {
        Task<Model1> GetByIdAsync(Guid id);
    }
}
