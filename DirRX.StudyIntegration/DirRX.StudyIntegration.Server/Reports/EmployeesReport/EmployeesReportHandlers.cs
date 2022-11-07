using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.StudyIntegration
{
  partial class EmployeesReportServerHandlers
  {

    /// <summary>
    /// Возвращает список сотрудников отфильтрованных по дате приёма и дате увольнения.
    /// </summary>
    /// <returns> Сотрудники. </returns>
    public virtual IQueryable<DirRX.IntegrationRubi.IEmployee> GetEmployees()
    {
      return DirRX.IntegrationRubi.Employees.GetAll()
        .Where(e => e.StartDateDirRX >= EmployeesReport.StartDate)
        .Where(e => EmployeesReport.EndDate.HasValue ? 
          e.EndDateDirRX <= EmployeesReport.EndDate : e.EndDateDirRX.Equals((DateTime?)null));
    }

  }
}