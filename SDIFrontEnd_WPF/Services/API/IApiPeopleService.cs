using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
namespace SDIFrontEnd_WPF
{
    public interface IApiPeopleService
    {
        Task<List<Person>> GetPeopleBasics();

    }
}
