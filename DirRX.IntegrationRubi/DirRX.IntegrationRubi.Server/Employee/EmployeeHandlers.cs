using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.IntegrationRubi.Employee;

namespace DirRX.IntegrationRubi
{
  partial class EmployeeServerHandlers
  {

    public override void BeforeDelete(Sungero.Domain.BeforeDeleteEventArgs e)
    {
      // Если у сотрудника нет подразделения, не удалять ссылку на него в подразделении(не вызываем базовое событие до удаления).
      if (_obj.Department != null)
        base.BeforeDelete(e);
    }

    public override void BeforeSaveHistory(Sungero.Domain.HistoryEventArgs e)
    {
      base.BeforeSaveHistory(e);
      
      var IsEmployeeCreated = e.Action == Sungero.CoreEntities.History.Action.Create;
      var IsEmployeeUpdated = e.Action == Sungero.CoreEntities.History.Action.Update;
      
      // Сотрудник создан.
      if(IsEmployeeCreated)
      {
        var createOperation = new Enumeration(Constants.Company.Employee.Created);
        var createOperationDetails = createOperation;
        
        var comment = DirRX.IntegrationRubi.Employees.Resources.CreateHistoryCommentFormat(_obj.Person.LastName,
          _obj.Person.FirstName, _obj.Person.MiddleName);
        
        e.Write(createOperation, createOperationDetails, comment);
      }
    }
  }

}