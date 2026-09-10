using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;

namespace Heimevernet.Web.DataAccess
{
    public interface IResourceRepository
    {
        Resource Create(ResourceViewModel model);
        bool Delete(int id);
        IReadOnlyCollection<Resource> GetAll();
        Resource? GetById(int id);
        bool Update(int id, ResourceViewModel model);
    }
}