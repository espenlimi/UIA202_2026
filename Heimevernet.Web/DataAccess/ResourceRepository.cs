using Heimevernet.Web.Models.Entities;
using Heimevernet.Web.Models.ViewModels.Resource;

namespace Heimevernet.Web.DataAccess;

public class ResourceRepository : IResourceRepository
{
    private readonly Dictionary<int, Resource> _resources = new();
    private int _nextId = 1;

    public ResourceRepository()
    {
        //Add a few dummy resources for testing
        Create(new ResourceViewModel { Name = "Resource 1", Description = "Description 1", Type = "Type A" });
        Create(new ResourceViewModel { Name = "Resource 2", Description = "Description 2", Type = "Type B" });
        Create(new ResourceViewModel { Name = "Resource 3", Description = "Description 3", Type = "Type C" });
    }

    public Resource Create(ResourceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var resource = new Resource
        {
            Id = _nextId++,
            Name = model.Name,
            Description = model.Description,
            Type = model.Type
        };

        _resources.Add(resource.Id, resource);
        return resource;
    }

    public Resource? GetById(int id)
    {
        return _resources.GetValueOrDefault(id);
    }

    public IReadOnlyCollection<Resource> GetAll()
    {
        return _resources.Values.ToList().AsReadOnly();
    }

    public bool Update(int id, ResourceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!_resources.TryGetValue(id, out var resource))
        {
            return false;
        }

        resource.Name = model.Name;
        resource.Description = model.Description;
        resource.Type = model.Type;
        return true;
    }

    public bool Delete(int id)
    {
        return _resources.Remove(id);
    }
}
