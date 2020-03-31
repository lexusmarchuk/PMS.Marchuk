using PMS.Marchuk.Models;
using System;

namespace PMS.Marchuk
{
    public interface IProjectUnitOfWork
    {
        PmsResponse CreateProject(string code, string name, Guid? parentId);        
        Project GetProjectByCode(string code);
        byte[] GenerateReport();
    }
}