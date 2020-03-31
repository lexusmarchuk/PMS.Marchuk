using PMS.Marchuk.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PMS.Marchuk.Repository
{
    public interface IProjectRepository
    {
        Project GetByCode(string code);

        IEnumerable<Project> Find(Expression<Func<Project, bool>> f);

        PmsResponse Create(string code, string name);
        PmsResponse Update(Guid id, string name, State? state, DateTime? startDate, DateTime? endDate);
        PmsResponse Delete(Guid id);

        PmsResponse AttachProject(Guid mainProjectId, Guid childProjectId);       
    }
}
