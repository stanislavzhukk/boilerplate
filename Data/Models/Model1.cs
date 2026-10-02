using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Model1
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Model2>? Model2s { get; set; }
    }
}
