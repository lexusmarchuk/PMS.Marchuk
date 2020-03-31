using PMS.Marchuk.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PMS.Marchuk.Repository
{
    public interface ITaskRepository
    {
        PmsResponse Create(string name, string description, Guid projectId);
        PmsResponse Update(Guid id, string name, string description);

        PmsResponse Delete(Guid id);

        PmsResponse Attach(Guid mainTaskId, Guid childTaskId);

        PmsResponse SetState(Guid id, State state);

        IEnumerable<Task> Find(Expression<Func<Task, bool>> f);
    }
}
