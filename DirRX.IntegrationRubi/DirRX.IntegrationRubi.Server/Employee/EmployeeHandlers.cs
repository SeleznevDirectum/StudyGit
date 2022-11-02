using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
      var IsEmployeeCreated = e.Action == Sungero.CoreEntities.History.Action.Create;
      var IsEmployeeUpdated = e.Action == Sungero.CoreEntities.History.Action.Update;
      
      // Сотрудник создан.
      if (IsEmployeeCreated)
      {
        var createOperation = new Enumeration(Constants.Company.Employee.Created);
        var createOperationDetails = createOperation;
        
        var comment = DirRX.IntegrationRubi.Employees.Resources.CreateHistoryCommentFormat(_obj.Person.LastName,
          _obj.Person.FirstName, _obj.Person.MiddleName);
        
        e.Write(createOperation, createOperationDetails, comment);
      }
      
      // Сотрудник обновлнён.
      if (IsEmployeeUpdated)
      {
        var updateOperation = new Enumeration(Constants.Company.Employee.Updated);
        var updateOperationDetails = updateOperation;
        var sb = new StringBuilder().AppendLine(DirRX.IntegrationRubi.Employees.Resources.UpdateHistoryComment);
        
        // Изменилось Id сотрудника.
        if (_obj.State.Properties.WWIDDirRX.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.WWIDDirRX.LocalizedName, _obj.WWIDDirRX));
        }
        
        // Изменился логин сотрудника.
        if (_obj.Login.State.Properties.LoginName.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Login.ReferencedEntityInfo.Properties.LoginName.LocalizedName,
            _obj.Login.LoginName));
        }
        
        // Изменилась почта сотрудника.
        if (_obj.State.Properties.Email.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Email.LocalizedName, _obj.Email));
        }
        
        // Изменилось состояние сотрудника.
        if (_obj.State.Properties.Status.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Status.LocalizedName, 
            Employees.Info.Properties.Status.GetLocalizedValue(_obj.Status)));
        }
        
        // Изменилась дата приёма сотрудника.
        if (_obj.State.Properties.StartDateDirRX.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.StartDateDirRX.LocalizedName, _obj.StartDateDirRX));
        }
        
        // Изменилась дата увольнения сотрудника.
        if (_obj.State.Properties.EndDateDirRX.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.EndDateDirRX.LocalizedName, _obj.EndDateDirRX));
        }
        
        // Изменилось имя сотрудника.
        if (_obj.Person.State.Properties.LastName.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Person.ReferencedEntityInfo.Properties.LastName.LocalizedName,
            _obj.Person.LastName));
        }
        
        // Изменилась фамилия сотрудника.
        if (_obj.Person.State.Properties.FirstName.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Person.ReferencedEntityInfo.Properties.FirstName.LocalizedName,
            _obj.Person.FirstName));
        }
        
        // Изменилось отчество сотрудника.
        if (_obj.Person.State.Properties.MiddleName.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Person.ReferencedEntityInfo.Properties.MiddleName.LocalizedName,
            _obj.Person.MiddleName));
        }
        
        // Изменилось подразделение сотрудника.
        if (_obj.Department != null && _obj.Department.State.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.Department.LocalizedName,
            _obj.Department.Name));
        }
        
        // Изменилась должность сотрудника.
        if (_obj.JobTitle != null && _obj.JobTitle.State.IsChanged)
        {
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.JobTitle.LocalizedName,
            _obj.JobTitle.Name));
        }
        
        // Изменился руководитель сотрудника.
        if (_obj.MangerDirRX != null && _obj.MangerDirRX.State.IsChanged)
        {         
          sb.AppendLine(DirRX.IntegrationRubi.Employees.Resources.PropertyIsChangedCommentFormat(
            Employees.Info.Properties.MangerDirRX.LocalizedName,
            string.Format("{0} {1} {2}",
              _obj.MangerDirRX.Person.LastName,
              _obj.MangerDirRX.Person.FirstName,
              _obj.MangerDirRX.Person.MiddleName)));
        }
        
        var comment = sb.ToString();
        e.Write(updateOperation, updateOperationDetails, comment);
      }
    }
  }

}